using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using HttpMultipartParser;

namespace PNPUTools
{
    /// <summary>
    /// Load toolbox parameters in order to all have access throuth the application
    /// </summary>
    public class ParamToolbox
    {
        public string sServerBefore { get; set; }
        private string sDatabaseBefore { get; set; }
        private string sPasswordBefore { get; set; }
        private string sServerAfter { get; set; }
        private string sDatabaseAfter { get; set; }
        private string sPasswordAfter { get; set; }
        private DateTime dtPaie { get; set; }
        private int clientId { get; set; }
        private int workflowId { get; set; }

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
            } else
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
            string sRequestSelect = "SELECT DATABASE_ID, HOST, USER_ACCOUNT, USER_PASSWORD, CLIENT_ID FROM DBS WHERE CLIENT_ID = {0} AND USER_ACCOUNT LIKE '%{1}%'";
            string sRequest = string.Format(sRequestSelect, sClientId, sDatabase);

            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringSupport);

            IEnumerable<SUPPORT_DBS> lstParams = dsDataSet.Tables[0].DataTableToList<SUPPORT_DBS>();

            return "server=" + lstParams.ElementAt(0).USER_ACCOUNT + ";uid=" + lstParams.ElementAt(0).HOST + ";pwd=" + lstParams.ElementAt(0).USER_PASSWORD + ";database=" + lstParams.ElementAt(0).USER_ACCOUNT + ";";
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
            sServerBefore = parser.GetParameterValue("serverBefore");
            sDatabaseBefore = parser.GetParameterValue("databaseBefore");
            sPasswordBefore = parser.GetParameterValue("passwordBefore");
            sServerAfter = parser.GetParameterValue("serverAfter");
            sDatabaseAfter = parser.GetParameterValue("databaseAfter");
            sPasswordAfter = parser.GetParameterValue("passwordAfter");
            dtPaie = DateTime.Parse(parser.GetParameterValue("dtPaie"));
            clientId = int.Parse(parser.GetParameterValue("clientID"));
            workflowId = int.Parse(parser.GetParameterValue("workflowID"));

            string[] sRequest = { "INSERT INTO PNPU_TMP_PARAM_TOOLBOX (SERVER_BEFORE, DATABASE_BEFORE, PASSWORD_BEFORE, SERVER_AFTER, DATABASE_AFTER, PASSWORD_AFTER, DT_PAIE, CLIENT_ID, WORKFLOW_ID, ID_H_WORKFLOW ) VALUES (@SERVER_BEFORE, @DATABASE_BEFORE, @PASSWORD_BEFORE, @SERVER_AFTER, @DATABASE_AFTER, @PASSWORD_AFTER, @DT_PAIE, @CLIENT_ID, @WORKFLOW_ID, @ID_H_WORKFLOW)" };
            string[] parameters = new string[] { "@SERVER_BEFORE", sServerBefore, "@DATABASE_BEFORE", sDatabaseBefore, "@PASSWORD_BEFORE", sPasswordBefore, "@SERVER_AFTER", sServerAfter, "@DATABASE_AFTER", sDatabaseAfter, "@PASSWORD_AFTER", sPasswordAfter, "@DT_PAIE", dtPaie.ToString("MM/dd/yyyy HH:mm:ss"), "@CLIENT_ID", clientId.ToString(), "@WORKFLOW_ID", workflowId.ToString(), "@ID_H_WORKFLOW", idInstanceWF.ToString() };

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
    }
}
