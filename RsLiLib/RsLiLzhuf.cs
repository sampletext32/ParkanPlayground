namespace RsLiLib;

/// <summary>
/// LZHUF (LZSS с адаптивным кодом Хаффмана, автор алгоритма - Haruyasu Yoshizaki) в варианте игры: ветка <c>FUN_100156BE</c> с флагом 0x80
/// (Ngi32.dll 0x100156BE; таблицы строятся <c>FUN_10015470</c>, обновление дерева - <c>FUN_1001553E</c>). Окно 4096 байт, F = 60, порог 2, 314 символов.
/// Реализация сверена с кодом игры по структуре, но на данных не проверена: в файлах первой части этого способа нет.
/// </summary>
public static class RsLiLzhuf
{
    private const int WindowSize = 4096;
    private const int MaxMatch = 60;
    private const int Threshold = 2;
    private const int CharCount = 256 - Threshold + MaxMatch;
    private const int TreeSize = CharCount * 2 - 1;
    private const int Root = TreeSize - 1;
    private const int MaxFrequency = 0x8000;

    private static readonly byte[] DistanceCode = new byte[256];
    private static readonly byte[] DistanceLength = new byte[256];

    static RsLiLzhuf()
    {
        // Таблицы позиции: (сколько кодов, сколько байт на код, длина кода в битах).
        (int Codes, int PerCode, int Length)[] groups = [(1, 32, 3), (3, 16, 4), (8, 8, 5), (12, 4, 6), (24, 2, 7), (16, 1, 8)];
        var index = 0;
        var code = 0;
        foreach (var (codes, perCode, length) in groups)
        {
            for (var c = 0; c < codes; c++, code++)
            {
                for (var k = 0; k < perCode; k++, index++)
                {
                    DistanceCode[index] = (byte)code;
                    DistanceLength[index] = (byte)length;
                }
            }
        }
    }

    /// <summary>Распаковывает данные.</summary>
    /// <param name="source">Упакованные байты.</param>
    /// <param name="unpackedSize">Ожидаемый размер результата.</param>
    public static byte[] Decode(ReadOnlySpan<byte> source, int unpackedSize)
    {
        var decoder = new Decoder(source.ToArray());
        return decoder.Run(unpackedSize);
    }

    private sealed class Decoder
    {
        private readonly byte[] _source;
        private readonly int[] _freq = new int[TreeSize + 1];
        private readonly int[] _parent = new int[TreeSize + CharCount];
        private readonly int[] _son = new int[TreeSize];

        private int _inputPos;
        private int _bitBuffer;
        private int _bitCount;

        public Decoder(byte[] source)
        {
            _source = source;

            for (var i = 0; i < CharCount; i++)
            {
                _freq[i] = 1;
                _son[i] = i + TreeSize;
                _parent[i + TreeSize] = i;
            }

            var left = 0;
            for (var node = CharCount; node <= Root; node++, left += 2)
            {
                _freq[node] = _freq[left] + _freq[left + 1];
                _son[node] = left;
                _parent[left] = _parent[left + 1] = node;
            }

            _freq[TreeSize] = 0xFFFF;
            _parent[Root] = 0;
        }

        public byte[] Run(int unpackedSize)
        {
            var window = new byte[WindowSize];
            Array.Fill(window, (byte)' ', 0, WindowSize - MaxMatch);

            var writePos = WindowSize - MaxMatch;
            var output = new byte[unpackedSize];
            var outputLength = 0;

            while (outputLength < unpackedSize)
            {
                var symbol = DecodeChar();
                if (symbol < 256)
                {
                    output[outputLength++] = (byte)symbol;
                    window[writePos] = (byte)symbol;
                    writePos = (writePos + 1) & (WindowSize - 1);
                    continue;
                }

                var start = (writePos - DecodePosition() - 1) & (WindowSize - 1);
                var length = symbol - 255 + Threshold;

                for (var k = 0; k < length && outputLength < unpackedSize; k++)
                {
                    var value = window[(start + k) & (WindowSize - 1)];
                    output[outputLength++] = value;
                    window[writePos] = value;
                    writePos = (writePos + 1) & (WindowSize - 1);
                }
            }

            return output;
        }

        private int GetBit()
        {
            FillBits();
            var bit = (_bitBuffer >> 15) & 1;
            _bitBuffer = (_bitBuffer << 1) & 0xFFFF;
            _bitCount--;
            return bit;
        }

        private int GetByte()
        {
            FillBits();
            var value = (_bitBuffer >> 8) & 0xFF;
            _bitBuffer = (_bitBuffer << 8) & 0xFFFF;
            _bitCount -= 8;
            return value;
        }

        private void FillBits()
        {
            while (_bitCount <= 8)
            {
                var next = _inputPos < _source.Length ? _source[_inputPos] : 0;
                _inputPos++;
                _bitBuffer |= next << (8 - _bitCount);
                _bitCount += 8;
            }
        }

        private int DecodeChar()
        {
            var node = _son[Root];
            while (node < TreeSize)
                node = _son[node + GetBit()];

            node -= TreeSize;
            Update(node);
            return node;
        }

        private int DecodePosition()
        {
            var i = GetByte();
            var high = DistanceCode[i] << 6;
            var extraBits = DistanceLength[i] - 2;

            while (extraBits-- > 0)
                i = (i << 1) + GetBit();

            return high | (i & 0x3F);
        }

        private void Rebuild()
        {
            var j = 0;
            for (var i = 0; i < TreeSize; i++)
            {
                if (_son[i] >= TreeSize)
                {
                    _freq[j] = (_freq[i] + 1) >> 1;
                    _son[j] = _son[i];
                    j++;
                }
            }

            for (int i = 0, node = CharCount; node < TreeSize; i += 2, node++)
            {
                var sum = _freq[i] + _freq[i + 1];
                _freq[node] = sum;

                var k = node - 1;
                while (sum < _freq[k])
                    k--;

                k++;
                var count = node - k;
                Array.Copy(_freq, k, _freq, k + 1, count);
                _freq[k] = sum;
                Array.Copy(_son, k, _son, k + 1, count);
                _son[k] = i;
            }

            for (var i = 0; i < TreeSize; i++)
            {
                var k = _son[i];
                if (k >= TreeSize)
                {
                    _parent[k] = i;
                }
                else
                {
                    _parent[k] = _parent[k + 1] = i;
                }
            }
        }

        private void Update(int symbol)
        {
            if (_freq[Root] == MaxFrequency)
                Rebuild();

            var node = _parent[symbol + TreeSize];
            do
            {
                var frequency = ++_freq[node];
                var next = node + 1;
                if (frequency > _freq[next])
                {
                    while (frequency > _freq[next + 1])
                        next++;

                    _freq[node] = _freq[next];
                    _freq[next] = frequency;

                    var i = _son[node];
                    _parent[i] = next;
                    if (i < TreeSize)
                        _parent[i + 1] = next;

                    var j = _son[next];
                    _son[next] = i;
                    _parent[j] = node;
                    if (j < TreeSize)
                        _parent[j + 1] = node;

                    _son[node] = j;
                    node = next;
                }

                node = _parent[node];
            }
            while (node != 0);
        }
    }
}
