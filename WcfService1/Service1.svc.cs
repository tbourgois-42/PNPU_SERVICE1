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

namespace WcfService1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration.
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage.
    public class Service1 : IService1
    {
        private static NamedPipeClientStream npcsPipeClient = null;
        private static StreamString ssStreamString = null;

        public string LaunchProcess(string ProcFile, int workflowId, String clientId)
        {
            if (npcsPipeClient == null)
            {
                npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE");
                npcsPipeClient.Connect();
                ssStreamString = new StreamString(npcsPipeClient);
            }

           
            ssStreamString.WriteString(ProcFile + "/" + workflowId + "/" + clientId);

            return (ssStreamString.ReadString());
        }


        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public string Alacon(string test)
        {
            return test;
        }

        public IEnumerable<InfoClientStep> GetInfoAllClient()
        {
            return RequestTool.GetAllInfoClient();
            //return RequestTool.GetAllStep();
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
                ssStreamString = new StreamString(npcsPipeClient);
            }

            ssStreamString.WriteString(WorkflowName);

            return (ssStreamString.ReadString());
        }

        public bool CreateWorkflow(PNPU_WORKFLOW input)
        {
            bool result = true;
           //DO something cool!

            return result;
        }

        public IEnumerable<PNPU_WORKFLOWPROCESSES> GetWorkflowProcesses(string workflowId)
        {
            return RequestTool.GetWorkflowProcesses(workflowId);
        }


        public string preflightRequest()
        {
            return "OK";
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
            LaunchProcess("ProcessControlePacks", workflowId, "ALL");

            //SUPPRESSION DU FICHIER
            File.Delete(FilePath);

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