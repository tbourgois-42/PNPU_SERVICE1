﻿CREATE TABLE [dbo].[PNPU_PROCESS] (
    [ID_PROCESS]    NUMERIC (18) NOT NULL,
    [PROCESS_LABEL] VARCHAR (50) NULL,
    [IS_LOOPABLE]   VARCHAR (1)  NULL,
    CONSTRAINT [PK_PNPU_PROCESS] PRIMARY KEY CLUSTERED ([ID_PROCESS] ASC),
    CONSTRAINT [IX_PNPU_PROCESS] UNIQUE NONCLUSTERED ([ID_PROCESS] ASC)
);
