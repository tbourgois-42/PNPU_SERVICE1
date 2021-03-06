﻿CREATE TABLE [dbo].[PNPU_MDB] (
    [ID_H_WORKFLOW] NUMERIC (18)    NOT NULL,
    [MDB]           VARBINARY (MAX) NULL,
    CONSTRAINT [PK_PNPU_MDB] PRIMARY KEY CLUSTERED ([ID_H_WORKFLOW] ASC),
    CONSTRAINT [FK_PNPU_MDB_PNPU_H_WORKFLOW] FOREIGN KEY ([ID_H_WORKFLOW]) REFERENCES [dbo].[PNPU_H_WORKFLOW] ([ID_H_WORKFLOW])
);

