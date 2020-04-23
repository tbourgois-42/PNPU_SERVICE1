﻿using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using PNPUTools;

namespace PNPUCore.Process
{
    /// <summary>  
    /// Cette classe correspond au process de contrôle des mdb. 
    /// </summary>  
    class ProcessControlePacks : Process, IProcess
    {
        public List<string> listMDB { get; set; }
        public string MDBCourant { get; set; }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessControlePacks(decimal wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = new List<IControle>();
            bool GlobalResult = false;
            listMDB = new List<string>();
            sRapport = string.Empty;
            string[] tMDB = null;
            RapportProcess.Id = this.ToString();
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.Source = new List<Rapport.Source>();

            //Pour test MHUM
            //listControl.Clear();
            /*foreach (string sfichier in Directory.GetFiles("D:\\PNPU","*.mdb"))
                listMDB.Add(sfichier);*/
            PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();
            gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tMDB, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli);
            foreach(String sFichier in tMDB)
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
                RapportSource.Id = System.IO.Path.GetFileName(sMDB);
                RapportSource.Controle = new List<Rapport.Controle>();
                foreach (IControle controle in listControl)
                {
                    Rapport.Controle RapportControle = new Rapport.Controle();
                    RapportControle.Id = controle.ToString();
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
            }

            // Le controle des dépendance est à part puisqu'il traite tous les mdb en une fois
            ControleDependancesMDB cdmControleDependancesMDB = new ControleDependancesMDB(this);
            Rapport.Source RapportSource2 = new Rapport.Source();
            RapportSource2.Id = string.Empty;
            foreach (string sMdb in listMDB)
            {
                if (RapportSource2.Id != string.Empty)
                    RapportSource2.Id += " - ";
                RapportSource2.Id += System.IO.Path.GetFileName(sMdb);
            }
            RapportSource2.Controle = new List<Rapport.Controle>();
            Rapport.Controle RapportControle2 = new Rapport.Controle();
            RapportControle2.Id = cdmControleDependancesMDB.ToString();
            RapportControle2.Message = new List<string>();
            RapportControleCourant = RapportControle2;
            RapportControle2.Result= cdmControleDependancesMDB.MakeControl();
            RapportSource2.Controle.Add(RapportControle2);
            RapportProcess.Source.Add(RapportSource2);

            //Si le contrôle est ok on génère les lignes d'historique pour signifier que le workflow est lancé
            if (GlobalResult)
                GenerateHistoric();

        }

        private void GenerateHistoric()
        {
            //récupérer la liste des clients concernés

            //Boucler sur la liste des clients
                //Générer le PNPU_H_WORKFLOW
                //Générer la ligne PNPU_H_STEP
        }
    }
}
