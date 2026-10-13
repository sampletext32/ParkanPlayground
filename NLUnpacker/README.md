# NLUnpacker: распаковка NL-архивов

Консольная утилита над [RsLiLib](../RsLiLib/README.md): показывает записи NL-архива (`gamefont.rlb`, `sprites.lib`) и при указании папки сохраняет
распакованные файлы.

```bash
dotnet run --project NLUnpacker -- "C:\Program Files (x86)\Nikita\Iron Strategy\sprites.lib" out
```

Формат и способы упаковки - в README библиотеки.
