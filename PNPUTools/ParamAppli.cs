using IniParser;
using IniParser.Model;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Pipes;


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
        public static Dictionary<string, string> ConnectionStringBaseRef { get; }

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

        public static Dictionary<string, InfoClient> ListeInfoClient { get; }
        public static string GeneratePackPath { get; internal set; }
        readonly public static DateTime DateNullPPN = new DateTime(1800, 1, 1);

        public const int TypologyDedie = 256;

        public const string ConnectionStringAccess = "Driver={Microsoft Access Driver (*.mdb)};Dbq={0};Uid=Admin;Pwd=;";


        public const string AnalyseImpactPathResult = "C:\\TEMPO\\AnalyseIpact";
        public const string RamdDlPAth = "C:\\Program Files (x86)\\meta4\\M4DevClient\\Bin\\RamDL.exe";//"C:\\meta4\\M4DevClient\\Bin\\RamDL.exe";
        public const string PackInstallationPathResult = "C:\\TEMPO\\PackInstallation";

        public const string StatutOk = "CORRECT";
        public const string StatutCompleted = "COMPLETED";
        public const string StatutError = "ERROR";
        public const string StatutWarning = "WARNING";
        public const string StatutInfo = "INFORMATION";
        public const string StatutInProgress = "IN PROGRESS";

        public const string LogLevel = "DEBUG";

        public const string connectionStringSupport = "server=M4FRDB16.fr.meta4.com;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";
        public const string connectionTemplate = "server={0};uid={1};pwd={2};database={3};";

        // public const string ConnectionStringBaseQA2 = "server=M4FRDB18.fr.meta4.com;uid=CAPITAL_DEV;pwd=Cpldev2017;database=CAPITAL_DEV;";
        // public const string ConnectionStringBaseQA1 = "server=M4FRDB18.fr.meta4.com;uid=CAPITAL_LIV;pwd=Cplliv2017;database=CAPITAL_LIV;";

        public const string ConnectionStringBaseQA2 = "server=M4FRDB20.fr.meta4.com;uid=GLS_TNR;pwd=F3?6D!Fk*f.;database=GLS_TNR;";
        public const string ConnectionStringBaseQA1 = "server=M4FRDB20.fr.meta4.com;uid=GLS_REF;pwd=o8lfIwUOBW;database=GLS_REF;";

        readonly public static int ProcessControlePacks;
        readonly public static int ProcessInit;
        readonly public static int ProcessGestionDependance;
        readonly public static int ProcessAnalyseImpact;
        readonly public static int ProcessIntegration;
        readonly public static int ProcessProcessusCritique;
        readonly public static int ProcessTNR;
        readonly public static int ProcessLivraison;
        readonly public static int ProcessAnalyseImpactData;
        readonly public static int ProcessAnalyseImpactLogique;

        public const int ProcessFinished = -1;

        public const bool SimpleCotesReport = true;

        readonly public static NamedPipeClientStream npcsPipeClient;

        readonly public static Dictionary<string, string> TranscoSatut;

        public const string templateIniFileAnalyseImpact = "<ORIGIN_CONN>\r\n{0}\r\n<TARGET_CONN>\r\n{1}\r\n<LOG_FILE>\r\n{2}\r\n<USER_CVM>\r\n{3}\r\n<PWD_CVM>\r\n{4}\r\n";//<CLEAR_PREVIOUS_ANALYSIS>\r\nYES\r\n<PACK_ANALYSIS>\r\n{5}\r\n<ANALYSE_RESULTS_FILE>\r\n{6}
        public const string templateIniFileGeneratePack = "<ORIGIN_CONN>\r\n{0}\r\n<LOG_FILE>\r\n{1}\r\n<PKGWZ_CONTENT_TYPE>\r\n3\r\n<PKGWZ_CCT_VERSION>\r\n8.1\r\n<PKGWZ_CCT_TASK_LIST>\r\n{2}\r\n<PKGWZ_MDB_ADD_TABLES>\r\n3\r\n<PKGWZ_MDB_PATH>\r\n{3}\r\n<PKGWZ_PACK_NAME>\r\nPNPU_\r\n<PKGWZ_PACK_LOAD_DATA>\r\n1\r\n<PKGWZ_PACK_REFRESH_CHANGES>\r\n0\r\n<PKGWZ_PACK_SAVE_ORDER>\r\n1\r\n<PKGWZ_PACK_SAVE_SCRIPT>\r\n0";
        public const string templateIniFileInstallPack = "<ORIGIN_CONN>\r\n{0}\r\n<TARGET_CONN>\r\n{1}\r\n<LOG_FILE>\r\n{2}\r\n<USER_CVM>\r\n{3}\r\n<PWD_CVM>\r\n{4}\r\n<PACK_LOAD>\r\n#ALL#\r\n<PACK_EXEC>\r\n#ALL#";
        public const string templateIniFileRemovePack = "<ORIGIN_CONN>\r\n{0}\r\n<TARGET_CONN>\r\n{1}\r\n<LOG_FILE>\r\n{2}\r\n<SCRIPT_EXEC>\r\n{3}\r\n";

        // Statuts des tâches planifiées
        public const string statusScheduleTaskTermine = "0";
        public const string statusScheduleTaskAttente = "1";
        public const string statusScheduleTaskEnCours = "2";
        public const string statusScheduleTaskAnnule1 = "3";
        public const string statusScheduleTaskAnnule2 = "5";
        public const string statusScheduleTaskAnnule3 = "7";
        public const string statusScheduleTaskExpire = "4";
        public const string statusScheduleTaskInterrompu = "6";
        public const string statusScheduleTaskAbandon1 = "10";
        public const string statusScheduleTaskAbandon2 = "11";

        readonly public static Queue<string> qFIFO = null;

        /// <summary>  
        /// Constructeur de la classe. Il charge toutes les valeurs du paramétrage.
        /// </summary>  
        static ParamAppli()
        {
            DataSet dsDataSet;
            DataManagerSQLServer dataManagerSQLServer = new DataManagerSQLServer();

            TranscoSatut = new Dictionary<string, string>();
            TranscoSatut.Add(StatutOk, "mdi-check-circle");
            TranscoSatut.Add(StatutCompleted, "mdi-check-circle");
            TranscoSatut.Add(StatutError, "mdi-alert-circle");
            TranscoSatut.Add(StatutWarning, "mdi-alert");
            TranscoSatut.Add(StatutInProgress, "mdi-progress-circle");
            TranscoSatut.Add(StatutInfo, "mdi-information-variant");

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
                qFIFO = new Queue<string>();

                //ConnectionStringBaseAppli = "server=M4FRDB18.fr.meta4.com;uid=CAPITAL_DEV;pwd=Cpldev2017;database=CAPITAL_DEV;";
                FileIniDataParser iniParser;
                IniData iniData;
                try
                {
                    string sCheminINI = "C:\\PNPU\\PNPUTools.ini";

                    iniParser = new FileIniDataParser();
                    iniData = iniParser.ReadFile(sCheminINI);
                    ConnectionStringBaseAppli = iniData["ConnectionStrings"]["BaseAppli"];
                }
                catch (Exception ex) {
                    //TODO LOG
                    Console.WriteLine(ex.Message);
                }

                // Si pas trouvé je me mets sur la base de prod
                if ((ConnectionStringBaseAppli == string.Empty) || (ConnectionStringBaseAppli == null))
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
                ConnectionStringBaseRef.Add("257", ConnectionStringBaseRefPlateforme);
                ConnectionStringBaseRef.Add("258", ConnectionStringBaseRefPlateforme);
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
                    //dsDataSet = dataManagerSQLServer.GetData("SELECT CLIENT_ID, CLIENT_NAME, SAAS, CODIFICATION_LIBELLE FROM A_CLIENT,A_CODIFICATION WHERE SAAS is not NULL AND SAAS = CODIFICATION_ID", ConnectionStringSupport);
                    dsDataSet = dataManagerSQLServer.GetData("SELECT CLIENT_ID, CLIENT_NAME, SAAS, C1.CODIFICATION_LIBELLE, C2.CODIFICATION_LIBELLE FROM A_CLIENT,A_CODIFICATION C1, A_CODIFICATION C2 WHERE SAAS is not NULL AND SAAS = C1.CODIFICATION_ID AND C1.CODIFICATION_TYPE = 'SAAS' AND C2.CODIFICATION_TYPE = 'SGBD' AND C2.CODIFICATION_ID = SGBD", ConnectionStringSupport);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            bool bOracle = drRow[4].ToString().ToUpper().Contains("ORACLE");
                            ListeInfoClient.Add(drRow[0].ToString(), new InfoClient(drRow[0].ToString(), drRow[1].ToString(), drRow[3].ToString(), drRow[2].ToString(), string.Empty, string.Empty, bOracle));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine(ex.Message);
            }


            /*
                Pré-Contrôle du HF
                Initialisation
                Packaging des dépendances
                Analyse d'impact
                Test d'intégration
                Tests des processus critique
                TNR standard
                Livraison
                Analyse d'impact Data
            */
            IEnumerable<PNPU_PROCESS> listAllProcess = RequestTool.GetAllProcesses();
            foreach (PNPU_PROCESS process in listAllProcess)
            {
                switch (process.PROCESS_LABEL)
                {
                    case "Pré - Contrôle du HF":
                        ProcessControlePacks = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Initialisation":
                        ProcessInit = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Packaging des dépendances":
                        ProcessGestionDependance = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Analyse d'impact":
                        ProcessAnalyseImpact = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Test d'intégration":
                        ProcessIntegration = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Tests des processus critique":
                        ProcessProcessusCritique = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "TNR standard":
                        ProcessTNR = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Livraison":
                        ProcessLivraison = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Analyse d'impact Data":
                        ProcessAnalyseImpactData = Decimal.ToInt32(process.ID_PROCESS);
                        break;

                    case "Analyse d'impact Logique":
                        ProcessAnalyseImpactLogique = Decimal.ToInt32(process.ID_PROCESS);
                        break;
                }
            }

        }
    }


}
