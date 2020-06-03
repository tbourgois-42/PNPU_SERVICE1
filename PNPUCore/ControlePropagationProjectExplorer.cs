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
    /// Cette classe permet de contrôler que les données livrées dans des tables multi orga sont propagées. 
    /// </summary>  
    class ControlePropaProjectExplorer : PControle, IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControlePropaProjectExplorer(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie si les données du project explorer livrées sur SOC_0001 sont propagées sur SOC_0002";
            LibControle = "Contrôle propagation dans project explorer";
            ResultatErreur = ParamAppli.StatutWarning;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControlePropaProjectExplorer(PNPUCore.Process.IProcess pProcess, DataRow drRow)
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
            DataSet dsDataSet = null;
 
            try
            {
                dmaManagerAccess = new DataManagerAccess();

                // Recherche des éléments livrés sur SOC_0001 et pas SOC_0002.
                sRequete = "SELECT A.ID_PROJECT, A.ID_CLASS, A.ID_INSTANCE ";
                sRequete += "FROM M4RDM_OS_PROJ_MEMS A ";
                sRequete += "WHERE A.ID_PROJECT = 'SOC_0001' AND ";
                sRequete += "NOT EXISTS(SELECT * FROM M4RDM_OS_PROJ_MEMS B WHERE B.ID_PROJECT = 'SOC_0002' AND B.ID_CLASS = A.ID_CLASS AND B.ID_INSTANCE = A.ID_INSTANCE)";

                dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    bResultat = ResultatErreur;
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        Process.AjouteRapport("Livraison de l'élément " + drRow[2].ToString() + " de classe " + drRow[1].ToString() + " sur SOC_0001 sans livraison sur SOC_0002.");
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
