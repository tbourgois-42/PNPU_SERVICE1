using System;
using System.Collections.Generic;
using System.Text;
using PNPUTools.DataManager;
using System.Data;


namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que les tâches livrées dans le packs sont sécurisées. 
    /// </summary>  
    class ControleTacheSecu : IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleTacheSecu(PNPUCore.Process.IProcess pProcess)
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

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess();
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_BP FROM M4RBP_DEF  WHERE SECURITY_TYPE <> 2", sPathMdb); 
                
                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    bResultat = false;
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        Process.AjouteRapport("Tâche " + drRow[0].ToString() + " non sécurisée.");
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


