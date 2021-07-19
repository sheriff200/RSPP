CREATE TABLE [dbo].[RoleFunctionalityMapping] (
    [RoleId] VARCHAR (20) NOT NULL,
    [FuncId] VARCHAR (20) NOT NULL,
    CONSTRAINT [PK_RoleFunctionalityMapping] PRIMARY KEY CLUSTERED ([RoleId] ASC, [FuncId] ASC)
);

