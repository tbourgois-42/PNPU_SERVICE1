﻿CREATE TABLE [dbo].[PNPU_H_WORKFLOW] (
    [ID_H_WORKFLOW]  NUMERIC (18) NOT NULL,
    [CLIENT_ID]      VARCHAR (50) NULL,
    [WORKFLOW_ID]    NUMERIC (18) NULL,
    [LAUNCHING_DATE] DATETIME     NULL,
    [ENDING_DATE]    DATETIME     NULL,
    [STATUT_GLOBAL]  VARCHAR (50) NULL,
    CONSTRAINT [PK_PNPU_H_WORKFLOW] PRIMARY KEY CLUSTERED ([ID_H_WORKFLOW] ASC)
);
