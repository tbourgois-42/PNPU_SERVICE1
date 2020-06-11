using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using PNPUCore;

namespace PNUDispatcher
{
    /// <summary>  
    /// Cette classe sert à communiquer avec le webservice via un PIPE nommé.  
    /// </summary>  
    public class Watcher
    {
        private static NamedPipeServerStream[] npssPipeClient = null;
        private static StreamString[] ssStreamString = null;
        private static Thread tListen = null;
        private static Thread tListen2 = null;
        private static Thread tLaunchQueue = null;
        private static Queue<string> qFIFO = null;

        /// <summary>  
        /// Constructeur de la classe. Lance juste la fonction ListenRequest dans un thread.  
        /// </summary>  
        public Watcher()
        {
            npssPipeClient = new NamedPipeServerStream[2];
            ssStreamString = new StreamString[2];
            qFIFO = new Queue<string>();

            tLaunchQueue = new Thread(() => LaunchQueue());
            tLaunchQueue.Start();

            tListen = new Thread(() => ListenRequest("PNPU_PIPE",0));
            tListen.Start();
            tListen2 = new Thread(() => ListenRequest("PNPU_PIPE2",1));
            tListen2.Start();
        }

        /// <summary>  
        /// Destructeur de la classe. Ferme le PIPE nommé s'il est ouvert.  
        /// </summary> 
        ~Watcher()
        {
            if (npssPipeClient[0] != null)
                npssPipeClient[0].Close();
            if (npssPipeClient[1] != null)
                npssPipeClient[1].Close();

        }

        /// <summary>  
        /// Fonction d'écoute lancée dans un thread. Elle attend les messages envoyés par le webservice via le PIPE et lui renvoie un résultat.
        /// </summary> 
        private static void ListenRequest(string sNomPipe, int iNum)
        {
            string sMessage = string.Empty;
            string sMessageResultat = string.Empty;
            npssPipeClient[iNum] = new NamedPipeServerStream(sNomPipe);

            ssStreamString[iNum] = new StreamString(npssPipeClient[iNum]);
            npssPipeClient[iNum].WaitForConnection();
            StreamReader sr = new StreamReader(npssPipeClient[iNum]);

            while (true)
            {

                //TEST TBO sMessage = ssStreamString[iNum].ReadString();

                sMessage = sr.ReadLine();
                
                Console.WriteLine("TEST LECTURE STRING : " + sMessage);

                if (IsValideJSON(sMessage) == false)
                    sMessageResultat = "KO";
                else
                {
                    sMessageResultat = "OK";
                    qFIFO.Enqueue(sMessage);
                }
                /*string[] listParam = sMessage.Split('/');

                LaunchProcess(listParam[0], int.Parse(listParam[1]), listParam[2]);*/

                //ssStreamString[iNum].WriteString(sMessageResultat);
                //npssPipeClient[iNum].Flush();
             }
        }

        private static void LaunchProcess(string listclientId, int workflowId, int process)
        {
            Thread thread = new Thread(() => LaunchProcessFunction(listclientId, workflowId, process));

            //Thread thread = new Thread(new ThreadStart(LaunchProcessFunction));
            thread.Start();
        }


        private static void LaunchProcessFunction(string listclientId, int workflowId, int process)
        {
            var launcher = new Launcher();
            launcher.Launch(listclientId, workflowId, process);
        }


        private static void LaunchQueue()
        {
            while (true)
            {
                if (qFIFO.Count > 0)
                {
                    string requestInit = qFIFO.Dequeue();
                    //Gestion des fin de fichiers
                    string request = requestInit.Replace("\0", "");
                    string[] listParam = request.Split('/');

                    if (listParam.Length == 3)
                    {
                        LaunchProcess(listParam[2], int.Parse(listParam[1]), int.Parse(listParam[0]));
                    }
                    else
                    {

                        Console.WriteLine("ERREUR - " + requestInit + "Cannot be manage for launch process");
                    }

                    if (qFIFO.Count == 0)
                        System.Threading.Thread.Sleep(200);
                }
            }
        }


        /// <summary>  
        /// Fonction de controle de validité du JSON.
        /// </summary> 
        private static bool IsValideJSON(string sJSON)
        {
            if (sJSON == string.Empty)
                return false;

            return true;
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
