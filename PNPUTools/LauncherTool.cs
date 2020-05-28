using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNPUTools
{
    public class LauncherViaDIspatcher
    {
        private static StreamString ssStreamString = null;

        public static void LaunchProcess(int ProcFile, int workflowId, String clientId)
        {
            if (ProcFile == ParamAppli.ProcessFinished)
                return;

            if (ParamAppli.npcsPipeClient == null)
                ParamAppli.npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE2");

            if (ParamAppli.npcsPipeClient.IsConnected == false)
                ParamAppli.npcsPipeClient.Connect();
         

            if (ssStreamString == null)
                ssStreamString = new StreamString(ParamAppli.npcsPipeClient);
            ssStreamString.WriteString(ProcFile + "/" + workflowId + "/" + clientId);


            //string result = ssStreamString.ReadString();
            //return result;
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
