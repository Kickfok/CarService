/*
    CarService - тестовые данные.

    Заполняет справочники, пользователей, услуги с изображениями, клиентов и записи клиентов на услуги.
    Изображения услуг читаются из папки database/seed-images через OPENROWSET, путь к ней
    передается переменной sqlcmd SeedImagesPath:

        sqlcmd -S "(localdb)\MSSQLLocalDB" -f 65001 -b -i 02-seed.sql -v SeedImagesPath="<полный путь>\database\seed-images"

    Setup-Database.ps1 подставляет путь сам.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

USE CarService;
GO

IF EXISTS (SELECT 1 FROM dbo.Service)
BEGIN
    RAISERROR(N'Тестовые данные уже загружены. Для пересоздания запустите Setup-Database.ps1 -Recreate.', 16, 1);
    SET NOEXEC ON;
END
GO

BEGIN TRANSACTION;

/* ---------- Справочники ---------- */

INSERT INTO dbo.Role (Id, Name) VALUES
    (1, N'Администратор'),
    (2, N'Пользователь');

INSERT INTO dbo.Gender (Code, Name) VALUES
    (N'м', N'мужской'),
    (N'ж', N'женский');

/* ---------- Пользователи ---------- */

-- Пароли: admin1 / admin1 и user1 / user1. В колонке хранятся хеши PBKDF2-SHA256.
-- Новый хеш можно получить методом UserRegistration.PasswordHasher.Hash (см. database/README.md).
INSERT INTO dbo.[User] (Login, Password, RoleId) VALUES
    (N'admin1', N'PBKDF2$SHA256$100000$8NxydbfH0tAplKGDMA602A==$rdRLdfZeXndKzf0iSDdLr0OoRd/nMsRQ8NPcnwpc6DU=', 1),
    (N'user1',  N'PBKDF2$SHA256$100000$Yx8DvYl46HUV4IEIXDv9iA==$UExbmU65HC2NgmrJxhBbzMbMTCInww8U64/KphRKy4Y=', 2);

/* ---------- Услуги ---------- */

-- Скидки подобраны так, чтобы в каждом диапазоне фильтра на странице услуг была хотя бы одна услуга.
DECLARE @Services TABLE
(
    Title       NVARCHAR(100),
    Cost        MONEY,
    Minutes     INT,
    Discount    FLOAT,
    ImageFile   NVARCHAR(260),
    Description NVARCHAR(MAX)
);

INSERT INTO @Services (Title, Cost, Minutes, Discount, ImageFile, Description) VALUES
    (N'Техническое обслуживание',          4500,  120, 0.10, N'maintenance.png',            N'Плановое ТО: замена масла и фильтров, проверка жидкостей и основных узлов.'),
    (N'Диагностика двигателя',             2500,   60, 0,    N'engine.png',                 N'Компьютерная диагностика, проверка компрессии и считывание ошибок.'),
    (N'Ремонт дизельного двигателя',       15000, 240, 0.05, N'diesel-engine.jpg',          N'Ремонт и регулировка дизельных двигателей, топливной аппаратуры и турбины.'),
    (N'Ремонт АКПП',                       18000, 240, 0,    N'automatic-transmission.jpg', N'Диагностика и ремонт автоматических коробок передач, замена масла АКПП.'),
    (N'Ремонт вариатора',                  20000, 240, 0.20, N'cvt.jpg',                    N'Ремонт вариаторных коробок передач, замена ремня и конусов.'),
    (N'Ремонт механической КПП',           12000, 180, 0.15, N'manual-transmission.png',    N'Ремонт механических коробок передач, замена синхронизаторов и подшипников.'),
    (N'Замена сцепления',                  9000,  180, 0.03, N'clutch.jpg',                 N'Замена комплекта сцепления, выжимного подшипника и прокачка привода.'),
    (N'Ремонт трансмиссии',                11000, 200, 0,    N'transmission.jpg',           N'Ремонт карданных валов, раздаточных коробок и мостов.'),
    (N'Ремонт рулевого управления',        6500,  120, 0.25, N'steering.jpg',               N'Ремонт рулевых реек, тяг и наконечников.'),
    (N'Ремонт гидроусилителя руля',        5500,   90, 0,    N'power-steering.png',         N'Диагностика и ремонт ГУР, замена жидкости и насоса.'),
    (N'Ремонт подвески',                   7000,  150, 0.30, N'suspension.png',             N'Замена амортизаторов, рычагов, сайлентблоков и шаровых опор.'),
    (N'Ремонт выхлопной системы',          4000,   90, 0,    N'exhaust-system.jpg',         N'Замена глушителя, резонатора и гофры, устранение прогаров.'),
    (N'Ремонт топливной системы',          5000,  120, 0.50, N'fuel-system.png',            N'Чистка форсунок, замена топливного насоса и фильтра.'),
    (N'Обслуживание кондиционера',         3500,   60, 0.12, N'air-conditioning.jpg',       N'Заправка хладагентом, поиск утечек и антибактериальная обработка.'),
    (N'Ремонт автоэлектрики',              3000,   90, 0,    N'electrics.png',              N'Поиск и устранение неисправностей электропроводки, генератора и стартера.'),
    (N'Шиномонтаж',                        2000,   40, 0.75, N'tire-fitting.jpg',           N'Сезонная замена шин, балансировка колес и ремонт проколов.'),
    (N'Дополнительные услуги',             1500,   30, 0,    N'additional-services.png',    N'Мойка двигателя, установка дополнительного оборудования и другие работы.');

