﻿using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using PNPUTools;
using PNPUTools.DataManager;
using System.Xml;

using PNPUCore.Rapport;
using System.Linq;

namespace PNPUCore.Process
{
    /// <summary>  
    /// Cette classe correspond au process de contrôle des mdb. 
    /// </summary>  
    class ProcessControlePacks : ProcessCore, IProcess
    {
        public List<string> listMDB { get; set; }
        public string MDBCourant { get; set; }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessControlePacks(int wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = ParamAppli.ProcessControlePacks;
            this.LibProcess = "Pré contrôle des .mdb";
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = new List<IControle>();
            string GlobalResult = ParamAppli.StatutOk;
            string SourceResult = ParamAppli.StatutOk;
            string statutControle;
            listMDB = new List<string>();
            sRapport = string.Empty;
            string[] tMDB = null;
            RControle RapportControle;
            Rapport.Source RapportSource;
            string[] listClientId = CLIENT_ID.Split(',');

            //POUR TEST 
            /*this.CLIENT_ID = "101";*/
            this.STANDARD = true;
            //this.TYPOLOGY = ParamAppli.ListeInfoClient[listClientId.First()].TYPOLOGY;
            Logger.Log(this, ParamAppli.StatutInfo, " Debut du process " + this.ToString());

            RapportProcess.Name = this.LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();

            PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();
            gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tMDB, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli);
            foreach (String sFichier in tMDB)
            {
                listMDB.Add(sFichier);
            }

            GetListControle(ref listControl);

            foreach (string sMDB in listMDB)
            {
                MDBCourant = sMDB;
                RapportSource = new Rapport.Source();
                RapportSource.Name = System.IO.Path.GetFileName(sMDB);
                RapportSource.Controle = new List<RControle>();
                SourceResult = ParamAppli.StatutOk;
                foreach (IControle controle in listControl)
                {
                    RapportControle = new RControle();
                    RapportControle.Name = controle.GetLibControle();
                    RapportControle.Tooltip = controle.GetTooltipControle();
                    RapportControle.Message = new List<string>();
                    RapportControleCourant = RapportControle;
                    Logger.Log(this, controle, ParamAppli.StatutInfo, "Début du contrôle " + controle.ToString());
                    statutControle = controle.MakeControl();
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

                    if (SourceResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutError)
                    {
                        SourceResult = statutControle;

                    }
                    else if (SourceResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutWarning)
                    {
                        SourceResult = statutControle;
                    }
                    RapportControle.Result = ParamAppli.TranscoSatut[statutControle];


                    RapportSource.Controle.Add(RapportControle);
                }
                RapportSource.Result = ParamAppli.TranscoSatut[SourceResult];
                RapportProcess.Source.Add(RapportSource);
            }

            // Le controle des dépendance est à part puisqu'il traite tous les mdb en une fois
            ControleDependancesMDB cdmControleDependancesMDB = new ControleDependancesMDB(this);
            RapportSource = new Rapport.Source();
            RapportSource.Name = "Contrôle des dépendances inter packages";
            RapportSource.Controle = new List<RControle>();
            RapportControle = new RControle();
            RapportControle.Name = cdmControleDependancesMDB.ToString();
            RapportControle.Message = new List<string>();
            RapportControleCourant = RapportControle;
            Logger.Log(this, cdmControleDependancesMDB, ParamAppli.StatutInfo, "Début du contrôle " + cdmControleDependancesMDB.ToString());
            statutControle = cdmControleDependancesMDB.MakeControl();
            Logger.Log(this, cdmControleDependancesMDB, statutControle, "Fin du contrôle " + cdmControleDependancesMDB.ToString());
            RapportControle.Result = ParamAppli.TranscoSatut[statutControle];
            //RapportSource2.Controle.Add(RapportControle2);
            RapportProcess.Source.Add(RapportSource);

            // Génération du fichier CSV des dépendances
            StreamWriter swFichierDep = new StreamWriter(Path.Combine(ParamAppli.DossierTemporaire, this.WORKFLOW_ID.ToString("000000") + "_DEPENDANCES.csv"));
            foreach (string sLig in RapportControle.Message)
                swFichierDep.WriteLine(sLig);
            swFichierDep.Close();
            // Je supprime les messages pour qu'ils ne sortent pas dans le report JSON
            RapportControle.Message.Clear();
            RapportSource.Result = RapportControle.Result;

