using HttpMultipartParser;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration. 
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage. 
    public class Service1 : IService1
    {
        private static NamedPipeClientStream npcsPipeClient;
        private static StreamString ssStreamString = null;
        private static StreamWriter sw = null;
        public string LaunchProcess(int ProcId, int workflowId, String clientId, int idInstanceWF)
        {
            if (npcsPipeClient == null)
            {
                npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE");
                npcsPipeClient.Connect();
            }

            /*if (ssStreamString == null)
                ssStreamString = new StreamString(npcsPipeClient);
            ssStreamString.WriteString(ProcId + "/" + workflowId + "/" + clientId);*/
            if (sw == null)
                sw = new StreamWriter(npcsPipeClient);


            sw.AutoFlush = true;
            Console.WriteLine("TEST WRITE PIPE : " + ProcId + "/" + workflowId + "/" + clientId + "/" + idInstanceWF);
            sw.WriteLine(ProcId + "/" + workflowId + "/" + clientId + "/" + idInstanceWF);



            //string result = ssStreamString.ReadString();
            return "OK";
        }


        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public IEnumerable<InfoClientStep> GetInfoDashboardCard(string sHabilitation, string sUser)
        {
            return RequestTool.GetInfoDashboardCard(sHabilitation, sUser);
        }

        public IEnumerable<ToolboxInfoLaunch> GetInfoLaunchToolBox(string sHabilitation, string sUser)
        {
            return RequestTool.GetToolBoxInfoLaunch(sHabilitation, sUser);
        }

        public IEnumerable<PNPU_H_WORKFLOW> GetHWorkflow(string sHabilitation, string sUser, int isToolBox = -1)
        {
            return RequestTool.GetHWorkflow(sHabilitation, sUser, isToolBox);
        }

        public string GetInfoOneClient(string ClientName)
        {
            return RequestTool.GetInfoOneClient(ClientName);
        }

        public IEnumerable<PNPU_PROCESS> GetAllProcesses()
        {
            return RequestTool.GetAllProcesses();
        }

        public PNPU_PROCESS GetProcess(string processId)
        {
            return RequestTool.GetProcess(processId);
        }

        public IEnumerable<PNPU_WORKFLOW> GetAllWorkFLow(int isToolBox = -1)
        {
            return RequestTool.GetAllWorkFLow(isToolBox);
        }

        public PNPU_WORKFLOW GetWorkflow(string workflowId)
        {
            return RequestTool.GetWorkflow(workflowId);
        }

        public string GetProcessusCritiquesAllCLient()
        {
            throw new NotImplementedException();
        }

        public string GetProcessusCritiquesOneClient(string ClientName)
        {
            throw new NotImplementedException();
        }

        public string GetIdOrgaAllClient()
        {
            throw new NotImplementedException();
        }

        public string GetIdOrgaOneClient(string ClientName)
        {
            throw new NotImplementedException();
        }

        public string GetTypoAllClient()
        {
            throw new NotImplementedException();
        }

        public string GetTypoOneClient(string ClientName)
        {
            throw new NotImplementedException();
        }

        public string RunWorkflow(string WorkflowName)
        {
            if (npcsPipeClient == null)
            {
                npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE");
                npcsPipeClient.Connect();
            }

            if (ssStreamString == null)
                ssStreamString = new StreamString(npcsPipeClient);

            ssStreamString.WriteString(WorkflowName);

            string result = (ssStreamString.ReadString());
            return result;
        }

        public string CreateWorkflow(PNPU_WORKFLOW input)
        {
            return RequestTool.CreateWorkflow(input);
        }

        public IEnumerable<PNPU_WORKFLOWPROCESSES> GetWorkflowProcesses(string workflowId)
        {
            return RequestTool.GetWorkflowProcesses(workflowId);
        }

        public void PreflightRequest()
        {
            // Method intentionally left empty.
        }

        public void UploadFile(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);

            string typology = parser.GetParameterValue("typology"); //separate by , 
            string clients = parser.GetParameterValue("clients"); // separate by ,
            bool standard = bool.Parse(parser.GetParameterValue("packStandard")); //string "true" "false"
            string instanceName = parser.GetParameterValue("instanceName");
            int workflowId = int.Parse(parser.GetParameterValue("workflowID"));

            // Files are stored in a list:
            FilePart file = parser.Files.First();
            string FileName = file.FileName;

            //EST CE QUE LE DOSSIER TEMP EXISTE
            if (!Directory.Exists(ParamAppli.DossierTemporaire))
                Directory.CreateDirectory(ParamAppli.DossierTemporaire);

            string FilePath = Path.Combine(ParamAppli.DossierTemporaire, FileName);

            using (FileStream fileStream = File.Create(FilePath))
            {
                file.Data.Seek(0, SeekOrigin.Begin);
                file.Data.CopyTo(fileStream);
            }

            string clientToLaunch = "";

            //Bring all client
            if ((String.IsNullOrEmpty(clients) || clients == "undefined") && !String.IsNullOrEmpty(typology))
            {
                //REQUETE
                int typologyId = Int32.Parse(typology);
                IEnumerable<InfoClient> listClient = RequestTool.GetClientsWithTypologies(typologyId);
                foreach (InfoClient client in listClient)
                {
                    clientToLaunch = clientToLaunch + client.ID_CLIENT + ",";
                }
                clientToLaunch = clientToLaunch.Remove(clientToLaunch.Length - 1);
            }
            else if (!String.IsNullOrEmpty(clients) && !String.IsNullOrEmpty(typology))
            {
                //Récupérer les clients du front
                clientToLaunch = clients;
            }
            else
            {
                //GENERATE EXCEPTION
                //TODO generate specific Exception
                throw new Exception();
            }

            // We generate instance of workflow in PNPU_H_WORKFLOW
            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW
            {
                WORKFLOW_ID = workflowId,
                CLIENT_ID = clientToLaunch,
                LAUNCHING_DATE = DateTime.Now,
                ENDING_DATE = new DateTime(1800, 1, 1),
                STATUT_GLOBAL = ParamAppli.StatutInProgress,
                INSTANCE_NAME = instanceName
            };

            int idInstanceWF = int.Parse(RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow));

            GereMDBDansBDD gestionMDBdansBDD = new GereMDBDansBDD();
            // Add zip into database
            gestionMDBdansBDD.AjouteZipBDD(FilePath, workflowId, ParamAppli.ConnectionStringBaseAppli, idInstanceWF);

            // By Default we allways first launch ProcessControlePacks
            LaunchProcess(ParamAppli.ProcessControlePacks, workflowId, clientToLaunch, idInstanceWF);

            //Test MDU launch ProcessTNR directly
            //LaunchProcess(ParamAppli.ProcessTNR, workflowId, clientToLaunch);

            // Delete file
            File.Delete(FilePath);

        }

        public string ModifyWorkflow(PNPU_WORKFLOW input, string workflowID)
        {
            return RequestTool.ModifyWorkflow(input, workflowID);
        }

        public string DeleteWorkflow(string workflowID)
        {
            return RequestTool.DeleteWorkflow(workflowID);
        }

        public string DeleteProcess(string processID)
        {
            return RequestTool.DeleteProcess(processID);
        }

        public string CreateProcess(PNPU_PROCESS input)
        {
            return RequestTool.CreateProcess(input);
        }

        public string ModifyProcessus(PNPU_PROCESS input, string processID)
        {
            return RequestTool.ModifyProcessus(input, processID);
        }

        public string AffectWorkflowsProcesses(PNPU_STEP input, string workflowID)
        {
            return RequestTool.AffectWorkflowsProcesses(input, workflowID);
        }

        public IEnumerable<PNPU_H_REPORT> GetReport(string idProcess, string workflowId, string clientId, string idInstanceWF)
        {
            return RequestTool.GetReport(decimal.Parse(idProcess), decimal.Parse(workflowId), clientId, int.Parse(idInstanceWF));
        }

        public IEnumerable<InfoClient> GetListClientsByTypo(string TypologyId)
        {
            return RequestTool.GetClientsWithTypologies(Int32.Parse(TypologyId));
        }

        public IEnumerable<InfoClient> GetListClients()
        {
            return RequestTool.GetClientsWithTypologies();
        }

        public string GetMaxStep(string workflowID)
        {
            return RequestTool.GetMaxStep(int.Parse(workflowID));
        }

        public string GetNbLocalisation(string workflowId, string idInstanceWF, string clientId)
        {
            return RequestTool.GetNbLocalisation(int.Parse(workflowId), int.Parse(idInstanceWF), int.Parse(clientId));
        }

        /// <summary>
        /// Get one compressed file with all .mdb file.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="idInstanceWF"></param>
        /// <param name="clientId"></param>
        /// <returns>Return Stream wich contains a zip file.</returns>
        public Stream GetMdbLivraison(string workflowId, string idInstanceWF, string clientId)
        {
            MemoryStream stream = new MemoryStream();

            WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";

            List<byte[]> mdb = RequestTool.GetMdbLivraison(workflowId, idInstanceWF, clientId);
            string sDossierTempo = string.Empty;

            foreach (byte[] fichier in mdb)
            {
                string sNom = "0000000000" + workflowId;
                if (clientId != "")
                    sNom += "_C" + clientId + "_N0";
                sDossierTempo = ParamAppli.DossierTemporaire + "\\" + sNom;
                string sFichierZip = sDossierTempo + "\\" + sNom + ".ZIP";
                if (!Directory.Exists(sDossierTempo))
                    Directory.CreateDirectory(sDossierTempo);

                File.WriteAllBytes(sFichierZip, fichier);
                PNPUTools.ZIP.ManageZip.DecompresseDansDossier(sFichierZip, sDossierTempo);

                File.Delete(sFichierZip);
            }
            string zipName = sDossierTempo + "\\" + "PNPU_WF_" + workflowId + "_" + idInstanceWF + "_C" + clientId + ".ZIP";
            PNPUTools.ZIP.ManageZip.CompresseDossier(sDossierTempo, zipName);

            byte[] zipLivraison = File.ReadAllBytes(zipName);

            if (zipLivraison != null)
            {
                stream.Write(zipLivraison, 0, zipLivraison.Length);
                stream.Position = 0;
            }
            else
            {
                // Return HTTP Code 204 - No Content
                throw new WebFaultException(HttpStatusCode.NoContent);
            }

            return stream;
        }

        public int GetNbAvailablePack(string workflowId, string idInstanceWF, string clientId)
        {
            string sRequest = "SELECT COUNT(MDB) FROM PNPU_MDB WHERE WORKFLOW_ID = " + workflowId + " AND ID_H_WORKFLOW = " + idInstanceWF + " AND CLIENT_ID IN ('" + clientId + "')";
            int nbAvailablePack = int.Parse(DataManagerSQLServer.SelectCount(sRequest, ParamAppli.ConnectionStringBaseAppli));

            return nbAvailablePack;
        }

        public string AuthUser(Stream stream)
        {
            string sToken = Authentification.AuthUser(stream);

            if (sToken == string.Empty)
            {
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }

            return sToken;

        }

        public string ConnectUser(string sToken)
        {
            return Authentification.ConnectUser(sToken);
        }

        public string SignOutUser(Stream stream)
        {
            return Authentification.SignOutUser(stream) ? "Déconnection effectué avec succès" : throw new WebFaultException(HttpStatusCode.BadRequest);
        }

        public string GetHabilitation(string user, string token)
        {
            return Authentification.GetHabilitation(user, token);
        }

        public IEnumerable<InfoClient> GetListClientsHabilitation(string user, string habilitation)
        {
            return Authentification.GetListClient(habilitation, user);
        }

        public IEnumerable<InfoClientStep> GetInfoDashboardCardByWorkflow(string sHabilitation, string sUser, string workflowID, string idInstanceWF)
        {
            return RequestTool.GetInfoDashboardCardByWorkflow(sHabilitation, sUser, decimal.Parse(workflowID), decimal.Parse(idInstanceWF));
        }

        public string LaunchToolBoxProcess(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);
            string clientId = parser.GetParameterValue("clientID");
            int workflowId = int.Parse(parser.GetParameterValue("workflowID"));
            
            string result = "";
            string sRequest = "SELECT ID_PROCESS FROM PNPU_STEP PS INNER JOIN PNPU_WORKFLOW PHW ON PHW.WORKFLOW_ID = PS.WORKFLOW_ID  WHERE PHW.WORKFLOW_ID = " + workflowId + " AND PS.ORDER_ID = 0 AND PHW.IS_TOOLBOX = 1";
            bool hadFile = parser.Files.Count > 0;
            string FilePath = "";


            if (hadFile)
            {
                // Files are stored in a list:
                FilePart file = parser.Files.First();
                string FileName = file.FileName;

                //EST CE QUE LE DOSSIER TEMP EXISTE
                if (!Directory.Exists(ParamAppli.DossierTemporaire))
                    Directory.CreateDirectory(ParamAppli.DossierTemporaire);

                FilePath = Path.Combine(ParamAppli.DossierTemporaire, FileName);

                using (FileStream fileStream = File.Create(FilePath))
                {
                    file.Data.Seek(0, SeekOrigin.Begin);
                    file.Data.CopyTo(fileStream);
                }
            }

            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                DataRow drRow = dsDataSet.Tables[0].Rows[0];
                // We generate instance of workflow in PNPU_H_WORKFLOW 
                PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW
                {
                    WORKFLOW_ID = workflowId,
                    CLIENT_ID = clientId,
                    LAUNCHING_DATE = DateTime.Now,
                    ENDING_DATE = new DateTime(1800, 1, 1),
                    STATUT_GLOBAL = ParamAppli.StatutInProgress,
                    INSTANCE_NAME = "Toolbox Workflow #" + workflowId
                };

                int idInstanceWF = int.Parse(RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow));

                if (hadFile)
                {
                    GereMDBDansBDD gestionMDBdansBDD = new GereMDBDansBDD();
                    // Add zip into database
                    gestionMDBdansBDD.AjouteZipBDD(FilePath, workflowId, ParamAppli.ConnectionStringBaseAppli, idInstanceWF);
                }

                try
                {
                    ParamToolbox paramToolbox = new ParamToolbox();
                    result = paramToolbox.SaveParamsToolbox(parser, idInstanceWF);

                    if (result != "Requête traité avec succès")
                    {
                        //TODO Suppresion historic workflow
                        //TODO LOG
                        throw new WebFaultException(HttpStatusCode.BadRequest);
                    }

                    LaunchProcess(int.Parse(drRow[0].ToString()), workflowId, clientId.ToString(), idInstanceWF);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }

        public IEnumerable<InfoClient> GetInfoDashboardToolbox(string workflowId, string idInstanceWF)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PNPU_WORKFLOW> GetToolboxWorkflow(int isToolBox)
        {
            return RequestTool.GetAllWorkFLow(isToolBox);
        }
    }


    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = ushort.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

}