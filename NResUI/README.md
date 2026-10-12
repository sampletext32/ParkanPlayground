# NResUI: просмотр файлов игры

Графическое приложение (ImGui, OpenGL, .NET 9, кросс-платформенное) для просмотра форматов игры и проверки гипотез.

## Запуск

```shell
dotnet run --project NResUI -c Release
```

Приложение читает `config.json` из текущей папки (рабочей папки запуска); если файла нет, при первом старте он создаётся с пустым путём:

```json
{ "GameBasePath": "C:\\Program Files (x86)\\Nikita\\Iron Strategy" }
```

`GameBasePath` - папка игры (с `DATA`, `MISSIONS`, `UNITS`). Для проигрывателей эффектов нужны также распакованные архивы либо исходные `.rlb`/`.lib`
в этой папке. Звук воспроизводится только в Windows (`winmm`).

## Меню `File`

| Пункт | Что открывает |
|---|---|
| Open NRes | любой NRes-архив: список элементов, просмотр, экспорт |
| Open TEXM / Open TFNT TEXM | текстура / шрифтовая текстура |
| Open Mission TMA | файл миссии (`MissionTmaLib`) |
| Open SCR Scripts File | скрипты ИИ (`ScrLib`) |
| Open Varset File | переменные `.var` (`VarsetLib`) |
| Open cp .dat Scheme File | схема объекта (`CpDatLib`) |
| Open research .trf File | дерево исследований (`ResTreeLib`) |

## Панели

| Панель | Для чего | Документация |
|---|---|---|
| NRes Explorer | содержимое NRes-архива, просмотр и экспорт элементов | [NResLib](../NResLib/README.md) |
| Material Explorer | MAT0: стадии, анимации, режим смешивания, свойства поверхности | [MaterialLib](../MaterialLib/README.md) |
| Effect Player | воспроизведение одного эффекта FXID: таймлайн компонентов, режимы силы, камера, звук | [FxidLib](../FxidLib/README.md) |
| Weapon Viewer | выстрел целиком: орудие → снаряд → эффекты → взрыв; модель орудия, выбор стволов и поверхности | [ControlLib](../ControlLib/README.md), §7 |
| Инспектор меша / Viewport | меш MSH, поза и анимация | [MshLib](../MshLib/README.md) |
| SCR Explorer | секции и инструкции скриптов ИИ с названиями встроенных функций | [ScrLib](../ScrLib/README.md) |
| VARSET Explorer | переменные и константы скриптов | [VarsetLib](../VarsetLib/README.md) |
| TEXM Explorer | текстуры и страницы атласа | [TexmLib](../TexmLib/README.md) |
| Mission TMA Explorer | ареалы, кланы, объекты миссии | [MissionTmaLib](../MissionTmaLib/README.md) |
| cp .dat Scheme Explorer | дерево частей объекта | [CpDatLib](../CpDatLib/README.md) |
| Research Tree Explorer (trf) | узлы исследований и зависимости | [ResTreeLib](../ResTreeLib/README.md) |
| Binary Explorer | просмотр произвольных байтов | - |
