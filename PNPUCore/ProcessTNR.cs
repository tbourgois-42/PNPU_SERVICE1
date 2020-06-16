﻿using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUCore.RapportTNR;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Process
{
    internal class ProcessTNR : ProcessCore, IProcess
    {

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessTNR(int wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = ParamAppli.ProcessTNR;
            this.LibProcess = "Tests de Non Régression (TNR)";
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            Logger.Log(this, ParamAppli.StatutInfo, " Debut du process " + this.ToString());

            ControleTNR CTNR = new ControleTNR(this);

            sRapport = string.Empty;
            RapportTNR.Name = "Tests de Non Régression (TNR)";
            RapportTNR.Debut = DateTime.Now;
            RapportTNR.IdClient = CLIENT_ID;

            Domaine RapportDomaine = new RapportTNR.Domaine();
            RapportDomaine.Name = "Paie";
            RapportDomaine.Result = string.Empty;

            SousDomaine RapportSousDomaine = new SousDomaine();
            RapportSousDomaine.Name = "Cumuls long de paie";
            RapportSousDomaine.Result = string.Empty;

            SousDomaineParts RapportSousDomaineParts = new SousDomaineParts();
            RapportSousDomaineParts.Name = "Ecarts agrégés par classification";
            RapportSousDomaineParts.Result = string.Empty;

            // Client TNR database Read Node items
            DataSet ItemsNoeudReadTNR = CTNR.GetItemsNoeudRead(ParamAppli.ConnectionStringBaseQA2);

            // Client REF database Read Node items
            DataSet ItemsNoeudReadREF = CTNR.GetItemsNoeudRead(ParamAppli.ConnectionStringBaseQA1);

            // Payment date
            // TOTO : Read M4SCO_HT_PAYS in order to get the last open pay
            DateTime sDate = new DateTime(2020, 2, 25);
            
            Dictionary<string, string> lstCumulativeTable = CTNR.GetListOfCumulativeTable(ItemsNoeudReadTNR);

            Dictionary<string, Classification>  lstClassification = new Dictionary<string, Classification>();
            Classification RapportClassification = null;

            int index = -1;
            int reg = 1;

            // Loop into TNR node.
            foreach (DataRow drRow in ItemsNoeudReadTNR.Tables[0].Rows)
            {
                Console.WriteLine("Traitement de l'item " + drRow[1].ToString() + ", " + Decimal.Round((reg * 100) / ItemsNoeudReadTNR.Tables[0].Rows.Count, 2) + "%");
                Logger.Log(this, ParamAppli.StatutInfo, "Traitement de l'item " + drRow[1].ToString() + ", " + Decimal.Round((reg * 100) / ItemsNoeudReadTNR.Tables[0].Rows.Count, 2) + "%");
                
                index = CTNR.FindIndexOfBaseRef(drRow[1].ToString(), ItemsNoeudReadREF, ItemsNoeudReadTNR);

                if (lstClassification.ContainsKey(drRow[6].ToString()) == false)
                {   
                    RapportClassification = new Classification();
                    CTNR.CreateClassification(RapportClassification, drRow, lstClassification, RapportSousDomaineParts);
                }
                else
                {
                    RapportClassification = CTNR.MoveToClassification(RapportClassification, lstClassification, drRow);
                }

                // Add item difference with "NEW" specificity if index equals to -1, If not we set comment at empty string
                Ecarts RapportEcarts = new Ecarts();
                CTNR.AddItemEcart(RapportEcarts, drRow, index, sDate, "NEW");

                if (RapportEcarts.Difference > 0 || RapportEcarts.Difference < 0 || RapportEcarts.Comment == "NEW")
                {
                    CTNR.SetStatusWarning(RapportTNR, RapportDomaine, RapportSousDomaine, RapportSousDomaineParts, RapportClassification);
                    RapportClassification.Ecarts.Add(RapportEcarts);

                    DataSet itemValuesBaseQA2 = CTNR.GetItemValues(drRow, sDate, ParamAppli.ConnectionStringBaseQA2, lstCumulativeTable);
                    DataSet itemValuesBaseQA1 = CTNR.GetItemValues(drRow, sDate, ParamAppli.ConnectionStringBaseQA1, lstCumulativeTable);
                    CTNR.CheckDifference(RapportEcarts, itemValuesBaseQA1, itemValuesBaseQA2, drRow, sDate);

                } else
                {
                    CTNR.SetStatusCorrect(RapportClassification);
                }
                reg++;
            }

            // Loop into REF node in order to find all items deleted in TNR database.
            foreach (DataRow drRow in ItemsNoeudReadREF.Tables[0].Rows)
            {
                index = CTNR.FindIndexOfBaseTnr(drRow[1].ToString(), ItemsNoeudReadREF, ItemsNoeudReadTNR);

                if (index == -1)
                {
                    if (lstClassification.ContainsKey(drRow[6].ToString()) == false)
                    {
                        // Create new instance of classification if not exist
                        RapportClassification = new Classification();
                        CTNR.CreateClassification(RapportClassification, drRow, lstClassification, RapportSousDomaineParts);
                    }
                    else
                    {
                        // MoveTo classification if allready exist
                        RapportClassification = CTNR.MoveToClassification(RapportClassification, lstClassification, drRow);
                    }
                    // Add item difference with "DELETED" specificity if index equals to -1
                    Ecarts RapportEcarts = new Ecarts();
                    CTNR.AddItemEcart(RapportEcarts, drRow, index, sDate, "DELETED");
                }
            }

            RapportSousDomaine.SousDomaineParts.Add(RapportSousDomaineParts);
            RapportDomaine.SousDomaine.Add(RapportSousDomaine);
            RapportTNR.Domaine.Add(RapportDomaine);

            RapportTNR.Fin = DateTime.Now;

            // If control is OK we generate historics lines
            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW();
            PNPU_H_STEP historicStep = new PNPU_H_STEP();

            historicWorkflow.CLIENT_ID = this.CLIENT_ID;
            historicWorkflow.LAUNCHING_DATE = RapportTNR.Debut;
            historicWorkflow.WORKFLOW_ID = this.WORKFLOW_ID;
            InfoClient client = RequestTool.getClientsById(this.CLIENT_ID);

            historicStep.ID_PROCESS = this.PROCESS_ID;
            historicStep.ITERATION = 1;
            historicStep.WORKFLOW_ID = this.WORKFLOW_ID;
            historicStep.CLIENT_ID = this.CLIENT_ID;
            historicStep.CLIENT_NAME = client.CLIENT_NAME;
            historicStep.USER_ID = "PNPUADM";
            historicStep.TYPOLOGY = "SAAS DEDIE";
            historicStep.LAUNCHING_DATE = RapportTNR.Debut;
            historicStep.ENDING_DATE = RapportTNR.Fin;
            historicStep.ID_STATUT = RapportTNR.Result;

            GenerateHistoric(RapportTNR.Fin, RapportTNR.Result);

            if (RapportTNR.Result == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessTNR);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID);
            } 
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessTNR(WORKFLOW_ID, CLIENT_ID);
        }
    }
}