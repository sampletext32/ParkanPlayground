using System.Runtime.InteropServices;
using FxidLib;

namespace NResUI.EffectPlayer;

/// <summary>WAV, приведённый к PCM: формат и данные.</summary>
public sealed record PcmWave(ushort Channels, uint SamplesPerSec, ushort BitsPerSample, byte[] Pcm)
{
    private static readonly int[] AdaptationTable =
        [230, 230, 230, 230, 307, 409, 512, 614, 768, 614, 512, 409, 307, 230, 230, 230];

    private static readonly short[,] DefaultCoefficients =
        { { 256, 0 }, { 512, -256 }, { 0, 0 }, { 192, 64 }, { 240, 0 }, { 460, -208 }, { 392, -232 } };

    /// <summary>
    /// Читает RIFF/WAVE: PCM (format tag 1) копируется как есть, Microsoft ADPCM (tag 2, 4 бита; 48 из 137 файлов sounds.lib)
    /// раскодируется в PCM 16 бит. Остальные форматы дают null.
    /// </summary>
    public static PcmWave? Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12 || data[0] != 'R' || data[1] != 'I' || data[2] != 'F' || data[3] != 'F')
            return null;

        ushort tag = 0, channels = 0, bits = 0, blockAlign = 0, samplesPerBlock = 0;
        uint rate = 0;
        var coefficients = DefaultCoefficients;
        var haveFmt = false;
        var pos = 12;
        while (pos + 8 <= data.Length)
        {
            var id = data.Slice(pos, 4);
            var size = (int)BitConverter.ToUInt32(data.Slice(pos + 4, 4));
            var body = pos + 8;
            if (id.SequenceEqual("fmt "u8) && body + 16 <= data.Length)
            {
                tag = BitConverter.ToUInt16(data.Slice(body, 2));
                channels = BitConverter.ToUInt16(data.Slice(body + 2, 2));
                rate = BitConverter.ToUInt32(data.Slice(body + 4, 4));
                blockAlign = BitConverter.ToUInt16(data.Slice(body + 12, 2));
                bits = BitConverter.ToUInt16(data.Slice(body + 14, 2));
                haveFmt = true;

                // WAVEFORMATEX: cbSize, затем для MS ADPCM: samplesPerBlock, numCoef, пары коэффициентов int16.
                if (tag == 2 && size >= 22 && body + 22 <= data.Length)
                {
                    samplesPerBlock = BitConverter.ToUInt16(data.Slice(body + 18, 2));
                    var count = BitConverter.ToUInt16(data.Slice(body + 20, 2));
                    if (count > 0 && count <= 16 && body + 22 + count * 4 <= data.Length)
                    {
                        coefficients = new short[count, 2];
                        for (var i = 0; i < count; i++)
                        {
                            coefficients[i, 0] = BitConverter.ToInt16(data.Slice(body + 22 + i * 4, 2));
                            coefficients[i, 1] = BitConverter.ToInt16(data.Slice(body + 24 + i * 4, 2));
                        }
                    }
                }
            }
            else if (id.SequenceEqual("data"u8) && haveFmt)
            {
                var length = Math.Clamp(size, 0, data.Length - body);
                var payload = data.Slice(body, length);
                if (tag == 1)
                    return new PcmWave(channels, rate, bits, payload.ToArray());

                if (tag == 2 && bits == 4 && (channels == 1 || channels == 2) && blockAlign > 0)
                {
                    var pcm = DecodeMsAdpcm(payload, channels, blockAlign, coefficients);
                    return pcm == null ? null : new PcmWave(channels, rate, 16, pcm);
                }

                return null;
            }

            pos = body + size + (size & 1);
        }

        return null;
    }

    private static byte[]? DecodeMsAdpcm(ReadOnlySpan<byte> data, int channels, int blockAlign, short[,] coefficients)
    {
        var headerSize = 7 * channels;
        if (blockAlign <= headerSize)
            return null;

        var output = new List<byte>(data.Length * 4);
        var predictor = new int[channels];
        var delta = new int[channels];
        var sample1 = new int[channels];
        var sample2 = new int[channels];

        void Emit(int value)
        {
            var clamped = (short)Math.Clamp(value, short.MinValue, short.MaxValue);
            output.Add((byte)(clamped & 0xFF));
            output.Add((byte)((clamped >> 8) & 0xFF));
        }

        for (var blockStart = 0; blockStart < data.Length; blockStart += blockAlign)
        {
            var block = data.Slice(blockStart, Math.Min(blockAlign, data.Length - blockStart));
            if (block.Length < headerSize)
                break;

            var p = 0;
            for (var ch = 0; ch < channels; ch++)
                predictor[ch] = Math.Clamp((int)block[p++], 0, coefficients.GetLength(0) - 1);
            for (var ch = 0; ch < channels; ch++, p += 2)
                delta[ch] = BitConverter.ToInt16(block.Slice(p, 2));
            for (var ch = 0; ch < channels; ch++, p += 2)
                sample1[ch] = BitConverter.ToInt16(block.Slice(p, 2));
            for (var ch = 0; ch < channels; ch++, p += 2)
                sample2[ch] = BitConverter.ToInt16(block.Slice(p, 2));

            // Сначала sample2, затем sample1 (в порядке времени).
            for (var ch = 0; ch < channels; ch++)
                Emit(sample2[ch]);
            for (var ch = 0; ch < channels; ch++)
                Emit(sample1[ch]);

            var nibbleIndex = 0;
            for (; p < block.Length; p++)
            {
                for (var half = 0; half < 2; half++)
                {
                    var nibble = half == 0 ? block[p] >> 4 : block[p] & 0x0F;
                    var ch = nibbleIndex++ % channels;
                    var signedNibble = nibble >= 8 ? nibble - 16 : nibble;

                    var predicted = (sample1[ch] * coefficients[predictor[ch], 0] +
                                     sample2[ch] * coefficients[predictor[ch], 1]) / 256;
                    var value = Math.Clamp(predicted + signedNibble * delta[ch], short.MinValue, short.MaxValue);
                    Emit(value);

                    sample2[ch] = sample1[ch];
                    sample1[ch] = value;
                    delta[ch] = Math.Max(16, delta[ch] * AdaptationTable[nibble] / 256);
                }
            }
        }

        return output.ToArray();
    }
}

