﻿CREATE TABLE [dbo].[PNPU_WORKFLOW] (
    [WORKFLOW_ID]    NUMERIC (18) NOT NULL,
    [WORKFLOW_LABEL] VARCHAR (50) NULL,
    CONSTRAINT [PK_PNPU_WORKFLOW] PRIMARY KEY CLUSTERED ([WORKFLOW_ID] ASC),
    CONSTRAINT [IX_PNPU_WORKFLOW] UNIQUE NONCLUSTERED ([WORKFLOW_ID] ASC)
);
