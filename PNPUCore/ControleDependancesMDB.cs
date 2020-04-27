using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;
using System.IO;
using PNPUTools;

namespace PNPUCore.Controle
{
    /// <summary>  
    /// Cette classe permet de controler les dépendances entre les packs des mdb. 
    /// </summary>  
    class ControleDependancesMDB : PControle, IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;
        private string ConnectionStringBaseRef;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleDependancesMDB(PNPUCore.Process.IProcess pProcess)
        {
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef;
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
        }

         /// <summary>  
        /// Méthode effectuant le contrôle. 
        /// <returns>Retourne un booléen, vrai si le contrôle est concluant et sinon faux.</returns>
        /// </summary>  
        public bool MakeControl()
        {
            bool bResultat = true;
            string sRequete;
            Dictionary<string,string> dListeAControler = new Dictionary<string,string>();
            string sRequeteControle = string.Empty;
            bool bPremierElement = true;

            DataSet dsDataSet = null;
            
            string sNomMdb1;
            string sNomMdb2;

            DataManagerAccess dmaManagerAccess1 = null;
            DataManagerAccess dmaManagerAccess2 = null;

            try
            {
                dmaManagerAccess1 = new DataManagerAccess();
                dmaManagerAccess2 = new DataManagerAccess();

                foreach (string sPathMdb1 in Process.listMDB)
                {
                    sNomMdb1 = Path.GetFileName(sPathMdb1);

                    // Recherche des mêmes éléments livrés dans 2 packs d'un mdb
                    sRequete = "select A.ID_PACKAGE, A.ID_CLASS, A.ID_OBJECT, B.ID_PACKAGE FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE (A.ID_PACKAGE LIKE '%_L' OR A.ID_PACKAGE LIKE '%_B') AND A.ID_CLASS = B.ID_CLASS AND A.ID_OBJECT = B.ID_OBJECT AND A.ID_PACKAGE <> B.ID_PACKAGE AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                    ChercheDependanceUnMDB(ref bPremierElement, false, sRequete, sPathMdb1, dmaManagerAccess1);

                    // Recherche dépendance Payroll Item et item dans le même mdb
                    sRequete = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B, M4RCH_PICOMPONENTS P WHERE A.ID_CLASS='PAYROLL ITEM' AND P.ID_T3 + '.' + P.ID_PAYROLL_ITEM= A.ID_OBJECT AND B.ID_CLASS='ITEM' AND B.ID_OBJECT=P.ID_TI + '.' + P.ID_ITEM AND A.ID_PACKAGE <> B.ID_PACKAGE AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE= -1";
                    ChercheDependanceUnMDB(ref bPremierElement, true, sRequete, sPathMdb1, dmaManagerAccess1);

                    // Recherche dépendance Node Structure et item dans le même mdb
                    sRequete = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='ITEM' AND B.ID_CLASS='NODE STRUCTURE' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE  AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                    ChercheDependanceUnMDB(ref bPremierElement, true, sRequete, sPathMdb1, dmaManagerAccess1);

                    // Recherche dépendance Field et Logical Table dans le même mdb
                    sRequete = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='FIELD' AND B.ID_CLASS='LOGICAL TABLE' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE  AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                    ChercheDependanceUnMDB(ref bPremierElement, true, sRequete, sPathMdb1, dmaManagerAccess1);

                    // Recherche dépendance Node et Meta4Objet dans le même mdb
                    sRequete = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='NODE' AND B.ID_CLASS='META4OBJECT' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE  AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                    ChercheDependanceUnMDB(ref bPremierElement, true, sRequete, sPathMdb1, dmaManagerAccess1);
                }

                // recherche des dépendances avec les autre mdb.
                for (int iIndex = 0; iIndex < Process.listMDB.Count - 1; iIndex++)
                {
                    sRequete = "select ID_PACKAGE, ID_CLASS, ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_PACKAGE LIKE '%_L' OR ID_PACKAGE LIKE '%_B' AND CMD_ACTIVE = -1";
                    dsDataSet = dmaManagerAccess1.GetData(sRequete, Process.listMDB[iIndex]);
                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            for (int iIndex2 = iIndex + 1; iIndex2 < Process.listMDB.Count; iIndex2++)
                            {
                                // Recherche des mêmes éléments livrés dans 2 packs de 2 mdb
                                sRequete = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE (A.ID_PACKAGE LIKE '%_L' OR A.ID_PACKAGE LIKE '%_B') AND A.ID_CLASS = '" + drRow[1].ToString() + "' AND A.ID_OBJECT = '" + drRow[2].ToString() + "' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());

                                switch (drRow[1].ToString())
                                {
                                    case "ITEM":
                                        // Recherche des dépendances entre Item et Node structure livrés dans 2 packs de 2 mdb
                                        string sNodeStructure = drRow[2].ToString().Substring(0, drRow[2].ToString().IndexOf("."));
                                        sRequete = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'NODE STRUCTURE' AND A.ID_OBJECT = '" + sNodeStructure + "' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());

                                        // Recherche des dépendances entre Item et payroll item livrés dans 2 packs de 2 mdb
                                        sRequete = "select A.ID_PACKAGE, A.ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A, M4RCH_PICOMPONENTS P WHERE A.ID_CLASS = 'PAYROLL ITEM' AND P.ID_T3 + '.' + P.ID_PAYROLL_ITEM= A.ID_OBJECT AND P.ID_TI + '.' + P.ID_ITEM = '" + drRow[2].ToString() + "' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());

                                        break;

                                    case "NODE STRUCTURE":
                                        // Recherche des dépendances entre Node structure et Item livrés dans 2 packs de 2 mdb
                                        sRequete = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'ITEM' AND A.ID_OBJECT LIKE '" + drRow[2].ToString() + ".%' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());

                                        break;

                                    case "FIELD":
                                        // Recherche des dépendances entre Field et Logical Table livrés dans 2 packs de 2 mdb
                                        string sTable = drRow[2].ToString().Substring(0, drRow[2].ToString().IndexOf("."));
                                        sRequete = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'LOGICAL TABLE' AND A.ID_OBJECT = '" + sTable + "' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());
                                        
                                        break;

                                    case "LOGICAL TABLE":
                                        // Recherche des dépendances entre Logical Table et Field livrés dans 2 packs de 2 mdb
                                        sRequete = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'FIELD' AND A.ID_OBJECT LIKE '" + drRow[2].ToString() + ".%' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());

