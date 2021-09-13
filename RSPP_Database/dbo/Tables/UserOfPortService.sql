CREATE TABLE [dbo].[UserOfPortService]
(
	[UserOfPortServiceId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Category] VARCHAR(100) NULL, 
    [AnyOtherInfo] VARCHAR(MAX) NULL, 
    [ApplicationId] VARCHAR(30) NULL
)
