using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Xml.XPath;

namespace PNPUCore.Process
{
    internal class ProcessAnalyseImpactData : ProcessCore, IProcess
    {
        public List<ElementLocaliser> listElementALocaliser;
        public Dictionary<string, List<string>> dListeTablesFieldsIgnore;
        public List<string> lListPersonnalTables;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessAnalyseImpactData(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            this.PROCESS_ID = ParamAppli.ProcessAnalyseImpactData;
            this.LibProcess = "Analyse d'impact sur les données";
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
            List<IControle> listControl = ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            rapportAnalyseImpactData = new RapportAnalyseData();
            rapportAnalyseImpactData.Debut = DateTime.Now;
            rapportAnalyseImpactData.Name = this.LibProcess;
            rapportAnalyseImpactData.IdClient = CLIENT_ID;
            rapportAnalyseImpactData.Result = ParamAppli.StatutInfo;
            rapportAnalyseImpactData.listRapportAnalyseImpactMDBData = new List<RapportAnalyseImpactMDBData>();
            string[] tListeMDB= null;
            string sPackCourrant = string.Empty;
            string sSequenceCourrante = string.Empty;
            RapportAnalyseImpactPackData rapportAnalyseImpactPackData = null;
            List<string> lColumnsList = new List<string>();
            List<string> lTablesPostPaie = new List<string> { "M4SCO_ROWS", "M4SCO_ROW_COL_DEF" };
            List<string> lTablesDSN = new List<string>();// { "M4SFR_DSN_CTP_PARAM","M4SFR_DSN_PARAM_RUB_NAT08","M4SFR_DSN_PARAM_RUB_NAT05"};
            int idInstanceWF = this.ID_INSTANCEWF;
            DataSet dsDataSet;


            //On génère les historic au début pour mettre en inprogress
            //DEVGenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress);

            // MHUM POUR TEST
            ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1 = "server=M4FRSQL13;uid=SAASSN305;pwd=SAASSN305;database=SAASSN305;";//ParamAppli.ConnectionStringBaseQA1;//"server=M4FRSQL13;uid=SAASSN305;pwd=SAASSN305;database=SAASSN305;";
            ParamAppli.ListeInfoClient[CLIENT_ID].ID_ORGA = "1600";//"0002";//

            // Récupération de la liste des champs à ignorer
            DataManagerSQLServer dmsDataManager = new DataManagerSQLServer();
            dListeTablesFieldsIgnore = dmsDataManager.GetIgnoredFields(ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1);

            //Recupération des tables liées à une personne
            lListPersonnalTables = dmsDataManager.GetPersonnalTables(ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1);

            // Recupération de la liste des tables DSN avec un champ SFR_ID_ORIG_PARAM
            dsDataSet = dmsDataManager.GetData("select ID_REAL_OBJECT from M4RDC_REAL_FIELDS where ID_REAL_FIELD = 'SFR_ID_ORIG_PARAM' AND ID_REAL_OBJECT LIKE '%DSN%'", ParamAppli.ListeInfoClient[CLIENT_ID].ConnectionStringQA1);
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    lTablesDSN.Add(drRow[0].ToString());
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
                RapportAnalyseImpactMDBData rapportAnalyseImpactMDBData = new RapportAnalyseImpactMDBData();
                rapportAnalyseImpactMDBData.Result = ParamAppli.StatutInfo;
                rapportAnalyseImpactMDBData.Name = Path.GetFileName(sMDB);
                rapportAnalyseImpactMDBData.Tooltip = "Analyse d'impact des données livrées dans le fichier " + rapportAnalyseImpactMDBData.Name;
                rapportAnalyseImpactMDBData.listRapportAnalyseImpactPackData = new List<RapportAnalyseImpactPackData>();
                rapportAnalyseImpactPackData = null;

                //Récupération de toutes les commandes data
                List<RmdCommandData> listCommandData = this.getAllDataCmd(sMDB, ParamAppli.ListeInfoClient[CLIENT_ID].bORACLE);
 
                foreach (RmdCommandData commandData in listCommandData)
                {
                    commandData.CmdCode = removeCommentOnCommand(commandData.CmdCode);
                    string[] listLineRequest = splitCmdCodeData(commandData.CmdCode);
                    string sTable=String.Empty;
                    string sFilter=String.Empty;

                    if ((sPackCourrant != commandData.IdPackage) || (sSequenceCourrante != commandData.CmdSequence))
                    {
                        if (rapportAnalyseImpactPackData != null)
                        {
                            if (rapportAnalyseImpactPackData.listCommandData.Count > 0)
                            {
                                
                                for (int iIndex2 = 0; (iIndex2 < rapportAnalyseImpactPackData.listCommandData.Count); iIndex2++)
                                {
                                    rapportAnalyseImpactMDBData.Result = TestStatut( rapportAnalyseImpactMDBData.Result, rapportAnalyseImpactPackData.listCommandData[iIndex2].Result);
                                    
                                }
                            }
                            rapportAnalyseImpactMDBData.listRapportAnalyseImpactPackData.Add(rapportAnalyseImpactPackData);

                        }
                        rapportAnalyseImpactPackData = new RapportAnalyseImpactPackData();
                        rapportAnalyseImpactPackData.listCommandData = new List<CommandData>();
                        rapportAnalyseImpactPackData.listEltsALocaliserData = new List<EltsALocaliserData>();
                        //rapportAnalyseImpactPackData.Name = commandData.IdPackage;
                        rapportAnalyseImpactPackData.CodePack = commandData.IdPackage;
                        rapportAnalyseImpactPackData.NumCommande = commandData.CmdSequence;
                        rapportAnalyseImpactPackData.Result = ParamAppli.StatutInfo;
                        sPackCourrant = commandData.IdPackage;
                    }

                    for(int iIndex = 0; iIndex < listLineRequest.Length; iIndex++)
                    {
                        //if (listLineRequest[iIndex].IndexOf("M4SFR_COPY_DATA_ORG") >= 0)
                        {
                            CommandData commandData1 = new CommandData();
                            commandData1.Result = ParamAppli.StatutInfo;
                            commandData1.Name = listLineRequest[iIndex];
                            commandData1.Message = string.Empty;
                            //commandData1.listControleCommandData = new List<ControleCommandData>();
                            EltsALocaliserData eltsALocaliserData = new EltsALocaliserData();
                            
                            if ((commandData.IdObject.Contains("0002") == true) || (commandData.IdObject.Contains("COPY_DATA_9999") == true))
                            {

                                ExtractTableFilter(listLineRequest[iIndex], ref sTable, ref sFilter, ref lColumnsList);
                                // Traitement des tables postpaie
                                if (lTablesPostPaie.Contains(sTable) == true)
                                {
                                    controleDataM4SCO_ROW_COL_DEF.AnalyzeCommand(listLineRequest[iIndex], ref commandData1, ref eltsALocaliserData, commandData);
                                }
                                // Traitement des tables DSN
                                else if (lTablesDSN.Contains(sTable) == true)
                                {
                                    controleDataTablesDSN.AnalyzeCommand(listLineRequest[iIndex], ref commandData1, ref eltsALocaliserData, commandData);
                                }
                                if (commandData1.Result == ParamAppli.StatutInfo)
                                    controleDataGeneric.AnalyzeCommand(listLineRequest[iIndex], ref commandData1, ref eltsALocaliserData, commandData);
                            }
                            else
                            {
                                commandData1.Message = "Pas de contrôle automatique sur cette commande.";
                                commandData1.Result = ParamAppli.StatutInfo;
                                
                            }

                            rapportAnalyseImpactPackData.listCommandData.Add(commandData1);
                            rapportAnalyseImpactPackData.Result = TestStatut(rapportAnalyseImpactPackData.Result, commandData1.Result);
                            if (eltsALocaliserData.Name != null)
                                rapportAnalyseImpactPackData.listEltsALocaliserData.Add(eltsALocaliserData);
                           
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


            /*DEVif (GlobalResult == ParamAppli.StatutOk)
            {*/
            int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, PROCESS_ID);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID, idInstanceWF);
            /* }*/

        }

        /* public List<string> GetDSNTables(string  sConnectionString)
         {
             List<string> lDSNTables = new List<string>();
             DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
             string sRequete = "SELECT "

             dataManagerSQL
         }*/


        public string GenerateReplace(string sTable, string sFilter, string sOrgaOrg, string sOrgaDest)
        {
            string sResultat = string.Empty;

            if (sFilter.Contains("ID_ORGANIZATION") == false)
                sFilter = " AND ID_ORGANIZATION='" + sOrgaDest + "'";
            sResultat = "Replace " + sTable + "  from origin to destination where \"" + sFilter + "\"";
            sResultat = ReplaceID_ORGA(sResultat, sOrgaOrg, sOrgaDest);

            return sResultat;
        }


        public void GetPKFields(string sTable, string sConnectionString, ref List<string> lPKFields)
        {
            DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
            DataSet dsDataSet;
            string sRequete = "SELECT B.ID_REAL_FIELD FROM M4RDC_FIELDS A,M4RDC_REAL_FIELDS B where B.ID_REAL_OBJECT = '" +sTable + "' AND B.ID_OBJECT=A.ID_OBJECT AND B.ID_FIELD=A.ID_FIELD AND A.POS_PK>0 AND A.ID_TYPE NOT LIKE '%DATE%' order by A.POS_PK";
            lPKFields.Clear();

            dsDataSet = dataManagerSQL.GetData(sRequete, sConnectionString);
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    lPKFields.Add(drRow[0].ToString());
                }
            }

        }

