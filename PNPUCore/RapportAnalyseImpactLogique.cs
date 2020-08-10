using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNPUCore
{


    class RapportAnalyseLogique : TemplateReport
    {
        public List<RapportAnalyseImpactMDBLogique> listRapportAnalyseImpactMDBLogique { set; get; }

        public String Result { set; get; }
        public String IdClient { set; get; }
        public DateTime Debut { set; get; }

        public DateTime Fin { set; get; }

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

            Id = "1";
            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("idClient");
            jw.WriteValue(sCote + IdClient + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + Result + sCote);
            jw.WritePropertyName("debut");
            jw.WriteValue(sCote + Debut.ToString("dd/MM/yy H:mm:ss") + sCote);
            jw.WritePropertyName("fin");
            jw.WriteValue(sCote + Fin.ToString("dd/MM/yy H:mm:ss") + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            int i = 0;
            foreach (RapportAnalyseImpactMDBLogique type in listRapportAnalyseImpactMDBLogique)
            {
                type.ToJSONRepresentation(jw, Id, i);
                i++;
            }

            jw.WriteEndArray();
            jw.WriteEndObject();

            return sb.ToString();
        }
    }

    class RapportAnalyseImpactMDBLogique : TemplateReport
    {

        public List<TypeAnalyseLogique> listTypeAnalyseLogique { set; get; }

        public void ToJSONRepresentation(JsonWriter jw, string id, int increment)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + increment).ToString();

            jw.Formatting = Formatting.Indented;
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("children");
            jw.WriteStartArray();

            int i = 0;
            foreach (TypeAnalyseLogique type in listTypeAnalyseLogique)
            {
                type.ToJSONRepresentation(jw, id, i);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }

    }

    class TypeAnalyseLogique : TemplateReport
    {
        public List<LineAnalyseLogique> listLineAnalyseLogique { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int increment)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + increment).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

            jw.WritePropertyName("line");
            jw.WriteStartArray();

            int i = 0;
            foreach (LineAnalyseLogique line in listLineAnalyseLogique)
            {
                line.ToJSONRepresentation(jw, id, i);
                i++;
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class LineAnalyseLogique : TemplateReport
    {
        public String currentCode { set; get; }
        public String newCode { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int increment)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + increment).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);
            jw.WriteEndObject();
        }
    }

}
