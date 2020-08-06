using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNPUCore.RapportLivraison
{
    class RLivraison
    {
        private string id;
        private string name;
        private DateTime debut;
        private DateTime fin;
        private string result;
        private List<Processus> processus;

        public RLivraison()
        {
            processus = new List<Processus>();
        }

        public string Id
        {
            set { id = value; }
            get { return id; }
        }

        public string Name
        {
            set { name = value; }
            get { return name; }
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

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
        public List<Processus> Processus
        {
            set { processus = value; }
            get { return processus; }
        }
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
            jw.WriteValue(rapportLivraison.name);
            jw.WritePropertyName("result");
            jw.WriteValue(rapportLivraison.result);
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
        private string id;
        private string name;
        private string result { get; set; }
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
        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class Localisation
    {
        private string id;
        private string name;
        private string cctTaskID;
        private string cctVersion;
        private int nbElements;
        private string result;
        private List<Elements> elements;

        public Localisation()
        {
            elements = new List<Elements>();
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

        public String CctTaskID
        {
            set { cctTaskID = value; }
            get { return cctTaskID; }
        }
        public String CctVersion
        {
            set { cctVersion = value; }
            get { return cctVersion; }
        }
        public int NbElements
        {
            set { nbElements = value; }
            get { return nbElements; }
        }
        public string Result
        {
            set { result = value; }
            get { return result; }
        }
        public List<Elements> Elements
        {
            set { elements = value; }
            get { return elements; }
        }
    }

    class Elements
    {
        private string id;
        private string objectType;
        private string objectID;
        private string parentObj;
        private string auxObj;
        private string aux2Obj;
        private string aux3Obj;

        public String Id
        {
            set { id = value; }
            get { return id; }
        }
        public String ObjectType
        {
            set { objectType = value; }
            get { return objectType; }
        }
        public String ObjectID
        {
            set { objectID = value; }
            get { return objectID; }
        }
        public String ParentObj
        {
            set { parentObj = value; }
            get { return parentObj; }
        }
        public String AuxObj
        {
            set { auxObj = value; }
            get { return auxObj; }
        }
        public String Aux2Obj
        {
            set { aux2Obj = value; }
            get { return aux2Obj; }
        }
        public String Aux3Obj
        {
            set { aux3Obj = value; }
            get { return aux3Obj; }
        }
    }
}
