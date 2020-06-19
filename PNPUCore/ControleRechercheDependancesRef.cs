using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;
using System.IO;
using PNPUTools;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler les dépendances entre les tâches CCT livrées dans le HF et tâches existantes sur la base de référence. 
    /// </summary>  
    class ControleRechercheDependancesRef : PControle, IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;
        private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleRechercheDependancesRef(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];

        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public string MakeControl()
        {
            bool bResultat;
            string sResultat = ParamAppli.StatutOk;
            string sRequete;
            string sNomMdb;
            DataManagerAccess dmaManagerAccess = null;
            DataSet dsDataSet;
            string sTacheCCT;
            List<string> lTacheCCTHF;
            bool bPremierElement = true;
            string sFiltreNiveauPrec = string.Empty;
            string sFiltreNiveauN = string.Empty;
            string sFiltreNiveauN1 = string.Empty;

            try
            {
                dmaManagerAccess = new DataManagerAccess();
                lTacheCCTHF = new List<string>();

                // Récupération de toutes les tâches CCT livrées dans le HF
                sRequete = "SELECT DISTINCT(CCT_TASK_ID) FROM M4RDL_PACKAGES";
                foreach (string sPathMdb in Process.listMDB)
                {
                    sNomMdb = Path.GetFileName(sPathMdb);
                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            sTacheCCT = drRow[0].ToString();
                            if ((sTacheCCT != string.Empty) && (lTacheCCTHF.Contains(sTacheCCT) == false))
                            {
                                lTacheCCTHF.Add(sTacheCCT);
                                if (bPremierElement == true)
                                    bPremierElement = false;
                                else
                                    sFiltreNiveauN += ",";

                                sFiltreNiveauN += "'" + sTacheCCT + "'";
                            }

                        }
                    }
                }

                // Recherche des dépendances de Niveau 1
                bResultat = RechercheDependances(1, sFiltreNiveauPrec, sFiltreNiveauN, ref sFiltreNiveauN1);

                // Recherche des dépendances de Niveau 2
                if (bResultat == true)
                {
                     sFiltreNiveauPrec = sFiltreNiveauN;
                    sFiltreNiveauN = sFiltreNiveauN1;
                    sFiltreNiveauN1 = string.Empty;
                    bResultat = RechercheDependances(2, sFiltreNiveauPrec, sFiltreNiveauN, ref sFiltreNiveauN1);
                }

                // Recherche des dépendances de Niveau 3
                if (bResultat == true)
                {
                    if (sFiltreNiveauPrec != string.Empty)
                        sFiltreNiveauPrec += ",";
                    sFiltreNiveauPrec += sFiltreNiveauN;
                    sFiltreNiveauN = sFiltreNiveauN1;
                    sFiltreNiveauN1 = string.Empty;
                    bResultat = RechercheDependances(3, sFiltreNiveauPrec, sFiltreNiveauN, ref sFiltreNiveauN1);
                }
                if (bResultat == false)
                    sResultat = ParamAppli.StatutError;
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
                sResultat = ParamAppli.StatutError;
            }

            return sResultat;

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
        private bool RechercheDependances(int iNiveau, string sFiltreNiveauxPrec, string sFiltreNiveauN, ref string sFiltreNiveauN1)
        {
            bool bResultat = true;
            string sTacheCCT;

            DataManagerSQLServer dmsManagerSQL = null;
            List<string> lTacheCCT;
            string sRequete;
            bool bPremierElement; 
            DataSet dsDataSet = null;
            const string CCT_OBJECT_TYPE_INT = "'WEB FILE','WEB LITERAL SOC'";



            string sListeTacheCCT = string.Empty;

            try
            {
                dmsManagerSQL = new DataManagerSQLServer();
                lTacheCCT = new List<string>();
 
               

                if (sFiltreNiveauN != string.Empty)
                {
                    sRequete = "SELECT A.CCT_TASK_ID,A.CCT_OBJECT_TYPE,A.CCT_OBJECT_ID,A.CCT_PARENT_OBJ_ID,A.DEP_CCT_TASK_ID,A.DEP_CCT_OBJECT_TYPE,A.DEP_CCT_OBJECT_ID,A.DEP_CCT_PARENT_OBJ_ID,A.DEP_CCT_ACTION_TYPE,A.DEP_CCT_PACK_TYPE,A.DEP_CCT_COMMAND_TYPE ";
                    sRequete += "FROM M4CFR_VW_CCT_DEPENDANCES A ";
                    sRequete += "INNER JOIN M4RDL_PACKAGES B ON (A.DEP_CCT_TASK_ID = B.CCT_TASK_ID) ";
                    sRequete += "INNER JOIN M4RDL_RAM_PACKS C ON (C.ID_PACKAGE = B.ID_PACKAGE) ";
                    sRequete += "WHERE A.CCT_TASK_ID IN (" + sFiltreNiveauN + ") ";
                    sRequete += "AND A.DEP_CCT_TASK_ID NOT IN (" + sFiltreNiveauN + ") ";
                    if (sFiltreNiveauxPrec != string.Empty)
                        sRequete += "AND A.DEP_CCT_TASK_ID NOT IN (" + sFiltreNiveauxPrec + ") ";
                    if (CCT_OBJECT_TYPE_INT != String.Empty)
                        sRequete += "AND A.CCT_OBJECT_TYPE NOT IN (" + CCT_OBJECT_TYPE_INT + ") ";
                    sRequete += "AND A.DEP_CCT_TASK_ID not like '%DEF%' ";
                    sRequete += "AND A.CCT_OBJECT_TYPE+A.CCT_OBJECT_ID NOT IN ('PRESENTATIONSFR_DP_PAYROLL_CHANNEL','PRESENTATIONSCO_DP_PAYROLL_CHANNEL') ";
                    

                    dsDataSet = dmsManagerSQL.GetData(sRequete, ConnectionStringBaseRef);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
                        {
                            conn.Open();
                            sRequete = "DELETE FROM PNPU_DEP_REF WHERE WORKFLOW_ID = " + Process.WORKFLOW_ID.ToString() + " AND  NIV_DEP = "+ iNiveau.ToString() + " AND ID_H_WORKFLOW = " + Process.ID_INSTANCEWF.ToString();
                            using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                            {
                                int rowsAffected = cmd.ExecuteNonQuery();
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
                                sFiltreNiveauN1 = string.Empty;
                                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                                {
                                    sTacheCCT = drRow[4].ToString();
                                    if (lTacheCCT.Contains(sTacheCCT) == false)
                                    {
                                        lTacheCCT.Add(sTacheCCT);
                                        if (bPremierElement == true)
                                            bPremierElement = false;
                                        else
                                            sFiltreNiveauN1 += ",";

                                        sFiltreNiveauN1 += "'" + sTacheCCT + "'";
                                    }

                                    cmd.Parameters[0].Value = Process.WORKFLOW_ID;
                                    cmd.Parameters[1].Value = iNiveau;
                                    cmd.Parameters[13].Value = Process.ID_INSTANCEWF;
                                    for (int iCpt=0; iCpt<11;iCpt++)
                                        cmd.Parameters[iCpt+2].Value = drRow[iCpt];
                                    
                                    int rowsAffected = cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
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
            DataManagerAccess dmaManagerAccess = null;
            DataManagerSQLServer dmsManagerSQL = null;
            List<string> lTacheCCTHF;
            List<string> lTacheDepN1;
            string sRequete;
            bool bPremierElement = true;
            DataSet dsDataSet = null;
            string sNomMdb;
            string sTacheCCT;
            string sListeTacheCCT = string.Empty;

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
                    sNomMdb = Path.GetFileName(sPathMdb);
                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            sTacheCCT = drRow[0].ToString();
                            if (lTacheCCTHF.Contains(sTacheCCT) == false)
                            {
                                lTacheCCTHF.Add(sTacheCCT);
                                if (bPremierElement == true)
                                    bPremierElement = false;
                                else
                                    sListeTacheCCT += ",";

                                sListeTacheCCT += "'" + sTacheCCT + "'";
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
                                    if (lTacheDepN1.Contains(sTacheCCT) == false)
                                    {
                                        lTacheDepN1.Add(sTacheCCT);
                                        if (lTacheDepN2.Contains(drRow[1].ToString()) == false)
                                            lTacheDepN2.Add(drRow[1].ToString());
                                        cmd.Parameters[0].Value = Process.WORKFLOW_ID;
                                        cmd.Parameters[1].Value = drRow[0].ToString();
                                        cmd.Parameters[2].Value = drRow[1].ToString();
                                        cmd.Parameters[3].Value = Process.ID_INSTANCEWF;
                                        int rowsAffected = cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = false;
            }

            return bResultat;
        }
    }
}
