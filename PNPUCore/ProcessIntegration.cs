using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;

using System;
using System.Collections.Generic;

namespace PNPUCore.Process
{
    internal class ProcessIntegration : ProcessCore, IProcess
    {

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessIntegration(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = ParamAppli.ProcessIntegration;
            LibProcess = "Intégration";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessIntegration(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }
        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportProcess.Name = LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();
            int idInstanceWF = ID_INSTANCEWF;
            Dictionary<string, List<string>> dResultat = new Dictionary<string, List<string>>();
            Rapport.Source RapportSource;
            string SourceResult = ParamAppli.StatutOk;


            // MHUM A terme devra être récupéré depuis l'écran de lancement des WF
            bool bRemovePack = true;


            // MHUM Pour tests
            /*GereMDBDansBDD gereMDBDansBDD = new GereMDBDansBDD();
            gereMDBDansBDD.AjouteZipBDD(@"D:\PNPU\MDB de test\Nouveau dossier\TEST_INSTALL_N1.zip", WORKFLOW_ID, ParamAppli.ConnectionStringBaseAppli, idInstanceWF, CLIENT_ID, 1);
            gereMDBDansBDD.AjouteZipBDD(@"D:\PNPU\MDB de test\Nouveau dossier\TEST_INSTALL_N2.zip", WORKFLOW_ID, ParamAppli.ConnectionStringBaseAppli, idInstanceWF, CLIENT_ID, 2);
            gereMDBDansBDD.AjouteZipBDD(@"D:\PNPU\MDB de test\Nouveau dossier\TEST_INSTALL_N3.zip", WORKFLOW_ID, ParamAppli.ConnectionStringBaseAppli, idInstanceWF, CLIENT_ID, 3);*/

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress, RapportProcess.Debut);

            Logger.Log(this, ParamAppli.StatutInfo, " Debut du process " + ToString());
            RamdlTool ramdlTool = new RamdlTool(CLIENT_ID, Decimal.ToInt32(WORKFLOW_ID), ID_INSTANCEWF);


            // Gestion des packs de dépendance
            for (int iNiv = 3; iNiv > 0; iNiv--)
            {
                ramdlTool.InstallMdbRAMDL(iNiv, ref dResultat, bRemovePack);
                if (dResultat.Count > 0)
                {
                    RapportSource = new Rapport.Source();
                    RapportSource.Name = "Installation des packs de dépendance Niveau " + iNiv.ToString();
                    RapportSource.Controle = new List<RControle>();
                    SourceResult = ParamAppli.StatutOk;

                    foreach (string sMdb in dResultat.Keys)
                    {
                        RControle RapportControle = new RControle();
                        RapportControle.Message = new List<string>();
                        RapportControle.Name = sMdb;

                        if (dResultat[sMdb].Count > 0)
                        {
                            if ((dResultat[sMdb].Count == 1) && (dResultat[sMdb][0] == "Aucun nouveau pack trouvé dans le mdb."))
                            {
                                RapportControle.Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning];
                                if (SourceResult == ParamAppli.StatutOk)
                                    SourceResult = ParamAppli.StatutWarning;
                            }
                            else
                            {
                                SourceResult = ParamAppli.StatutError;
                                RapportControle.Result = ParamAppli.TranscoSatut[ParamAppli.StatutError];
                            }
                            RapportControle.Message = dResultat[sMdb];
                        }
                        else
                            RapportControle.Result = ParamAppli.TranscoSatut[ParamAppli.StatutOk];

                        RapportSource.Controle.Add(RapportControle);
                        RapportSource.Result = ParamAppli.TranscoSatut[SourceResult];
                        if (GlobalResult == ParamAppli.StatutOk)
                            GlobalResult = SourceResult;
                        else if ((GlobalResult == ParamAppli.StatutWarning) && (SourceResult == ParamAppli.StatutError))
                            GlobalResult = ParamAppli.StatutError;

                    }
                    RapportProcess.Source.Add(RapportSource);
                    dResultat.Clear();
                }
            }


            //Lancement installation mdb

            RapportSource = new Rapport.Source();
            RapportSource.Name = "Installation des packs du HF";
            RapportSource.Controle = new List<RControle>();
            SourceResult = ParamAppli.StatutOk;

            ramdlTool.InstallMdbRAMDL(0, ref dResultat, bRemovePack);


            foreach (string sMdb in dResultat.Keys)
            {
                RControle RapportControle = new RControle();
                RapportControle.Message = new List<string>();
                RapportControle.Name = sMdb;

                if (dResultat[sMdb].Count > 0)
                {
                    if ((dResultat[sMdb].Count == 1) && (dResultat[sMdb][0] == "Aucun nouveau pack trouvé dans le mdb."))
                    {
                        RapportControle.Result = ParamAppli.TranscoSatut[ParamAppli.StatutWarning];
                        if (SourceResult == ParamAppli.StatutOk)
                            SourceResult = ParamAppli.StatutWarning;
                    }
                    else
                    {
                        SourceResult = ParamAppli.StatutError;
                        RapportControle.Result = ParamAppli.TranscoSatut[ParamAppli.StatutError];
                    }
                    RapportControle.Message = dResultat[sMdb];
                }
                else
                    RapportControle.Result = ParamAppli.TranscoSatut[ParamAppli.StatutOk];

                RapportSource.Controle.Add(RapportControle);
                RapportSource.Result = ParamAppli.TranscoSatut[SourceResult];
                if (GlobalResult == ParamAppli.StatutOk)
                    GlobalResult = SourceResult;
                else if ((GlobalResult == ParamAppli.StatutWarning) && (SourceResult == ParamAppli.StatutError))
                    GlobalResult = ParamAppli.StatutError;

            }
            RapportProcess.Source.Add(RapportSource);

            RapportProcess.Fin = DateTime.Now;
            RapportProcess.Result = ParamAppli.TranscoSatut[GlobalResult];

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult, RapportProcess.Debut);
            Logger.Log(this, GlobalResult, "Fin du process " + ToString());

            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessIntegration);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, idInstanceWF);
            }

        }

    }
}