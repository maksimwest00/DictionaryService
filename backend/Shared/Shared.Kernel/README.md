# MaksimWest00.SharedKernel

Самая стабильная часть Shared-кода: `Error` + фабрики, `Errors`, `ErrorType`, `GeneralErrors`, `ErrorsJsonConverter`, обёртки `Result` / `UnitResult`.

Пакет не знает ничего про Web, EF Core и конкретный сервис. В зависимостях допускается только `CSharpFunctionalExtensions`.

## Установка

```powershell
dotnet add package MaksimWest00.SharedKernel --version 0.1.0
```

Источник — GitHub Packages (`https://nuget.pkg.github.com/maksimwest00/index.json`). Авторизация через PAT с правом `read:packages` (локально), в CI — `secrets.GITHUB_TOKEN`.

## Регламент версионирования (SemVer)

Версия пакета — `MAJOR.MINOR.PATCH`. Повышается **одна** компонента за релиз, выбирается старшая по значимости из внесённых изменений.

### MAJOR

Breaking change в публичном API:

- удалён или переименован публичный тип/метод/свойство (например, фабрика `Error.NotFound()`);
- изменена сигнатура публичного метода (параметры, тип возврата);
- сужен доступ (`public` → `internal`);
- изменён контракт поведения, ломающий существующих потребителей (исключения, формат, семантика).

Пример: `0.1.0` → `1.0.0`.

### MINOR

Новая функциональность, совместимая назад:

- новый публичный тип, метод, перегрузка, extension-метод;
- новый optional-параметр в конце сигнатуры.

Пример: `0.1.0` → `0.2.0`.

### PATCH

Багфикс без изменения публичного API:

- правки внутри тел методов без смены сигнатур;
- внутренние рефакторинги (`private`, `internal`);
- обновление зависимостей без изменения контракта.

Пример: `0.1.0` → `0.1.1`.

Если изменения затрагивают несколько уровней — действует старший: breaking + feature = **MAJOR**.

### Превью-версии

Для промежуточных публикаций (dev-ветки) используется суффикс: `0.2.0-preview-1`, `0.2.0-preview-2`. Превью не вытесняет стабильную версию из production-stream.

## Правило git-тегов

- На каждый релиз ставится аннотированный тег `vX.Y.Z` на коммит, в котором поднята `<Version>` в `Shared.Kernel.csproj`.
- Тег и bump версии — в одном коммите, чтобы релиз был воспроизводим.
- В release notes указывается тег и ссылка на коммит релиза.

```powershell
# bump версии в Shared.Kernel.csproj
git add backend/Shared/Shared.Kernel
git commit -m "feat(kernel): ... ; bump MaksimWest00.SharedKernel to 0.2.0"
git tag v0.2.0
git push origin main --tags
```

Ссылка на коммит для release notes:

```powershell
git log v0.2.0 -1 --format="%H %s"
```

## Публикация

Локально:

```powershell
dotnet pack Shared.Kernel -c Release
dotnet nuget push .\Shared.Kernel\bin\Release\MaksimWest00.SharedKernel.0.1.0.nupkg --source github
```

В CI — job `.github/workflows/publish-shared-kernel.yml`: на push в `main` (при изменениях в `Shared.Kernel`) и по `workflow_dispatch`. Версия берётся из input → git-тега `vX.Y.Z` → `<Version>` в `.csproj`.
