using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNPUCore.RapportLivraison
{
    class RLivraison
    {
        public RLivraison()
        {
            Processus = new List<Processus>();
        }

        public string Id { set; get; }

        public string Name { set; get; }

        public DateTime Debut { set; get; }

        public DateTime Fin { set; get; }

        public string Result { set; get; }
        public List<Processus> Processus { set; get; }
        public string IdClient { get; internal set; }

        public string ToJSONRepresentation(Localisation RapportLocalisation)
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonTextWriter(new StringWriter(sb));

            string idxLivraison = AddRapport(jw, this);
            List<Processus> registerProcess = Processus;

            for (int i = 0; i < registerProcess.Count; i++)
            {
                AddProcessus(jw, registerProcess, i, idxLivraison);
            }
            jw.WriteEndArray();
            jw.WriteEndObject();

            jw.WriteStartObject();
            string idxLocalisation = AddLocalisation(jw, RapportLocalisation);
            List<Elements> registerElements = RapportLocalisation.Elements;

            for (int i = 0; i < registerElements.Count; i++)
            {
                AddElements(jw, registerElements, i, idxLocalisation);
            }

            jw.WriteEndObject();

            jw.WriteEndArray();

            return sb.ToString();
        }

        private string AddLocalisation(JsonWriter jw, Localisation RapportLocalisation)
        {
            jw.Formatting = Formatting.Indented;
            jw.WritePropertyName("id");
            jw.WriteValue("2");
            jw.WritePropertyName("name");
            jw.WriteValue(RapportLocalisation.Name);
            jw.WritePropertyName("cctTaskID");
            jw.WriteValue(RapportLocalisation.CctTaskID);
            jw.WritePropertyName("cctVersion");
            jw.WriteValue(RapportLocalisation.CctVersion);
            jw.WritePropertyName("nbElements");
            jw.WriteValue(RapportLocalisation.NbElements);
            jw.WritePropertyName("result");
            jw.WriteValue(RapportLocalisation.Result);

            jw.WritePropertyName("elements");
            jw.WriteStartArray();

            return "2";
        }

        private void AddElements(JsonWriter jw, List<Elements> registerElements, int i, string idxLocalisation)
        {
            string sIDElements;

            sIDElements = String.Concat(idxLocalisation, (i + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDElements);
            jw.WritePropertyName("objectType");
            jw.WriteValue(registerElements[i].ObjectType);
            jw.WritePropertyName("objectID");
            jw.WriteValue(registerElements[i].ObjectID);
            jw.WritePropertyName("parentObj");
            jw.WriteValue(registerElements[i].ParentObj);
            jw.WritePropertyName("auxObj");
            jw.WriteValue(registerElements[i].AuxObj);
            jw.WritePropertyName("aux2Obj");
            jw.WriteValue(registerElements[i].Aux2Obj);
            jw.WritePropertyName("aux3Obj");
            jw.WriteValue(registerElements[i].Aux3Obj);
            jw.WriteEndObject();
        }

        private void AddProcessus(JsonWriter jw, List<Processus> registerProcess, int i, string idxLivraison)
        {
            string sIDProcess;

            sIDProcess = String.Concat(idxLivraison, (i + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDProcess);
            jw.WritePropertyName("name");
            jw.WriteValue(registerProcess[i].Name);
            jw.WritePropertyName("result");
            jw.WriteValue(registerProcess[i].Result);
            jw.WriteEndObject();
        }

        private string AddRapport(JsonWriter jw, RLivraison rapportLivraison)
        {
            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue("1");
            jw.WritePropertyName("name");
            jw.WriteValue(rapportLivraison.Name);
            jw.WritePropertyName("result");
            jw.WriteValue(rapportLivraison.Result);
            jw.WritePropertyName("debut");
            jw.WriteValue(rapportLivraison.Debut.ToString("dd/MM/yy H:mm:ss"));
            jw.WritePropertyName("fin");
            jw.WriteValue(rapportLivraison.Fin.ToString("dd/MM/yy H:mm:ss"));

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            return "1";
        }
    }

    class Processus
    {
        private string result { get; set; }
        public String Id { set; get; }
        public String Name { set; get; }
        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class Localisation
    {
        public Localisation()
        {
            Elements = new List<Elements>();
        }

        public String Id { set; get; }
        public String Name { set; get; }

        public String CctTaskID { set; get; }
        public String CctVersion { set; get; }
        public int NbElements { set; get; }
        public string Result { set; get; }
        public List<Elements> Elements { set; get; }
    }

    class Elements
    {
        public String Id { set; get; }
        public String ObjectType { set; get; }
        public String ObjectID { set; get; }
        public String ParentObj { set; get; }
        public String AuxObj { set; get; }
        public String Aux2Obj { set; get; }
        public String Aux3Obj { set; get; }
    }
}
