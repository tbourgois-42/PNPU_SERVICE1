using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de contrôler qu'aucun pack ne modifie d'objet techno. 
    /// </summary>  
    internal class ControleObjetTechno : PControle, IControle
    {
        readonly private PNPUCore.Process.ProcessControlePacks Process;
        readonly private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleObjetTechno(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie que le mdb standard ne livre pas d'élément techno. Les éléments techno ne doivent être livrés uniquement dans les HF Technos";
            LibControle = "Contrôle livraison d'objets techno";
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];
            ResultatErreur = ParamAppli.StatutError;
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleObjetTechno(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            LibControle = drRow[1].ToString();
            ToolTipControle = drRow[6].ToString();
            ResultatErreur = drRow[5].ToString();
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef[Process.TYPOLOGY];

        }

        /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        new public string MakeControl()
        {
            string bResultat = ParamAppli.StatutOk;
            string sPathMdb = Process.MDBCourant;
            string sRequete;
            string sIDPackageCourant;
            string sTempo;

            List<string> lListeM4O = new List<string>();
            List<string> lListeNODESTRUCTURE = new List<string>();

            DataManagerAccess dmaManagerAccess;

            try
            {
                dmaManagerAccess = new DataManagerAccess();


                // La modification d'un M4O peut provenir de la livraison d'un M4O, d'une NODE, NODE STRUCTURE ou d'un item. On recherche tous ces éléments.
                sRequete = "select A.ID_PACKAGE, A.ID_CLASS,A.ID_OBJECT FROM M4RDL_PACK_CMDS A ";
                sRequete += "WHERE (A.ID_PACKAGE LIKE '%_L' OR A.ID_PACKAGE LIKE '%_B') AND A.ID_CLASS IN ('META4OBJECT & NODE STRUCTURES','META4OBJECT','NODE STRUCTURE','NODE','ITEM') AND A.CMD_ACTIVE = -1 ";
                sRequete += "ORDER BY ID_PACKAGE ";
                sIDPackageCourant = String.Empty;
                DataSet dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {

                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        if (drRow[0].ToString() != sIDPackageCourant)
                        {
                            if ((sIDPackageCourant != String.Empty) && (lListeM4O.Count + lListeNODESTRUCTURE.Count > 0))
                            {
                                if (!ControleM4OModifiesPack(lListeM4O, lListeNODESTRUCTURE, sIDPackageCourant))
                                {
                                    bResultat = ResultatErreur;
                                }

                                lListeM4O.Clear();
                                lListeNODESTRUCTURE.Clear();
                            }
                            sIDPackageCourant = drRow[0].ToString();
                        }

                        switch (drRow[1].ToString())
                        {
                            case "META4OBJECT & NODE STRUCTURES":
                            case "META4OBJECT":
                                if (!lListeM4O.Contains(drRow[2].ToString()))
                                {
                                    lListeM4O.Add(drRow[2].ToString());
                                }

                                break;

                            case "NODE":
                                sTempo = drRow[2].ToString();
                                sTempo = sTempo.Substring(0, sTempo.IndexOf('.'));
                                if (!lListeM4O.Contains(sTempo))
                                {
                                    lListeM4O.Add(sTempo);
                                }

                                break;

                            case "NODE STRUCTURE":
                                if (!lListeNODESTRUCTURE.Contains(drRow[2].ToString()))
                                {
                                    lListeNODESTRUCTURE.Add(drRow[2].ToString());
                                }

                                break;

                            case "ITEM":
                                sTempo = drRow[2].ToString();
                                sTempo = sTempo.Substring(0, sTempo.IndexOf('.'));
                                if (!lListeNODESTRUCTURE.Contains(sTempo))
                                {
                                    lListeNODESTRUCTURE.Add(sTempo);
                                }

                                break;
                        }

                    }

                    if (lListeM4O.Count + lListeNODESTRUCTURE.Count > 0)
                    {
                        if (!ControleM4OModifiesPack(lListeM4O, lListeNODESTRUCTURE, sIDPackageCourant))
                        {
                            bResultat = ResultatErreur;
                        }

                        lListeM4O.Clear();
                        lListeNODESTRUCTURE.Clear();
                    }

                    //Process.AjouteRapport("Livraison de la table " + sCle + " dans le pack " + dListeAControler[sCle] + " sans mise à jour du catalogue des tables.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
                bResultat = ParamAppli.StatutError;
            }

            return bResultat;

        }

        /// <summary>  
        /// Méthode vérifiant que les M4O modifiés par un pack ne sont pas des objets technos. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// <param name="lListeM4O">Contient la liste des M4O livrés et des M4O des NODES livrés.</param>
        /// <param name="lListeNODESTRUCTURE">Contient la liste des NODE STRUCTURES livrées et des NODES STRUCTURES des ITEM livrés.</param>
        /// <param name="sIDPackageCourant">Contient le nom du pack courant.</param>
        /// </summary>  
        private bool ControleM4OModifiesPack(List<string> lListeM4O, List<string> lListeNODESTRUCTURE, string sIDPackageCourant)
        {
            DataManagerAccess dmaManagerAccess;
            DataManagerSQLServer dmsManagerSQL;
            bool bPremier;
            string sRequete;
            string sPathMdb = Process.MDBCourant;
            DataSet dsDataSet;
            bool bResultat = true;

            try
            {
                dmsManagerSQL = new DataManagerSQLServer();
                dmaManagerAccess = new DataManagerAccess();

                // Si la liste n'est pas vide on va rechercher les M4O à partir des NODE STRUCTURES modifiées.
                if (lListeNODESTRUCTURE.Count > 0)
                {
                    sRequete = "select ID_T3,ID_TI from M4RCH_NODES where ID_TI IN (";
                    bPremier = true;
                    foreach (string s in lListeNODESTRUCTURE)
                    {
                        if (bPremier)
                        {
                            bPremier = false;
                        }
                        else
                        {
                            sRequete += ",";
                        }

                        sRequete += "'" + s + "'";
                    }
                    sRequete += ") UNION select ID_NODE_T3 AS ID_T3,ID_TI from M4RCH_OVERWRITE_NO where ID_TI IN (";
                    bPremier = true;
                    foreach (string s in lListeNODESTRUCTURE)
                    {
                        if (bPremier)
                        {
                            bPremier = false;
                        }
                        else
                        {
                            sRequete += ",";
                        }

                        sRequete += "'" + s + "'";
                    }
                    sRequete += ")";


                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            lListeNODESTRUCTURE.Remove(drRow[1].ToString()); // Je supprime les NS dont on trouve le M4O dans le MDB
                            if (!lListeM4O.Contains(drRow[0].ToString()))
                            {
                                lListeM4O.Add(drRow[0].ToString());
                            }
                        }
                        dsDataSet.Clear();
                    }

                    // S'il reste des NODE STRUCTURES pour lesquelles nous n'avons pas trouvé le M4O dans le MDB on recherche dans la base de ref.
                    if (lListeNODESTRUCTURE.Count > 0)
                    {
                        sRequete = "select ID_T3,ID_TI from M4RCH_NODES where ID_TI IN (";
                        bPremier = true;
                        foreach (string s in lListeNODESTRUCTURE)
                        {
                            if (bPremier)
                            {
                                bPremier = false;
                            }
                            else
                            {
                                sRequete += ",";
                            }

                            sRequete += "'" + s + "'";
                        }
                        sRequete += ") UNION select ID_NODE_T3 AS ID_T3,ID_TI from M4RCH_OVERWRITE_NO where ID_TI IN (";
                        bPremier = true;
                        foreach (string s in lListeNODESTRUCTURE)
                        {
                            if (bPremier)
                            {
                                bPremier = false;
                            }
                            else
                            {
                                sRequete += ",";
                            }

                            sRequete += "'" + s + "'";
                        }
                        sRequete += ")";

                        if (ConnectionStringBaseRef != string.Empty)
                        {
                            dsDataSet = dmsManagerSQL.GetData(sRequete, ConnectionStringBaseRef);
                            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                            {
                                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                                {
                                    lListeNODESTRUCTURE.Remove(drRow[1].ToString()); // Je supprime les NS dont on trouve le M4O dans le MDB
                                    if (!lListeM4O.Contains(drRow[0].ToString()))
                                    {
                                        lListeM4O.Add(drRow[0].ToString());
                                    }
                                }
                                dsDataSet.Clear();
                            }
                        }
                    }
                }

                // On va rechercher les M4O de la liste pour vérifier si ce sont des objets technos.

                // On cherche déja dans le MDB
                if (lListeM4O.Count > 0)
                {
                    sRequete = "select ID_T3, N_T3FRA, OWNER_FLAG from M4RCH_T3S where ID_T3 IN (";
                    bPremier = true;
                    foreach (string s in lListeM4O)
                    {
                        if (bPremier)
                        {
                            bPremier = false;
                        }
                        else
                        {
                            sRequete += ",";
                        }

                        sRequete += "'" + s + "'";
                    }
                    sRequete += ")";

                    dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            lListeM4O.Remove(drRow[0].ToString()); // Je supprime les M4O trouvés dans le MDB

                            // Controle si c'est un objet techno
                            if ((drRow[2].ToString() == "1") || (drRow[0].ToString().Substring(0, 4) == "SRTC"))
                            {
                                Process.AjouteRapport("Modification de l'objet techno " + drRow[0].ToString() + " dans le pack " + sIDPackageCourant + ".");
                                bResultat = false;
                            }
                        }
                    }

                    // S'il reste des M4O non trouvés dans le MDB on va chercher dans la base de ref.
                    if (lListeM4O.Count > 0)
                    {
                        sRequete = "select ID_T3, N_T3FRA, OWNER_FLAG from M4RCH_T3S where ID_T3 IN (";
                        bPremier = true;
                        foreach (string s in lListeM4O)
                        {
                            if (bPremier)
                            {
                                bPremier = false;
                            }
                            else
                            {
                                sRequete += ",";
                            }

                            sRequete += "'" + s + "'";
                        }
                        sRequete += ")";

                        if (ConnectionStringBaseRef != string.Empty)
                        {
                            dsDataSet = dmsManagerSQL.GetData(sRequete, ConnectionStringBaseRef);
                            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                            {
                                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                                {
                                    lListeM4O.Remove(drRow[0].ToString()); // Je supprime les M4O trouvés dans le MDB

                                    // Controle si c'est un objet techno
                                    if ((drRow[2].ToString() == "1") || (drRow[0].ToString().Substring(0, 4) == "SRTC"))
                                    {
                                        Process.AjouteRapport("Modification de l'objet techno " + drRow[0].ToString() + " dans le pack " + sIDPackageCourant + ".");
                                        bResultat = false;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine(ex.Message);
                bResultat = true;
            }

            return bResultat;
        }
    }
}
