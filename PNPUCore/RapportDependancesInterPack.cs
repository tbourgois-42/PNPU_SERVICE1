using Newtonsoft.Json;
using PNPUTools;
using System;
using System.Collections.Generic;

namespace PNPUCore
{

    class RapportDependancesInterPack : TemplateReport
    {
        public String Result { set; get; }


        public List<RapportDependancesInterPackMDB> listRapportDependancesInterPackMDB { set; get; }

        private string DetermineFormat(int iNombre)
        {
            string sFormat = "0";
            for (int i = 10; i <= iNombre; i *= 10)
                sFormat += "0";

            return sFormat;
        }

        public void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            /*StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonTextWriter(new StringWriter(sb));*/
            int iIndex = 0;

            string sCote = string.Empty;

            Id = id;
            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);
            jw.WritePropertyName("result");
            jw.WriteValue(sCote + ParamAppli.TranscoSatut[Result] + sCote);

            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (RapportDependancesInterPackMDB type in listRapportDependancesInterPackMDB)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
            jw.WriteEndArray();

            /*if (PNPUTools.ParamAppli.SimpleCotesReport == false)
            {
                sb = sb.Replace("\"", "");
                sb = sb.Replace(sCote2, "\"");
            }

            return sb.ToString();*/
        }
    }

    class RapportDependancesInterPackMDB : TemplateReport
    {
        public List<RapportDependancesInterPackPack> listRapportDependancesInterPackPack { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            int iIndex = 0;

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);


            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (RapportDependancesInterPackPack type in listRapportDependancesInterPackPack)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class RapportDependancesInterPackPack : TemplateReport
    {
        public List<RapportDependancesInterPackMDBN2> listRapportDependancesInterPackMDBN2 { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            int iIndex = 0;

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);


            jw.WritePropertyName("elements");
            jw.WriteStartArray();
            foreach (RapportDependancesInterPackMDBN2 type in listRapportDependancesInterPackMDBN2)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class RapportDependancesInterPackMDBN2 : TemplateReport
    {
        public List<RapportDependancesInterPack2> listRapportDependancesInterPack2 { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            int iIndex = 0;

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);


            jw.WritePropertyName("children");
            jw.WriteStartArray();
            foreach (RapportDependancesInterPack2 type in listRapportDependancesInterPack2)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class RapportDependancesInterPack2 : TemplateReport
    {
        public List<RapportDependancesInterPackElt> listRapportDependancesInterPackElt { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;
            int iIndex = 0;

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("name");
            jw.WriteValue(sCote + Name + sCote);


            jw.WritePropertyName("elements");
            jw.WriteStartArray();
            foreach (RapportDependancesInterPackElt type in listRapportDependancesInterPackElt)
            {
                iIndex++;
                type.ToJSONRepresentation(jw, Id, iIndex);
            }

            jw.WriteEndArray();
            jw.WriteEndObject();
        }
    }

    class RapportDependancesInterPackElt : TemplateReport
    {
        public String ObjectType { set; get; }
        public String ObjectID { set; get; }

        internal void ToJSONRepresentation(JsonWriter jw, string id, int index)
        {
            string sCote = string.Empty;

            Id = ((Int32.Parse(id) * 100) + index).ToString();

            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(Id);
            jw.WritePropertyName("objectType");
            jw.WriteValue(sCote + ObjectType + sCote);
            jw.WritePropertyName("objectID");
            jw.WriteValue(sCote + ObjectID + sCote);
            jw.WriteEndObject();
        }
    }
}
