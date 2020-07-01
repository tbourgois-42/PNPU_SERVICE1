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

        void LaunchProcess(IProcess process)
        {
            process.ExecuteMainProcess();
            String json = process.FormatReport(process);
            process.SaveReportInBDD(json, process);
            Console.WriteLine(json);
        }

        /// <summary>  
        /// Lancement d'une process pour un client.
        /// </summary>  
        /// <param name="clientName">Client pour lequel on lance le preocess.</param>
        /// <param name="processName">Nom du process à lancer.</param>
        public void Launch(String listclientId, int workflowId, int processId, int idInstanceWF)
        {
            IProcess process = CreateProcess(processId, workflowId, listclientId, idInstanceWF);

            LaunchProcess(process);
        }


        IProcess CreateProcess(int process, int workflowId, String client, int idInstanceWF)
        {

            switch (process)
            {
                case ParamAppli.ProcessControlePacks:
                    return ProcessControlePacks.CreateProcess(workflowId, client, idInstanceWF);

                /*case ParamAppli.ProcessAnalyseImpact:
                    return ProcessAnalyseImpact.CreateProcess(workflowId, client);*/
                    
                case ParamAppli.ProcessInit:
                    return ProcessInit.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessGestionDependance:
                    return ProcessGestionDependance.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessIntegration:
                    return ProcessIntegration.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessProcessusCritique:
                    return ProcessProcessusCritique.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessTNR:
                    return ProcessTNR.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessLivraison:
                    return ProcessLivraison.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessAnalyseImpactLogique:
                    return ProcessAnalyseImpactLogique.CreateProcess(workflowId, client, idInstanceWF);

                case ParamAppli.ProcessAnalyseImpactData:
                    return ProcessAnalyseImpactData.CreateProcess(workflowId, client, idInstanceWF);

                default:
                    return ProcessMock.CreateProcess(workflowId, client, idInstanceWF);

            }
             
        }
    }
}
