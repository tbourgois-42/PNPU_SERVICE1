using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        NamedPipeClientStream npcsPipeClient = null;
        StreamString ssStreamString = null;


        public Form1()
        {
            InitializeComponent();
            npcsPipeClient = new NamedPipeClientStream("PNPU_PIPE");
            npcsPipeClient.Connect();
            ssStreamString = new StreamString(npcsPipeClient);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
            string returnString;

            returnString = client.GetData(textBox1.Text);
            label1.Text = returnString;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
            string returnString;

            returnString = client.LaunchProcess(textBox1.Text);
            label1.Text = returnString;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ServiceReference1.Service1Client client = new ServiceReference1.Service1Client();
            string returnString;
            string sJSON = "{ \"menu\": {\n";
            sJSON += "   \"id\": \"file\",\n";
            sJSON += "   \"value\": \"File\",\n";
            sJSON += "   \"popup\": {\n";
            sJSON += "   \"menuitem\": [\n";
            sJSON += "   {\"value\": \"New\", \"onclick\": \"CreateNewDoc()\"},\n";
            sJSON += "   {\"value\": \"Open\", \"onclick\": \"OpenDoc()\"},\n";
            sJSON += "   {\"value\": \"Close\", \"onclick\": \"CloseDoc()\"}\n";
            sJSON += "   ]\n";
            sJSON += "   }\n";
            sJSON += "   }\n";
            sJSON += "   }\n";

            returnString = client.LaunchProcess(sJSON);
            label1.Text = returnString;
        }

        // Test de lancement du process de controle des packs
        private void bntCtrlPack_Click(object sender, EventArgs e)
        {
            ssStreamString.WriteString("Test");
            label1.Text = ssStreamString.ReadString();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();

            //gereMDBDansBDD.AjouteFichiersMDBBDD(Directory.GetFiles("D:\\PNPU\\TEST", "*.mdb"), 12345, "D:\\PNPU\\TEMPO", "server=M4FRSQL13;uid=SAASSN306;pwd=SAASSN306;database=SAASSN306;");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string[] sFichiers=null;
            string sResultat = string.Empty;

            PNPUTools.GereMDBDansBDD gereMDBDansBDD = new PNPUTools.GereMDBDansBDD();

            //gereMDBDansBDD.ExtraitFichiersMDBBDD(ref sFichiers, 12345, "D:\\PNPU\\TEMPO", "server=M4FRSQL13;uid=SAASSN306;pwd=SAASSN306;database=SAASSN306;");
            foreach (string sNom in sFichiers)
                sResultat += "\n" + sNom;

            MessageBox.Show(sResultat);
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
