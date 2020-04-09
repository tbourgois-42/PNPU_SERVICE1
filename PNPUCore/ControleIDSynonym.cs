using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;
using PNPUTools;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler que les ID_SYNONYME des items livrés ne sont pas dans les plages réservées aux clients. 
    /// </summary>  
    class ControleIDSynonym : IControle
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
        }

         /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public bool MakeControl()
        {
            bool bResultat = true;
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
                            bResultat = false;
                            Process.AjouteRapport("L'ID_SYNONYM de l'item " + drRow[0].ToString() + "(" + drRow[1].ToString() + ") est dans les plages réservées client.");
                                
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
