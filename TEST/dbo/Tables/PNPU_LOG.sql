﻿CREATE TABLE [dbo].[PNPU_LOG] (
    [ID_LOG]         NUMERIC (18) NOT NULL,
    [ID_PROCESS]     NUMERIC (18) NULL,
    [ITERATION]      NUMERIC (18) NULL,
    [WORKFLOW_ID]    NUMERIC (18) NULL,
    [MESSAGE]        TEXT         NULL,
    [STATUT_MESSAGE] VARCHAR (50) NULL,
    [ID_CONTROLE]    VARCHAR (50) NULL,
    [IS_CONTROLE]    VARCHAR (50) NULL,
    [DATE_LOG]       DATETIME     NULL,
    [SERVER]         VARCHAR (50) NULL,
    [BASE]           VARCHAR (50) NULL,
    [NIVEAU_LOG]     VARCHAR (50) NULL
);