                                        break;

                                    case "NODE":
                                        // Recherche des dépendances entre Node et M4O livrés dans 2 packs de 2 mdb
                                        string sNode = drRow[2].ToString().Substring(0, drRow[2].ToString().IndexOf("."));
                                        sRequete = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'META4OBJECT' AND A.ID_OBJECT = '" + sNode + "' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());

                                        break;

                                    case "META4OBJECT":
                                        // Recherche des dépendances entre M4O et Node livrés dans 2 packs de 2 mdb
                                        sRequete= "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'NODE' AND A.ID_OBJECT LIKE '" + drRow[2].ToString() + ".%' AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                        ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());
                                        break;

                                    case "PAYROLL ITEM":
                                        // Recherche des dépendances entre Payroll Item et Item livrés dans 2 packs de 2 mdb
                                        string sListeComposant = string.Empty;
                                        DataSet dsDataSet2;

                                        dsDataSet2 = dmaManagerAccess2.GetData("select ID_TI +'.' + ID_ITEM FROM M4RCH_PICOMPONENTS WHERE ID_T3 + '.' + ID_PAYROLL_ITEM = '" + drRow[2].ToString() + "'", Process.listMDB[iIndex]);
                                        if ((dsDataSet2 != null) && (dsDataSet2.Tables[0].Rows.Count > 0))
                                        {
                                            foreach(DataRow drRow2 in dsDataSet2.Tables[0].Rows)
                                            {
                                                if (sListeComposant == string.Empty)
                                                    sListeComposant = "('" + drRow2[0].ToString() + "'";
                                                else
                                                    sListeComposant += ",'" + drRow2[0].ToString() + "'";
                                            }
                                        }
                                        if (sListeComposant != string.Empty)
                                        {
                                            sListeComposant += ")";
                                            sRequete= "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'ITEM' AND A.ID_OBJECT IN " + sListeComposant + " AND A.ID_PACKAGE <> '" + drRow[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                            ChercheDependanceEntreMDB(ref bPremierElement, sRequete, Process.listMDB[iIndex2], dmaManagerAccess2, Process.listMDB[iIndex], drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString());
                                        }

                                        break;
                                }
                            }
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

        /// <summary>  
        /// Fonction cherchant des dépendances entre différents packs d'un MDB. 
        /// </summary>  
        /// <param name="bPremierElement">Booleen passé en référence. Utilisé pour savoir s'il faut écrire la ligne d'entête.</param>
        /// <param name="bGereInversion">Booléen indiquant s'il faut indiquer la dépendance également dans l'autre sens.</param>
        /// <param name="sRequete">Requête à lancer dans le mdb.</param>
        /// <param name="sPathMdb">Chemin complet du mdb.</param>
        /// <param name="dmaManagerAccess">Objet gérant les accès à la base Acces</param>
        private void ChercheDependanceUnMDB(ref bool bPremierElement, bool bGereInversion, string sRequete, string sPathMdb, DataManagerAccess dmaManagerAccess)
        {
            string sNomMdb = Path.GetFileName(sPathMdb);

            DataSet dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    if (bGereInversion == true)
                        AjouteDependance(ref bPremierElement, bGereInversion, sNomMdb, drRow[0].ToString(), sNomMdb, drRow[3].ToString(), drRow[1].ToString(), drRow[5].ToString(), drRow[2].ToString(), drRow[4].ToString());
                    else
                        AjouteDependance(ref bPremierElement, bGereInversion, sNomMdb, drRow[0].ToString(), sNomMdb, drRow[3].ToString(), drRow[1].ToString(), drRow[1].ToString(), drRow[2].ToString(), drRow[2].ToString());

                }
            }

        }

        /// <summary>  
        /// Fonction cherchant des dépendances entre différents packs de différents MDB. 
        /// </summary>  
        /// <param name="bPremierElement">Booleen passé en référence. Utilisé pour savoir s'il faut écrire la ligne d'entête.</param>
        /// <param name="sRequete">Requête à lancer dans le mdb.</param>
        /// <param name="sPathMdb">Chemin complet du mdb.</param>
        /// <param name="dmaManagerAccess">Objet gérant les accès à la base Acces</param>
        private void ChercheDependanceEntreMDB(ref bool bPremierElement, string sRequete, string sPathMdb, DataManagerAccess dmaManagerAccess, string sPathMdb1, string sPack1, string sClasse1, string sElt1)
        {
            string sNomMdb1 = Path.GetFileName(sPathMdb1);
            string sNomMdb2 = Path.GetFileName(sPathMdb);

            DataSet dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    AjouteDependance(ref bPremierElement, true, sNomMdb1, sPack1, sNomMdb2, drRow[0].ToString(), sClasse1, drRow[1].ToString(), sElt1, drRow[2].ToString());
                 }
            }

        }

        /// <summary>  
        /// Fonction ajoutant une information de dépendance dans le rapport. 
        /// </summary>  
        /// <param name="bPremierElement">Booleen passé en référence. Utilisé pour savoir s'il faut écrire la ligne d'entête.</param>
        /// <param name="bGereInversion">Booléen indiquant s'il faut indiquer la dépendance également dans l'autre sens.</param>
        /// <param name="sNomMdb1">Nom du mdb du premier élément de la dépendance.</param>
        /// <param name="sNomPack1">Nom du pack du premier élément de la dépendance.</param>
        /// <param name="sNomMdb2">Nom du mdb du deuxième élément de la dépendance.</param>
        /// <param name="sNomPack2">Nom du pack du deuxième élément de la dépendance.</param>
        /// <param name="sClasse1">Classe du premier élément de la dépendance.</param>
        /// <param name="sClasse2">Classe du deuxième élément de la dépendance.</param>
        /// <param name="sElt1">Nom du premier élément de la dépendance.</param>
        /// <param name="sElt2">Nom du deuxième élément de la dépendance.</param>
        private void AjouteDependance(ref bool  bPremierElement, bool bGereInversion, string sNomMdb1, string sNomPack1, string sNomMdb2, string sNomPack2, string sClasse1, string sClasse2, string sElt1, string sElt2 )
        {
            // On ne prend pas en compte les dépendances sur le bulletin électronique
            if ((sClasse1 == "PRESENTATION") && (sElt1.IndexOf("DP_PAYROLL_CHANNEL") > -1))
                return;

            // Si c'est le premier élément j'ajoute l'entête
            if (bPremierElement == true)
            {
                Process.AjouteRapport("Mdb;Pack;Mdb2;Pack2;Classe elt1 / Classe elt2;Elt1;Elt2");
                bPremierElement = false;
            }

            Process.AjouteRapport(sNomMdb1 + ";" + sNomPack1 + ";" + sNomMdb2 + ";" + sNomPack2 + ";" + sClasse1 + " / " + sClasse2 + ";" + sElt1 + ";" + sElt2);
            
            if (bGereInversion == true)
                Process.AjouteRapport(sNomMdb2 + ";" + sNomPack2 + ";" + sNomMdb1 + ";" + sNomPack1 + ";" + sClasse2 + " / " + sClasse1 + ";" + sElt2 + ";" + sElt1);

        }
    }
}