        public bool ExistsField(DataTable dtTable, string sField)
        {
            bool bResult = false;

            foreach (DataColumn dcColumn in dtTable.Columns)
            {
                if (dcColumn.ColumnName == sField)
                {
                    bResult = true;
                    break;
                }
            }

            return bResult;
        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'un script SQL
        /// </summary>
        /// <param name="sCommand">Script SQL à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est le script</param>
        /// <param name="sFilter">En sortie contient le filtre contenue dans le script</param>
        public void ExtractTableFilter(string sCommand, ref string sTable, ref string sFilter, ref List<string> lColumnsList)
        {
            lColumnsList.Clear();
            if (sCommand.Contains("M4SFR_COPY_DATA_ORG") == true)
            {
                ExtractTableFilterCOPY_DATA(sCommand, ref sTable, ref sFilter);
            }
            else if (sCommand.ToUpper().Contains("DELETE") == true)
            {
                ExtractTableFilterDELETE(sCommand, ref sTable, ref sFilter);
            }
            else if (sCommand.ToUpper().Contains("UPDATE") == true)
            {
                ExtractTableFilterUPDATE(sCommand, ref sTable, ref sFilter, ref lColumnsList);
            }
            else
            {
                sTable = String.Empty;
                sFilter = string.Empty;
            }
        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'un script UPDATE
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la suppression</param>
        /// <param name="sFilter">En sortie contien le filtre du script</param>
        public void ExtractTableFilterUPDATE(string sCommand, ref string sTable, ref string sFilter, ref List<string> lColumnsList)
        {
            int iIndex1;
            int iIndex2;
            string sCommandMAJ = sCommand.ToUpper();
            string sColumn;

            sTable = String.Empty;
            sFilter = String.Empty;

            iIndex1 = sCommandMAJ.IndexOf("UPDATE");
            if (iIndex1 >= 0)
            {
                iIndex1 += "UPDATE".Length;
                // Récupération du nom de la table
                while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                    iIndex1++;

                iIndex2 = iIndex1;
                while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                    iIndex2++;

                sTable = sCommand.Substring(iIndex1, iIndex2 - iIndex1);

                // Rechcerche des champs mis à jour
                iIndex1 = sCommandMAJ.IndexOf("SET", iIndex2);
                while (iIndex1 >= 0)
                {
                    iIndex1 += "SET".Length;
                    while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                        iIndex1++;
                    
                    iIndex2 = iIndex1;
                    while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                        iIndex2++;
                    sColumn = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                    lColumnsList.Add(sColumn);
                    iIndex1 = sCommandMAJ.IndexOf("SET", iIndex2);
                }

                

                // Récupération du filtre
                iIndex1 = sCommandMAJ.IndexOf("WHERE", iIndex2);
                if (iIndex1 >= 0)
                {
                    // Vérification de la présence d'un FROM
                    iIndex2 = sCommandMAJ.IndexOf("FROM", iIndex2);
                    if ((iIndex2 >= 0) && (iIndex2 < iIndex1))
                    {
                        int iIndex3;
                        iIndex2 += "FROM".Length;
                        while (Char.IsWhiteSpace(sCommandMAJ[iIndex2]) == true)
                            iIndex2++;
                        iIndex3 = iIndex2;
                        while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex3]) == true) || (sCommandMAJ[iIndex3] == '_'))
                            iIndex3++;

                        sTable = sCommand.Substring(iIndex2, iIndex3 - iIndex2);
                    }

                    iIndex1 += "WHERE".Length + 1;
                    sFilter = sCommand.Substring(iIndex1);
                }
                else // Vérification de la présence d'un FROM
                {
                    iIndex2 = sCommandMAJ.IndexOf("FROM", iIndex2);
                    if (iIndex2 >= 0)
                    {
                        int iIndex3;
                        iIndex2 += "FROM".Length;
                        while (Char.IsWhiteSpace(sCommandMAJ[iIndex2]) == true)
                            iIndex2++;
                        iIndex3 = iIndex2;
                        while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex3]) == true) || (sCommandMAJ[iIndex3] == '_'))
                            iIndex3++;

                        sTable = sCommand.Substring(iIndex2, iIndex3 - iIndex2);
                    }
                }

            }

        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'un script DELETE
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la suppression</param>
        /// <param name="sFilter">En sortie contien le filtre du script</param>
        public void ExtractTableFilterDELETE(string sCommand, ref string sTable, ref string sFilter)
        {
            int iIndex1;
            int iIndex2;
            string sCommandMAJ = sCommand.ToUpper();

            sTable = String.Empty;
            sFilter = String.Empty;

            iIndex1 = sCommandMAJ.IndexOf("DELETE");
            if (iIndex1 >= 0)
            {
                // Gestion de la présence de FROM
                iIndex2 = sCommandMAJ.IndexOf("FROM", iIndex1);
                if (iIndex2 >= 0)
                     iIndex1 = iIndex2 + "FROM".Length;
                else
                    iIndex1 += "DELETE".Length;

                // Récupération du nom de la table
                while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                    iIndex1++;

                iIndex2 = iIndex1;
                while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                    iIndex2++;

                sTable = sCommand.Substring(iIndex1, iIndex2 - iIndex1);

                // Récupération du filtre
                iIndex1 = sCommandMAJ.IndexOf("WHERE",iIndex2);
                if (iIndex1 >= 0)
                {
                    iIndex1 += "WHERE".Length + 1;
                    sFilter = sCommand.Substring(iIndex1);
                }

            }

        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'une commande de propagation
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la propagation</param>
        /// <param name="sFilter">En sortie contien le filtre de la propagation</param>
        public void ExtractTableFilterCOPY_DATA(string sCommand, ref string sTable, ref string sFilter)
        {
            if (sCommand.ToUpper().IndexOf("EXEC") >= 0)
            {
                sTable = ExtractParameterCOPY_DATA(sCommand, "@table");
                sFilter = ExtractParameterCOPY_DATA(sCommand, "@opt_where").Replace("''", "'");
            }
            else
            {
                sTable = ExtractParameterCOPY_DATA(sCommand, 1);
                sFilter = ExtractParameterCOPY_DATA(sCommand, 4).Replace("''", "'");
            }

        }

        /// <summary>
        /// Cette méthode permet de récupérer la valeur d'un paramètre passé à une procédure stockée type SQL server (EXEC...)
        /// </summary>
        /// <param name="sCommand">Commande à analyser</param>
        /// <param name="sParameterName">Nom du paramètre à extraire</param>
        private string ExtractParameterCOPY_DATA(string sCommand, string sParameterName)
        {
            int iIndex1, iIndex2;
            string sResultat = String.Empty;
            bool bContinue = true;

            try
            {
                iIndex1 = sCommand.IndexOf(sParameterName);
                if (iIndex1 >= 0)
                {
                    iIndex1 += sParameterName.Length;
                    while (sCommand[iIndex1] != '\'') iIndex1++;
                    iIndex1++;
                    iIndex2 = iIndex1;
                    while (bContinue == true)
                    {
                        if (iIndex2 >= sCommand.Length)
                            return (String.Empty);

                        if (sCommand[iIndex2] == '\'')
                        {
                            if (sCommand.Substring(iIndex2, 2) == "''")
                                iIndex2 += 2;
                            else
                                bContinue = false;
                        }
                        else
                            iIndex2++;
                    }

                    sResultat = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                }

            }
            catch (Exception)
            {
                sResultat = String.Empty;
            }
            return sResultat;
        }

        /// <summary>
        /// Cette méthode permet de récupérer la valeur d'un paramètre passé à une procédure stockée type Oracle (CALL...)
        /// </summary>
        /// <param name="sCommand">Commande à analyser</param>
        /// <param name="iParameterPos">Position du paramètre à extraire</param>
        private string ExtractParameterCOPY_DATA(string sCommand, int iParameterPos)
        {
            int iIndex1 = 0;
            int iIndex2 = 0;
            string sResultat = String.Empty;
            bool bContinue = true;
            int iNumeroParam = 1;

            try
            {
                iIndex1 = sCommand.IndexOf("M4SFR_COPY_DATA_ORG");
                if (iIndex1 >= 0)
                {
                    iIndex1 += "M4SFR_COPY_DATA_ORG".Length;
                    while (sCommand[iIndex1] != '\'') iIndex1++;
                    iIndex1++;
                    while (iNumeroParam < iParameterPos)
                    {
                        iIndex1 = sCommand.IndexOf(',', iIndex1);
                        if (iIndex1 == -1)
                            return String.Empty;

                        while (sCommand[iIndex1] != '\'') iIndex1++;
                        iIndex1++;

                        // Test si on est sur '', dans le filtre par exemple
                        if (sCommand[iIndex1] != '\'')
                            iNumeroParam++;
                    }

                    if (iNumeroParam == iParameterPos)
                    {
                        iIndex2 = iIndex1;
                        while (bContinue == true)
                        {
                            if (iIndex2 >= sCommand.Length)
                                return (String.Empty);

                            if (sCommand[iIndex2] == '\'')
                            {
                                if (sCommand.Substring(iIndex2, 2) == "''")
                                    iIndex2 += 2;
                                else
                                    bContinue = false;
                            }
                            else
                                iIndex2++;
                        }
                    }

                    sResultat = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                }

            }
            catch (Exception)
            {
                sResultat = String.Empty;
            }
            return sResultat;
        }


        private List<RmdCommandData> getAllDataCmd(String sConnection, bool bOracle)
        {
            DataManagerAccess dataManager = new DataManagerAccess();
            string requete = "select A.ID_PACKAGE, A.ID_CLASS, A.ID_OBJECT, A.CMD_CODE, A.CMD_SEQUENCE, B.CCT_TASK_ID from M4RDL_PACK_CMDS A, M4RDL_PACKAGES B where A.ID_PACKAGE like '%_D' AND A.ID_PACKAGE = B.ID_PACKAGE";
            if (TYPOLOGY != "Dédié") requete += " AND  CMD_ACTIVE = -1 "; // Hors dédié on ne prend que les commandes actives
            List<RmdCommandData> listDatacmd = new List<RmdCommandData>();
            DataSet result = dataManager.GetData(requete, sConnection);
            DataTable tableCmd = result.Tables[0];
            string sCommande;

            foreach (DataRow row in tableCmd.Rows)
            {
                sCommande = row[3].ToString();
                if (sCommande.Contains("DBMS") == true)
                    sCommande = GereOracle(sCommande, bOracle);
                if (sCommande != string.Empty)
                {
                    RmdCommandData commandData = new RmdCommandData(row[0].ToString(), row[1].ToString(), row[2].ToString(), sCommande, ((decimal)(row[4])).ToString("###0"), this, row[5].ToString());
                    listDatacmd.Add(commandData);

                }
            }

            return listDatacmd;
        }

        /// <summary>
        /// Methode permettant de gérer les scripts contenant un test sur la base de destination (Oracle). Elle retourne uniquement
        /// les commandes concernant le client en fonction du paramètre bOracle
        /// </summary>
        /// <param name="sCommande">Commande à analyser</param>
        /// <param name="bOracle">Vrai si la base du client est sous Oracle, faux sinon</param>
        /// <returns>Retourne la chaine de commande ne contenant que les lignes concernant le client</returns>
        private string GereOracle(string  sCommande,bool bOracle)
        {
            int iIndex1 = 0;
            int iIndex2;
            string sResultat = string.Empty;
            bool bScriptOracle;
            bool bContinue = true;
            string sCommandeMaj = sCommande.ToUpper();
            string sEtiquette;


            while (bContinue == true)
            {
                iIndex1 = sCommandeMaj.IndexOf("DBMS(", iIndex1);
                if (iIndex1 >= 0)
                {
                    while ((sCommandeMaj[iIndex1] != '=') && (iIndex1 < sCommandeMaj.Length) && ((sCommandeMaj[iIndex1] != '<') || (sCommandeMaj[iIndex1 + 1] != '>')))
                        iIndex1++;

                    if (iIndex1 < sCommandeMaj.Length)
                    {
                        if (sCommandeMaj[iIndex1] == '=')
                            bScriptOracle = false;
                        else
                            bScriptOracle = true;

                        iIndex1 = sCommandeMaj.IndexOf("GOTO", iIndex1);
                        if (iIndex1 >= 0)
                        {
                            iIndex1 += "GOTO".Length;
                            while (Char.IsWhiteSpace(sCommandeMaj[iIndex1]) == true) iIndex1++;
                            iIndex2 = iIndex1;
                            while ((char.IsLetterOrDigit(sCommandeMaj[iIndex2]) == true) || (sCommandeMaj[iIndex2] == '_')) iIndex2++;
                            sEtiquette = sCommandeMaj.Substring(iIndex1, iIndex2 - iIndex1);

                            iIndex2 = sCommandeMaj.IndexOf("->" + sEtiquette, iIndex2);
                            iIndex2 = sCommandeMaj.IndexOf("*/", iIndex2);
                            iIndex2 += "*/".Length;
                            // Si on doit garder ce code
                            if (bOracle == bScriptOracle)
                            {
                                iIndex1 = sCommandeMaj.IndexOf("\\", iIndex1);
                                iIndex1++;
                                sResultat += sCommande.Substring(iIndex1, iIndex2 - iIndex1);
                            }
                            iIndex1 = iIndex2;
                            if (iIndex1 == sCommandeMaj.Length)
                                bContinue = false;
                        }
                    }
                    else
                        bContinue = false;
                }
                else
                    bContinue = false;
            }
 
            return sResultat;
        }

        /// <summary>
        /// Remplace dans un script SQL une ID_ORGA par une autre
        /// </summary>
        /// <param name="sCommand">Script SQL à modifier</param>
        /// <param name="sID_OrgaOrg">ID_ORGA à remplacer</param>
        /// <param name="sID_OrgaDest">Nouvel ID_ORGA</param>
        /// <returns>Retourne le script modifié</returns>
        public string ReplaceID_ORGA(string sCommand, string sID_OrgaOrg, string sID_OrgaDest)
        {
            List<string> lID_Orga = new List<string>();
            lID_Orga.Add(sID_OrgaDest);
            return (ReplaceID_ORGA(sCommand, sID_OrgaOrg, lID_Orga));
        }

        /// <summary>
        /// Remplace dans un script SQL une ID_ORGA par une liste d'ID_ORGA
        /// </summary>
        /// <param name="sCommand">Script SQL à modifier</param>
        /// <param name="sID_OrgaOrg">ID_ORGA à remplacer</param>
        /// <param name="sID_OrgaDest">Liste des nouveaux ID_ORGA</param>
        /// <returns>Retourne le script modifié</returns>
        public string ReplaceID_ORGA(string sCommand, string sID_OrgaOrg, List<string> sID_OrgaDest)
        {
            string sORGA_COPY = string.Empty;
            string sORGA_SCRIPT = string.Empty;
            bool bPremierElement = true;
            string sResultat = sCommand;

            foreach (string orga in sID_OrgaDest)
            {
                if (bPremierElement == true)
                {
                    bPremierElement = false;
                    sORGA_COPY = "'";
                    sORGA_SCRIPT = "'";
                }
                else
                {
                    sORGA_COPY += ",";
                    sORGA_SCRIPT += ",'";
                }
                sORGA_COPY += orga;
                sORGA_SCRIPT += orga + "'";
            }
            if (sORGA_COPY.Length > 0)
                sORGA_COPY += "'";

            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "(|\\s+),(|\\s+)'" + sID_OrgaOrg + "'", "," + sORGA_COPY, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "@id_orgas_dest(|\\s+)=(|\\s+)'" + sID_OrgaOrg + "'", "@id_orgas_dest = " + sORGA_COPY, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION(|\\s+)=(|\\s+)'" + sID_OrgaOrg + "'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION\\s+LIKE(|\\s+)'" + sID_OrgaOrg + "'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION\\s+IN(|\\s+)\\((|\\s+)'" + sID_OrgaOrg + "'(|\\s+)\\)", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return sResultat;
        }

        public static string[] splitCmdCodeData(string cmdCode)
        {
            //We use an ugly text to replace \\\\ for come back to this after the split on \\
            string uglyString = "A?DJUEBISKJLERPDLZMLERJD?ZADLKJZDKD.§ZDI";
            cmdCode = cmdCode.Replace("\\\\", uglyString);
            string[] result = cmdCode.Split('\\');



            for (int i = 0; i < result.Length; i++)
            {



                result[i] = result[i].Replace(uglyString, "\\\\");
                result[i] = result[i].Trim();
                if (String.IsNullOrEmpty(result[i]))
                {
                    RemoveAt(ref result, i);
                    i--;
                }
            }
            return result;
        }



        public static string removeCommentOnCommand(string cmdCode)
        {



            int indexOpenComment = cmdCode.IndexOf("/*");
            int indexCloseComment = -1;
            if (indexOpenComment != -1)
            {
                indexCloseComment = cmdCode.IndexOf("*/", indexOpenComment);
            }
            while (indexOpenComment != -1)
            {
                cmdCode = cmdCode.Remove(indexOpenComment, (indexCloseComment - indexOpenComment) + 2); //+2 to remove the / at the end of the comment
                indexOpenComment = cmdCode.IndexOf("/*");
                if (indexOpenComment != -1)
                {
                    indexCloseComment = cmdCode.IndexOf("*/", indexOpenComment);
                }
            }




            indexOpenComment = cmdCode.IndexOf("//");
            if (indexOpenComment != -1)
            {
                indexCloseComment = cmdCode.IndexOf(System.Environment.NewLine, indexOpenComment);
            }
            while (indexOpenComment != -1)
            {
                cmdCode = cmdCode.Remove(indexOpenComment, (indexCloseComment - indexOpenComment) + 2); //+2 to remove the / at the end of the comment
                indexOpenComment = cmdCode.IndexOf("//");
                if (indexOpenComment != -1)
                {
                    indexCloseComment = cmdCode.IndexOf(System.Environment.NewLine, indexOpenComment);
                }
            }



            return cmdCode;
        }



        public static void RemoveAt<T>(ref T[] arr, int index)
        {
            for (int a = index; a < arr.Length - 1; a++)
            {
                // moving elements downwards, to fill the gap at [index]
                arr[a] = arr[a + 1];
            }
            // finally, let's decrement Array's size by one
            Array.Resize(ref arr, arr.Length - 1);
        }

        private string TestStatut(string sStatutParent, string sStatutEnfant)
        {
            string sResultat = sStatutParent;
            if (sStatutParent == ParamAppli.StatutInfo)
            {
                if (sStatutEnfant != ParamAppli.StatutInfo)
                    sResultat = sStatutEnfant;
            }
            else if (sStatutParent == ParamAppli.StatutOk)
            {
                if ((sStatutEnfant != ParamAppli.StatutOk) && (sStatutEnfant != ParamAppli.StatutInfo))
                    sResultat = sStatutEnfant;
            }
            else if ((sStatutParent == ParamAppli.StatutWarning) && (sStatutEnfant != ParamAppli.StatutError))
                sResultat = sStatutEnfant;
            return sResultat;
        }
    }
}
