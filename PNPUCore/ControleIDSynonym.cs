using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que les ID_SYNONYME des items livrés ne sont pas dans les plages réservées aux clients. 
    /// </summary>  
    class ControleIDSynonym : PControle, IControle
    {
        private string sPathMdb = string.Empty;
        private List<int> lLIM_INF;
        private List<int> lLIM_SUP;
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleIDSynonym(PNPUCore.Process.IProcess pProcess)
        {
            lLIM_INF = ParamAppli.ListeLimInf;
            lLIM_SUP = ParamAppli.ListeLimSup;
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie si les items livrés dans le mdb ne sont pas livrés sur des plages d'ID Synonym réservées au client";
            LibControle = "Contrôle des ID Synoym";
            ResultatErreur = ParamAppli.StatutError;
            CompleteToolTip();
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleIDSynonym(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            lLIM_INF = ParamAppli.ListeLimInf;
            lLIM_SUP = ParamAppli.ListeLimSup;
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
            CompleteToolTip();
        }


        /// <summary>
        /// Complete le Tooltip avec la liste des plages réservées aux clients.
        /// </summary>
        private void CompleteToolTip()
        {
            ToolTipControle += ". Liste des plages réservées aux clients :";
            for (int i = 0; i < lLIM_INF.Count; i++)
            {
                ToolTipControle += " - De " + lLIM_INF[i].ToString() + " à " + lLIM_SUP[i].ToString();
            }
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        new public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            int iID_SYNONYM;
            string sPathMdb = Process.MDBCourant;

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess();
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE ID_TI LIKE '%HR%CALC' AND ID_TI NOT LIKE '%DIF%' AND ID_SYNONYM <> 0", sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        bool bPlageOK = true;

                        iID_SYNONYM = Int32.Parse(drRow[1].ToString());
                        for (int j = 0; j < lLIM_INF.Count && bPlageOK == true; j++)
                        {
                            if (iID_SYNONYM >= lLIM_INF[j] && iID_SYNONYM <= lLIM_SUP[j])
                                bPlageOK = false;

                        }
                        if (bPlageOK == false)
                        {
                            bResultat = ResultatErreur;
                            Process.AjouteRapport("L'ID_SYNONYM de l'item " + drRow[0].ToString() + "(" + drRow[1].ToString() + ") est dans les plages réservées client.");

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
