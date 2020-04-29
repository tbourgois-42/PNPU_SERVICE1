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

        static string requestGetWorkflowProcesses = "SELECT PP.PROCESS_LABEL, PS.ORDER_ID, PS.ID_PROCESS FROM PNPU_STEP PS, PNPU_PROCESS PP, PNPU_WORKFLOW PW WHERE PS.ID_PROCESS = PP.ID_PROCESS AND PS.WORKFLOW_ID = PW.WORKFLOW_ID AND PS.WORKFLOW_ID = ";
        static string requestHistoricWorkflow = "SELECT PHW.ID_ORGANIZATION, PHW.ID_H_WORKFLOW, PHW.WORKFLOW_ID, PW.WORKFLOW_LABEL, PHW.LAUNCHING_DATE, PHW.ENDING_DATE, PHW.STATUT_GLOBAL FROM PNPU_H_WORKFLOW PHW INNER JOIN PNPU_WORKFLOW PW ON PHW.WORKFLOW_ID = PW.WORKFLOW_ID ORDER BY PHW.WORKFLOW_ID";
        static string requestGetNextProcess = "select * from PNPU_STEP STP, PNPU_PROCESS PRO, (select ORDER_ID + 1 AS NEXT_ORDER from PNPU_STEP STEP2, PNPU_PROCESS PRO2 where STEP2.ID_PROCESS = PRO2.ID_PROCESS AND STEP2.WORKFLOW_ID = {0} AND PRO2.ID_PROCESS = '{1}') AS STEPN where STP.ORDER_ID = STEPN.NEXT_ORDER AND STP.WORKFLOW_ID = {0} AND STP.ID_PROCESS = PRO.ID_PROCESS";
        private static string requestOneWorkflowHistoric = "select * from PNPU_H_WORKFLOW where WORKFLOW_ID = ";
        private static string requestGetStepHistoric = "select * from PNPU_H_STEP where WORKFLOW_ID = {0} AND CLIENT_ID = '{1}' AND ID_PROCESS = '{2}' AND ITERATION = {3}";

        public static IEnumerable<PNPU_H_WORKFLOW> GetHWorkflow()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestHistoricWorkflow, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_H_WORKFLOW> listTest = table.DataTableToList<PNPU_H_WORKFLOW>();

            return listTest;
        }

        public static IEnumerable<InfoClientStep> GetAllInfoClient(decimal WORKFLOW_ID)
        {
            // Par défault on charge sur le dashboard le dernier Workflow en cours
            string defaultWorkflowID = "(SELECT TOP(1) PHW.WORKFLOW_ID FROM PNPU_H_WORKFLOW PHW WHERE PHW.ENDING_DATE = '1800/01/01' ORDER BY ENDING_DATE)";

            string filtre = (WORKFLOW_ID == 0) ? defaultWorkflowID : WORKFLOW_ID.ToString();

            string request = "SELECT PHS.ID_ORGANIZATION, PHS.ITERATION, PHS.WORKFLOW_ID, PHS.LAUNCHING_DATE, PHS.ENDING_DATE, PHS.ID_STATUT, PHS.CLIENT_ID, PHS.TYPOLOGY, PS.ORDER_ID, ";
            request += "PS.ID_PROCESS / (SELECT MAX(PS.ID_PROCESS) AS NB_PROCESS FROM PNPU_WORKFLOW PW INNER JOIN PNPU_STEP PS ON PW.WORKFLOW_ID = PS.WORKFLOW_ID WHERE PW.WORKFLOW_ID = " + filtre + " GROUP BY PS.WORKFLOW_ID) *100 AS PERCENTAGE_COMPLETUDE " ;
            request += "FROM PNPU_H_STEP PHS, PNPU_STEP PS, PNPU_STEP PS2 WHERE PHS.LAUNCHING_DATE = (SELECT MAX(PHS2.LAUNCHING_DATE) FROM PNPU_H_STEP PHS2 WHERE PHS.ID_ORGANIZATION = PHS2.ID_ORGANIZATION AND PHS.WORKFLOW_ID = PHS2.WORKFLOW_ID AND PHS.CLIENT_ID = PHS2.CLIENT_ID) AND PHS.WORKFLOW_ID = " + filtre + " ";
            request += "AND PS.ID_ORGANIZATION = PS2.ID_ORGANIZATION AND PS.ORDER_ID = PS2.ORDER_ID AND PS.ID_PROCESS = PS2.ID_PROCESS AND PS.WORKFLOW_ID = PS2.WORKFLOW_ID AND PS.WORKFLOW_ID = PHS.WORKFLOW_ID AND PS.ID_PROCESS = PHS.ID_PROCESS ";
            request += "GROUP BY PHS.ID_ORGANIZATION, PHS.ITERATION, PHS.WORKFLOW_ID, PHS.LAUNCHING_DATE, PHS.ENDING_DATE, PHS.ID_STATUT, PHS.CLIENT_ID, PHS.TYPOLOGY, PHS.ID_PROCESS, PS.ID_PROCESS, PS.ORDER_ID ORDER BY PHS.CLIENT_ID";
            
            DataSet result = DataManagerSQLServer.GetDatas(request, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<InfoClientStep> listTest = table.DataTableToList<InfoClientStep>();

            return listTest;
        }

        public static int GetNextProcess(decimal wORKFLOW_ID, int processId)
        {

            string finalRequest = string.Format(requestGetNextProcess, wORKFLOW_ID, processId);
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

        public static IEnumerable<PNPU_H_REPORT> getReport(decimal idProcess, decimal workflowId, string clientId)
        {
            String sRequest = "SELECT ID_ORGANIZATION, ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, JSON_TEMPLATE FROM PNPU_H_REPORT ";
            sRequest += "WHERE ID_PROCESS = @ID_PROCESS AND WORKFLOW_ID = @WORKFLOW_ID AND CLIENT_ID = @CLIENT_ID";

            DataSet result = DataManagerSQLServer.GetDatasWithParams(sRequest, ParamAppli.ConnectionStringBaseAppli, idProcess, workflowId, clientId);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_H_REPORT> listTest = table.DataTableToList<PNPU_H_REPORT>();

            return listTest;

        }

        public static string CreateUpdateWorkflowHistoric(PNPU_H_WORKFLOW input)
        {
            int workFlowId = Decimal.ToInt32(input.WORKFLOW_ID);
            if (historicWorkflowExist(workFlowId))
            {
                //Update for the moment do nothing
                return "Requête traitée avec succès et création/mis à jour d'un historique de workflow";
            }
            else
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                {
                    try
                    {

                        conn.Open();

                        using (var cmd = new System.Data.SqlClient.SqlCommand("insert into PNPU_H_WORKFLOW (ID_ORGANIZATION, CLIENT_ID, WORKFLOW_ID, LAUNCHING_DATE, ENDING_DATE, STATUT_GLOBAL) values ('9999', @CLIENT_ID, @WORKFLOW_ID, @LAUNCHING_DATE, @ENDING_DATE, @STATUT)", conn))
                        {
                            cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = input.WORKFLOW_ID;
                            cmd.Parameters.Add("@CLIENT_ID", SqlDbType.VarChar, 254).Value = input.CLIENT_ID;
                            cmd.Parameters.Add("@LAUNCHING_DATE", SqlDbType.DateTime).Value = input.LAUNCHING_DATE;
                            cmd.Parameters.Add("@STATUT", SqlDbType.VarChar, 254).Value = input.STATUT_GLOBAL;
                            cmd.Parameters.Add("@ENDING_DATE", SqlDbType.DateTime).Value = input.ENDING_DATE;

                            int rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        return ex.ToString();
                    }
                    return "Requête traitée avec succès et création/mis à jour d'un historique de workflow";
                }
            }
        }

        public static string CreateUpdateStepHistoric(PNPU_H_STEP input)
        {
            if (historicStepExist(input))
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                {
                    try
                    {

                        conn.Open();
                        using (var cmd = new System.Data.SqlClient.SqlCommand("update PNPU_H_STEP set ENDING_DATE = @ENDING_DATE  ID_STATUT = @ID_STATUT where ITERATION = @ITERATION AND WORKFLOW_ID = @WORKFLOW_ID AND ID_PROCESS = @ID_PROCESS AND CLIENT_ID = @CLIENT_ID", conn))
                        {
                            cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = input.ITERATION;
                            cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = input.WORKFLOW_ID;
                            cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int).Value = input.ID_PROCESS;
                            cmd.Parameters.Add("@CLIENT_ID", SqlDbType.Int).Value = input.CLIENT_ID;
                            cmd.Parameters.Add("@ID_STATUT", SqlDbType.VarChar, 254).Value = input.ID_STATUT;
                            cmd.Parameters.Add("@ENDING_DATE", SqlDbType.VarChar, 254).Value = input.ENDING_DATE;

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
            else
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                {
                    try
                    {

                        conn.Open();
                        using (var cmd = new System.Data.SqlClient.SqlCommand("insert into PNPU_H_STEP(ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, USER_ID, LAUNCHING_DATE, ENDING_DATE, ID_STATUT, TYPOLOGY) values('1', @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @USER_ID, @LAUNCHING_DATE, null, 'IN PROGRESS', @TYPOLOGY)", conn))
                        {
                            cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = input.ITERATION;
                            cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = input.WORKFLOW_ID;
                            cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int).Value = input.ID_PROCESS;
                            cmd.Parameters.Add("@CLIENT_ID", SqlDbType.Int).Value = input.CLIENT_ID;
                            cmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = input.USER_ID;
                            cmd.Parameters.Add("@LAUNCHING_DATE", SqlDbType.Int).Value = input.LAUNCHING_DATE;
                            cmd.Parameters.Add("@TYPOLOGY", SqlDbType.VarChar, 254).Value = input.TYPOLOGY;
                            cmd.Parameters.Add("@ID_STATUT", SqlDbType.VarChar, 254).Value = input.ID_STATUT;

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
        }

        public static PNPU_WORKFLOW getWorkflow(string workflowId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneWorkflow + "'" + workflowId + "'", ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest.First();
        }

        public static PNPU_H_WORKFLOW getWorkflowHistoric(int workflowId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneWorkflowHistoric + "'" + workflowId + "'", ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_H_WORKFLOW> listTest = table.DataTableToList<PNPU_H_WORKFLOW>();
            if (listTest.Count() >= 1)
                return listTest.First();
            else
                return null;
        }

        public static bool historicWorkflowExist(int workflowId)
        {
            return getWorkflowHistoric(workflowId) != null;
        }

        public static PNPU_H_STEP getStepHistoric(PNPU_H_STEP step)
        {
            DataSet result = DataManagerSQLServer.GetDatas(string.Format(requestGetStepHistoric, step.WORKFLOW_ID, step.CLIENT_ID, step.ID_PROCESS, step.ITERATION), ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_H_STEP> listTest = table.DataTableToList<PNPU_H_STEP>();
            if (listTest.Count() >= 1)
                return listTest.First();
            else
                return null;
        }

        public static bool historicStepExist(PNPU_H_STEP step)
        {
            return getStepHistoric(step) != null;
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
