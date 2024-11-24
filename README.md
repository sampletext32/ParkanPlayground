# Reverse Engineering игры Parkan Железная стратегия 1998

## Сборка проекта

Проект написан на C# под `.NET 8`

Вам должно хватить `dotnet build` для сборки всех проектов отдельно.

Все приложения кросс-платформенные, в том числе UI.

### Состояние проекта

- Распаковка всех `NRes` файлов
- Распаковка всех `TEXM` текстур
  + формат 565 работает некорректно
  + не понятно назначение двух магических чисел в заголовке
- Распаковка данных миссии `.tma`. Пока работает чтение ареалов и кланов.
- Распаковка файла NL. Есть только декодирование заголовка. Формат кажется не используется игрой, а реверс бинарника игры то ещё занятие.
- Распаковка текстуры шрифта формата TFNT. Встроен прямо в UI. По сути шрифт это 4116 байт заголовка и текстура TEXM сразу после.


### Структура проекта

Внимание! 

Проект делается как небольшой PET, поэтому тут может не быть 
- чёткой структуры
- адекватных названий
- комментариев 

Я конечно стараюсь, но ничего не обещаю.

#### NResUI

UI приложение на OpenGL + ImGui.

Туда постепенно добавляю логику.

#### NResLib

Библиотека распаковки формата NRes и всех файлов, которые им запакованы. 

Есть логика импорта и экспорта. Работа не завершена, но уже сейчас можно читать любые архивы такого формата.

#### TexmLib

Библиотека распаковки текстур TEXM.

Есть логика импорта и экспорта, хотя к UI последняя не подключена.

#### NLUnpacker

Приложение распаковки NL.

Работа приостановлена, т.к. кажется игра не использует эти файлы.

#### MissionDataUnpacker

Приложение распаковки миссий `.tma`.

Готово чтение ареалов и кланов. Пока в процессе.

#### ParkanPlayground

Пустой проект, использую для локальных тестов.

#### TextureDecoder

Приложение для экспорта текстур TEXM.

Изначально тут игрался с текстурами.


## Для Reverse Engineering-а использую Ghidra

### Наблюдения 

- Игра использует множество стандартных библиотек, в частности stl_port, vc++6 и другие. Если хотите что-то изучить в игре, стоит поискать по строкам и сигнатурам, что именно используется в конкретной `dll`.
- Строки в основном используются двух форматов - `char*` и `std::string`. Последняя состоит из 16 байт - `undefined4, char* data, int length, int capacity`.
- В игре очень много `inline` функции, которые повторяются по куче раз в бинарнике. 
- Игра загружает и выгружает свои `dll` файлы по несколько раз, так что дебаг с `Memory Map` очень затруднён.
- Игра активно и обильно течёт по памяти, оставляя после чтения файлов их `MapViewOfFile` и подобные штуки.
- Игра нормально не работает на Win10. Мне помог dgVoodoo. Хотя с ним не работает `MisEditor`.

## Контакты

Вы можете связаться со мной в [Telegram](https://t.me/bird_egop).