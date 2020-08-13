﻿using PNPUCore.Controle;
using PNPUCore.Process;
using PNPUTools;

namespace PNPUCore
{
    class LoggerHelper
    {

        public static void Log(IProcess process, IControle controle, string statutMessage, string message)
        {
            Logger.Log(process.PROCESS_ID.ToString(), 1, process.WORKFLOW_ID, message, statutMessage, controle.GetLibControle(), "Y", "", "", process.ID_INSTANCEWF);
        }



        public static void Log(IProcess process, string statutMessage, string message)
        {
            Logger.Log(process.PROCESS_ID.ToString(), 1, process.WORKFLOW_ID, message, statutMessage, "", "", process.ID_INSTANCEWF);
        }

    }
}
    