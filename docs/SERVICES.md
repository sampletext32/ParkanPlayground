# services.dll: службы игры

Сводка по реверсу `services.dll` (проект Ghidra `parkan1`, 2373 функции; значительная часть - STL и locale из статически слинкованной C++-библиотеки).
Игровые службы: конфигурация, менеджер ресурсов, дисплей, звуковой сервер, таймер, сеть. Разобрано только то, что подтверждено кодом или файлами; остальное
помечено «не выяснено».

## Классы

| Класс | Назначение |
|---|---|
| `CConfiguration` | чтение конфигурационных файлов: `LoadIniFile`, `LoadCfgFile`; значения `ConfigValue` (тип, скаляр, строка) |
| `CResManager` | менеджер ресурсов интерфейса: текстуры, шрифты, таблицы цвета, звуки, строки; читает `ui/game_resources.cfg` и `data/TextRes.cfg` |
| `CDisplay` | список драйверов Direct3D, режимы экрана, выбор и применение настроек (`RebuildDisplayDriverList`, `SelectD3DDriver`, `FindAndSetMostSuitableScreenSettings`...) |
| `CGUIServer` | GUI-сервер (создаётся дисплеем, `CDisplay::GetGUIServer`); методы не разобраны |
| `CSoundServer` | звуковой сервер (`ctor`, `init`, `delete`) |
| `CTimer` | таймер на `QueryPerformanceCounter` (`get_perf_counter`, `QueryPerfFreq`) |
| `CNetManager` | сеть: обёртка над интерфейсом `INetworkInterface` из `Net.dll` (DirectPlay) |

## Конфигурационные файлы `.cfg`

Блоки вида `object <имя> ... end`, внутри строки `ключ = значение`; значение - число или строка в кавычках; `//` начинает комментарий. Ключи в `CConfiguration`
хранятся как `<имя блока>#<ключ>` (`ConcatWithHashSeparator`). Менеджер ресурсов (`CResManager::LoadResourceConfig`, 0x1000C780) использует ключи блока:

| Ключ | Значение |
|---|---|
| `desc` | `"resource"` - блок ресурсов; `"font-substitute"` - подмена шрифта под разрешение |
| `library` | файл библиотеки (NRes) или DLL со строками; путь относительно папки игры |
| `libtype` | `"single"` - файл открывается `niOpenResFileEx(файл, 8)`; иначе (`"multi"`) - `niOpenResFileEx(файл, 0)`; один файл открывается один раз |
| `type` | вид ресурсов (таблица ниже) |
| остальные | `ИМЯ = индекс записи` (число) или `ИМЯ = "имя файла"` (звуки) |

Виды `type`:

| `type` | Что создаётся | Хранилище в `CResManager` | Пример |
|:--:|---|---|---|
| 1 | текстура (`I3DRender::CreateTexture`, запись NRes по индексу) | вектор +0x04 | `textures` -> `ui\ui.lib` |
| 2 | растровый шрифт (`CreateBitmapFont`) | вектор +0x10 | `fonts` -> `ui\font.lib` |
| 3 | таблица цвета (`LoadColorLookup`) | вектор +0x28 | (в файлах первой части не встречается) |
| 4 | звук WAVE (`LoadWaveSoundBuffer`, режим 0) | вектор +0x1C | `sounds`, `voices` |
| 5 | зацикленный звук (`LoadWaveSoundBuffer`, режим 2) | вектор +0x1C | `looped_sounds` |
| 6 | строки: `library` - DLL, значения - идентификаторы строк, `LoadStringA` | дерево +0x34 | `text_resources` -> `data\TextRes.dll` |

`font-substitute` с `dimension_x`/`dimension_y` задаёт, какой именно шрифт подставлять под логические имена `MENU_FONT`, `HELP_FONT`, `GAME_FONT`,
`TOOL_FONT` для разрешений 640x480, 800x600, 1024x768 (в `ui/game_resources.cfg` три блока).

Запросы: `GetTextureByName`, `GetBitmapFontByName`, `GetColorLookupByName`, `GetSoundBufferByName` (поиск по имени среди загруженных библиотек, при
отсутствии - значение по умолчанию), `GetTextStringByName`, `GetTextStringById` (строки локализации из `TextRes.dll`; результат кэшируется).

## Сеть

`CNetManager` (1264 байта) работает через `INetworkInterface` (получается `GetNetworkInterfaceAndEnableDirectPlay` из `Net.dll`; сама `Net.dll` в Ghidra
пока не разобрана). По именам функций:

- поставщики услуг: `RefreshServiceProviderNameList`, `SetServiceProviderIndex`, `LaunchSelectedServiceProvider`, `SetSelectedTcpIpHostAddress`;
- сессии: `RefreshDiscoveredSessionNameList`, `SetSessionIndex`, `JoinDiscoveredSession`, `CreateSession`, `CloseSession`, `GetNetworkSessionStatus`;
- игроки: `CreatePlayerWithNameAndPassword`, `HasPlayerWithName`, `HasPlayerWithNameAndPassword`, `GetNetworkPlayerData`,
  `SetHostNetworkPlayerData`, `SetClientNetworkPlayerData`;
- лобби: `GetHasLobbyConnectionSettings`.

Идентификаторы интерфейсов в `QueryInterface`: 0 и 0x803 - сам `INetManager`, 0x802 - `INetSessionBrowser`.
`NetworkPlayerData` (1080 байт): `+0x00` число игроков, `+0x18` имя (512 байт), `+0x218` пароль (512 байт), `+0x418` не выяснено.

## Не выяснено

- устройство `CDisplay` и источник его настроек (формат `Iron_3D.ini`);
- формат `.ini` (`LoadIniFile`) - файлы `ArealMap.ini`, `Behavior.ini`, `Iron_3D.ini`;
- протокол сети: пакеты и сообщения (в `Net.dll`), порядок рукопожатия;
- методы `CGUIServer` и `CSoundServer`.
