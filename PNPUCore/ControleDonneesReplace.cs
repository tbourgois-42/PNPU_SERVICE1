using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que des données sont présentes pour toutes les commandes REPLACE. 
    /// </summary>  
    class ControleDonneesReplace : IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleDonneesReplace(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public bool MakeControl()
        {
            bool bResultat = true;
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
                        int iIndex = 0;
                        int iIndex2 = 0;
                        int iIndex3 = 0;
                        
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

                                sRequete = "SELECT COUNT(*) FROM " + sTable;
                                
                                if (sWhere != string.Empty)
                                    sRequete += " WHERE " + sWhere;

                                dsDataSet2 = dmaManagerAccess2.GetData(sRequete, sPathMdb);
                                if ((dsDataSet2 != null) && (dsDataSet2.Tables[0].Rows.Count > 0))
                                {
                                    foreach (DataRow drRow2 in dsDataSet2.Tables[0].Rows)
                                    {
                                        if (drRow2[0].ToString() == "0")
                                        {
                                            Process.AjouteRapport("Replace sur la table " + sTable + " dans le pack " + drRow[0].ToString() + " sans donnée dans le mdb(filtre: " + sWhere + ")");
                                            bResultat = false;
                                        }
                                    }
                                }
                                else
                                {
                                    Process.AjouteRapport("Replace sur la table " + sTable + " dans le pack " + drRow[0].ToString() + " sans donnée dans le mdb(filtre: " + sWhere + ")");
                                    bResultat = false;
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
                // TODO, loguer l'exception
                bResultat = false;
            }

            return bResultat;

        }
    }
}
