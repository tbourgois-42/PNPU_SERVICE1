﻿using PNPUCore.Controle;
using PNPUCore.Rapport;
using System;
using System.Collections.Generic;


namespace PNPUCore.Process
{
    class ProcessMock : ProcessCore, IProcess
    {

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessMock(decimal wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
        }

        internal static new IProcess CreateProcess(decimal WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessMock(WORKFLOW_ID, CLIENT_ID);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<PControle> listControl = ListControls.listOfMockControl;
            bool GlobalResult = false;
            sRapport = string.Empty;
            RapportProcess.Id = this.ToString();
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.Source = new List<Rapport.Source>();


            Rapport.Source RapportSource = new Rapport.Source();
            RapportSource.Id = "IdRapport - ProcessMock";
            RapportSource.Controle = new List<RControle>();
            foreach (PControle controle in listControl)
            {
                RControle RapportControle = new RControle();
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
