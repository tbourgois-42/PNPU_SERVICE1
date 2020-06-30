using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            set { this.id = value; }
            get { return this.id; }
        }

        public string Name
        {
            set { this.name = value; }
            get { return this.name; }
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

        public string Result
        {
            set { this.result = value; }
            get { return this.result; }
        }
        public List<Processus> Processus
        {
            set { this.processus = value; }
            get { return this.processus; }
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
            set { this.id = value; }
            get { return this.id; }
        }
        public String Name
        {
            set { this.name = value; }
            get { return this.name; }
        }
        public string Result
        {
            set { this.result = value; }
            get { return this.result; }
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
            set { this.id = value; }
            get { return this.id; }
        }
        public String Name
        {
            set { this.name = value; }
            get { return this.name; }
        }

        public String CctTaskID
        {
            set { this.cctTaskID = value; }
            get { return this.cctTaskID; }
        }
        public String CctVersion
        {
            set { this.cctVersion = value; }
            get { return this.cctVersion; }
        }
        public int NbElements
        {
            set { this.nbElements = value; }
            get { return this.nbElements; }
        }
        public string Result
        {
            set { this.result = value; }
            get { return this.result; }
        }
        public List<Elements> Elements
        {
            set { this.elements = value; }
            get { return this.elements; }
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
            set { this.id = value; }
            get { return this.id; }
        }
        public String ObjectType
        {
            set { this.objectType = value; }
            get { return this.objectType; }
        }
        public String ObjectID
        {
            set { this.objectID = value; }
            get { return this.objectID; }
        }
        public String ParentObj
        {
            set { this.parentObj = value; }
            get { return this.parentObj; }
        }
        public String AuxObj
        {
            set { this.auxObj = value; }
            get { return this.auxObj; }
        }
        public String Aux2Obj
        {
            set { this.aux2Obj = value; }
            get { return this.aux2Obj; }
        }
        public String Aux3Obj
        {
            set { this.aux3Obj = value; }
            get { return this.aux3Obj; }
        }
    }
}
