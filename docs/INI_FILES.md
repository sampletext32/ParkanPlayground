# Файлы `.ini` игры

В папке игры лежат файлы настроек двух разных форматов. Описано по коду `services.dll`, `Behavior.dll` и по файлам из установки.

## 1. Стандартный Windows INI: `Iron_3D.ini`

Секции `[CS]`, `[MULTIPLAYER]`, `[TEMP]`, строки `КЛЮЧ=значение`. Читается через `GetPrivateProfileStringA` (`CConfiguration::LoadIniFile`,
0x10001980: сначала список секций, затем список ключей каждой секции, затем значение; в память кладётся как `секция#ключ` с исходным именем файла) и
`StdString::IniFile_ReadInt`.

Раздел `[CS]` применяет `CDisplay::InitializeDisplayAndApplyConfig` (services.dll 0x10005720):

| Ключ | Что делает |
|---|---|
| `DISPLAY_WIDTH`, `DISPLAY_HEIGHT`, `BITDEPTH` | подбор ближайшего режима экрана из списка драйвера |
| `WINDOW_MODE` | 1 - оконный режим, окно по центру экрана |
| `RENDER_QUALITY` | 0, 1, 2 - уровень детализации текстур (`ApplyTextureDetailLevel`) |
| `REFLECTIONS` | в реестр настроек, опция `0x1A001E` |
| `EMBOSS_BUMP` | опции `0x6D000A` и `0x19001E` |
| `EMBM` | опция `0x1E001E` |
| `MOUSE_SENS` | чувствительность мыши в процентах (`/100`), опция `0x66000A`; без ключа 1.0 |
| `JOY_SENS` | то же для джойстика, опция `0x67000A` |
| `MOUSE_REV_Y`, `JOY_REV_Y` | инверсия оси Y, опции `0x6A000A`, `0x6C000A` |

Код опции - `секция << 16 | номер` в реестре настроек `World3D.dll` (`CreateGameSettings`). Остальные ключи файла (`CURRENT_D3DCARD`,
`FORCE_SOFTWARE_CURSOR`, `FORCE_CD_SOUND`, `PLAY_CD_MUSIC`, `JOY_ENABLE`, `SUBTITLES`, `SFX_VOLUME`, `CD_VOLUME`, секции `[MULTIPLAYER]`, `[TEMP]`)
читают другие части игры (в `CDisplay` их нет): не выяснено где.

Замечание: список ключей секции читается в буфер 255 байт; в `[CS]` имена ключей вместе с разделителями занимают 247 байт, то есть запас - несколько
символов; новый ключ мог бы потеряться при чтении.

## 2. Плоский `Ключ = значение`: `Behavior.ini`, `ArealMap.ini`

Без секций. Читается построчным поиском ключа (`MFile::Read(файл, "Ключ", буфер, ...)`, затем после `=` `atoi`) в `DllMain` библиотеки при загрузке;
отсутствие файла - значения по умолчанию. `//` - комментарии.

Общие ключи журнала (оба файла): `LogFile` (имя журнала), `SaveLog` (1 - писать), `MaxErrorLevel`, `DefErrorLevel`, `LookBugMode`.

`Behavior.ini` (`Behavior.dll`, `DllMain` 0x10003120), значения по умолчанию в скобках:

| Ключ | Переменная | Смысл |
|---|---|---|
| `LockBehaviour` (0) | `g_lock_behaviour` | отладка: блокировка поведения |
| `ShowVectors` (0) | `g_show_vectors` | рисовать векторы движения |
| `ShowMessage` (не задан) | `g_show_message` | показывать сообщения |
| `NoZBuffer` | `g_nozbuffer` | рисовать без Z-буфера |
| `UseDebugKey` | `g_usedebugkey` | отладочные клавиши (`PressDebugKey`) |
| `GiveDefaultOrder` (0), `DefaultOrderPhase` (1) | `g_give_default_order`, `g_default_order_phase` | выдавать приказ по умолчанию и его фаза |
| `DeterminMode` | `g_determin_mode` | режим детерминизма (смысл не проверен) |
| `ImmortalHero` (0) | `g_immortal_hero` | бессмертный герой |
| `UseWizard` | `g_usewizard` | использовать [Wizard](WIZARD.md); как именно - не проверено |

`ArealMap.ini`: те же ключи журнала и отладки отрисовки (`ShowAreals`, `Areal_NoZBuffer`, `HallWay_NoZBuffer`, `EdgeUp`, `RunBehDebug`); код чтения
в `ArealMap.dll` не просматривался.

## 3. `Comp.ini`

Своя таблица `CID  DLL  Функция  // комментарий`; описана в [README.md](../README.md) (загрузка компонентов, `Comp.ini`).
