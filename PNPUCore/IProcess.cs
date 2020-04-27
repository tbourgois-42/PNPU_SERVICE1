using System;
using System.Data;
using System.Data.SqlClient;
using PNPUCore.Rapport;
using PNPUTools;
using PNPUTools.DataManager;

namespace PNPUCore.Process
{
    public interface IProcess
    {

        void ExecuteMainProcess();

        String FormatReport();
        void AjouteRapport(string v);
        bool SaveReportInBDD(string json, IProcess process);
        decimal WORKFLOW_ID { get; set; }
        string CLIENT_ID { get; set; }

    }

    class ProcessCore : IProcess
    {
        public decimal WORKFLOW_ID { get; set; }
        public string CLIENT_ID { get; set; }

        public string sRapport;
        public RProcess RapportProcess;
        public RControle RapportControleCourant;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>

        public ProcessCore(decimal wORKFLOW_ID, string cLIENT_ID)
        {
            RapportProcess = new RProcess();
            WORKFLOW_ID = wORKFLOW_ID;
            CLIENT_ID = cLIENT_ID;
        }

        private void GenerateHistoric()
        {
            //récupérer la liste des clients concernés

            //Boucler sur la liste des clients
            //Générer le PNPU_H_WORKFLOW
            //Générer la ligne PNPU_H_STEP
        }

        /// <summary>  
        /// Méthode appelée par les contrôle pour ajouter un message dans le rapport d'exécution du process.
        /// <param name="sMessage">Message à ajouter dans le rapport d'exécution du process.</param>
        /// </summary>  
        public void AjouteRapport(string sMessage)
        {
            RapportControleCourant.Message.Add(sMessage);
        }

        /// <summary>  
        /// Méthode appelée par le launcher pour créé le process. 
        /// </summary>  
        /// <param name="rapportProcess">Objet permettant de générer le rapport au format JSON sur le résultat du déroulement des contrôles.</param>
        /// <returns>Retourne l'instance du process créé.</returns>
        internal static IProcess CreateProcess(decimal WORKFLOW_ID, string CLIENT_ID)
        {
            return new ProcessCore(WORKFLOW_ID, CLIENT_ID);
        }

        public void ExecuteMainProcess()
        {
            throw new NotImplementedException();
        }

        /// <summary>  
        /// Méthode générant le rapport du déroulement du process au format JSON.
        /// <returns>Retourne le rapport au format JSON dans une chaine de caractères.</returns>
        /// </summary>  
        public string FormatReport()
        {
            RapportProcess.Fin = DateTime.Now;
            return (RapportProcess.ToJSONRepresentation());
        }

        public bool SaveReportInBDD(string json, IProcess process)
        {
            string processName = (RapportProcess.Id).Split('.')[2];
            string processId = "";

            if (processName == "ProcessControlePacks")
            {
                processId = ParamAppli.ProcessControlePacks;
            } else if (processName == "ProcessInit")
            {
                processId = ParamAppli.ProcessInit;
            } else if (processName == "ProcessGestionDependance")
            {
                processId = ParamAppli.ProcessGestionDependance;
            } else if (processName == "ProcessAnalyseImpact")
            {
                processId = ParamAppli.ProcessAnalyseImpact;
            } else if (processName == "ProcessIntegration")
            {
                processId = ParamAppli.ProcessIntegration;
            } else if (processName == "ProcessProcessusCritique")
            {
                processId = ParamAppli.ProcessProcessusCritique;
            } else if (processName == "ProcessTNR")
            {
                processId = ParamAppli.ProcessTNR;
            } else if (processName == "ProcessLivraison")
            {
                processId = ParamAppli.ProcessLivraison;
            }

            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_H_REPORT (ID_ORGANIZATION, ITERATION, WORKFLOW_ID, ID_PROCESS, CLIENT_ID, JSON_TEMPLATE, CLIENT_ID1) VALUES('0000', @ITERATION, @WORKFLOW_ID, @ID_PROCESS, @CLIENT_ID, @JSON_TEMPLATE, @CLIENT_ID1)", conn))
                    {
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int, 10).Value = 1;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int, 15).Value = process.WORKFLOW_ID;
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.Int, 15).Value = processId;
                        cmd.Parameters.Add("@CLIENT_ID", SqlDbType.VarChar, 64).Value = process.CLIENT_ID;
                        cmd.Parameters.Add("@JSON_TEMPLATE", SqlDbType.Text).Value = json.Replace("\r\n", "");
                        cmd.Parameters.Add("@CLIENT_ID1", SqlDbType.VarChar, 64).Value = process.CLIENT_ID;
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    return false;
                }
                return true;
            }
        }
    }
}