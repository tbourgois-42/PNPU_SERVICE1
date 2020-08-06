using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


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
            set { id = value; }
            get { return id; }
        }

        public List<RProcess> Process
        {
            set { process = value; }
            get { return process; }
        }

        public DateTime Debut
        {
            set { debut = value; }
            get { return debut; }
        }

        public DateTime Fin
        {
            set { fin = value; }
            get { return fin; }
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

            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue("1");
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + name + sCote);
            /*jw.WritePropertyName("id-client"); 
            jw.WriteValue(this.IdClient);*/
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + result + sCote);
            jw.WritePropertyName("debut");
            jw.WriteValue(sCote + Debut.ToString("dd/MM/yy H:mm:ss") + sCote);
            jw.WritePropertyName("fin");
            jw.WriteValue(sCote + Fin.ToString("dd/MM/yy H:mm:ss") + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            for (int i = 0; i < Source.Count; i++)
            {
                string sIDSource;
                sIDSource = (i + 2).ToString(DetermineFormat(Source.Count + 1));
                // Cas particulier des dépendances 
                if (Source[i].Name == "Contrôle des dépendances du livrable")
                {
                    rapportDependancesInterPack.ToJSONRepresentation(jw, "3", 0);
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

            return sb.ToString();
        }

        public String Id
        {
            set { id = value; }
            get { return id; }
        }

        public String Name
        {
            set { name = value; }
            get { return name; }
        }
        public List<Source> Source
        {
            set { source = value; }
            get { return source; }
        }

        public DateTime Debut
        {
            set { debut = value; }
            get { return debut; }
        }

        public DateTime Fin
        {
            set { fin = value; }
            get { return fin; }
        }

        public String Result
        {
            set { result = value; }
            get { return result; }
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
            set { id = value; }
            get { return id; }
        }

        public String Name
        {
            set { name = value; }
            get { return name; }
        }

        public List<RControle> Controle
        {
            set { controle = value; }
            get { return controle; }
        }

        public string Result
        {
            set { result = value; }
            get { return result; }
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
            set { id = value; }
            get { return id; }
        }

        public String Name
        {
            set { name = value; }
            get { return name; }
        }

        public String Tooltip
        {
            set { tooltip = value; }
            get { return tooltip; }
        }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }

        public List<string> Message
        {
            set { message = value; }
            get { return message; }
        }
    }

}
