﻿using Newtonsoft.Json;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        
        static string requestAllProcess = "select * from PNPU_PROCESS";
        static string requestOneProcess = "select * from PNPU_PROCESS where ID_PROCESS = ";

        static string requestAllWorkflow = "SELECT PW.WORKFLOW_ID, PW.WORKFLOW_LABEL , COUNT(PS.ID_PROCESS) AS NB_PROCESS FROM PNPU_WORKFLOW PW LEFT JOIN PNPU_STEP PS ON PS.WORKFLOW_ID = PW.WORKFLOW_ID GROUP BY  PW.WORKFLOW_ID, PW.WORKFLOW_LABEL";
        static string requestOneWorkflow = "select * from PNPU_WORKFLOW where WORKFLOW_ID = ";

        static string requestGetWorkflowProcesses = "SELECT PP.PROCESS_LABEL, PS.ORDER_ID, PS.ID_PROCESS FROM PNPU_STEP PS, PNPU_PROCESS PP, PNPU_WORKFLOW PW WHERE PS.ID_PROCESS = PP.ID_PROCESS AND PS.WORKFLOW_ID = PW.WORKFLOW_ID AND PS.WORKFLOW_ID = ";
        static string requestHistoricWorkflow = "SELECT PHW.ID_H_WORKFLOW, PHW.WORKFLOW_ID, PW.WORKFLOW_LABEL, PHW.LAUNCHING_DATE, PHW.ENDING_DATE, PHW.STATUT_GLOBAL, PHW.INSTANCE_NAME FROM PNPU_H_WORKFLOW PHW INNER JOIN PNPU_WORKFLOW PW ON PHW.WORKFLOW_ID = PW.WORKFLOW_ID ORDER BY PHW.LAUNCHING_DATE";
        static string requestGetNextProcess = "select * from PNPU_STEP STP, PNPU_PROCESS PRO, (select ORDER_ID + 1 AS NEXT_ORDER from PNPU_STEP STEP2, PNPU_PROCESS PRO2 where STEP2.ID_PROCESS = PRO2.ID_PROCESS AND STEP2.WORKFLOW_ID = {0} AND PRO2.ID_PROCESS = '{1}') AS STEPN where STP.ORDER_ID = STEPN.NEXT_ORDER AND STP.WORKFLOW_ID = {0} AND STP.ID_PROCESS = PRO.ID_PROCESS";

        static string requestListClient = "select CLI.CLIENT_ID as ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS as TYPOLOGY_ID, COD.CODIFICATION_LIBELLE as TYPOLOGY from A_CLIENT CLI, A_CODIFICATION COD  where COD.CODIFICATION_ID = CLI.SAAS AND CLI.SAAS = '{0}'";
        static string requestListClientAll = "select CLI.CLIENT_ID as ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS as TYPOLOGY_ID, COD.CODIFICATION_LIBELLE as TYPOLOGY from A_CLIENT CLI, A_CODIFICATION COD  where COD.CODIFICATION_ID = CLI.SAAS";
        static string requestClientById = "select CLI.CLIENT_ID as ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS as TYPOLOGY_ID, COD.CODIFICATION_LIBELLE as TYPOLOGY from A_CLIENT CLI, A_CODIFICATION COD  where COD.CODIFICATION_ID = CLI.SAAS AND CLI.CLIENT_ID = '{0}'";

        private static string requestOneWorkflowHistoric = "select * from PNPU_H_WORKFLOW where WORKFLOW_ID = {0} AND ID_H_WORKFLOW = {1}";
        
        private static string requestGetStepHistoric = "SELECT COUNT(*) FROM PNPU_H_STEP WHERE WORKFLOW_ID = {0} AND CLIENT_ID = '{1}' AND ID_PROCESS = '{2}' AND ITERATION = {3} AND ID_H_WORKFLOW = {4}";
        private static string requestGetWorkflowHistoric = "SELECT COUNT(*) FROM PNPU_H_WORKFLOW WHERE WORKFLOW_ID = {0} AND ID_H_WORKFLOW = {1}";
        
        /// <summary>
        /// Get workflow hitoric
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PNPU_H_WORKFLOW> GetHWorkflow()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestHistoricWorkflow, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_H_WORKFLOW> listTest = table.DataTableToList<PNPU_H_WORKFLOW>();

            return listTest;
        }

        /// <summary>
        /// Get all information to be displayed on the Dashboard
        /// </summary>
        /// <param name="WORKFLOW_ID"></param>
        /// <param name="ID_H_WORKFLOW"></param>
        /// <returns></returns>
        public static IEnumerable<InfoClientStep> GetAllInfoClient(decimal WORKFLOW_ID, int ID_H_WORKFLOW, string sHabilitation, string sUser)
        {
            // Par défault on charge sur le dashboard la dernière instance de workflow lancée
            string defaultWorkflowID = "(SELECT TOP(1) PHW.WORKFLOW_ID FROM PNPU_H_WORKFLOW PHW, PNPU_H_WORKFLOW PHW2 WHERE PHW.ID_H_WORKFLOW = PHW2.ID_H_WORKFLOW AND PHW.WORKFLOW_ID = PHW2.WORKFLOW_ID AND PHW.LAUNCHING_DATE = (SELECT MAX(PHW2.LAUNCHING_DATE) FROM PNPU_H_WORKFLOW PHW2))";

            string sWhereHabilitation = Authentification.GetHabilitationWhereClause(sHabilitation, sUser, "PHS");

            string filtre = (WORKFLOW_ID == 0) ? defaultWorkflowID : WORKFLOW_ID.ToString();

            string sSelect = "SELECT PHS.ITERATION, PHS.WORKFLOW_ID, PHS.ID_H_WORKFLOW, MAX(PHS.LAUNCHING_DATE) AS LAUNCHING_DATE, PHS.ENDING_DATE, PHS.ID_STATUT, PHS.CLIENT_ID, PHS.CLIENT_NAME, PHS.TYPOLOGY, PS.ORDER_ID, PS.ID_PROCESS,  PS.ID_PROCESS / (SELECT MAX(PS.ID_PROCESS) AS NB_PROCESS FROM PNPU_WORKFLOW PW INNER JOIN PNPU_STEP PS ON PW.WORKFLOW_ID = PS.WORKFLOW_ID WHERE PW.WORKFLOW_ID = " + filtre + " GROUP BY PS.WORKFLOW_ID) *100 AS PERCENTAGE_COMPLETUDE ";
            string sFrom = "FROM PNPU_H_STEP PHS, PNPU_STEP PS ";
            string sWhere = "WHERE PHS.WORKFLOW_ID = " + filtre + " AND PHS.ID_H_WORKFLOW = " + ID_H_WORKFLOW + " AND (PHS.ENDING_DATE = {d'1800-01-01'} OR (PS.ID_PROCESS / (SELECT MAX(PS.ID_PROCESS) AS NB_PROCESS FROM PNPU_WORKFLOW PW INNER JOIN PNPU_STEP PS ON PW.WORKFLOW_ID = PS.WORKFLOW_ID WHERE PW.WORKFLOW_ID = 28 GROUP BY PS.WORKFLOW_ID) *100) = '100') AND PS.ID_PROCESS = PHS.ID_PROCESS AND PS.WORKFLOW_ID = PHS.WORKFLOW_ID ";

            sWhere += sWhereHabilitation;

            string sGroupBy = " GROUP BY PHS.CLIENT_ID, PHS.CLIENT_NAME, PHS.TYPOLOGY, PHS.ID_PROCESS, PHS.ITERATION, PHS.WORKFLOW_ID, PHS.ID_H_WORKFLOW, PHS.ID_STATUT, PHS.ENDING_DATE, PS.ORDER_ID, PS.ID_PROCESS ";
            string sOrderBy = "ORDER BY PHS.ID_PROCESS DESC";

            string sRequest = sSelect + sFrom + sWhere + sGroupBy + sOrderBy;

            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<InfoClientStep> listTest = table.DataTableToList<InfoClientStep>();

            return listTest;
        }

        /// <summary>
        /// Get next process to be executed
        /// </summary>
        /// <param name="wORKFLOW_ID"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get client info from support database
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public static string GetInfoOneClient(string clientName)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneClient + "'" + clientName + "'", ParamAppli.ConnectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            var regex = new Regex(Regex.Escape("Table"));
            var newJson = regex.Replace(json, "Clients", 1);
            newJson = newJson.Replace("\r\n", "");
            return newJson;
        }

        /// <summary>
        /// Get client by ID from support database
        /// </summary>
        /// <param name="idClient"></param>
        /// <returns></returns>
        public static InfoClient getClientsById(string idClient)
        {

            string finalRequest = string.Format(requestClientById, idClient);
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.connectionStringSupport);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClient> listTest = table.DataTableToList<InfoClient>();

            return listTest.First();
        }

        /// <summary>
        /// Get client typology from support database for a particular typology
        /// </summary>
        /// <param name="typology"></param>
        /// <returns></returns>
        public static IEnumerable<InfoClient> getClientsWithTypologies(int typology)
        {

            string finalRequest = string.Format(requestListClient, typology);
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.connectionStringSupport);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClient> listTest = table.DataTableToList<InfoClient>();

            return listTest;
        }

        /// <summary>
        /// Get client typology from support database
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InfoClient> getClientsWithTypologies()
        {
            string finalRequest = requestListClientAll;
            DataSet result = DataManagerSQLServer.GetDatas(finalRequest, ParamAppli.connectionStringSupport);
            DataTable table = result.Tables[0];


            IEnumerable<InfoClient> listTest = table.DataTableToList<InfoClient>();

            return listTest;
        }

        /// <summary>
        /// Get one process from PNPU_PROCESS
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static PNPU_PROCESS GetProcess(string processId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneProcess + processId, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_PROCESS> listTest = table.DataTableToList<PNPU_PROCESS>();

            return listTest.First();
        }

        /// <summary>
        /// Get all process from PNPU_PROCESS
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PNPU_PROCESS> GetAllProcesses()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllProcess, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_PROCESS> listTest = table.DataTableToList<PNPU_PROCESS>();

            return listTest;
        }

        public static List<byte[]> GetMdbLivraison(string workflowId, string idInstanceWF, string clientId)
        {
            string sRequest = "SELECT MDB FROM PNPU_MDB WHERE WORKFLOW_ID = " + workflowId + " AND ID_H_WORKFLOW = " + idInstanceWF + " AND CLIENT_ID = " + clientId;
            List<byte[]> mdb = DataManagerSQLServer.ReadBinaryDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);
            return mdb;
        }

        /// <summary>
        /// Get number of localisation element
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="idInstanceWF"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static string GetNbLocalisation(int workflowId, int idInstanceWF, int clientId)
        {
            string sRequest = "SELECT COUNT(*) FROM PNPU_H_LOCALISATION WHERE WORKFLOW_ID = " + workflowId + " AND CLIENT_ID = " + clientId + " AND ID_H_WORKFLOW = " + idInstanceWF;
            string nbLocalisation = DataManagerSQLServer.SelectCount(sRequest, ParamAppli.ConnectionStringBaseAppli);
            
            return nbLocalisation;

        }

        /// <summary>
        /// Allow to create a workflow
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CreateWorkflow(PNPU_WORKFLOW input)
        {
            string[] requests = { "INSERT INTO PNPU_WORKFLOW ( WORKFLOW_LABEL) VALUES( @WORKFLOW_LABEL)" };
            string[] parameters = new string[] { "@WORKFLOW_LABEL", input.WORKFLOW_LABEL };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_WORKFLOW", parameters, true);
        }

        /// <summary>
        /// Get number of step for a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns>Number of step</returns>
        public static string GetMaxStep(int workflowId)
        {
            string sRequest = "SELECT COUNT(ID_PROCESS) - 1 FROM PNPU_STEP WHERE WORKFLOW_ID = " + workflowId;
            string MaxStep = DataManagerSQLServer.SelectCount(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return MaxStep;
        }

        /// <summary>
        /// Get process report
        /// </summary>
        /// <param name="idProcess"></param>
        /// <param name="workflowId"></param>
        /// <param name="clientId"></param>
        /// <param name="idInstanceWF"></param>
        /// <returns>Return report</returns>
        public static IEnumerable<PNPU_H_REPORT> getReport(decimal idProcess, decimal workflowId, string clientId, int idInstanceWF)
        {
            String sRequest = "SELECT PHR.ITERATION, PHR.WORKFLOW_ID, PHR.ID_PROCESS, PHR.CLIENT_ID, PHR.JSON_TEMPLATE, PHR.ID_H_WORKFLOW";
            sRequest += " FROM PNPU_H_REPORT PHR, PNPU_PROCESS PR ";
            sRequest += " WHERE PHR.ID_PROCESS = @ID_PROCESS AND PHR.WORKFLOW_ID = @WORKFLOW_ID AND PHR.ID_PROCESS =  PR.ID_PROCESS  AND ";
            sRequest += " PHR.ID_H_WORKFLOW = @ID_H_WORKFLOW AND ";
            sRequest += " ( PHR.CLIENT_ID = @CLIENT_ID OR PR.IS_GLOBAL = '1') ";

            DataSet result = DataManagerSQLServer.GetDatasWithParams(sRequest, ParamAppli.ConnectionStringBaseAppli, idProcess, workflowId, clientId, idInstanceWF);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_H_REPORT> listTest = table.DataTableToList<PNPU_H_REPORT>();

            return listTest;

        }

        /// <summary>
        /// Allow to create a PNPU_H_WORKFLOW historic line,or Update. It depends on the existing value 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CreateUpdateWorkflowHistoric(PNPU_H_WORKFLOW input)
        {
            int workFlowId = Decimal.ToInt32(input.WORKFLOW_ID);
            int idInstanceWF = Decimal.ToInt32(input.ID_H_WORKFLOW);

            if (historicWorkflowExist(workFlowId, idInstanceWF))
            {
                //If exist return the id instance
                return input.ID_H_WORKFLOW.ToString();
            }
            else
            {
                string[] requests = { "INSERT INTO PNPU_H_WORKFLOW ( CLIENT_ID, WORKFLOW_ID, LAUNCHING_DATE, ENDING_DATE, STATUT_GLOBAL, INSTANCE_NAME) VALUES (@CLIENT_ID, @WORKFLOW_ID, @LAUNCHING_DATE, @ENDING_DATE, @STATUT, @INSTANCE_NAME)" };
                string[] parameters = new string[] { "@CLIENT_ID", input.CLIENT_ID, "@WORKFLOW_ID", input.WORKFLOW_ID.ToString(), "@LAUNCHING_DATE", input.LAUNCHING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@ENDING_DATE", input.ENDING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@STATUT", input.STATUT_GLOBAL, "@INSTANCE_NAME", input.INSTANCE_NAME };

                return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_WORKFLOW", parameters, true);
            }
        }

        /// <summary>
        /// Allow to Create a PNPU_H_STEP historic line, or Update. It depends on the existing value
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CreateUpdateStepHistoric(PNPU_H_STEP input)
        {
            if (historicStepExist(input))
            {
                string[] requests = { "UPDATE PNPU_H_STEP SET ENDING_DATE = @ENDING_DATE, CLIENT_NAME = @CLIENT_NAME, ID_STATUT = @ID_STATUT WHERE ITERATION = @ITERATION AND WORKFLOW_ID = @WORKFLOW_ID AND ID_PROCESS = @ID_PROCESS AND CLIENT_ID = @CLIENT_ID AND ID_H_WORKFLOW = @ID_H_WORKFLOW" };
                string[] parameters = new string[] { "@ITERATION", input.ITERATION.ToString(), "@WORKFLOW_ID", input.WORKFLOW_ID.ToString(), "@ID_PROCESS", input.ID_PROCESS.ToString(), "@CLIENT_ID", input.CLIENT_ID, "@CLIENT_NAME", input.CLIENT_NAME, "@ID_STATUT", input.ID_STATUT, "@ENDING_DATE", input.ENDING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@ID_H_WORKFLOW", input.ID_H_WORKFLOW.ToString() };

                return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_STEP", parameters, true);
            }
            else
            {
                string[] requests = { "INSERT INTO PNPU_H_STEP (ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, CLIENT_NAME, USER_ID, LAUNCHING_DATE, ENDING_DATE, ID_STATUT, TYPOLOGY, ID_H_WORKFLOW) VALUES (@ITERATION, @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @CLIENT_NAME, @USER_ID, @LAUNCHING_DATE, @ENDING_DATE, @ID_STATUT, @TYPOLOGY, @ID_H_WORKFLOW)" };
                string[] parameters = new string[] { "@ITERATION", input.ITERATION.ToString(), "@WORKFLOW_ID", input.WORKFLOW_ID.ToString(), "@ID_PROCESS", input.ID_PROCESS.ToString(), "@CLIENT_ID", input.CLIENT_ID, "@CLIENT_NAME", input.CLIENT_NAME, "@USER_ID", input.USER_ID, "@LAUNCHING_DATE", input.LAUNCHING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@ENDING_DATE", input.ENDING_DATE.ToString("MM/dd/yyyy HH:mm:ss"), "@TYPOLOGY", input.TYPOLOGY, "@ID_STATUT", input.ID_STATUT, "@ID_H_WORKFLOW", input.ID_H_WORKFLOW.ToString() };

                return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_H_STEP", parameters, true);
            }
        }

        /// <summary>
        /// Get a workflow from PNPU_WORKFLOW
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns>Return a workflow</returns>
        public static PNPU_WORKFLOW getWorkflow(string workflowId)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneWorkflow + "'" + workflowId + "'", ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];

            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest.First();
        }

        /// <summary>
        /// Get the instance of workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public static int getWorkflowHistoric(int workflowId, int instanceId)
        {            
            string sRequest = string.Format(requestGetWorkflowHistoric, workflowId, instanceId);

            return int.Parse(DataManagerSQLServer.SelectCount(sRequest, ParamAppli.ConnectionStringBaseAppli));
        }

        /// <summary>
        /// Check if workflow exist in PNPU_H_WORKFLOW
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="instanceId"></param>
        /// <returns>Return the id of workflow instance if exist, Return "0" if not</returns>
        public static bool historicWorkflowExist(int workflowId, int instanceId)
        {
            // return getWorkflowHistoric(workflowId) != null
            return getWorkflowHistoric(workflowId, instanceId) == 0 ? false : true;
        }

        /// <summary>
        /// Get historic step
        /// </summary>
        /// <param name="step"></param>
        /// <returns>Return 0 if not exist, 1 if exist</returns>
        public static int getStepHistoric(PNPU_H_STEP step)
        {
            string sRequest = string.Format(requestGetStepHistoric, step.WORKFLOW_ID, step.CLIENT_ID, step.ID_PROCESS, step.ITERATION, step.ID_H_WORKFLOW);

            return int.Parse(DataManagerSQLServer.SelectCount(sRequest, ParamAppli.ConnectionStringBaseAppli));
        }

        /// <summary>
        /// Check if the step exist in PNPU_H_STEP
        /// </summary>
        /// <param name="step"></param>
        /// <returns>Return true if exist, false if not</returns>
        public static bool historicStepExist(PNPU_H_STEP step)
        {
            return getStepHistoric(step) == 0 ? false : true;
        }

        /// <summary>
        /// Allow to modify name (WORKFLOW_LABEL) of a workflow
        /// </summary>
        /// <param name="input"></param>
        /// <param name="workflowID"></param>
        /// <returns></returns>
        public static string ModifyWorkflow(PNPU_WORKFLOW input, string workflowID)
        {
            string[] requests = { "UPDATE PNPU_WORKFLOW SET WORKFLOW_LABEL = @WORKFLOW_LABEL WHERE WORKFLOW_ID = @WORKFLOW_ID " };
            string[] parameters = new string[] { "@WORKFLOW_ID", workflowID, "@WORKFLOW_LABEL", input.WORKFLOW_LABEL };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_WORKFLOW", parameters, false);
        }

        /// <summary>
        /// Allow to affect processes to a workflow
        /// </summary>
        /// <param name="input"></param>
        /// <param name="workflowID"></param>
        /// <returns></returns>
        public static string AffectWorkflowsProcesses(PNPU_STEP input, string workflowID)
        {
            string[] requests = { "INSERT INTO PNPU_STEP ( ORDER_ID, ID_PROCESS, WORKFLOW_ID) VALUES( @ORDER_ID, @ID_PROCESS, @WORKFLOW_ID)" };
            string[] parameters = new string[] { "@ORDER_ID", input.ID_ORDER.ToString(), "@ID_PROCESS", input.ID_PROCESS.ToString(), "@WORKFLOW_ID", input.ID_WORKFLOW };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_STEP", parameters, false);
        }

        /// <summary>
        /// Allow to modify name (PROCESS_LABEL) and the property (IS_LOOPABLE) of process
        /// </summary>
        /// <param name="input"></param>
        /// <param name="processID"></param>
        /// <returns></returns>
        public static string ModifyProcessus(PNPU_PROCESS input, string processID)
        {
            string[] requests = { "UPDATE PNPU_PROCESS SET PROCESS_LABEL = @PROCESS_LABEL, IS_LOOPABLE = @IS_LOOPABLE WHERE ID_PROCESS = @ID_PROCESS" };
            string[] parameters = new string[] { "@ID_PROCESS", processID, "@PROCESS_LABEL", input.PROCESS_LABEL, "@IS_LOOPABLE", input.IS_LOOPABLE };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_PROCESS", parameters, false);
        }

        /// <summary>
        /// Allow to create a process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CreateProcess(PNPU_PROCESS input)
        {
            string[] requests = { "INSERT INTO PNPU_PROCESS ( PROCESS_LABEL, IS_LOOPABLE) VALUES( @PROCESS_LABEL, @IS_LOOPABLE)" };
            string[] parameters = new string[] { "@PROCESS_LABEL", input.PROCESS_LABEL, "@IS_LOOPABLE", input.IS_LOOPABLE };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_PROCESS", parameters, true);
        }

        /// <summary>
        /// Allow to delete a process
        /// </summary>
        /// <param name="processID"></param>
        /// <returns></returns>
        public static string DeleteProcess(string processID)
        {
            string[] requests = { "DELETE FROM PNPU_PROCESS WHERE ID_PROCESS = @ID_PROCESS" };
            string[] parameters = new string[] { "@ID_PROCESS", processID };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_PROCESS", parameters, false);
        }

        /// <summary>
        /// Allow to delete a workflow
        /// </summary>
        /// <param name="workflowID"></param>
        /// <returns></returns>
        public static string DeleteWorkflow(string workflowID)
        {
            string[] requests = { "DELETE FROM PNPU_WORKFLOW WHERE WORKFLOW_ID = @WORKFLOW_ID " };
            string[] parameters = new string[] { "@WORKFLOW_ID", workflowID };

            return DataManagerSQLServer.ExecuteSqlTransaction(requests, "PNPU_WORKFLOW", parameters, false);
        }

        /// <summary>
        /// Get all workflow from PNPU_WORKFLOW
        /// </summary>
        /// <returns>Return a list of workflow</returns>
        public static IEnumerable<PNPU_WORKFLOW> GetAllWorkFLow()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllWorkflow, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_WORKFLOW> listTest = table.DataTableToList<PNPU_WORKFLOW>();

            return listTest;
        }

        /// <summary>
        /// Get all step from PNPU_STEP
        /// </summary>
        /// <returns>Return a list of step</returns>
        public static IEnumerable<PNPU_STEP> GetAllStep()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllStep, ParamAppli.ConnectionStringBaseAppli);
            DataTable table = result.Tables[0];


            IEnumerable<PNPU_STEP> listTest = table.DataTableToList<PNPU_STEP>();

            return listTest;
        }

        /// <summary>
        /// Get all processes affeteced to a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns>Return a list of processes</returns>
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
