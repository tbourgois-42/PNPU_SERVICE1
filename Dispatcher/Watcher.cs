using PNPUCore;
using PNPUTools;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace PNUDispatcher
{
    /// <summary>  
    /// Cette classe sert à communiquer avec le webservice via un PIPE nommé.  
    /// </summary>  
    public class Watcher
    {
        private static readonly NamedPipeServerStream[] npssPipeClient = new NamedPipeServerStream[2];
        private static readonly StreamString[] ssStreamString = new StreamString[2];
        private static readonly Thread tListen = new Thread(() => ListenRequest("PNPU_PIPE", 0));
        private static readonly Thread tListen2 = null;
        private static readonly Thread tLaunchQueue = new Thread(() => LaunchQueue());

        /// <summary>  
        /// Lance juste la fonction ListenRequest dans un thread.  
        /// </summary>  
        public void launchWatcher()
        {
            tLaunchQueue.Start();
            tListen.Start();
        }

        /// <summary>  
        /// Destructeur de la classe. Ferme le PIPE nommé s'il est ouvert.  
        /// </summary> 
        ~Watcher()
        {
            if (npssPipeClient[0] != null)
            {
                npssPipeClient[0].Close();
            }

            if (npssPipeClient[1] != null)
            {
                npssPipeClient[1].Close();
            }
        }

        /// <summary>  
        /// Fonction d'écoute lancée dans un thread. Elle attend les messages envoyés par le webservice via le PIPE et lui renvoie un résultat.
        /// </summary> 
        private static void ListenRequest(string sNomPipe, int iNum)
        {
            string sMessage;
            npssPipeClient[iNum] = new NamedPipeServerStream(sNomPipe);

            ssStreamString[iNum] = new StreamString(npssPipeClient[iNum]);
            npssPipeClient[iNum].WaitForConnection();
            StreamReader sr = new StreamReader(npssPipeClient[iNum]);

            while (true)
            {

                //TEST TBO sMessage = ssStreamString[iNum].ReadString();

                sMessage = sr.ReadLine();

                Console.WriteLine("TEST LECTURE STRING : " + sMessage);

                if (!IsValideJSON(sMessage))
                {
                    //TODO add exception
                }
                else
                {
                    ParamAppli.qFIFO.Enqueue(sMessage);
                }
                /*string[] listParam = sMessage.Split('/');

                LaunchProcess(listParam[0], int.Parse(listParam[1]), listParam[2]);*/

                //ssStreamString[iNum].WriteString(sMessageResultat);
                //npssPipeClient[iNum].Flush();
            }
        }

        private static void LaunchProcess(string listclientId, int workflowId, int process, int idInstanceWF)
        {
            Thread thread = new Thread(() => LaunchProcessFunction(listclientId, workflowId, process, idInstanceWF));

            //Thread thread = new Thread(new ThreadStart(LaunchProcessFunction));
            thread.Start();
        }


        private static void LaunchProcessFunction(string listclientId, int workflowId, int process, int idInstanceWF)
        {
            var launcher = new Launcher();
            launcher.Launch(listclientId, workflowId, process, idInstanceWF);
        }


        private static void LaunchQueue()
        {
            while (true)
            {
                if (ParamAppli.qFIFO.Count > 0)
                {
                    string requestInit = ParamAppli.qFIFO.Dequeue();
                    //TODO Gestion d'exception dans le cas où requetsInit est null (Cas possible)
                    //TODO En cas d'exception loggé ce qui a tenté d'être lancé
                    Console.WriteLine(requestInit);
                    //Gestion des fin de fichiers
                    string request = requestInit.Replace("\0", "");
                    string[] listParam = request.Split('/');

                    if (listParam.Length == 4)
                    {
                        LaunchProcess(listParam[2], int.Parse(listParam[1]), int.Parse(listParam[0]), int.Parse(listParam[3]));
                    }
                    else
                    {

                        Console.WriteLine("ERREUR - " + requestInit + " Cannot be manage for launch process");
                    }

                    if (ParamAppli.qFIFO.Count == 0)
                    {
                        System.Threading.Thread.Sleep(200);
                    }
                }
            }
        }


        /// <summary>  
        /// Fonction de controle de validité du JSON.
        /// </summary> 
        private static bool IsValideJSON(string sJSON)
        {
            if (sJSON == string.Empty)
            {
                return false;
            }

            return true;
        }

    }



    public class StreamString
    {
        readonly private Stream ioStream;
        readonly private UnicodeEncoding streamEncoding;

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
