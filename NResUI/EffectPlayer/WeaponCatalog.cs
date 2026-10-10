using System.Numerics;
using ControlLib;
using ExplLib;
using NResUI.Abstractions;

namespace NResUI.EffectPlayer;

/// <summary>
/// Матрица размещения эффекта в системе координат объекта (X вправо, Y вперёд, Z вверх): колонки Ex, Ey, Ez и позиция.
/// Строится как в <c>ExecuteControlCommandList</c> для команды SPAWN_ORIENTED_EFFECT_FROM_CPTS: три CPT (arg0..arg2),
/// позиция - среднее позиций; если направления трёх CPT попарно различны, колонками матрицы служат сами направления
/// (их длина - масштаб эффекта); если все равны - направление вперёд и масштаб равен длине.
/// </summary>
public readonly record struct EffectPlacement(Vector3 Ex, Vector3 Ey, Vector3 Ez, Vector3 Position);

/// <summary>Эффект, который запускает команда списка CTL (SPAWN_* с ресурсом из effects.rlb).</summary>
/// <param name="HookList">Номер списка команд внутри CTL (0 - on_load, остальные номера см. таблицу крючков CTL).</param>
/// <param name="Opcode">Код команды (3, 4 - spawn на деталях/CPT, 5 - на границах объекта).</param>
/// <param name="LocalId">local_id эффекта (arg3): по нему START/REMOVE адресуют эффект.</param>
/// <param name="Placement">Размещение по CPT (только команда 4); null - единичная матрица.</param>
public sealed record WeaponEffectRef(
    int HookList, uint Opcode, string Effect, int LocalId, EffectPlacement? Placement = null);

/// <summary>Снаряд (запись BULL objects.rlb + его CTL в weapon.rlb).</summary>
/// <param name="Speed">MaxTangentialSpeed.Y корневых параметров CTL, ед./с (ось Y - вперёд).</param>
/// <param name="FlightEffects">Эффекты, создаваемые снарядом при загрузке (шлейф, свет, дым, факел ракеты).</param>
/// <param name="StartOnEndLocalIds">
/// local_id эффектов, которые CTL снаряда запускает командой START_EFFECT (10) в состояниях конца полёта (лазеры:
/// шлейф стартует с источником Life при попадании и затухает). У остальных снарядов эффект выключается (DEACTIVATE).
/// </param>
/// <param name="ExplName">Имя EXPL (команда 27) - взрыв при попадании.</param>
/// <param name="Expl">Разобранный EXPL; <see cref="ExplFile.Slots"/> - эффекты по материалу поверхности.</param>
public sealed record BulletInfo(
    string Name,
    float Speed,
    IReadOnlyList<WeaponEffectRef> FlightEffects,
    string? ExplName,
    ExplFile? Expl,
    IReadOnlyList<int> StartOnEndLocalIds);

/// <summary>Орудие (CTL из guns.rlb): предмет типа 0x02/0x1E, снаряд и эффекты выстрела.</summary>
/// <param name="FireDelayMs">CP0 предмета (пауза между выстрелами, мс).</param>
/// <param name="Ammo">CP1 предмета (боезапас, -1 = бесконечный).</param>
/// <param name="MuzzleEffects">Эффекты, создаваемые CTL орудия (вспышка, звук, дым).</param>
/// <param name="ShotAnimSpeed">
/// Скорость канала анимации привязки предмета (<c>raw_binding_record+0x24</c>, ед./с): переход значения канала на 1.0 занимает
/// 1000 / скорость мс (<c>ExtendTransitionEndTimeForBinding</c>). Определяет длительность выстрела по автомату орудия.
/// </param>
/// <param name="ShotPauseMs">CP2 предмета: пауза после такта «выстрел» (SpawnProjectile) перед последним тактом анимации.</param>
public sealed record WeaponInfo(
    string Name,
    string? BulletName,
    float FireDelayMs,
    float Ammo,
    IReadOnlyList<WeaponEffectRef> MuzzleEffects,
    BulletInfo? Bullet,
    float ShotAnimSpeed = 4f,
    float ShotPauseMs = 0f);

/// <summary>
/// Каталог орудий первой части: guns.rlb (CTL орудий) -> objects.rlb (BULL) -> weapon.rlb (CTL снаряда, EXPL).
/// Связь выведена по данным: CTL орудия создаёт эффекты вспышки/звука, у предмета типа 0x02 в Resource лежит имя BULL,
/// CTL снаряда создаёт эффекты полёта (шлейф) и по команде 27 взрыв (EXPL) с эффектом по материалу поверхности.
/// </summary>
public sealed class WeaponCatalog : IService
{
    private readonly EffectAssetCache _assets;
    private string _basePath = string.Empty;
    private NResStore? _guns;
    private NResStore? _weapon;
    private NResStore? _objects;
    private readonly Dictionary<string, WeaponInfo?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public WeaponCatalog(EffectAssetCache assets)
    {
        _assets = assets;
    }

