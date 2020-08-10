using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que tous les items présent dans les totaux livrés existent. 
    /// </summary>  
    class ControleItemsTotaux : PControle, IControle
    {
        readonly private PNPUCore.Process.ProcessControlePacks Process;
        readonly private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleItemsTotaux(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie que les éléments utilisés dans les totaux livrés existent";
            LibControle = "Contrôle des totaux";
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleItemsTotaux(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        new public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            string sPathMdb = Process.MDBCourant;
            string sRequete;
            List<string[]> lListeAControler = new List<string[]>();
            bool bPremierElement = true;
            string sItem;
            DataSet dsDataSet;

            string sListeItemsLivres = string.Empty;

            DataManagerAccess dmaManagerAccess = null;

            try
            {
                dmaManagerAccess = new DataManagerAccess();


                // Recherche des items de paie livrés dans les packs du mdb
                sRequete = "SELECT ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_CLASS = 'ITEM' AND CMD_ACTIVE=-1 AND ID_OBJECT LIKE '%HR%CALC%' AND ID_OBJECT NOT LIKE '%DIF%'";
                dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);
                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        if (sListeItemsLivres != string.Empty)
                            sListeItemsLivres += ",";
                        sListeItemsLivres += "'" + drRow[0].ToString() + "'";
                    }
                }


                sRequete = "select ID_TI, ID_ITEM, ID_ITEM_USED_TI, ID_ITEM_USED FROM M4RCH_TOTAL_REF A";
                // On livre le total 
                sRequete += " WHERE (A.ID_TI+'.'+A.ID_ITEM IN (" + sListeItemsLivres + ")) ";
                sRequete += " AND  (A.ID_ITEM_USED_TI+'.'+ID_ITEM_USED NOT IN (" + sListeItemsLivres + ")) ";

                // Et il existe des items du total qu'on ne livre pas
                sRequete += " AND (EXISTS (SELECT * FROM M4RCH_TOTAL_REF C WHERE A.ID_TI=C.ID_TI AND A.ID_ITEM=C.ID_ITEM AND C.ID_ITEM_USED_TI+'.'+C.ID_ITEM_USED NOT IN (" + sListeItemsLivres + ")))";

                dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {

                    sRequete = "SELECT ID_TI + '.' + ID_ITEM FROM M4RCH_ITEMS WHERE ID_TI + '.' + ID_ITEM IN (";

                    // Recherche les items utilisés dans les totaux
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        sItem = drRow[2].ToString() + "." + drRow[3].ToString();

                        lListeAControler.Add(new string[] { sItem, drRow[0].ToString() + "." + drRow[1].ToString() });

                        if (bPremierElement)
                            bPremierElement = false;
                        else
                            sRequete += ",";

                        sRequete += "'" + sItem + "'";

                    }
                    sRequete += ")";
                }

                // Recherche des items sur la base de ref
                if (lListeAControler.Count > 0)
                {
                    DataManagerSQLServer dmsManagerSQL = new DataManagerSQLServer();
                    dsDataSet = dmsManagerSQL.GetData(sRequete, ConnectionStringBaseRef);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            for (int iCpt = 0; iCpt < lListeAControler.Count; iCpt++)
                            {
                                if (lListeAControler[iCpt][0] == drRow[0].ToString())
                                {
                                    lListeAControler.RemoveAt(iCpt);
                                    iCpt--;
                                }
                            }

                        }
                    }

                    if (lListeAControler.Count > 0)
                    {
                        bResultat = ResultatErreur;
                        foreach (string[] sElements in lListeAControler)
                            Process.AjouteRapport("Le total " + sElements[1] + " utilise un item inexistant (" + sElements[0] + ").");
                    }
                }


            }
            catch (Exception ex)
            {
                LoggerHelper.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;

        }
    }
}
