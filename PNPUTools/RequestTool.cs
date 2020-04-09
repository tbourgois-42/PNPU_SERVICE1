using Newtonsoft.Json;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PNPUTools
{
    public class RequestTool
    {

        static string requestAllClient = "select CLI.CLIENT_ID, DATABASE_ID, CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID";
        static string requestOneClient = "select CLI.CLIENT_ID, DATABASE_ID, CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID AND TRIGRAMME = ";
        static string connectionStringSupport = "server=M4FRDB16;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";
        static string connectionStringCapitalDev = "server=M4FRDB18;uid=CAPITAL_DEV;pwd=Cpldev2017;database=CAPITAL_DEV;";

        static string requestAllStep = "select * from PNPU_STEP";
        static string requestAllInfoClient = "select CLIENT_ID, LAUNCHING_DATE, ID_STATUT, PROCESS_LABEL, PS.ORDER_ID, (PS.ORDER_ID/(MAX(PSS.ORDER_ID) + 1))*100 AS PERCENTAGE_COMPLETUDE, PHS.TYPOLOGY from PNPU_H_STEP PHS, PNPU_STEP PS, PNPU_PROCESS PR, PNPU_STEP PSS  where PHS.WORKFLOW_ID = PS.WORKFLOW_ID AND PHS.ID_PROCESS = PS.ID_PROCESS AND PR.ID_PROCESS = PS.ID_PROCESS AND PSS.WORKFLOW_ID = PHS.WORKFLOW_ID group by CLIENT_ID, LAUNCHING_DATE, ID_STATUT, PROCESS_LABEL, PS.ORDER_ID, PHS.TYPOLOGY";

        static string requestAllProcess = "select * from PNPU_PROCESS";
        static string requestOneProcess = "select * from PNPU_PROCESS where ID_PROCESS = ";

        static string requestAllWorkflow = "select * from PNPU_WORKFLOW";
        static string requestOneWorkflow = "select * from PNPU_WORKFLOW where WORKFLOW_ID = ";

        static string requestGetWorkflowProcesses = "SELECT PP.PROCESS_LABEL, PS.ORDER_ID FROM PNPU_STEP PS, PNPU_PROCESS PP, PNPU_WORKFLOW PW WHERE PS.ID_PROCESS = PP.ID_PROCESS AND PS.WORKFLOW_ID = PW.WORKFLOW_ID AND PS.WORKFLOW_ID = ";


        public static IEnumerable<InfoClientStep> GetAllInfoClient()
        {

            DataSet result = DataManagerSQLServer.GetDatas(requestAllInfoClient, connectionStringCapitalDev);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClientStep> listTest = table.DataTableToList<InfoClientStep>();

            return listTest;
        }

        public static string GetInfoOneClient(string clientName)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneClient + "'" + clientName + "'", connectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            var regex = new Regex(Regex.Escape("Table"));
            var newJson = regex.Replace(json, "Clients", 1);
            newJson = newJson.Replace("\r\n", "");
            return newJson;
        }


        public static PNPU_PROCESS GetProcess(string processId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneProcess + processId, connectionStringCapitalDev);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_PROCESS> listTest = table.DataTableToList<PNPU_PROCESS>();

            return listTest.First();
        }

        public static IEnumerable<PNPU_PROCESS> GetAllProcesses()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllProcess, connectionStringCapitalDev);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_PROCESS> listTest = table.DataTableToList<PNPU_PROCESS>();

            return listTest;
        }

        public static string CreateWorkflow(PNPU_WORKFLOW input)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connectionStringCapitalDev))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_WORKFLOW (ID_ORGANIZATION, WORKFLOW_ID, WORKFLOW_LABEL) VALUES('0000', @WORKFLOW_ID, @WORKFLOW_LABEL)", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = input.WORKFLOW_ID;
                        cmd.Parameters.Add("@WORKFLOW_LABEL", SqlDbType.VarChar, 254).Value = input.WORKFLOW_LABEL;
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    return ex.ToString();
                }
                return "Requête traitée avec succès et création d’un document.";
            }
        }

        public static PNPU_WORKFLOW getWorkflow(string workflowId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneWorkflow + workflowId, connectionStringCapitalDev);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest.First();
        }

        public static string ModifyWorkflow(PNPU_WORKFLOW input, string workflowID)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connectionStringCapitalDev))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("UPDATE PNPU_WORKFLOW SET WORKFLOW_LABEL = @WORKFLOW_LABEL WHERE WORKFLOW_ID = @WORKFLOW_ID AND ID_ORGANIZATION = '0000'", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = workflowID;
                        cmd.Parameters.Add("@WORKFLOW_LABEL", SqlDbType.VarChar, 254).Value = input.WORKFLOW_LABEL;
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    return ex.ToString();
                }
                return "Requête traitée avec succès et création d’un document.";
            }
        }

        public static IEnumerable<PNPU_WORKFLOW> GetAllWorkFLow()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllWorkflow, connectionStringCapitalDev);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest;
        }


        public static IEnumerable<PNPU_STEP> GetAllStep()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllStep, connectionStringCapitalDev);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_STEP> listTest = table.DataTableToList<PNPU_STEP>();

            return listTest;
        }

        public static IEnumerable<PNPU_WORKFLOWPROCESSES> GetWorkflowProcesses(string workflowId)
        {
            string OrderBy = " ORDER BY PS.ORDER_ID";

            DataSet result = DataManagerSQLServer.GetDatas(requestGetWorkflowProcesses + workflowId + OrderBy, connectionStringCapitalDev);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_WORKFLOWPROCESSES> listTest = table.DataTableToList<PNPU_WORKFLOWPROCESSES>();

            return listTest;
        }
    }

}
