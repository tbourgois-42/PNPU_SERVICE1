using PNPUCore.Database;
using PNPUCore.Process;
using PNPUTools;
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
            bool saved = process.SaveReportInBDD(json, process);
            Console.WriteLine(json);
        }

        /// <summary>  
        /// Lancement d'une process pour un client.
        /// </summary>  
        /// <param name="clientName">Client pour lequel on lance le preocess.</param>
        /// <param name="processName">Nom du process à lancer.</param>
        public void Launch(String clientName, int workflowId, int processId)
        {
            IProcess process = CreateProcess(processId, workflowId, clientName);

            LaunchProcess(process);
        }


        IProcess CreateProcess(int process, int workflowId, String client)
        {

            switch (process)
            {
                case ParamAppli.ProcessControlePacks:
                    return ProcessControlePacks.CreateProcess(workflowId, client);

                case ParamAppli.ProcessAnalyseImpact:
                    return ProcessAnalyseImpact.CreateProcess(workflowId, client);

                case ParamAppli.ProcessInit:
                    return ProcessInit.CreateProcess(workflowId, client);

                case ParamAppli.ProcessGestionDependance:
                    return ProcessGestionDependance.CreateProcess(workflowId, client);

                case ParamAppli.ProcessIntegration:
                    return ProcessIntegration.CreateProcess(workflowId, client);

                case ParamAppli.ProcessProcessusCritique:
                    return ProcessProcessusCritique.CreateProcess(workflowId, client);

                case ParamAppli.ProcessTNR:
                    return ProcessTNR.CreateProcess(workflowId, client);

                case ParamAppli.ProcessLivraison:
                    return ProcessLivraison.CreateProcess(workflowId, client);

                default:
                    return ProcessMock.CreateProcess(workflowId, client);

            }
             
        }
    }
}
