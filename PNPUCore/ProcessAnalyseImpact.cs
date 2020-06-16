using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;

namespace PNPUCore.Process
{
    internal class ProcessAnalyseImpact : ProcessCore, IProcess
    {
        
        public List<ElementLocaliser> listElementALocaliser; 
        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessAnalyseImpact(int wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = ParamAppli.ProcessAnalyseImpact;
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessAnalyseImpact(WORKFLOW_ID, CLIENT_ID);
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {

            /*RamdlTool ramdlTool = new RamdlTool(CLIENT_ID, Decimal.ToInt32(WORKFLOW_ID));
            ramdlTool.AnalyseMdbRAMDL();*/

            List<IControle> listControl = ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportAnalyseImpact = new RapportProcessAnalyseImpact();
            RapportProcess.Name = this.LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportAnalyseLogique rapportAnalyseLogique = new RapportAnalyseLogique();
            RapportAnalyseData rapportAnalyseData = new RapportAnalyseData();
            RapportElementLocaliser rapportElementLocaliser = new RapportElementLocaliser();

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress);

            //Lancement analyse d'impact RamDl
            RamdlTool ramdlTool = new RamdlTool(CLIENT_ID, Decimal.ToInt32(WORKFLOW_ID));
            List<String> pathList = ramdlTool.AnalyseMdbRAMDL();
            List<AnalyseResultFile> resultFileList = new List<AnalyseResultFile>();
            foreach (string pathFile in pathList)
            {
                String analyseFileName = Path.GetFileNameWithoutExtension(pathFile);
                //PARTIE LOGIQUE
                using (StreamReader reader = new StreamReader(pathFile))
                {
                    string line;
                    AnalyseResultFile resultFile = new AnalyseResultFile(pathFile, analyseFileName);
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] listElement = line.Split('\t');
                        AnalyseResultLine resultLine = new AnalyseResultLine(listElement);
                        resultFile.addLine(resultLine);
                    }
                    resultFileList.Add(resultFile);
                }

                //PARTIE DATA
                //On recrée le chemin du fichier mdb.
                string fileMdb =  analyseFileName.Remove(0, 8); //Remove Analyse_
                string sNom = WORKFLOW_ID.ToString("0000000000");
                string sPathFichierMdb = ParamAppli.DossierTemporaire + "\\" + sNom + "\\" + fileMdb + ".mdb";
                //Récupération de toutes les commandes data
                List<RmdCommandData> listCommandData = this.getAllDataCmd(sPathFichierMdb);

                foreach(RmdCommandData commandData in listCommandData)
                {


                }


            
            }

            this.addRapportAnalyseLogique(rapportAnalyseLogique, resultFileList);
            RapportAnalyseImpact.rapportAnalyseLogique = rapportAnalyseLogique;
            RapportAnalyseImpact.rapportAnalyseData = rapportAnalyseData;
            RapportAnalyseImpact.rapportElementLocaliser = rapportElementLocaliser;

            //Gestion contrôle data
            //GetListControle(ref listControl);

            //gestion des requêtes data génériques (controle)



            /*foreach (IControle controle in listControl)
            {
                controle.SetProcessControle(this);
                RControle RapportControle = new RControle();
                RapportControle.Name = controle.GetLibControle();
                RapportControle.Tooltip = controle.GetTooltipControle();
                RapportControle.Message = new List<string>();
                RapportControleCourant = RapportControle;
                string statutControle = controle.MakeControl();
                //ERROR > WARNING > OK
                if (GlobalResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutError)
                {
                    GlobalResult = statutControle;

                }
                else if (GlobalResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutWarning)
                {
                    GlobalResult = statutControle;
                }
                RapportControle.Result = ParamAppli.TranscoSatut[statutControle];


                RapportSource.Controle.Add(RapportControle);
            }

                if (SourceResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutError)
                {
                    SourceResult = statutControle;

                }
                else if (SourceResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutWarning)
                {
                    SourceResult = statutControle;
                }
            }*/


            //RapportProcess.rapportAnalyseData = rapportAnalyseData;




            //Elements à localiser


