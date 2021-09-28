CREATE TABLE [dbo].[LineOfBusiness]
(
	[LineOfBusinessId] INT NOT NULL PRIMARY KEY, 
    [LineOfBusinessName] VARCHAR(100) NOT NULL, 
    [Amount] DECIMAL(18, 2) NOT NULL, 
    [FormTypeId] INT NULL
)
