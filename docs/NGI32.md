# Ngi32.dll: ядро движка

Сводка по реверсу `Ngi32.dll` (проект Ghidra `parkan1`). Ngi32 - нижний слой игры: работа с файлами ресурсов, 3D-рендер (DirectDraw/Direct3D 7), звук
(DirectSound), быстрая векторная математика и растеризатор. Всё, что видно в коде и не подтверждено данными, помечено «не выяснено».

## Экспорты

Экспорты (у каждого есть порядковый номер и имя; несколько имён указывают на один адрес-заглушку). Группы:

| Группа | Примеры | Что это |
|---|---|---|
| `ni*` | `niOpenResFile`, `niOpenResFileEx`, `niCreateResFile`, `niOpenResInMem`, `niGetResPath` | файлы ресурсов NRes (класс `NResFile`) |
| `ni*` | `niCreate3DRender`, `niGet3DRender`, `niGetD3DDriverAmount`, `niSelectD3DDriver`, `niGetD3DVideoModeList` | рендер и выбор драйвера Direct3D |
| `ni*` | `niCreate3DSound`, `niGet3DSound`, `niMuteSound`, `niGet3DSoundCaps` | звук (`C3DSound`) |
| `ni*` | `niGetProcAddress`, `niGetVerBuildNumber`, `niOutputDebugString`, `niSetPerformanceReport` | служебные |
| `rs*` | `rsOpenLib`, `rsLoad`, `rsFind`, `rsGetInfo`, `rsGetPackMethod`, `rsLoadPacked`, `rsLoadFast`, `rsCloseLib` | NL-архивы («RsLi»), см. [RsLiLib](../RsLiLib/README.md) |
| `mrn*`, `mrt*` | `mrnPointInPoly`, `mrnGetPlane`, `mrnCubeIntersectsPoly`, `mrnRayIntersectsPlane`, `mrtParaIntersectsPoly` | геометрические тесты |
| `ngi*`, `vrt*`, `n3d*`, `bsp*` | `ngiAlloc`, `ngiFree`, `ngiMulDiv`, `vrtLine`, `n3dPrimitive`, `bspBrowse`... | остаток старого программного рендера; в этой версии почти все - заглушки (один общий адрес `0x10002B60`/`0x10002720`) |

## Таблица FastProc

`niGetProcAddress(index)` и экспорт `FastProc` отдают таблицу указателей на функции (`0x1003AF18`), заполняемую `setup_sse_fastproc` (`0x10003110`) по
возможностям процессора: базовые реализации на FPU, затем замены MMX, 3DNow, Pentium Pro, SSE (две ветки по флагу «advanced»). Остальные DLL игры вызывают
её как `(**(code **)(g_FastProc_exref + смещение))(...)`. Известные слоты (смещение в байтах; базовая реализация):

| Смещение | Функция |
|---|---|
| 0x00 / 0x78 | `ModelToWorldBatch` (точки модели в мир, пакетом) |
| 0x04 / 0x7C | `DirectionsToWorldBatch` |
| 0x08 / 0x80 | `WorldToViewTransform` |
| 0x0C / 0x88 | `TransformNormalsBatch` |
| 0x10 / 0x70 | `MultiplyAffine` (умножение матриц 3x4) |
| 0x1C | `TransformDirection` |
| 0x20 / 0x84 | `PointWorldToLocal` |
| 0x24 / 0x8C | `RotateVectorByMatrix3x3` |
| 0x2C | `FastInvSqrt` |
| 0x30, 0x34 | `NormalizeVector` |
| 0x38 | `QuaternionToMatrix4x4` |
| 0x3C | `QuaternionToMatrix` |
| 0x40 | `MatrixToQuaternion` |
| 0x44 / 0x58 | `QuaternionSlerp` |
| 0x48 | `AccumulatePointLightPerVertex` |
| 0x4C | `AccumulateDirectionalLightPerVertex` |
| 0x50 | `Vec3_AddAndClampMin` |
| 0x54 | `PackTwoFloatColorsToARGB` |
| 0x5C / 0x74 | `MatrixMultiply` |
| 0x64 | `IsBoundingSphereInsidePlanes` |
| 0x6C | `TransformPointWorldToLocal` |
| 0x98 | `Vector3_NormalizeAndLength` |
| 0x9C | `TransformWorldPointToScaledLocal` |
| 0xA0 | `ApplyInverseTransformAndScale` |
| 0xA4 | `ApplyRotationAndScale` |
| 0xAC | `FastProc_TransformPointsAffineScaled` |
| 0xB0 | `TransformVectorWithPreScale` |
| 0xB4 | `TryIntersectSegmentWithPlane` |
| 0xB8 / 0xBC | `memcmp_64` |
| 0xC4 / 0xC8 | `FastShadePointLightIndexedStrided` / `FastShadeDirectionalLightIndexedStrided` |
| 0xCC | `FastProc_ComputePolygonPlane` (нормаль и смещение плоскости многоугольника методом Ньюэлла) |

