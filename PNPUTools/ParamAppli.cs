using System;
using System.Collections.Generic;
using System.Text;
using PNPUTools.DataManager;
using System.Data;
using System.IO.Pipes;
using System.Data.SqlClient;

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
        public static Dictionary<string,string> ConnectionStringBaseRef { get; }

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

        public static Dictionary<string, InfoClient> ListeInfoClient {get;}

        public const string StatutOk = "CORRECT";
        public const string StatutCompleted = "COMPLETED";
        public const string StatutError = "ERROR";
        public const string StatutWarning = "WARNING";
        public static string LogLevel = "DEBUG";

        public const string connectionStringSupport = "server=M4FRDB16;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";


        public const int ProcessControlePacks = 1;
        public const int ProcessInit = 2;
        public const int ProcessGestionDependance = 3;
        public const int ProcessAnalyseImpact = 4;
        public const int ProcessIntegration = 5;
        public const int ProcessProcessusCritique = 6;
        public const int ProcessTNR = 7;
        public const int ProcessLivraison = 8;
 
        public const int ProcessFinished = -1;

        public const bool SimpleCotesReport = true;

        public static NamedPipeClientStream npcsPipeClient;

        public static Dictionary<string, string> TranscoSatut;

        /// <summary>  
        /// Constructeur de la classe. Il charge toutes les valeurs du paramétrage.
        /// </summary>  
        static ParamAppli()
        {
            DataSet dsDataSet = null;
            DataManagerSQLServer dataManagerSQLServer = new DataManagerSQLServer();

            TranscoSatut = new Dictionary<string, string>();
            TranscoSatut.Add(StatutOk, "mdi-check-circle");
            TranscoSatut.Add(StatutCompleted, "mdi-check-circle");
            TranscoSatut.Add(StatutError, "mdi-alert-circle");
            TranscoSatut.Add(StatutWarning, "mdi-alert");

            npcsPipeClient = null;

            ListeInfoClient = new Dictionary<string, InfoClient>();


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
                //ConnectionStringBaseAppli = "server=M4FRDB18;uid=CAPITAL_DEV;pwd=Cpldev2017;database=CAPITAL_DEV;";

                ConnectionStringBaseAppli = "server=M4FRDB22;uid=PNPU_DEV;pwd=PNPU_DEV;database=PNPU_DEV;";
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


                // On valorise en fonction de la typologie
                ConnectionStringBaseRef = new Dictionary<string, string>();
                ConnectionStringBaseRef.Add("Dédié", ConnectionStringBaseRefDedie);
                ConnectionStringBaseRef.Add("Mutualisé", ConnectionStringBaseRefPlateforme);
                ConnectionStringBaseRef.Add("Désynchronisé", ConnectionStringBaseRefPlateforme);


                // N'existe que sur la base plateforme
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

                //Chargement des infos clients
                if (ConnectionStringSupport != string.Empty)
                {
                    dsDataSet = dataManagerSQLServer.GetData("SELECT CLIENT_ID, CLIENT_NAME, SAAS, CODIFICATION_LIBELLE FROM A_CLIENT,A_CODIFICATION WHERE SAAS is not NULL AND SAAS = CODIFICATION_ID", ConnectionStringSupport);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            ListeInfoClient.Add(drRow[0].ToString(), new InfoClient(drRow[0].ToString(), drRow[1].ToString(), drRow[2].ToString(), drRow[3].ToString(), string.Empty, string.Empty)) ;
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
