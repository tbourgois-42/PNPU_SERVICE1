using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using PNPUTools;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler que des clés et des sections dans les paramètres applicatifs ne sont pas livrés dans un pack. 
    /// </summary>  
    class ControleParamAppli : PControle, IControle
    {
        string sCLE = string.Empty;
        string sSECTION = string.Empty;
        private PNPUCore.Process.ProcessControlePacks Process;

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

            Process = (PNPUCore.Process.ProcessControlePacks) pProcess;
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
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public string MakeControl()
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
                // TODO, loguer l'exception
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;

        }
    }
}
