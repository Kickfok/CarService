<#
.SYNOPSIS
    Создает базу CarService и загружает тестовые данные.

.DESCRIPTION
    Выполняет 01-schema.sql и 02-seed.sql через sqlcmd с проверкой подлинности Windows.
    По умолчанию работает с SQL Server LocalDB, который ставится вместе с Visual Studio.

.PARAMETER Server
    Имя экземпляра SQL Server. По умолчанию (localdb)\MSSQLLocalDB.

.PARAMETER Recreate
    Удалить существующую базу CarService перед созданием. Все данные в ней будут потеряны.

.EXAMPLE
    .\Setup-Database.ps1

.EXAMPLE
    .\Setup-Database.ps1 -Server ".\SQLEXPRESS" -Recreate
#>
[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [switch]$Recreate
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$imagesDir = Join-Path $scriptDir 'seed-images'

function Invoke-SqlScript([string[]]$Arguments) {
    # -E: проверка подлинности Windows, -C: доверять сертификату сервера, -b: код ошибки при сбое скрипта,
    # -f 65001: скрипты в UTF-8.
    & sqlcmd -S $Server -E -C -b -f 65001 @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd завершился с кодом $LASTEXITCODE."
    }
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw 'Не найдена утилита sqlcmd. Установите SQL Server Command Line Utilities или SQL Server Management Studio.'
}

if ($Server -like '(localdb)\*') {
    $instance = $Server.Substring('(localdb)\'.Length)
    Write-Host "Запуск экземпляра LocalDB $instance..."
    & sqllocaldb start $instance | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Не удалось запустить LocalDB $instance. Проверьте установку: sqllocaldb info."
    }
}

if ($Recreate) {
    Write-Host 'Удаление существующей базы CarService...'
    Invoke-SqlScript @('-d', 'master', '-Q',
        "IF DB_ID(N'CarService') IS NOT NULL BEGIN ALTER DATABASE CarService SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE CarService; END")
}

Write-Host 'Создание схемы...'
Invoke-SqlScript @('-d', 'master', '-i', (Join-Path $scriptDir '01-schema.sql'))

Write-Host 'Загрузка тестовых данных...'
# Путь передается переменной окружения: sqlcmd читает их как переменные сценария, а ключ -v
# неверно разбирает значения с двоеточием (D:\...).
$env:SeedImagesPath = $imagesDir
try {
    Invoke-SqlScript @('-d', 'master', '-i', (Join-Path $scriptDir '02-seed.sql'))
}
finally {
    Remove-Item Env:\SeedImagesPath -ErrorAction SilentlyContinue
}

Write-Host ''
Write-Host "Готово. База CarService создана на сервере $Server." -ForegroundColor Green
Write-Host 'Вход в приложение: admin1 / admin1 (администратор) или user1 / user1 (пользователь).'
