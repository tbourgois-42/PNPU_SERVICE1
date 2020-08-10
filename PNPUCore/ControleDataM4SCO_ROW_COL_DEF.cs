using PNPUCore.Process;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleDataM4SCO_ROW_COL_DEF
    {
        readonly private List<string> TableTraitee = new List<string>() { "M4SCO_ROW_COL_DEF", "M4SCO_ROWS" };
        readonly ProcessAnalyseImpactData processAnalyseImpactData;
        private string sOrgaCour;
        private string sConnectionString;
        private DataManagerSQLServer dmsDataManager;
        CommandData commandDataCour;


        public ControleDataM4SCO_ROW_COL_DEF(ProcessAnalyseImpactData processAnalyseImpact)
        {
            processAnalyseImpactData = processAnalyseImpact;
        }

        public void AnalyzeCommand(string commandeLine, ref CommandData commandData, ref EltsALocaliserData eltsALocaliserData, RmdCommandData rmdCommandData)
        {
            string sTable = String.Empty;
            string sFilter = String.Empty;
            string sRequeteRef;
            //Variable is not use string sRequeteClient;
            DataSet dsDataSetRef;
            //Variable is not use DataSet dsDataSetClient;
            string sFiltreRef;
            //Variable is not use string sFiltreClient;
            string sOrgaOrg;
            string sOrgaOrgFiltre;
            //ControleCommandData controleCommandData;
            List<string> lColumnsList = new List<string>();
            string sCommandeGeneree;

            dmsDataManager = new DataManagerSQLServer();
            dmsDataManager.ExtractTableFilter(commandeLine, ref sTable, ref sFilter, ref lColumnsList);

            if (!TableTraitee.Contains(sTable)) return;
            sOrgaCour = ParamAppli.ListeInfoClient[processAnalyseImpactData.CLIENT_ID].ID_ORGA;
            sConnectionString = ParamAppli.ListeInfoClient[processAnalyseImpactData.CLIENT_ID].ConnectionStringQA1;
            commandDataCour = commandData;


            // On traite une commande de propagation
            if ((commandeLine.IndexOf("M4SFR_COPY_DATA_ORG") >= 0) || (commandeLine.ToUpper().IndexOf("DELETE") >= 0) || (commandeLine.ToUpper().IndexOf("UPDATE") >= 0))
            {
                if (processAnalyseImpactData.TYPOLOGY == "Dédié")
                    sOrgaOrg = "0002";
                else
                    sOrgaOrg = "9999";

                if (commandeLine.IndexOf("M4SFR_COPY_DATA_ORG") >= 0)
                {
                    sOrgaOrgFiltre = "0001";
                }
                else
                {
                    sOrgaOrgFiltre = sOrgaOrg;
                }

                if (sFilter.IndexOf("ID_ORGA") >= 0)
                {
                    sFiltreRef = dmsDataManager.ReplaceID_ORGA(sFilter, sOrgaOrgFiltre, "0001");
                    //Variable is not use sFiltreClient = dmsDataManager.ReplaceID_ORGA(sFilter, sOrgaOrgFiltre, sOrgaCour);
                }
                else
                {
                    if (sFilter != string.Empty)
                        sFilter = " AND ";
                    sFiltreRef = sFilter + " ID_ORGANIZATION='0001'";
                    //Variable is not use sFiltreClient = sFilter + " ID_ORGANIZATION='" + sOrgaCour + "'";
                }

                sRequeteRef = "SELECT * FROM " + sTable + " WHERE " + sFiltreRef;
                //Variable is not use sRequeteClient = "SELECT * FROM " + sTable + " WHERE " + sFiltreClient;
                dsDataSetRef = dmsDataManager.GetData(sRequeteRef, sConnectionString);
                //Variable is not use dsDataSetClient = dmsDataManager.GetData(sRequeteClient, sConnectionString);

                if ((dsDataSetRef != null) && (dsDataSetRef.Tables[0].Rows.Count > 0))
                {
                    string sSCO_ID_REPORT = String.Empty;
                    string sSCO_ID_BODY = String.Empty;
                    string sSCO_ID_ROW = String.Empty;
                    // Cas particulier du paramétrage client sur une autre ligne
                    foreach (DataRow drRowRef in dsDataSetRef.Tables[0].Rows)
                    {
                        if ((sSCO_ID_REPORT != drRowRef[1].ToString()) || (sSCO_ID_BODY != drRowRef[2].ToString()) || (sSCO_ID_ROW != drRowRef[3].ToString()))
                        {
                            sSCO_ID_REPORT = drRowRef[1].ToString();
                            sSCO_ID_BODY = drRowRef[2].ToString();
                            sSCO_ID_ROW = drRowRef[3].ToString();

                            if (!ControleAutreLigne(sSCO_ID_REPORT, sSCO_ID_BODY, sSCO_ID_ROW))
                            {
                                sCommandeGeneree = dmsDataManager.GenerateReplace(sTable, sFilter, sOrgaOrgFiltre, sOrgaCour);
                                RequestTool.AddLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);

                            }
                        }
                    }
                }
                eltsALocaliserData.Name = commandDataCour.Name;
                eltsALocaliserData.Message = commandDataCour.Message;
                eltsALocaliserData.Result = commandDataCour.Result;
            }
        }

        private bool ControleAutreLigne(string sSCO_ID_REPORT, string sSCO_ID_BODY, string sSCO_ID_ROW)
        {
            bool bResultat = true;
            string sSCO_ID_PRT_ITEM = String.Empty;
            string sSFR_ID_SOURCE_ITEM = String.Empty;

            string sSCO_ID_BODY_CLIENT;
            string sSCO_ID_ROW_CLIENT;


            string sRequete = "SELECT * FROM M4SCO_ROW_COL_DEF WHERE ID_ORGANIZATION = '0001' AND SCO_ID_REPORT = '" + sSCO_ID_REPORT + "' AND SCO_ID_BODY = '" + sSCO_ID_BODY + "' AND SCO_ID_ROW = '" + sSCO_ID_ROW + "' AND (SCO_ID_PRT_ITEM IS NOT NULL OR SFR_ID_SOURCE_ITEM IS NOT NULL)";
            DataSet dsDataset = dmsDataManager.GetData(sRequete, sConnectionString);

            if ((dsDataset != null) && (dsDataset.Tables[0].Rows.Count > 0))
            {
                sSCO_ID_PRT_ITEM = dmsDataManager.GetFieldValue(dsDataset.Tables[0].Rows[0], dsDataset.Tables[0], "SCO_ID_PRT_ITEM");
                sSFR_ID_SOURCE_ITEM = dmsDataManager.GetFieldValue(dsDataset.Tables[0].Rows[0], dsDataset.Tables[0], "SFR_ID_SOURCE_ITEM");

                sRequete = "SELECT * FROM M4SCO_ROW_COL_DEF WHERE ID_ORGANIZATION = '" + sOrgaCour + "' AND SCO_ID_REPORT = '" + sSCO_ID_REPORT + "' AND (SCO_ID_BODY <> '" + sSCO_ID_BODY + "' OR SCO_ID_ROW<>'" + sSCO_ID_ROW + "') ";
                if (sSCO_ID_PRT_ITEM != string.Empty)
                    sRequete += "AND SCO_ID_PRT_ITEM = '" + sSCO_ID_PRT_ITEM + "'";
                else
                    sRequete += "AND SFR_ID_SOURCE_ITEM = '" + sSFR_ID_SOURCE_ITEM + "'";

                dsDataset = dmsDataManager.GetData(sRequete, sConnectionString);
                if ((dsDataset != null) && (dsDataset.Tables[0].Rows.Count > 0) )
                {
                    commandDataCour.Result = ParamAppli.StatutError;
                    bResultat = false;
                    if (commandDataCour.Message != String.Empty)
                        commandDataCour.Message += " - ";

                    sSCO_ID_BODY_CLIENT = dmsDataManager.GetFieldValue(dsDataset.Tables[0].Rows[0], dsDataset.Tables[0], "SCO_ID_BODY");
                    sSCO_ID_ROW_CLIENT = dmsDataManager.GetFieldValue(dsDataset.Tables[0].Rows[0], dsDataset.Tables[0], "SCO_ID_ROW");
                    commandDataCour.Message += "Paramétrage spécifique client à un emplacement différent du standard (";
                    if (sSCO_ID_BODY_CLIENT != sSCO_ID_BODY)
                    {
                        commandDataCour.Message += "SCO_ID_BODY = " + sSCO_ID_BODY_CLIENT;
                        if (sSCO_ID_ROW_CLIENT != sSCO_ID_ROW)
                            commandDataCour.Message += " / SCO_ID_ROW = " + sSCO_ID_ROW_CLIENT;
                    }
                    else
                        commandDataCour.Message += "SCO_ID_ROW = " + sSCO_ID_ROW_CLIENT;

                    commandDataCour.Message += ").";
                }
            }
            return bResultat;
        }
    }
}
