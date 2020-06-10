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
        public RapportProcessAnalyseImpact RapportProcess;
        //public list<ElementALocaliser> listElementALocaliser; 
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

            
            List<IControle> listControl = ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportProcess.Name = this.LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportAnalyseLogique rapportAnalyseLogique = new RapportAnalyseLogique();

            //On génère les historic au début pour mettre en inprogress
            //DEVGenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress);

            //Lancement analyse d'impact RamDl
            RamdlTool ramdlTool = new RamdlTool(CLIENT_ID, Decimal.ToInt32(WORKFLOW_ID));
            List<String> pathList = ramdlTool.AnalyseMdbRAMDL();
            List<AnalyseResultFile> resultFileList = new List<AnalyseResultFile>();
            foreach (string pathFile in pathList)
            {
                using (StreamReader reader = new StreamReader(pathFile))
                {
                    string line;
                    AnalyseResultFile resultFile = new AnalyseResultFile(pathFile, Path.GetFileName(pathFile));
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] listElement = line.Split('\t');
                        AnalyseResultLine resultLine = new AnalyseResultLine(listElement);
                        resultFile.addLine(resultLine);
                    }
                    resultFileList.Add(resultFile);
                }
            }
            this.addRapportAnalyseLogique(rapportAnalyseLogique, resultFileList);

            RapportProcess.rapportAnalyseLogique = rapportAnalyseLogique;

            //Gestion contrôle data
            //GetListControle(ref listControl);
            RapportAnalyseData rapportAnalyseData = new RapportAnalyseData();
            List<ControleAnalyseData> listControleAnalyseData = new List<ControleAnalyseData>();

            ControleAnalyseData controleAnalyseData = new ControleAnalyseData();

            controleAnalyseData.Name = "Controle KO table bidule etc..";
            controleAnalyseData.Tooltip = "";
            controleAnalyseData.Result = "Warning";

            listControleAnalyseData.Add(controleAnalyseData);
            //gestion des requêtes data génériques (controle)



            /*foreach (IControle controle in listControl)
            {
                Logger.Log(this, controle, ParamAppli.StatutInfo, "Début du contrôle " + controle.ToString());
                statutControle = controle.MakeControl();
                Logger.Log(this, controle, statutControle, "Fin du contrôle " + controle.ToString());
                //ERROR > WARNING > OK
                if (GlobalResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutError)
                {
                    GlobalResult = statutControle;

                }
                else if (GlobalResult != ParamAppli.StatutError && statutControle == ParamAppli.StatutWarning)
                {
                    GlobalResult = statutControle;
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


            rapportAnalyseData.listControleAnalyseData = listControleAnalyseData;
            RapportProcess.rapportAnalyseData = rapportAnalyseData;
            



            //Elements à localiser





            RapportProcess.Fin = DateTime.Now;
            RapportProcess.Result = ParamAppli.TranscoSatut[GlobalResult];
            
            //On fait un update pour la date de fin du process et son statut
            //DEVGenerateHistoric(RapportProcess.Fin, GlobalResult);


            /*DEVif (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessAnalyseImpact);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID);
            }*/

        }

        private void addRapportAnalyseLogique(RapportAnalyseLogique rapportAnalyseLogique, List<AnalyseResultFile> resultFile)
        {
            rapportAnalyseLogique.listTypeAnalyseLogique = new List<TypeAnalyseLogique>();
            TypeAnalyseLogique typeNew = new TypeAnalyseLogique();
            typeNew.Name = "New";
            typeNew.Tooltip = "Ceci contient la liste des élements nouveau du pack";
            typeNew.listLineAnalyseLogique = new List<LineAnalyseLogique>();

            TypeAnalyseLogique typeModified = new TypeAnalyseLogique();
            typeModified.Name = "Modified";
            typeModified.Tooltip = "Ceci contient la liste des élements modifié du pack";
            typeModified.listLineAnalyseLogique = new List<LineAnalyseLogique>();

            TypeAnalyseLogique typeDeleted = new TypeAnalyseLogique();
            typeDeleted.Name = "Deleted";
            typeDeleted.Tooltip = "Ceci contient la liste des élements supprimées du pack";
            typeDeleted.listLineAnalyseLogique = new List<LineAnalyseLogique>();


            foreach (AnalyseResultFile file in resultFile)
            {
                foreach(AnalyseResultLine line in file.ListLine())
                {
                    //TODO METTRE ELEMENT DANS PARAM APPLI
                    if(line.TransferFlag == "New")
                    {

                    }else if(line.TransferFlag == "")
                    {

                    }else if (line.TransferFlag == "New")
                    {

                    }
                }
            }

        }
    }
}