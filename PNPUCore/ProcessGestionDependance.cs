using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;
using System;
using System.Collections.Generic;

namespace PNPUCore.Process
{
    internal class ProcessGestionDependance : ProcessCore, IProcess
    {

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessGestionDependance(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = ParamAppli.ProcessGestionDependance;
            LibProcess = "Gestion des dépendances";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessGestionDependance(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = new List<IControle>();//ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;

            Logger.Log(this, ParamAppli.StatutInfo, " Debut du process " + ToString());


            GetListControle(ref listControl);
            //!!!!!!!!!!!!!!!!!!!! Pour test !!!!!!!!!!!!!!!!!!!!!!!
            //ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1 = ParamAppli.ConnectionStringBaseRefPlateforme;

            sRapport = string.Empty;
            RapportProcess.Name = LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();
            int idInstanceWF = ID_INSTANCEWF;

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress, RapportProcess.Debut);

            Rapport.Source RapportSource = new Rapport.Source();
            RapportSource.Name = "IdRapport - ProcessGestionDependance";
            RapportSource.Controle = new List<RControle>();
            foreach (IControle controle in listControl)
            {
                controle.SetProcessControle(this);
                RControle RapportControle = new RControle();
                RapportControle.Name = controle.GetLibControle();
                RapportControle.Tooltip = controle.GetTooltipControle();
                RapportControle.Message = new List<string>();
                RapportControleCourant = RapportControle;
                Logger.Log(this, controle, ParamAppli.StatutInfo, "Début du contrôle " + controle.ToString());
                string statutControle = controle.MakeControl();
                Logger.Log(this, controle, statutControle, "Fin du contrôle " + controle.ToString());

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
                RapportSource.Result = RapportControle.Result;
                RapportSource.Controle.Add(RapportControle);
            }
            RapportProcess.Source.Add(RapportSource);
            RapportProcess.Fin = DateTime.Now;
            RapportProcess.Result = ParamAppli.TranscoSatut[GlobalResult];

            Logger.Log(this, GlobalResult, "Fin du process " + ToString());

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult, RapportProcess.Debut);

            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessGestionDependance);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            }

        }

    }
}