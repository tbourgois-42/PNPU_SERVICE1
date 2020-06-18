using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }
            this.Id = "1";
            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(this.Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + this.Name + sCote);
            jw.WritePropertyName("idClient");
            jw.WriteValue(sCote + this.IdClient + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + this.Result + sCote);
            jw.WritePropertyName("debut");
            jw.WriteValue(sCote + this.Debut.ToString("dd/MM/yy H:mm:ss") + sCote);
            jw.WritePropertyName("fin");
            jw.WriteValue(sCote + this.Fin.ToString("dd/MM/yy H:mm:ss") + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            foreach (RapportAnalyseImpactMDBLogique type in listRapportAnalyseImpactMDBLogique)
            {
                type.ToJSONRepresentation(jw, this.Id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sb = sb.Replace("\"", "");
                sb = sb.Replace(sCote2, "\"");
            }

            return sb.ToString();
        }
    }

    class RapportAnalyseImpactMDBLogique : TemplateReport
    {

        public List<TypeAnalyseLogique> listTypeAnalyseLogique { set; get; }

        public void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }
            this.Id = "1";
            jw.Formatting = Formatting.Indented;
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(this.Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + this.Name + sCote);
            jw.WritePropertyName("children");
            jw.WriteStartArray();

            foreach (TypeAnalyseLogique type in listTypeAnalyseLogique)
            {
                type.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }

    }

    class TypeAnalyseLogique : TemplateReport
    {
        public List<LineAnalyseLogique> listLineAnalyseLogique { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            this.Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(this.Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + this.Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + this.Tooltip + sCote);

            jw.WritePropertyName("line");
            jw.WriteStartArray();
            foreach (LineAnalyseLogique line in this.listLineAnalyseLogique)
            {
                line.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class LineAnalyseLogique : TemplateReport
    {
        public String currentCode { set; get; }
        public String newCode { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            this.Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(this.Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + this.Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + this.Tooltip + sCote);
            jw.WriteEndObject();
        }
    }

}
