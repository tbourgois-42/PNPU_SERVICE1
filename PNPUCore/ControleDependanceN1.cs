using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de détecter les tâches CCT dépendantes au niveau 1 des tâches du HF. 
    /// </summary>  
    internal class ControleDependanceN1 : PControle, IControle
    {
        readonly private PNPUCore.Process.IProcess Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleDependanceN1(PNPUCore.Process.IProcess pProcess)
        {
            Process = pProcess;
            ToolTipControle = "Récupération des tâches CCT dépendantes de niveau 1";
            LibControle = "Contrôle des dépendances de niveau 1";
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleDependanceN1(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            Process = pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        new public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            StringBuilder sRequete = new StringBuilder();
            List<string> lListeCCTManquants = new List<string>();

            DataManagerSQLServer dataManagerSQLServer;

            ParamToolbox paramToolbox = new ParamToolbox();

            string sConnectionStringBaseQA1 = paramToolbox.GetConnexionString("Before", Process.WORKFLOW_ID, Process.CLIENT_ID, Process.ID_INSTANCEWF) ;

            try
            {
                // Chargement de la liste des tâche CCT de niveau 1 depuis la base de référence
                dataManagerSQLServer = new DataManagerSQLServer();
                sRequete.Append("SELECT DISTINCT(DEP_CCT_TASK_ID) from PNPU_DEP_REF where NIV_DEP = '1' AND WORKFLOW_ID = " + Process.WORKFLOW_ID + " AND ID_H_WORKFLOW = " + Process.ID_INSTANCEWF);
                DataSet dsDataSet = dataManagerSQLServer.GetData(sRequete.ToString(), ParamAppli.ConnectionStringBaseAppli);

                sRequete.Clear();
                sRequete.Append("SELECT B.CCT_TASK_ID,A.ID_PACKAGE,A.DT_LAUNCHED from M4RDL_RAM_PACKS A,M4RDL_PACKAGES B where A.ID_PACKAGE = B.ID_PACKAGE AND B.CCT_TASK_ID IN (");
                bool bPremier = true;
                string sCCT;

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        sCCT = drRow[0].ToString();
                        lListeCCTManquants.Add(sCCT);
                        if (bPremier)
                        {
                            bPremier = false;
                        }
                        else
                        {
                            sRequete.Append(",");
                        }

                        sRequete.Append("'" + sCCT + "'");
                    }
                }
                sRequete.Append(")");

                // Recherche sur la base du client si les tâches ont été installées
                if (!bPremier)
                {
                    dsDataSet = dataManagerSQLServer.GetData(sRequete.ToString(), sConnectionStringBaseQA1);

                    // Si le dataset est à null il y a un problème de connexion à la base client
                    if (dsDataSet == null)
                    {
                        lListeCCTManquants.Clear();
                        bResultat = ResultatErreur;
                        Process.AjouteRapport("Erreur de connexion sur la base client.");
                    }

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            sCCT = drRow[0].ToString();
                            if (lListeCCTManquants.Contains(sCCT))
                            {
                                lListeCCTManquants.Remove(sCCT);
                            }
                        }
                    }
                }

                //POUR TEST
                /*lListeCCTManquants.Clear();
                lListeCCTManquants.Add("PLFR_74326");
                lListeCCTManquants.Add("PLFR_137306");*/
                //FIN POUR TEST

                // Recherche si la tâche CCT a été installée via une tâche générée par l'outil PNPU
                if (lListeCCTManquants.Count > 0)
                {
                    Dictionary<string, string> dCorrespondanceCCT = new Dictionary<string, string>();
                    sRequete.Clear();
                    sRequete.Append("SELECT CCT_TASK_ID,CCT_TASK_ID_ORG FROM PNPU_H_CCT WHERE CLIENT_ID='" + Process.CLIENT_ID + "' AND LEVEL_DEPENDANCE = 1 AND CCT_TASK_ID_ORG IN (");
                    bPremier = true;
                    foreach (string sTacheCCT in lListeCCTManquants)
                    {
                        if (bPremier)
                        {
                            bPremier = false;
                        }
                        else
                        {
                            sRequete.Append(",");
                        }

                        sRequete.Append("'" + sTacheCCT + "'");
                    }
                    sRequete.Append(")");

                    dsDataSet = dataManagerSQLServer.GetData(sRequete.ToString(), ParamAppli.ConnectionStringBaseAppli);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        sRequete.Clear();
                        sRequete.Append("SELECT B.CCT_TASK_ID,A.ID_PACKAGE,A.DT_LAUNCHED from M4RDL_RAM_PACKS A,M4RDL_PACKAGES B where A.ID_PACKAGE = B.ID_PACKAGE AND B.CCT_TASK_ID IN (");
                        bPremier = true;
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            dCorrespondanceCCT.Add(drRow[0].ToString(), drRow[1].ToString());
                            if (bPremier)
                            {
                                bPremier = false;
                            }
                            else
                            {
                                sRequete.Append(",");
                            }

                            sRequete.Append("'" + drRow[0].ToString() + "'");
                        }
                        sRequete.Append(")");

                        dsDataSet = dataManagerSQLServer.GetData(sRequete.ToString(), sConnectionStringBaseQA1);
                        {
                            foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                            {
                                sCCT = drRow[0].ToString();
                                if (lListeCCTManquants.Contains(dCorrespondanceCCT[sCCT]))
                                {
                                    lListeCCTManquants.Remove(dCorrespondanceCCT[sCCT]);
                                }
                            }
                        }
                    }
                }

                //POUR TEST
                /*lListeCCTManquants.Clear();
                lListeCCTManquants.Add("SFR_152319");*/
                //FIN POUR TEST

                // La liste ne doit contenir que les tâches CCT non instalées sur le client
                if (lListeCCTManquants.Count > 0)
                {
                    List<string> lListeTacheCrees = new List<string>();
                    if (!DupliqueTachesCCT(lListeCCTManquants, "PNPUN1_" + Process.WORKFLOW_ID.ToString("########0") + "_" + Process.CLIENT_ID, ref lListeTacheCrees))
                    {
                        bResultat = ResultatErreur;
                    }
                    else if (lListeTacheCrees.Count > 0)
                    {
                        string sName = "PNPUN1_" + Process.WORKFLOW_ID.ToString("########0") + "_" + Process.CLIENT_ID;
                        GereMDBDansBDD gereMDBDansBDD = new GereMDBDansBDD();
                        PNPUTools.RamdlTool ramdlTool = new RamdlTool(Process.CLIENT_ID, Process.WORKFLOW_ID, Process.ID_INSTANCEWF);
                        ramdlTool.GeneratePackFromCCT(sName, lListeTacheCrees.ToArray());
                        Process.AjouteRapport("Génération du fichier MDB.");
                        sName = ParamAppli.GeneratePackPath + "\\" + Process.WORKFLOW_ID + "\\" + Process.ID_INSTANCEWF + "_" + Process.CLIENT_ID + "\\" + sName + ".mdb";
                        if (gereMDBDansBDD.AjouteFichiersMDBBDD(new string[] { sName }, Process.WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli, Process.ID_INSTANCEWF, Process.CLIENT_ID, 1) == 0)
                            Process.AjouteRapport("Ajout du fichier MDB en base de données.");
                        else
                        {
                            Process.AjouteRapport("Erreur lors de l'ajout du fichier MDB en base de données.");
                            LoggerHelper.Log(Process, this, ParamAppli.StatutError, "Erreur lors de l'ajout du fichier MDB en base de données.");
                            bResultat = ParamAppli.StatutError;
                        }
                    }
                }
                else // Tous les packs de niveau 1 sont installés chez le client. On ne traite pas les niveaux suivants
                {
                    Process.StopLoop();
                    Process.AjouteRapport("Toutes les tâches dépendantes de niveau 1 sont déjà installées chez le client.");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;

        }

        public bool DupliqueTachesCCT(List<string> lListeTachesCCT, string sPrefixe, ref List<string> lListeTachesCrees)
        {
            StringBuilder sRequete = new StringBuilder();
            string sNouvTacheCCT;
            string sUserPNPU = "MT4PNPU";
            int iCptPack = 0;
            bool bResultat = true;
            try
            {
                lListeTachesCrees.Clear();
                foreach (string sTacheCCT in lListeTachesCCT)
                {
                    iCptPack++;
                    sNouvTacheCCT = sPrefixe + "_" + iCptPack.ToString("000");

                    Process.AjouteRapport("Création de la tâche CCT " + sNouvTacheCCT + " pour livrer les éléments de la tâche " + sTacheCCT);

                    using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]))
                    {
                        conn.Open();
                        sRequete.Append("DELETE FROM M4RCT_TASK WHERE CCT_TASK_ID = '" + sNouvTacheCCT + "'");
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        sRequete.Clear();
                        sRequete.Append("DELETE FROM M4RCT_OBJECTS WHERE CCT_TASK_ID = '" + sNouvTacheCCT + "'");
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        sRequete.Clear();
                        sRequete.Append("INSERT INTO M4RCT_TASK (");
                        sRequete.Append("CCT_TASK_ID");
                        sRequete.Append(",CCT_VERSION");
                        sRequete.Append(",CCT_TASK_NAMEENG");
                        sRequete.Append(",CCT_TASK_NAMEFRA");
                        sRequete.Append(",CCT_START_DATE");
                        sRequete.Append(",CCT_DESC_ENG");
                        sRequete.Append(",CCT_DESC_FRA");
                        sRequete.Append(",CCT_STATUS");
                        sRequete.Append(",CCT_RESPONSIBLE");
                        sRequete.Append(",CCT_DOC_TEC");
                        sRequete.Append(",CCT_USE_TYPE");
                        sRequete.Append(",CCT_ID_FUN_MODULE");
                        sRequete.Append(",CCT_DOC_HELP");
                        sRequete.Append(",CCT_ID_FUN_AREA");
                        sRequete.Append(",CCT_DOC_TASK");
                        sRequete.Append(",CCT_PATH_RESULT");
                        sRequete.Append(",CCT_OTHER_COMMENT");
                        sRequete.Append(",CCT_TRANSFERRED");
                        sRequete.Append(",CCT_TYPE_TASK");
                        sRequete.Append(",CCT_SERVICK_PACK");
                        sRequete.Append(",CCT_REVIEW_DOC");
                        sRequete.Append(",ID_APPROLE");
                        sRequete.Append(",ID_SECUSER");
                        sRequete.Append(",DT_LAST_UPDATE");
                        if (Process.TYPOLOGY != "Dédié") // Ce champ n'existe que sur la plateforme
                            sRequete.Append(",CCT_BUILD_LABEL");
                        sRequete.Append(") SELECT ");
                        sRequete.Append("'" + sNouvTacheCCT + "'");
                        sRequete.Append(",CCT_VERSION");
                        sRequete.Append(",'Génération auto'");
                        sRequete.Append(",'Génération auto'");
                        sRequete.Append(",GETDATE()");
                        sRequete.Append(", 'Copie de la tache " + sTacheCCT + "'");
                        sRequete.Append(", 'Copie de la tache " + sTacheCCT + "'");
                        sRequete.Append(",'3'");
                        sRequete.Append(",'" + sUserPNPU + "'");
                        sRequete.Append(",CCT_DOC_TEC");
                        sRequete.Append(",CCT_USE_TYPE");
                        sRequete.Append(",CCT_ID_FUN_MODULE");
                        sRequete.Append(",CCT_DOC_HELP");
                        sRequete.Append(",CCT_ID_FUN_AREA");
                        sRequete.Append(",CCT_DOC_TASK");
                        sRequete.Append(",CCT_PATH_RESULT");
                        sRequete.Append(",CCT_OTHER_COMMENT");
                        sRequete.Append(",CCT_TRANSFERRED");
                        sRequete.Append(",CCT_TYPE_TASK");
                        sRequete.Append(",'" + sPrefixe + "'");
                        sRequete.Append(",CCT_REVIEW_DOC");
                        sRequete.Append(",ID_APPROLE");
                        sRequete.Append(",'" + sUserPNPU + "'");
                        sRequete.Append(",GETDATE()");
                        if (Process.TYPOLOGY != "Dédié") // Ce champ n'existe que sur la plateforme
                            sRequete.Append(",CCT_BUILD_LABEL");
                        sRequete.Append(" FROM M4RCT_TASK WHERE CCT_TASK_ID='" + sTacheCCT + "'");
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        sRequete.Clear();
                        sRequete.Append("INSERT INTO M4RCT_OBJECTS(");
                        sRequete.Append("CCT_TASK_ID");
                        sRequete.Append(", CCT_VERSION");
                        sRequete.Append(", CCT_OBJECT_ID");
                        sRequete.Append(", CCT_OBJECT_TYPE");
                        sRequete.Append(", CCT_PARENT_OBJ_ID");
                        sRequete.Append(", CCT_AUX_OBJECT_ID");
                        sRequete.Append(", CCT_RULE_START_DAT");
                        sRequete.Append(", CCT_OBJ_TYPE_AUX");
                        sRequete.Append(", CCT_ACTION_TYPE");
                        sRequete.Append(", CCT_PACK_TYPE");
                        sRequete.Append(", CCT_DESCRIPTION");
                        sRequete.Append(", CCT_LAST_CHG_DATE");
                        sRequete.Append(", CCT_ORDER");
                        sRequete.Append(", CCT_USER_ID");
                        sRequete.Append(", CCT_FROM");
                        sRequete.Append(", CCT_COMMENT");
                        sRequete.Append(", CCT_COMMAND_TYPE");
                        sRequete.Append(", CCT_TRANSFER_OBJEC");
                        sRequete.Append(", CCT_BUG_ID");
                        sRequete.Append(", ID_APPROLE");
                        sRequete.Append(", ID_SECUSER");
                        sRequete.Append(", DT_LAST_UPDATE");
                        sRequete.Append(", CCT_RDL");
                        sRequete.Append(", CCT_AUX2_OBJECT_ID");
                        sRequete.Append(", CCT_AUX3_OBJECT_ID");
                        sRequete.Append(") SELECT ");
                        sRequete.Append("'" + sNouvTacheCCT + "'");
                        sRequete.Append(", CCT_VERSION");
                        sRequete.Append(", CCT_OBJECT_ID");
                        sRequete.Append(", CCT_OBJECT_TYPE");
                        sRequete.Append(", CCT_PARENT_OBJ_ID");
                        sRequete.Append(", CCT_AUX_OBJECT_ID");
                        sRequete.Append(", CCT_RULE_START_DAT");
                        sRequete.Append(", CCT_OBJ_TYPE_AUX");
                        sRequete.Append(", CCT_ACTION_TYPE");
                        sRequete.Append(", CCT_PACK_TYPE");
                        sRequete.Append(", CCT_DESCRIPTION");
                        sRequete.Append(", GETDATE()");
                        sRequete.Append(", CCT_ORDER");
                        sRequete.Append(", CCT_USER_ID");
                        sRequete.Append(", CCT_FROM");
                        sRequete.Append(", CCT_COMMENT");
                        sRequete.Append(", CCT_COMMAND_TYPE");
                        sRequete.Append(", CCT_TRANSFER_OBJEC");
                        sRequete.Append(", CCT_BUG_ID");
                        sRequete.Append(", ID_APPROLE");
                        sRequete.Append(",'" + sUserPNPU + "'");
                        sRequete.Append(", GETDATE()");
                        sRequete.Append(", CCT_RDL");
                        sRequete.Append(", CCT_AUX2_OBJECT_ID");
                        sRequete.Append(", CCT_AUX3_OBJECT_ID");
                        sRequete.Append(" FROM M4RCT_OBJECTS WHERE CCT_TASK_ID='" + sTacheCCT + "'");
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete.ToString(), conn))
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                lListeTachesCrees.Add(sNouvTacheCCT);
                            }
                        }
                    }

                    using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                    {
                        conn.Open();
                        sRequete.Clear();
                        sRequete.Append("DELETE FROM PNPU_H_CCT WHERE CCT_TASK_ID = '" + sNouvTacheCCT + "'");
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        sRequete.Clear();
                        sRequete.Append("INSERT INTO PNPU_H_CCT (");
                        sRequete.Append("CCT_TASK_ID");
                        sRequete.Append(",CCT_TASK_ID_ORG");
                        sRequete.Append(",WORKFLOW_ID");
                        sRequete.Append(",CLIENT_ID");
                        sRequete.Append(",LEVEL_DEPENDANCE");
                        sRequete.Append(",ID_H_WORKFLOW");
                        sRequete.Append(") VALUES ( ");
                        sRequete.Append("'" + sNouvTacheCCT + "'");
                        sRequete.Append(",'" + sTacheCCT + "'");
                        sRequete.Append("," + Process.WORKFLOW_ID);
                        sRequete.Append(",'" + Process.CLIENT_ID + "'");
                        sRequete.Append(",'1'");
                        sRequete.Append("," + Process.ID_INSTANCEWF);
                        sRequete.Append(")");
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete.ToString(), conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, Ex.Message);
                bResultat = false;
            }
            return bResultat;
        }


    }
}