INSERT INTO dbo.Service (Title, Cost, DurationInSeconds, Description, Discount)
SELECT Title, Cost, Minutes * 60, Description, Discount
FROM @Services;

/* Изображения услуг. OPENROWSET принимает путь к файлу только литералом, поэтому запрос собирается динамически. */
DECLARE @ImagesPath NVARCHAR(400) = N'$(SeedImagesPath)';
DECLARE @Title NVARCHAR(100), @File NVARCHAR(260), @Sql NVARCHAR(MAX);

DECLARE ImageCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT Title, ImageFile FROM @Services;

OPEN ImageCursor;
FETCH NEXT FROM ImageCursor INTO @Title, @File;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'UPDATE dbo.Service
                 SET MainImage = (SELECT BulkColumn FROM OPENROWSET(BULK N'''
               + REPLACE(@ImagesPath + N'\' + @File, N'''', N'''''')
               + N''', SINGLE_BLOB) AS ImageData)
                 WHERE Title = @Title;';
    EXEC sys.sp_executesql @Sql, N'@Title NVARCHAR(100)', @Title = @Title;

    FETCH NEXT FROM ImageCursor INTO @Title, @File;
END

CLOSE ImageCursor;
DEALLOCATE ImageCursor;

COMMIT TRANSACTION;
GO

/* ---------- Клиенты и записи на услуги ---------- */

BEGIN TRANSACTION;

INSERT INTO dbo.Client (LastName, FirstName, Patronymic, GenderCode, Phone, Birthday, Email, RegistrationDate) VALUES
    (N'Иванов',    N'Сергей',    N'Петрович',     N'м', N'+7 (912) 345-67-01', '1985-03-14', N'ivanov@example.com',    '20230115'),
    (N'Смирнова',  N'Анна',      N'Владимировна', N'ж', N'+7 (912) 345-67-02', '1992-07-02', N'smirnova@example.com',  '20230220'),
    (N'Кузнецов',  N'Дмитрий',   N'Алексеевич',   N'м', N'+7 (912) 345-67-03', '1978-11-23', N'kuznetsov@example.com', '20230305'),
    (N'Попова',    N'Елена',     N'Игоревна',     N'ж', N'+7 (912) 345-67-04', '1999-05-30', N'popova@example.com',    '20230411'),
    (N'Соколов',   N'Андрей',    N'Николаевич',   N'м', N'+7 (912) 345-67-05', '1988-09-17', N'sokolov@example.com',   '20230527'),
    (N'Морозова',  N'Ольга',     NULL,            N'ж', N'+7 (912) 345-67-06', NULL,         NULL,                     '20230608');

-- Услуги с записями нельзя удалить из приложения - так проверяется защита от удаления.
INSERT INTO dbo.ClientService (ClientID, ServiceID, StartTime, Comment)
SELECT c.ID, s.ID, v.StartTime, v.Comment
FROM (VALUES
        (N'Иванов',   N'Техническое обслуживание',  CAST('2023-09-04T10:00:00' AS DATETIME), N'Плановое ТО на 60 000 км'),
        (N'Смирнова', N'Шиномонтаж',                CAST('2023-10-16T09:30:00' AS DATETIME), NULL),
        (N'Кузнецов', N'Ремонт подвески',           CAST('2023-10-18T14:00:00' AS DATETIME), N'Стук спереди справа'),
        (N'Попова',   N'Обслуживание кондиционера', CAST('2024-05-20T11:00:00' AS DATETIME), NULL),
        (N'Соколов',  N'Диагностика двигателя',     CAST('2024-06-03T16:30:00' AS DATETIME), N'Горит Check Engine'),
        (N'Иванов',   N'Шиномонтаж',                CAST('2024-04-08T12:00:00' AS DATETIME), NULL)
     ) AS v (LastName, ServiceTitle, StartTime, Comment)
JOIN dbo.Client  AS c ON c.LastName = v.LastName
JOIN dbo.Service AS s ON s.Title = v.ServiceTitle;

COMMIT TRANSACTION;
GO

DECLARE @Total INT = (SELECT COUNT(*) FROM dbo.Service);
DECLARE @WithImage INT = (SELECT COUNT(*) FROM dbo.Service WHERE MainImage IS NOT NULL);
PRINT N'Тестовые данные загружены: услуг ' + CAST(@Total AS NVARCHAR(10))
    + N', с изображением ' + CAST(@WithImage AS NVARCHAR(10)) + N'.';
GO

SET NOEXEC OFF;
GO
