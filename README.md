# Reverse Engineering игры Parkan Железная стратегия 1998

<div align="center">
  <img width="300" height="300" src="https://github.com/user-attachments/assets/dcd9ac8f-7d30-491c-ae6c-537267beb7dc" alt="x86 Registers" />
  <img width="817" height="376" alt="Image" src="https://github.com/user-attachments/assets/c4959106-9da4-4c78-a2b7-6c94e360a89e" />
</div>


## Сборка проекта

Проект написан на C# под `.NET 9`

Вам должно хватить `dotnet build` для сборки всех проектов отдельно.

Все приложения кросс-платформенные, в том числе UI.

### Состояние проекта

- Поддержка всех `NRes` файлов - звуки, музыка, текстуры, карты и другие файлы. Есть документация.
- Поддержка всех `TEXM` текстур. Есть документация.
- Поддержка файлов миссий `.tma`.
- Поддержка шрифтов TFNT.
- Поддержка файлов скриптов `.scr`.
- Поддержка файлов параметров `.var`.
- Поддержка файлов схем объектов `.dat`.


### Структура проекта

Внимание! 

Проект делается как небольшой PET, поэтому тут может не быть 
- чёткой структуры
- адекватных названий
- комментариев 

Я конечно стараюсь, но ничего не обещаю.

## Для Reverse Engineering-а использую Ghidra

### Наблюдения 

- Игра использует множество стандартных библиотек, в частности stl_port, vc++6 и другие. Если хотите что-то изучить в игре, стоит поискать по строкам и сигнатурам, что именно используется в конкретной `dll`.
- Строки в основном используются двух форматов - `char*` и `std::string`. Последняя состоит из 16 байт - `undefined4, char* data, int length, int capacity`.
- В игре очень много `inline` функции, которые повторяются по куче раз в бинарнике. 
- Игра загружает и выгружает свои `dll` файлы по несколько раз, так что дебаг с `Memory Map` очень затруднён.
- Игра активно и обильно течёт по памяти, оставляя после чтения файлов их `MapViewOfFile` и подобные штуки.
- Игра нормально не работает на Win10. Мне помог dgVoodoo. Хотя с ним не работает `MisEditor`.

## Как быстро найти текст среди всех файлов игры

```shell
grep -rl --include="*" "s_tree_05" .
```

## Как быстро найти байты среди всех файлов игры

```shell
grep -rlU $'\x73\x5f\x74\x72\x65\x65\x5f\x30\x35' .
```

## Как работает игра

Главное меню:

Игра сканирует хардкод папку `missions` на наличие файлов миссий. (буквально 01, 02, 03 и т.д.)

Сначала игра читает название миссии из файла `descr` - тут название для меню.

- Одиночные игры - `missions/single.{index}/descr`
- Тренировочные миссии - `missions/tutorial.{index}/descr`
- Кампания - `missions/campaign/campaign.{index1}/descr`
  * Далее используются подпапки - `missions/campaign/campaign.{index1}/mission.{index2}/descr`

Как только игра не находит файл `descr`, заканчивается итерация по папкам (понял, т.к. пробуется файл 05 - он не существует).

Загрузка миссии:

Читается файл `ui/game_resources.cfg`
Из этого файла загружаются ресурсы
- `library = "ui\\ui.lib"` - загружается файл `ui.lib`
- `library = "ui\\font.lib"` - загружается файл `font.lib`
- `library = "sounds.lib"` - загружается файл `sounds.lib`
- `library = "voices.lib"` - загружается файл `voices.lib`

Затем игра читает `save/saveslots.cfg` - тут слоты сохранения

Затем `Comp.ini` - тут системные функции, которые используются для загрузки объектов.

```
IComponent ** LoadSomething(undefined4, undefined4, undefined4, undefined4)
```