    public void Reset()
    {
        _guns = null;
        _weapon = null;
        _objects = null;
        _cache.Clear();
    }

    private bool EnsureStores(out string? error)
    {
        var path = _assets.GameBasePath;
        if (_guns == null || !string.Equals(path, _basePath, StringComparison.Ordinal))
        {
            Reset();
            _basePath = path;
            _guns = new NResStore(Path.Combine(path, "guns.rlb"));
            _weapon = new NResStore(Path.Combine(path, "weapon.rlb"));
            _objects = new NResStore(Path.Combine(path, "objects.rlb"));
        }

        foreach (var store in new[] { _guns, _weapon!, _objects! })
        {
            if (!store.Load())
            {
                error = store.Error;
                return false;
            }
        }

        error = null;
        return true;
    }

    /// <summary>Имена CTL орудий (o_gun_*) из guns.rlb.</summary>
    public IReadOnlyList<string> GetWeaponNames(out string? error)
    {
        if (!EnsureStores(out error))
            return [];

        return _guns!.Files
            .Where(f => f.FileType == "CTLD" && f.FileName.StartsWith("o_gun_", StringComparison.OrdinalIgnoreCase))
            .Select(f => f.FileName)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public WeaponInfo? GetWeapon(string ctlName)
    {
        if (!EnsureStores(out _))
            return null;

        if (_cache.TryGetValue(ctlName, out var cached))
            return cached;

        WeaponInfo? result = null;
        try
        {
            result = Build(ctlName);
        }
        catch (Exception)
        {
            // Повреждённый CTL просто не попадает в каталог.
        }

        _cache[ctlName] = result;
        return result;
    }

    private WeaponInfo? Build(string ctlName)
    {
        var entry = _guns!.Find(ctlName);
        if (entry == null)
            return null;

        using var ms = new MemoryStream(_guns.Read(entry), writable: false);
        var ctl = ControlResourceReader.ReadCtlFile(ms, ctlName);

        var item = ctl.ItemRecords.FirstOrDefault(i => i.ItemType is 2 or 0x1E);
        var bullet = item != null ? BuildBullet(item.Resource.ResourceName) : null;

        // CPT орудия лежит рядом с CTL: o_gun_xx.cpt.
        CptFile? cpt = null;
        var cptEntry = _guns.Find(Path.ChangeExtension(ctlName, ".cpt"));
        if (cptEntry != null)
        {
            using var cptStream = new MemoryStream(_guns.Read(cptEntry), writable: false);
            cpt = ControlResourceReader.ReadCptFile(cptStream, cptEntry.FileName);
        }

        return new WeaponInfo(
            ctlName,
            item?.Resource.ResourceName,
            item?.CommonParams.Count > 0 ? item.CommonParams[0] : 0f,
            item?.CommonParams.Count > 1 ? item.CommonParams[1] : 0f,
            CollectEffects(ctl, cpt),
            bullet,
            ShotAnimSpeed(ctl, item),
            item?.CommonParams.Count > 2 ? item.CommonParams[2] : 0f);
    }

    private static float ShotAnimSpeed(CtlFile ctl, CtlItemRecordRaw? item)
    {
        if (item == null || item.PieceBindingIndices.Count == 0)
            return 4f;

        var index = item.PieceBindingIndices[0];
        if (index < 0 || index >= ctl.PieceBindings.Count)
            return 4f;

        var speed = ctl.PieceBindings[index].Param24;
        return speed > 0.01f ? speed : 4f;
    }

    private BulletInfo? BuildBullet(string bulletName)
    {
        if (string.IsNullOrEmpty(bulletName))
            return null;

        var record = _objects!.Find(bulletName);
        if (record == null)
            return null;

        var agent = AgentRecord.Parse(AgentRecordType.Bull, bulletName, _objects.Read(record));
        var ctlRef = agent.Control;
        var ctlEntry = _weapon!.Find(ctlRef.ResourceName);
        if (ctlEntry == null)
            return null;

        using var ms = new MemoryStream(_weapon.Read(ctlEntry), writable: false);
        var ctl = ControlResourceReader.ReadCtlFile(ms, ctlRef.ResourceName);

        string? explName = null;
        ExplFile? expl = null;
        var startOnEnd = new List<int>();
        foreach (var list in ctl.CommandLists)
        {
            foreach (var cmd in list.Records)
            {
                if ((uint)cmd.Opcode == 10)
                    startOnEnd.Add(unchecked((int)cmd.Arg0));

                if ((uint)cmd.Opcode != 27 || string.IsNullOrEmpty(cmd.Resource.ResourceName))
                    continue;

                explName = cmd.Resource.ResourceName;
                var explEntry = _weapon.Find(explName);
                if (explEntry != null)
                {
                    try
                    {
                        expl = ExplFile.Read(_weapon.Read(explEntry));
                    }
                    catch (InvalidDataException)
                    {
                        // не 0x318 байт - игнорируем
                    }
                }
            }
        }

        CptFile? cpt = null;
        var cptEntry = _weapon.Find(agent.ControlPoints.ResourceName);
        if (cptEntry != null)
        {
            using var cptStream = new MemoryStream(_weapon.Read(cptEntry), writable: false);
            cpt = ControlResourceReader.ReadCptFile(cptStream, cptEntry.FileName);
        }

        return new BulletInfo(bulletName, ctl.RootParams.MaxTangentialSpeed.Y, CollectEffects(ctl, cpt), explName, expl, startOnEnd);
    }

    private static List<WeaponEffectRef> CollectEffects(CtlFile ctl, CptFile? cpt)
    {
        var result = new List<WeaponEffectRef>();
        for (var i = 0; i < ctl.CommandLists.Count; i++)
        {
            foreach (var cmd in ctl.CommandLists[i].Records)
            {
                var opcode = (uint)cmd.Opcode;
                if (opcode is not (3 or 4 or 5))
                    continue;

                if (string.IsNullOrEmpty(cmd.Resource.ResourceName))
                    continue;

                EffectPlacement? placement = null;
                if (opcode == 4 && cpt != null)
                {
                    int a = (int)cmd.Arg0, b = (int)cmd.Arg1, c = (int)cmd.Arg2;
                    if (a >= 0 && b >= 0 && c >= 0 && a < cpt.Records.Count && b < cpt.Records.Count && c < cpt.Records.Count)
                        placement = BuildPlacement(cpt.Records[a], cpt.Records[b], cpt.Records[c]);
                }

                result.Add(new WeaponEffectRef(i, opcode, cmd.Resource.ResourceName, unchecked((int)cmd.Arg3), placement));
            }
        }

        return result;
    }

    private static Vector3 V(Vec3 v) => new(v.X, v.Y, v.Z);

    /// <summary>Повторяет ветвление SPAWN_EFFECT_FROM_3_JOINTS (Control.dll, 0x100028A0).</summary>
    private static EffectPlacement BuildPlacement(CptRecordRaw r0, CptRecordRaw r1, CptRecordRaw r2)
    {
        Vector3 d0 = V(r0.Direction), d1 = V(r1.Direction), d2 = V(r2.Direction);
        var position = (V(r0.Position) + V(r1.Position) + V(r2.Position)) / 3f;

        if (d0 == d1 && d0 == d2)
        {
            // все три равны: направление вперёд, масштаб - длина вектора
            var length = d0.Length();
            var forward = length > 1e-6f ? d0 / length : Vector3.UnitY;
            var up = MathF.Abs(forward.Z) < 0.99f ? Vector3.UnitZ : Vector3.UnitX;
            var side = Vector3.Normalize(Vector3.Cross(forward, up));
            var third = Vector3.Cross(forward, side);
            return new EffectPlacement(forward * length, side * length, third * length, position);
        }

        if (d0 != d1 && d0 != d2 && d1 != d2)
            return new EffectPlacement(d0, d1, d2, position);

        // два равны: BuildBasisFromPrimaryAxis(повторяющийся, другой)
        Vector3 primary, secondary;
        if (d0 == d2)
        {
            primary = d0;
            secondary = d1;
        }
        else if (d0 == d1)
        {
            primary = d0;
            secondary = d2;
        }
        else
        {
            primary = d1;
            secondary = d0;
        }

        var x = Vector3.Normalize(primary);
        var y = secondary - x * Vector3.Dot(secondary, x);
        y = y.LengthSquared() > 1e-8f ? Vector3.Normalize(y) : Vector3.Normalize(Vector3.Cross(x, Vector3.UnitZ));
        return new EffectPlacement(x, y, Vector3.Cross(x, y), position);
    }
}
