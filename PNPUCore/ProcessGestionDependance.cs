﻿using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;
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

        public ProcessGestionDependance(int wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = ParamAppli.ProcessGestionDependance;
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessGestionDependance(WORKFLOW_ID, CLIENT_ID);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = new List<IControle>();//ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;

            Logger.Log(this, ParamAppli.StatutInfo, " Debut du process " + this.ToString());


            GetListControle(ref listControl);
            //!!!!!!!!!!!!!!!!!!!! Pour test !!!!!!!!!!!!!!!!!!!!!!!
            ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1 = ParamAppli.ConnectionStringBaseRefPlateforme;

            sRapport = string.Empty;
            RapportProcess.Name = this.LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();


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

            Logger.Log(this, GlobalResult, "Fin du process " + this.ToString());

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
            historicStep.CLIENT_NAME = ParamAppli.ListeInfoClient[this.CLIENT_ID].CLIENT_NAME;
            historicStep.USER_ID = "PNPUADM";
            historicStep.TYPOLOGY = "SAAS DEDIE";
            historicStep.LAUNCHING_DATE = RapportProcess.Debut;
            historicStep.ENDING_DATE = RapportProcess.Fin;
            historicStep.ID_STATUT = GlobalResult;
            

            GenerateHistoric(historicWorkflow, historicStep);

            // MHUM je bloque en attendant que l'analyse d'impact soit ok
            /*if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessGestionDependance);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID);
            }*/

        }

    }
}