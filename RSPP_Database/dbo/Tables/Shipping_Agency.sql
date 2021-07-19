CREATE TABLE [dbo].[Shipping_Agency] (
    [Shipping_Agency_Id]                  INT           IDENTITY (1, 1) NOT NULL,
    [Line_of_Business]                    VARCHAR (50)  NULL,
    [Vessel_Lines_Represented_in_Nigeria] VARCHAR (MAX) NULL,
    [Cargo_Type]                          VARCHAR (MAX) NULL,
    [Any_Other_Info]                      VARCHAR (MAX) NULL,
    [ApplicationId]                       VARCHAR (30)  NULL,
    CONSTRAINT [PK_Shipping_Agency] PRIMARY KEY CLUSTERED ([Shipping_Agency_Id] ASC),
    CONSTRAINT [FK_Shipping_Agency_ApplicationRequestForm] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequestForm] ([ApplicationId])
);

