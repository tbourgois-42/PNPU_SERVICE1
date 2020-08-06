using HttpMultipartParser;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PNPUTools
{
    /// <summary>
    /// Load toolbox parameters in order to all have access throuth the application
    /// </summary>
    public class ParamToolbox
    {
        public string ServerBefore { get; set; }
        private string DatabaseBefore { get; set; }
        private string PasswordBefore { get; set; }
        private string ServerAfter { get; set; }
        private string DatabaseAfter { get; set; }
        private string PasswordAfter { get; set; }
        private DateTime DtPaie { get; set; }
        private int ClientId { get; set; }
        private int WorkflowId { get; set; }

        /// <summary>
        /// Get connexion string for QA1 database.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns>Return connexion string.</returns>
        private string GetConnexionStringQA1(int workflowId, string sClientId)
        {
            if (IsWorkflowToolbox(workflowId))
            {
                string sRequestSelect = "SELECT * FROM PNPU_TMP_PARAM_TOOLBOX WHERE WORKFLOW_ID = {0}";

                string sRequest = string.Format(sRequestSelect, workflowId);

                DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

                IEnumerable<PNPU_TMP_PARAM_TOOLBOX> lstParams = dsDataSet.Tables[0].DataTableToList<PNPU_TMP_PARAM_TOOLBOX>();

                return "server=" + lstParams.ElementAt(0).SERVER_BEFORE + ";uid=" + lstParams.ElementAt(0).DATABASE_BEFORE + ";pwd=" + lstParams.ElementAt(0).PASSWORD_BEFORE + ";database=" + lstParams.ElementAt(0).DATABASE_BEFORE + ";";
            }
            else
            {
                return GetConnexionStringFromInfoClient("QA1", sClientId);
            }

        }

        /// <summary>
        /// Get connexion string for QA2 database.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns>Return connexion string.</returns>
        private string GetConnexionStringQA2(int workflowId, string sClientId)
        {
            if (IsWorkflowToolbox(workflowId))
            {
                string sRequestSelect = "SELECT * FROM PNPU_TMP_PARAM_TOOLBOX WHERE WORKFLOW_ID = {0}";

                string sRequest = string.Format(sRequestSelect, workflowId);

                DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

                IEnumerable<PNPU_TMP_PARAM_TOOLBOX> lstParams = dsDataSet.Tables[0].DataTableToList<PNPU_TMP_PARAM_TOOLBOX>();

                return "server=" + lstParams.ElementAt(0).SERVER_AFTER + ";uid=" + lstParams.ElementAt(0).DATABASE_AFTER + ";pwd=" + lstParams.ElementAt(0).PASSWORD_AFTER + ";database=" + lstParams.ElementAt(0).DATABASE_AFTER + ";";
            }
            else
            {
                return GetConnexionStringFromInfoClient("QA2", sClientId);
            }
        }

        /// <summary>
        /// Get connexion string both on QA1 or QA2. If you want QA1 database please enter Before for sDatabase parameter, otherwise enter After for QA2.
        /// </summary>
        /// <param name="sDatabase"></param>
        /// <param name="workflowId"></param>
        /// <returns>Return connexion string.</returns>
        public string GetConnexionString(string sDatabase, int workflowId, string sClientId)
        {
            switch (sDatabase)
            {
                case "Before":
                    return GetConnexionStringQA1(workflowId, sClientId);
                case "After":
                    return GetConnexionStringQA2(workflowId, sClientId);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get connexion string from fiche info client.
        /// </summary>
        /// <returns></returns>
        public string GetConnexionStringFromInfoClient(string sDatabase, string sClientId)
        {
            string sDatabaseNameTranscoded = GetDatabaseNameTranscoded(sClientId, sDatabase);

            string sRequestSelect = "SELECT DATABASE_ID, HOST, USER_ACCOUNT, USER_PASSWORD, CLIENT_ID FROM DBS WHERE CLIENT_ID = {0} AND USER_ACCOUNT LIKE '%{1}%'";

            string sRequest = string.Format(sRequestSelect, sClientId, sDatabaseNameTranscoded);

            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringSupport);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                /// Database name as MT4XXXQAX was found in support database
                return GenerateODBCConnexionString(dsDataSet.Tables[0]);
            }
            else
            {
                // Database name as MT4XXXQAX format does not exist
                string sDatabaseNameClient = GetDatabaseNameClient(sClientId, sDatabase);

                sRequest = string.Format(sRequestSelect, sClientId, sDatabaseNameClient);

                dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringSupport);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    /// Database client name was found in support database
                    return GenerateODBCConnexionString(dsDataSet.Tables[0]);
                }
                else
                {
                    return "No database was found for client " + sClientId;
                }
            }
        }

        private string GenerateODBCConnexionString(DataTable dataTable)
        {
            StringBuilder sConnexionString = new StringBuilder();

            sConnexionString.Append("server=");
            sConnexionString.Append(dataTable.Rows[2].ToString());
            sConnexionString.Append(";uid=");
            sConnexionString.Append(dataTable.Rows[1].ToString());
            sConnexionString.Append(";pwd=");
            sConnexionString.Append(dataTable.Rows[3].ToString());
            sConnexionString.Append(";database=");
            sConnexionString.Append(dataTable.Rows[2].ToString());
            sConnexionString.Append(";");

            return sConnexionString.ToString();
        }

        /// <summary>
        /// Get paiement date entered in toolbox params. 
        /// </summary>
        /// <param name="wORKFLOW_ID"></param>
        /// <param name="iD_INSTANCEWF"></param>
        /// <returns>Return paiement date.</returns>
        public DateTime GetDtPaie(int wORKFLOW_ID, int iD_INSTANCEWF)
        {
            string sRequestSelect = "SELECT DT_PAIE FROM PNPU_TMP_PARAM_TOOLBOX WHERE WORKFLOW_ID = {0} AND ID_H_WORKFLOW = {1}";

            string sRequest = string.Format(sRequestSelect, wORKFLOW_ID, iD_INSTANCEWF);

            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return DateTime.Parse(result.Tables[0].Rows[0].ItemArray[0].ToString());
        }

        /// <summary>
        /// Save toolbox params in PNPU temporary table.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="idInstanceWF"></param>
        public string SaveParamsToolbox(MultipartFormDataParser parser, int idInstanceWF)
        {
            ServerBefore = parser.GetParameterValue("serverBefore");
            DatabaseBefore = parser.GetParameterValue("databaseBefore");
            PasswordBefore = parser.GetParameterValue("passwordBefore");
            ServerAfter = parser.GetParameterValue("serverAfter");
            DatabaseAfter = parser.GetParameterValue("databaseAfter");
            PasswordAfter = parser.GetParameterValue("passwordAfter");
            DtPaie = DateTime.Parse(parser.GetParameterValue("dtPaie"));
            ClientId = int.Parse(parser.GetParameterValue("clientID"));
            WorkflowId = int.Parse(parser.GetParameterValue("workflowID"));

            string[] sRequest = { "INSERT INTO PNPU_TMP_PARAM_TOOLBOX (SERVER_BEFORE, DATABASE_BEFORE, PASSWORD_BEFORE, SERVER_AFTER, DATABASE_AFTER, PASSWORD_AFTER, DT_PAIE, CLIENT_ID, WORKFLOW_ID, ID_H_WORKFLOW ) VALUES (@SERVER_BEFORE, @DATABASE_BEFORE, @PASSWORD_BEFORE, @SERVER_AFTER, @DATABASE_AFTER, @PASSWORD_AFTER, @DT_PAIE, @CLIENT_ID, @WORKFLOW_ID, @ID_H_WORKFLOW)" };
            string[] parameters = new string[] { "@SERVER_BEFORE", ServerBefore, "@DATABASE_BEFORE", DatabaseBefore, "@PASSWORD_BEFORE", PasswordBefore, "@SERVER_AFTER", ServerAfter, "@DATABASE_AFTER", DatabaseAfter, "@PASSWORD_AFTER", PasswordAfter, "@DT_PAIE", DtPaie.ToString("MM/dd/yyyy HH:mm:ss"), "@CLIENT_ID", ClientId.ToString(), "@WORKFLOW_ID", WorkflowId.ToString(), "@ID_H_WORKFLOW", idInstanceWF.ToString() };

            try
            {
                return DataManagerSQLServer.ExecuteSqlTransaction(sRequest, "PNPU_TMP_PARAM_TOOLBOX", parameters);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Delete all toolbox params.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="idInstanceWF"></param>
        /// <returns></returns>
        public string DeleteParamsToolbox(int workflowId, int idInstanceWF)
        {
            string[] requests = { "DELETE FROM PNPU_TMP_PARAM_TOOLBOX WHERE WORKFLOW_ID = @WORKFLOW_ID AND ID_H_WORKFLOW = @ID_H_WORKFLOW" };
            string[] parameters = new string[] { "@WORKFLOW_ID", workflowId.ToString(), "@ID_H_WORKFLOW", idInstanceWF.ToString() };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_TMP_PARAM_TOOLBOX", parameters);
        }

        /// <summary>
        /// Check if workflow if a toolbox flag workflow.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns>Return true if it is, false if not.</returns>
        public bool IsWorkflowToolbox(int workflowId)
        {
            string sRequestSelect = "SELECT IS_TOOLBOX FROM PNPU_WORKFLOW WHERE WORKFLOW_ID = {0}";

            string sRequest = string.Format(sRequestSelect, workflowId);

            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return bool.Parse(result.Tables[0].Rows[0].ItemArray[0].ToString());
        }


        /// <summary>
        /// Set database name transcoded.
        /// </summary>
        /// <param name="sClientId"></param>
        /// <returns>Return string database name transcoded like MT4XXXQA1 or MT4XXXQA2 where XXX corespond to client trigram.</returns>
        public string GetDatabaseNameTranscoded(string sClientId, string sDatabase)
        {
            string sTrigram = GetClientTrigram(sClientId);
            string sRequestSelect = "SELECT TRIGRAMME FROM A_CLIENT WHERE CLIENT_ID = {0}";

            string sRequest = string.Format(sRequestSelect, sClientId);

            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringSupport);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                return "MT4" + sTrigram + sDatabase;
            }
            else
            {
                return "No transcoded database name was found for client" + sClientId;
            }
        }

        public string GetDatabaseNameClient(string sClientId, string sDatabase)
        {
            string sRequestSelect = "SELECT DATABASE_NAME_CLIENT FROM PNPU_TRANSCO_DATABASE WHERE CLIENT_ID = {0} AND DATABASE_NAME_TRANSCO = '{1}'";

            string sRequest = string.Format(sRequestSelect, sClientId, sDatabase);

            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                return dsDataSet.Tables[0].Rows[0].ItemArray[0].ToString();
            }
            else
            {
                return "No client database name was found for client" + sClientId;
            }
        }

        /// <summary>
        /// Get client trigram.
        /// </summary>
        /// <param name="sClientId"></param>
        /// <returns>Return trigram string.</returns>
        public string GetClientTrigram(string sClientId)
        {
            string sRequestSelect = "SELECT TRIGRAMME FROM A_CLIENT WHERE CLIENT_ID = {0}";

            string sRequest = string.Format(sRequestSelect, sClientId);

            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringSupport);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                return dsDataSet.Tables[0].Rows[0].ItemArray[0].ToString();
            }
            else
            {
                return "Client trigram does not exist.";
            }
        }
    }
}
