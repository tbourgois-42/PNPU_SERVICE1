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

namespace WcfService1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration.
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage.
    public class Service1 : IService1
    {
        private static NamedPipeClientStream npcsPipeClient = null;
        private static StreamString ssStreamString = null;

        public string LaunchProcess(string ProcFile)
        {
            if (npcsPipeClient == null)
            {
                npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE");
                npcsPipeClient.Connect();
                ssStreamString = new StreamString(npcsPipeClient);
            }

            ssStreamString.WriteString(ProcFile);

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

        public string GetInfoAllClient()
        {
            return RequestTool.GetAllInfoClient();
        }

        public string GetInfoOneClient(string ClientName)
        {
            return RequestTool.GetInfoOneClient(ClientName);
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

        public bool CreateWorkflow(Stream input)
        {
            bool result = false;
            WorkFlow newWorkflow = new WorkFlow();

            /*using (StreamReader sr = new StreamReader(input))
            {
                NameValueCollection qs = HttpUtility.ParseQueryString(sr.ReadToEnd());
                newBooking.DateBooking = DateTime.Parse(qs["dateBooking"].ToString());
                newBooking.NbGuest = int.Parse(qs["nbGuest"].ToString());
            }

            if (newBooking != null && !MenuRepository.Bookings.Any(bo => bo.DateBooking.Equals(newBooking.DateBooking)))
            {
                result = true;
                MenuRepository.Bookings.Add(newBooking);
            }*/

            return result;
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