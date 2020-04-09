using PNPUCore.Database;
using PNPUCore.Process;
using System;
using System.Collections.Generic;


namespace PNPUCore
{
    /// <summary>  
    /// Cette classe permet de gérer le lancement des process. 
    /// </summary>  
    public class Launcher
    {
        List<InfoClient> listClient;


        void LaunchProcess(IProcess process)
        {
            process.ExecuteMainProcess();
            String json = process.FormatReport();
            Console.WriteLine(json);
        }

        /// <summary>  
        /// Lancement d'une process pour un client.
        /// </summary>  
        /// <param name="clientName">Client pour lequel on lance le preocess.</param>
        /// <param name="processName">Nom du process à lancer.</param>
        public void Launch(String clientName, int workflowId, String processName)
        {
            IProcess process = CreateProcess(clientName, workflowId, processName);

            LaunchProcess(process);
        }


        IProcess CreateProcess(String process, int workflowId, String client)
        {
            PNPUCore.Rapport.Process rapportProcess = new Rapport.Process();

            switch (process)
            {
                case "ProcessControlePacks" :
                    return ProcessControlePacks.CreateProcess(rapportProcess, workflowId);

                default:
                    return ProcessMock.CreateProcess();
            }
        }

    }
}