            // Recherche des dépendances avec les tâches CCT sur la base de référence
            ControleRechercheDependancesRef crdrControleRechercheDependancesRef = new ControleRechercheDependancesRef(this);
            RapportSource = new Rapport.Source();
            RapportSource.Name = "Recherche des dépendances avec les tâches CCT sur la base de référence";
            RapportSource.Controle = new List<RControle>();
            RapportControle = new RControle();
            RapportControle.Name = cdmControleDependancesMDB.ToString();
            RapportControle.Message = new List<string>();
            RapportControleCourant = RapportControle;
            Logger.Log(this, crdrControleRechercheDependancesRef, ParamAppli.StatutInfo, "Début du contrôle " + crdrControleRechercheDependancesRef.ToString());
            statutControle = crdrControleRechercheDependancesRef.MakeControl();
            Logger.Log(this, crdrControleRechercheDependancesRef, statutControle, "Fin du contrôle " + crdrControleRechercheDependancesRef.ToString());
            RapportControle.Result = ParamAppli.TranscoSatut[statutControle];
            //RapportSource2.Controle.Add(RapportControle2);
            RapportProcess.Source.Add(RapportSource);

            // Je supprime les messages pour qu'ils ne sortent pas dans le report JSON
            RapportControle.Message.Clear();
            RapportSource.Result = RapportControle.Result;


            RapportProcess.Fin = DateTime.Now;
            RapportProcess.Result = ParamAppli.TranscoSatut[GlobalResult];
            Logger.Log(this, GlobalResult, "Fin du process " + this.ToString());


            //Si le contrôle est ok on génère les lignes d'historique pour signifier que le workflow est lancé
            //string[] listClientId = new string[] { "DASSAULT SYSTEME", "SANEF", "DRT", "GALILEO", "IQERA", "ICL", "CAMAIEU", "DANONE", "HOLDER", "OCP", "UNICANCER", "VEOLIA" };
            //string[] listClientId = new string[] { "111" };//{ "DASSAULT SYSTEME", "SANEF", "DRT", "GALILEO", "IQERA", "ICL", "CAMAIEU", "DANONE", "HOLDER", "OCP", "UNICANCER", "VEOLIA" };
 

            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW();
            historicWorkflow.CLIENT_ID = this.CLIENT_ID;
            historicWorkflow.LAUNCHING_DATE = RapportProcess.Debut;
            historicWorkflow.ENDING_DATE = new DateTime(1800, 1, 1);
            historicWorkflow.STATUT_GLOBAL = "IN PROGRESS";
            historicWorkflow.WORKFLOW_ID = this.WORKFLOW_ID;

            RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow);

            foreach (string clientId in listClientId) {
                InfoClient client = RequestTool.getClientsById(clientId);
                PNPU_H_STEP historicStep = new PNPU_H_STEP();
                historicStep.ID_PROCESS = this.PROCESS_ID;
                historicStep.ITERATION = 1;
                historicStep.WORKFLOW_ID = this.WORKFLOW_ID;
                historicStep.CLIENT_ID = clientId;
                historicStep.CLIENT_NAME = client.CLIENT_NAME;
                historicStep.USER_ID = "PNPUADM";
                historicStep.LAUNCHING_DATE = RapportProcess.Debut;
                historicStep.ENDING_DATE = RapportProcess.Fin;
                historicStep.TYPOLOGY = "SAAS DEDIE";
                historicStep.ID_STATUT = GlobalResult;
                RequestTool.CreateUpdateStepHistoric(historicStep);
            }

            // TEST ajout MDB
            //gereMDBDansBDD.AjouteFichiersMDBBDD(new string[] { "D:\\PNPU\\Tests_RAMDL\\test.mdb" }, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli, CLIENT_ID,1);

            int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessControlePacks);
            foreach(string clienId in listClientId)
            {
               LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), clienId);

            }
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessControlePacks(WORKFLOW_ID, CLIENT_ID);
        }
    }
}
