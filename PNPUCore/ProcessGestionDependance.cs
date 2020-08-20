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

            LoggerHelper.Log(this, ParamAppli.StatutInfo, " Debut du process " + ToString());


            // Lancer la recherche des dépendances que si le process de précontrole n'est pas dans le worklow
            PNPUTools.DataManager.DataManagerSQLServer dataManagerSQLServer = new PNPUTools.DataManager.DataManagerSQLServer();
            System.Data.DataSet dataSet = dataManagerSQLServer.GetData("select * from PNPU_STEP where WORKFLOW_ID=" + WORKFLOW_ID.ToString("########0") + " AND ID_PROCESS=1", ParamAppli.ConnectionStringBaseAppli);
            if ((dataSet != null) && (dataSet.Tables[0].Rows.Count == 0))
            {
                IControle iControle = (IControle) new ControleRechercheDependancesRef(this);
                listControl.Add(iControle);
            }

            GetListControle(ref listControl);
            

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
                Name = "IdRapport - ProcessGestionDependance",
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
                LoggerHelper.Log(this, controle, ParamAppli.StatutInfo, "Début du contrôle " + controle.ToString());
                string statutControle = controle.MakeControl();
                LoggerHelper.Log(this, controle, statutControle, "Fin du contrôle " + controle.ToString());

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

            LoggerHelper.Log(this, GlobalResult, "Fin du process " + ToString());

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult, RapportProcess.Debut);

            // Suppresion des paramètres toolbox temporaires
            //paramToolbox.DeleteParamsToolbox(WORKFLOW_ID, ID_INSTANCEWF);

            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessGestionDependance);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            }

        }

    }
}