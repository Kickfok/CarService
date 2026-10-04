/*
    CarService - схема базы данных.

    Создает базу CarService и все таблицы модели Entity Framework (CarService/Entities/CarServiceModel.edmx).
    Если таблицы уже есть, скрипт останавливается и ничего не меняет.

    Запуск: см. database/README.md (рекомендуется Setup-Database.ps1).
*/

SET NOCOUNT ON;
GO

IF DB_ID(N'CarService') IS NULL
    CREATE DATABASE CarService;
GO

USE CarService;
GO

IF OBJECT_ID(N'dbo.Service', N'U') IS NOT NULL
BEGIN
    RAISERROR(N'Таблицы базы CarService уже существуют. Для пересоздания запустите Setup-Database.ps1 -Recreate.', 16, 1);
    SET NOEXEC ON;
END
GO

/* ---------- Справочники ---------- */

CREATE TABLE dbo.Role
(
    Id   INT       NOT NULL CONSTRAINT PK_Role PRIMARY KEY,
    Name NCHAR(50) NOT NULL
);

CREATE TABLE dbo.Gender
(
    Code NCHAR(1)     NOT NULL CONSTRAINT PK_Gender PRIMARY KEY,
    Name NVARCHAR(10) NULL
);

CREATE TABLE dbo.Tag
(
    ID    INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Tag PRIMARY KEY,
    Title NVARCHAR(30)       NOT NULL,
    Color NCHAR(6)           NOT NULL
);

CREATE TABLE dbo.Manufacturer
(
    ID        INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Manufacturer PRIMARY KEY,
    Name      NVARCHAR(100)      NOT NULL,
    StartDate DATE               NULL
);

/* ---------- Пользователи ---------- */

-- Password хранит хеш PBKDF2 (см. UserRegistration/PasswordHasher.cs), а не сам пароль.
CREATE TABLE dbo.[User]
(
    Login    NVARCHAR(50)  NOT NULL CONSTRAINT PK_User PRIMARY KEY,
    Password NVARCHAR(128) NOT NULL,
    RoleId   INT           NOT NULL CONSTRAINT FK_User_Role1 REFERENCES dbo.Role (Id)
);

/* ---------- Клиенты ---------- */

CREATE TABLE dbo.Client
(
    ID               INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Client PRIMARY KEY,
    FirstName        NVARCHAR(50)       NOT NULL,
    LastName         NVARCHAR(50)       NOT NULL,
    Patronymic       NVARCHAR(50)       NULL,
    Birthday         DATE               NULL,
    RegistrationDate DATETIME           NOT NULL,
    Email            NVARCHAR(255)      NULL,
    Phone            NVARCHAR(20)       NOT NULL,
    GenderCode       NCHAR(1)           NOT NULL CONSTRAINT FK_Client_Gender REFERENCES dbo.Gender (Code),
    PhotoPath        NVARCHAR(1000)     NULL
);

CREATE TABLE dbo.TagOfClient
(
    ClientID INT NOT NULL CONSTRAINT FK_TagOfClient_Client REFERENCES dbo.Client (ID),
    TagID    INT NOT NULL CONSTRAINT FK_TagOfClient_Tag REFERENCES dbo.Tag (ID),
    CONSTRAINT PK_TagOfClient PRIMARY KEY (ClientID, TagID)
);

/* ---------- Услуги ---------- */

-- Discount - доля от 0 до 1 (0.15 = 15 %). MainImage - изображение услуги в двоичном виде.
CREATE TABLE dbo.Service
(
    ID                INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Service PRIMARY KEY,
    Title             NVARCHAR(100)      NOT NULL,
    Cost              MONEY              NOT NULL,
    DurationInSeconds INT                NOT NULL,
    Description       NVARCHAR(MAX)      NULL,
    Discount          FLOAT              NULL,
    MainImage         IMAGE              NULL
);

CREATE TABLE dbo.ServicePhoto
(
    ID        INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ServicePhoto PRIMARY KEY,
    ServiceID INT                NOT NULL CONSTRAINT FK_ServicePhoto_Service REFERENCES dbo.Service (ID),
    PhotoPath NVARCHAR(1000)     NOT NULL
);

CREATE TABLE dbo.ClientService
(
    ID        INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ClientService PRIMARY KEY,
    ClientID  INT                NOT NULL CONSTRAINT FK_ClientService_Client REFERENCES dbo.Client (ID),
    ServiceID INT                NOT NULL CONSTRAINT FK_ClientService_Service REFERENCES dbo.Service (ID),
    StartTime DATETIME           NOT NULL,
    Comment   NVARCHAR(MAX)      NULL
);

CREATE TABLE dbo.DocumentByService
(
    ID              INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_DocumentByService PRIMARY KEY,
    ClientServiceID INT                NOT NULL CONSTRAINT FK_DocumentByService_ClientService REFERENCES dbo.ClientService (ID),
    DocumentPath    NVARCHAR(1000)     NOT NULL
);

/* ---------- Товары ---------- */

CREATE TABLE dbo.Product
(
    ID             INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Product PRIMARY KEY,
    Title          NVARCHAR(100)      NOT NULL,
    Cost           MONEY              NOT NULL,
    Description    NVARCHAR(MAX)      NULL,
    MainImagePath  NVARCHAR(1000)     NULL,
    IsActive       BIT                NOT NULL,
    ManufacturerID INT                NULL CONSTRAINT FK_Product_Manufacturer REFERENCES dbo.Manufacturer (ID)
);

CREATE TABLE dbo.AttachedProduct
(
    MainProductID     INT NOT NULL CONSTRAINT FK_AttachedProduct_Product REFERENCES dbo.Product (ID),
    AttachedProductID INT NOT NULL CONSTRAINT FK_AttachedProduct_Product1 REFERENCES dbo.Product (ID),
    CONSTRAINT PK_AttachedProduct PRIMARY KEY (MainProductID, AttachedProductID)
);

CREATE TABLE dbo.ProductPhoto
(
    ID        INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ProductPhoto PRIMARY KEY,
    ProductID INT                NOT NULL CONSTRAINT FK_ProductPhoto_Product REFERENCES dbo.Product (ID),
    PhotoPath NVARCHAR(1000)     NOT NULL
);

CREATE TABLE dbo.ProductSale
(
    ID              INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ProductSale PRIMARY KEY,
    SaleDate        DATETIME           NOT NULL,
    ProductID       INT                NOT NULL CONSTRAINT FK_ProductSale_Product REFERENCES dbo.Product (ID),
    Quantity        INT                NOT NULL,
    ClientServiceID INT                NULL CONSTRAINT FK_ProductSale_ClientService REFERENCES dbo.ClientService (ID)
);
GO

/*
    Таблица sysdiagrams из модели не создается: ее создает SQL Server Management Studio
    при первом открытии раздела "Диаграммы баз данных".
*/

PRINT N'Схема базы CarService создана.';
GO

SET NOEXEC OFF;
GO