            RapportAnalyseImpact.Fin = DateTime.Now;
            RapportAnalyseImpact.Result = ParamAppli.TranscoSatut[GlobalResult];
            
            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult);


            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessAnalyseImpact);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID);
            }

        }


        //=> MHUM le 22/11/2019 - Gestion création des dossiers nécessaires à l'analyse RAMDL
        //--------------------------------------------------------------------
        // Récupération du paramétrage dans la chaine
        //
        private string LitValeurParam(string pChaineEntiere, string pParam)
        {
            string sResultat = string.Empty;
            int iIndexDeb = 0;
            int iIndexFin = 0;

            try
            {
                iIndexDeb = pChaineEntiere.IndexOf(pParam);
                if (iIndexDeb > -1)
                {
                    iIndexDeb = pChaineEntiere.IndexOf("'", iIndexDeb);
                    iIndexFin = pChaineEntiere.IndexOf("'", iIndexDeb + 1);
                    sResultat = pChaineEntiere.Substring(iIndexDeb + 1, iIndexFin - iIndexDeb - 1);
                }
            }
            catch (Exception ex)
            {
                //lbErreurs.Items.Add("LitValeurParam - Erreur d'exécution (exception) : " + ex.Message);
                Logger.Log(this, "ERROR", "LitValeurParam - Erreur d'exécution (exception) : " + ex.Message);
            }
            return (sResultat);
        }

        // MHUM le 24/09/2019 - Test MAJ MDB pour supprimer les CRLF des commentaires
        private void MiseAJourMDB(string sCheminMDB)
        {
            OdbcConnection dbConn = new OdbcConnection();
            dbConn.ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=" + sCheminMDB + ";Uid=Admin;Pwd=;";
            dbConn.Open();

            OdbcCommand objCmd = new OdbcCommand();

            objCmd.Connection = dbConn;
            objCmd.CommandType = CommandType.Text;
            objCmd.CommandText = "UPDATE M4RDL_PACK_CMDS SET CMD_COMMENTS = ' ' WHERE CMD_COMMENTS LIKE '%'+CHR(13)+CHR(10)+'%'";

            objCmd.ExecuteNonQuery();
            dbConn.Close();
        }

        private string MiseEnformeChaineConnexion(string sChaineConnexion, string sNomSourceODBC)
        {
            string sResultat = string.Empty;
            int iIndex = 0;

            if (sChaineConnexion.ToUpper().Substring(0, 6) == "SERVER")
            {
                iIndex = sChaineConnexion.IndexOf(";");
                if (iIndex > -1)
                    sResultat = "DSN=" + sNomSourceODBC + sChaineConnexion.Substring(iIndex);
            }
            return (sResultat);
        }


        private List<RmdCommandData> getAllDataCmd(String sConnection)
        {
            DataManagerAccess dataManager = new DataManagerAccess();
            string requete = "select ID_PACKAGE, ID_CLASS, ID_OBJECT, CMD_CODE from M4RDL_PACK_CMDS where ID_PACKAGE like '%_D'";
            List<RmdCommandData> listDatacmd = new List<RmdCommandData>();
            DataSet result = dataManager.GetData(requete, sConnection);
            DataTable tableCmd = result.Tables[0];

            foreach (DataRow row in tableCmd.Rows)
            {
                RmdCommandData commandData = new RmdCommandData(row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), this);
                listDatacmd.Add(commandData);


            }

            return listDatacmd;
        }
    }


    class GereAccess
    {
        //--------------------------------------------------------------------
        // Retourne la chaîne de connection pour un fichier Access.
        //
        public static string GetConnectionStringMDB(string sFichier)
        {
            return "Driver={Microsoft Access Driver (*.mdb)};Dbq="
                + sFichier
                + ";Uid=Admin;Pwd=;";
        }


        private List<RmdCommandData> getAllDataCmd(String sConnection)
        {
            DataManagerAccess dataManager = new DataManagerAccess();
            string requete = "select ID_PACKAGE, ID_CLASS, ID_OBJECT, CMD_CODE from M4RDL_PACK_CMDS where ID_PACKAGE like '%_D'";
            List<RmdCommandData> listDatacmd = new List<RmdCommandData>();
            DataSet result = dataManager.GetData(requete, sConnection);
            DataTable tableCmd = result.Tables[0];

            foreach (DataRow row in tableCmd.Rows)
            {
                RmdCommandData commandData = new RmdCommandData(row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), this);
                listDatacmd.Add(commandData);


            }

            return listDatacmd;
        }
    }
}
