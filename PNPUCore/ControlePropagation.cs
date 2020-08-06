using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que les données livrées dans des tables multi orga sont propagées. 
    /// </summary>  
    class ControlePropagation : PControle, IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControlePropagation(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie si les données des tables multi orga livrées dans le mdb sont propagées";
            LibControle = "Contrôle propagation des données";
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControlePropagation(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            string sPathMdb = Process.MDBCourant;
            string sRequete = string.Empty;

            DataManagerAccess dmaManagerAccess = null;
            DataManagerAccess dmaManagerAccess2 = null;
            DataSet dsDataSet = null;
            DataSet dsDataSet2 = null;

            try
            {
                dmaManagerAccess = new DataManagerAccess();
                dmaManagerAccess2 = new DataManagerAccess();

                dsDataSet = dmaManagerAccess.GetData("select ID_PACKAGE, CMD_CODE FROM M4RDL_PACK_CMDS WHERE UCase(CMD_CODE) LIKE '%REPLACE%' AND CMD_ACTIVE=-1 ", sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        string stempo = drRow[1].ToString();
                        string sTable = string.Empty;
                        string sWhere = string.Empty;
                        string sWhere2 = string.Empty;
                        int iIndex = 0;
                        int iIndex2 = 0;
                        int iIndex3 = 0;
                        bool bMultiOrga;

                        dmaManagerAccess = new DataManagerAccess();

                        iIndex = stempo.ToUpper().IndexOf("REPLACE ", iIndex);
                        while (iIndex >= 0)
                        {
                            iIndex += "REPLACE".Length + 1;
                            iIndex2 = stempo.ToUpper().IndexOf("FROM", iIndex);
                            if (iIndex2 >= 0)
                            {
                                // Récupréation du nom de la table
                                sTable = stempo.ToUpper().Substring(iIndex, iIndex2 - iIndex).Trim();
                                sWhere = string.Empty;

                                // Recherche s'il y a une clause where
                                iIndex = stempo.IndexOf('\"', iIndex2);
                                if (iIndex >= 0)
                                {
                                    // Vérifie qu'il ne s'agit pas du WHERE d'une commande replace suivante
                                    iIndex3 = stempo.ToUpper().IndexOf("REPLACE ", iIndex2);
                                    if ((iIndex3 < 0) || (iIndex3 > iIndex))
                                    {

                                        iIndex2 = stempo.IndexOf('\"', iIndex + 1);
                                        if (iIndex2 < 0)
                                            iIndex2 = stempo.Length - 1;
                                        else
                                            sWhere = stempo.Substring(iIndex + 1, iIndex2 - iIndex - 1);
                                    }
                                    else
                                    {
                                        if ((iIndex3 >= 0) && (iIndex3 < iIndex))
                                            iIndex = iIndex3;
                                    }
                                }

                                bMultiOrga = false;
                                // Controle si la table est multi orga
                                sRequete = "SELECT ID_ORG_TYPE FROM M4RDC_LOGIC_OBJECT WHERE REAL_NAME = '" + sTable + "'";
                                dsDataSet2 = dmaManagerAccess.GetData(sRequete, sPathMdb);
                                if ((dsDataSet2 != null) && (dsDataSet2.Tables[0].Rows.Count > 0))
                                {
                                    if (dsDataSet2.Tables[0].Rows[0][0].ToString() == "2")
                                        bMultiOrga = true;
                                }
                                else
                                {
                                    DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
                                    dsDataSet2 = dataManagerSQL.GetData(sRequete, ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]);
                                    if ((dsDataSet2 != null) && (dsDataSet2.Tables[0].Rows.Count > 0))
                                    {
                                        if (dsDataSet2.Tables[0].Rows[0][0].ToString() == "2")
                                            bMultiOrga = true;
                                    }
                                }

                                if (bMultiOrga == true)
                                {
                                    // Recherche de la commande de propagation SQL Server
                                    sRequete = "select ID_PACKAGE, CMD_CODE FROM M4RDL_PACK_CMDS WHERE UCase(CMD_CODE) LIKE '%EXEC%M4SFR_COPY_DATA_ORG%" + sTable + "%";
                                    if (sWhere != string.Empty)
                                    {
                                        sWhere2 = sWhere.Trim();
                                        sWhere2 = sWhere.ToUpper();
                                        sWhere2 = sWhere2.Trim();
                                        sWhere2 = sWhere2.Replace("'", "' + CHR(39) + CHR(39) + '");
                                        sRequete += sWhere2 + "%";
                                    }
                                    sRequete += "'";

                                    dsDataSet2 = dmaManagerAccess.GetData(sRequete, sPathMdb);
                                    if ((dsDataSet2 == null) || (dsDataSet2.Tables[0].Rows.Count == 0))
                                    {
                                        string sTypeBase = string.Empty;

                                        if (Process.TYPOLOGY == "Dédié") sTypeBase = " SQL Server ";

                                        bResultat = ParamAppli.StatutError;
                                        if (sWhere != string.Empty)
                                            Process.AjouteRapport("Propagation " + sTypeBase + "manquante dans le pack " + drRow[0].ToString() + " pour la table " + sTable + " (filtre " + sWhere + ").");
                                        else
                                            Process.AjouteRapport("Propagation " + sTypeBase + "manquante dans le pack " + drRow[0].ToString() + " pour la table " + sTable + ".");

                                    }

                                    // Si on est en dédié on vérifie la propagation Oracle
                                    if (Process.TYPOLOGY == "Dédié")
                                    {
                                        sRequete = "select ID_PACKAGE, CMD_CODE FROM M4RDL_PACK_CMDS WHERE UCase(CMD_CODE) LIKE '%CALL%M4SFR_COPY_DATA_ORG%" + sTable + "%";
                                        if (sWhere != string.Empty)
                                        {
                                            sWhere2 = sWhere.ToUpper();
                                            sWhere2 = sWhere2.Replace("'", "' + CHR(39) + CHR(39) + '");
                                            if (sWhere2.Contains("{D") == true)
                                            {
                                                sWhere2 = sWhere2.Replace("{D", "TO_DATE(");
                                                sWhere2 = sWhere2.Replace("}", ",' + CHR(39) + CHR(39) + 'YYYY-MM-DD' + CHR(39) + CHR(39) +')");
                                            }
                                            sRequete += sWhere2 + "%";
                                        }
                                        sRequete += "'";

                                        dsDataSet2 = dmaManagerAccess.GetData(sRequete, sPathMdb);
                                        if ((dsDataSet2 == null) || (dsDataSet2.Tables[0].Rows.Count == 0))
                                        {
                                            bResultat = ParamAppli.StatutError;
                                            if (sWhere != string.Empty)
                                                Process.AjouteRapport("Propagation Oracle manquante dans le pack " + drRow[0].ToString() + " pour la table " + sTable + " (filtre " + sWhere + ").");
                                            else
                                                Process.AjouteRapport("Propagation Oracle manquante dans le pack " + drRow[0].ToString() + " pour la table " + sTable + ".");

                                        }


                                    }
                                }


                            }
                            if (iIndex >= 0)
                                iIndex = stempo.ToUpper().IndexOf("REPLACE ", iIndex);

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;

        }
    }
}
