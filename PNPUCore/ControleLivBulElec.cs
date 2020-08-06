using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler que si le bulletin électronique SCO_DP_PAYROLL_CHANNEL est livré. 
    /// </summary>  
    class ControleLivBulElec : PControle, IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleLivBulElec(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie si la présentation SCO_DP_PAYROLL_CHANNEL est livrée";
            LibControle = "Contrôle si livraison de la présentation SCO_DP_PAYROLL_CHANNEL";
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleLivBulElec(PNPUCore.Process.IProcess pProcess, DataRow drRow)
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

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess();
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_PACKAGE FROM M4RDL_PACK_CMDS WHERE UCase(CMD_CODE) LIKE '%TRANSFER%PRESENTATION%SCO_DP_PAYROLL_CHANNEL%' AND CMD_ACTIVE=-1", sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    bResultat = ResultatErreur;
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        Process.AjouteRapport("La présentation SCO_DP_PAYROLL_CHANNEL est livrée dans le pack " + drRow[0].ToString() + ".");
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