/// <summary>Один звучащий экземпляр: собственное устройство waveOut, поэтому звуки смешиваются системой и могут перекрываться.</summary>
internal sealed class WaveVoice : IDisposable
{
    private IntPtr _device;
    private IntPtr _header;
    private IntPtr _data;
    private bool _prepared;

    public int ComponentIndex { get; }
    public bool Looped { get; }

    private WaveVoice(int componentIndex, bool looped)
    {
        ComponentIndex = componentIndex;
        Looped = looped;
    }

    public static WaveVoice? Start(PcmWave wave, float volume, bool looped, int componentIndex)
    {
        if (!OperatingSystem.IsWindows() || wave.Pcm.Length == 0)
            return null;

        var voice = new WaveVoice(componentIndex, looped);
        if (!voice.Open(wave, volume))
        {
            voice.Dispose();
            return null;
        }

        return voice;
    }

    private bool Open(PcmWave wave, float volume)
    {
        var blockAlign = (ushort)(wave.Channels * wave.BitsPerSample / 8);
        var format = new WinMm.WaveFormatEx
        {
            FormatTag = 1,
            Channels = wave.Channels,
            SamplesPerSec = wave.SamplesPerSec,
            AvgBytesPerSec = wave.SamplesPerSec * blockAlign,
            BlockAlign = blockAlign,
            BitsPerSample = wave.BitsPerSample,
            Size = 0,
        };

        if (WinMm.waveOutOpen(out _device, WinMm.WaveMapper, ref format, IntPtr.Zero, IntPtr.Zero, 0) != 0)
            return false;

        _data = Marshal.AllocHGlobal(wave.Pcm.Length);
        Marshal.Copy(wave.Pcm, 0, _data, wave.Pcm.Length);

        var header = new WinMm.WaveHdr
        {
            Data = _data,
            BufferLength = (uint)wave.Pcm.Length,
            Flags = Looped ? WinMm.WhdrBeginLoop | WinMm.WhdrEndLoop : 0u,
            Loops = Looped ? 0x00FFFFFFu : 0u,
        };
        _header = Marshal.AllocHGlobal(Marshal.SizeOf<WinMm.WaveHdr>());
        Marshal.StructureToPtr(header, _header, false);

        var size = Marshal.SizeOf<WinMm.WaveHdr>();
        if (WinMm.waveOutPrepareHeader(_device, _header, size) != 0)
            return false;

        _prepared = true;
        SetVolume(volume);
        return WinMm.waveOutWrite(_device, _header, size) == 0;
    }

    public void SetVolume(float volume)
    {
        if (_device == IntPtr.Zero)
            return;

        var v = (uint)(Math.Clamp(volume, 0f, 1f) * 0xFFFF);
        WinMm.waveOutSetVolume(_device, v | (v << 16));
    }

    public bool IsFinished
    {
        get
        {
            if (_header == IntPtr.Zero)
                return true;

            var flags = (uint)Marshal.ReadInt32(_header, WinMm.FlagsOffset);
            return (flags & WinMm.WhdrDone) != 0;
        }
    }

    public void Dispose()
    {
        if (_device != IntPtr.Zero)
        {
            WinMm.waveOutReset(_device);
            if (_prepared && _header != IntPtr.Zero)
                WinMm.waveOutUnprepareHeader(_device, _header, Marshal.SizeOf<WinMm.WaveHdr>());

            WinMm.waveOutClose(_device);
            _device = IntPtr.Zero;
        }

        if (_header != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_header);
            _header = IntPtr.Zero;
        }

        if (_data != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_data);
            _data = IntPtr.Zero;
        }
    }
}

internal static class WinMm
{
    public const uint WaveMapper = 0xFFFFFFFF;
    public const uint WhdrDone = 0x00000001;
    public const uint WhdrBeginLoop = 0x00000004;
    public const uint WhdrEndLoop = 0x00000008;

