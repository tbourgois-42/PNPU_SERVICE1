using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PNPUTools
{
    public class RamdlTool
    {
        private readonly InfoClient InfoClient;
        private readonly int WORKFLOW_ID;
        private readonly int ID_H_WORKFLOW;
        private string pathLogFile;
        private string pathResult;
        private string pathIni;
        private string DirectoryResult;
        private readonly string sConnectionStringBaseQA1;
        private readonly string sConnectionStringBaseQA2;

        private string iniFile;

        //TMP
        private readonly string sLogin = "";
        private readonly string sMdp = "";

        public RamdlTool(string id_client, int WORKFLOW_ID_, int ID_H_WORKFLOW_)
        {

            InfoClient = ParamAppli.ListeInfoClient[id_client];
            WORKFLOW_ID = WORKFLOW_ID_;
            ID_H_WORKFLOW = ID_H_WORKFLOW_;
            ParamToolbox paramToolbox = new ParamToolbox();
            sConnectionStringBaseQA1 = paramToolbox.GetConnexionString("Before", WORKFLOW_ID_, id_client, ID_H_WORKFLOW_);
            sConnectionStringBaseQA2 = paramToolbox.GetConnexionString("After", WORKFLOW_ID_, id_client, ID_H_WORKFLOW_);

            sLogin = "M4ADM";
            sMdp = "M4ADM";
            ParamRamDlInit();
        }

        public RamdlTool(InfoClient infoClient_, int WORKFLOW_ID_, int ID_H_WORKFLOW_)
        {
            InfoClient = infoClient_;
            WORKFLOW_ID = WORKFLOW_ID_;
            ID_H_WORKFLOW = ID_H_WORKFLOW_;

            ParamToolbox paramToolbox = new ParamToolbox();
            sConnectionStringBaseQA1 = paramToolbox.GetConnexionString("Before", WORKFLOW_ID_, infoClient_.ID_CLIENT, ID_H_WORKFLOW_);
            sConnectionStringBaseQA2 = paramToolbox.GetConnexionString("After", WORKFLOW_ID_, infoClient_.ID_CLIENT, ID_H_WORKFLOW_);

            sLogin = "M4ADM";
            sMdp = "M4ADM";
            ParamRamDlInit();
        }

        /// <summary>
        /// Cette méthode lance l'installation des mdb stockés en base en fonction du workflow, du client et du niveau de dépendance pour les packs de dépendance
        /// </summary>
        /// <param name="Niv">Niveau du pack de dépendance à installé (1,2 ou 3) ou le pack de base du HF si 0.</param>
        /// <param name="dResultat">Au retour contient le résultat fe l'installation sous forme de dictionnaire, nom du mdb en clé et la liste des erreurs en valeur</param>
        /// <param name="bRemovePack">Si true, effectue un remove pack pour chaque pack du mdb avant de lancer l'installation</param>
        public void InstallMdbRAMDL(int Niv, ref Dictionary<string, List<string>> dResultat, bool bRemovePack = false)
        {
            string sCheminMDB;
            ProcessStartInfo psiStartInfo;
            System.Diagnostics.Process pProcess;
            StreamWriter swFichierIni;
            string sCheminCommandFile;

            List<string> lListError = null;

            pathLogFile = ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\RAMDL_";
            pathIni = ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL\\CmdRAMDL.ini";
            DirectoryResult = ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT;

            sCheminCommandFile = ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL\\CmdRamDL.sql";

            // Suppression des fichiers d'analyse et de log précédents
            //this.deleteTempFile();
            try
            {

                if (!Directory.Exists(ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL"))
                {
                    Directory.CreateDirectory(ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL");
                }

                if (!Directory.Exists(ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT))
                {
                    Directory.CreateDirectory(ParamAppli.PackInstallationPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT);
                }

                string[] tMdb = null;
                List<string> listMDB = new List<string>();

                PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();
                if (Niv > 0)
                {
                    gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tMdb, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli, ID_H_WORKFLOW, InfoClient.ID_CLIENT, Niv);
                }
                else
                {
                    gereMDBDansBDD.ExtraitFichiersMDBBDD(ref tMdb, WORKFLOW_ID, ParamAppli.DossierTemporaire, ParamAppli.ConnectionStringBaseAppli, ID_H_WORKFLOW);
                }

                if ((tMdb != null) && (tMdb.Length > 0))
                {
                    foreach (String sFichier in tMdb)
                    {
                        listMDB.Add(sFichier);
                    }

                    for (int i = 0; i < listMDB.Count; i++)
                    {
                        sCheminMDB = listMDB[i];

                        //< LOG_FILE >
                        string logFile = pathLogFile + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log";
                        //<ORIGIN_CONN>
                        string originConn = "DRIVER={Microsoft Access Driver (*.mdb)}; DBQ=" + sCheminMDB;
                        //<TARGET_CONN>
                        string targetConn = MiseEnformeChaineConnexion(sConnectionStringBaseQA2);
                        //<USER_CVM>
                        string userCVM = sLogin;
                        //<PWD_CVM>
                        string pwdCVM = sMdp;

                        // Gestion des remove pack
                        if (bRemovePack)
                        {
                            iniFile = String.Format(ParamAppli.templateIniFileRemovePack, targetConn, targetConn, logFile, sCheminCommandFile);
                            swFichierIni = new StreamWriter(pathIni, false);
                            swFichierIni.WriteLine(iniFile);
                            swFichierIni.Close();

                            // Ecriture du fichier de commandes
                            swFichierIni = new StreamWriter(sCheminCommandFile, false);
                            DataManagerAccess dataManagerAccess = new DataManagerAccess();
                            DataSet dataSet = dataManagerAccess.GetData("SELECT ID_PACKAGE FROM M4RDL_PACKAGES", sCheminMDB);
                            if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
                            {
                                foreach (DataRow drRow in dataSet.Tables[0].Rows)
                                {
                                    swFichierIni.WriteLine("Remove Pack \"" + drRow[0].ToString() + "\" From Destination\\");
                                }
                            }

                            swFichierIni.Close();

                            // Encodage du mot de passe
                            psiStartInfo = new ProcessStartInfo
                            {
                                FileName = ParamAppli.RamdDlPAth,
                                Arguments = "ENC " + pathIni
                            };
                            pProcess = System.Diagnostics.Process.Start(psiStartInfo);
                            pProcess.WaitForExit();
                            pProcess.Close();

                            // Lancement des remove pack
                            psiStartInfo = new ProcessStartInfo
                            {
                                FileName = ParamAppli.RamdDlPAth,
                                Arguments = pathIni
                            };
                            pProcess = System.Diagnostics.Process.Start(psiStartInfo);

                            pProcess.WaitForExit();
                            pProcess.Close();
                        }

                        iniFile = String.Format(ParamAppli.templateIniFileInstallPack, originConn, targetConn, logFile, userCVM, pwdCVM);
                        swFichierIni = new StreamWriter(pathIni, false);
                        swFichierIni.WriteLine(iniFile);
                        swFichierIni.Close();

                        // Encodage du mot de passe
                        psiStartInfo = new ProcessStartInfo
                        {
                            FileName = ParamAppli.RamdDlPAth,
                            Arguments = "ENC " + pathIni
                        };
                        pProcess = System.Diagnostics.Process.Start(psiStartInfo);
                        pProcess.WaitForExit();
                        pProcess.Close();

                        // Lancement de l'installation
                        psiStartInfo = new ProcessStartInfo
                        {
                            FileName = ParamAppli.RamdDlPAth,
                            Arguments = pathIni
                        };
                        pProcess = System.Diagnostics.Process.Start(psiStartInfo);

                        pProcess.WaitForExit();
                        pProcess.Close();

                        lListError = new List<string>();
                        ExecutionInstallationCorrecte(logFile, ref lListError);
                        dResultat.Add(Path.GetFileName(sCheminMDB), lListError);
                    }
                }
            }
            catch (Exception ex)
            {
                //LoggerHelper.Log(this, "ERROR", "AnalyseUnMdbRAMDL - Erreur d'exécution (exception) : " + ex.Message);
                //TODO LOG
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Méthode vérifiant le résultat d'une installation.
        /// </summary>
        /// <param name="sCheminFichierLog">Chemin du fichier de log Ramdl à vérifier</param>
        /// <param name="lErrorMessages">au retour liste contenant les messages d'erreur trouvé dans le fichier</param>
        /// <returns></returns>
        private bool ExecutionInstallationCorrecte(string sCheminFichierLog, ref List<string> lErrorMessages)
        {
            StreamReader srFichierLog = new StreamReader(sCheminFichierLog);
            string sContenufichierLog;
            bool bResultat = true;
            List<string> lListePacksErreur = new List<string>();


            try
            {
                while (!srFichierLog.EndOfStream)
                {
                    sContenufichierLog = srFichierLog.ReadLine();

                    if (Regex.IsMatch(sContenufichierLog, @"^\[[^\]]*\]\*(.*)$"))
                    {
                        foreach (string sExpression in Regex.Split(sContenufichierLog, @"^\[[^\]]*\]\*(.*)$"))
                        {
                            if (sExpression != string.Empty && !Regex.IsMatch(sExpression, @"^\[Error 0\] Disconnect from") && Regex.IsMatch(sExpression, @"^\[Error -1\] Failed to execute package '([^']+)'"))
                            {
                                foreach (string sExpression2 in Regex.Split(sExpression, @"^\[Error -1\] Failed to execute package '([^']+)'"))
                                {
                                    if (sExpression2 != string.Empty)
                                    {
                                        lErrorMessages.Add("Erreur d'installation du pack " + sExpression2 + ".");
                                        bResultat = false;
                                    }
                                }
                            }

                            if (Regex.IsMatch(sExpression, @"^\[Error -1\] Package '([^']+)' executed with errors"))
                            {
                                foreach (string sExpression2 in Regex.Split(sExpression, @"^\[Error -1\] Package '([^']+)' executed with errors"))
                                {
                                    // Vérifie si la deuxième exécution n'a pas abouti
                                    if (sExpression2 != string.Empty && (sContenufichierLog.IndexOf("'" + sExpression2 + "' executed successfully") == -1) && (lListePacksErreur.IndexOf(sExpression2) == -1))
                                    {
                                        lErrorMessages.Add("Erreur d'installation du pack " + sExpression2 + ".");
                                        lListePacksErreur.Add(sExpression2);
                                        bResultat = false;
                                    }
                                }
                            }
                            if ((sExpression.IndexOf("[Error -1] The RAM-DL repository") > -1) && (sExpression.IndexOf(" is not compatible with the connected repository") > -1))
                            {
                                lErrorMessages.Add("MDB non compatible.");
                                bResultat = false;
                            }
                            if (sExpression.Contains("[Error 100] No packages found in origin source or already loaded at destination"))
                            {
                                lErrorMessages.Add("Aucun nouveau pack trouvé dans le mdb.");
                            }


                        }
                    }
                }

                srFichierLog.Close();
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine(ex.Message);
                bResultat = false;
            }

            return bResultat;
        }


        public List<String> AnalyseMdbRAMDL()
        {
            string sCheminMDB;
            ProcessStartInfo psiStartInfo;
            System.Diagnostics.Process pProcess;
            StreamReader srResultat;
            string sContenuFichierLog;
            List<string> listPathResult = new List<string>();

            pathLogFile = ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\RAMDL_";
            pathIni = ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL\\CmdRAMDL.ini";
            DirectoryResult = ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT;
            // Suppression des fichiers d'analyse et de log précédents
            //this.deleteTempFile();
            try
            {

                if (!Directory.Exists(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL"))
                {
                    Directory.CreateDirectory(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT + "\\TempoRAMDL");
                }

                if (!Directory.Exists(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT))
                {
                    Directory.CreateDirectory(ParamAppli.AnalyseImpactPathResult + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "\\" + InfoClient.ID_CLIENT);
                }

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
                    //LoggerHelper.Log(this, "INFO", "Analyse RAMDL du fichier " + sCheminMDB);

                    MiseAJourMDB(sCheminMDB);

                    GenerateIniForAnalyseImpact(sCheminMDB);

                    // Encodage du mot de passe
                    psiStartInfo = new ProcessStartInfo
                    {
                        FileName = ParamAppli.RamdDlPAth,
                        Arguments = "ENC " + pathIni
                    };
                    pProcess = System.Diagnostics.Process.Start(psiStartInfo);
                    pProcess.WaitForExit();
                    pProcess.Close();

                    // Lancement de l'analyse
                    psiStartInfo = new ProcessStartInfo
                    {
                        FileName = ParamAppli.RamdDlPAth,
                        Arguments = pathIni
                    };
                    pProcess = System.Diagnostics.Process.Start(psiStartInfo);

                    pProcess.WaitForExit();
                    pProcess.Close();

                    if (File.Exists(Path.GetDirectoryName(sCheminMDB) + "\\" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".ldb"))
                    {
                        File.Delete(Path.GetDirectoryName(sCheminMDB) + "\\" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".ldb");
                    }

                    // Contrôle du résultat
                    srResultat = new StreamReader(pathLogFile + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                    sContenuFichierLog = srResultat.ReadToEnd();
                    srResultat.Close();

                    int iIndexErreur = sContenuFichierLog.IndexOf("[Error");
                    if ((iIndexErreur > -1) && (sContenuFichierLog.IndexOf("[Error 429] ActiveX component can't create object") != iIndexErreur))
                    {
                        //LoggerHelper.Log(this, "ERROR", "Erreur lors de l'analyse du mdb " + Path.GetFileNameWithoutExtension(sCheminMDB) + ". Voir le fichier de log " + sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                    }
                    else
                    {
                        listPathResult.Add(pathResult);
                    }
                }
            }
            catch (Exception ex)
            {
                //LoggerHelper.Log(this, "ERROR", "AnalyseUnMdbRAMDL - Erreur d'exécution (exception) : " + ex.Message);
                //TODO LOG
                Console.WriteLine(ex.Message);
                return new List<String>();
            }
            return listPathResult;
        }

        private void GenerateIniForAnalyseImpact(string sCheminMDB)
        {
            string sConnection = GetConnectionStringMDB(sCheminMDB);
            OdbcDataReader reader;
            //< LOG_FILE >
            string logFile = pathLogFile + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log";
            //<ORIGIN_CONN>
            string originConn = "DRIVER={Microsoft Access Driver (*.mdb)}; DBQ=" + sCheminMDB;
            //<TARGET_CONN>
            string targetConn = MiseEnformeChaineConnexion(sConnectionStringBaseQA1);
            //<USER_CVM>
            string userCVM = sLogin;
            //<PWD_CVM>
            string pwdCVM = sMdp;
            pathResult = DirectoryResult + "\\Analyse_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".TXT";

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
                    string TmpFichierIni = pathIni;
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

            ParamAppli.GeneratePackPath = ParamAppli.DossierTemporaire + "\\RAMDL";
            pathIni = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT + "\\RamdlCmd.INI";
            // FIN MHUM pour test

            pathLogFile = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT + "\\RAMDL_DEPENDANCE_" + namePack + ".log"; //{1}
            pathResult = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT + "\\" + namePack + ".mdb";
            DirectoryResult = ParamAppli.GeneratePackPath + "\\" + WORKFLOW_ID + "\\" + ID_H_WORKFLOW + "_" + InfoClient.ID_CLIENT;
            try
            {
                if (!Directory.Exists(ParamAppli.GeneratePackPath + "\\TempoRAMDL"))
                {
                    Directory.CreateDirectory(ParamAppli.GeneratePackPath + "\\TempoRAMDL");
                }

                if (!Directory.Exists(DirectoryResult))
                {
                    Directory.CreateDirectory(DirectoryResult);
                }

                if (File.Exists(pathResult))
                {
                    File.Delete(pathResult);
                }

                GenerateIniForGeneratePack(namePack, listPack);
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
                psiStartInfo = new ProcessStartInfo
                {
                    FileName = ParamAppli.RamdDlPAth,
                    Arguments = pathIni
                };
                pProcess = System.Diagnostics.Process.Start(psiStartInfo);

                pProcess.WaitForExit();
                pProcess.Close();

                // Contrôle du résultat
                srResultat = new StreamReader(pathLogFile);
                sContenuFichierLog = srResultat.ReadToEnd();
                srResultat.Close();

                int iIndexErreur = sContenuFichierLog.IndexOf("[Error");
                if ((iIndexErreur > -1) && (sContenuFichierLog.IndexOf("[Error 429] ActiveX component can't create object") != iIndexErreur))
                {
                    //LoggerHelper.Log(this, "ERROR", "Erreur lors de l'analyse du mdb " + Path.GetFileNameWithoutExtension(sCheminMDB) + ". Voir le fichier de log " + sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                }
            }
            catch (Exception ex)
            {
                //LoggerHelper.Log(this, "ERROR", "AnalyseUnMdbRAMDL - Erreur d'exécution (exception) : " + ex.Message);
                //TODO LOG
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private void GenerateIniForGeneratePack(string namePack, string[] listTask)
        {
            //<ORIGIN_CONN>
            string originConn  = MiseEnformeChaineConnexion(ParamAppli.ConnectionStringBaseRef[InfoClient.TYPOLOGY]);
            StringBuilder slistTask = new StringBuilder(); //{ 2}
            foreach (String sTask in listTask)
            {
                slistTask.Append(sTask + Environment.NewLine);
            }
            //TBO CREATION DU FICHIER INI
            iniFile = String.Format(ParamAppli.templateIniFileGeneratePack, originConn, pathLogFile, slistTask.ToString(), pathResult);

            StreamWriter swFichierIni = new StreamWriter(pathIni, false);
            swFichierIni.WriteLine(iniFile);
            swFichierIni.Close();

        }
        private void deleteTempFile()
        {
            string[] sListeFichier = Directory.GetFiles(DirectoryResult, "*.log");
            foreach (string sFichier in sListeFichier)
            {
                File.Delete(sFichier);
            }

            sListeFichier = Directory.GetFiles(DirectoryResult, "*.TXT");
            foreach (string sFichier in sListeFichier)
            {
                File.Delete(sFichier);
            }
        }

        //=> MHUM le 22/11/2019 - Gestion création des dossiers nécessaires à l'analyse RAMDL
        //--------------------------------------------------------------------
        // Récupération du paramétrage dans la chaine
        //
        private string LitValeurParam(string pChaineEntiere, string pParam)
        {
            string sResultat = string.Empty;
            int iIndexDeb;
            int iIndexFin;

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
                //LoggerHelper.Log(this, "ERROR", "LitValeurParam - Erreur d'exécution (exception) : " + ex.Message);
                //TODO LOG
                Console.WriteLine(ex.Message);
            }
            return (sResultat);
        }

        // MHUM le 24/09/2019 - Test MAJ MDB pour supprimer les CRLF des commentaires
        private void MiseAJourMDB(string sCheminMDB)
        {
            try
            {
                OdbcConnection dbConn = new OdbcConnection
                {
                    ConnectionString = "Driver={Microsoft Access Driver (*.mdb)};Dbq=" + sCheminMDB + ";Uid=Admin;Pwd=;"
                };
                dbConn.Open();

                OdbcCommand objCmd = new OdbcCommand
                {
                    Connection = dbConn,
                    CommandType = CommandType.Text,
                    CommandText = "UPDATE M4RDL_PACK_CMDS SET CMD_COMMENTS = ' ' WHERE CMD_COMMENTS LIKE '%'+CHR(13)+CHR(10)+'%'"
                };

                objCmd.ExecuteNonQuery();
                dbConn.Close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// Met en forme une chaine de connexion en mettant la source ODBC et en enlevant le server. Prend le même code que la database pour la source ODBC.
        /// </summary>
        /// <param name="sChaineConnexion">Chaine de connexion à mettre en forme.</param>
        /// <returns>Chaine de connexion modifiée.</returns>
        private string MiseEnformeChaineConnexion(string sChaineConnexion)
        {
            string sResultat = string.Empty;
            int iIndex;
            int iIndex2;

            string sNomSourceODBC;

            iIndex = sChaineConnexion.ToUpper().IndexOf("DATABASE");
            if (iIndex > -1)
            {
                iIndex += "DATABASE".Length;
                while ((char.IsWhiteSpace(sChaineConnexion[iIndex])) || (sChaineConnexion[iIndex] == '=')) iIndex++;
                iIndex2 = iIndex;
                while (char.IsLetterOrDigit(sChaineConnexion[iIndex2])) iIndex2++;
                sNomSourceODBC = sChaineConnexion.Substring(iIndex, iIndex2 - iIndex);
                sResultat = MiseEnformeChaineConnexion(sChaineConnexion, sNomSourceODBC);
            }
            return (sResultat);
        }

        /// <summary>
        /// Met en forme une chaine de connexion en mettant la source ODBC et en enlevant le server.
        /// </summary>
        /// <param name="sChaineConnexion">Chaine de connexion à mettre en forme.</param>
        /// <param name="sNomSourceODBC">Code de la source ODBC à mettre dans la chaine de connexion.</param>
        /// <returns>Chaine de connexion modifiée.</returns>
        private string MiseEnformeChaineConnexion(string sChaineConnexion, string sNomSourceODBC)
        {
            string sResultat = string.Empty;
            int iIndex;

            if (sChaineConnexion.ToUpper().Substring(0, 6) == "SERVER")
            {
                iIndex = sChaineConnexion.IndexOf(";");
                if (iIndex > -1)
                {
                    sResultat = "DSN=" + sNomSourceODBC + sChaineConnexion.Substring(iIndex);
                }
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
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\META4\\regmeta4.xml"))
            {
                StreamReader srRegMeta4 = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\META4\\regmeta4.xml");
                bool bContinue = !srRegMeta4.EndOfStream;
                string sBuffer;
                string sBuffer2;


                while (bContinue)
                {
                    sBuffer = srRegMeta4.ReadLine();
                    if (sBuffer.IndexOf("<RAMDL ") > -1)
                    {
                        sBuffer2 = LitValeurParam(sBuffer, "LastCacheDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        sBuffer2 = LitValeurParam(sBuffer, "LastLogDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        sBuffer2 = LitValeurParam(sBuffer, "LastCVSDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        sBuffer2 = LitValeurParam(sBuffer, "LastClientDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        sBuffer2 = LitValeurParam(sBuffer, "LastPackageDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        sBuffer2 = LitValeurParam(sBuffer, "LastStandardDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        sBuffer2 = LitValeurParam(sBuffer, "LastBackupDirectory=");
                        if (sBuffer2 != string.Empty && !Directory.Exists(sBuffer2))
                        {
                            Directory.CreateDirectory(sBuffer2);
                        }

                        bContinue = false;
                    }
                    else
                    {
                        bContinue = !srRegMeta4.EndOfStream;
                    }
                }

            }
            //<= MHUM le 22/11/2019
        }


    }


}
