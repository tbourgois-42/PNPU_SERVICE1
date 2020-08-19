using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;

using System;
using System.Collections.Generic;


namespace PNPUCore.Process
{
    internal class ProcessMock : ProcessCore, IProcess
    {

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessMock(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = -1;
            LibProcess = "Contrôle mock";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessMock(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportProcess.Name = LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();
            int idInstanceWF = ID_INSTANCEWF;

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress, RapportProcess.Debut);

            Rapport.Source RapportSource = new Rapport.Source
            {
                Name = "IdRapport - ProcessMock",
                Controle = new List<RControle>()
            };
            foreach (IControle controle in listControl)
            {
                controle.SetProcessControle(this);
                RControle RapportControle = new RControle
                {
                    Name = controle.GetLibControle(),
                    Tooltip = controle.GetTooltipControle(),
                    Message = new List<string>()
                };
                RapportControleCourant = RapportControle;
                string statutControle = controle.MakeControl();
                //ERROR > WARNING > OK
                if (GlobalResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutError)
                {
                    GlobalResult = statutControle;

                }
                else if (GlobalResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutWarning)
                {
                    GlobalResult = statutControle;
                }
                RapportControle.Result = ParamAppli.TranscoSatut[statutControle];

                RapportSource.Controle.Add(RapportControle);
            }
            RapportProcess.Source.Add(RapportSource);
            RapportProcess.Fin = DateTime.Now;

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult, RapportProcess.Debut);

            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, -1);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            }

        }

    }
}
