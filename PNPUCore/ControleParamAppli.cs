using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;
using System.Text;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler que des clés et des sections dans les paramètres applicatifs ne sont pas livrés dans un pack. 
    /// </summary>  
    internal class ControleParamAppli : PControle, IControle
    {
        private readonly StringBuilder sCLE = new StringBuilder();
        private readonly StringBuilder sSECTION = new StringBuilder();
        readonly private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleParamAppli(PNPUCore.Process.IProcess pProcess)
        {
            ToolTipControle = "Vérifie que le mdb standard ne livre pas des paramètres applicatifs non autorisés";
            LibControle = "Contrôle des paramètres applicatifs";
            foreach (string cle in ParamAppli.ListeCleInterdite)
            {
                if (sCLE.Length > 0)
                {
                    sCLE.Append(",");
                }

                sCLE.Append("'" + cle + "'");
            }

            foreach (string section in ParamAppli.ListeSectionInterdite)
            {
                if (sSECTION.Length >0)
                {
                    sSECTION.Append(",");
                }

                sSECTION.Append("'" + section + "'");
            }

            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ResultatErreur = ParamAppli.StatutError;
            CompleteToolTip();
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleParamAppli(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            foreach (string cle in ParamAppli.ListeCleInterdite)
            {
                if (sCLE.Length > 0)
                {
                    sCLE.Append(",");
                }

                sCLE.Append("'" + cle + "'");
            }

            foreach (string section in ParamAppli.ListeSectionInterdite)
            {
                if (sSECTION.Length > 0)
                {
                    sSECTION.Append(",");
                }

                sSECTION.Append("'" + section + "'");
            }
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
            CompleteToolTip();
        }

        /// <summary>
        /// Complete le Tooltip avec la liste clés et sections interdites.
        /// </summary>
        private void CompleteToolTip()
        {
            StringBuilder sbToolTipControle = new StringBuilder();

            sbToolTipControle.Append(ToolTipControle);
            sbToolTipControle.Append(". Liste des clés interdites :");

            foreach (string sElt in ParamAppli.ListeCleInterdite)
            {
                sbToolTipControle.Append(" - " + sElt);
            }
            sbToolTipControle.Append(" - Liste des sections interdites :");
            foreach (string sElt in ParamAppli.ListeSectionInterdite)
            {
                sbToolTipControle.Append(" - " + sElt);
            }

            ToolTipControle = sbToolTipControle.ToString();
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        new public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            string sCommandPack = string.Empty;
            string sPathMdb = Process.MDBCourant;


            DataManagerAccess dmaManagerAccess;
            try
            {
                if (sCLE.Length > 0)
                {
                    sCommandPack = "ID_KEY IN (" + sCLE + ")";
                }

                if (sSECTION.Length > 0)
                {
                    if (sCommandPack != string.Empty)
                    {
                        sCommandPack += " OR ";
                    }

                    sCommandPack += "ID_SECTION IN(" + sSECTION + ")";
                }

                if (sCommandPack != string.Empty)
                {
                    sCommandPack = "SELECT DISTINCT ID_SECTION + '\\' + ID_KEY AS CLE FROM  M4RAV_APP_VAL_LG1 WHERE " + sCommandPack;
                    dmaManagerAccess = new DataManagerAccess();
                    DataSet dsDataSet = dmaManagerAccess.GetData(sCommandPack, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            // TODO Loguer les paramètres applicatyif livrés à tort
                            bResultat = ResultatErreur;
                            Process.AjouteRapport("Livraison du paramètre applicatif " + drRow[0].ToString() + " interdite.");

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
