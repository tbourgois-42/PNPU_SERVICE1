﻿CREATE TABLE [dbo].[PNPU_WORKFLOW_HABILITATION] (
    [WORKFLOW_ID]  NUMERIC (18) NOT NULL,
    [USER_PROFILE] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_PNPU_WORKFLOW_HABILITATION] PRIMARY KEY CLUSTERED ([WORKFLOW_ID] ASC, [USER_PROFILE] ASC)
);

