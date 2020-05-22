using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;
using PNPUCore.Controle;

namespace PNPUCore.Process
{
    public interface IProcess
    {
        void ExecuteMainProcess();

        String FormatReport();
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

        public string sRapport;
        public RProcess RapportProcess;
        public RControle RapportControleCourant;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessCore(int wORKFLOW_ID, string cLIENT_ID)
        {
            RapportProcess = new RProcess();
            WORKFLOW_ID = wORKFLOW_ID;
            CLIENT_ID = cLIENT_ID;
            STANDARD = true;
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
            if (ParamAppli.SimpleCotesReport == true)
                sMessage = sMessage.Replace("'", "''");
            RapportControleCourant.Message.Add(sMessage);
        }

        /// <summary>  
        /// Méthode appelée par le launcher pour créé le process. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>
        /// <returns>Retourne l'instance du process créé.</returns>
        internal static IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessCore(WORKFLOW_ID, CLIENT_ID);
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

        public string SaveReportInBDD(string json, IProcess process)
        {
            string[] requests = { "INSERT INTO PNPU_H_REPORT (ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, JSON_TEMPLATE) VALUES(@ITERATION, @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @JSON_TEMPLATE)" };
            string[] parameters = new string[] { "@ITERATION", "1", "@WORKFLOW_ID", process.WORKFLOW_ID.ToString(), "@ID_PROCESS", process.PROCESS_ID.ToString(), "@CLIENT_ID", process.CLIENT_ID, "@JSON_TEMPLATE", json.Replace("\r\n", "") };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_REPORT", parameters, false);

            /*using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_H_REPORT (ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, JSON_TEMPLATE) VALUES(@ITERATION, @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @JSON_TEMPLATE)", conn))
                    {
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int, 10).Value = 1;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int, 15).Value = process.WORKFLOW_ID;
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int, 15).Value = process.PROCESS_ID;
                        cmd.Parameters.Add("@CLIENT_ID", SqlDbType.VarChar, 64).Value = process.CLIENT_ID;
                        cmd.Parameters.Add("@JSON_TEMPLATE", SqlDbType.Text).Value = json.Replace("\r\n", "");
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    string msgError = "L'insertion du report du client " + process.CLIENT_ID + ", process " + process.PROCESS_ID + " ,workflow " + process.WORKFLOW_ID + " a échoué !";
                    Console.WriteLine(msgError);
                    return false;
                }
                return true;
            }*/
        }

        internal void GenerateHistoric(PNPU_H_WORKFLOW historicWorkflow, PNPU_H_STEP historicStep)
        {
            RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow);
            RequestTool.CreateUpdateStepHistoric(historicStep);
        }


        /// <summary>
        /// Methode permettant de récupérer dynamiquement la liste des contrôles à lancer en fonction du process, de la typologie client et du type de pack (standard ou non).
        /// </summary>
        /// <param name="listControl">Au retour de l'appel contient la liste des contrôles à exécuter dans le process</param>
        protected void GetListControle(ref List<IControle> listControl)
        {
            DataManagerSQLServer dmsDataManager = new DataManagerSQLServer();
            DataSet dsDataSet;
            string sRequete = "SELECT ID_CONTROLE, CONTROLE_LABEL, TYPOLOGY, RUN_STANDARD, ID_PROCESS, ERROR_TYPE, TOOLTIP FROM PNPU_CONTROLE WHERE ID_PROCESS =" + this.PROCESS_ID.ToString();

            if ((this.CLIENT_ID != string.Empty) && (this.CLIENT_ID != "ALL"))
            {
                try
                {
                    string sClient_ID;
                    if (this.CLIENT_ID.Contains(",") == true)
                        sClient_ID = this.CLIENT_ID.Split(',')[0];
                    else
                        sClient_ID = this.CLIENT_ID;
                    this.TYPOLOGY = ParamAppli.ListeInfoClient[sClient_ID].TYPOLOGY_ID;
                    if (this.TYPOLOGY != string.Empty)
                        sRequete += " AND ((TYPOLOGY IS NULL) OR (TYPOLOGY LIKE '%*" + this.TYPOLOGY + "*%'))";
                }
                catch (Exception)
                { }
            }
            /*else if (this.TYPOLOGY != string.Empty)
            {
                sRequete += " AND ((TYPOLOGY IS NULL) OR (TYPOLOGY LIKE '%*" + this.TYPOLOGY + "*%'))";
            }
            */
            if (this.STANDARD == false)
            {
                sRequete += " AND ((RUN_STANDARD IS NULL) OR (RUN_STANDARD <> 'YES'))";
            }

            listControl.Clear();
            dsDataSet = dmsDataManager.GetData(sRequete,ParamAppli.ConnectionStringBaseAppli);

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