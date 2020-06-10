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
    /// Cette classe permet de controler que les commandes des packs ne font pas partie de la liste des commandes interdites. 
    /// </summary>  
    class ControleCmdInterdites : PControle, IControle
    {
        private List<string> lL_INTERDIT;
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleCmdInterdites(PNPUCore.Process.IProcess pProcess)
        {
            lL_INTERDIT = ParamAppli.ListeCmdInterdite;
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie si le mdb standard ne contient pas de commande interdite";
            LibControle = "Contrôle des commandes interdites";
            ResultatErreur = ParamAppli.StatutError;
            CompleteToolTip();
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleCmdInterdites(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            lL_INTERDIT = ParamAppli.ListeCmdInterdite;
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
            CompleteToolTip();
        }

        /// <summary>
        /// Complete le Tooltip avec la liste des élements interdits qui sont paramétérés.
        /// </summary>
        private void CompleteToolTip()
        {
            ToolTipControle +=  ". Liste des commandes interdites :";
            foreach(string sElt in lL_INTERDIT)
            {
                ToolTipControle += " - " + sElt;
            }
        }


        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            string sCommandPack = string.Empty;
            int iCpt = 0;
            bool bTrouve;
            string sPathMdb = Process.MDBCourant;


            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess();
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_PACKAGE,CMD_SEQUENCE,CMD_CODE FROM M4RDL_PACK_CMDS WHERE CMD_ACTIVE =-1", sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        sCommandPack = drRow[2].ToString().ToUpper().Trim();

                        // Je remplace les espaces et tabulation par un seul espace.
                        sCommandPack = System.Text.RegularExpressions.Regex.Replace(sCommandPack, "\\s+", " ");

                        iCpt = 0;
                        bTrouve = false;
                        while ((iCpt < lL_INTERDIT.Count()) && (bTrouve == false))
                        {
                            if (sCommandPack.IndexOf(lL_INTERDIT[iCpt++]) >= 0)
                            {
                                bTrouve = true;
                                bResultat = ResultatErreur;
                                double dConv;

                                try
                                {
                                    dConv = Convert.ToDouble(drRow[1].ToString());
                                }
                                catch
                                {
                                    dConv = 0;
                                }
                                bTrouve = true;
                                Process.AjouteRapport("La commande " + dConv.ToString("###0") + " du pack " + drRow[0].ToString() + " est interdite.");
                          
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
    }
}
