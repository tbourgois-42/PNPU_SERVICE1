using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace PNPUCore.Rapport
{
    internal class TemplateReport
    {
        public String Id { set; get; }

        public String Name { set; get; }

        public String Tooltip { set; get; }

    }

    internal class RapportProcessAnalyseImpact : TemplateReport
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
            StringBuilder sFormat = new StringBuilder();

            sFormat.Append("0");

            for (int i = 10; i <= iNombre; i *= 10)
            {
                sFormat.Append("0");
            }

            return sFormat.ToString();
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

            rapportAnalyseLogique.ToJSONRepresentation(jw, Id);

            rapportAnalyseData.ToJSONRepresentation(jw, Id);

            rapportElementLocaliser.ToJSONRepresentation(jw, Id);

            jw.WriteEndArray();
            jw.WriteEndObject();


            return sb.ToString();
        }
    }

    internal class RapportAnalyseImpactMDBData : TemplateReport
    {
        public List<CommandData> listCommandData { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;


            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

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

    internal class RapportAnalyseLogique : TemplateReport
    {
        public List<RapportAnalyseImpactMDBLogique> listRapportAnalyseImpactMDBLogique { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;


            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

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

    internal class RapportAnalyseImpactMDBLogique : TemplateReport
    {

        public List<TypeAnalyseLogique> listTypeAnalyseLogique { set; get; }

        public void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = "1";
            jw.Formatting = Formatting.Indented;
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
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

    internal class TypeAnalyseLogique : TemplateReport
    {
        public List<LineAnalyseLogique> listLineAnalyseLogique { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

            jw.WritePropertyName("line");
            jw.WriteStartArray();
            foreach (LineAnalyseLogique line in listLineAnalyseLogique)
            {
                line.ToJSONRepresentation(jw, id);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    internal class LineAnalyseLogique : TemplateReport
    {
        public String currentCode { set; get; }
        public String newCode { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

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

    internal class RapportAnalyseData : TemplateReport
    {
        public List<CommandData> listCommandData { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

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

    internal class CommandData : TemplateReport
    {
        public String Result { get; set; }
        public List<ControleCommandData> listControleCommandData;
        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

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

    internal class ControleCommandData : TemplateReport
    {
        public String Result { get; set; }
        public string Message { get; set; }

        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("message");
            jw.WriteValue(sCote + Message + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);
            jw.WriteEndObject();
        }
    }

    internal class RapportElementLocaliser : TemplateReport
    {
        public List<RapportAnalyseImpactMDBElementALocaliser> listRapportAnalyseImpactMDBElementALocaliser { set; get; }
        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

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

    internal class RapportAnalyseImpactMDBElementALocaliser : TemplateReport
    {
        public List<ElementLocaliser> listElementLocaliser { set; get; }

        public void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = "1";
            jw.Formatting = Formatting.Indented;
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
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

    internal class ElementLocaliser : TemplateReport
    {
        internal void ToJSONRepresentation(JsonWriter jw, string id)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 10) + 1).ToString();

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
