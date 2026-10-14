# World3D.dll: игровой мир, очередь сообщений, ввод

Сводка по `World3D.dll` (проект Ghidra `parkan1`, 593 функции; из них безымянных осталось около 100 - почти все CRT после 0x10017D83). Это корневая
библиотека игры: `iron_3d.exe` вызывает её экспорты `std*`, она загружает остальные компоненты и держит общую очередь объектов. Описано только
подтверждённое кодом, строками отладки или данными.

## Экспорты (группы)

| Группа | Примеры |
|---|---|
| жизненный цикл игры | `stdInitGame` (22 параметра), `stdCalculateGame`, `stdRenderGame`, `stdCloseGame`, `stdNewScreenSize`, `stdSetGameMode`/`stdGetGameMode` |
| очередь и объекты | `CreateQueue`, `GetQueue`, `CreateObject`, `AddObjectToGame`, `AddNewObjectToGame`, `DeleteGameObject`, `KillGameObject`, `SetStateForGameObjects` |
| сеть | `netCreateNetWatcher`, `CreateMirrorObject`, `AddMirrorObjectToGame`, `AddNewMirrorToGame`, `GetNetPlayerNum`, `GetSaveGameFromHost` |
| время | `SetGameTime`, `PauseGameTime`, `ResumeGameTime` |
| компоненты | `LoadComponentsAddr`, `LoadComponent`, `FreeComponentsAddr`, `CreateGameSettings`, `CreateManManager` |
| ресурсы рендера | `SetPalettesLib`, `SetTexturesLib`, `SetMaterialLib`, `SetLightMapLib`, `LoadMatManager`, `PreLoadAllTextures`, `UnloadAllTextures` |
| ввод | `stdInitDIMouse`, `UpdateManualEventsList`, `ConvertKeybScanCode`, `ConvertMouseScanCode`, `ConvertJoyScanCode`, `ConvertCommand`, `ScanCode2Str`, `CreateTranslateTable` |
| отладка | `camGetDebugFont`, `camPutDebugStr`, `camSetFontColor`, `camSetFontZ` |

Загрузка компонентов: по таблице `Comp.ini` (см. корневой [README](../README.md)), ошибки «LoadComponent: Component ID not found.».
`CreateGameSettings` создаёт реестр настроек и вызывает `InitializeSettings` в `Control.dll`, `Effect.dll`, `Terrain.dll`.

## Очередь (`Queue`, `IQueue`)

Один объект на процесс; хранит игроков (`QueuePlayers`, до 8), реестр игровых объектов и принимает сообщения. Идентификатор объекта `ObjectHandle`:

| Биты | Поле |
|:--:|---|
| 0..15 | индекс объекта у игрока |
| 16..23 | индекс игрока |
| 24..31 | класс логики (`ENewLogicID`, 1..0xC) |

Маршрут сообщения (`PackObjectHandle`): `OBJECT_ROUTE_LOCAL`, `OBJECT_ROUTE_REMOTE_MIRROR`, `OBJECT_ROUTE_REMOTE_AUTHORITY`. У каждого объекта
владелец («authority», хозяин) и «зеркала» (mirror) на других машинах; `SendCommandToObjectMirror`, `SendCommandToObjectAuthority`, `ExecuteCommand`
(0x10006C40) разбирают сообщения `GMSG_*`. Методы очереди: `Registrate`/`Unregister` (ошибки «illegal player number», «Too many objects.»),
`PutMessage`, `SendTextMessageToPlayer`, `NetworkUpdateTick`, `GetIGObject`, `QueryGameObjectsByLogicID`, `GetLandscape`.

Сообщения сетевого уровня (строки `NetWatcher`): `GMSG_CREATE_REMOTE_PLAYER`, `GMSG_APPEND_RESOURCE`, `GMSG_REMOVE_RESOURCE`, `GMSG_CHANGE_OBJECT_OWNER`,
`GMSG_SET_PLAYER_DATA`, `GMSG_CONFIRM_PLAYER_DATA`, `GMSG_MISSION_DATA_PATH`, `GMSG_TAKE_OBJECT`, `GMSG_TEXT_FOR_PLAYER`, `GMSG_SYNC_STATE`,
`GMSG_CREATE_MIRROR`, `GMSG_PAUSE_REMOTE_PLAYER`, `GMSG_KILL_PLAYER`, `GMSG_COLLISION_DETECTED`, `GMSG_END_MESSAGE_SEQ`, `SYSMSG_SET_TIME`,
`SYSMSG_SET_PLAYER_NUMBER`.

Названы в этой работе:

| Функция | Адрес | Назначение |
|---|---|---|
| `ReceiveTimeSyncMessage` | 0x10007D90 | принимает сообщение 0x3E9: задержка `g_Latency = время_хоста - timeGetTime()`, `g_game_time = timeGetTime() + задержка`; прочие сообщения пишет в `res.log` и в NetWatcher |
| `SendCreateMirrorRecursive` | 0x10009D10 | отправляет игроку `GMSG_CREATE_MIRROR` для объекта (архив, файл, родитель, шарнир, состояние) и рекурсивно для всех детей |
| `SendQueueCommandToPlayer` | 0x10006910 | команда очереди одному игроку или всем (`-1`) |
| `SerializeObjectTreeForSave` | 0x1000AA70 | упаковка объекта и детей в буфер сохранения: хэндл, имена архива и файла, родитель и шарнир, состояние |
| `LoadSavedGameObject` | 0x1000AE60 | обратное: создаёт объект по записи, ставит хэндл, регистрирует, `TransmitInitToAll`, восстанавливает состояние, цепляет к родителю |

