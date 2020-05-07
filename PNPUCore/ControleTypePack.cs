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
    /// Cette classe permet de contrôler que les commandes des packs sont bien dans le bon type de pack (L,B,D ou F). 
    /// </summary>  
    class ControleTypePack : PControle, IControle
    {
        private List<string> lCMD_L;
        private List<string> lCMD_D;
        private List<string> lCMD_F;
        private List<string> lCMD_B;
        private PNPUCore.Process.ProcessControlePacks Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleTypePack(PNPUCore.Process.IProcess pProcess)
        {
            ToolTipControle = "Vérifie la cohérence des types de packages livrés. Exemple: Script de création de colonne physique dans un pack logique.";
            LibControle = "Contrôle des types de packages";
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            lCMD_L = ParamAppli.ListeCmdL;
            lCMD_D = ParamAppli.ListeCmdD;
            lCMD_F = ParamAppli.ListeCmdF;
            lCMD_B = ParamAppli.ListeCmdB;
        }

        /// <summary>  
        /// Méthode effectuant le contrôle. Elle va appeler la méthode ControleUnTypack pour chaque type de pack (L, B, D et F).
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            DataManagerAccess dmaManagerAccess = null;


            try
            {
                dmaManagerAccess = new DataManagerAccess();

                if (ControleUnTypack("_L", lCMD_L, dmaManagerAccess) == false)
                {
                    bResultat = ParamAppli.StatutError;
                }

                if (ControleUnTypack("_D", lCMD_D, dmaManagerAccess) == false)
                {
                    bResultat = ParamAppli.StatutError;
                }

                if (ControleUnTypack("_F", lCMD_F, dmaManagerAccess) == false)
                {
                    bResultat = ParamAppli.StatutError;
                }

                if (ControleUnTypack("_B", lCMD_B, dmaManagerAccess) == false)
                {
                    bResultat = ParamAppli.StatutError;
                }

            }
            catch (Exception ex)
            {
                // TODO, loguer l'exception
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;

        }

        /// <summary>  
        /// Méthode effectuant le contrôle pour un type de pack. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// <param name="TypePack">Contient une chaîne de caractère permettant de filtrer le type de pack à contrôler.</param>
        /// <param name="CMD">Contient la liste des commandes autorisées pour le type de pack à contrôler.</param>
        /// <param name="dmaManagerAccess">Objet permettant de faire des requêtes dans le MDB à contrôler.</param>
        /// </summary>  
        private bool ControleUnTypack(string TypePack, List<string> CMD, DataManagerAccess dmaManagerAccess )
        {
            bool bResultat = true;
            
            bool bTrouve;
            int iCpt;
            string sCommandPack;
            string sPathMdb = Process.MDBCourant;

            try
            {
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_PACKAGE,CMD_SEQUENCE,CMD_CODE FROM M4RDL_PACK_CMDS WHERE ID_PACKAGE LIKE '%" + TypePack + "' AND CMD_ACTIVE =-1", sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        iCpt = 0;
                        bTrouve = false;
                        sCommandPack = drRow[2].ToString().ToUpper().Trim();

                        // Je remplace les espaces et tabulation par un seul espace.
                        sCommandPack = System.Text.RegularExpressions.Regex.Replace(sCommandPack, "\\s+", " ");

                        while ((iCpt < CMD.Count()) && (bTrouve == false))
                        {
                            if (sCommandPack.IndexOf(CMD[iCpt++]) >= 0)
                                bTrouve = true;
                        }
                        if (bTrouve == false)
                        {
                             double dConv;

                            try
                            {
                                dConv = Convert.ToDouble(drRow[1].ToString());
                            }
                            catch
                            {
                                dConv = 0;
                            }
                            bResultat = false;
                            Process.AjouteRapport("La commande " + dConv.ToString("###0") + " du pack " + drRow[0].ToString() + " est interdite.");
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
