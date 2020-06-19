using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNPUTools
{
    public class RamdlTool
    {
        InfoClient InfoClient;
        int WORKFLOW_ID;
        int ID_H_WORKFLOW;
        private string pathLogFile;
        private string pathResult;
        private string pathIni;
        private string DirectoryResult;

        /*
         * Cela doit normalement être dans infoClient. Je laisse pour les tests
         * string sChaineDeConnexion = "";
        string sLogin = "";
        string sMdp = "";

        string cbBaseSQLQA1 = "XXXM4QA1";
        string cbBaseSQLQA2 = "XXXM4QA2";*/

        private string iniFile;

        //TMP
        string sChaineDeConnexion = "";
        string sLogin = "";
        string sMdp = "";

        public RamdlTool(string id_client, int WORKFLOW_ID_, int ID_H_WORKFLOW_)
        {

            InfoClient = ParamAppli.ListeInfoClient[id_client];
            WORKFLOW_ID = WORKFLOW_ID_;
            ID_H_WORKFLOW = ID_H_WORKFLOW_;
            //string sEnvironnement = InfoClient.;
            sChaineDeConnexion = "DSN=CAPITAL_DEV;SRVR=M4FRDB18;UID=CAPITAL_DEV;PWD=Cpldev2017;";
            sLogin = "M4ADM";
            sMdp = "CapitalM4ADM";
            ParamRamDlInit();
        }

        public RamdlTool(InfoClient infoClient_, int WORKFLOW_ID_, int ID_H_WORKFLOW_)
        {
            InfoClient = infoClient_;
            WORKFLOW_ID = WORKFLOW_ID_;
            ID_H_WORKFLOW = ID_H_WORKFLOW_;
            //TODO in waiting having r eal data;
            sChaineDeConnexion = "DSN=CAPITAL_DEV;SRVR=M4FRDB18;UID=CAPITAL_DEV;PWD=Cpldev2017;";
            sLogin = "M4ADM";
            sMdp = "CapitalM4ADM";
            ParamRamDlInit();
        }

        public List<String> AnalyseMdbRAMDL()
        {
            string sCheminMDB;
            ProcessStartInfo psiStartInfo;
            System.Diagnostics.Process pProcess;
            StreamReader srResultat;
            string sContenuFichierLog;
            List<string> listPathResult = new List<string>();

            this.pathLogFile = ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW  + "\\" + InfoClient.ID_CLIENT + "\\RAMDL_";
            this.pathIni = ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL\\CmdRAMDL.ini";
            this.DirectoryResult = ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT;
            // Suppression des fichiers d'analyse et de log précédents
            //this.deleteTempFile();
            try
            {
                
                if (Directory.Exists(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL") == false) 
                    Directory.CreateDirectory(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL");

                if (Directory.Exists(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT) == false)
                    Directory.CreateDirectory(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT);

                //TBO Download from DB MDB
                string[] tMdb = null;
                List<string> listMDB = new List<string>();

                PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();
                gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tMdb, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli, ID_H_WORKFLOW);
                
                foreach (String sFichier in tMdb)
                {
                    listMDB.Add(sFichier);
                }

                for (int i = 0; i < listMDB.Count; i++)
                {
                    sCheminMDB = listMDB[i];
                    //Logger.Log(this, "INFO", "Analyse RAMDL du fichier " + sCheminMDB);

                    MiseAJourMDB(sCheminMDB);

                    this.GenerateIniForAnalyseImpact(sCheminMDB);

                    // Encodage du mot de passe
                    psiStartInfo = new ProcessStartInfo();
                    psiStartInfo.FileName = ParamAppli.RamdDlPAth;
                    psiStartInfo.Arguments = "ENC " + this.pathIni;
                    pProcess = System.Diagnostics.Process.Start(psiStartInfo);
                    pProcess.WaitForExit();
                    pProcess.Close();

                    // Lancement de l'analyse
                    psiStartInfo = new ProcessStartInfo();
                    psiStartInfo.FileName = ParamAppli.RamdDlPAth;
                    psiStartInfo.Arguments = this.pathIni;
                    pProcess = System.Diagnostics.Process.Start(psiStartInfo);

                    pProcess.WaitForExit();
                    pProcess.Close();

                    if (File.Exists(Path.GetDirectoryName(sCheminMDB) + "\\" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".ldb") == true)
                        File.Delete(Path.GetDirectoryName(sCheminMDB) + "\\" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".ldb");

                    // Contrôle du résultat
                    srResultat = new StreamReader(pathLogFile + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                    sContenuFichierLog = srResultat.ReadToEnd();
                    srResultat.Close();

                    int iIndexErreur = sContenuFichierLog.IndexOf("[Error");
                    if ((iIndexErreur > -1) && (sContenuFichierLog.IndexOf("[Error 429] ActiveX component can't create object") != iIndexErreur))
                    {
                        //Logger.Log(this, "ERROR", "Erreur lors de l'analyse du mdb " + Path.GetFileNameWithoutExtension(sCheminMDB) + ". Voir le fichier de log " + sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                    }
                    else
                    {
                        listPathResult.Add(this.pathResult);
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Log(this, "ERROR", "AnalyseUnMdbRAMDL - Erreur d'exécution (exception) : " + ex.Message);
                return new List<String>();
            }
            return listPathResult;
        }

        private void GenerateIniForAnalyseImpact(string sCheminMDB)
        {
            string sConnection = this.GetConnectionStringMDB(sCheminMDB);
            OdbcDataReader reader;
            //< LOG_FILE >
            string logFile = pathLogFile + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log";
            //<ORIGIN_CONN>
            string originConn = "DRIVER={Microsoft Access Driver (*.mdb)}; DBQ=" + sCheminMDB;
            //<TARGET_CONN>
            string targetConn = sChaineDeConnexion;
            //<USER_CVM>
            string userCVM = sLogin;
            //<PWD_CVM>
            string pwdCVM = sMdp;//swFichierIni.WriteLine("Logan%Celya09");
            this.pathResult = DirectoryResult + "\\Analyse_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".TXT";

            iniFile = String.Format(ParamAppli.templateIniFileAnalyseImpact, originConn, targetConn, logFile, userCVM, pwdCVM);
            using (OdbcConnection connection = new OdbcConnection(sConnection))
            {
                OdbcCommand command = connection.CreateCommand();

                connection.Open();
                command.CommandText = "SELECT ID_PACKAGE FROM M4RDL_PACKAGES WHERE ID_PACKAGE LIKE '%_L'";
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    //TBO CREATION DU FICHIER INI
                    string TmpFichierIni = this.pathIni;
                    StreamWriter swFichierIni = new StreamWriter(TmpFichierIni, false);
                    swFichierIni.WriteLine(iniFile);
                    swFichierIni.WriteLine("<CLEAR_PREVIOUS_ANALYSIS>");
                    swFichierIni.WriteLine("YES");
                    swFichierIni.WriteLine("<PACK_ANALYSIS>");

                    while (reader.Read())
                    {
                        swFichierIni.WriteLine(reader[0].ToString());
                    }
                    reader.Close();
                    connection.Close();
                    swFichierIni.WriteLine("<ANALYSE_RESULTS_FILE>");
                    swFichierIni.WriteLine(pathResult);

                    swFichierIni.Close();
                }
            }
        }

        public void GeneratePackFromCCT(string namePack, string[] listPack)
        {
            ProcessStartInfo psiStartInfo;
            System.Diagnostics.Process pProcess;
            StreamReader srResultat;
            string sContenuFichierLog;

            // MHUM pour test
            this.sChaineDeConnexion = "DSN=SAASSN305;uid=SAASSN305;pwd=SAASSN305;database=SAASSN305;";//ParamAppli.ConnectionStringBaseRefPlateforme;
            ParamAppli.GeneratePackPath = ParamAppli.DossierTemporaire+"\\RAMDL";
            this.pathIni = ParamAppli.GeneratePackPath +"\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT + "\\RamdlCmd.INI";
            // FIN MHUM pour test

            this.pathLogFile = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT + "\\RAMDL_DEPENDANCE_" + namePack + ".log"; //{1}
            this.pathResult = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT + "\\" + namePack + ".mdb";
            this.DirectoryResult = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT;
            try
            {
                if (Directory.Exists(ParamAppli.GeneratePackPath + "\\TempoRAMDL") == false) Directory.CreateDirectory(ParamAppli.GeneratePackPath + "\\TempoRAMDL");
                if (Directory.Exists(DirectoryResult) == false) Directory.CreateDirectory(DirectoryResult);

                if (File.Exists(this.pathResult))
                    File.Delete(this.pathResult);

                string sDossierFichiersRAMDL = ParamAppli.GeneratePackPath;

                this.GenerateIniForGeneratePack(namePack, listPack);
                // Encodage du mot de passe
                /*psiStartInfo = new ProcessStartInfo();
                psiStartInfo.FileName = ParamAppli.RamdDlPAth;
                psiStartInfo.Arguments = "ENC " + this.pathIni;
                pProcess = System.Diagnostics.Process.Start(psiStartInfo);
                pProcess.WaitForExit();
                pProcess.Close();*/

                // TODO Suppression des fichiers d'analyse et de log précédents
                //this.deleteTempFile();

                // Lancement de l'analyse
                psiStartInfo = new ProcessStartInfo();
                psiStartInfo.FileName = ParamAppli.RamdDlPAth;
                psiStartInfo.Arguments = this.pathIni;
                pProcess = System.Diagnostics.Process.Start(psiStartInfo);

                pProcess.WaitForExit();
                pProcess.Close();

                // Contrôle du résultat
                srResultat = new StreamReader(this.pathLogFile);
                sContenuFichierLog = srResultat.ReadToEnd();
                srResultat.Close();

                int iIndexErreur = sContenuFichierLog.IndexOf("[Error");
                if ((iIndexErreur > -1) && (sContenuFichierLog.IndexOf("[Error 429] ActiveX component can't create object") != iIndexErreur))
                {
                    //Logger.Log(this, "ERROR", "Erreur lors de l'analyse du mdb " + Path.GetFileNameWithoutExtension(sCheminMDB) + ". Voir le fichier de log " + sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                }
            }
            catch (Exception ex)
            {
                //Logger.Log(this, "ERROR", "AnalyseUnMdbRAMDL - Erreur d'exécution (exception) : " + ex.Message);
                return;
            }
        }

        private void GenerateIniForGeneratePack(string namePack, string[] listTask)
        {
            //<ORIGIN_CONN>
            string originConn = sChaineDeConnexion; //{0}
            string slistTask = ""; //{ 2}
            foreach (String sTask in listTask)
            {
                slistTask = slistTask + sTask + Environment.NewLine;
            }            
            //TBO CREATION DU FICHIER INI
            iniFile = String.Format(ParamAppli.templateIniFileGeneratePack, originConn, this.pathLogFile, slistTask, this.pathResult);

            StreamWriter swFichierIni = new StreamWriter(this.pathIni, false);
            swFichierIni.WriteLine(iniFile);
            swFichierIni.Close();

        }
        private void deleteTempFile()
        {
            string[] sListeFichier = Directory.GetFiles(DirectoryResult, "*.log");
            foreach (string sFichier in sListeFichier)
                File.Delete(sFichier);
            sListeFichier = Directory.GetFiles(DirectoryResult, "*.TXT");
            foreach (string sFichier in sListeFichier)
                File.Delete(sFichier);
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
                //Logger.Log(this, "ERROR", "LitValeurParam - Erreur d'exécution (exception) : " + ex.Message);
            }
            return (sResultat);
        }

        // MHUM le 24/09/2019 - Test MAJ MDB pour supprimer les CRLF des commentaires
        private void MiseAJourMDB(string sCheminMDB)
        {
            try
            {
                OdbcConnection dbConn = new OdbcConnection();
                dbConn.ConnectionString = "Driver={Microsoft Access Driver (*.mdb)};Dbq=" + sCheminMDB + ";Uid=Admin;Pwd=;";
                dbConn.Open();

                OdbcCommand objCmd = new OdbcCommand();

                objCmd.Connection = dbConn;
                objCmd.CommandType = CommandType.Text;
                objCmd.CommandText = "UPDATE M4RDL_PACK_CMDS SET CMD_COMMENTS = ' ' WHERE CMD_COMMENTS LIKE '%'+CHR(13)+CHR(10)+'%'";

                objCmd.ExecuteNonQuery();
                dbConn.Close();
            }catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

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

        public string GetConnectionStringMDB(string sFichier)
        {
            return "Driver={Microsoft Access Driver (*.mdb)};Dbq="
                + sFichier
                + ";Uid=Admin;Pwd=;";
        }


        private void ParamRamDlInit()
        {
            //=> MHUM le 22/11/2019 - Lecture du paramétrage de RAMDL et création des dossiers si nécessaire 
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\META4\\regmeta4.xml") == true)
            {
                StreamReader srRegMeta4 = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\META4\\regmeta4.xml");
                bool bContinue = !srRegMeta4.EndOfStream;
                string sBuffer = string.Empty;
                string sBuffer2 = string.Empty;


                while (bContinue == true)
                {
                    sBuffer = srRegMeta4.ReadLine();
                    if (sBuffer.IndexOf("<RAMDL ") > -1)
                    {
                        //if (Directory.Exists("V:\\M4Temp\\PM\\Cache") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Cache");
                        sBuffer2 = LitValeurParam(sBuffer, "LastCacheDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                        //if (Directory.Exists("V:\\M4Temp\\PM\\Log") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Log");
                        sBuffer2 = LitValeurParam(sBuffer, "LastLogDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                        //if (Directory.Exists("V:\\M4Temp\\PM\\CVS") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\CVS");
                        sBuffer2 = LitValeurParam(sBuffer, "LastCVSDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                        //if (Directory.Exists("V:\\M4Temp\\PM\\Client") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Client");
                        sBuffer2 = LitValeurParam(sBuffer, "LastClientDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                        //if (Directory.Exists("V:\\M4Temp\\PM\\Package") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Package");
                        sBuffer2 = LitValeurParam(sBuffer, "LastPackageDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                        //if (Directory.Exists("V:\\M4Temp\\PM\\Standard") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Standard");
                        sBuffer2 = LitValeurParam(sBuffer, "LastStandardDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);


                        //if (Directory.Exists("V:\\M4Temp\\PM\\Backup\\client") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Backup\\client");
                        sBuffer2 = LitValeurParam(sBuffer, "LastBackupDirectory=");
                        if (sBuffer2 != string.Empty)
                            if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);


                        bContinue = false;
                    }
                    else
                        bContinue = !srRegMeta4.EndOfStream;
                }

            }
            //<= MHUM le 22/11/2019
        }


    }

    
}
