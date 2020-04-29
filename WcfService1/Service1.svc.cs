using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.IO.Pipes;
using PNPUCore.Database;
using System.Collections.Specialized;
using System.Web;
using PNPUTools;
using PNPUTools.DataManager;
using System.Web.Hosting;
using AntsCode.Util;
using System.Configuration;

namespace WcfService1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration. 
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage. 
    public class Service1 : IService1
    {
        private static NamedPipeClientStream npcsPipeClient;
        private static StreamString ssStreamString = null;

        public string LaunchProcess(int ProcId, int workflowId, String clientId)
        {
            if (npcsPipeClient == null)
            {
                npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE");
                npcsPipeClient.Connect();
            }

            if (ssStreamString == null)
                ssStreamString = new StreamString(npcsPipeClient);
            ssStreamString.WriteString(ProcFile + "/" + workflowId + "/" + clientId);

            
            string result = ssStreamString.ReadString();
             return result;
        }


        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public string Alacon(string test)
        {
            return test;
        }

        public IEnumerable<InfoClientStep> GetInfoAllClient(string workflowId_)
        {
            int workflowId = int.Parse(workflowId_);
            return RequestTool.GetAllInfoClient(workflowId);
            //return RequestTool.GetAllStep(); 
        }
        public IEnumerable<PNPU_H_WORKFLOW> GetHWorkflow()
        {
            return RequestTool.GetHWorkflow();
        }

        public string GetInfoOneClient(string ClientName)
        {
            return RequestTool.GetInfoOneClient(ClientName);
        }

        public IEnumerable<PNPU_PROCESS> GetAllProcesses()
        {
            return RequestTool.GetAllProcesses();
        }

        public PNPU_PROCESS getProcess(string processId)
        {
            return RequestTool.GetProcess(processId);
        }

        public IEnumerable<PNPU_WORKFLOW> GetAllWorkFLow()
        {
            return RequestTool.GetAllWorkFLow();
        }

        public PNPU_WORKFLOW getWorkflow(string workflowId)
        {
            return RequestTool.getWorkflow(workflowId);
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

        public void preflightRequest()
        {
        }

        public void UploadFile(Stream stream, string workflowId_)
        {
            MultipartParser parser = new MultipartParser(stream);

            int workflowId = int.Parse(workflowId_);
            
            //EST CE QUE LE DOSSIER TEMP EXISTE
            if (Directory.Exists(ParamAppli.DossierTemporaire) == false)
                Directory.CreateDirectory(ParamAppli.DossierTemporaire);

            string FileName = parser.Filename;
            string FilePath = Path.Combine(ParamAppli.DossierTemporaire, FileName);

            File.WriteAllBytes(FilePath, parser.FileContents);

            GereMDBDansBDD gestionMDBdansBDD = new GereMDBDansBDD();
            //AJOUT DU ZIP DE MDB DANS LA BDD
            gestionMDBdansBDD.AjouteZipBDD(FilePath, workflowId, ParamAppli.ConnectionStringBaseAppli);

            //Launch process
            LaunchProcess(ParamAppli.ProcessControlePacks, workflowId, "ALL");

            //SUPPRESSION DU FICHIER
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

        public IEnumerable<PNPU_H_REPORT> getReport(string idProcess_, string workflowId_, string clientId)
        {
            decimal workflowId = decimal.Parse(workflowId_);
            decimal idProcess = decimal.Parse(idProcess_);
            return RequestTool.getReport(idProcess, workflowId, clientId);
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
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

}