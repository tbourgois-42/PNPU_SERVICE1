using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUCore.RapportLivraison;
using PNPUCore.RapportTNR;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Process
{
    public interface IProcess
    {
        void ExecuteMainProcess();

        String FormatReport(IProcess process);
        void AjouteRapport(string v);
        string SaveReportInBDD(string json, IProcess process);
        int WORKFLOW_ID { get; set; }
        string CLIENT_ID { get; set; }

        string STATUT { get; set; }

        int PROCESS_ID { get; set; }

        string BASE { get; set; }

        string SERVER { get; set; }

        string TYPOLOGY { get; set; }

        bool STANDARD { get; set; }
        int ID_INSTANCEWF { get; set; }
    }

    internal class ProcessCore : IProcess
    {
        public string LibProcess { get; set; }
        public int WORKFLOW_ID { get; set; }
        public string CLIENT_ID { get; set; }
        public string STATUT { get; set; }
        public int PROCESS_ID { get; set; }
        public string BASE { get; set; }
        public string SERVER { get; set; }
        public string TYPOLOGY { get; set; }
        public bool STANDARD { get; set; }
        public int ID_INSTANCEWF { get; set; }

        public string sRapport;
        public RProcess RapportProcess;
        public RTNR RapportTNR;
        public RControle RapportControleCourant;
        public RLivraison RapportLivraison;
        public Localisation RapportLocalisation;
        public RapportAnalyseData rapportAnalyseImpactData;
        public RapportAnalyseLogique RapportAnalyseImpactLogique;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessCore(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF)
        {
            RapportProcess = new RProcess();
            RapportTNR = new RTNR();
            RapportLivraison = new RLivraison();
            RapportLocalisation = new Localisation();
            WORKFLOW_ID = wORKFLOW_ID;
            CLIENT_ID = cLIENT_ID;
            STANDARD = true;
            ID_INSTANCEWF = idInstanceWF;

            if (!CLIENT_ID.Contains(","))
            {
                TYPOLOGY = ParamAppli.ListeInfoClient[CLIENT_ID].TYPOLOGY;
            }
            else
                TYPOLOGY = ParamAppli.ListeInfoClient[CLIENT_ID.Split(',')[0]].TYPOLOGY;
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
            sMessage = sMessage.Replace("'", "''");
            RapportControleCourant.Message.Add(sMessage);
        }

        /// <summary>  
        /// Méthode appelée par le launcher pour créé le process. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>
        /// <returns>Retourne l'instance du process créé.</returns>
        internal static IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessCore(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }

        public void ExecuteMainProcess()
        {
            throw new NotImplementedException();
        }

        /// <summary>  
        /// Méthode générant le rapport du déroulement du process au format JSON.
        /// <returns>Retourne le rapport au format JSON dans une chaine de caractères.</returns>
        /// </summary>  
        public string FormatReport(IProcess process)
        {
            if (ParamAppli.ProcessTNR == process.PROCESS_ID)
            {
                RapportTNR.Fin = DateTime.Now;
                return (RapportTNR.ToJSONRepresentation());
            }
            else if (ParamAppli.ProcessAnalyseImpactData == process.PROCESS_ID)
            {
                rapportAnalyseImpactData.Fin = DateTime.Now;
                return (rapportAnalyseImpactData.ToJSONRepresentation());
            }
            else if (ParamAppli.ProcessLivraison == process.PROCESS_ID)
            {
                RapportAnalyseImpactLogique.Fin = DateTime.Now;
                return (RapportAnalyseImpactLogique.ToJSONRepresentation());
            }
            else if (ParamAppli.ProcessAnalyseImpactLogique == process.PROCESS_ID)
            {
                RapportLivraison.Fin = DateTime.Now;
                return (RapportLivraison.ToJSONRepresentation(RapportLocalisation));
            }
            else
            {
                RapportProcess.Fin = DateTime.Now;
                return (RapportProcess.ToJSONRepresentation());
            }
        }

        /// <summary>  
        /// Méthode générant le rapport du déroulement du process au format JSON.
        /// <returns>Retourne le rapport TNR au format JSON dans une chaine de caractères.</returns>
        /// </summary>  
        public string FormatReportTNR()
        {
            RapportTNR.Fin = DateTime.Now;
            return (RapportTNR.ToJSONRepresentation());
        }

        /// <summary>
        /// Insert Json Report into PNPU_H_REPORT
        /// </summary>
        /// <param name="json"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public string SaveReportInBDD(string json, IProcess process)
        {
            string[] requests = { "INSERT INTO PNPU_H_REPORT (ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, JSON_TEMPLATE, ID_H_WORKFLOW) VALUES(@ITERATION, @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @JSON_TEMPLATE, @ID_H_WORKFLOW)" };
            string[] parameters = new string[] { "@ITERATION", "1", "@WORKFLOW_ID", process.WORKFLOW_ID.ToString(), "@ID_PROCESS", process.PROCESS_ID.ToString(), "@CLIENT_ID", process.CLIENT_ID, "@JSON_TEMPLATE", json.Replace("\r\n", ""), "@ID_H_WORKFLOW", process.ID_INSTANCEWF.ToString() };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_REPORT", parameters, false);
        }

        /// <summary>
        /// Generate historic line for PNPU_H_STEP and PNPU_H_WORKFLOW for one client
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="statut"></param>
        internal void GenerateHistoric(DateTime endDate, string statut, DateTime debut)
        {
            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW();
            PNPU_H_STEP historicStep = new PNPU_H_STEP();

            historicWorkflow.CLIENT_ID = CLIENT_ID;
            historicWorkflow.LAUNCHING_DATE = debut;
            historicWorkflow.WORKFLOW_ID = WORKFLOW_ID;
            historicWorkflow.ID_H_WORKFLOW = ID_INSTANCEWF;
            InfoClient client = RequestTool.getClientsById(CLIENT_ID);

            historicStep.ID_H_WORKFLOW = ID_INSTANCEWF;
            historicStep.ID_PROCESS = PROCESS_ID;
            historicStep.ITERATION = 1;
            historicStep.WORKFLOW_ID = WORKFLOW_ID;
            historicStep.CLIENT_ID = CLIENT_ID;
            historicStep.CLIENT_NAME = client.CLIENT_NAME;
            historicStep.USER_ID = "PNPUADM";
            if (client.TYPOLOGY == "Dédié")
            {
                historicStep.TYPOLOGY = "SAAS DEDIE";
            }
            else if (client.TYPOLOGY == "Désynchronisé")
            {
                historicStep.TYPOLOGY = "SAAS DESYNCHRONISE";
            }
            else if (client.TYPOLOGY == "Mutualisé")
            {
                historicStep.TYPOLOGY = "SAAS MUTUALISE";
            }
            else
            {
                historicStep.TYPOLOGY = "Typo not found";
            }
            historicStep.LAUNCHING_DATE = debut;
            historicStep.ENDING_DATE = endDate;
            historicStep.ID_STATUT = statut;

            RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow);
            RequestTool.CreateUpdateStepHistoric(historicStep);
        }

        /// <summary>
        /// Generate hitoric line for PNPU_H_STEP and PNPU_H_WORKFLOW
        /// One line by client
        /// </summary>
        /// <param name="listClientId"></param>
        /// <param name="fin"></param>
        /// <param name="globalResult"></param>
        public void GenerateHistoricGlobal(string[] listClientId, DateTime fin, string globalResult, int idInstanceWF, DateTime debut)
        {
            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW();
            historicWorkflow.CLIENT_ID = CLIENT_ID;
            historicWorkflow.LAUNCHING_DATE = debut;
            historicWorkflow.ENDING_DATE = new DateTime(1800, 1, 1);
            historicWorkflow.STATUT_GLOBAL = "IN PROGRESS";
            historicWorkflow.WORKFLOW_ID = WORKFLOW_ID;
            historicWorkflow.ID_H_WORKFLOW = ID_INSTANCEWF;

            idInstanceWF = int.Parse(RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow));

            foreach (string clientId in listClientId)
            {
                InfoClient client = RequestTool.getClientsById(clientId);
                PNPU_H_STEP historicStep = new PNPU_H_STEP();
                historicStep.ID_PROCESS = PROCESS_ID;
                historicStep.ITERATION = 1;
                historicStep.WORKFLOW_ID = WORKFLOW_ID;
                historicStep.CLIENT_ID = clientId;
                historicStep.CLIENT_NAME = client.CLIENT_NAME;
                historicStep.USER_ID = "PNPUADM";
                historicStep.LAUNCHING_DATE = debut;
                historicStep.ENDING_DATE = fin;
                historicStep.ID_H_WORKFLOW = idInstanceWF;

                if (client.TYPOLOGY == "Dédié")
                {
                    historicStep.TYPOLOGY = "SAAS DEDIE";
                }
                else if (client.TYPOLOGY == "Désynchronisé")
                {
                    historicStep.TYPOLOGY = "SAAS DESYNCHRONISE";
                }
                else if (client.TYPOLOGY == "Mutualisé")
                {
                    historicStep.TYPOLOGY = "SAAS MUTUALISE";
                }
                else
                {
                    historicStep.TYPOLOGY = "Typo not found";
                }

                historicStep.ID_STATUT = globalResult;
                RequestTool.CreateUpdateStepHistoric(historicStep);
            }

        }

        /// <summary>
        /// Methode permettant de récupérer dynamiquement la liste des contrôles à lancer en fonction du process, de la typologie client et du type de pack (standard ou non).
        /// </summary>
        /// <param name="listControl">Au retour de l'appel contient la liste des contrôles à exécuter dans le process</param>
        protected void GetListControle(ref List<IControle> listControl)
        {
            DataManagerSQLServer dmsDataManager = new DataManagerSQLServer();
            DataSet dsDataSet;
            string sTypo;
            string sRequete = "SELECT ID_CONTROLE, CONTROLE_LABEL, TYPOLOGY, RUN_STANDARD, ID_PROCESS, ERROR_TYPE, TOOLTIP FROM PNPU_CONTROLE WHERE ID_PROCESS =" + PROCESS_ID.ToString();

            if ((CLIENT_ID != string.Empty) && (CLIENT_ID != "ALL"))
            {
                try
                {
                    string sClient_ID;
                    if (CLIENT_ID.Contains(","))
                        sClient_ID = CLIENT_ID.Split(',')[0];
                    else
                        sClient_ID = CLIENT_ID;
                    sTypo = ParamAppli.ListeInfoClient[sClient_ID].TYPOLOGY_ID;
                    if (sTypo != string.Empty)
                        sRequete += " AND ((TYPOLOGY IS NULL) OR (TYPOLOGY = '') OR (TYPOLOGY LIKE '%*" + sTypo + "*%'))";
                }
                catch (Exception ex)
                {
                    //TODO LOG
                    Console.WriteLine(ex.Message);
                }
            }

            if (!STANDARD )
            {
                sRequete += " AND ((RUN_STANDARD IS NULL) OR (RUN_STANDARD <> 'YES'))";
            }

            listControl.Clear();
            dsDataSet = dmsDataManager.GetData(sRequete, ParamAppli.ConnectionStringBaseAppli);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    string sControle = drRow[0].ToString();
                    IControle iControle = (IControle)Activator.CreateInstance(Type.GetType(sControle), this, drRow);
                    listControl.Add(iControle);
                }
            }
        }
    }
}