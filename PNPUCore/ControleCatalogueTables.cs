using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que les tables livrées dans le packs sont bien présentes dans le catalogue des tables. 
    /// </summary>  
    internal class ControleCatalogueTable : PControle, IControle
    {
        readonly private PNPUCore.Process.ProcessControlePacks Process;
        readonly private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleCatalogueTable(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie que les tables livrées sont référencées dans le catalogue des tables";
            LibControle = "Contrôle du catalogue des tables";
            ResultatErreur = ParamAppli.StatutError;
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleCatalogueTable(PNPUCore.Process.IProcess pProcess, DataRow drRow)
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
            Dictionary<string, string> dListeAControler = new Dictionary<string, string>();
            StringBuilder sRequeteControle = new StringBuilder();
            bool bPremierElement = true;


            DataManagerAccess dmaManagerAccess;

            try
            {
                dmaManagerAccess = new DataManagerAccess();

                sRequete = "select A.ID_PACKAGE, A.ID_OBJECT,B.REAL_NAME FROM M4RDL_PACK_CMDS A INNER JOIN M4RDC_LOGIC_OBJECT B ON (A.ID_OBJECT = B.ID_OBJECT) ";
                sRequete += "WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS = 'LOGICAL TABLE' AND A.CMD_ACTIVE = -1 AND (B.ID_OBJECT_TYPE = 1 OR B.ID_OBJECT_TYPE = 3) AND ID_ORG_TYPE <> 1";

                DataSet dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    sRequeteControle.Append("SELECT ID_OBJECT FROM M4RDC_LOGIC_OBJECT WHERE ID_OBJECT IN (");

                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        // Pour chaque table livrée, vérifie si elle est dans le catalogue.
                        sRequete = "select * from M4RDL_PACK_CMDS WHERE CMD_CODE LIKE '%M4CFR_X_DATA_TABLES%' + CHR(39) + '" + drRow[2].ToString() + "' + CHR(39) + '%' OR CMD_CODE LIKE '%M4CFR_X_DATA_TABLES%' + CHR(39) + '" + drRow[1].ToString() + "' + CHR(39) + '%'";
                        DataSet dsDataSet2 = dmaManagerAccess.GetData(sRequete, sPathMdb);

                        // Si la table n'est pas dans le catalogue on prévoit de vérifier si elle est déja présente sur la base de ref
                        if ((dsDataSet2 == null) || (dsDataSet2.Tables[0].Rows.Count == 0))
                        {
                            dListeAControler.Add(drRow[1].ToString(), drRow[0].ToString());
                            if (bPremierElement)
                            {
                                bPremierElement = false;
                            }
                            else
                            {
                                sRequeteControle.Append(",");
                            }

                            sRequeteControle.AppendFormat("'{0}'", drRow[1].ToString());
                        }

                    }

                    // Vérification dans la base de référence si les tables sont présentes.
                    if (!bPremierElement && (ConnectionStringBaseRef != string.Empty))
                    {
                        DataManagerSQLServer dmsManagerSQL = new DataManagerSQLServer();
                        sRequeteControle.Append(")");
                        dsDataSet = dmsManagerSQL.GetData(sRequeteControle.ToString(), ConnectionStringBaseRef);
                        if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                        {
                            foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                            {
                                // On supprime de la liste des tables à contrôler celles existantes sur la base de ref
                                if (dListeAControler.ContainsKey(drRow[0].ToString()))
                                {
                                    dListeAControler.Remove(drRow[0].ToString());
                                }
                            }
                        }
                    }

                    //Le tables restant dans la liste sont en erreur
                    foreach (string sCle in dListeAControler.Keys)
                    {
                        Process.AjouteRapport("Livraison de la table " + sCle + " dans le pack " + dListeAControler[sCle] + " sans mise à jour du catalogue des tables.");
                        bResultat = ResultatErreur;
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
