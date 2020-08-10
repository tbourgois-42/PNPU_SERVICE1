using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que les tables livrées dans le packs sont bien présentes dans le catalogue des tables. 
    /// </summary>  
    class ControleLivraisonTablecomplete : PControle, IControle
    {
        readonly private PNPUCore.Process.ProcessControlePacks Process;
        readonly private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleLivraisonTablecomplete(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie que les données livrées dans les tables sont filtrées pour une livraison au plus fin afin de limiter le risque d'écrasement de données spécifiques.";
            LibControle = "Contrôle livraison d'une table complète";
            ResultatErreur = ParamAppli.StatutError;
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleLivraisonTablecomplete(PNPUCore.Process.IProcess pProcess, DataRow drRow)
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
            string sTable = string.Empty;
            string sFilter = string.Empty;
            List<string> lColumnsList = new List<string>(); ;


            DataManagerAccess dmaManagerAccess = null;
            DataManagerSQLServer dmsManagerSQL = null;

            try
            {
                dmaManagerAccess = new DataManagerAccess();
                dmsManagerSQL = new DataManagerSQLServer();

                sRequete = "select ID_PACKAGE,CMD_CODE from M4RDL_PACK_CMDS where CMD_CODE LIKE '%1=1%'";

                DataSet dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        // Découpage par ligne de commande
                        string[] lListeCommandes = dmaManagerAccess.splitCmdCodeData(drRow[1].ToString());
                        foreach (string LigneCommande in lListeCommandes)
                        {
                            if (LigneCommande.ToUpper().Contains("REPLACE"))
                            {
                                dmaManagerAccess.ExtractTableFilter(LigneCommande, ref sTable, ref sFilter, ref lColumnsList);
                                if (sFilter == "1=1")
                                {
                                    DataSet dsDataSet2 = dmsManagerSQL.GetData("SELECT COUNT(*) FROM " + sTable, ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]);
                                    if ((dsDataSet2 != null) && (dsDataSet2.Tables[0].Rows.Count > 0) && dsDataSet2.Tables[0].Rows[0].ToString() != "0")
                                    {
                                        Process.AjouteRapport("Presence d'un REPLACE sans filtre sur la table " + sTable + " dans le pack " + drRow[0].ToString());
                                        bResultat = ResultatErreur;
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
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;
        }
    }
}
