using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler les dépendances entre les tâches CCT livrées dans le HF et tâches existantes sur la base de référence. 
    /// </summary>  
    internal class ControleRechercheDependancesRef : PControle, IControle
    {
        readonly private PNPUCore.Process.IProcess Process;
        readonly private string ConnectionStringBaseRef;
        private string sCCTIgnore = string.Empty;


        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleRechercheDependancesRef(PNPUCore.Process.IProcess pProcess)
        {
            Process = pProcess;
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
            ToolTipControle = "Détection des tâches dépendantes";
            LibControle = "Détection des tâches CCT dépendantes sur la base de référence";

        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        new public string MakeControl()
        {
            bool bResultat;
            string sResultat = ParamAppli.StatutOk;
            string sRequete;
            DataManagerAccess dmaManagerAccess;
            DataSet dsDataSet;
            string sTacheCCT;
            List<string> lTacheCCTHF;
            bool bPremierElement = true;
            bool bPremierElementMDB;
            StringBuilder sbFiltreMdb = new StringBuilder();
            string sFiltreNiveauPrec = string.Empty;
            StringBuilder sFiltreNiveauN = new StringBuilder();
            StringBuilder sFiltreNiveauN1 = new StringBuilder();
            List<string> lListCCTIgnore;
            StringBuilder sbCCTIgnore = new StringBuilder();

            try
            {
                dmaManagerAccess = new DataManagerAccess();
                lTacheCCTHF = new List<string>();

                // Récupération de la liste des tâches CCT à ignorer en focntion de la typologie du client
                if (Process.TYPOLOGY == "Dédié")
                    lListCCTIgnore = ParamAppli.ListeCCTIgD;
                else
                    lListCCTIgnore = ParamAppli.ListeCCTIgP;
                foreach (string sCCT in lListCCTIgnore)
                {
                    if (bPremierElement)
                        bPremierElement = false;
                    else
                        sbCCTIgnore.Append(",");
                    sbCCTIgnore.Append("'");
                    sbCCTIgnore.Append(sCCT);
                    sbCCTIgnore.Append("'");
                }
                sCCTIgnore = sbCCTIgnore.ToString();
                bPremierElement = true;

                // Récupération de toutes les tâches CCT livrées dans le HF
                sRequete = "SELECT DISTINCT(CCT_TASK_ID) FROM M4RDL_PACKAGES";
                foreach (string sPathMdb in Process.listMDB)
                {
                    bPremierElementMDB = true;
                    sbFiltreMdb.Clear();
                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            sTacheCCT = drRow[0].ToString();
                            if ((sTacheCCT != string.Empty) && (!lTacheCCTHF.Contains(sTacheCCT)))
                            {
                                lTacheCCTHF.Add(sTacheCCT);
                                if (bPremierElement)
                                {
                                    bPremierElement = false;
                                }
                                else
                                {
                                    sFiltreNiveauN.Append(",");
                                }

                                sFiltreNiveauN.Append("'" + sTacheCCT + "'");

                                if (bPremierElementMDB)
                                    bPremierElementMDB = false;
                                else
                                    sbFiltreMdb.Append(",");
                                sbFiltreMdb.Append("'");
                                sbFiltreMdb.Append(sTacheCCT);
                                sbFiltreMdb.Append("'");
                            }

                        }
                    }
                    AjouteCCT(sbFiltreMdb.ToString(), sPathMdb);
                }

                

                // Recherche des dépendances de Niveau 1
                bResultat = RechercheDependances(1, sFiltreNiveauPrec, sFiltreNiveauN.ToString(), ref sFiltreNiveauN1);

                // Recherche des dépendances de Niveau 2
                if (bResultat)
                {
                    sFiltreNiveauPrec = sFiltreNiveauN.ToString();
                    sFiltreNiveauN.Clear();
                    sFiltreNiveauN.Append(sFiltreNiveauN1);
                    sFiltreNiveauN1.Clear();
                    bResultat = RechercheDependances(2, sFiltreNiveauPrec, sFiltreNiveauN.ToString(), ref sFiltreNiveauN1);
                }

                // Recherche des dépendances de Niveau 3
                if (bResultat)
                {
                    if (sFiltreNiveauPrec != string.Empty)
                    {
                        sFiltreNiveauPrec += ",";
                    }

                    sFiltreNiveauPrec += sFiltreNiveauN;
                    sFiltreNiveauN.Clear();
                    sFiltreNiveauN.Append(sFiltreNiveauN1);
                    sFiltreNiveauN1.Clear();
                    bResultat = RechercheDependances(3, sFiltreNiveauPrec, sFiltreNiveauN.ToString(), ref sFiltreNiveauN1);
                }
                if (!bResultat)
                {
                    sResultat = ParamAppli.StatutError;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, ex.Message);
                sResultat = ParamAppli.StatutError;
            }

            return sResultat;

        }

        /// <summary>
        /// Ajout des tâches CCT livrées dans le MDB dans les tables M4RCT_TASK et M4RCT_OBJECTS
        /// </summary>
        /// <param name="sFiltreCCT">Liste des tâches CCT du mdb au format d'un IN dans un filtre SQL</param>
        /// <param name="sPathMDB">Chemin complet du fichier MDB</param>
        private void AjouteCCT(string sFiltreCCT, string sPathMDB)
        {
            DataManagerAccess dmaManagerAccess = new DataManagerAccess();
            DataSet dsDataSetMDB;
            DataManagerSQLServer dmsqlManagerSQL = new DataManagerSQLServer();
            DataSet dsDataSetSQL;
            string sRequete ;
            DataRow drNouvelleLigne;
            StringBuilder sB = new StringBuilder();

            try
            {
                // Suppression des tâcheq CCT de la table M4RCT_TASK de la base de référence
                sB.AppendFormat("DELETE FROM M4RCT_TASK WHERE CCT_TASK_ID IN ({0})", sFiltreCCT);
                sRequete = sB.ToString();
                using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                // Recopies des données des tâcheq CCT de la table M4RCT_TASK du MDB à la base de référence
                sB.Clear();
                sB.AppendFormat("SELECT * FROM M4RCT_TASK WHERE CCT_TASK_ID IN ({0})", sFiltreCCT);
                sRequete = sB.ToString();

                dsDataSetMDB = dmaManagerAccess.GetData(sRequete, sPathMDB);

                if ((dsDataSetMDB != null) && (dsDataSetMDB.Tables[0].Rows.Count > 0))
                {

                    using (SqlConnection connection = new SqlConnection(ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]))
                    {
                        connection.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(sRequete, connection);
                        SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                        builder.GetInsertCommand();

                        adapter.SelectCommand.CommandTimeout = 300;
                        dsDataSetSQL = new DataSet();
                        adapter.Fill(dsDataSetSQL/*, "M4RCT_TASK"*/);
                        
                        foreach (DataRow drRow in dsDataSetMDB.Tables[0].Rows)
                        {
                            drNouvelleLigne = dsDataSetSQL.Tables[0].NewRow();
                            for (int iIndex = 0; iIndex < dsDataSetMDB.Tables[0].Columns.Count - 1; iIndex++)
                            {
                                drNouvelleLigne[dsDataSetMDB.Tables[0].Columns[iIndex].ColumnName] = drRow[dsDataSetMDB.Tables[0].Columns[iIndex].ColumnName];
                            }
                            dsDataSetSQL.Tables[0].Rows.Add(drNouvelleLigne);
                        }
                        adapter.Update(dsDataSetSQL);
                    }
                   
                }

                // Suppression des tâcheq CCT de la table M4RCT_OBJECTS de la base de référence
                sB.AppendFormat("DELETE FROM M4RCT_OBJECTS WHERE CCT_TASK_ID IN ({0})", sFiltreCCT);
                sRequete = sB.ToString();
                using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }


                // Recopies des données des tâcheq CCT de la table M4RCT_OBJECTS du MDB à la base de référence
                sB.Clear();
                sB.AppendFormat("SELECT * FROM M4RCT_OBJECTS WHERE CCT_TASK_ID IN ({0})", sFiltreCCT);
                sRequete = sB.ToString();

                dsDataSetMDB = dmaManagerAccess.GetData(sRequete, sPathMDB);

                if ((dsDataSetMDB != null) && (dsDataSetMDB.Tables[0].Rows.Count > 0))
                {

                    using (SqlConnection connection = new SqlConnection(ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]))
                    {
                        connection.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(sRequete, connection);
                        SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                        builder.GetInsertCommand();

                        adapter.SelectCommand.CommandTimeout = 300;
                        dsDataSetSQL = new DataSet();
                        adapter.Fill(dsDataSetSQL/*, "M4RCT_OBJECTS"*/);

                        foreach (DataRow drRow in dsDataSetMDB.Tables[0].Rows)
                        {
                            drNouvelleLigne = dsDataSetSQL.Tables[0].NewRow();
                            for (int iIndex = 0; iIndex < dsDataSetMDB.Tables[0].Columns.Count - 1; iIndex++)
                            {
                                drNouvelleLigne[dsDataSetMDB.Tables[0].Columns[iIndex].ColumnName] = drRow[dsDataSetMDB.Tables[0].Columns[iIndex].ColumnName];
                            }
                            dsDataSetSQL.Tables[0].Rows.Add(drNouvelleLigne);
                        }
                        adapter.Update(dsDataSetSQL);
                    }

                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, ex.Message);
            }

        }

        /// <summary>
        /// Le but de cette fonction est de déterminer les dépendances de de niveau 1 pour les packs livrés dans le HF.
        /// On détecte les tâches CCT dépendantes car au niveau 1 on va récupérer tout le contenu des tâches dépendantes qui manquent sur l'environnement client.
        /// </summary>
        /// <param name="iNiveau">Niveau de dépendance recherché.</param>
        /// <param name="sFiltreNiveauxPrec">Liste des tâches CCT des niveaux précédents, pour ne pas avoir de dépendance cyclique</param>
        /// <param name="sFiltreNiveauN">Liste des tâche CCT pour lesquelles on cherche les dépendances</param>
        /// <param name="sFiltreNiveauN1">Au retour contient la liste des tâches CCT dépendantes</param>
        /// <returns></returns>
        private bool RechercheDependances(int iNiveau, string sFiltreNiveauxPrec, string sFiltreNiveauN, ref StringBuilder sFiltreNiveauN1)
        {
            bool bResultat = true;
            string sTacheCCT;

            DataManagerSQLServer dmsManagerSQL;
            List<string> lTacheCCT;
            string sRequete;
            bool bPremierElement;
            DataSet dsDataSet;
            const string CCT_OBJECT_TYPE_INT = "'WEB FILE','WEB LITERAL SOC'";

            try
            {
                dmsManagerSQL = new DataManagerSQLServer();
                lTacheCCT = new List<string>();



                if (sFiltreNiveauN != string.Empty)
                {
                    sRequete = "SELECT A.CCT_TASK_ID,A.CCT_OBJECT_TYPE,A.CCT_OBJECT_ID,A.CCT_PARENT_OBJ_ID,A.DEP_CCT_TASK_ID,A.DEP_CCT_OBJECT_TYPE,A.DEP_CCT_OBJECT_ID,A.DEP_CCT_PARENT_OBJ_ID,A.DEP_CCT_ACTION_TYPE,A.DEP_CCT_PACK_TYPE,A.DEP_CCT_COMMAND_TYPE ";
                    sRequete += "FROM M4CFR_VW_CCT_DEPENDANCES A ";
                    /*sRequete += "INNER JOIN M4RDL_PACKAGES B ON (A.DEP_CCT_TASK_ID = B.CCT_TASK_ID) ";
                    sRequete += "INNER JOIN M4RDL_RAM_PACKS C ON (C.ID_PACKAGE = B.ID_PACKAGE) ";*/
                    sRequete += "WHERE A.CCT_TASK_ID IN (" + sFiltreNiveauN + ") ";
                    sRequete += "AND A.DEP_CCT_TASK_ID NOT IN (" + sFiltreNiveauN + ") ";
                    if (sFiltreNiveauxPrec != string.Empty)
                    {
                        sRequete += "AND A.DEP_CCT_TASK_ID NOT IN (" + sFiltreNiveauxPrec + ") ";
                    }

                    if (CCT_OBJECT_TYPE_INT != String.Empty)
                    {
                        sRequete += "AND A.CCT_OBJECT_TYPE NOT IN (" + CCT_OBJECT_TYPE_INT + ") ";
                    }

                    sRequete += "AND A.DEP_CCT_TASK_ID not like '%DEF%' ";
                    sRequete += "AND A.CCT_OBJECT_TYPE+A.CCT_OBJECT_ID NOT IN ('PRESENTATIONSFR_DP_PAYROLL_CHANNEL','PRESENTATIONSCO_DP_PAYROLL_CHANNEL') ";
                    sRequete += "AND EXISTS (SELECT TOP 1 B.CCT_TASK_ID FROM M4RDL_PACKAGES B,M4RDL_RAM_PACKS C WHERE  A.DEP_CCT_TASK_ID = B.CCT_TASK_ID AND C.ID_PACKAGE = B.ID_PACKAGE) ";
                    if ((sCCTIgnore != null) && (sCCTIgnore != string.Empty))
                        sRequete += "AND A.DEP_CCT_TASK_ID NOT IN (" + sCCTIgnore + ") ";

                    dsDataSet = dmsManagerSQL.GetData(sRequete, ConnectionStringBaseRef);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                        {
                            conn.Open();
                            sRequete = "DELETE FROM PNPU_DEP_REF WHERE WORKFLOW_ID = " + Process.WORKFLOW_ID.ToString() + " AND  NIV_DEP = " + iNiveau.ToString() + " AND ID_H_WORKFLOW = " + Process.ID_INSTANCEWF.ToString();
                            using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }

                            sRequete = "INSERT INTO PNPU_DEP_REF (";
                            sRequete += "WORKFLOW_ID";
                            sRequete += ",NIV_DEP";
                            sRequete += ",CCT_TASK_ID";
                            sRequete += ",CCT_OBJECT_TYPE";
                            sRequete += ",CCT_OBJECT_ID";
                            sRequete += ",CCT_PARENT_OBJ_ID";
                            sRequete += ",DEP_CCT_TASK_ID";
                            sRequete += ",DEP_CCT_OBJECT_TYPE";
                            sRequete += ",DEP_CCT_OBJECT_ID";
                            sRequete += ",DEP_CCT_PARENT_OBJ_ID";
                            sRequete += ",DEP_CCT_ACTION_TYPE";
                            sRequete += ",DEP_CCT_PACK_TYPE";
                            sRequete += ",DEP_CCT_COMMAND_TYPE";
                            sRequete += ",ID_H_WORKFLOW";
                            sRequete += ") VALUES (";
                            sRequete += "@WORKFLOW_ID";
                            sRequete += ",@NIV_DEP";
                            sRequete += ",@CCT_TASK_ID";
                            sRequete += ",@CCT_OBJECT_TYPE";
                            sRequete += ",@CCT_OBJECT_ID";
                            sRequete += ",@CCT_PARENT_OBJ_ID";
                            sRequete += ",@DEP_CCT_TASK_ID";
                            sRequete += ",@DEP_CCT_OBJECT_TYPE";
                            sRequete += ",@DEP_CCT_OBJECT_ID";
                            sRequete += ",@DEP_CCT_PARENT_OBJ_ID";
                            sRequete += ",@DEP_CCT_ACTION_TYPE";
                            sRequete += ",@DEP_CCT_PACK_TYPE";
                            sRequete += ",@DEP_CCT_COMMAND_TYPE ";
                            sRequete += ",@ID_H_WORKFLOW ";
                            sRequete += ")";

                            using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                            {
                                cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int);
                                cmd.Parameters.Add("@NIV_DEP", SqlDbType.Int);
                                cmd.Parameters.Add("@CCT_TASK_ID", SqlDbType.VarChar, 30);
                                cmd.Parameters.Add("@CCT_OBJECT_TYPE", SqlDbType.VarChar, 30);
                                cmd.Parameters.Add("@CCT_OBJECT_ID", SqlDbType.VarChar, 255);
                                cmd.Parameters.Add("@CCT_PARENT_OBJ_ID", SqlDbType.VarChar, 255);
                                cmd.Parameters.Add("@DEP_CCT_TASK_ID", SqlDbType.VarChar, 30);
                                cmd.Parameters.Add("@DEP_CCT_OBJECT_TYPE", SqlDbType.VarChar, 30);
                                cmd.Parameters.Add("@DEP_CCT_OBJECT_ID", SqlDbType.VarChar, 255);
                                cmd.Parameters.Add("@DEP_CCT_PARENT_OBJ_ID", SqlDbType.VarChar, 255);
                                cmd.Parameters.Add("@DEP_CCT_ACTION_TYPE", SqlDbType.VarChar, 10);
                                cmd.Parameters.Add("@DEP_CCT_PACK_TYPE", SqlDbType.VarChar, 255);
                                cmd.Parameters.Add("@DEP_CCT_COMMAND_TYPE ", SqlDbType.Decimal);
                                cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int);

                                bPremierElement = true;
                                sFiltreNiveauN1.Clear();
                                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                                {
                                    sTacheCCT = drRow[4].ToString();
                                    if (!lTacheCCT.Contains(sTacheCCT))
                                    {
                                        lTacheCCT.Add(sTacheCCT);
                                        if (bPremierElement)
                                        {
                                            bPremierElement = false;
                                        }
                                        else
                                        {
                                            sFiltreNiveauN1.Append(",");
                                        }

                                        sFiltreNiveauN1.Append("'" + sTacheCCT + "'");
                                    }

                                    cmd.Parameters[0].Value = Process.WORKFLOW_ID;
                                    cmd.Parameters[1].Value = iNiveau;
                                    cmd.Parameters[13].Value = Process.ID_INSTANCEWF;
                                    for (int iCpt = 0; iCpt < 11; iCpt++)
                                    {
                                        cmd.Parameters[iCpt + 2].Value = drRow[iCpt];
                                    }

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = false;
            }

            return bResultat;
        }


        /// <summary>
        /// Le but de cette fonction est de déterminer les dépendances de de niveau 1 pour les packs livrés dans le HF.
        /// On détecte les tâches CCT dépendantes car au niveau 1 on va récupérer tout le contenu des tâches dépendantes qui manquent sur l'environnement client.
        /// </summary>
        /// <param name="lTacheDepN2">Ce paramètre contient au retour de l'appel toutes les tâches</param>
        /// <returns></returns>
        private bool RechercheDepN1(ref List<string> lTacheDepN2)
        {
            bool bResultat = true;
            DataManagerAccess dmaManagerAccess;
            DataManagerSQLServer dmsManagerSQL;
            List<string> lTacheCCTHF;
            List<string> lTacheDepN1;
            string sRequete;
            bool bPremierElement = true;
            DataSet dsDataSet;
            string sTacheCCT;
            StringBuilder sListeTacheCCT = new StringBuilder();

            try
            {
                lTacheDepN2.Clear();
                dmaManagerAccess = new DataManagerAccess();
                dmsManagerSQL = new DataManagerSQLServer();
                lTacheCCTHF = new List<string>();
                lTacheDepN1 = new List<string>();

                // Récupération de toutes les tâches CCT livrées dans le HF
                sRequete = "SELECT DISTINCT(CCT_TASK_ID) FROM M4RDL_PACKAGES";
                foreach (string sPathMdb in Process.listMDB)
                {
                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            sTacheCCT = drRow[0].ToString();
                            if (!lTacheCCTHF.Contains(sTacheCCT))
                            {
                                lTacheCCTHF.Add(sTacheCCT);
                                if (bPremierElement)
                                {
                                    bPremierElement = false;
                                }
                                else
                                {
                                    sListeTacheCCT.Append(",");
                                }

                                sListeTacheCCT.Append("'" + sTacheCCT + "'");
                            }

                        }
                    }
                }

                if (lTacheCCTHF.Count > 0)
                {
                    sRequete = "SELECT CCT_TASK_ID, DEP_CCT_TASK_ID from M4CFR_VW_CCT_DEPENDANCES where ";
                    sRequete += "CCT_TASK_ID IN (" + sListeTacheCCT + ") ";
                    sRequete += "AND DEP_CCT_TASK_ID NOT IN (" + sListeTacheCCT + ") ";
                    sRequete += "AND DEP_CCT_TASK_ID not like '%DEF%' ";
                    sRequete += "AND CCT_OBJECT_TYPE+CCT_OBJECT_ID NOT IN ('PRESENTATIONSFR_DP_PAYROLL_CHANNEL','PRESENTATIONSCO_DP_PAYROLL_CHANNEL')";

                    dsDataSet = dmsManagerSQL.GetData(sRequete, ConnectionStringBaseRef);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                        {
                            conn.Open();
                            using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_DEPN1_REF (WORKFLOW_ID, CCT_TASK_ID, DEP_CCT_TASK_ID, ID_H_WORKFLOW) VALUES( @WORKFLOW_ID, @CCT_TASK_ID,@DEP_CCT_TASK_ID, @ID_H_WORKFLOW)", conn))
                            {
                                cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int);
                                cmd.Parameters.Add("@CCT_TASK_ID", SqlDbType.VarChar, 30);
                                cmd.Parameters.Add("@DEP_CCT_TASK_ID", SqlDbType.VarChar, 30);
                                cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int);

                                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                                {
                                    sTacheCCT = drRow[0].ToString() + "*" + drRow[1].ToString();
                                    if (!lTacheDepN1.Contains(sTacheCCT))
                                    {
                                        lTacheDepN1.Add(sTacheCCT);
                                        if (!lTacheDepN2.Contains(drRow[1].ToString()))
                                        {
                                            lTacheDepN2.Add(drRow[1].ToString());
                                        }

                                        cmd.Parameters[0].Value = Process.WORKFLOW_ID;
                                        cmd.Parameters[1].Value = drRow[0].ToString();
                                        cmd.Parameters[2].Value = drRow[1].ToString();
                                        cmd.Parameters[3].Value = Process.ID_INSTANCEWF;
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = false;
            }

            return bResultat;
        }
    }
}