- `Host.url` - этого файла нет
- `palettes.lib` - тут палитры, но этот NRes пустой
- `system.rlb` - не понятно что
- `Textures.lib` - тут текстуры
- `Material.lib` - тут какие-то материалы - не понятно
- `LightMap.lib` - видимо это карты освещения - не понятно
- `sys.lib` - не понятно


- `ScanCode.dsc` - текстовый файл с мапом клавиш
- `command.dsc` - текстовый файл с мапом клавиш

Тут видимо идёт конфигурация ввода

- `table_1.man` - текстовый файл
- `table_2.man` - текстовый файл
- `hero.man` - текстовый файл
- `addition.man` - текстовый файл
- Снова `table_1.man`
- Снова `table_1.man`
- `M1.tbl` - текстовый файл
- Снова `table_2.man`
- Снова `table_2.man`
- `M2.tbl` - текстовый файл
- Снова `hero.man`
- Снова `hero.man`
- `HERO.TBL`
- Снова `addition.man`


- `ui/hq.cfg`
- Снова `ui/hq.cfg`


Дальше непосредственно читается миссия

- `mission.cfg` - метадата миссии
- `units\\units\\prebld\\scr_pre1.dat` из метаданных `object prebuild` - `cp` файл (грузятся подряд все)
- Опять `ui/hq.cfg`
- `mistips.mis` - описание для игрока (экран F1)
- `scancode.dsc` - хз
- `command.dsc` - хз
- `ui_hero.man` - хз
- `ui_bots.man` - хз
- `ui_hq.man` - хз
- `ui_other.man` - хз
- Цикл чтения курсоров
  * `ui/cursor.cfg` - тут настройки курсора.
  * `ui/{name}` - курсор
- Снова `mission.cfg` - метадата миссии
- `descr` - название
- `data/textres.cfg` - конфиг текстов
- Снова `mission.cfg` - метадата миссии
- Ещё раз `mission.cfg` - метадата миссии
- `ui/minimap.lib` - NRes с текстурами миникарты. 
- `messages.cfg` - Tutorial messages

УРА НАКОНЕЦ-ТО `data.tma`

- Из `.tma` берётся LAND строка (я её так назвал)
- `DATA\\MAPS\\SC_3\\land1.wea`
- `DATA\\MAPS\\SC_3\\land2.wea`
- `BuildDat.lst` - Behaviour will use these schemes to Build Fortification
- `DATA\\MAPS\\SC_3\\land.map`
- `DATA\\MAPS\\SC_3\\land.msh`


- `effects.rlb`

Цикл по кланам из `.tma`
- `MISSIONS\\SCRIPTS\\screampl.scr`
- `varset.var`
- `MISSIONS\\SCRIPTS\\varset.var`
- `MISSIONS\\SCRIPTS\\screampl.fml`


- `missions/single.01/sky.ske`
- `missions/single.01/sky.wea`

Дальше начинаются объекты игры
- `"UNITS\\BUILDS\\BUNKER\\mbunk01.dat"` - cp файл

## Загрузка `cp` файлов

`cp` файл - схема. Он содержит дерево частей объекта.

`cp` файл читается в `ArealMap.dll/CreateObjectFromScheme`

В зависимости от типа объекта внутри схемы (байты 4..8) выбирается функция, с помощью которой загружается схема.

Функция выбирается на основе файла `Comp.ini`.
- Для ClassBuilding (0x80000000) - вызывается функция c классом 3 (по таблице ниже Building).
- Для всех остальных - функция с классом 4 (по таблице ниже Agent).

На основе файла `Comp.ini` и первом вызове внутри функции `World3D.dll/CreateObject` ремаппинг id:

|  Class ID  |   ClassName   | Function                       |
|:----------:|:-------------:|--------------------------------|
|     1      |   Landscape   | `terrain.dll LoadLandscape`    |
|     2      |     Agent     | `animesh.dll LoadAgent`        |
|     3      |   Building    | `terrain.dll LoadBuilding`     |
|     4      |     Agent     | `animesh.dll LoadAgent`        |
|     5      |    Camera     | `terrain.dll LoadCamera`       |
|     7      |  Atmospehere  | `terrain.dll CreateAtmosphere` |
|     9      |     Agent     | `animesh.dll LoadAgent`        |
|     10     |     Agent     | `animesh.dll LoadAgent`        |
|     11     |   Research    | `misload.dll LoadResearch`     |
|     12     |     Agent     | `animesh.dll LoadAgent`        |

Будет дополняться по мере реверса.

Всем этим функциям передаётся `nres_file_name, nres_entry_name, 0, player_id`

## `fr FORT` файл

Всегда 0x80 байт
Содержит 2 ссылки на файлы:
- `.bas`
- `.ctl` - вызывается `LoadAgent`

## `.msh`

### Описание ниже валидно только для моделей роботов и зданий.
##### Land.msh использует другой формат, хотя 03 файл это всё ещё точки.

Загружается в `AniMesh.dll/LoadAniMesh`

- Тип 01 - заголовок. Он хранит список деталей (submesh) в разных LOD
  ```
  нулевому элементу добавляется флаг 0x1000000
  Содержит 2 ссылки на файлы анимаций (короткие - файл 13, длинные - файл 08)
  Если интерполируется анимация -0.5s короче чем magic1 у файла 13
  И у файла есть OffsetIntoFile13
  И ushort значение в файле 13 по этому оффсету > IndexInFile08 (это по-моему выполняется всегда)
  Тогда вместо IndexInFile08 используется значение из файла 13 по этому оффсету (второй байт)
  ```
- Тип 02 - описание одного LOD Submesh
  ```
  Вначале идёт заголовок 0x8C (140) байт
  В заголовке:
  8 Vector3 (x,y,z) - bounding box
  1 Vector4 - center
  1 Vector3 - bottom
  1 Vector3 - top
  1 float - xy_radius
  Далее инфа про куски меша
  ```
- Тип 03 - это вершины (vertex)
- Тип 06 - индексы треугольников в файле 03
- Тип 04 - скорее всего какие-то цвета RGBA или типа того
- Тип 08 - меш-анимации (см файл 01)
  ```
  Индексируется по IndexInFile08 из файла 01 либо по файлу 13 через OffsetIntoFile13
  Структура:
  Vector3 position;
  float time; // содержит только целые секунды
  short rotation_x; // делится на 32767
  short rotation_y; // делится на 32767
  short rotation_z; // делится на 32767
  short rotation_w; // делится на 32767
  ---
  Игра интерполирует анимацию между текущим стейтом и следующим по time.
  Если время интерполяции совпадает с исходным time, жёстко берётся первый стейт из 0x13.
  Если время интерполяции совпадает с конечным time, жёстко берётся второй стейт из 0x13.
  Если ни то и ни другое, тогда t = (time - souce.time) / (dest.time - source.time)
  ```
- Тип 12 - microtexture mapping
- Тип 13 - короткие меш-анимации (почему я это не дописал?)
  ```
  Буквально (hex)
  00 01 01 02 ...
  ```
- Тип 0A - ссылка на части меша, не упакованные в текущий меш (например у бункера 4 и 5 части хранятся в parts.rlb)
  ```
  Не имеет фиксированной длины. Хранит строки в следующем формате.
  Игра обращается по индексу, пропуская суммарную длину и пропуская 4 байта на каждую строку (длина).
  т.е. буквально файл выглядит так
  00 00 00 00 - пустая строка
  03 00 00 00 - длина строки 1
  73 74 72 00 - строка "str" + null terminator
  .. и повторяется до конца файла
  Кол-во элементов из файла 01 должно быть равно кол-ву строк в этом файле, хотя игра это не проверяет.
  Если у элемента эта строка равна "central", ему выставляется флаг (flag |= 1)
  ```

## `.wea`

Загружается в `World3D.dll/LoadMatManager`

По сути это текстовый файл состоящий из 2 частей:
- Материалы
  ```
  {count}
  {id} {name}
  ```
