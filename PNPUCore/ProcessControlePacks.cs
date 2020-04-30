using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using PNPUTools;
using PNPUTools.DataManager;

using PNPUCore.Rapport;

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

        public ProcessControlePacks(decimal wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = ParamAppli.ProcessControlePacks;
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = new List<IControle>();
            string GlobalResult = ParamAppli.StatutOk;
            listMDB = new List<string>();
            sRapport = string.Empty;
            string[] tMDB = null;
            RapportProcess.Name = this.ToString();
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();

            //Pour test MHUM
            //listControl.Clear();
            /*foreach (string sfichier in Directory.GetFiles("D:\\PNPU","*.mdb"))
                listMDB.Add(sfichier);*/
            PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();
            gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tMDB, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli);
            foreach (String sFichier in tMDB)
            {
                listMDB.Add(sFichier);
            }

            listControl.Add(new ControleCatalogueTable(this));
            listControl.Add(new ControleCmdInterdites(this));
            listControl.Add(new ControleDonneesReplace(this));
            listControl.Add(new ControleIDSynonym(this));
            listControl.Add(new ControleItemsTotaux(this));
            listControl.Add(new ControleNiveauHeritage(this));
            listControl.Add(new ControleNiveauSaisie(this));
            listControl.Add(new ControleObjetTechno(this));
            listControl.Add(new ControleParamAppli(this));
            listControl.Add(new ControleTacheSecu(this));
            listControl.Add(new ControleTableSecu(this));
            listControl.Add(new ControleTypePack(this));

            foreach (string sMDB in listMDB)
            {
                MDBCourant = sMDB;
                Rapport.Source RapportSource = new Rapport.Source();
                RapportSource.Name = System.IO.Path.GetFileName(sMDB);
                RapportSource.Controle = new List<RControle>();
                foreach (IControle controle in listControl)
                {
                    RControle RapportControle = new RControle();
                    RapportControle.Name = controle.ToString();
                    RapportControle.Message = new List<string>();
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

                    RapportControle.Result = statutControle;
                    

                    RapportSource.Controle.Add(RapportControle);
                }
                RapportProcess.Source.Add(RapportSource);
            }

            // Le controle des dépendance est à part puisqu'il traite tous les mdb en une fois
            ControleDependancesMDB cdmControleDependancesMDB = new ControleDependancesMDB(this);
            Rapport.Source RapportSource2 = new Rapport.Source();
            RapportSource2.Name = string.Empty;
            foreach (string sMdb in listMDB)
            {
                if (RapportSource2.Name != string.Empty)
                    RapportSource2.Name += " - ";
                RapportSource2.Name += System.IO.Path.GetFileName(sMdb);
            }
            RapportSource2.Controle = new List<RControle>();
            RControle RapportControle2 = new RControle();
            RapportControle2.Name = cdmControleDependancesMDB.ToString();
            RapportControle2.Message = new List<string>();
            RapportControleCourant = RapportControle2;
            RapportControle2.Result = cdmControleDependancesMDB.MakeControl();
            RapportSource2.Controle.Add(RapportControle2);
            RapportProcess.Source.Add(RapportSource2);
            RapportProcess.Fin = DateTime.Now;
            //Si le contrôle est ok on génère les lignes d'historique pour signifier que le workflow est lancé
            string[] listClientId = new string[] { "DASSAULT SYSTEME", "SANEF", "DRT", "GALILEO", "IQERA", "ICL", "CAMAIEU", "DANONE", "HOLDER", "OCP", "UNICANCER", "VEOLIA" };

            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW();
            historicWorkflow.CLIENT_ID = this.CLIENT_ID;
            historicWorkflow.LAUNCHING_DATE = RapportProcess.Debut;
            historicWorkflow.ENDING_DATE = new DateTime(1800, 1, 1);
            historicWorkflow.STATUT_GLOBAL = "IN PROGRESS";
            historicWorkflow.WORKFLOW_ID = this.WORKFLOW_ID;

            RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow);

            foreach (string clienId in listClientId) { 
                PNPU_H_STEP historicStep = new PNPU_H_STEP();
                historicStep.ID_PROCESS = this.PROCESS_ID;
                historicStep.ITERATION = 1;
                historicStep.WORKFLOW_ID = this.WORKFLOW_ID;
                historicStep.CLIENT_ID = clienId;
                historicStep.USER_ID = "PNPUADM";
                historicStep.LAUNCHING_DATE = RapportProcess.Debut;
                historicStep.ENDING_DATE = RapportProcess.Fin;
                historicStep.TYPOLOGY = "SAAS DEDIE";
                historicStep.ID_STATUT = GlobalResult;
                
                RequestTool.CreateUpdateStepHistoric(historicStep);
            }

            int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessControlePacks);
            foreach(string clienId in listClientId)
            {
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), clienId);

            }
        }

        internal static new IProcess CreateProcess(decimal WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessControlePacks(WORKFLOW_ID, CLIENT_ID);
        }
    }
}
