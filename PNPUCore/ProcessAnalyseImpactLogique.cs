using PNPUCore.Controle;
using PNPUCore.Rapport;
using PNPUTools;
using System;
using System.Collections.Generic;
using System.IO;

namespace PNPUCore.Process
{
    internal class ProcessAnalyseImpactLogique : ProcessCore, IProcess
    {

        public RapportProcessAnalyseImpact RapportAnalyseImpact;


        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessAnalyseImpactLogique(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            PROCESS_ID = ParamAppli.ProcessAnalyseImpactLogique;
            LibProcess = "Analyse d'impact logique";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessAnalyseImpactLogique(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportAnalyseImpactLogique = new RapportAnalyseLogique();
            RapportAnalyseImpactLogique.Name = LibProcess;
            RapportAnalyseImpactLogique.Debut = DateTime.Now;
            RapportAnalyseImpactLogique.IdClient = CLIENT_ID;

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress, DateTime.Now);

            ParamToolbox paramToolbox = new ParamToolbox();

            //Lancement analyse d'impact RamDl
            RamdlTool ramdlTool = new RamdlTool(CLIENT_ID, Decimal.ToInt32(WORKFLOW_ID), ID_INSTANCEWF);
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
            }

            addRapportAnalyseLogique(RapportAnalyseImpactLogique, resultFileList);


            //Elements à localiser


            RapportAnalyseImpactLogique.Fin = DateTime.Now;
            RapportAnalyseImpactLogique.Result = ParamAppli.TranscoSatut[GlobalResult];

            //On fait un update pour la date de fin du process et son statut
            GenerateHistoric(RapportProcess.Fin, GlobalResult, RapportAnalyseImpact.Debut);

            // Suppresion des paramètres toolbox temporaires
            paramToolbox.DeleteParamsToolbox(WORKFLOW_ID, ID_INSTANCEWF);


            if (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessAnalyseImpact);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(WORKFLOW_ID), CLIENT_ID, ID_INSTANCEWF);
            }

        }

        private void addRapportAnalyseLogique(RapportAnalyseLogique rapportAnalyseLogique, List<AnalyseResultFile> resultFile)
        {

            List<RapportAnalyseImpactMDBLogique> listRapportAnalyseImpactMDBLogique = new List<RapportAnalyseImpactMDBLogique>();
            LineAnalyseLogique lineAnalyseLogique;
            RapportAnalyseImpactMDBLogique analyseMdbLogique;
            foreach (AnalyseResultFile file in resultFile)
            {

                analyseMdbLogique = new RapportAnalyseImpactMDBLogique();
                analyseMdbLogique.Name = file.fileName;
                analyseMdbLogique.Tooltip = "";
                analyseMdbLogique.listTypeAnalyseLogique = new List<TypeAnalyseLogique>();

                TypeAnalyseLogique typeNew = new TypeAnalyseLogique();
                typeNew.Name = "New";
                typeNew.Tooltip = "Ceci contient la liste des élements nouveaux du pack";
                typeNew.listLineAnalyseLogique = new List<LineAnalyseLogique>();

                TypeAnalyseLogique typeModified = new TypeAnalyseLogique();
                typeModified.Name = "Modified";
                typeModified.Tooltip = "Ceci contient la liste des élements modifiés du pack";
                typeModified.listLineAnalyseLogique = new List<LineAnalyseLogique>();

                TypeAnalyseLogique typeHerited = new TypeAnalyseLogique();
                typeModified.Name = "Herited";
                typeModified.Tooltip = "Ceci contient la liste des élements modifiés sur un élément hérités du pack";
                typeModified.listLineAnalyseLogique = new List<LineAnalyseLogique>();

                TypeAnalyseLogique typeDeleted = new TypeAnalyseLogique();
                typeDeleted.Name = "Deleted";
                typeDeleted.Tooltip = "Ceci contient la liste des élements supprimés du pack";
                typeDeleted.listLineAnalyseLogique = new List<LineAnalyseLogique>();

                foreach (AnalyseResultLine line in file.ListLine())
                {
                    //TODO METTRE ELEMENT DANS PARAM APPLI
                    if (line.OriginDestination == "New")
                    {
                        lineAnalyseLogique = new LineAnalyseLogique();
                        lineAnalyseLogique.Name = line.ObjectType + " : " + line.IdObject + ", " + line.IdObject2 + ", " + line.IdObject3 + ", " + line.IdObject4;
                        lineAnalyseLogique.Tooltip = "";

                        typeNew.listLineAnalyseLogique.Add(lineAnalyseLogique);
                    }
                    else if (line.OriginDestination == "Equal")
                    {
                        lineAnalyseLogique = new LineAnalyseLogique();
                        lineAnalyseLogique.Name = "EQUAL  :  " + line.ObjectType + " : " + line.IdObject + ", " + line.IdObject2 + ", " + line.IdObject3 + ", " + line.IdObject4;
                        lineAnalyseLogique.Tooltip = "";

                        typeModified.listLineAnalyseLogique.Add(lineAnalyseLogique);
                    }
                    else if (line.OriginDestination == "Modified")
                    {

                        lineAnalyseLogique = new LineAnalyseLogique();
                        lineAnalyseLogique.Name = "MODIFIED  :  " + line.ObjectType + " : " + line.IdObject + ", " + line.IdObject2 + ", " + line.IdObject3 + ", " + line.IdObject4;
                        lineAnalyseLogique.Tooltip = "";

                        if (line.CommandDetail == "Inherit")
                        {
                            typeHerited.listLineAnalyseLogique.Add(lineAnalyseLogique);
                            RequestTool.AddLocalisationByALineAnalyseLogique(CLIENT_ID, WORKFLOW_ID, line.Package, line, ID_INSTANCEWF);
                            //TODO ADD LOCALISATION ON THIS ELEMENT
                        }
                        else
                        {
                            typeModified.listLineAnalyseLogique.Add(lineAnalyseLogique);
                        }
                    }
                    else if (line.OriginDestination == "Delete")
                    {
                        lineAnalyseLogique = new LineAnalyseLogique();
                        lineAnalyseLogique.Name = line.ObjectType + " : " + line.IdObject + ", " + line.IdObject2 + ", " + line.IdObject3 + ", " + line.IdObject4;
                        lineAnalyseLogique.Tooltip = "";

                        typeModified.listLineAnalyseLogique.Add(lineAnalyseLogique);
                    }
                }

                analyseMdbLogique.listTypeAnalyseLogique.Add(typeNew);
                analyseMdbLogique.listTypeAnalyseLogique.Add(typeNew);
                analyseMdbLogique.listTypeAnalyseLogique.Add(typeNew);
                listRapportAnalyseImpactMDBLogique.Add(analyseMdbLogique);
            }

            rapportAnalyseLogique.listRapportAnalyseImpactMDBLogique = listRapportAnalyseImpactMDBLogique;



        }

    }
}
