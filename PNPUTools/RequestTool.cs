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

        static string requestAllStep = "select * from PNPU_STEP";
        static string requestAllInfoClient = "select CLIENT_ID, LAUNCHING_DATE, ID_STATUT, PROCESS_LABEL, PS.ORDER_ID, (PS.ORDER_ID/(MAX(PSS.ORDER_ID) + 1))*100 AS PERCENTAGE_COMPLETUDE, PHS.TYPOLOGY from PNPU_H_STEP PHS, PNPU_STEP PS, PNPU_PROCESS PR, PNPU_STEP PSS  where PHS.WORKFLOW_ID = PS.WORKFLOW_ID AND PHS.ID_PROCESS = PS.ID_PROCESS AND PR.ID_PROCESS = PS.ID_PROCESS AND PSS.WORKFLOW_ID = PHS.WORKFLOW_ID group by CLIENT_ID, LAUNCHING_DATE, ID_STATUT, PROCESS_LABEL, PS.ORDER_ID, PHS.TYPOLOGY";

        static string requestAllProcess = "select * from PNPU_PROCESS";
        static string requestOneProcess = "select * from PNPU_PROCESS where ID_PROCESS = ";

        static string requestAllWorkflow = "SELECT PW.ID_ORGANIZATION, PW.WORKFLOW_ID, PW.WORKFLOW_LABEL , COUNT(PS.ID_PROCESS) AS NB_PROCESS FROM PNPU_WORKFLOW PW LEFT JOIN PNPU_STEP PS ON PS.WORKFLOW_ID = PW.WORKFLOW_ID GROUP BY PW.ID_ORGANIZATION, PW.WORKFLOW_ID, PW.WORKFLOW_LABEL";
        static string requestOneWorkflow = "select * from PNPU_WORKFLOW where WORKFLOW_ID = ";

        static string requestGetWorkflowProcesses = "SELECT PP.PROCESS_LABEL, PS.ORDER_ID FROM PNPU_STEP PS, PNPU_PROCESS PP, PNPU_WORKFLOW PW WHERE PS.ID_PROCESS = PP.ID_PROCESS AND PS.WORKFLOW_ID = PW.WORKFLOW_ID AND PS.WORKFLOW_ID = ";

        static string requestGetNextProcess = "select * from PNPU_STEP STP, PNPU_PROCESS PRO, (select ORDER_ID + 1 AS NEXT_ORDER from PNPU_STEP STEP2, PNPU_PROCESS PRO2 where STEP2.ID_PROCESS = PRO2.ID_PROCESS AND STEP2.WORKFLOW_ID = {0} AND PRO2.ID_PROCESS = '{1}') AS STEPN where STP.ORDER_ID = STEPN.NEXT_ORDER AND STP.WORKFLOW_ID = {0} AND STP.ID_PROCESS = PRO.ID_PROCESS";

        public static IEnumerable<InfoClientStep> GetAllInfoClient()
        {
            
            DataSet result = DataManagerSQLServer.GetDatas(requestAllInfoClient, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];



            IEnumerable<InfoClientStep> listTest = table.DataTableToList<InfoClientStep>();

            return listTest;
        }

        public static string GetNextProcess(decimal wORKFLOW_ID, string processTNR)
        {

            string finalRequest = string.Format(requestGetNextProcess, wORKFLOW_ID, processTNR);
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];



            IEnumerable<PNPU_STEP> listTest = table.DataTableToList<PNPU_STEP>();
            if (listTest.Count() == 0)
            {
                return ParamAppli.ProcessFinished;
            }
            else
            {
                return listTest.First().ID_PROCESS;
            }
        }

        public static string GetInfoOneClient(string clientName)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneClient + "'" + clientName + "'", ParamAppli.ConnectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            var regex = new Regex(Regex.Escape("Table"));
            var newJson = regex.Replace(json, "Clients", 1);
            newJson = newJson.Replace("\r\n", "");
            return newJson;
        }


        public static PNPU_PROCESS GetProcess(string processId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneProcess +  processId , ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_PROCESS> listTest = table.DataTableToList<PNPU_PROCESS>();

            return listTest.First();
        }

        public static IEnumerable<PNPU_PROCESS> GetAllProcesses()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllProcess, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_PROCESS> listTest = table.DataTableToList<PNPU_PROCESS>();

            return listTest;
        }

        public static string CreateWorkflow(PNPU_WORKFLOW input)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                string LastInsertedPK = "";
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_WORKFLOW (ID_ORGANIZATION, WORKFLOW_LABEL) VALUES('0000', @WORKFLOW_LABEL)", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_LABEL", SqlDbType.VarChar, 254).Value = input.WORKFLOW_LABEL;
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            LastInsertedPK = DataManagerSQLServer.GetLastInsertedPK("PNPU_WORKFLOW", ParamAppli.ConnectionStringBaseAppli);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    return ex.ToString();
                }
                return LastInsertedPK;
            }
        }

        public static string CreateWorkflowHistoric(PNPU_H_WORKFLOW input)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("insert into PNPU_H_WORKFLOW (ID_ORGANIZATION, CLIENT_ID, WORKFLOW_ID, LAUNCHING_DATE, ENDING_DATE, STATUT_GLOBAL) values ('PNPU', @CLIENT_ID, @WORKFLOW_ID, @LAUNCHING_DATE, null, 'IN PROGRESS')", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = input.WORKFLOW_ID;
                        //cmd.Parameters.Add("@WORKFLOW_LABEL", SqlDbType.VarChar, 254).Value = input.WORKFLOW_LABEL;
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

        public static string CreateStepHistoric(PNPU_H_STEP input)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("insert into PNPU_H_STEP(ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, USER_ID, LAUNCHING_DATE, ENDING_DATE, ID_STATUT, TYPOLOGY) values('1', @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @USER_ID, @LAUNCHING_DATE, null, 'IN PROGRESS', @TYPOLOGY)", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = input.WORKFLOW_ID;
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int).Value = input.ID_PROCESS;
                        cmd.Parameters.Add("@CLIENT_ID", SqlDbType.Int).Value = input.CLIENT_ID;
                        cmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = input.USER_ID;
                        cmd.Parameters.Add("@LAUNCHING_DATE", SqlDbType.Int).Value = input.LAUNCHING_DATE ;
                        cmd.Parameters.Add("@TYPOLOGY", SqlDbType.VarChar, 254).Value = input.TYPOLOGY;
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
            DataSet result = DataManagerSQLServer.GetDatas(requestOneWorkflow + "'" + workflowId + "'", ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest.First();
        }

        public static string ModifyWorkflow(PNPU_WORKFLOW input, string workflowID)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
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

        public static string AffectWorkflowsProcesses(PNPU_STEP input, string workflowID)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_STEP (ID_ORGANIZATION, ORDER_ID, ID_PROCESS, WORKFLOW_ID) VALUES('0000', @ORDER_ID, @ID_PROCESS, @WORKFLOW_ID)", conn))
                    {
                        cmd.Parameters.Add("@ORDER_ID", SqlDbType.Int).Value = input.ID_ORDER;
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.VarChar, 254).Value = input.ID_PROCESS;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.VarChar, 254).Value = input.ID_WORKFLOW;
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

        public static string ModifyProcessus(PNPU_PROCESS input, string processID)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("UPDATE PNPU_PROCESS SET PROCESS_LABEL = @PROCESS_LABEL, IS_LOOPABLE = @IS_LOOPABLE WHERE ID_PROCESS = @ID_PROCESS AND ID_ORGANIZATION = '0000'", conn))
                    {
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int).Value = processID;
                        cmd.Parameters.Add("@PROCESS_LABEL", SqlDbType.VarChar, 254).Value = input.PROCESS_LABEL;
                        cmd.Parameters.Add("@IS_LOOPABLE", SqlDbType.VarChar, 254).Value = input.IS_LOOPABLE;
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

        public static string CreateProcess(PNPU_PROCESS input)
        {
            // string[] sParameters = { "PROCESS_LABEL", "IS_LOOPABLE" };
            string sRequest = "INSERT INTO PNPU_PROCESS (ID_ORGANIZATION, PROCESS_LABEL, IS_LOOPABLE) VALUES('0000', @PROCESS_LABEL, @IS_LOOPABLE)";
            string sTable = "PNPU_PROCESS";
            string result = DataManagerSQLServer.SendTransactionWithGetLastPKid(sRequest, input, sTable);

            return result;
        }

        public static string DeleteProcess(string processID)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM PNPU_PROCESS WHERE ID_PROCESS = @ID_PROCESS AND ID_ORGANIZATION = '0000'", conn))
                    {
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int).Value = processID;
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

        public static string DeleteWorkflow(string workflowID)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM PNPU_WORKFLOW WHERE WORKFLOW_ID = @WORKFLOW_ID AND ID_ORGANIZATION = '0000'", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = workflowID;
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM PNPU_STEP WHERE WORKFLOW_ID = @WORKFLOW_ID", conn))
                    {
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = workflowID;
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
            DataSet result = DataManagerSQLServer.GetDatas(requestAllWorkflow, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest;
        }


        public static IEnumerable<PNPU_STEP> GetAllStep()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllStep, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_STEP> listTest = table.DataTableToList<PNPU_STEP>();

            return listTest;
        }

        public static IEnumerable<PNPU_WORKFLOWPROCESSES> GetWorkflowProcesses(string workflowId)
        {
            string OrderBy = " ORDER BY PS.ORDER_ID";

            DataSet result = DataManagerSQLServer.GetDatas(requestGetWorkflowProcesses + workflowId + OrderBy, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_WORKFLOWPROCESSES> listTest = table.DataTableToList<PNPU_WORKFLOWPROCESSES>();

            return listTest;
        }
    }

}
