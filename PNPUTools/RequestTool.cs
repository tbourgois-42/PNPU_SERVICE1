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

        static string requestAllWorkflow = "SELECT PW.WORKFLOW_ID, PW.WORKFLOW_LABEL , COUNT(PS.ID_PROCESS) AS NB_PROCESS FROM PNPU_WORKFLOW PW LEFT JOIN PNPU_STEP PS ON PS.WORKFLOW_ID = PW.WORKFLOW_ID GROUP BY  PW.WORKFLOW_ID, PW.WORKFLOW_LABEL";
        static string requestOneWorkflow = "select * from PNPU_WORKFLOW where WORKFLOW_ID = ";

        static string requestGetWorkflowProcesses = "SELECT PP.PROCESS_LABEL, PS.ORDER_ID, PS.ID_PROCESS FROM PNPU_STEP PS, PNPU_PROCESS PP, PNPU_WORKFLOW PW WHERE PS.ID_PROCESS = PP.ID_PROCESS AND PS.WORKFLOW_ID = PW.WORKFLOW_ID AND PS.WORKFLOW_ID = ";
        static string requestHistoricWorkflow = "SELECT PHW.ID_H_WORKFLOW, PHW.WORKFLOW_ID, PW.WORKFLOW_LABEL, PHW.LAUNCHING_DATE, PHW.ENDING_DATE, PHW.STATUT_GLOBAL FROM PNPU_H_WORKFLOW PHW INNER JOIN PNPU_WORKFLOW PW ON PHW.WORKFLOW_ID = PW.WORKFLOW_ID ORDER BY PHW.WORKFLOW_ID";
        static string requestGetNextProcess = "select * from PNPU_STEP STP, PNPU_PROCESS PRO, (select ORDER_ID + 1 AS NEXT_ORDER from PNPU_STEP STEP2, PNPU_PROCESS PRO2 where STEP2.ID_PROCESS = PRO2.ID_PROCESS AND STEP2.WORKFLOW_ID = {0} AND PRO2.ID_PROCESS = '{1}') AS STEPN where STP.ORDER_ID = STEPN.NEXT_ORDER AND STP.WORKFLOW_ID = {0} AND STP.ID_PROCESS = PRO.ID_PROCESS";

        static string requestListClient = "select CLI.CLIENT_ID as ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS as TYPOLOGY_ID, COD.CODIFICATION_LIBELLE as TYPOLOGY from A_CLIENT CLI, A_CODIFICATION COD  where COD.CODIFICATION_ID = CLI.SAAS AND CLI.SAAS = '{0}'";
        static string requestListClientAll = "select CLI.CLIENT_ID as ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS as TYPOLOGY_ID, COD.CODIFICATION_LIBELLE as TYPOLOGY from A_CLIENT CLI, A_CODIFICATION COD  where COD.CODIFICATION_ID = CLI.SAAS";
        static string requestClientById = "select CLI.CLIENT_ID as ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS as TYPOLOGY_ID, COD.CODIFICATION_LIBELLE as TYPOLOGY from A_CLIENT CLI, A_CODIFICATION COD  where COD.CODIFICATION_ID = CLI.SAAS AND CLI.CLIENT_ID = '{0}'";

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
            string defaultWorkflowID = "(SELECT TOP(1) PHW.WORKFLOW_ID FROM PNPU_H_WORKFLOW PHW, PNPU_H_WORKFLOW PHW2 WHERE PHW.ID_H_WORKFLOW = PHW2.ID_H_WORKFLOW AND PHW.LAUNCHING_DATE = (SELECT MAX(PHW2.LAUNCHING_DATE) FROM PNPU_H_WORKFLOW PHW2))";

            string filtre = (WORKFLOW_ID == 0) ? defaultWorkflowID : WORKFLOW_ID.ToString();

            string request = "SELECT PHS.ITERATION, PHS.WORKFLOW_ID, PHS.LAUNCHING_DATE, PHS.ENDING_DATE, PHS.ID_STATUT, PHS.CLIENT_ID, PHS.CLIENT_NAME, PHS.TYPOLOGY, PS.ORDER_ID, ";
            request += "(PS.ORDER_ID)  / (SELECT (MAX(PS.ORDER_ID) ) AS NB_PROCESS FROM PNPU_WORKFLOW PW INNER JOIN PNPU_STEP PS ON PW.WORKFLOW_ID = PS.WORKFLOW_ID WHERE PW.WORKFLOW_ID = " + filtre + " GROUP BY PS.WORKFLOW_ID) *100 AS PERCENTAGE_COMPLETUDE " ;
            request += "FROM PNPU_H_STEP PHS, PNPU_STEP PS, PNPU_STEP PS2 WHERE PHS.LAUNCHING_DATE = (SELECT MAX(PHS2.LAUNCHING_DATE) FROM PNPU_H_STEP PHS2 WHERE PHS.WORKFLOW_ID = PHS2.WORKFLOW_ID AND PHS.CLIENT_ID = PHS2.CLIENT_ID) AND PHS.WORKFLOW_ID = " + filtre + " ";
            request += " AND PS.ORDER_ID = PS2.ORDER_ID AND PS.ID_PROCESS = PS2.ID_PROCESS AND PS.WORKFLOW_ID = PS2.WORKFLOW_ID AND PS.WORKFLOW_ID = PHS.WORKFLOW_ID AND PS.ID_PROCESS = PHS.ID_PROCESS ";
            request += "GROUP BY  PHS.ITERATION, PHS.WORKFLOW_ID, PHS.LAUNCHING_DATE, PHS.ENDING_DATE, PHS.ID_STATUT, PHS.CLIENT_ID, PHS.CLIENT_NAME, PHS.TYPOLOGY, PHS.ID_PROCESS, PS.ID_PROCESS, PS.ORDER_ID ORDER BY PHS.ID_STATUT, PHS.ID_PROCESS, PHS.CLIENT_NAME";
            
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

        public static InfoClient getClientsById(string idClient)
        {

            string finalRequest = string.Format(requestClientById, idClient);
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.connectionStringSupport);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClient> listTest = table.DataTableToList<InfoClient>();

            return listTest.First();
        }

        public static IEnumerable<InfoClient> getClientsWithTypologies(int typology)
        {

            string finalRequest = string.Format(requestListClient, typology);
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.connectionStringSupport);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClient> listTest = table.DataTableToList<InfoClient>();

            return listTest;
        }

        public static IEnumerable<InfoClient> getClientsWithTypologies()
        {
            string finalRequest = requestListClientAll;
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.connectionStringSupport);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClient> listTest = table.DataTableToList<InfoClient>();

            return listTest;
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
            string[] requests = { "INSERT INTO PNPU_WORKFLOW ( WORKFLOW_LABEL) VALUES( @WORKFLOW_LABEL)" };
            string[] parameters = new string[] { "@WORKFLOW_LABEL", input.WORKFLOW_LABEL };
            
            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_WORKFLOW", parameters, true);
        }

        public static string GetMaxStep(int workflowId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                string MaxStep = "";
                try
                {
                    conn.Open();
                    SqlCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT COUNT(ID_PROCESS) - 1 FROM PNPU_STEP WHERE WORKFLOW_ID = " + workflowId;
                    MaxStep = command.ExecuteScalar().ToString();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Impossible de récupérer le nombre de processus du workflow " + workflowId);
                    return ex.ToString();
                }
                return MaxStep;
            }
        }

        public static IEnumerable<PNPU_H_REPORT> getReport(decimal idProcess, decimal workflowId, string clientId)
        {
            String sRequest = "SELECT PHR.ITERATION, PHR.WORKFLOW_ID, PHR.ID_PROCESS, PHR.CLIENT_ID, PHR.JSON_TEMPLATE FROM PNPU_H_REPORT PHR, PNPU_PROCESS PR ";
            sRequest += " WHERE PHR.ID_PROCESS = @ID_PROCESS AND PHR.WORKFLOW_ID = @WORKFLOW_ID AND PHR.ID_PROCESS =  PR.ID_PROCESS  AND ";
            sRequest += " ( PHR.CLIENT_ID = @CLIENT_ID OR PR.IS_GLOBAL = '1') ";

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
                string[] requests = { "INSERT INTO PNPU_H_WORKFLOW ( CLIENT_ID, WORKFLOW_ID, LAUNCHING_DATE, ENDING_DATE, STATUT_GLOBAL) VALUES (@CLIENT_ID, @WORKFLOW_ID, @LAUNCHING_DATE, @ENDING_DATE, @STATUT)" };
                string[] parameters = new string[] { "@CLIENT_ID", input.CLIENT_ID, "@WORKFLOW_ID", input.WORKFLOW_ID.ToString(), "@LAUNCHING_DATE", input.LAUNCHING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@ENDING_DATE", input.ENDING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@STATUT", input.STATUT_GLOBAL };

                return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_WORKFLOW", parameters, true);
            }
        }

        public static string CreateUpdateStepHistoric(PNPU_H_STEP input)
        {
            if (historicStepExist(input))
            {
                string[] requests = { "UPDATE PNPU_H_STEP SET ENDING_DATE = @ENDING_DATE, CLIENT_NAME = @CLIENT_NAME, ID_STATUT = @ID_STATUT WHERE ITERATION = @ITERATION AND WORKFLOW_ID = @WORKFLOW_ID AND ID_PROCESS = @ID_PROCESS AND CLIENT_ID = @CLIENT_ID" };
                string[] parameters = new string[] { "@ITERATION", input.ITERATION.ToString(), "@WORKFLOW_ID", input.WORKFLOW_ID.ToString(), "@ID_PROCESS", input.ID_PROCESS.ToString(), "@CLIENT_ID", input.CLIENT_ID, "@CLIENT_NAME", input.CLIENT_NAME, "@ID_STATUT", input.ID_STATUT, "@ENDING_DATE", input.ENDING_DATE.ToString("MM/dd/yyyy HH:mm:ss") };

                return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_STEP", parameters, true);
            }
            else
            {
                string[] requests = { "INSERT INTO PNPU_H_STEP (ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, CLIENT_NAME, USER_ID, LAUNCHING_DATE, ENDING_DATE, ID_STATUT, TYPOLOGY) VALUES (@ITERATION, @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @CLIENT_NAME, @USER_ID, @LAUNCHING_DATE, @ENDING_DATE, @ID_STATUT, @TYPOLOGY)" };
                string[] parameters = new string[] { "@ITERATION", input.ITERATION.ToString(), "@WORKFLOW_ID", input.WORKFLOW_ID.ToString(), "@ID_PROCESS", input.ID_PROCESS.ToString(), "@CLIENT_ID", input.CLIENT_ID, "@CLIENT_NAME", input.CLIENT_NAME, "@USER_ID", input.USER_ID, "@LAUNCHING_DATE", input.LAUNCHING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@ENDING_DATE", input.ENDING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@TYPOLOGY", input.TYPOLOGY, "@ID_STATUT", input.ID_STATUT };

                return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_STEP", parameters, true);

               
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
            string[] requests = { "UPDATE PNPU_WORKFLOW SET WORKFLOW_LABEL = @WORKFLOW_LABEL WHERE WORKFLOW_ID = @WORKFLOW_ID " };
            string[] parameters = new string[] { "@WORKFLOW_ID", workflowID, "@WORKFLOW_LABEL", input.WORKFLOW_LABEL };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_WORKFLOW", parameters, false);
           
        }

        public static string AffectWorkflowsProcesses(PNPU_STEP input, string workflowID)
        {
            string[] requests = { "INSERT INTO PNPU_STEP ( ORDER_ID, ID_PROCESS, WORKFLOW_ID) VALUES( @ORDER_ID, @ID_PROCESS, @WORKFLOW_ID)" };
            string[] parameters = new string[] { "@ORDER_ID", input.ID_ORDER.ToString(), "@ID_PROCESS", input.ID_PROCESS.ToString(), "@WORKFLOW_ID", input.ID_WORKFLOW };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_STEP", parameters, false);

           
        }

        public static string ModifyProcessus(PNPU_PROCESS input, string processID)
        {
            string[] requests = { "UPDATE PNPU_PROCESS SET PROCESS_LABEL = @PROCESS_LABEL, IS_LOOPABLE = @IS_LOOPABLE WHERE ID_PROCESS = @ID_PROCESS" };
            string[] parameters = new string[] { "@ID_PROCESS", processID, "@PROCESS_LABEL", input.PROCESS_LABEL, "@IS_LOOPABLE", input.IS_LOOPABLE };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_PROCESS", parameters, false);
           
        }

        public static string CreateProcess(PNPU_PROCESS input)
        {
            string[] requests = { "INSERT INTO PNPU_PROCESS ( PROCESS_LABEL, IS_LOOPABLE) VALUES( @PROCESS_LABEL, @IS_LOOPABLE)" };
            string[] parameters = new string[] { "@PROCESS_LABEL", input.PROCESS_LABEL, "@IS_LOOPABLE", input.IS_LOOPABLE };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_PROCESS", parameters, true);

           
        }

        public static string DeleteProcess(string processID)
        {
            string[] requests = { "DELETE FROM PNPU_PROCESS WHERE ID_PROCESS = @ID_PROCESS" };
            string[] parameters = new string[] { "@ID_PROCESS", processID };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_PROCESS", parameters, false);

            
        }

        public static string DeleteWorkflow(string workflowID)
        {
            string[] requests = { "DELETE FROM PNPU_WORKFLOW WHERE WORKFLOW_ID = @WORKFLOW_ID " };
            string[] parameters = new string[] { "@WORKFLOW_ID", workflowID };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_WORKFLOW", parameters, false);

            
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
