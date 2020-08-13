using PNPUCore.Controle;
using PNPUCore.Process;
using PNPUTools;

namespace PNPUCore
{
    class LoggerHelper
    {

        public static void Log(IProcess process, IControle controle, string statutMessage, string message)
        {
            Logger.Log(process.PROCESS_ID.ToString(), 1, process.WORKFLOW_ID, message, controle.GetLibControle(), "Y", statutMessage, "", process.BASE, process.ID_INSTANCEWF);
        }



        public static void Log(IProcess process, string statutMessage, string message)
        {
            Logger.Log(process.PROCESS_ID.ToString(), 1, process.WORKFLOW_ID, message, statutMessage, "", process.BASE, process.ID_INSTANCEWF);
        }

    }
}
    