using PNPUCore.Controle;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PNPUCore.Process
{
    internal class ProcessAnalyseImpactData : ProcessCore, IProcess
    {
        public Dictionary<string, List<string>> dListeTablesFieldsIgnore;
        public List<string> lListPersonnalTables;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessAnalyseImpactData(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = ParamAppli.ProcessAnalyseImpactData;
            LibProcess = "Analyse d'impact sur les données";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessAnalyseImpactData(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            sRapport = string.Empty;
            rapportAnalyseImpactData = new RapportAnalyseData
            {
                Debut = DateTime.Now,
                Name = LibProcess,
                IdClient = CLIENT_ID,
                Result = ParamAppli.StatutInfo,
                listRapportAnalyseImpactMDBData = new List<RapportAnalyseImpactMDBData>()
            };
            string[] tListeMDB = null;
            string sPackCourrant = string.Empty;
            string sSequenceCourrante = string.Empty;
            RapportAnalyseImpactPackData rapportAnalyseImpactPackData = null;
            List<string> lColumnsList = new List<string>();
            List<string> lTablesPostPaie = new List<string> { "M4SCO_ROWS", "M4SCO_ROW_COL_DEF" };
            List<string> lTablesDSN = new List<string>();// { "M4SFR_DSN_CTP_PARAM","M4SFR_DSN_PARAM_RUB_NAT08","M4SFR_DSN_PARAM_RUB_NAT05"};
            int idInstanceWF = ID_INSTANCEWF;
            DataSet dsDataSet;
            DataManagerSQLServer dataManagerSQLServer = new DataManagerSQLServer();


            //On génère les historic au début pour mettre en inprogress
            //GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress);

            ParamToolbox paramToolbox = new ParamToolbox();

            string sConnectionStringBaseQA1 = paramToolbox.GetConnexionString("Before", WORKFLOW_ID, CLIENT_ID);

            // MHUM POUR TEST
            // TO REMOVE ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1 = "server=M4FRSQL13;uid=SAASSN305;pwd=SAASSN305;database=SAASSN305;";//ParamAppli.ConnectionStringBaseQA1;//"server=M4FRSQL13;uid=SAASSN305;pwd=SAASSN305;database=SAASSN305;";
            //TO REMOVE ParamAppli.ListeInfoClient[CLIENT_ID].ID_ORGA = "1600";//"0002";//

            // Récupération de la liste des champs à ignorer
            DataManagerSQLServer dmsDataManager = new DataManagerSQLServer();
            dListeTablesFieldsIgnore = dmsDataManager.GetIgnoredFields(sConnectionStringBaseQA1);

            //Recupération des tables liées à une personne
            lListPersonnalTables = dmsDataManager.GetPersonnalTables(sConnectionStringBaseQA1);

            // Recupération de la liste des tables DSN avec un champ SFR_ID_ORIG_PARAM
            dsDataSet = dmsDataManager.GetData("select ID_REAL_OBJECT from M4RDC_REAL_FIELDS where ID_REAL_FIELD = 'SFR_ID_ORIG_PARAM' AND ID_REAL_OBJECT LIKE '%DSN%'", sConnectionStringBaseQA1);
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    lTablesDSN.Add(drRow[0].ToString());
                }
            }

            //Création des contrôles
            ControleDataGeneric controleDataGeneric = new ControleDataGeneric(this);
            ControleDataM4SCO_ROW_COL_DEF controleDataM4SCO_ROW_COL_DEF = new ControleDataM4SCO_ROW_COL_DEF(this);
            ControleDataTablesDSN controleDataTablesDSN = new ControleDataTablesDSN(this);


            // Récupération des mdb
            GereMDBDansBDD gereMDBDansBDD = new GereMDBDansBDD();
            gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tListeMDB, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli, idInstanceWF);

            foreach (string sMDB in tListeMDB)
            {
                sPackCourrant = string.Empty;
                RapportAnalyseImpactMDBData rapportAnalyseImpactMDBData = new RapportAnalyseImpactMDBData
                {
                    Result = ParamAppli.StatutInfo,
                    Name = Path.GetFileName(sMDB)
                };
                rapportAnalyseImpactMDBData.Tooltip = "Analyse d'impact des données livrées dans le fichier " + rapportAnalyseImpactMDBData.Name;
                rapportAnalyseImpactMDBData.listRapportAnalyseImpactPackData = new List<RapportAnalyseImpactPackData>();
                rapportAnalyseImpactPackData = null;

                //Récupération de toutes les commandes data
                List<RmdCommandData> listCommandData = getAllDataCmd(sMDB, ParamAppli.ListeInfoClient[CLIENT_ID].bORACLE);

                foreach (RmdCommandData commandData in listCommandData)
                {
                    commandData.CmdCode = dataManagerSQLServer.removeCommentOnCommand(commandData.CmdCode);
                    string[] listLineRequest = dataManagerSQLServer.splitCmdCodeData(commandData.CmdCode);
                    string sTable = String.Empty;
                    string sFilter = String.Empty;

                    if ((sPackCourrant != commandData.IdPackage) || (sSequenceCourrante != commandData.CmdSequence))
                    {
                        if (rapportAnalyseImpactPackData != null)
                        {
                            if (rapportAnalyseImpactPackData.listCommandData.Count > 0)
                            {

                                for (int iIndex2 = 0; (iIndex2 < rapportAnalyseImpactPackData.listCommandData.Count); iIndex2++)
                                {
                                    rapportAnalyseImpactMDBData.Result = TestStatut(rapportAnalyseImpactMDBData.Result, rapportAnalyseImpactPackData.listCommandData[iIndex2].Result);

                                }
                            }
                            rapportAnalyseImpactMDBData.listRapportAnalyseImpactPackData.Add(rapportAnalyseImpactPackData);

                        }
                        rapportAnalyseImpactPackData = new RapportAnalyseImpactPackData
                        {
                            listCommandData = new List<CommandData>(),
                            listEltsALocaliserData = new List<EltsALocaliserData>(),
                            //rapportAnalyseImpactPackData.Name = commandData.IdPackage;
                            CodePack = commandData.IdPackage,
                            NumCommande = commandData.CmdSequence,
                            Result = ParamAppli.StatutInfo
                        };
                        sPackCourrant = commandData.IdPackage;
                    }

                    for (int iIndex = 0; iIndex < listLineRequest.Length; iIndex++)
                    {
                        //if (listLineRequest[iIndex].IndexOf("M4SFR_COPY_DATA_ORG") >= 0)
                        {
                            CommandData commandData1 = new CommandData
                            {
                                Result = ParamAppli.StatutInfo,
                                Name = listLineRequest[iIndex],
                                Message = string.Empty
                            };
                            EltsALocaliserData eltsALocaliserData = new EltsALocaliserData();

                            if ((commandData.IdObject.Contains("0002")) || (commandData.IdObject.Contains("COPY_DATA_9999")))
                            {

                                dataManagerSQLServer.ExtractTableFilter(listLineRequest[iIndex], ref sTable, ref sFilter, ref lColumnsList);
                                // Traitement des tables postpaie
                                if (lTablesPostPaie.Contains(sTable))
                                {
                                    controleDataM4SCO_ROW_COL_DEF.AnalyzeCommand(listLineRequest[iIndex], ref commandData1, ref eltsALocaliserData, commandData);
                                }
                                // Traitement des tables DSN
                                else if (lTablesDSN.Contains(sTable))
                                {
                                    controleDataTablesDSN.AnalyzeCommand(listLineRequest[iIndex], ref commandData1, ref eltsALocaliserData, commandData);
                                }
                                if (commandData1.Result == ParamAppli.StatutInfo)
                                {
                                    controleDataGeneric.AnalyzeCommand(listLineRequest[iIndex], ref commandData1, ref eltsALocaliserData, commandData);
                                }
                            }
                            else
                            {
                                commandData1.Message = "Pas de contrôle automatique sur cette commande.";
                                commandData1.Result = ParamAppli.StatutInfo;

                            }

                            rapportAnalyseImpactPackData.listCommandData.Add(commandData1);
                            rapportAnalyseImpactPackData.Result = TestStatut(rapportAnalyseImpactPackData.Result, commandData1.Result);
                            if (eltsALocaliserData.Name != null)
                            {
                                rapportAnalyseImpactPackData.listEltsALocaliserData.Add(eltsALocaliserData);
                            }
                        }
                    }

                }


                if (rapportAnalyseImpactPackData.listCommandData.Count > 0)
                {
                    for (int iIndex2 = 0; (iIndex2 < rapportAnalyseImpactPackData.listCommandData.Count); iIndex2++)
                    {
                        rapportAnalyseImpactMDBData.Result = TestStatut(rapportAnalyseImpactMDBData.Result, rapportAnalyseImpactPackData.listCommandData[iIndex2].Result);
                    }
                }
                rapportAnalyseImpactMDBData.listRapportAnalyseImpactPackData.Add(rapportAnalyseImpactPackData);

                rapportAnalyseImpactData.listRapportAnalyseImpactMDBData.Add(rapportAnalyseImpactMDBData);
                rapportAnalyseImpactData.Result = TestStatut(rapportAnalyseImpactData.Result, rapportAnalyseImpactMDBData.Result);

            }

            rapportAnalyseImpactData.Fin = DateTime.Now;

            //On fait un update pour la date de fin du process et son statut
            //DEVGenerateHistoric(RapportProcess.Fin, GlobalResult);

            GenerateHistoric(rapportAnalyseImpactData.Fin, rapportAnalyseImpactData.Result, rapportAnalyseImpactData.Debut);

            // Suppresion des paramètres toolbox temporaires
            paramToolbox.DeleteParamsToolbox(WORKFLOW_ID, ID_INSTANCEWF);

            /*DEVif (GlobalResult == ParamAppli.StatutOk)
            {*/
            int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, PROCESS_ID);
            LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            /* }*/

        }

        /* public List<string> GetDSNTables(string  sConnectionString)
         {
             List<string> lDSNTables = new List<string>();
             DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
             string sRequete = "SELECT "

             dataManagerSQL
         }*/


        private List<RmdCommandData> getAllDataCmd(String sConnection, bool bOracle)
        {
            DataManagerAccess dataManager = new DataManagerAccess();
            string requete = "select A.ID_PACKAGE, A.ID_CLASS, A.ID_OBJECT, A.CMD_CODE, A.CMD_SEQUENCE, B.CCT_TASK_ID from M4RDL_PACK_CMDS A, M4RDL_PACKAGES B where A.ID_PACKAGE like '%_D' AND A.ID_PACKAGE = B.ID_PACKAGE";
            if (TYPOLOGY != "Dédié")
            {
                requete += " AND  CMD_ACTIVE = -1 "; // Hors dédié on ne prend que les commandes actives
            }

            List<RmdCommandData> listDatacmd = new List<RmdCommandData>();
            DataSet result = dataManager.GetData(requete, sConnection);
            DataTable tableCmd = result.Tables[0];
            string sCommande;

            foreach (DataRow row in tableCmd.Rows)
            {
                sCommande = row[3].ToString();
                if (sCommande.Contains("DBMS"))
                {
                    sCommande = dataManager.GereOracle(sCommande, bOracle);
                }

                if (sCommande != string.Empty)
                {
                    RmdCommandData commandData = new RmdCommandData(row[0].ToString(), row[1].ToString(), row[2].ToString(), sCommande, ((decimal)(row[4])).ToString("###0"), this, row[5].ToString());
                    listDatacmd.Add(commandData);

                }
            }

            return listDatacmd;
        }

        public string TestStatut(string sStatutParent, string sStatutEnfant)
        {
            string sResultat = sStatutParent;
            if (sStatutParent == ParamAppli.StatutInfo)
            {
                if (sStatutEnfant != ParamAppli.StatutInfo)
                {
                    sResultat = sStatutEnfant;
                }
            }
            else if (sStatutParent == ParamAppli.StatutOk)
            {
                if ((sStatutEnfant != ParamAppli.StatutOk) && (sStatutEnfant != ParamAppli.StatutInfo))
                {
                    sResultat = sStatutEnfant;
                }
            }
            else if ((sStatutParent == ParamAppli.StatutWarning) && (sStatutEnfant != ParamAppli.StatutError))
            {
                sResultat = sStatutEnfant;
            }

            return sResultat;
        }
    }
}
