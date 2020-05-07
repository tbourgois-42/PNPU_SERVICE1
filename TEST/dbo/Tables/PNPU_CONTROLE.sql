﻿CREATE TABLE [dbo].[PNPU_CONTROLE] (
    [ID_CONTROLE]    VARCHAR (50) NOT NULL,
    [CONTROLE_LABEL] VARCHAR (50) NULL,
    [TYPOLOGY]       VARCHAR (50) NULL,
    [RUN_STANDARD]   VARCHAR (50) NULL,
    [ID_PROCESS]     NUMERIC (18) NOT NULL,
    CONSTRAINT [PK_PNPU_CONTROLE] PRIMARY KEY CLUSTERED ([ID_CONTROLE] ASC),
    CONSTRAINT [FK_PNPU_CONTROLE_PNPU_PROCESS] FOREIGN KEY ([ID_PROCESS]) REFERENCES [dbo].[PNPU_PROCESS] ([ID_PROCESS]),
    CONSTRAINT [IX_PNPU_CONTROLE] UNIQUE NONCLUSTERED ([ID_CONTROLE] ASC)
);
