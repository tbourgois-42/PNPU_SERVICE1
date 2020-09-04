using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PNPUCore.Process
{
    internal class ThreadProcessusCritique
    {
        ProcessProcessusCritique Process;
        int IIndex;


        public ThreadProcessusCritique(ProcessProcessusCritique process, int iIndex)
        {
            Process = process;
            IIndex = iIndex;
        }


        public void ThreadProcCrit()
        {
            //List<IControle> listControl = ListControls.listOfMockControl;
            string sResultTask;
            string sRequete;
            string sModelCode = "PNPU_TRT_CRIT"; // MHUM pour tests
            Source RapportSource;
            RControle RapportControle;
            bool bBoucle;

            try
            {
                // MHUM pour tests
                ParamAppli.ListeInfoClient[Process.CLIENT_ID].ID_ORGA = "0002";

                ParamToolbox paramToolbox = new ParamToolbox();
                //DateTime sDatePaie = paramToolbox.GetDtPaie(Process.WORKFLOW_ID, Process.ID_INSTANCEWF);


                RapportSource = new Rapport.Source
                {
                    Name = "Planification des processus critiques",
                    Controle = new List<RControle>(),
                    Result = ParamAppli.StatutOk
                };


                List<string> lParameters = new List<string>();
                List<string> lRequests = new List<string>();
                int iID_SCHED_TASK = 1;
                DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();

                sRequete = "select MAX(ID_SCHED_TASK) from M4RJS_SCHED_TASKS";
                DataSet dataSet = dataManagerSQL.GetData(sRequete, Process.sConnectionString[IIndex]);
                if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
                {
                    if (Int32.TryParse(dataSet.Tables[0].Rows[0][0].ToString(), out iID_SCHED_TASK))
                    {
                        iID_SCHED_TASK++;
                    }
                    else
                    {
                        iID_SCHED_TASK = 1;
                    }
                }

                RapportControle = new RControle
                {
                    Name = "Planification (" + iID_SCHED_TASK.ToString("########0") + ")",
                    Tooltip = "Génération de la planification des processus critiques",
                    Message = new List<string>()
                };

                lParameters.Add("@ID_SCHED_TASK");
                lParameters.Add(iID_SCHED_TASK.ToString());
                lParameters.Add("@SCHED_TASK_NAME");
                lParameters.Add("PNPU_EXEC_TRAITEMENT_GRP_JS");
                lParameters.Add("@ID_ORGA_TMP");
                lParameters.Add(ParamAppli.ListeInfoClient[Process.CLIENT_ID].ID_ORGA);
                lParameters.Add("@USER");
                lParameters.Add("M4ADM");
                lParameters.Add("@ROLE");
                lParameters.Add("M4ADM");

                lParameters.Add("@TASK_DESC");
                lParameters.Add("PNPU - Planification des processus critiques");
                lParameters.Add("@DATE_LAUNCH");
                lParameters.Add(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:00"));
                lParameters.Add("@DATE_PAY");
                // MHUM Pour tests
                //lParameters.Add(sDatePaie.ToString("yyyy-MM-dd"));
                lParameters.Add("2020-08-25");

                //lRequests.Add("INSERT INTO M4RJS_SCHED_TASKS(ID_SCHED_TASK, SCHED_TASK_NAME, SCHED_TASK_DESC, ID_TASK, CREATOR, USER_PLANNER, USER_EXECUTOR, ROLE_EXECUTOR, PRIORITY, SERVER_NAME, SERVICE_NAME, ID_TIMEZONE, MAX_DELAY, HAS_CALENDARS, SET_STATISTICS, STAT_FILE_BASENAME, SET_NOTIFICATIONS, START_DATE, END_DATE, DATE_CREATED, STATISTICS_LEVEL, IS_DYNAMIC, PREVIEW_PERIOD, DATE_MAX, DATE_NEXT, EXECUTION_MAX, EXECUTION_NUMBER, ID_APPROLE, ID_SECUSER, DT_LAST_UPDATE, ID_ORGANIZATION, IS_ADMINISTRATIVE, SET_NOTIFY_EXEC_START, SET_NOTIFY_EXEC_END, MAILS_TO_NOTIFY_EXEC, AUTO_NOTIFY_EXEC, SET_ATTACH_RESOURCES, SET_ATTACH_PASSWORD) VALUES (@ID_SCHED_TASK, @SCHED_TASK_NAME + ' ' + @ID_ORGA_TMP, @TASK_DESC, @SCHED_TASK_NAME, 29, @USER, @USER, @ROLE, 5, @SERVER_NAME, @SERVICE_NAME, 'Romance', 0, 0, 0, null, 0, getUTCDate(), null, getUTCDate(), 0, 0, null, null, null, null, null, @ROLE, @USER, getUTCDate(), @ID_ORGA_TMP, null, null, null, null, null, null, null)");
                lRequests.Add("INSERT INTO M4RJS_SCHED_TASKS(ID_SCHED_TASK, SCHED_TASK_NAME, SCHED_TASK_DESC, ID_TASK, CREATOR, USER_PLANNER, USER_EXECUTOR, ROLE_EXECUTOR, PRIORITY, SERVER_NAME, SERVICE_NAME, ID_TIMEZONE, MAX_DELAY, HAS_CALENDARS, SET_STATISTICS, STAT_FILE_BASENAME, SET_NOTIFICATIONS, START_DATE, END_DATE, DATE_CREATED, STATISTICS_LEVEL, IS_DYNAMIC, PREVIEW_PERIOD, DATE_MAX, DATE_NEXT, EXECUTION_MAX, EXECUTION_NUMBER, ID_APPROLE, ID_SECUSER, DT_LAST_UPDATE, ID_ORGANIZATION, IS_ADMINISTRATIVE, SET_NOTIFY_EXEC_START, SET_NOTIFY_EXEC_END, MAILS_TO_NOTIFY_EXEC, AUTO_NOTIFY_EXEC, SET_ATTACH_RESOURCES, SET_ATTACH_PASSWORD) VALUES (@ID_SCHED_TASK, @SCHED_TASK_NAME + ' ' + @ID_ORGA_TMP, @TASK_DESC, @SCHED_TASK_NAME, 29, @USER, @USER, @ROLE, 5, null, null, 'Romance', 0, 0, 0, null, 0, getUTCDate(), null, getUTCDate(), 0, 0, null, null, null, null, null, @ROLE, @USER, getUTCDate(), @ID_ORGA_TMP, null, null, null, null, null, null, null)");
                lRequests.Add("INSERT INTO M4RJS_SCHED_TASKS1 VALUES (@ID_SCHED_TASK,@USER+'|'+@ROLE+'|ROOT_SESSION#ID_RSM#\"'+@ROLE+'\"|ROOT_SESSION#LICENSE#\"\"|ROOT_SESSION#WU_MASK#\"''; 430; 460; 200; ''\"|ROOT_SESSION#LANGUAGE#4|ROOT_SESSION#ID_PERSON#\"\"|ROOT_SESSION#ID_PROFILE#\"RSM_DEVELOPPEMENT\"|ROOT_SESSION#ID_APP_ROLE#\"'+@ROLE+'\"|ROOT_SESSION#ID_APP_USER#\"'+@USER+'\"|ROOT_SESSION#ROUND_NUMBER#0|ROOT_SESSION#DATA_END_DATE#{4000-01-01 00:00:00}|ROOT_SESSION#ID_DEBUG_USER#\"\"|ROOT_SESSION#APP_USER_ALIAS#\"\"|ROOT_SESSION#EXECUTION_DATE#{'+convert(varchar(19),getUTCDate(),20)+'}|ROOT_SESSION#ROUND_CURRENCY#0|ROOT_SESSION#USR_AUDIT_CRED#\"\"|ROOT_SESSION#DATA_START_DATE#{1800-01-01 00:00:00}|ROOT_SESSION#DEFAULT_PROJECT#\"\"|ROOT_SESSION#WU_MASK_CONTROL#0|ROOT_SESSION#PREFIX_OVERWRITE#\"\"|ROOT_SESSION#NUM_ROWS_DB_LIMIT#0|ROOT_SESSION#EXECUTION_END_DATE#{4000-01-01 00:00:00}|ROOT_SESSION#META_DATA_END_DATE#{4000-01-01 00:00:00}|ROOT_SESSION#USR_AUDIT_SRV_NAME#\"\"|ROOT_SESSION#ID_DEFAULT_CURRENCY#\"EUR\"|ROOT_SESSION#DATA_CORRECTION_DATE#{'+convert(varchar(10),getUTCdate(),103)+'}|ROOT_SESSION#EXECUTE_REALSQL_MODE#0|ROOT_SESSION#EXECUTION_START_DATE#{1800-01-01 00:00:00}|ROOT_SESSION#META_DATA_START_DATE#{1800-01-01 00:00:00}|ROOT_SESSION#USR_AUDIT_SESSION_KEY#\"\"|ROOT_SESSION#SELECT_TO_GET_DB_USER_2X#\"\"|ROOT_SESSION#USR_AUDIT_CLIENT_MACHINE#\"''\"|ROOT_SESSION#USR_AUDIT_SRV_ID_SESSION#\"\"|ROOT_SESSION#META_DATA_CORRECTION_DATE#{'+convert(varchar(19),getUTCDate(), 120)+'}|ROOT_SESSION#ID_ORGANIZATION#\"'+@ID_ORGA_TMP+'\"')");
                lRequests.Add("INSERT INTO M4RJS_SCHED_TASKS2 values(@ID_SCHED_TASK,'ASAP')");
                //lRequests.Add("INSERT INTO M4RJS_TASK_EXE (ID_SCHED_TASK,ID_TASK_EXE,PLANNED_DATETIME,EXPIRATION_DATETIM,STATUS,EXCEPTION_FLAG,START_DATETIME,END_DATETIME,SERVER_NAME,SERVICE_NAME,SELECTED_BY_SERVIC,EXE_SERVER,EXE_SERVICE,TASK_EXE_DESC,ID_APPROLE,ID_SECUSER,DT_LAST_UPDATE,USER_ABORTER) VALUES (@ID_SCHED_TASK,1,@DATE_LAUNCH,null,1,0,null,null,@SERVER_NAME,@SERVICE_NAME,0,null,null,@TASK_DESC,@ROLE,@USER,getUTCDate(),null)");
                lRequests.Add("INSERT INTO M4RJS_TASK_EXE (ID_SCHED_TASK,ID_TASK_EXE,PLANNED_DATETIME,EXPIRATION_DATETIM,STATUS,EXCEPTION_FLAG,START_DATETIME,END_DATETIME,SERVER_NAME,SERVICE_NAME,SELECTED_BY_SERVIC,EXE_SERVER,EXE_SERVICE,TASK_EXE_DESC,ID_APPROLE,ID_SECUSER,DT_LAST_UPDATE,USER_ABORTER) VALUES (@ID_SCHED_TASK,1,@DATE_LAUNCH,null,1,0,null,null,null,null,0,null,null,@TASK_DESC,@ROLE,@USER,getUTCDate(),null)");
                lRequests.Add("INSERT INTO M4RJS_SUBTASK_EXE (ID_SCHED_TASK,ID_TASK_EXE,ID_SUBTASK_EXE,ORDER_IN_TREE,ID_TASK,COMPOSITE,LOCAL_ID,START_DATETIME,END_DATETIME,VM_EXIT_FLAG,PARENT_ORDER,ID_APPROLE,ID_SECUSER,DT_LAST_UPDATE,QUOTA_ROWS,QUOTA_MAX_ROWS,QUOTA_PEAK_ROWS) VALUES (@ID_SCHED_TASK,1,1,1,@SCHED_TASK_NAME,1,0,null,null,null,1,@ROLE,@USER,getUTCDate(),null,null,null)");
                lRequests.Add("INSERT INTO  M4RJS_SUBTASK_EXE1 (ID_SCHED_TASK,ID_TASK_EXE,ID_SUBTASK_EXE,ORDER_IN_TREE,LOG_MESSAGES) VALUES (@ID_SCHED_TASK,1,1,1,null)");
                lRequests.Add("INSERT INTO M4RJS_SUBTASK_EXE2 (ID_SCHED_TASK,ID_TASK_EXE,ID_SUBTASK_EXE,ORDER_IN_TREE,STATISTICS_LOG) VALUES (@ID_SCHED_TASK,1,1,1,null)");
                lRequests.Add("INSERT INTO M4RJS_DEF_PARAMS (DT_LAST_UPDATE, ID_SECUSER, ID_APPROLE, ID_SCHED_TASK, PARAM_NAME) VALUES (getUTCDate(), @USER, @ROLE, @ID_SCHED_TASK, 'ARG_DT_PAIE_JS')");
                lRequests.Add("INSERT INTO M4RJS_DEF_PARAMS1 (ID_SCHED_TASK, PARAM_NAME, PARAM_VALUE) VALUES (@ID_SCHED_TASK, 'ARG_DT_PAIE_JS', @DATE_PAY)");

                string sTest = DataManagerSQLServer.ExecuteSqlTransaction(lRequests.ToArray(), lParameters.ToArray(), Process.sConnectionString[IIndex]);
                if (sTest == "OK")
                {
                    string sTaskStatus;
                    string sNumTraitement = "NA";
                    int iID_SCHED_TASKTrt = -1;


                    // Attente de l'exécution de la tâche créant le traitement groupé
                    sTaskStatus = ResultScheduleTask(iID_SCHED_TASK, 500, out sResultTask);
                    TraiteResultat(sTaskStatus, sResultTask, ref RapportControle);
                    if (RapportControle.Result == ParamAppli.StatutError)
                    {
                        RapportSource.Result = ParamAppli.StatutError;
                    }
                    RapportControle.Result = RapportControle.Result;
                    RapportSource.Controle.Add(RapportControle);
                    Process.rSource[IIndex].Add(RapportSource);


                    if ((sTaskStatus == ParamAppli.statusScheduleTaskTermine) && (sResultTask == ParamAppli.StatutOk))
                    {

                        // Recherche du numéro de traitement affecté
                        sRequete = "select TOP 1 B.PARAM_VALUE from M4RJS_SCHED_TASKS A, M4RJS_DEF_PARAMS1 B where A.SCHED_TASK_NAME LIKE 'PNPU%' AND A.ID_SCHED_TASK>" + iID_SCHED_TASK.ToString("########0") + " AND A.ID_TASK='CFR_BP_START_TRAIT_GROUP_JS' AND A.ID_SCHED_TASK = B.ID_SCHED_TASK AND B.PARAM_NAME = 'ARG_ID_EXEC' ORDER BY A.ID_SCHED_TASK";
                        dataSet = dataManagerSQL.GetData(sRequete, Process.sConnectionString[IIndex]);
                        if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
                        {
                            sNumTraitement = dataSet.Tables[0].Rows[0][0].ToString();
                        }

                        if (sNumTraitement != "NA")
                        {
                            RapportSource = new Rapport.Source
                            {
                                Name = "Processus critiques",
                                Controle = new List<RControle>(),
                                Result = ParamAppli.StatutOk
                            };

                            iID_SCHED_TASKTrt = iID_SCHED_TASK;
                            // Boucle sur les traitements
                            foreach (string sTraitement in Process.lTraitements[IIndex])
                            { 
                                RapportControle = new RControle
                                {
                                    Name = sTraitement,
                                    Message = new List<string>()
                                };

                                sRequete = "select ID_SCHED_TASK from M4RJS_SCHED_TASKS where SCHED_TASK_NAME like 'Traitement " + sNumTraitement + " : " + sTraitement + "' and ID_SCHED_TASK> " + iID_SCHED_TASKTrt.ToString("########0");
                                bBoucle = true;

                                while (bBoucle)
                                {
                                    dataSet = dataManagerSQL.GetData(sRequete, Process.sConnectionString[IIndex]);
                                    if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
                                    {
                                        bBoucle = false;
                                        if (!Int32.TryParse(dataSet.Tables[0].Rows[0][0].ToString(), out iID_SCHED_TASKTrt))
                                        {
                                            iID_SCHED_TASKTrt = -1;
                                        }
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(500);
                                    }
                                }

                                // Attente de l'exécution du traitement
                                RapportControle.Name += " (" + iID_SCHED_TASKTrt.ToString("########0") + ")";
                                
                                sTaskStatus = ResultScheduleTask(iID_SCHED_TASKTrt, 500, out sResultTask);

                                TraiteResultat(sTaskStatus, sResultTask, ref RapportControle);
                                RapportSource.Controle.Add(RapportControle);
                                if (RapportControle.Result == ParamAppli.StatutError)
                                {
                                    RapportSource.Result = ParamAppli.StatutError;
                                    RapportControle.Result = ParamAppli.StatutError;
                                    //break;
                                }
                                else
                                {
                                    RapportControle.Result = RapportControle.Result;
                                }

                            }
                            Process.rSource[IIndex].Add(RapportSource);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception dans ThreadProcCrit : " + ex.Message);
            }
            Process.bThreadTermine[IIndex] = true;
        }


        private void TraiteResultat(string sTaskStatus, string sResultTask, ref RControle rapportControle)
        {
            try
            {
                if (sTaskStatus == ParamAppli.statusScheduleTaskTermine)
                {
                    if (sResultTask == ParamAppli.StatutOk)
                    {
                        rapportControle.Result = ParamAppli.StatutOk;
                    }
                    else
                    {
                        rapportControle.Result = ParamAppli.StatutError;
                        rapportControle.Message.Add("Le traitement s'est terminé en erreur");
                    }
                }
                else
                {
                    switch (sTaskStatus)
                    {
                        case ParamAppli.statusScheduleTaskAbandon1:
                        case ParamAppli.statusScheduleTaskAbandon2:
                        case ParamAppli.statusScheduleTaskAnnule1:
                        case ParamAppli.statusScheduleTaskAnnule2:
                        case ParamAppli.statusScheduleTaskAnnule3:
                            rapportControle.Result = ParamAppli.StatutError;
                            rapportControle.Message.Add("La tâche a été annulée.");
                            break;

                        case ParamAppli.statusScheduleTaskExpire:
                            rapportControle.Result = ParamAppli.StatutError;
                            rapportControle.Message.Add("La tâche a expirée.");
                            break;

                        case ParamAppli.statusScheduleTaskInterrompu:
                            rapportControle.Result = ParamAppli.StatutError;
                            rapportControle.Message.Add("La tâche a été interrompue.");
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("TraiteResultat : " + ex.Message);
            }
        }

        private string ResultScheduleTask(int iID_SCHED_TASK, int iTimeOut, out string sResultTask)
        {
            DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
            string sStatus = string.Empty;
            DataSet dataSet;
            bool bBoucle = true;
            sResultTask = string.Empty;

            while (bBoucle)
            {
                dataSet = dataManagerSQL.GetData("SELECT STATUS FROM M4RJS_TASK_EXE WHERE ID_SCHED_TASK =" + iID_SCHED_TASK.ToString("########0"), Process.sConnectionString[IIndex]);
                if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
                {
                    sStatus = dataSet.Tables[0].Rows[0][0].ToString();

                    if ((sStatus != ParamAppli.statusScheduleTaskAttente) && (sStatus != ParamAppli.statusScheduleTaskEnCours))
                    {
                        bBoucle = false;
                    }

                    if (bBoucle)
                    {
                        System.Threading.Thread.Sleep(iTimeOut);
                    }
                }
            }

            // Si la tâche est terminée, on récupère le résultat de l'exécution
            if (sStatus == ParamAppli.statusScheduleTaskTermine)
            {
                dataSet = dataManagerSQL.GetData("SELECT VM_EXIT_FLAG FROM M4RJS_SUBTASK_EXE WHERE ID_SCHED_TASK =" + iID_SCHED_TASK.ToString("########0"), Process.sConnectionString[IIndex]);
                if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
                {
                    sResultTask = string.Empty;
                    foreach (DataRow drRow in dataSet.Tables[0].Rows)
                    {
                        if ((drRow[0].ToString() == "0") && (sResultTask != ParamAppli.StatutError))
                        {
                            sResultTask = ParamAppli.StatutOk;
                        }
                        else
                        {
                            sResultTask = ParamAppli.StatutError;
                        }
                    }
                }

            }
            return sStatus;
        }
    }
}