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
            RapportProcess.Name = this.LibProcess;
            RapportProcess.Debut = DateTime.Now;
            RapportProcess.IdClient = CLIENT_ID;
            RapportProcess.Source = new List<Rapport.Source>();

            //On génère les historic au début pour mettre en inprogress
            GenerateHistoric(new DateTime(1800, 1, 1), ParamAppli.StatutInProgress);


            Rapport.Source RapportSource = new Rapport.Source();
            RapportSource.Name = "IdRapport - ProcessAnalyseImpact DEV";
            RapportSource.Controle = new List<RControle>();

            RapportProcess.Source.Add(RapportSource);
            RapportProcess.Fin = DateTime.Now;
            RapportProcess.Result = ParamAppli.TranscoSatut[GlobalResult];

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
    }
}