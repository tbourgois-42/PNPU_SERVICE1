using System;

namespace PNPUCore.Process
{
    public interface IProcess
    {

        void ExecuteMainProcess();

        String FormatReport();
        void AjouteRapport(string v);
    }

    public class Process : IProcess
    {
        public decimal WORKFLOW_ID { get; set; }
        public string CLIENT_ID { get; set; }

        internal string sRapport;
        internal PNPUCore.Rapport.Process RapportProcess;
        internal PNPUCore.Rapport.Controle RapportControleCourant;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public Process(decimal wORKFLOW_ID, string cLIENT_ID)
        {
            PNPUCore.Rapport.Process rapportProcess = new Rapport.Process();
            WORKFLOW_ID = wORKFLOW_ID;
            CLIENT_ID = cLIENT_ID;
        }

        private void GenerateHistoric()
        {
            //récupérer la liste des clients concernés

            //Boucler sur la liste des clients
            //Générer le PNPU_H_WORKFLOW
            //Générer la ligne PNPU_H_STEP
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
        internal static IProcess CreateProcess(decimal WORKFLOW_ID, string CLIENT_ID)
        {
            return new Process(WORKFLOW_ID, CLIENT_ID);
        }

        public void ExecuteMainProcess()
        {
            throw new NotImplementedException();
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
    }
}