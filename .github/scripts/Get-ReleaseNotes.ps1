<#
.SYNOPSIS
    Формирует описание релиза из раздела CHANGELOG.md для указанной версии.

.DESCRIPTION
    Берет раздел "## [<версия>]" до следующего заголовка второго уровня и склеивает строки-продолжения
    пунктов списка (с отступом в два пробела) с предыдущей строкой: в описании релиза на GitHub
    одиночный перенос строки отображается как разрыв.

.EXAMPLE
    .\Get-ReleaseNotes.ps1 -Version 1.1.0 -OutFile release-notes.md
#>
param(
    [Parameter(Mandatory)] [string]$Version,
    [string]$ChangelogPath = (Join-Path $PSScriptRoot '..\..\CHANGELOG.md'),
    [string]$OutFile
)

$ErrorActionPreference = 'Stop'

$lines = Get-Content -LiteralPath $ChangelogPath -Encoding UTF8
$header = "## [$Version]"
$start = [Array]::FindIndex($lines, [Predicate[string]] { param($l) $l.StartsWith($header) })
if ($start -lt 0) {
    throw "В $ChangelogPath нет раздела '$header'."
}

$section = New-Object System.Collections.Generic.List[string]
for ($i = $start + 1; $i -lt $lines.Count -and -not $lines[$i].StartsWith('## '); $i++) {
    $line = $lines[$i]
    if ($line -match '^  \S' -and $section.Count -gt 0 -and $section[$section.Count - 1] -match '^\s*- ') {
        $section[$section.Count - 1] += ' ' + $line.Trim()
    }
    else {
        $section.Add($line)
    }
}

while ($section.Count -gt 0 -and $section[$section.Count - 1].Trim() -eq '') {
    $section.RemoveAt($section.Count - 1)
}

$repo = 'https://github.com/Kickfok/CarService'
$body = @(
    '## Установка'
    ''
    "1. Скачайте архив ``CarService-$Version.zip`` ниже и распакуйте его."
    '2. Создайте базу данных: откройте PowerShell в распакованной папке и выполните `powershell -ExecutionPolicy Bypass -File database\Setup-Database.ps1` (нужны SQL Server LocalDB и `sqlcmd`).'
    '3. Запустите `app\CarService.exe`. Вход: `admin1` / `admin1` или `user1` / `user1`.'
    ''
    "Подробнее - в [README]($repo#быстрый-старт) и [руководстве пользователя]($repo/blob/v$Version/docs/USER_GUIDE.md)."
    ''
    '## Изменения'
) + $section + @(
    ''
    '## NuGet-пакет'
    ''
    "Библиотека хеширования паролей опубликована в GitHub Packages: ``CarService.UserRegistration $Version``."
)

$text = ($body -join "`n").Trim() + "`n"
if ($OutFile) {
    [IO.File]::WriteAllText($OutFile, $text, (New-Object Text.UTF8Encoding $false))
}
else {
    $text
}
