CREATE TABLE [dbo].[Documents] (
    [DocId]        INT           IDENTITY (1, 1) NOT NULL,
    [DocumentName] VARCHAR (500) NULL,
    [AgencyId]     INT           NULL,
    [IsMandatory]  VARCHAR (2)   NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED ([DocId] ASC),
    CONSTRAINT [FK_Documents_Agency] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agency] ([AgencyId])
);



