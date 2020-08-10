using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler que des clés et des sections dans les paramètres applicatifs ne sont pas livrés dans un pack. 
    /// </summary>  
    class ControleParamAppli : PControle, IControle
    {
        readonly string sCLE = string.Empty;
        readonly string sSECTION = string.Empty;
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
                if (sCLE != string.Empty)
                    sCLE += ",";
                sCLE += "'" + cle + "'";
            }

            foreach (string section in ParamAppli.ListeSectionInterdite)
            {
                if (sSECTION != string.Empty)
                    sSECTION += ",";
                sSECTION += "'" + section + "'";
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
                if (sCLE != string.Empty)
                    sCLE += ",";
                sCLE += "'" + cle + "'";
            }

            foreach (string section in ParamAppli.ListeSectionInterdite)
            {
                if (sSECTION != string.Empty)
                    sSECTION += ",";
                sSECTION += "'" + section + "'";
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
            ToolTipControle += ". Liste des clés interdites :";
            foreach (string sElt in ParamAppli.ListeCleInterdite)
            {
                ToolTipControle += " - " + sElt;
            }
            ToolTipControle += " - Liste des sections interdites :";
            foreach (string sElt in ParamAppli.ListeSectionInterdite)
            {
                ToolTipControle += " - " + sElt;
            }
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


            DataManagerAccess dmaManagerAccess = null;
            try
            {
                if (sCLE != string.Empty)
                {
                    sCommandPack = "ID_KEY IN (" + sCLE + ")";
                }

                if (sSECTION != string.Empty)
                {
                    if (sCommandPack != string.Empty)
                        sCommandPack += " OR ";
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