Для каждого объекта очередь хранит имена архива и файла (`QueuePlayers[i].nres_archive_names`/`nres_file_names`, по 128 байт) - так объект создаётся
на другой машине и из сохранения.

## Ввод: ManManager и таблицы `.tbl`

`CreateManManager` создаёт `IManManager` (0x19) - обработчик ручного управления объектом игроком. Он читает таблицу привязок - обычный текстовый файл в папке игры (`m1.tbl`, `m2.tbl`, `hero.tbl`; их имена указаны в данных управления,
см. [ControlLib](../ControlLib/README.md)). Формат проверен на трёх файлах игры (88 строк `KEY`, 24 строки `MOUSE`):

```
KEY   SCAN_NULL SCAN_W 1 CICLS_UNKNOWN MCMD_WALK_F  1.0 0 0 0.0 0 // комментарий
```

| № | Поле | Значения |
|:--:|---|---|
| 1 | устройство | `KEY`, `MOUSE` (в данных других нет; в экспортах есть `ConvertJoyScanCode`, формат строки для джойстика не проверен) |
| 2 | клавиша-модификатор | `SCAN_NULL` или `SCAN_LSHIFT` и т.д. |
| 3 | клавиша или ось | `SCAN_*`, `SCAN_MOUSE_X`, `SCAN_MOUSE_Y`, `SCAN_LMOUSE`, `SCAN_RMOUSE`, `SCAN_MMOUSE` |
| 4 | нажатие | 1 - нажата, 0 - отпущена |
| 5 | класс предмета | `CICLS_*`: `UNKNOWN` (сам объект) 0, `TURRET` 1, `MULTIGUN` 2, `SIMPLE` 3, `CAMERA` 4, `ENGINE` 5, `STORAGE` 6, `FUELSTOR` 7, `RADAR` 8, `FIGHTSHIELD` 9, `DETECTSHIELD` 0xA, `ELEVATOR` 0xB, `DOOR` 0xC, `COMPUTER` 0xD, `GENERATOR` 0xE, `REPAIRSYS` 0xF, `ANTIGRAVSYS` 0x10, `PROJECTOR` 0x12, `POWERSTOR` 0x13, `SPOTLIGHT` 0x17 |
| 6 | команда | `MCMD_*` (таблица ниже) |
| 7 | значение | float |
| 8 | номер предмета | -1 - все, 1..n - n-й предмет класса |
| 9 | режим | `0`, `MAN_WRAP` (0) / `MAN_NOTWRAP` (1) или состояние `CIS_*` |
| 10 | нарастание | float: шаг; int: время в мс (плавный выход на значение) |

Команды `MCMD_*`: `STATE` 1, `ROTATE_X/Y/Z` 2..4, `ANGLE_X/Y` 5, 6, `FORWARD` 7, `BACK` 8, `LEFT` 9, `RIGHT` 10, `UP` 11, `DOWN` 12, `SELECT` 13,
`SELECT_NEXT` 14, `TABLE` 15, `ANGLE_Z` 0x10, `MISSILE` 0x11, `FIRE_ALL` 0x12, `WALK_F` 0x13, `WALK_B` 0x14, `LOCK` 0x15.
Состояния `CIS_*` (битовые маски): `SWITCHOFF` 0, `SWITCHON` 0x20, `SWITCH_INV` 0x40, `CONTINUEFIGHT`/`TURRETCONTROL`/`ANGLETRACE` 0x100,
`SINGLEFIGHT`/`MANUALCONTROL`/`MANUALTRACE` 0x200, `GROUPFIGHT`/`POINTTRACE` 0x400, `INFRARED_ON` 0x1000, `INFRARED_OFF` 0x2000,
`INFRARED_INV`/`CHAMELEON_INV` 0x4000.

Разбор имён: `ParseItemClassName` (0x1000D700), `ParseItemStateName` (0x1000EC40), `ParseManualCommandName` (0x1000EDF0). Применение команды -
`CManManager::ApplyManualCommand` (0x100111A0): по команде пишет в `IControl` (`SetNormSpeed`, `SetTangAccel`, `SetNormAngle`, `SetStrafeAngle`) или
в предметы (`IItemManager`); `ComputeBindingAxisValue` (0x10012050) считает значение оси с учётом чувствительности мыши и джойстика.
Привязка в памяти - `ManBinding` (0x88 байт; создана в Ghidra).
Команда `TABLE` переключает таблицу `m1.tbl` на `m2.tbl` и обратно.

## Не выяснено

- структура `ManEntry` (таблица привязок на объект) целиком; привязка в памяти (`ManBinding`, 0x88 байт) - поля `+0x5C..+0x84` соответствуют полям
  `.tbl` (код клавиши, нажатие, класс, команда, значение, номер предмета, режим, нарастание, состояние и время активации), назначение `+0x00..+0x5B`
  (вероятно имена из строки) не выяснено;
- часть сообщений `GMSG_*` (`SYSMSG` разбор в `ReceiveTimeSyncMessage`);
- остальные безымянные функции до адреса 0x10017D83.
