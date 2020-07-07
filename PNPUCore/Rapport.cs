using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;


namespace PNPUCore.Rapport
{
    class RTraitement
    {
        private string id;
        private List<RProcess> process;
        private DateTime debut;
        private DateTime fin;

        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public List<RProcess> Process
        {
            set { this.process = value; }
            get { return this.process; }
        }

        public DateTime Debut
        {
            set { this.debut = value; }
            get { return this.debut; }
        }

        public DateTime Fin
        {
            set { this.fin = value; }
            get { return this.fin; }
        }
    }

    class RProcess
    {
        private string id;
        private string name;
        private List<Source> source;
        private DateTime debut;
        private DateTime fin;
        private string result;
        public RapportDependancesInterPack rapportDependancesInterPack = new RapportDependancesInterPack();

        private string DetermineFormat(int iNombre)
        {
            string sFormat = "0";
            for (int i = 10; i <= iNombre; i *= 10)
                sFormat += "0";

            return sFormat;
        }

        public String ToJSONRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonTextWriter(new StringWriter(sb));


            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue("1");
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + this.name + sCote);
            /*jw.WritePropertyName("id-client"); 
            jw.WriteValue(this.IdClient);*/
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + this.result + sCote);
            jw.WritePropertyName("debut");
            jw.WriteValue(sCote + this.Debut.ToString("dd/MM/yy H:mm:ss") + sCote);
            jw.WritePropertyName("fin");
            jw.WriteValue(sCote + this.Fin.ToString("dd/MM/yy H:mm:ss") + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            for (int i = 0; i < Source.Count; i++)
            {
                string sIDSource;
                sIDSource = (i + 2).ToString(DetermineFormat(Source.Count + 1));
                // Cas particulier des dépendances 
                if (Source[i].Name == "Contrôle des dépendances du livrable")
                {
                    rapportDependancesInterPack.ToJSONRepresentation(jw,"3",0);
                }
                else
                {
                    jw.WriteStartObject();
                    jw.WritePropertyName("id");
                    jw.WriteValue(sIDSource);
                    jw.WritePropertyName("name");
                    jw.WriteValue(sCote + Source[i].Name + sCote);
                    jw.WritePropertyName("result");
                    jw.WriteValue(sCote + Source[i].Result + sCote);


                    if (Source[i].Controle.Count > 0)
                    {
                        jw.WritePropertyName("children");
                        jw.WriteStartArray();
                        for (int j = 0; j < Source[i].Controle.Count; j++)
                        {
                            string sIDControle;
                            sIDControle = sIDSource + (j + 1).ToString(DetermineFormat(Source[i].Controle.Count));
                            jw.WriteStartObject();
                            jw.WritePropertyName("id");
                            jw.WriteValue(sIDControle);
                            jw.WritePropertyName("name");
                            jw.WriteValue(sCote2 + Source[i].Controle[j].Name + sCote2);
                            jw.WritePropertyName("Tooltip");
                            jw.WriteValue(sCote2 + Source[i].Controle[j].Tooltip + sCote2);
                            jw.WritePropertyName("result");
                            jw.WriteValue(sCote + Source[i].Controle[j].Result + sCote);

                            if (Source[i].Controle[j].Message.Count > 0)
                            {

                                jw.WritePropertyName("message");
                                jw.WriteStartArray();
                                for (int k = 0; k < Source[i].Controle[j].Message.Count; k++)
                                {
                                    string sIDMessage;
                                    sIDMessage = sIDControle + (k + 1).ToString(DetermineFormat(Source[i].Controle[j].Message.Count));
                                    jw.WriteStartObject();
                                    jw.WritePropertyName("id");
                                    jw.WriteValue(sIDMessage);
                                    jw.WritePropertyName("name");
                                    jw.WriteValue(sCote + Source[i].Controle[j].Message[k] + sCote);
                                    jw.WriteEndObject();
                                }
                                jw.WriteEndArray();

                            }
                            jw.WriteEndObject();
                        }
                        jw.WriteEndArray();

                    }
                    jw.WriteEndObject();
                }

            }

            jw.WriteEndArray();

            jw.WriteEndObject();

            jw.WriteEndArray();

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sb = sb.Replace("\"", "");
                sb = sb.Replace(sCote2, "\"");
            }
            return sb.ToString();
        }

        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public String Name
        {
            set { this.name = value; }
            get { return this.name; }
        }
        public List<Source> Source
        {
            set { this.source = value; }
            get { return this.source; }
        }

        public DateTime Debut
        {
            set { this.debut = value; }
            get { return this.debut; }
        }

        public DateTime Fin
        {
            set { this.fin = value; }
            get { return this.fin; }
        }

        public String Result
        {
            set { this.result = value; }
            get { return this.result; }
        }

        public string IdClient { get; internal set; }

    }

    class Source
    {
        private string id;
        private string name;
        private string result { get; set; }
        private List<RControle> controle;

        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public String Name
        {
            set { this.name = value; }
            get { return this.name; }
        }

        public List<RControle> Controle
        {
            set { this.controle = value; }
            get { return this.controle; }
        }

        public string Result
        {
            set { this.result = value; }
            get { return this.result; }
        }

    }

    class RControle
    {
        private string id;
        private string name;
        private string tooltip { get; set; }
        private string result { get; set; }
        private List<string> message { get; set; }

        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public String Name
        {
            set { this.name = value; }
            get { return this.name; }
        }

        public String Tooltip
        {
            set { this.tooltip = value; }
            get { return this.tooltip; }
        }

        public string Result
        {
            set { this.result = value; }
            get { return this.result; }
        }

        public List<string> Message
        {
            set { this.message = value; }
            get { return this.message; }
        }
    }

}
