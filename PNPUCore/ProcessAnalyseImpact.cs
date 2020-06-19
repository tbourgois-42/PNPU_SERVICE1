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

        public ProcessAnalyseImpact(int wORKFLOW_ID, string cLIENT_ID, int idInstanceWF) : base(wORKFLOW_ID, cLIENT_ID, idInstanceWF)
        {
            this.PROCESS_ID = ParamAppli.ProcessAnalyseImpact;
            this.LibProcess = "Analyse d'impact";
        }

        internal static new IProcess CreateProcess(int WORKFLOW_ID, string CLIENT_ID, int idInstanceWF)
        {
            return new ProcessAnalyseImpact(WORKFLOW_ID, CLIENT_ID, idInstanceWF);
        }

        /// <summary>  
        /// Méthode principale du process qui appelle les différents contrôles. 
        /// </summary>  
        public new void ExecuteMainProcess()
        {
            List<IControle> listControl = ListControls.listOfMockControl;
            string GlobalResult = ParamAppli.StatutOk;
            sRapport = string.Empty;
            RapportAnalyseImpact = new RapportProcessAnalyseImpact();
            RapportProcess.Name = this.LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            int idInstanceWF = this.ID_INSTANCEWF;
            RapportAnalyseLogique rapportAnalyseLogique = new RapportAnalyseLogique();
            RapportAnalyseData rapportAnalyseData = new RapportAnalyseData();
            RapportElementLocaliser rapportElementLocaliser = new RapportElementLocaliser();

            //On génère les historic au début pour mettre en inprogress
            //DEVGenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress);

            //Lancement analyse d'impact RamDl
            RamdlTool ramdlTool = new RamdlTool(CLIENT_ID, Decimal.ToInt32(WORKFLOW_ID), idInstanceWF);
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
                    commandData.CmdCode = removeCommentOnCommand(commandData.CmdCode);
                    string[] listLineRequest = splitCmdCodeData(commandData.CmdCode);

                }


            
            }

            this.addRapportAnalyseLogique(rapportAnalyseLogique, resultFileList);
            RapportAnalyseImpact.rapportAnalyseLogique = rapportAnalyseLogique;
            RapportAnalyseImpact.rapportAnalyseData = rapportAnalyseData;
            RapportAnalyseImpact.rapportElementLocaliser = rapportElementLocaliser;

            

            //RapportProcess.rapportAnalyseData = rapportAnalyseData;

            //Elements à localiser


            RapportAnalyseImpact.Fin = DateTime.Now;
            RapportAnalyseImpact.Result = ParamAppli.TranscoSatut[GlobalResult];

            //On fait un update pour la date de fin du process et son statut
            //DEVGenerateHistoric(RapportProcess.Fin, GlobalResult);


            /*DEVif (GlobalResult == ParamAppli.StatutOk)
            {
                int NextProcess = RequestTool.GetNextProcess(WORKFLOW_ID, ParamAppli.ProcessAnalyseImpact);
                LauncherViaDIspatcher.LaunchProcess(NextProcess, decimal.ToInt32(this.WORKFLOW_ID), this.CLIENT_ID, idInstanceWF);
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
                typeNew.Tooltip = "Ceci contient la liste des élements nouveaux du pack";
                typeNew.listLineAnalyseLogique = new List<LineAnalyseLogique>();

                TypeAnalyseLogique typeModified = new TypeAnalyseLogique();
                typeModified.Name = "Modified";
                typeModified.Tooltip = "Ceci contient la liste des élements modifiés du pack";
                typeModified.listLineAnalyseLogique = new List<LineAnalyseLogique>();

                TypeAnalyseLogique typeDeleted = new TypeAnalyseLogique();
                typeDeleted.Name = "Deleted";
                typeDeleted.Tooltip = "Ceci contient la liste des élements supprimés du pack";
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


        private List<RmdCommandData> getAllDataCmd(String sConnection)
        {
            DataManagerAccess dataManager = new DataManagerAccess();
            string requete = "select ID_PACKAGE, ID_CLASS, ID_OBJECT, CMD_CODE from M4RDL_PACK_CMDS where ID_PACKAGE like '%_D'";
            if (TYPOLOGY != "Dédié") requete += " AND  CMD_ACTIVE = -1 "; // Hors dédié on ne prend que les commandes actives
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


        public string ReplaceID_ORGA(string sCommand, string sID_OrgaOrg, string sID_OrgaDest)
        {
            List<string> lID_Orga = new List<string>();
            lID_Orga.Add(sID_OrgaDest);
            return (ReplaceID_ORGA(sCommand, sID_OrgaOrg, lID_Orga));
        }


        public string ReplaceID_ORGA(string sCommand, string sID_OrgaOrg, List<string> sID_OrgaDest)
        {
            string sORGA_COPY = string.Empty;
            string sORGA_SCRIPT = string.Empty;
            bool bPremierElement = true;
            string sResultat = sCommand;

            foreach (string orga in sID_OrgaDest)
            {
                if (bPremierElement == true)
                {
                    bPremierElement = false;
                    sORGA_COPY = "'";
                    sORGA_SCRIPT = "'";
                }
                else
                {
                    sORGA_COPY += ",";
                    sORGA_SCRIPT += ",'";
                }
                sORGA_COPY += orga;
                sORGA_SCRIPT += orga + "'";
            }
            if (sORGA_COPY.Length > 0)
                sORGA_COPY += "'";

            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "(|\\s+),(|\\s+)'" + sID_OrgaOrg + "'", "," + sORGA_COPY, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "@id_orgas_dest(|\\s+)=(|\\s+)'" + sID_OrgaOrg + "'", "@id_orgas_dest = " + sORGA_COPY, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION(|\\s+)=(|\\s+)'" + sID_OrgaOrg + "'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION\\s+LIKE(|\\s+)'" + sID_OrgaOrg + "'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION\\s+IN(|\\s+)\\((|\\s+)'" + sID_OrgaOrg + "'(|\\s+)\\)", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return sResultat;
        }

        public string[] splitCmdCodeData(string cmdCode)
        {
            //We use an ugly text to replace \\\\ for come back to this after the split on \\
            string uglyString = "A?DJUEBISKJLERPDLZMLERJD?ZADLKJZDKD.§ZDI";
            cmdCode = cmdCode.Replace("\\\\", uglyString);
            string[] result = cmdCode.Split('\\');

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = result[i].Replace(uglyString, "\\\\");
            }
            return result;
        }

        public string removeCommentOnCommand(string cmdCode)
        {

            int indexOpenComment = cmdCode.IndexOf("/*");
            int indexCloseComment = cmdCode.IndexOf("*/");

            while (indexOpenComment != -1)
            {
                cmdCode = cmdCode.Remove(indexOpenComment, (indexCloseComment - indexOpenComment) + 2); //+2 to remove the / at the end of the comment
                indexOpenComment = cmdCode.IndexOf("/*");
                indexCloseComment = cmdCode.IndexOf("*/");
            }


            indexOpenComment = cmdCode.IndexOf("//");
            indexCloseComment = cmdCode.IndexOf(System.Environment.NewLine);
            while (indexOpenComment != -1)
            {
                cmdCode = cmdCode.Remove(indexOpenComment, (indexCloseComment - indexOpenComment) + 2); //+2 to remove the / at the end of the comment
                indexOpenComment = cmdCode.IndexOf("//");
                indexCloseComment = cmdCode.IndexOf(System.Environment.NewLine);
            }

            return cmdCode;
        }
    }
}
