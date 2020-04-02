using PNPUCore.Control;
using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;



namespace PNPUCore.Process
{
    /// <summary>  
    /// Cette classe correspond au process de contrôle des mdb. 
    /// </summary>  
    class ProcessControlePacks : IProcess
    {
        private readonly object listOfMockControl;
        private List<string> listMDB;
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
            listMDB.Add( "D:\\PNPU\\02_8.1_HF2003_PLFR_HP.mdb");
            listMDB.Add("D:\\PNPU\\8.1_HF2003_PLFR_PAY.mdb");
            listMDB.Add("D:\\PNPU\\TEST.mdb");
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
