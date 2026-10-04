# База данных CarService

| Файл | Назначение |
|---|---|
| [`01-schema.sql`](01-schema.sql) | Создает базу `CarService` и все таблицы модели Entity Framework |
| [`02-seed.sql`](02-seed.sql) | Загружает тестовые данные: роли, пользователей, 17 услуг с изображениями, клиентов и записи на услуги |
| [`Setup-Database.ps1`](Setup-Database.ps1) | Выполняет оба скрипта одной командой |
| [`seed-images/`](seed-images) | Изображения услуг для тестовых данных |

## Быстрая установка (LocalDB)

```powershell
cd database
.\Setup-Database.ps1
```

Скрипт запускает экземпляр LocalDB `MSSQLLocalDB`, создает базу и загружает данные.
Повторный запуск на уже созданной базе остановится с сообщением и ничего не изменит.

## Другой сервер

```powershell
.\Setup-Database.ps1 -Server ".\SQLEXPRESS"
```

Используется проверка подлинности Windows. После создания базы укажите тот же сервер в строке подключения
`CarServiceEntities` в [CarService/App.config](../CarService/App.config).

> [!NOTE]
> Изображения загружаются функцией `OPENROWSET(BULK ...)`, поэтому файлы из `seed-images` должна читать
> учетная запись службы SQL Server. LocalDB работает от имени текущего пользователя, и для нее это выполняется
> всегда. Для полноценного SQL Server скопируйте папку проекта туда, куда у службы есть доступ, например в `C:\CarService`.

## Пересоздание базы

```powershell
.\Setup-Database.ps1 -Recreate
```

> [!CAUTION]
> Ключ `-Recreate` удаляет базу `CarService` вместе со всеми данными.

## Ручной запуск скриптов

Через `sqlcmd`:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -b -f 65001 -i 01-schema.sql
$env:SeedImagesPath = "$PWD\seed-images"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -b -f 65001 -i 02-seed.sql
```

Путь к изображениям передается переменной окружения `SeedImagesPath`: `sqlcmd` читает переменные окружения
как переменные сценария.

В SQL Server Management Studio: откройте скрипт, включите **Запрос → Режим SQLCMD** и для `02-seed.sql`
добавьте в начало строку `:setvar SeedImagesPath "C:\путь\к\database\seed-images"`.

## Тестовые данные

| Таблица | Записей | Примечание |
|---|---:|---|
| `Role` | 2 | 1 - Администратор, 2 - Пользователь |
| `User` | 2 | `admin1` / `admin1`, `user1` / `user1` |
| `Gender` | 2 | `м`, `ж` |
| `Service` | 17 | в каждом диапазоне фильтра скидок есть хотя бы одна услуга |
| `Client` | 6 | |
| `ClientService` | 6 | записи на 5 услуг: их нельзя удалить из программы |

## Пароли

В колонке `User.Password` хранится хеш в формате `PBKDF2$SHA256$<итерации>$<соль>$<хеш>`.
Чтобы добавить пользователя, получите хеш методом `UserRegistration.PasswordHasher.Hash`:

```powershell
Add-Type -Path ..\UserRegistration\bin\Debug\UserRegistration.dll
$hash = [UserRegistration.PasswordHasher]::Hash('новый_пароль')
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -d CarService -Q "INSERT INTO dbo.[User] (Login, Password, RoleId) VALUES (N'manager', N'$hash', 2)"
```

## Отличия от исходной учебной базы

- Имя базы и контекста записано латиницей: `CarService`. В исходной версии первая буква была кириллической `С`.
- `User.Login` и `User.Password` имеют тип `nvarchar` вместо `nchar(50)`, `Password` расширен до 128 символов под хеш.
- Таблица `sysdiagrams` не создается: ее создает SQL Server Management Studio по необходимости.
