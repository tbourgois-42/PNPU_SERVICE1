﻿using PNPUCore.Process;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    internal class ControleDataTablesDSN
    {
        private readonly ProcessAnalyseImpactData processAnalyseImpactData;
        private string sOrgaCour;
        private DataManagerSQLServer dmsDataManager;
        private CommandData commandDataCour;


        public ControleDataTablesDSN(ProcessAnalyseImpactData processAnalyseImpact)
        {
            processAnalyseImpactData = processAnalyseImpact;
        }

        public void AnalyzeCommand(string commandeLine, ref CommandData commandData, ref EltsALocaliserData eltsALocaliserData, RmdCommandData rmdCommandData)
        {
            string sTable = String.Empty;
            string sFilter = String.Empty;
            string sRequeteRef;
            string sRequeteClient;
            DataSet dsDataSetRef;
            DataSet dsDataSetClient;
            string sFiltreRef;
            // VARIABLE NOT USE string sFiltreClient;
            string sOrgaOrg;
            //ControleCommandData controleCommandData;
            List<string> lColumnsList = new List<string>();
            // VARIABLE NOT USE string sFilterTraite;
            List<string> lPKFields = new List<string>();
            bool bSFR_CK_IS_ACTIF = false;
            string sFiltreSuite;
            string sOrgaOrgFiltre;
            string sCommandeGeneree;
            bool bFlagCommandAjoutee = false;
            string sConnectionString;

            dmsDataManager = new DataManagerSQLServer();
            dmsDataManager.ExtractTableFilter(commandeLine, ref sTable, ref sFilter, ref lColumnsList);

            sOrgaCour = ParamAppli.ListeInfoClient[processAnalyseImpactData.CLIENT_ID].ID_ORGA;
            sConnectionString = ParamAppli.ListeInfoClient[processAnalyseImpactData.CLIENT_ID].ConnectionStringQA1;
            commandDataCour = commandData;

            dmsDataManager.GetPKFields(sTable, sConnectionString, ref lPKFields);

            // On traite une commande de propagation
            if ((commandeLine.IndexOf("M4SFR_COPY_DATA_ORG") >= 0) || (commandeLine.ToUpper().IndexOf("DELETE") >= 0) || (commandeLine.ToUpper().IndexOf("UPDATE") >= 0))
            {
                // VARIABLE NOT USE sFilterTraite = SupprimerChampFiltre(sFilter, "SFR_ID_ORIG_PARAM");
                // VARIABLE NOT USE sFilterTraite = SupprimerChampFiltre(sFilterTraite, "SFR_CK_IS_ACTIF");


                if (processAnalyseImpactData.TYPOLOGY == "Dédié")
                {
                    sOrgaOrg = "0002";
                }
                else
                {
                    sOrgaOrg = "9999";
                }

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
                    // VARIABLE NOT USE sFiltreClient = dmsDataManager.ReplaceID_ORGA(sFilter, sOrgaOrgFiltre, sOrgaCour);
                }
                else
                {
                    if (sFilter != string.Empty)
                    {
                        sFilter = " AND ";
                    }

                    sFiltreRef = sFilter + " ID_ORGANIZATION='0001'";
                    // VARIABLE NOT USE sFiltreClient = sFilter + " ID_ORGANIZATION='" + sOrgaCour + "'";
                }

                // VARIABLE NOT USE sFiltreClient += " AND SFR_ID_ORIG_PARAM = 'CLI'";


                sRequeteRef = "SELECT * FROM " + sTable + " WHERE " + sFiltreRef;
                dsDataSetRef = dmsDataManager.GetData(sRequeteRef, sConnectionString);

                if ((dsDataSetRef != null) && (dsDataSetRef.Tables[0].Rows.Count > 0))
                {
                    bSFR_CK_IS_ACTIF = dmsDataManager.ExistsField(dsDataSetRef.Tables[0], "SFR_CK_IS_ACTIF");
                    // Cas particulier du paramétrage client sur une autre ligne
                    foreach (DataRow drRowRef in dsDataSetRef.Tables[0].Rows)
                    {
                        string sSFR_ID_ORIG_PARAM_REF;
                        string sSFR_CK_IS_ACTIF_REF;
                        sSFR_ID_ORIG_PARAM_REF = dmsDataManager.GetFieldValue(drRowRef, dsDataSetRef.Tables[0], "SFR_ID_ORIG_PARAM");
                        if (bSFR_CK_IS_ACTIF)
                        {
                            sSFR_CK_IS_ACTIF_REF = dmsDataManager.GetFieldValue(drRowRef, dsDataSetRef.Tables[0], "SFR_CK_IS_ACTIF");
                        }
                        else
                        {
                            sSFR_CK_IS_ACTIF_REF = "1";
                        }

                        if ((sSFR_CK_IS_ACTIF_REF == "1") && (sSFR_ID_ORIG_PARAM_REF == "STD"))
                        {
                            sRequeteClient = "SELECT * FROM " + sTable + " WHERE ID_ORGANIZATION ='" + sOrgaCour + "' ";
                            sFiltreSuite = " AND NOT EXISTS (SELECT * FROM " + sTable + " WHERE ID_ORGANIZATION ='0001' ";
                            foreach (string sField in lPKFields)
                            {
                                if (!processAnalyseImpactData.dListeTablesFieldsIgnore[sTable].Contains(sField) && (sField != "SFR_ID_ORIG_PARAM") && (sField != "SFR_CK_IS_ACTIF"))
                                {
                                    sRequeteClient += " AND " + sField + "='" + dmsDataManager.GetFieldValue(drRowRef, dsDataSetRef.Tables[0], sField) + "'";
                                    sFiltreSuite += " AND " + sField + "='" + dmsDataManager.GetFieldValue(drRowRef, dsDataSetRef.Tables[0], sField) + "'";
                                }
                            }
                            sRequeteClient += " AND SFR_ID_ORIG_PARAM = 'CLI'";
                            sFiltreSuite += " AND SFR_ID_ORIG_PARAM = 'CLI'";
                            if (bSFR_CK_IS_ACTIF)
                            {
                                sRequeteClient += " AND SFR_CK_IS_ACTIF='1'";
                                sFiltreSuite += " AND SFR_CK_IS_ACTIF='1'";
                            }

                            sFiltreSuite += ")";
                            sRequeteClient += sFiltreSuite;
                            dsDataSetClient = dmsDataManager.GetData(sRequeteClient, sConnectionString);
                            if ((dsDataSetClient != null) && (dsDataSetClient.Tables[0].Rows.Count > 0))
                            {
                                commandDataCour.Name = commandeLine;
                                commandDataCour.Message = "Les données du client contiennent du spécifique (CLI). Commande à traiter à la main.";
                                commandDataCour.Result = ParamAppli.StatutError;
                                eltsALocaliserData.Name = commandDataCour.Name;
                                eltsALocaliserData.Message = commandDataCour.Message;
                                eltsALocaliserData.Result = commandDataCour.Result;
                                if (!bFlagCommandAjoutee)
                                {
                                    bFlagCommandAjoutee = true;
                                    sCommandeGeneree = dmsDataManager.GenerateReplace(sTable, sFilter, sOrgaOrgFiltre, sOrgaCour);
                                    RequestTool.AddLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);
                                }

                            }
                        }

                    }
                }

            }
        }

        private string SupprimerChampFiltre(string sFilter, string sChampASupprimer)
        {
            string sResultat = sFilter;
            int iIndex = sFilter.IndexOf(sChampASupprimer);
            int iIndex2;
            bool bPremier = true;

            if (iIndex >= 0)
            {
                if (iIndex > 0)
                {
                    iIndex2 = iIndex;
                    iIndex--;

                    while (Char.IsWhiteSpace(sFilter[iIndex]))
                    {
                        iIndex--;
                    }

                    while (Char.IsLetter(sFilter[iIndex]))
                    {
                        iIndex--;
                    }

                    sResultat = sFilter.Substring(0, iIndex);
                    bPremier = false;
                    iIndex = iIndex2;
                }
                else
                {
                    sResultat = string.Empty;
                }

                iIndex = iIndex + sChampASupprimer.Length;
                while ((Char.IsWhiteSpace(sFilter[iIndex])) || (sFilter[iIndex] == '='))
                {
                    iIndex++;
                }

                while (((Char.IsLetter(sFilter[iIndex])) || (sFilter[iIndex] == '\'')) && (iIndex < sFilter.Length - 1))
                {
                    iIndex++;
                }

                if (iIndex < sFilter.Length - 1)
                {
                    if (bPremier)
                    {
                        while (Char.IsWhiteSpace(sFilter[iIndex]))
                        {
                            iIndex++;
                        }

                        while (Char.IsLetter(sFilter[iIndex]))
                        {
                            iIndex++;
                        }
                    }

                    sResultat += sFilter.Substring(iIndex);
                }
            }

            return (sResultat);
        }
    }
}

