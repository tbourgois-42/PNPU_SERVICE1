using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;


namespace PNPUCore.Rapport
{

    class TemplateReport
    {
        public String Id { set; get; }

        public String Name { set; get; }

        public String Tooltip { set; get; }

    }

    class RapportProcessAnalyseImpact : TemplateReport
    {
        public String Result { set; get; }

        public RapportAnalyseData rapportAnalyseData { set; get; }

        public RapportAnalyseLogique rapportAnalyseLogique { set; get; }

        public RapportElementLocaliser rapportElementLocaliser { set; get; }

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

            rapportAnalyseLogique.ToJSONRepresentation(jw, this.Id);

            rapportAnalyseData.ToJSONRepresentation(jw, this.Id);

            rapportElementLocaliser.ToJSONRepresentation(jw, this.Id);

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



    class RapportAnalyseImpactMDBData : TemplateReport
    {
        public List<CommandData> listCommandData { set; get; }

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

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (CommandData type in listCommandData)
            {
                type.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }



    class RapportAnalyseLogique : TemplateReport
    {
        public List<RapportAnalyseImpactMDBLogique> listRapportAnalyseImpactMDBLogique { set; get; }

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

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (RapportAnalyseImpactMDBLogique type in listRapportAnalyseImpactMDBLogique)
            {
                type.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
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


    class RapportAnalyseData : TemplateReport
    {
        public List<CommandData> listCommandData { set; get; }

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

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            if (listCommandData != null)
            {
                foreach (CommandData type in listCommandData)
                {
                    type.ToJSONRepresentation(jw, id);
                }
            }
            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class CommandData : TemplateReport
    {
        public String Result { get; set; }
        public List<ControleCommandData> listControleCommandData;
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

            jw.WritePropertyName("message");
            jw.WriteStartArray();
            foreach (ControleCommandData type in listControleCommandData)
            {
                type.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class ControleCommandData : TemplateReport
    {
        public String Result { get; set; }
        public string Message { get; set; }

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
            jw.WritePropertyName("message");
            jw.WriteValue(sCote + this.Message + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + this.Tooltip + sCote);
            jw.WriteEndObject();
        }
    }

    class RapportElementLocaliser : TemplateReport
    {
        public List<RapportAnalyseImpactMDBElementALocaliser> listRapportAnalyseImpactMDBElementALocaliser { set; get; }
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

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            {
                if (listRapportAnalyseImpactMDBElementALocaliser != null)
                {
                    foreach (RapportAnalyseImpactMDBElementALocaliser element in listRapportAnalyseImpactMDBElementALocaliser)
                    {
                        element.ToJSONRepresentation(jw, id);
                    }
                }
            }
            jw.WriteEndArray();
            jw.WriteEndObject();
        }

    }
    class RapportAnalyseImpactMDBElementALocaliser : TemplateReport
    {
        public List<ElementLocaliser> listElementLocaliser { set; get; }

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

            foreach (ElementLocaliser element in listElementLocaliser)
            {
                element.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }

    }
    class ElementLocaliser : TemplateReport
    {
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
