﻿CREATE TABLE [dbo].[ApplicationRequestForm] (
    [ApplicationId]       VARCHAR (30)  NOT NULL,
    [AgencyName]          VARCHAR (500) NULL,
    [DateofEstablishment] DATETIME      NULL,
    [CompanyAddress]      VARCHAR (MAX) NULL,
    [PostalAddress]       VARCHAR (MAX) NULL,
    [PhoneNum]            VARCHAR (30)  NULL,
    [CompanyEmail]        VARCHAR (100) NULL,
    [CompanyWebsite]      VARCHAR (500) NULL,
    [AgencyId]            INT           NULL,
    [LicenseReference]    VARCHAR (50)  NULL,
    [LicenseIssuedDate]   DATETIME      NULL,
    [LicenseExpiryDate]   DATETIME      NULL,
    [CurrentStageID]      INT           NULL,
    [LastAssignedUser]    VARCHAR (50)  NULL,
    [AddedDate]           DATETIME      NULL,
    [Status]              VARCHAR (40)  NULL,
    [ModifiedDate] DATETIME NULL, 
    [ApplicationTypeId] VARCHAR(10) NULL, 
    [PrintedStatus] VARCHAR(20) NULL, 
    CONSTRAINT [PK_ApplicationRequestForm_1] PRIMARY KEY CLUSTERED ([ApplicationId] ASC),
    CONSTRAINT [FK_ApplicationRequestForm_Agency] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agency] ([AgencyId])
);

