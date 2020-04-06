using PNPUCore.Control;
using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace PNPUCore.Process
{
    /// <summary>  
    /// Cette classe correspond au process de contrôle des mdb. 
    /// </summary>  
    class ProcessControlePacks : IProcess
    {
        private readonly object listOfMockControl;
        public List<string> listMDB { get; set; }
        public string MDBCourant { get; set; }
        private string sRapport;
        private PNPUCore.Rapport.Process RapportProcess;
        private PNPUCore.Rapport.Controle RapportControleCourant;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>
        public ProcessControlePacks(PNPUCore.Rapport.Process rapportProcess)
        {
            RapportProcess = rapportProcess;
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
         /// </summary>  
        public void ExecuteMainProcess()
        {
            List<IControle> listControl =  ListControls.listOfMockControl;
            listMDB = new List<string>();
            sRapport = string.Empty;

            RapportProcess.Id = this.ToString();
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.Source = new List<Rapport.Source>();

            //Pour test MHUM
            listControl.Clear();
            foreach (string sfichier in Directory.GetFiles("D:\\PNPU","*.mdb"))
                listMDB.Add(sfichier);


            listControl.Add(new ControleCatalogueTable(this));
            listControl.Add(new ControleCmdInterdites(this));
            listControl.Add(new ControleDonneesReplace(this));
            listControl.Add(new ControleIDSynonym(this));
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
                        RapportControle.Result = false;
                    else
                        RapportControle.Result = true;

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
                    RapportSource2.Id += " \\ ";
                RapportSource2.Id += System.IO.Path.GetFileName(sMdb);
            }
            RapportSource2.Controle = new List<Rapport.Controle>();
            Rapport.Controle RapportControle2 = new Rapport.Controle();
            RapportControle2.Id = cdmControleDependancesMDB.ToString();
            RapportControle2.Message = new List<string>();
            RapportControleCourant = RapportControle2;
            cdmControleDependancesMDB.MakeControl();
            RapportSource2.Controle.Add(RapportControle2);
            RapportProcess.Source.Add(RapportSource2);

            //MHUM pour vérifier les dépendances j'écris le résultat dans un fichier
            StreamWriter swFichierResultat = new StreamWriter("d:\\PNPU\\dependances.csv", false);
            foreach (string sLigne in RapportControle2.Message)
            {
                swFichierResultat.WriteLine(sLigne);
            }
            swFichierResultat.Close();
        }

        /// <summary>  
        /// Méthode générant le rapport du déroulement du process au format JSON.
        /// <returns>Retourne le rapport au format JSON dans une chaine de caractères.</returns>
        /// </summary>  
        public string FormatReport()
        {
            RapportProcess.Fin = DateTime.Now;
            return (RapportProcess.ToJSONRepresentation());
            
        }

        /// <summary>  
        /// Méthode appelée par les contrôle pour ajouter un message dans le rapport d'exécution du process.
        /// <param name="sMessage">Message à ajouter dans le rapport d'exécution du process.</param>
        /// </summary>  
        public void AjouteRapport(string sMessage)
        {
            RapportControleCourant.Message.Add(sMessage);
        }

        /// <summary>  
        /// Méthode appelée par le launcher pour créé le process. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>
        /// <returns>Retourne l'instance du process créé.</returns>
        internal static IProcess CreateProcess(PNPUCore.Rapport.Process rapportProcess)
        {
            return new ProcessControlePacks(rapportProcess);
        }
    }
}
