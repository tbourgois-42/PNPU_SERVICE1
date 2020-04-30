using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;

using System;
using System.Collections.Generic;


namespace PNPUCore.Process
{
    class ProcessMock : ProcessCore, IProcess
    {

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessMock(decimal wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = -1;
        }

        internal static new IProcess CreateProcess(decimal WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessMock(WORKFLOW_ID, CLIENT_ID);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = ListControls.listOfMockControl;
            bool GlobalResult = true;
            sRapport = string.Empty;
            RapportProcess.Name = this.ToString();
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;

            RapportProcess.Source = new List<Rapport.Source>();


            Rapport.Source RapportSource = new Rapport.Source();
            RapportSource.Name = "IdRapport - ProcessMock";
            RapportSource.Controle = new List<RControle>();
            foreach (IControle controle in listControl)
            {
                controle.SetProcessControle(this);
                RControle RapportControle = new RControle();
                RapportControle.Name = controle.ToString();
                RapportControle.Message = new List<string>();
                RapportControleCourant = RapportControle;

                if (controle.MakeControl() == false)
                {
                    GlobalResult = false;
                    RapportControle.Result = false;
                }
                else
                {
                    RapportControle.Result = true;
                }

                RapportSource.Controle.Add(RapportControle);
            }
            RapportProcess.Source.Add(RapportSource);

            //Si le contrôle est ok on génère les lignes d'historique pour signifier que le workflow est lancé
            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW();
            PNPU_H_STEP historicStep = new PNPU_H_STEP();

            historicWorkflow.CLIENT_ID = this.CLIENT_ID;
            historicWorkflow.LAUNCHING_DATE = RapportProcess.Debut;
            historicWorkflow.WORKFLOW_ID = this.WORKFLOW_ID;

            historicStep.ID_PROCESS = this.PROCESS_ID;
            historicStep.ITERATION = 1;
            historicStep.WORKFLOW_ID = this.WORKFLOW_ID;
            historicStep.CLIENT_ID = this.CLIENT_ID;
            historicStep.USER_ID = "PNPUADM";
            historicStep.LAUNCHING_DATE = RapportProcess.Debut;
            historicStep.ENDING_DATE = RapportProcess.Fin;
            if (GlobalResult)
            {
                historicStep.ID_STATUT = ParamAppli.StatutCompleted;
            }
            else
            {
                historicStep.ID_STATUT = ParamAppli.StatutError;
            }

            GenerateHistoric(historicWorkflow, historicStep);

            if (GlobalResult)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, -1);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID);
            }

        }

    }
}
