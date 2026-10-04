# CarService.UserRegistration

Библиотека для .NET Framework 4.7.2+: хеширование паролей и оценка их сложности.
Используется в приложении [Car Service](https://github.com/Kickfok/CarService).

## Установка

Пакет опубликован в GitHub Packages. Добавьте источник (нужен
[personal access token](https://docs.github.com/ru/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
с правом `read:packages`):

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/Kickfok/index.json" --name github-kickfok --username <логин GitHub> --password <токен>
dotnet add package CarService.UserRegistration
```

## PasswordHasher

PBKDF2-SHA256, 100 000 итераций, случайная соль 16 байт. Результат - строка
`PBKDF2$SHA256$<итерации>$<соль Base64>$<хеш Base64>` длиной 90 символов.

```csharp
using UserRegistration;

string stored = PasswordHasher.Hash("P@ssw0rd");          // сохранить в БД
bool ok = PasswordHasher.Verify("P@ssw0rd", stored);      // true
bool bad = PasswordHasher.Verify("wrong", stored);        // false
```

- `Verify` сравнивает хеши за постоянное время.
- Для строки неизвестного формата, в том числе пароля открытым текстом, `Verify` возвращает `false`.
- Число итераций хранится в самой строке, поэтому его можно увеличить, не пересчитывая старые хеши.

## PasswordStrengthChecker

Оценка сложности от 0 до 5: по баллу за длину от 8 символов, строчную букву, заглавную букву,
цифру и специальный символ. Буквы учитываются любого алфавита, включая кириллицу.

```csharp
PasswordStrengthChecker.GetPasswordStrength("Passw0rd#");  // 5
PasswordStrengthChecker.GetPasswordStrength("Пароль12!");  // 5
PasswordStrengthChecker.GetPasswordStrength("abcdefg");    // 1
```

## Лицензия

MIT
