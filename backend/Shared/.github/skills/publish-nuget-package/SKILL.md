---
name: publish-nuget-package
description: "Публикация NuGet-пакетов Shared.* (MaksimWest00.SharedCore, MaksimWest00.SharedFramework, MaksimWest00.SharedKernel) в GitHub Packages. Use when: publish package, публикация пакета, dotnet pack, dotnet nuget push, выпустить новую версию пакета, залить пакет в github feed."
argument-hint: "[core|framework|kernel|all]"
---

# Публикация NuGet-пакетов Shared.\*

Пакеты собираются из проектов в этом репозитории и публикуются в GitHub Packages feed `github`
(`https://nuget.pkg.github.com/maksimwest00/index.json`, источник задан в пользовательском `%APPDATA%\NuGet\NuGet.Config`, не в репозитории).

## Состав пакетов

| Пакет (PackageId)              | Проект                                     | Папка              |
| ------------------------------ | ------------------------------------------ | ------------------ |
| `MaksimWest00.SharedKernel`    | `Shared.Kernel/Shared.Kernel.csproj`       | `Shared.Kernel`    |
| `MaksimWest00.SharedCore`      | `Shared.Core/Shared.Core.csproj`           | `Shared.Core`      |
| `MaksimWest00.SharedFramework` | `Shared.Framework/Shared.Framework.csproj` | `Shared.Framework` |

Порядок зависимостей: `Shared.Kernel` → `Shared.Core` → `Shared.Framework`.
Публиковать нужно снизу вверх, иначе потребитель не сможет восстановить зависимости.

## Когда использовать

- Нужно выпустить новую версию одного из пакетов `MaksimWest00.Shared*`.
- Нужно опубликовать все три пакета сразу.
- Нужно проверить, что версия и `PackageId` в `.csproj` корректны перед публикацией.

## Предварительные условия

1. Источник `github` настроен **в пользовательском** конфиге `%APPDATA%\NuGet\NuGet.Config` (не в репозитории). Добавляется один раз:

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/maksimwest00/index.json" `
  --name github `
  --username maksimwest00 `
  --password "<PAT>"
```

Без флага `--store-password-in-clear-text` пароль на Windows шифруется через DPAPI. Токен вводится вручную в терминале и **никогда не передаётся в чат/коммит**.

2. PAT должен иметь право `write:packages`. При компрометации — отозвать и создать новый.
3. Версия задаётся в `PropertyGroup` соответствующего `.csproj` (`<Version>`). Перед публикацией поднимите её вручную — пакет с уже существующей версией GitHub отклонит.

## Безопасная передача токена

- Токен хранится только в `%APPDATA%\NuGet\NuGet.Config` (вариант A) либо в переменной окружения пользователя (вариант B).
- Нельзя: класть PAT в `SKILL.md`, `NuGet.Config` репозитория, `.env`, `appsettings.json`; использовать `--store-password-in-clear-text`; коммитить файл с паролем.
- При публикации флаг `--api-key` не нужен — `dotnet` берёт сохранённый пароль по имени источника `github`.
- Если источник `github` удалён из проектного `NuGet.Config`, mapping на `MaksimWest00.*` всё равно должен ссылаться на ключ `github`.

## Процедура

### 1. Проверить версию и PackageId

Откройте `.csproj` нужного проекта и убедитесь, что:

- `<PackageId>` = `MaksimWest00.<Имя>`;
- `<RepositoryUrl>` заполнен.

**Обязательно проверьте, что `<Version>` ещё не опубликована.** Иначе `push` вернёт `409 Conflict`.

Как проверить версию в feed перед `pack`:

```powershell
dotnet package search MaksimWest00.SharedKernel --source github --exact-match
```

Либо просто запустить push с `--skip-duplicate` — `dotnet` сообщит `409`/`already exists`, но не сломает остальные пакеты:

```powershell
dotnet nuget push <путь>.nupkg --source github --skip-duplicate
```

**Предупреждение пользователю (обязательно).** Если `<Version>` в `.csproj` уже присутствует в feed, агент **не публикует молча**, а сообщает:

> Версия `X.Y.Z` пакета `MaksimWest00.Shared<Имя>` уже опубликована в feed. Поднимите `<Version>` в `<проект>.csproj` (например, до `X.Y.Z+1`) или подтвердите публикацию с `--skip-duplicate`.

Только после подтверждения/правки версии продолжать `pack` и `push`.

### 2. Опубликовать один пакет

Из папки проекта (например, `Shared.Core`):

```powershell
dotnet pack -c Release
dotnet nuget push .\bin\Release\MaksimWest00.SharedCore.0.0.1.nupkg --source github
```

Имя `.nupkg` = `<PackageId>.<Version>.nupkg`, поэтому при смене версии поменяйте имя файла.

### 3. Опубликовать все пакеты по порядку

Из корня `backend/Shared`:

```powershell
# 1. Kernel
dotnet pack Shared.Kernel -c Release
dotnet nuget push .\Shared.Kernel\bin\Release\MaksimWest00.SharedKernel.<VERSION>.nupkg --source github