Слоты 0x60, 0x90/0x94, 0xA8, 0xC0, 0xD0 и большая часть слотов растеризатора (0x60..0x228: копирование и заливка строк, палитровые и цветовые
преобразования, смешивание через таблицы, текстурные строки) в базовой версии - безымянные участки кода; для них названы варианты `Raster_*`.

## Классы

| Класс | Назначение |
|---|---|
| `NResFile`, `NResFileMetadata` | открытый NRes-файл (файловое отображение в память); `ResizeItem`, `DeleteItem`, `SetItemName`, `RebuildNameSortIndex` - редактирование |
| `3DRender` | устройство Direct3D 7: поверхности, состояния рендера, матрицы, примитивы, вершинные буферы, текстуры |
| `CTextureBase`, `CWritableTextureAtlas`, `CBitmapFont`, `CColorLookup`, `CVertexBuffer` | ресурсы рендера |
| `C3DSound` | звук; интерфейсы приходят по `QueryInterface` с идентификаторами 0x107 (`C3DSound`), 0x108 (`IListenerTransform`), 0x109 (`ISoundPool`) |
| `CListenerTransform` | положение слушателя: позиция, скорость, базис, углы; `GetState`/`SetState` по маске |
| `CSoundPool`, `CSoundBuffer` | набор звуковых экземпляров `SoundInstance` (DirectSound + 3D-буфер) |
| `CCDPlayer` | проигрыватель компакт-диска |

### Маска состояния звука (`CSoundPool::GetSoundState`/`SetSoundState`)

| Бит | Поле |
|---|---|
| 0x1 | флаги (в `GetSoundState`: 0x1 - второй флаг, 0x2 - первый, 0x200 - третий; точный смысл не выяснен) |
| 0x2 | множитель частоты (`pitch`; длительность в мс = `sampleCount * 1000 / (sampleRate * pitch)`) |
| 0x4 | позиция |
| 0x8 | скорость |
| 0x10 | углы внутреннего и внешнего конусов |
| 0x20 | направление конуса |
| 0x40 | громкость вне конуса |
| 0x80 / 0x100 | минимальная / максимальная дистанция |
| 0x200 | число с плавающей точкой по смещению +0x54 (не выяснено) |

## Файлы

- **NRes**: запись метаданных 0x40 байт: `+0x00` тип, `+0x0C` размер, `+0x14` имя (до 35 символов), `+0x38` смещение, `+0x3C` индекс в порядке
  сортировки по имени без учёта регистра; формат - [NResLib](../NResLib/README.md).
- **NL («RsLi»)**: [RsLiLib](../RsLiLib/README.md). Способы упаковки: LZSS, LZHUF, deflate (Info-ZIP inflate: `Inflate_HuftBuild`, `Inflate_Codes`,
  `Inflate_StoredBlock`, `Inflate_FixedBlock`, `Inflate_DynamicBlock`).
- `UnloadAllNResFiles` выгружает все открытые NRes; `ResPathList::Set` копирует список путей поиска (каждый путь дополняется `\`).

## Не выяснено

- смысл части слотов `FastProc` (0x60, 0x90, 0xA8, 0xC0, 0xD0) и растровых слотов;
- состав структуры `SoundInstance` (поля за пределами позиции, скорости, конусов);
- соответствие индексов `niGetProcAddress` слотам у остальных DLL (названы только по использованию в `Behavior.dll`, `AniMesh.dll`, `Control.dll`).
