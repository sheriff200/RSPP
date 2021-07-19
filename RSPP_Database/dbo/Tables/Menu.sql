CREATE TABLE [dbo].[Menu] (
    [MenuId]      VARCHAR (20)  NOT NULL,
    [Description] VARCHAR (100) NULL,
    [IconName]    VARCHAR (50)  NULL,
    [SeqNo]       TINYINT       NULL,
    [Status]      VARCHAR (10)  NULL,
    CONSTRAINT [PK_Menu] PRIMARY KEY CLUSTERED ([MenuId] ASC)
);