    /// <summary>Смещение поля dwFlags в WAVEHDR (после lpData, dwBufferLength, dwBytesRecorded, dwUser).</summary>
    public static int FlagsOffset => IntPtr.Size + 4 + 4 + IntPtr.Size;

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct WaveFormatEx
    {
        public ushort FormatTag;
        public ushort Channels;
        public uint SamplesPerSec;
        public uint AvgBytesPerSec;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public ushort Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WaveHdr
    {
        public IntPtr Data;
        public uint BufferLength;
        public uint BytesRecorded;
        public IntPtr User;
        public uint Flags;
        public uint Loops;
        public IntPtr Next;
        public IntPtr Reserved;
    }

    [DllImport("winmm.dll")]
    public static extern int waveOutOpen(out IntPtr device, uint deviceId, ref WaveFormatEx format, IntPtr callback,
        IntPtr instance, uint flags);

    [DllImport("winmm.dll")]
    public static extern int waveOutPrepareHeader(IntPtr device, IntPtr header, int size);

    [DllImport("winmm.dll")]
    public static extern int waveOutUnprepareHeader(IntPtr device, IntPtr header, int size);

    [DllImport("winmm.dll")]
    public static extern int waveOutWrite(IntPtr device, IntPtr header, int size);

    [DllImport("winmm.dll")]
    public static extern int waveOutReset(IntPtr device);

    [DllImport("winmm.dll")]
    public static extern int waveOutClose(IntPtr device);

    [DllImport("winmm.dll")]
    public static extern int waveOutSetVolume(IntPtr device, uint volume);
}

/// <summary>
/// Запускает звуковые компоненты FXID (тип 2) по значению силы: одиночные срабатывают при пересечении начала окна вверх,
/// зацикленные (<see cref="EffectSoundData.IsLooped"/>, режимы 2 и 3) звучат, пока сила внутри окна.
/// Громкость = lerp(VolumeStart, VolumeEnd, сила) × общая громкость. 3D позиция и затухание по расстоянию не моделируются.
/// Работает только в Windows (winmm); в других ОС звук молча отключён.
/// </summary>
public sealed class EffectSoundPlayer
{
    private readonly List<WaveVoice> _voices = [];

    public static bool IsSupported => OperatingSystem.IsWindows();

    /// <summary>Сколько звуков звучит сейчас.</summary>
    public int ActiveVoices => _voices.Count;

    /// <summary>Последняя причина, по которой звук не запустился (нет файла, формат не PCM и т.д.).</summary>
    public string? LastWarning { get; private set; }

    public void Update(
        PreparedEffect effect,
        EffectAssetCache assets,
        float previousStrength,
        float strength,
        float masterVolume)
    {
        _voices.RemoveAll(v =>
        {
            if (!v.IsFinished)
                return false;

            v.Dispose();
            return true;
        });

        foreach (var c in effect.Components)
        {
            if (c.Type != FxidComponentType.Sound || c.Sound is not { } sound)
                continue;

            var volume = (sound.VolumeStart + (sound.VolumeEnd - sound.VolumeStart) * Math.Clamp(strength, 0f, 1f)) *
                         masterVolume;

            if (sound.IsLooped)
            {
                var running = _voices.Find(v => v.ComponentIndex == c.Index);
                if (c.Active)
                {
                    if (running == null)
                        Start(assets, c, sound, volume, looped: true);
                    else
                        running.SetVolume(volume);
                }
                else if (running != null)
                {
                    running.Dispose();
                    _voices.Remove(running);
                }
            }
            else if (previousStrength < sound.WindowStart && strength >= sound.WindowStart &&
                     (sound.WindowEnd < sound.WindowStart || strength <= sound.WindowEnd))
            {
                Start(assets, c, sound, volume, looped: false);
            }
        }
    }

    private void Start(EffectAssetCache assets, PreparedComponent c, EffectSoundData sound, float volume, bool looped)
    {
        if (!IsSupported)
        {
            LastWarning = "Звук доступен только в Windows";
            return;
        }

        var bytes = assets.GetSoundBytes(sound.Resource.ResourceName);
        if (bytes == null)
        {
            LastWarning = $"Звук '{sound.Resource.ResourceName}' не найден в sounds.lib";
            return;
        }

        var wave = PcmWave.Parse(bytes);
        if (wave == null)
        {
            LastWarning = $"Звук '{sound.Resource.ResourceName}': формат WAV не поддерживается (нужен PCM или MS ADPCM)";
            return;
        }

        var voice = WaveVoice.Start(wave, volume, looped, c.Index);
        if (voice == null)
        {
            LastWarning = $"Звук '{sound.Resource.ResourceName}': не удалось открыть устройство вывода";
            return;
        }

        _voices.Add(voice);
    }

    /// <summary>Остановить зацикленные звуки (пауза, перемотка).</summary>
    public void StopLooped()
    {
        _voices.RemoveAll(v =>
        {
            if (!v.Looped)
                return false;

            v.Dispose();
            return true;
        });
    }

    public void StopAll()
    {
        foreach (var voice in _voices)
            voice.Dispose();

        _voices.Clear();
    }
}
