using PNPUCore.RapportLivraison;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;

namespace PNPUCore.Process
{
    internal class ProcessLivraison : ProcessCore, IProcess
    {


        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessLivraison(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = ParamAppli.ProcessLivraison;
            LibProcess = "Livraison";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessLivraison(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            Logger.Log(this, ParamAppli.StatutInfo, " Debut du process " + ToString());

            string[] listClientId = CLIENT_ID.Split(',');
            int idInstanceWF = ID_INSTANCEWF;

            string GlobalResult = ParamAppli.StatutOk;

            RapportLivraison.Name = LibProcess;
            RapportLivraison.Debut = DateTime.Now;
            RapportLivraison.IdClient = CLIENT_ID;

            //Generate historic line with in progress status
            GenerateHistoricGlobal(listClientId, new DateTime(1800, 1, 1), ParamAppli.StatutInProgress, idInstanceWF, RapportLivraison.Debut);

            DataSet workflowProcesses = GetworkflowProcesses(ParamAppli.ConnectionStringBaseAppli, WORKFLOW_ID);

            foreach (DataRow drRow in workflowProcesses.Tables[0].Rows)
            {
                Processus processus = new Processus();

                processus.Name = drRow[0].ToString();
                processus.Result = GetProcessStatut(WORKFLOW_ID, int.Parse(drRow[2].ToString()), CLIENT_ID, idInstanceWF, ParamAppli.ConnectionStringBaseAppli);
                GlobalResult = SetGlobalStatut(RapportLivraison);
                RapportLivraison.Processus.Add(processus);
            }

            DataSet elementsLocalisation = GetHistLocalisation(ParamAppli.ConnectionStringBaseAppli, WORKFLOW_ID, ID_INSTANCEWF, CLIENT_ID);

            RapportLocalisation.Name = "Eléments à localiser";
            RapportLocalisation.NbElements = elementsLocalisation.Tables[0].Rows.Count;
            RapportLocalisation.Result = ParamAppli.TranscoSatut[ParamAppli.StatutInfo];

            if (elementsLocalisation.Tables[0].Rows.Count > 0)
            {
                RapportLocalisation.CctTaskID = elementsLocalisation.Tables[0].Rows[0].ItemArray[0].ToString();
                RapportLocalisation.CctVersion = elementsLocalisation.Tables[0].Rows[0].ItemArray[1].ToString();
            }
            else
            {
                RapportLocalisation.CctTaskID = "#N/A";
                RapportLocalisation.CctVersion = "#N/A";
            }

            foreach (DataRow drRow in elementsLocalisation.Tables[0].Rows)
            {
                Elements elements = new Elements();

                elements.ObjectID = drRow[3].ToString();
                elements.ObjectType = drRow[2].ToString();
                elements.ParentObj = drRow[4].ToString();
                elements.AuxObj = drRow[5].ToString();
                elements.Aux2Obj = drRow[6].ToString();
                elements.Aux3Obj = drRow[7].ToString();

                RapportLocalisation.Elements.Add(elements);
            }

            RapportLivraison.Fin = DateTime.Now;
            RapportLivraison.Result = GlobalResult;

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportLivraison.Fin, GlobalResult, RapportLivraison.Debut);

            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessLivraison);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            }

        }

        /// <summary>
        /// Get items localization.
        /// </summary>
        /// <param name="connectionStringBaseAppli"></param>
        /// <param name="wORKFLOW_ID"></param>
        /// <param name="iD_INSTANCEWF"></param>
        /// <param name="cLIENT_ID"></param>
        /// <returns>Return a DataSet wich contain all items localization.</returns>
        private DataSet GetHistLocalisation(string connectionStringBaseAppli, int wORKFLOW_ID, int iD_INSTANCEWF, string cLIENT_ID)
        {
            DataSet result = null;

            string sSelect = "SELECT CCT_TASK_ID, CCT_VERSION, CCT_OBJECT_TYPE, CCT_OBJECT_ID, CCT_PARENT_OBJ_ID, CCT_AUX_OBJECT_ID, CCT_AUX2_OBJECT_ID, CCT_AUX3_OBJECT_ID ";
            string sFrom = "FROM PNPU_H_LOCALISATION ";
            string sWhere = "WHERE WORKFLOW_ID = " + wORKFLOW_ID + " AND ID_H_WORKFLOW = " + iD_INSTANCEWF + " AND CLIENT_ID = " + cLIENT_ID + " ";
            string sOrder = "ORDER BY CCT_OBJECT_TYPE";

            string sRequest = sSelect + sFrom + sWhere + sOrder;

            try
            {
                result = DataManagerSQLServer.GetDatas(sRequest, connectionStringBaseAppli);
            }
            catch (Exception ex)
            {
                RapportLivraison.Result = ParamAppli.TranscoSatut["ERROR"];
                LoggerHelper.Log(this, ParamAppli.StatutError, "Une erreur s'est produite lors de la récupération des éléments à localiser depuis la table PNPU_H_LOCALISATION, " + ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Set Global statut to report
        /// </summary>
        /// <param name="rapportLivraison"></param>
        /// <returns></returns>
        private string SetGlobalStatut(RLivraison rapportLivraison)
        {
            return rapportLivraison.Result != "ERROR" ? "ERROR" : null;
        }

        /// <summary>
        /// Get process statut from PNPU_H_STEP by ID_PROCESS
        /// </summary>
        /// <param name="wORKFLOW_ID"></param>
        /// <param name="idProcess"></param>
        /// <param name="cLIENT_ID"></param>
        /// <param name="idInstanceWF"></param>
        /// <param name="connectionStringBaseAppli"></param>
        /// <returns>Return statut string</returns>
        private string GetProcessStatut(int wORKFLOW_ID, int idProcess, string cLIENT_ID, int idInstanceWF, string connectionStringBaseAppli)
        {
            string sRequest = "SELECT ID_STATUT FROM PNPU_H_STEP WHERE WORKFLOW_ID = " + wORKFLOW_ID + " AND ID_H_WORKFLOW = " + idInstanceWF + " AND ID_PROCESS = " + idProcess + " AND CLIENT_ID = " + cLIENT_ID;

            DataSet result = DataManagerSQLServer.GetDatas(sRequest, connectionStringBaseAppli);
            DataTable table = result.Tables[0];
            if (ParamAppli.ProcessLivraison == idProcess)
            {
                return ParamAppli.TranscoSatut["CORRECT"];
            }
            return table.Rows[0].ItemArray[0].ToString() == "mdi-alert" ? "mdi-alert" : null;
            //return table.Rows.Count == 0 ? null : ParamAppli.TranscoSatut[table.Rows[0].ItemArray[0].ToString()];
        }

        /// <summary>
        /// Get all processes associated with a workflow.
        /// </summary>
        /// <param name="connectionStringBaseAppli"></param>
        /// <param name="idWorkflow"></param>
        /// <returns>Return a DataSet wich contain the processes associated with a workflow</returns>
        private DataSet GetworkflowProcesses(string connectionStringBaseAppli, int idWorkflow)
        {
            string sSelect = "SELECT PP.PROCESS_LABEL, PS.ORDER_ID, PS.ID_PROCESS ";
            string sFrom = "FROM PNPU_STEP PS, PNPU_PROCESS PP, PNPU_WORKFLOW PW ";
            string sWhere = "WHERE PS.ID_PROCESS = PP.ID_PROCESS AND PS.WORKFLOW_ID = PW.WORKFLOW_ID AND PS.WORKFLOW_ID = " + idWorkflow + " ";
            string sOrderBy = "ORDER BY PS.ORDER_ID";

            string sRequest = sSelect + sFrom + sWhere + sOrderBy;

            DataSet result = DataManagerSQLServer.GetDatas(sRequest, connectionStringBaseAppli);

            return result;
        }
    }
}