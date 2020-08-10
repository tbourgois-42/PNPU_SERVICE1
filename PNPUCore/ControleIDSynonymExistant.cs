using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{

    /// <summary>  
    /// Cette classe permet de contrôler que les ID_SYNONYME des items livrés n'existent pas déja pour d'autres items. 
    /// </summary>  
    class ControleIDSynonymExistant : PControle, IControle
    {
        readonly private string sPathMdb = string.Empty;
        readonly private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleIDSynonymExistant(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie si les items livrés dans le mdb n'utilisent pas un ID Synonym déjà utilisé";
            LibControle = "Contrôles des ID Synonym existant";
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleIDSynonymExistant(PNPUCore.Process.IProcess pProcess, DataRow drRow)
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
        new public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            string sPathMdb = Process.MDBCourant;
            string sID_SYNONYM;
            Dictionary<string, string> dicListItems = new Dictionary<string, string>();
            bool bItemAControler = false;
            string sRequeteSqlServer = string.Empty;

            DataManagerAccess dmaManagerAccess;

            ParamToolbox paramToolbox = new ParamToolbox();

            string sConnectionStringBaseQA1 = paramToolbox.GetConnexionString("Before", Process.WORKFLOW_ID, Process.CLIENT_ID);

            try
            {
                dmaManagerAccess = new DataManagerAccess();
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE (ID_TI LIKE '%HRPERIOD%CALC' OR ID_TI LIKE '%HRROLE%CALC') AND ID_TI NOT LIKE '%DIF%' AND  ID_SYNONYM <> 0", sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    sRequeteSqlServer = "select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE (ID_TI LIKE '%HRPERIOD%CALC' OR ID_TI LIKE '%HRROLE%CALC') ";
                    // Ne faire que si pack standard
                    sRequeteSqlServer += "AND(ID_TI LIKE 'SCO%' OR ID_TI LIKE 'SFR%' OR ID_TI LIKE 'CFR%') ";

                    sRequeteSqlServer += "AND ID_TI NOT LIKE '%DIF%' AND (";

                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        sID_SYNONYM = drRow[1].ToString();
                        if (!dicListItems.ContainsKey(sID_SYNONYM))
                        {
                            dicListItems.Add(sID_SYNONYM, drRow[0].ToString());
                            if (!bItemAControler)
                                bItemAControler = true;
                            else
                                sRequeteSqlServer += "OR ";

                            sRequeteSqlServer += " (ID_SYNONYM = " + drRow[1].ToString() + " AND ID_ITEM <> '" + drRow[0].ToString() + "') ";
                        }
                    }

                    if (bItemAControler)
                    {
                        sRequeteSqlServer += ")";
                        DataManagerSQLServer dmasqlManagerSQL = new DataManagerSQLServer();

                        // Contrôle sur la base de référence si pack standard, sinon sur base client
                        if (Process.STANDARD)
                            dsDataSet = dmasqlManagerSQL.GetData(sRequeteSqlServer, ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY]);
                        else
                            dsDataSet = dmasqlManagerSQL.GetData(sRequeteSqlServer, sConnectionStringBaseQA1);

                        if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                        {
                            bResultat = ResultatErreur;
                            foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                            {
                                Process.AjouteRapport("L'ID_SYNONYM de l'item " + dicListItems[drRow[1].ToString()] + "(" + drRow[1].ToString() + ") est déja utilisé pour l'item " + drRow[0].ToString() + ".");
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
