using System;
using System.Collections.Generic;
using System.Text;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUTools
{

    /// <summary>  
    /// Cette classe static charge les paramètres de l'application et permet aux objets de récupérer les différentes valeurs directement.
    /// </summary>  
    public static class ParamAppli
    {
        /// <summary>  
        /// Liste des commandes dont la présence est interdite dans les packs.
        /// </summary>  
        public static List<string> ListeCmdInterdite { get; }

        /// <summary>  
        /// Liste des valeurs inférieures des plages ID_SYNONYM réservées aux clients.
        /// </summary>  
        public static List<int> ListeLimInf { get; }

        /// <summary>  
        /// Liste des valeurs supérieures des plages ID_SYNONYM réservées aux clients.
        /// </summary> 
        public static List<int> ListeLimSup { get; }

        /// <summary>  
        /// Liste des clés des paramètres applicatifs qu'il est interdit de livrer.
        /// </summary> 
        public static List<string> ListeCleInterdite { get; }

        /// <summary>  
        /// Liste des sections des paramètres applicatifs qu'il est interdit de livrer.
        /// </summary> 
        public static List<string> ListeSectionInterdite { get; }

        /// <summary>  
        /// Liste des commandes autorisées dans le packs de type "L".
        /// </summary> 
        public static List<string> ListeCmdL { get; }
        
        /// <summary>  
        /// Liste des commandes autorisées dans le packs de type "D".
        /// </summary> 
        public static List<string> ListeCmdD { get; }

        /// <summary>  
        /// Liste des commandes autorisées dans le packs de type "F".
        /// </summary> 
        public static List<string> ListeCmdF { get; }

        /// <summary>  
        /// Liste des commandes autorisées dans le packs de type "B".
        /// </summary> 
        public static List<string> ListeCmdB { get; }

        /// <summary>  
        /// Chaine de connexion à la base de référence.
        /// </summary> 
        public static string ConnectionStringBaseRef { get; }

        /// <summary>  
        /// Chaine de connexion à la base de référence SAAS dédié.
        /// </summary>
        public static string ConnectionStringBaseRefDedie { get; }

        /// <summary>  
        /// Chaine de connexion à la base de référence SAAS mutualisé / désynchro.
        /// </summary>
        public static string ConnectionStringBaseRefPlateforme { get; }

        /// <summary>  
        /// Chaine de connexion à la base de l'application.
        /// </summary> 
        public static string ConnectionStringBaseAppli { get; }
        
        /// <summary>  
        /// Chaine de connexion à la base de du support.
        /// </summary> 
        public static string ConnectionStringSupport { get; }

        /// <summary>
        /// Dossier temporaire utilisé pour l'application.
        /// </summary>
        public static string DossierTemporaire { get; }


        public static string ProcessAnalyseImpact { get; }
        public static string ProcessGestionDependance { get; }
        public static string ProcessIntegration { get; }
        public static string ProcessProcessusCritique { get; }
        public static string ProcessTNR { get; }
        public static string ProcessLivraison { get; }
        public static string ProcessControlePacks { get; }

        
        /// <summary>  
        /// Constructeur de la classe. Il charge toutes les valeurs du paramétrage.
        /// </summary>  
        static ParamAppli()
        {
            DataSet dsDataSet = null;
            DataManagerSQLServer dataManagerSQLServer = new DataManagerSQLServer();

            ProcessAnalyseImpact = "ProcesAnalyseImpact";
            ProcessGestionDependance = "ProcessGestionDependance";
            ProcessIntegration = "ProcessIntegration";
            ProcessProcessusCritique = "ProcessProcessusCritique";
            ProcessTNR = "ProcessTNR";
            ProcessLivraison = "ProcessLivraison";
            ProcessControlePacks = "ProcessControlePacks";
            DossierTemporaire = "C:\\TEMPO";
            // A lire dans base de ref
            ListeCmdInterdite = new List<string>();
            ListeCleInterdite = new List<string>();
            ListeSectionInterdite = new List<string>();
            ListeCmdL = new List<string>();
            ListeCmdD = new List<string>();
            ListeCmdF = new List<string>();
            ListeCmdB = new List<string>();
            ListeLimInf = new List<int>();
            ListeLimSup = new List<int>();


            try
            {
                ConnectionStringBaseAppli = "server=M4FRDB18;uid=CAPITAL_DEV;pwd=Cpldev2017;database=CAPITAL_DEV;";
                dsDataSet = dataManagerSQLServer.GetData("SELECT PARAMETER_ID,PARAMETER_VALUE FROM PNPU_PARAMETERS ORDER BY PARAMETER_ID", ConnectionStringBaseAppli);

                if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        switch (drRow[0].ToString().Substring(0, 7))
                        {
                            case "INTERD_":
                                ListeCmdInterdite.Add(drRow[1].ToString());
                                break;

                            case "PACK_B_":
                                ListeCmdB.Add(drRow[1].ToString());
                                break;

                            case "PACK_D_":
                                ListeCmdD.Add(drRow[1].ToString());
                                break;

                            case "PACK_F_":
                                ListeCmdF.Add(drRow[1].ToString());
                                break;

                            case "PACK_L_":
                                ListeCmdL.Add(drRow[1].ToString());
                                break;

                            case "PARKEY_":
                                ListeCleInterdite.Add(drRow[1].ToString());
                                break;

                            case "PARSEC_":
                                ListeSectionInterdite.Add(drRow[1].ToString());
                                break;

                            case "BASREFD":
                                ConnectionStringBaseRefDedie = drRow[1].ToString();
                                break;

                            case "BASREFP":
                                ConnectionStringBaseRefPlateforme = drRow[1].ToString();
                                break;

                            case "BASESUP":
                                ConnectionStringSupport = drRow[1].ToString();
                                break;

                            case "DOSTEMP":
                                DossierTemporaire = drRow[1].ToString();
                                break;

                        }

                    }
                }


                // Pour l'instant il n'y a qu'une base de référence. Ensuite à valorisé en fonction de la typologie
                ConnectionStringBaseRef = ConnectionStringBaseRefPlateforme;

                // Pour l'instant n'existe que sur la base plateforme
                if (ConnectionStringBaseRefPlateforme != string.Empty)
                {
                     dsDataSet = dataManagerSQLServer.GetData("SELECT CFR_PLAGE_DEBUT, CFR_PLAGE_FIN  FROM  M4CFR_PLAGES_ID_SYNONYM WHERE ID_ORGANIZATION ='0000' and CFR_ID_TYPE = 'CLIENT'", ConnectionStringBaseRefPlateforme);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            ListeLimInf.Add(Int32.Parse(drRow[0].ToString()));
                            ListeLimSup.Add(Int32.Parse(drRow[1].ToString()));
                        }
                    }
  
                }
            }
            catch (Exception ex)
            {

            }

        }
    }


}
