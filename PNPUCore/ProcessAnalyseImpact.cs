﻿using PNPUCore.Controle;
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
        public RapportAnalyseData RapportAnalyseDataCourant;
        //public list<ElementALocaliser> listElementALocaliser; 
        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessAnalyseImpact(int wORKFLOW_ID, string cLIENT_ID) : base(wORKFLOW_ID, cLIENT_ID)
        {
            this.PROCESS_ID = ParamAppli.ProcessAnalyseImpact;
            this.LibProcess = "Analyse d'impact";
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
            RapportProcess = new RapportProcessAnalyseImpact();
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


            //RapportProcess.rapportAnalyseData = rapportAnalyseData;
            



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

                foreach (AnalyseResultLine line in file.ListLine())
                {
                    //TODO METTRE ELEMENT DANS PARAM APPLI
                    if(line.OriginDestination == "New")
                    {
                        lineAnalyseLogique = new LineAnalyseLogique();
                        lineAnalyseLogique.Name = line.ObjectType + " : " + line.IdObject + ", " + line.IdObject2 + ", " + line.IdObject3 + ", " + line.IdObject4;
                        lineAnalyseLogique.Tooltip = "";

                        typeNew.listLineAnalyseLogique.Add(lineAnalyseLogique);
                    }else if(line.OriginDestination == "Equal")
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

                        typeModified.listLineAnalyseLogique.Add(lineAnalyseLogique);
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

        public void ExtractTableFilter(string sCommand, out string sTable, out string sFilter)
        {
            if (sCommand.ToUpper().IndexOf("EXEC") >= 0)
            {
                sTable = ExtractParameter(sCommand, "@table");
                sFilter = ExtractParameter(sCommand, "@opt_where").Replace("''", "'");
            }
            else
            {
                sTable = ExtractParameter(sCommand, 1);
                sFilter = ExtractParameter(sCommand, 4).Replace("''", "'");
            }

        }

        private string ExtractParameter(string sCommand, string sParameterName)
        {
            int iIndex1, iIndex2;
            string sResultat = String.Empty;
            bool bContinue = true;

            try
            {
                iIndex1 = sCommand.IndexOf(sParameterName);
                if (iIndex1 >= 0)
                {
                    iIndex1 += sParameterName.Length;
                    while (sCommand[iIndex1] != '\'') iIndex1++;
                    iIndex1++;
                    iIndex2 = iIndex1;
                    while (bContinue == true)
                    {
                        if (iIndex2 >= sCommand.Length)
                            return (String.Empty);

                        if (sCommand[iIndex2] == '\'')
                        {
                            if (sCommand.Substring(iIndex2, 2) == "''")
                                iIndex2 += 2;
                            else
                                bContinue = false;
                        }
                        else
                            iIndex2++;
                    }

                    sResultat = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                }

            }
            catch (Exception)
            {
                sResultat = String.Empty;
            }
            return sResultat;
        }

        private string ExtractParameter(string sCommand, int iParameterPos)
        {
            int iIndex1 = 0;
            int iIndex2 = 0;
            string sResultat = String.Empty;
            bool bContinue = true;
            int iNumeroParam = 1;

            try
            {
                iIndex1 = sCommand.IndexOf("M4SFR_COPY_DATA_ORG");
                if (iIndex1 >= 0)
                {
                    iIndex1 += "M4SFR_COPY_DATA_ORG".Length;
                    while (sCommand[iIndex1] != '\'') iIndex1++;
                    iIndex1++;
                    while (iNumeroParam < iParameterPos)
                    {
                        iIndex1 = sCommand.IndexOf(',', iIndex1);
                        if (iIndex1 == -1)
                            return String.Empty;

                        while (sCommand[iIndex1] != '\'') iIndex1++;
                        iIndex1++;

                        // Test si on est sur '', dans le filtre par exemple
                        if (sCommand[iIndex1] != '\'')
                            iNumeroParam++;
                    }

                    if (iNumeroParam == iParameterPos)
                    {
                        iIndex2 = iIndex1;
                        while (bContinue == true)
                        {
                            if (iIndex2 >= sCommand.Length)
                                return (String.Empty);

                            if (sCommand[iIndex2] == '\'')
                            {
                                if (sCommand.Substring(iIndex2, 2) == "''")
                                    iIndex2 += 2;
                                else
                                    bContinue = false;
                            }
                            else
                                iIndex2++;
                        }
                    }

                    sResultat = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                }

            }
            catch (Exception)
            {
                sResultat = String.Empty;
            }
            return sResultat;
        }

    }
}