# 2. Core
dotnet pack Shared.Core -c Release
dotnet nuget push .\Shared.Core\bin\Release\MaksimWest00.SharedCore.<VERSION>.nupkg --source github

# 3. Framework
dotnet pack Shared.Framework -c Release
dotnet nuget push .\Shared.Framework\bin\Release\MaksimWest00.SharedFramework.<VERSION>.nupkg --source github
```

### 4. Проверить результат

```powershell
dotnet nuget list source
```

Пакет появится на `https://github.com/maksimwest00?tab=packages`.

## Токен протух (401/403)

Если `dotnet nuget push` вернул `401 Unauthorized` или `403 Forbidden` — PAT истёк, отозван или не имеет права `write:packages`. Действия агента:

1. Сообщить пользователю, что токен неактуален.
2. Выдать команду для терминала с плейсхолдером `<НОВЫЙ_PAT>` — пользователь подставляет токен сам:

```powershell
dotnet nuget update source github `
  --username maksimwest00 `
  --password "<НОВЫЙ_PAT>"
```

Альтернатива, если `update` не сработал:

```powershell
dotnet nuget remove source github
dotnet nuget add source "https://nuget.pkg.github.com/maksimwest00/index.json" `
  --name github `
  --username maksimwest00 `
  --password "<НОВЫЙ_PAT>"
```

3. Дождаться, пока пользователь выполнит команду в терминале (агент не принимает PAT в чате и не вводит его сам).
4. Проверить `dotnet nuget list source` и повторить push.

Пароль перезаписывается там же, где хранится сейчас — в `%APPDATA%\NuGet\NuGet.Config`, зашифрованным через DPAPI. Флаг `--store-password-in-clear-text` не использовать.

## Частые ошибки

| Ошибка                          | Причина                                          | Решение                                                            |
| ------------------------------- | ------------------------------------------------ | ------------------------------------------------------------------ |
| `401 Unauthorized`              | Токен истёк или отозван                          | Перезаписать источник новым PAT (см. раздел выше)                  |
| `403 Forbidden`                 | Нет прав `write:packages` или неверный PAT       | Пересоздать PAT, обновить источник                                 |
| `409 Conflict`                  | Версия уже опубликована                          | Поднять `<Version>` и повторить                                    |
| `NU1101` при сборке потребителя | Пакет не попал в mapping                         | В `NuGet.Config` источник `github` должен матчить `MaksimWest00.*` |
| `Cannot find .nupkg`            | Имя файла не совпадает с `<PackageId>.<Version>` | Проверить содержимое `bin/Release`                                 |

## Правила

- Никогда не публиковать пакет с версией, которая уже есть в feed.
- Соблюдать порядок Kernel → Core → Framework.
- Не коммитить PAT в репозиторий.
- После правки `.csproj` пересобрать перед `pack`.
