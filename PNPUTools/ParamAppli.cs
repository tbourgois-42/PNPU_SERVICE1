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
        /// Chaine de connexion à la base de de l'application.
        /// </summary> 
        public static string ConnectionStringBaseAppli { get; }

        public static string ConnectionStringSupport { get; }

        /// <summary>
        /// Dossier temporaire utilisé pour l'application.
        /// </summary>
        public static string DossierTemporaire { get; }

        /// <summary>  
        /// Constructeur de la classe. Il charge toutes les valeurs du paramétrage.
        /// </summary>  
        static ParamAppli()
        {
            // Pour tests, à remplacer par le chargement du paramétrage
            ListeCmdInterdite = new List<string> { "TRANSFER 'SECURITY", "KILL 'SECURITY", "REPLACE M4RSC_APPROLE" };
            ListeCleInterdite = new List<string> { "ENC_CONN_STR_RAMDL", "FILESERVICE_URI", "M2M_PROXY_HOST" };
            ListeSectionInterdite = new List<string> { "AUTHENTICATION", "PORTS" };
            ListeCmdL = new List<string> { "TRANSFER", "COMPILE" };
            ListeCmdD = new List<string> { "APPEND", "REPLACE" };
            ListeCmdF = new List<string> { "CREATE TABLE", "CREATE VIEW" };
            ListeCmdB = new List<string> { "KILL" };
            ConnectionStringBaseRef = "server=M4FRSQL13;uid=SAASSN306;pwd=SAASSN306;database=SAASSN306;";
            ConnectionStringBaseAppli = "server=M4FRDB18;uid=CAPITAL_DEV;pwd=Cpldev2017;database=CAPITAL_DEV;";
            ConnectionStringSupport = "server=M4FRDB16;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";

            DossierTemporaire = "C:\\TEMPO";
            // A lire dans base de ref
            if (ConnectionStringBaseRef == string.Empty)
            {
                ListeLimInf = new List<int> { 5001, 10301, 11301 };
                ListeLimSup = new List<int> { 9999, 10999, 11999 };
            }
            else
            {
                try
                {
                    ListeLimInf = new List<int>();
                    ListeLimSup = new List<int>();

                    DataManagerSQLServer dataManagerSQLServer = new DataManagerSQLServer();
                    DataSet dsDataSet = dataManagerSQLServer.GetData("SELECT CFR_PLAGE_DEBUT, CFR_PLAGE_FIN  FROM  M4CFR_PLAGES_ID_SYNONYM WHERE ID_ORGANIZATION ='0000' and CFR_ID_TYPE = 'CLIENT'", ConnectionStringBaseRef);

                    if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
                    {
                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            ListeLimInf.Add(Int32.Parse(drRow[0].ToString()));
                            ListeLimSup.Add(Int32.Parse(drRow[1].ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

        }
    }


}
