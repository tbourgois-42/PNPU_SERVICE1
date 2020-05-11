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
    /// Cette classe permet de controler qu'il n'y a pas livraison d'un héritage d'un M4O ou d'une présentation alors qu'elle est déja héritée en standard. 
    /// Cela permet de vérifier que c'est bien le niveau standard le plus bas qui est hérité dans le pack.
    /// </summary>  
    class ControleNiveauHeritage : PControle, IControle
    {
        private PNPUCore.Process.ProcessControlePacks Process;
        private string ConnectionStringBaseRef;
        private List<string[]> lObjetsHeritesSTD = null;
        private List<string[]> lPresentsHeritesSTD = null;
        private string sListeID_T3 = string.Empty;
        private string sListeID_PRES = string.Empty;


        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleNiveauHeritage(PNPUCore.Process.IProcess pProcess)
        {
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef;
            Process = (PNPUCore.Process.ProcessControlePacks)pProcess;
            ToolTipControle = "Vérifie que les éléments livrés sont au niveau d'héritage le plus fin";
            LibControle = "Contrôle des niveaux d'héritage";
            ChargeM4OPresHerites();
        }

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        /// <param name="drRow">Enregistrement contnenant les informations sur le contrôle</param>
        public ControleNiveauHeritage(PNPUCore.Process.IProcess pProcess, DataRow drRow)
        {
            ConnectionStringBaseRef = ParamAppli.ConnectionStringBaseRef;
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
            DataSet dsDataSet = null;

            DataManagerAccess dmaManagerAccess = null;
            
            try
            {
                if (ConnectionStringBaseRef != string.Empty)
                {
                    
                    dmaManagerAccess = new DataManagerAccess();

                    // Contrôle des M4O hérités.
                    sRequete = "select OC.ID_T3, PMP.ID_PROJECT,PMP.ID_INSTANCE FROM SPR_DIN_OBJECTS OC inner join M4RDM_OS_PROJ_MEMS PMP on (OC.ID = PMP.ID_INSTANCE and PMP.ID_CLASS = 'DIN_OBJECT' AND PMP.ID_PROJECT NOT IN ('STANDARD','_M4ROOT','PLATFORM')) ";
                    sRequete += "WHERE OC.ID_T3 IN (" + sListeID_T3 + ")";

                    dsDataSet = dmaManagerAccess.GetData(sRequete,sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            int j = 0;

                            bResultat = ResultatErreur;
                            while (j < (lObjetsHeritesSTD.Count - 1) && (lObjetsHeritesSTD[j][0] != drRow[0].ToString())) j++;
                            if (lObjetsHeritesSTD[j][0] != drRow[0].ToString())
                                Process.AjouteRapport("Héritage de l'objet " + drRow[0].ToString() + " au niveau " + drRow[1].ToString() + " (" + drRow[2].ToString() + ") alors qu'il est hérité au niveau standard.");
                            else
                                Process.AjouteRapport("Héritage de l'objet " + drRow[0].ToString() + " au niveau " + drRow[1].ToString() + " (" + drRow[2].ToString() + ") alors qu'il est hérité au niveau " + lObjetsHeritesSTD[j][1] + " (" + lObjetsHeritesSTD[j][2] + ").");
                        }
                        dsDataSet.Clear();
                    }

                    // Contrôle des présentations héritées.
                    sRequete = "select  PC.ID_PRESENTATION,PMP.ID_PROJECT,PMP.ID_INSTANCE from SPR_DIN_PRESENTS PC  inner join M4RDM_OS_PROJ_MEMS PMP on (PC.ID = PMP.ID_INSTANCE and PMP.ID_CLASS = 'DIN_PRESENT' AND PMP.ID_PROJECT NOT IN ('STANDARD','_M4ROOT','PLATFORM')) ";
                    sRequete += "WHERE PC.ID_PRESENTATION IN (" + sListeID_PRES + ")";

                     dsDataSet = dmaManagerAccess.GetData(sRequete, sPathMdb);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            int j = 0;

                            bResultat = ResultatErreur;
                            while (j < (lPresentsHeritesSTD.Count - 1) && (lPresentsHeritesSTD[j][0] != drRow[0].ToString())) j++;
                            if (lPresentsHeritesSTD[j][0] != drRow[0].ToString())
                                Process.AjouteRapport("Héritage de la présentation " + drRow[0].ToString() + " au niveau " + drRow[1].ToString() + " (" + drRow[2].ToString() + ") alors qu'elle est héritée au niveau standard.");
                            else
                                Process.AjouteRapport("Héritage de la présentation " + drRow[0].ToString() + " au niveau " + drRow[1].ToString() + " (" + drRow[2].ToString() + ") alors qu'elle est héritée au niveau " + lPresentsHeritesSTD[j][1] + " (" + lPresentsHeritesSTD[j][2] + ").");
                        }
                        dsDataSet.Clear();
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

        /// <summary>  
        /// Méthode chargeant la liste des M4O et des présentation hérités en standard. 
        /// </summary>  
        private void ChargeM4OPresHerites()
        {
            string sRequete = string.Empty;
            DataSet dsDataSet = null;
            DataManagerSQLServer dmaManagersqlServer = null;
            lObjetsHeritesSTD = new List<string[]>();
            lPresentsHeritesSTD = new List<string[]>();

            try
            {
                if (ConnectionStringBaseRef != string.Empty)
                {
                    dmaManagersqlServer = new DataManagerSQLServer();
                    /* Récupération des M4O hérités au niveau standard */
                    sRequete = "select OS.ID_T3 AS ID_T3, PMS.ID_PROJECT AS ID_PROJECT, PMS.ID_INSTANCE AS ID_INSTANCE FROM SPR_DIN_OBJECTS OS inner join M4RDM_OS_PROJ_MEMS PMS on (OS.ID = PMS.ID_INSTANCE and PMS.ID_CLASS = 'DIN_OBJECT' AND PMS.ID_PROJECT IN ('STANDARD','_M4ROOT'))";
                    dsDataSet = dmaManagersqlServer.GetData(sRequete, ConnectionStringBaseRef);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            lObjetsHeritesSTD.Add(new string[] { drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString() });
                            if (sListeID_T3 != string.Empty)
                                sListeID_T3 += ",";
                            sListeID_T3 += "'" + drRow[0].ToString() + "'";
                        }
                        dsDataSet.Clear();
                    }

                    /* Récupération des présentations héritées au niveau standard */
                    sRequete = "select PS.ID_PRESENTATION,PMS.ID_PROJECT,PMS.ID_INSTANCE FROM SPR_DIN_PRESENTS  PS inner join M4RDM_OS_PROJ_MEMS PMS on (PS.ID = PMS.ID_INSTANCE and PMS.ID_CLASS = 'DIN_PRESENT' AND PMS.ID_PROJECT IN ('STANDARD','_M4ROOT'))";
                    dsDataSet = dmaManagersqlServer.GetData(sRequete, ConnectionStringBaseRef);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            lPresentsHeritesSTD.Add(new string[] { drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString() });
                            if (sListeID_PRES != string.Empty)
                                sListeID_PRES += ",";
                            sListeID_PRES += "'" + drRow[0].ToString() + "'";
                        }
                        dsDataSet.Clear();
                    }

                }
            }
            catch (Exception ex)
            {
                // TODO, loguer l'exception

            }
        }
    }
}
