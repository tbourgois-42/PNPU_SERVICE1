using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler si les niveaux de saisie d'un item existant n'ont pas été supprimés.
    /// </summary>  
    class ControleNiveauSaisie : PControle, IControle
    {
        readonly private PNPUCore.Process.ProcessControlePacks Process;
        readonly private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleNiveauSaisie(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
            ToolTipControle = "Vérifie qu'il n'y a pas de perte de niveau de saisie entre le mdb standard et la base client";
            LibControle = "Contrôle des niveaux de saisies";
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleNiveauSaisie(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
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
            string sRequete = string.Empty;
            DataSet dsDataSet;
            List<string[]> lListeITEMS = new List<string[]>();
            string sRequeteControle = string.Empty;
            bool bPremierElement;
            bool bItemTrouve;

            DataManagerAccess dmaManagerAccess;
            DataManagerSQLServer dmsManagerSQL;

            ParamToolbox paramToolbox = new ParamToolbox();

            string sConnectionStringBaseQA1 = paramToolbox.GetConnexionString("Before", Process.WORKFLOW_ID, Process.CLIENT_ID);

            try
            {
                if (ConnectionStringBaseRef != string.Empty)
                {

                    dmaManagerAccess = new DataManagerAccess();

                    sRequeteControle = "SELECT ID_DMD_COMPONENT, ID_DMD_GROUP FROM M4RCH_DMD_GRP_CMP WHERE ID_DMD_GROUP<> 'DMD_INC_VAL_MSS' AND ID_DMD_COMPONENT IN (";
                    bPremierElement = true;

                    // Recherche des items de paie livrés
                    sRequete = "SELECT A.ID_PACKAGE AS ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT, B.ID_DMD_COMPONENT AS ID_DMD_COMPONENT FROM M4RDL_PACK_CMDS A, M4RCH_ITEMS B WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS='ITEM' AND (A.ID_OBJECT LIKE '%HRPERIOD_CALC.%' OR A.ID_OBJECT LIKE '%HRROLE_CALC.%') AND A.CMD_ACTIVE = -1 AND B.ID_TI + '.' + B.ID_ITEM = A.ID_OBJECT ";

                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            if (bPremierElement)
                                bPremierElement = false;
                            else
                                sRequeteControle += ",";

                            if (drRow[2].ToString() != string.Empty)
                                sRequeteControle += "'" + drRow[2].ToString() + "'";
                            else
                                sRequeteControle += "'" + drRow[1].ToString().Substring(drRow[1].ToString().LastIndexOf(".") + 1) + "'";

                            bItemTrouve = false;
                            for (int elt = 0; elt < lListeITEMS.Count && !bItemTrouve; elt++)
                            {
                                if (lListeITEMS[elt][0] == drRow[1].ToString())
                                {
                                    lListeITEMS[elt][1] += " / " + drRow[0].ToString();
                                    bItemTrouve = true;
                                }
                            }

                            if (!bItemTrouve)
                            {
                                string sDMD_COMPONENT = string.Empty;


                                if (drRow[2].ToString() != string.Empty)
                                    sDMD_COMPONENT = drRow[2].ToString();
                                else
                                    sDMD_COMPONENT = drRow[1].ToString().Substring(drRow[1].ToString().LastIndexOf(".") + 1);

                                lListeITEMS.Add(new string[] { drRow[1].ToString(), drRow[0].ToString(), sDMD_COMPONENT, string.Empty, string.Empty });
                            }


                        }
                        dsDataSet.Clear();
                    }

                    // Recherche des payroll items livrés
                    sRequete = "SELECT A.ID_PACKAGE AS ID_PACKAGE, B.ID_TI + '.' + B.ID_ITEM AS ID_OBJECT, B.ID_DMD_COMPONENT AS ID_DMD_COMPONENT FROM M4RDL_PACK_CMDS A, M4RCH_ITEMS B, M4RCH_PICOMPONENTS C WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS='PAYROLL ITEM' AND A.CMD_ACTIVE = -1 AND C.ID_T3 + '.' + C.ID_PAYROLL_ITEM = A.ID_OBJECT AND B.ID_TI =C.ID_TI AND B.ID_ITEM=C.ID_ITEM";
                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            if (bPremierElement)
                                bPremierElement = false;
                            else
                                sRequeteControle += ",";

                            if (drRow[2].ToString() != string.Empty)
                                sRequeteControle += "'" + drRow[2].ToString() + "'";
                            else
                                sRequeteControle += "'" + drRow[1].ToString().Substring(drRow[1].ToString().LastIndexOf(".") + 1) + "'";

                            bItemTrouve = false;
                            for (int elt = 0; elt < lListeITEMS.Count && !bItemTrouve; elt++)
                            {
                                if (lListeITEMS[elt][0] == drRow[1].ToString())
                                {
                                    lListeITEMS[elt][1] += " / " + drRow[0].ToString();
                                    bItemTrouve = true;
                                }
                            }

                            if (!bItemTrouve)
                            {
                                string sDMD_COMPONENT = string.Empty;


                                if (drRow[2].ToString() != string.Empty)
                                    sDMD_COMPONENT = drRow[2].ToString();
                                else
                                    sDMD_COMPONENT = drRow[1].ToString().Substring(drRow[1].ToString().LastIndexOf(".") + 1);

                                lListeITEMS.Add(new string[] { drRow[1].ToString(), drRow[0].ToString(), sDMD_COMPONENT, string.Empty, string.Empty });

                            }
                        }
                        dsDataSet.Clear();
                    }

                    // Recherche des niveaux de saisie dans le mdb
                    if (!bPremierElement)
                    {
                        sRequeteControle += ") ORDER BY ID_DMD_COMPONENT, ID_DMD_GROUP";

                        dsDataSet = dmaManagerAccess.GetData(sRequeteControle, sPathMdb);

                        if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                        {
                            foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                            {
                                bItemTrouve = false;
                                for (int elt = 0; elt < lListeITEMS.Count && !bItemTrouve; elt++)
                                {
                                    if (lListeITEMS[elt][2] == drRow[0].ToString())
                                    {
                                        if (lListeITEMS[elt][3] == string.Empty)
                                            lListeITEMS[elt][3] = "*";
                                        lListeITEMS[elt][3] += drRow[1].ToString() + "*";
                                        bItemTrouve = true;
                                    }
                                }
                            }
                        }

                        // Recherche des niveaux de saisie dans la base de référence ou la base client
                        dmsManagerSQL = new DataManagerSQLServer();
                        if (Process.PROCESS_ID == ParamAppli.ProcessControlePacks)
                            dsDataSet = dmsManagerSQL.GetData(sRequeteControle, ConnectionStringBaseRef);
                        else
                            dsDataSet = dmsManagerSQL.GetData(sRequeteControle, sConnectionStringBaseQA1);

                        if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                        {
                            foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                            {
                                bItemTrouve = false;
                                for (int elt = 0; elt < lListeITEMS.Count && !bItemTrouve; elt++)
                                {
                                    if (lListeITEMS[elt][2] == drRow[0].ToString())
                                    {
                                        if (lListeITEMS[elt][4] == string.Empty)
                                            lListeITEMS[elt][4] = "*";
                                        lListeITEMS[elt][4] += drRow[1].ToString() + "*";
                                        bItemTrouve = true;
                                    }
                                }
                            }
                        }

                        for (int elt = 0; elt < lListeITEMS.Count; elt++)
                        {
                            if ((lListeITEMS[elt][3] != lListeITEMS[elt][4]) && (lListeITEMS[elt][4] != string.Empty))
                            {
                                string sListeElement = VerifieListe(lListeITEMS[elt][4], lListeITEMS[elt][3]);

                                if (sListeElement != string.Empty)
                                {
                                    if (sListeElement.IndexOf(",") > -1)
                                        Process.AjouteRapport("Perte des niveaux de saisie " + sListeElement + " pour l'item " + lListeITEMS[elt][0] + " (DMD_COMPONENT " + lListeITEMS[elt][2] + ") livré dans le(s) pack(s) " + lListeITEMS[elt][1]);
                                    else
                                        Process.AjouteRapport("Perte du niveau de saisie " + sListeElement + " pour l'item " + lListeITEMS[elt][0] + " (DMD_COMPONENT " + lListeITEMS[elt][2] + ") livré dans le(s) pack(s) " + lListeITEMS[elt][1]);
                                    bResultat = ResultatErreur;
                                }
                            }
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

        /// <summary>  
        /// Méthode Comparant la liste des niveaux de saisie avant et après le pack. 
        /// <returns>Retourn un chaîne de caratère contenant la liste des niveaux de saisie qui ont été supprimés.</returns>
        /// </summary>  
        private string VerifieListe(string sListeAvant, string sListeApres)
        {
            string sResultat = string.Empty;
            string sElement;
            int iIndex = 1;
            int iIndexPrec = 0;

            iIndex = sListeAvant.IndexOf("*", 1);
            while (iIndex > -1)
            {
                sElement = sListeAvant.Substring(iIndexPrec, iIndex - iIndexPrec + 1);
                if (sListeApres.IndexOf(sElement) == -1)
                {
                    if (sResultat != string.Empty)
                        sResultat += ", ";
                    sResultat += sElement.Substring(1, sElement.Length - 2);
                }
                iIndexPrec = iIndex;
                iIndex = sListeAvant.IndexOf("*", iIndexPrec + 1);
            }

            return sResultat;
        }

    }
}
