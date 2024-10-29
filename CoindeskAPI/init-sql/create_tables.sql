USE CoindeskDB;

IF OBJECT_ID('Currencies', 'U') IS NULL
BEGIN
    CREATE TABLE Currencies (
        Code     VARCHAR (10)  NOT NULL,
        Name     NVARCHAR (50) NOT NULL,
        Language VARCHAR (10)  NOT NULL,
        PRIMARY KEY CLUSTERED (Code ASC, Language ASC)
    );
END;