- Карты освещения
  ```
  LIGHTMAPS
  {count}
  {id} {name}
  ```

Может как-то анимироваться. Как - пока не понятно.


# Внутренняя система ID

- `1` - 
- `4` - IShader
- `5` - ITerrain
- `6` - IGameObject (0x138)
- `7` - IAtmosphereObject
- `8` - ICamera
- `9` - IQueue
- `10` - IControl
- `0xb` - IAnimation
- `0xd` - IMatManager
- `0xe` - ILightManager
- `0x10` - IBehavior
- `0x11` - IBasement
- `0x12` - ICamera2 или IBufferingCamera
- `0x13` - IEffectManager
- `0x14` - IPosition
- `0x15` - IAgent
- `0x16` - ILifeSystem
- `0x17` - IBuilding - точно он, т.к. ArealMap.CreateObject на него проверяет
- `0x18` - IMesh2
- `0x19` - IManManager
- `0x20` - IJointMesh
- `0x21` - IShade
- `0x23` - IGameSettings
- `0x24` - IGameObject2
- `0x25` - unknown (implemented by AniMesh)
- `0x26` - unknown (implemented by AniMesh)
- `0x28` - ICollObject
- `0x101` - 3DRender
- `0x105` - NResFile
- `0x106` - NResFileMetadata
- `0x107` - 3DSound
- `0x201` - IWizard
- `0x202` - IItemManager
- `0x203` - ICollManager
- `0x301` - IArealMap
- `0x302` - ISystemArealMap
- `0x303` - IHallway
- `0x304` - Distributor
- `0x401` - ISuperAI
- `0x501` - MissionData
- `0x502` - ResTree
- `0x700` - NetWatcher
- `0x701` - INetworkInterface
- `0x10d` - CreateVertexBufferData


## Опции

World3D.dll содержит функцию CreateGameSettings.
Она создаёт объект настроек и далее вызывает методы в соседних библиотеках.
- Terrain.dll - InitializeSettings
- Effect.dll - InitializeSettings
- Control.dll - InitializeSettings

Остальные наверное не трогают настройки.

| Resource ID |    wOptionID    |            Name            | Default | Description        |
|:-----------:|:---------------:|:--------------------------:|:-------:|--------------------|
|      1      |   100 (0x64)    |      "Texture detail"      |         |                    |
|      2      |   101 (0x65)    |         "3D Sound"         |         |                    |
|      3      |   102 (0x66)    |    "Mouse sensitivity"     |         |                    |
|      4      |   103 (0x67)    |   "Joystick sensitivity"   |         |                    |
|      5      | !not a setting! |    "Illegal wOptionID"     |         |                    |
|      6      |   104 (0x68)    |     "Wait for retrace"     |         |                    |
|      7      |   105 (0x69)    |     "Inverse mouse X"      |         |                    |
|      8      |   106 (0x6a)    |     "Inverse mouse Y"      |         |                    |
|      9      |   107 (0x6b)    |    "Inverse joystick X"    |         |                    |
|     10      |   108 (0x6c)    |    "Inverse joystick Y"    |         |                    |
|     11      |   109 (0x6d)    |     "Use BumpMapping"      |         |                    |
|     12      |   110 (0x6e)    |     "3D Sound quality"     |         |                    |
|     13      |    90 (0x5a)    |      "Reverse sound"       |         |                    |
|     14      |    91 (0x5b)    |  "Sound buffer frequency"  |         |                    |
|     15      |    92 (0x5c)    | "Play sound buffer always" |         |                    |
|     16      |    93 (0x5d)    | "Select best sound device" |         |                    |
|    ----     |    30 (0x1e)    |        ShadeConfig         |         | из файла shade.cfg |
|    ----     |    (0x8001e)    |                            |         | добавляет AniMesh  |


## Контакты

Вы можете связаться со мной в [Telegram](https://t.me/bird_egop).
