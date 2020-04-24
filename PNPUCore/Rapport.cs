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
        private List<Source> source;
        private DateTime debut;
        private DateTime fin;

        public String ToJSONRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonTextWriter(new StringWriter(sb));

            jw.Formatting = Formatting.Indented;
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(this.Id);
            jw.WritePropertyName("debut");
            jw.WriteValue(this.Debut.ToString("dd/MM/yy H:mm:ss"));
            jw.WritePropertyName("fin");
            jw.WriteValue(this.Fin.ToString("dd/MM/yy H:mm:ss"));

            jw.WritePropertyName("source");
            jw.WriteStartArray();

            for (int i = 0; i < Source.Count; i++)
            {
                jw.WriteStartObject();
                jw.WritePropertyName("id");
                jw.WriteValue(Source[i].Id);
                
                jw.WritePropertyName("controle");
                jw.WriteStartArray();
                for (int j = 0; j < Source[i].Controle.Count; j++)
                {
                    jw.WriteStartObject();
                    jw.WritePropertyName("id");
                    jw.WriteValue(Source[i].Controle[j].Id);
                    jw.WritePropertyName("result");
                    jw.WriteValue(Source[i].Controle[j].Result);

                    jw.WritePropertyName("message");
                    jw.WriteStartArray();
                    for (int k = 0; k < Source[i].Controle[j].Message.Count; k++)
                    {
                        jw.WriteStartObject();
                        jw.WritePropertyName("message");
                        jw.WriteValue(Source[i].Controle[j].Message[k]);
                        jw.WriteEndObject();
                    }
                    jw.WriteEndArray();
                    jw.WriteEndObject();
                }
                jw.WriteEndArray();
                jw.WriteEndObject();
            }

            jw.WriteEndArray();

            jw.WriteEndObject();

            return sb.ToString();
        }


        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
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
    }

    class Source
    {
        private string id;
        private List<RControle> controle;

        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public List<RControle> Controle
        {
            set { this.controle = value; }
            get { return this.controle; }
        }
    }

    class RControle
    {
        private string id;
        private bool result { get; set; }
        private List<string> message { get; set; }
    
        public String Id
        {
            set { this.id = value; }
            get { return this.id; }
        }

        public Boolean Result
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
