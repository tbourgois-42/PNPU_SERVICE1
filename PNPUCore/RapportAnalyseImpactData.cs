using Newtonsoft.Json;
using PNPUTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNPUCore
{
    class TemplateReport
    {
        public String Id { set; get; }

        public String Name { set; get; }

        public String Tooltip { set; get; }

    }


    class RapportAnalyseData : TemplateReport
    {
        public String Result { set; get; }

        public String IdClient { set; get; }
        public DateTime Debut { set; get; }

        public DateTime Fin { set; get; }
        public List<RapportAnalyseImpactMDBData> listRapportAnalyseImpactMDBData { set; get; }

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
            int iIndex = 0;

            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }
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
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);
            jw.WritePropertyName("debut");
            jw.WriteValue(sCote + Debut.ToString("dd/MM/yy H:mm:ss") + sCote);
            jw.WritePropertyName("fin");
            jw.WriteValue(sCote + Fin.ToString("dd/MM/yy H:mm:ss") + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (RapportAnalyseImpactMDBData type in listRapportAnalyseImpactMDBData)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
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
    }

    class RapportAnalyseImpactMDBData : TemplateReport
    {
        public String Result { set; get; }
        public List<RapportAnalyseImpactPackData> listRapportAnalyseImpactPackData { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;
            int iIndex = 0;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (RapportAnalyseImpactPackData type in listRapportAnalyseImpactPackData)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class RapportAnalyseImpactPackData : TemplateReport
    {
        public String Result { set; get; }
        public String CodePack { set; get; }
        public String NumCommande { set; get; }
        public List<CommandData> listCommandData { set; get; }
        public List<EltsALocaliserData> listEltsALocaliserData { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;
            int iIndex = 0;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            /*jw.WritePropertyName("name");
            jw.WriteValue(sCote + this.Name + sCote);
            jw.WritePropertyName("codePack");
            jw.WriteValue(this.CodePack);
            jw.WritePropertyName("numCommande");
            jw.WriteValue(this.NumCommande);*/
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + CodePack + " - " + NumCommande + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);
            jw.WritePropertyName("tooltip");
            jw.WriteValue(sCote + Tooltip + sCote);

            jw.WritePropertyName("listCommand");
            jw.WriteStartArray();
            foreach (CommandData type in listCommandData)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }
            jw.WriteEndArray();

            if (listEltsALocaliserData.Count > 0)
            {
                jw.WritePropertyName("Eléments à localiser");

                jw.WriteStartArray();
                foreach (EltsALocaliserData elts in listEltsALocaliserData)
                {
                    iIndex++;
                    elts.ToJSONRepresentation(jw, Id, iIndex++);
                }
                jw.WriteEndArray();
            }
            jw.WriteEndObject();
        }
    }

    class CommandData : TemplateReport
    {
        public String Result { get; set; }
        public String Message { get; set; }
        //public List<ControleCommandData> listControleCommandData;
        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;
            int iIndex = 0;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);
            jw.WritePropertyName("message");
            jw.WriteValue(sCote + Message + sCote);
            //jw.WritePropertyName("tooltip");
            //jw.WriteValue(sCote + this.Tooltip + sCote);
            /* if (listControleCommandData.Count > 0)
             {
                 jw.WritePropertyName("message");
                 jw.WriteStartArray();
                 foreach (ControleCommandData type in listControleCommandData)
                 {
                     iIndex++;
                     type.ToJSONRepresentation(jw, this.Id, iIndex);
                 }

                 jw.WriteEndArray();
             }*/
            jw.WriteEndObject();
        }
    }

    class ControleCommandData : TemplateReport
    {
        public String Result { get; set; }
        public string Message { get; set; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            //jw.WritePropertyName("name");
            //jw.WriteValue(sCote + this.Name + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Message + sCote);
            //jw.WritePropertyName("tooltip");
            //jw.WriteValue(sCote + this.Tooltip + sCote);
            jw.WriteEndObject();
        }
    }


    class EltsALocaliserData : TemplateReport
    {
        public String Result { get; set; }
        public string Message { get; set; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            string sCote2 = string.Empty;

            if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sCote = "'";
                sCote2 = "*";
            }

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);
            jw.WritePropertyName("message");
            jw.WriteValue(sCote + Message + sCote);
            jw.WriteEndObject();
        }
    }


}
