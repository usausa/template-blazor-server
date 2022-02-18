-- Account
CREATE TABLE Account (
    Id           nvarchar(8)  NOT NULL,
    Name         nvarchar(10) NOT NULL,
    PasswordHash varchar(256) NOT NULL,
    IsAdmin      bit          NOT NULL,
    CONSTRAINT PK_Account PRIMARY KEY CLUSTERED (Id ASC)
)
GO

-- Data
CREATE TABLE Data (
    Id       int          NOT NULL,
    Name     nvarchar(50) NOT NULL,
    Flag     bit          NOT NULL,
    DateTime datetime2(3) NOT NULL
    CONSTRAINT PK_Data PRIMARY KEY CLUSTERED (Id ASC)
)
GO

-- Item
CREATE TABLE Item (
    Code     char(13)     NOT NULL,
    Category char(3)      NOT NULL,
    Name     nvarchar(50) NOT NULL,
    Value    int          NOT NULL,
    CONSTRAINT PK_Item PRIMARY KEY CLUSTERED (Code ASC)
)
GO

CREATE NONCLUSTERED INDEX IX_Item_Category ON Item
    (Category ASC)
GO
