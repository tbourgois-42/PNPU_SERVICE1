using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace PNPUCore.Process
{
    internal class ProcessProcessusCritique : ProcessCore, IProcess
    {
        public string[] sConnectionString;
        public List<Rapport.Source>[] rSource;
        public bool[] bThreadTermine;
        public List<string>[] lTraitements;
        private ThreadProcessusCritique[] threadProcessusCritiques;
        


        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessProcessusCritique(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = ParamAppli.ProcessProcessusCritique;
            LibProcess = "Tests des processus critiques";


        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessProcessusCritique(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportProcess.Name = LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();
            RapportProcess.Result = ParamAppli.StatutOk;
            int idInstanceWF = ID_INSTANCEWF;
            Source RapportSource;
            RControle RapportControle;
            StringBuilder stringBuilder = new StringBuilder();
            bool bTraitementInterrompu;
            string sMessageErreur;
            string sModelCode = "PNPU_TRT_CRIT";
            string sRequete;
            DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
            DataSet dataSet;
            bool bTraitementsIdentiques = true;
            bool[] bCSIsValide = new bool[2];
            string sDataBase;

            sConnectionString = new string[2];

            // MHUM pour tests, nous ne récupérons pas les orgas des clients
            ParamAppli.ListeInfoClient[CLIENT_ID].ID_ORGA = "0002";

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress, RapportProcess.Debut);

            ParamToolbox paramToolbox = new ParamToolbox();

            
            rSource = new List<Source>[2];
            rSource[0] = new List<Source>();
            rSource[1] = new List<Source>();

            lTraitements = new List<string>[2];
            lTraitements[0] = new List<string>();
            lTraitements[1] = new List<string>();

            // MHUM POUR TESTS, j'utilise les clients zero SAAS dédié 
            sConnectionString[0] = "server=10.113.24.81;uid=FRACUSQA1;pwd=FRACUSQA1;database=FRACUSQA1;";
            sConnectionString[1] = "server=10.113.24.81;uid=FRACUSQA2;pwd=FRACUSQA2;database=FRACUSQA2;";
            //sConnectionString[0] = paramToolbox.GetConnexionString("Before", WORKFLOW_ID, CLIENT_ID, ID_INSTANCEWF);
            //sConnectionString[1] = paramToolbox.GetConnexionString("After", WORKFLOW_ID, CLIENT_ID, ID_INSTANCEWF);

            bCSIsValide[0] = dataManagerSQL.CheckConnectionString(sConnectionString[0]);
            bCSIsValide[1] = dataManagerSQL.CheckConnectionString(sConnectionString[1]);

            bThreadTermine = new bool[2];
            bThreadTermine[0] = false;
            bThreadTermine[1] = false;

            // Vérification que les traitements groupés sont paramétré de la même façon sur les deux environnements
            // Lecture du paramétrage du premier environnement
            sRequete = "select A.CFR_ID_STEP, B.CFR_TASK_NMFRA, B.CFR_METHODE_LANCEMENT from M4CFR_MODEL_TACHES A,M4CFR_TACHES B where A.ID_ORGANIZATION='" + ParamAppli.ListeInfoClient[CLIENT_ID].ID_ORGA + "' AND A.ID_ORGANIZATION=B.ID_ORGANIZATION AND A.CFR_ID_MODEL='" + sModelCode + "' AND A.CFR_ID_TASK=B.CFR_ID_TASK ORDER BY A.CFR_ID_STEP";
            dataSet = dataManagerSQL.GetData(sRequete, sConnectionString[0]);
            if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dataSet.Tables[0].Rows)
                {
                    lTraitements[0].Add(drRow[1].ToString());
                }
            }

            // Lecture du paramétrage du deuxième environnement
            dataSet = dataManagerSQL.GetData(sRequete, sConnectionString[1]);
            if ((dataSet != null) && (dataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dataSet.Tables[0].Rows)
                {
                    lTraitements[1].Add(drRow[1].ToString());
                }
            }

            // Comparaison du paramétrage des 2 environnements
            if (lTraitements[0].Count != lTraitements[1].Count)
                bTraitementsIdentiques = false;
            else
            {
                for (int index = 0; (index < lTraitements[0].Count) && (bTraitementsIdentiques); index++)
                {
                    if (lTraitements[0][index] != lTraitements[1][index])
                    { 
                        bTraitementsIdentiques = false; 
                    }
                }

            }

            if ((bTraitementsIdentiques) && (bCSIsValide[0]) && (bCSIsValide[1]))
            {
                // Instanciation des objets thrad
                threadProcessusCritiques = new ThreadProcessusCritique[2];
                threadProcessusCritiques[0] = new ThreadProcessusCritique(this, 0);
                threadProcessusCritiques[1] = new ThreadProcessusCritique(this, 1);

                Thread[] thread = new Thread[2];
                thread[0] = new Thread(new ThreadStart(threadProcessusCritiques[0].ThreadProcCrit));
                thread[1] = new Thread(new ThreadStart(threadProcessusCritiques[1].ThreadProcCrit));

                thread[0].Start();
                thread[1].Start();

                // Attente de la fin des 2 threads
                while ((!bThreadTermine[0]) || (!bThreadTermine[1])) Thread.Sleep(500);


                for (int indexSource = 0; indexSource < rSource[0].Count; indexSource++)
                {
                    RapportSource = new Source();
                    RapportSource.Controle = new List<RControle>();

                    RapportSource.Name = rSource[0][indexSource].Name;
                    RapportSource.Result = ParamAppli.StatutOk;

                    for (int indexControle = 0; indexControle < rSource[0][indexSource].Controle.Count; indexControle++)
                    {
                        bTraitementInterrompu = false;
                        RapportControle = new RControle();
                        RapportControle.Message = new List<string>();
                        RapportControle.Result = ParamAppli.StatutOk;
                        int iIndexString = rSource[0][indexSource].Controle[indexControle].Name.IndexOf('(');
                        RapportControle.Name = rSource[0][indexSource].Controle[indexControle].Name.Substring(0, iIndexString);
                        RapportControle.Tooltip = rSource[0][indexSource].Controle[indexControle].Tooltip;
                        if ((rSource[0][indexSource].Controle[indexControle].Result == ParamAppli.StatutOk) && (rSource[1][indexSource].Controle[indexControle].Result == ParamAppli.StatutOk))
                        {
                            RapportControle.Result = ParamAppli.StatutOk;
                        }
                        else if ((rSource[0][indexSource].Controle[indexControle].Result == ParamAppli.StatutError) && (rSource[1][indexSource].Controle[indexControle].Result == ParamAppli.StatutError))
                        {
                            RapportControle.Result = ParamAppli.StatutWarning;
                        }
                        else
                        {
                            RapportControle.Result = ParamAppli.StatutError;
                        }

                        // Constitution du report du résultat
                        stringBuilder.Clear();
                        stringBuilder.Append("Base avant : ");
                        stringBuilder.Append(rSource[0][indexSource].Controle[indexControle].Name);
                        stringBuilder.Append(" - ");
                        if (rSource[0][indexSource].Controle[indexControle].Result == ParamAppli.StatutOk)
                        {
                            stringBuilder.Append("OK");
                        }
                        else
                        {
                            stringBuilder.Append("Erreur - ");
                            if (rSource[0][indexSource].Controle[indexControle].Message.Count > 0)
                            {
                                sMessageErreur = rSource[0][indexSource].Controle[indexControle].Message[0];
                                stringBuilder.Append(sMessageErreur);
                                if ((sMessageErreur.Contains("annulée")) || (sMessageErreur.Contains("expirée")) || (sMessageErreur.Contains("interrompue")))
                                    bTraitementInterrompu = true;
                            }
                        }
                        RapportControle.Message.Add(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append("Base après : ");
                        stringBuilder.Append(rSource[1][indexSource].Controle[indexControle].Name);
                        stringBuilder.Append(" - ");
                        if (rSource[1][indexSource].Controle[indexControle].Result == ParamAppli.StatutOk)
                        {
                            stringBuilder.Append("OK");
                        }
                        else
                        {
                            stringBuilder.Append("Erreur - ");
                            if (rSource[1][indexSource].Controle[indexControle].Message.Count > 0)
                            {
                                sMessageErreur = rSource[1][indexSource].Controle[indexControle].Message[0];
                                stringBuilder.Append(sMessageErreur);
                                if ((sMessageErreur.Contains("annulée")) || (sMessageErreur.Contains("expirée")) || (sMessageErreur.Contains("interrompue")))
                                    bTraitementInterrompu = true;
                            }
                        }
                        RapportControle.Message.Add(stringBuilder.ToString());

                        // Gestion du cas où la tâche ne s'est pas terminée sur au moins un environnement
                        if (bTraitementInterrompu)
                        {
                            RapportControle.Message.Add("La tâche ne s'est pas terminée. La comparaison entre les deux environnements n'est pas pertinente.");
                            RapportControle.Result = ParamAppli.StatutInfo;
                        }

                        if (RapportSource.Result == ParamAppli.StatutOk)
                        {
                            RapportSource.Result = RapportControle.Result;
                        }
                        else if ((RapportSource.Result == ParamAppli.StatutWarning) && (RapportControle.Result == ParamAppli.StatutError))
                        {
                            RapportSource.Result = ParamAppli.StatutError;
                        }

                        RapportControle.Result = ParamAppli.TranscoSatut[RapportControle.Result];
                        RapportSource.Controle.Add(RapportControle);
                    }

                    if (RapportProcess.Result == ParamAppli.StatutOk)
                    {
                        RapportProcess.Result = RapportSource.Result;
                    }
                    else if ((RapportProcess.Result == ParamAppli.StatutWarning) && (RapportSource.Result == ParamAppli.StatutError))
                    {
                        RapportProcess.Result = ParamAppli.StatutError;
                    }
                    RapportSource.Result = ParamAppli.TranscoSatut[RapportSource.Result];
                    RapportProcess.Source.Add(RapportSource);
                }
            }
            // Les chaines de connexions sont invalides
            else if ((!bCSIsValide[0]) && (!bCSIsValide[1]))  
            {
                sDataBase = dataManagerSQL.ExtractDataBase(sConnectionString[0]);
                if (sDataBase == string.Empty)
                {
                    sDataBase = "QA1";
                }
                stringBuilder.Clear();
                stringBuilder.AppendFormat("Echec de la connexion sur les serveurs clients {0}", sDataBase);
                sDataBase = dataManagerSQL.ExtractDataBase(sConnectionString[1]);
                if (sDataBase == string.Empty)
                {
                    sDataBase = "QA2";
                }
                stringBuilder.AppendFormat(" et {0}. Veuillez contacter un administrateur", sDataBase);
                RapportSource = new Rapport.Source
                {
                    Name = "Planification des processus critiques",
                    Controle = new List<RControle>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };
                RapportControle = new RControle
                {
                    Name = "ERREUR DE CONNEXION",
                    Tooltip = stringBuilder.ToString(),
                    Message = new List<string>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };
                
                RapportControle.Message.Add(stringBuilder.ToString());
                RapportSource.Controle.Add(RapportControle);
                RapportProcess.Result = ParamAppli.StatutWarning;
                RapportProcess.Source.Add(RapportSource);
            }
            else if (!bCSIsValide[0])
            {
                sDataBase = dataManagerSQL.ExtractDataBase(sConnectionString[0]);
                if (sDataBase == string.Empty)
                {
                    sDataBase = "QA1";
                }
                stringBuilder.Clear();
                stringBuilder.AppendFormat("Echec de la connexion sur le serveur client {0}. Veuillez contacter un administrateur",sDataBase);
                RapportSource = new Rapport.Source
                {
                    Name = "Planification des processus critiques",
                    Controle = new List<RControle>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };
                RapportControle = new RControle
                {
                    Name = "ERREUR DE CONNEXION",
                    Tooltip = stringBuilder.ToString(),
                    Message = new List<string>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };

                RapportControle.Message.Add(stringBuilder.ToString());
                RapportSource.Controle.Add(RapportControle);
                RapportProcess.Result = ParamAppli.StatutWarning;
                RapportProcess.Source.Add(RapportSource);
            }
            else if (!bCSIsValide[1])
            {
                sDataBase = dataManagerSQL.ExtractDataBase(sConnectionString[1]);
                if (sDataBase == string.Empty)
                {
                    sDataBase = "QA2";
                }
                stringBuilder.Clear();
                stringBuilder.AppendFormat("Echec de la connexion sur le serveur client {0}. Veuillez contacter un administrateur", sDataBase);
                RapportSource = new Rapport.Source
                {
                    Name = "Planification des processus critiques",
                    Controle = new List<RControle>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };
                RapportControle = new RControle
                {
                    Name = "ERREUR DE CONNEXION",
                    Tooltip = stringBuilder.ToString(),
                    Message = new List<string>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };

                RapportControle.Message.Add(stringBuilder.ToString());
                RapportSource.Controle.Add(RapportControle);
                RapportProcess.Result = ParamAppli.StatutWarning;
                RapportProcess.Source.Add(RapportSource);
            }
            else // Les traitements groupés sont paramétrés différement sur les 2 environnements
            {
                RapportSource = new Rapport.Source
                {
                    Name = "Planification des processus critiques",
                    Controle = new List<RControle>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };
                RapportControle = new RControle
                {
                    Name = "Planification",
                    Tooltip = "Génération de la planification des processus critiques",
                    Message = new List<string>(),
                    Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning]
                };
                stringBuilder.Clear();
                stringBuilder.AppendFormat("Le paramétrage des traitements groupés {0} est différent entre les deux environnements. Le processus est annulé.", sModelCode);
                
                RapportControle.Message.Add(stringBuilder.ToString());
                RapportSource.Controle.Add(RapportControle);
                RapportProcess.Result = ParamAppli.StatutWarning;
                RapportProcess.Source.Add(RapportSource);
            }
            RapportProcess.Fin = DateTime.Now;
            RapportProcess.Result = ParamAppli.TranscoSatut[RapportProcess.Result];

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult, RapportProcess.Debut);



            // Suppresion des paramètres toolbox temporaires

            paramToolbox.DeleteParamsToolbox(WORKFLOW_ID, ID_INSTANCEWF);

            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessProcessusCritique);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            }
            
        }
        
    }
}