using PNPUCore.Process;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleDataGeneric
    {
        readonly ProcessAnalyseImpactData processAnalyseImpactData;

        public ControleDataGeneric(ProcessAnalyseImpactData pProcessAnalyseImpact)
        {
            processAnalyseImpactData = pProcessAnalyseImpact;
        }

        public void AnalyzeCommand(string commandeLine, ref CommandData commandData, ref EltsALocaliserData eltsALocaliserData, RmdCommandData rmdCommandData)
        {
            string sTable = String.Empty;
            string sFilter = String.Empty;
            string sRequeteRef;
            string sRequeteClient;
            DataSet dsDataSetRef;
            DataSet dsDataSetClient;
            string sConnectionString;
            DataManagerSQLServer dmsDataManager = new DataManagerSQLServer();
            string sFiltreRef;
            string sFiltreClient;
            bool bIdentique;
            string sOrgaCour;
            List<string> lColumnsList = new List<string>();
            string sOrgaOrg;
            string sOrgaOrgFiltre;
            bool bFlagUpdate;
            string sCommandeGeneree;

            dmsDataManager.ExtractTableFilter(commandeLine, ref sTable, ref sFilter, ref lColumnsList);

            sOrgaCour = ParamAppli.ListeInfoClient[processAnalyseImpactData.CLIENT_ID].ID_ORGA;
            sConnectionString = ParamAppli.ListeInfoClient[processAnalyseImpactData.CLIENT_ID].ConnectionStringQA1;

            // On traite les commandes pour lesquelles on peut extraire la table
            if ((sTable != String.Empty) && !(processAnalyseImpactData.lListPersonnalTables.Contains(sTable)))
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
                    sFiltreClient = dmsDataManager.ReplaceID_ORGA(sFilter, sOrgaOrgFiltre, sOrgaCour);
                }
                else
                {
                    if (sFilter != string.Empty)
                        sFilter = " AND ";
                    sFiltreRef = sFilter + " ID_ORGANIZATION='0001'";
                    sFiltreClient = sFilter + " ID_ORGANIZATION='" + sOrgaCour + "'";
                }

                sRequeteRef = "SELECT * FROM " + sTable + " WHERE " + sFiltreRef;
                sRequeteClient = "SELECT * FROM " + sTable + " WHERE " + sFiltreClient;
                dsDataSetRef = dmsDataManager.GetData(sRequeteRef, sConnectionString);
                dsDataSetClient = dmsDataManager.GetData(sRequeteClient, sConnectionString);

                if ((dsDataSetRef != null) && (dsDataSetClient != null))
                {

                    // Pas le même nombre de ligne, à traiter à la main.
                    if (dsDataSetRef.Tables[0].Rows.Count != dsDataSetClient.Tables[0].Rows.Count)
                    {
                        commandData.Result = ParamAppli.StatutError;
                        if (dsDataSetClient.Tables[0].Rows.Count == 0)
                        {
                            commandData.Message = "Ces données existaient sur le référentiel mais pas sur le client. Commande à traiter à la main.";
                        }
                        else if (dsDataSetRef.Tables[0].Rows.Count == 0)
                        {
                            commandData.Message = "Ces données existaient sur le client mais pas sur le référentiel. Commande à traiter à la main.";
                        }
                        else
                        {
                            commandData.Message = "Nombre différents d'enregistrements entre référentiel et client. Commande à traiter à la main.";
                        }
                        eltsALocaliserData.Result = commandData.Result;
                        eltsALocaliserData.Name = commandData.Name;
                        eltsALocaliserData.Message = commandData.Message;
                        sCommandeGeneree = dmsDataManager.GenerateReplace(sTable, sFilter, sOrgaOrgFiltre, sOrgaCour);
                        RequestTool.addLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);

                    }
                    else
                    {
                        if (dsDataSetRef.Tables[0].Rows.Count == 0)
                        {
                            commandData.Message = "Les données n'existaient pas avant ce HF. La commande peut être exécutée.";
                            commandData.Result = ParamAppli.StatutOk;
                            eltsALocaliserData.Result = commandData.Result;
                            eltsALocaliserData.Name = commandData.Name;
                            eltsALocaliserData.Message = commandData.Message;
                            sCommandeGeneree = dmsDataManager.ReplaceID_ORGA(commandeLine, sOrgaOrg, sOrgaCour);
                            RequestTool.addLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);
                        }
                        else
                        {
                            bIdentique = true;
                            List<string> ListColumsDif = new List<string>();
                            bFlagUpdate = false;
                            for (int iIndex = 0; ((iIndex < dsDataSetRef.Tables[0].Rows.Count) && bIdentique); iIndex++)
                            {
                                if (!dmsDataManager.CompareRows(dsDataSetRef.Tables[0].Rows[iIndex], dsDataSetRef.Tables[0], dsDataSetClient.Tables[0].Rows[iIndex], dsDataSetClient.Tables[0], processAnalyseImpactData.dListeTablesFieldsIgnore, ref ListColumsDif))
                                {
                                    // Si on est dans un update on croise la liste des champs en écarts et la liste des champs modifiés
                                    if (lColumnsList.Count > 0)
                                    {
                                        bFlagUpdate = true;
                                        foreach (string sChamp in ListColumsDif)
                                        {
                                            // Vérifie si les champs en écarts sont dans 
                                            if (lColumnsList.Contains(sChamp))
                                                bIdentique = false;
                                        }
                                    }
                                    else
                                        bIdentique = false;
                                }
                            }
                            if (bIdentique)
                            {
                                if (bFlagUpdate)
                                    commandData.Message = "Les champs mis à jour dans le script ont la valeur standard. La commande peut être exécutée.";
                                else
                                    commandData.Message = "Les données du client sont standards. La commande peut être exécutée.";
                                commandData.Result = ParamAppli.StatutOk;
                                sCommandeGeneree = dmsDataManager.ReplaceID_ORGA(commandeLine, sOrgaOrg, sOrgaCour);
                                RequestTool.addLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);

                            }
                            else
                            {
                                commandData.Message = "Les données du client contiennent du spécifique. Commande à traiter à la main.";
                                commandData.Result = ParamAppli.StatutError;
                                sCommandeGeneree = dmsDataManager.GenerateReplace(sTable, sFilter, sOrgaOrgFiltre, sOrgaCour);
                                RequestTool.addLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);

                            }
                            eltsALocaliserData.Result = commandData.Result;
                            eltsALocaliserData.Name = commandData.Name;
                            eltsALocaliserData.Message = commandData.Message;
                        }
                    }
                }
                else
                {
                    commandData.Message = "Pas de contrôle automatique sur cette commande.";
                    commandData.Result = ParamAppli.StatutInfo;
                    eltsALocaliserData.Result = commandData.Result;
                    eltsALocaliserData.Name = commandData.Name;
                    eltsALocaliserData.Message = commandData.Message;
                    sCommandeGeneree = dmsDataManager.ReplaceID_ORGA(commandeLine, sOrgaOrg, sOrgaCour);
                    RequestTool.addLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, sCommandeGeneree, processAnalyseImpactData.ID_INSTANCEWF);
                }
            }
            else
            {
                commandData.Message = "Pas de contrôle automatique sur cette commande.";
                commandData.Result = ParamAppli.StatutInfo;
                eltsALocaliserData.Result = commandData.Result;
                eltsALocaliserData.Name = commandData.Name;
                eltsALocaliserData.Message = commandData.Message;
                RequestTool.addLocalisationByALineAnalyseData(processAnalyseImpactData.CLIENT_ID, processAnalyseImpactData.WORKFLOW_ID, rmdCommandData.IdCCTTask, rmdCommandData.IdObject, commandeLine, processAnalyseImpactData.ID_INSTANCEWF);
            }
        }
    }
}
