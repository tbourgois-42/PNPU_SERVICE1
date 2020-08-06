using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNPUCore.RapportTNR
{
    class RTNR
    {
        private string id;
        private string name;
        private DateTime debut;
        private DateTime fin;
        private string result;
        private List<Domaine> domaine;

        public RTNR()
        {
            domaine = new List<Domaine>();
        }

        /// <summary>
        /// Build a JSON object from RapportTNR class.
        /// </summary>
        /// <returns>Return a JSON object formatted into string format.</returns>
        public String ToJSONRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonTextWriter(new StringWriter(sb));

            string idxDomaine = AddRapport(jw, this);
            List<Domaine> registerDomaine = Domaine;

            for (int i = 0; i < registerDomaine.Count; i++)
            {
                string idxSousDomaine = AddDomaine(jw, registerDomaine, i, idxDomaine);
                List<SousDomaine> registerSousDomaine = registerDomaine[i].SousDomaine;

                for (int j = 0; j < registerSousDomaine.Count; j++)
                {
                    string idxSousDomaineParts = AddSousDomaine(jw, registerSousDomaine, j, idxSousDomaine);
                    List<SousDomaineParts> registerSousDomaineParts = registerSousDomaine[j].SousDomaineParts;

                    for (int k = 0; k < registerSousDomaineParts.Count; k++)
                    {
                        string idxClassification = AddSousDomaineParts(jw, registerSousDomaineParts, k, idxSousDomaineParts);
                        List<Classification> registerClassification = registerSousDomaineParts[k].Classification;

                        for (int l = 0; l < registerClassification.Count; l++)
                        {
                            string idxEcarts = AddClassification(jw, registerClassification, l, idxClassification);
                            List<Ecarts> registerEcarts = registerClassification[l].Ecarts;

                            for (int m = 0; m < registerEcarts.Count; m++)
                            {
                                string idxMatricules = AddEcarts(jw, registerEcarts, m, idxEcarts);
                                List<Matricules> registerMatricules = registerEcarts[m].Matricules;

                                for (int n = 0; n < registerMatricules.Count; n++)
                                {
                                    AddMatricule(jw, registerMatricules, n, idxMatricules);
                                }
                                CloseObject(jw);
                            }
                            CloseObject(jw);
                        }
                        CloseObject(jw);
                    }
                    CloseObject(jw);
                }
                CloseObject(jw);
            }
            CloseObject(jw);
            CloseRapport(jw);

            return sb.ToString();
        }

        private void CloseRapport(JsonWriter jw)
        {
            jw.WriteEndArray();
        }

        private void CloseObject(JsonWriter jw)
        {
            jw.WriteEndArray();
            jw.WriteEndObject();
        }

        private string AddEcarts(JsonWriter jw, List<Ecarts> registerEcarts, int m, string id)
        {
            string sIDEcarts;

            sIDEcarts = String.Concat(id, (m + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDEcarts);
            jw.WritePropertyName("name");
            jw.WriteValue(registerEcarts[m].Name);
            jw.WritePropertyName("valueBefore");
            jw.WriteValue(registerEcarts[m].ValueBefore);
            jw.WritePropertyName("valueAfter");
            jw.WriteValue(registerEcarts[m].ValueAfter);
            jw.WritePropertyName("difference");
            jw.WriteValue(registerEcarts[m].Difference);
            jw.WritePropertyName("comment");
            jw.WriteValue(registerEcarts[m].Comment);

            jw.WritePropertyName("matricules");
            jw.WriteStartArray();

            return sIDEcarts;
        }

        private string AddClassification(JsonWriter jw, List<Classification> registerClassification, int l, string id)
        {
            string sIDClassification;

            sIDClassification = String.Concat(id, (l + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDClassification);
            jw.WritePropertyName("name");
            jw.WriteValue(registerClassification[l].Name);
            jw.WritePropertyName("result");
            jw.WriteValue(registerClassification[l].Result);

            jw.WritePropertyName("ecarts");
            jw.WriteStartArray();

            return sIDClassification;
        }

        private string AddSousDomaineParts(JsonWriter jw, List<SousDomaineParts> registerSousDomaineParts, int k, string id)
        {
            string sIDSousDomaineParts;

            sIDSousDomaineParts = String.Concat(id, (k + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDSousDomaineParts);
            jw.WritePropertyName("name");
            jw.WriteValue(registerSousDomaineParts[k].Name);
            jw.WritePropertyName("result");
            jw.WriteValue(registerSousDomaineParts[k].Result);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            return sIDSousDomaineParts;
        }

        private string AddSousDomaine(JsonWriter jw, List<SousDomaine> registerSousDomaine, int j, string id)
        {
            string sIDSousDomaine;

            sIDSousDomaine = String.Concat(id, (j + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDSousDomaine);
            jw.WritePropertyName("name");
            jw.WriteValue(registerSousDomaine[j].Name);
            jw.WritePropertyName("result");
            jw.WriteValue(registerSousDomaine[j].Result);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            return sIDSousDomaine;
        }

        private string AddRapport(JsonWriter jw, RTNR rTNR)
        {
            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue("1");
            jw.WritePropertyName("name");
            jw.WriteValue(rTNR.name);
            jw.WritePropertyName("result");
            jw.WriteValue(rTNR.result);
            jw.WritePropertyName("debut");
            jw.WriteValue(rTNR.Debut.ToString("dd/MM/yy H:mm:ss"));
            jw.WritePropertyName("fin");
            jw.WriteValue(rTNR.Fin.ToString("dd/MM/yy H:mm:ss"));

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            return "1";
        }

        private string AddDomaine(JsonWriter jw, object registerDomaine, int i, string id)
        {
            string sIDDomaine;

            sIDDomaine = String.Concat(id, (i + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDDomaine);
            jw.WritePropertyName("name");
            jw.WriteValue(Domaine[i].Name);
            jw.WritePropertyName("result");
            jw.WriteValue(Domaine[i].Result);

            jw.WritePropertyName("children");
            jw.WriteStartArray();

            return sIDDomaine;
        }

        private void AddMatricule(JsonWriter jw, List<Matricules> registerMatricules, int n, string id)
        {
            string sIDMatricule;

            sIDMatricule = String.Concat(id, (n + 1).ToString());
            jw.WriteStartObject();
            jw.WritePropertyName("id");
            jw.WriteValue(sIDMatricule);
            jw.WritePropertyName("dtpaie");
            jw.WriteValue(registerMatricules[n].Dtpaie.ToShortDateString());
            jw.WritePropertyName("dtalloc");
            jw.WriteValue(registerMatricules[n].Dtalloc.ToShortDateString());
            jw.WritePropertyName("idorga");
            jw.WriteValue(registerMatricules[n].Idorga);
            jw.WritePropertyName("societe");
            jw.WriteValue(registerMatricules[n].Societe);
            jw.WritePropertyName("etablissement");
            jw.WriteValue(registerMatricules[n].Etablissement);
            jw.WritePropertyName("matricule");
            jw.WriteValue(registerMatricules[n].Matricule);
            jw.WritePropertyName("period");
            jw.WriteValue(registerMatricules[n].Periode);
            jw.WritePropertyName("valueBefore");
            jw.WriteValue(registerMatricules[n].ValueBefore);
            jw.WritePropertyName("valueAfter");
            jw.WriteValue(registerMatricules[n].ValueAfter);
            jw.WritePropertyName("difference");
            jw.WriteValue(registerMatricules[n].Difference);
            jw.WriteEndObject();
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

        public String Result
        {
            set { result = value; }
            get { return result; }
        }
        public List<Domaine> Domaine
        {
            set { domaine = value; }
            get { return domaine; }
        }
        public string IdClient { get; internal set; }
    }

    class Domaine
    {
        private string id;
        private string name;
        private string result { get; set; }
        private List<SousDomaine> sousdomaine;

        public Domaine()
        {
            sousdomaine = new List<SousDomaine>();
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

        public List<SousDomaine> SousDomaine
        {
            set { sousdomaine = value; }
            get { return sousdomaine; }
        }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }

    }

    class SousDomaine
    {
        private string id;
        private string name;
        private string result { get; set; }
        private List<SousDomaineParts> sousdomaineparts;

        public SousDomaine()
        {
            sousdomaineparts = new List<SousDomaineParts>();
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

        public List<SousDomaineParts> SousDomaineParts
        {
            set { sousdomaineparts = value; }
            get { return sousdomaineparts; }
        }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class SousDomaineParts
    {
        private string id;
        private string name;
        private string result { get; set; }
        private List<Classification> classification;

        public SousDomaineParts()
        {
            classification = new List<Classification>();
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

        public List<Classification> Classification
        {
            set { classification = value; }
            get { return classification; }
        }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class Classification
    {
        private string id;
        private string name;
        private string result { get; set; }
        private List<Ecarts> ecarts;

        public Classification()
        {
            ecarts = new List<Ecarts>();
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

        public List<Ecarts> Ecarts
        {
            set { ecarts = value; }
            get { return ecarts; }
        }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class Ecarts
    {
        private string id;
        private string name;
        private decimal valueBefore;
        private decimal valueAfter;
        private decimal difference;
        private string comment;
        private List<Matricules> matricules { get; set; }

        public Ecarts()
        {
            matricules = new List<Matricules>();
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
        public decimal ValueBefore
        {
            set { valueBefore = value; }
            get { return valueBefore; }
        }
        public decimal ValueAfter
        {
            set { valueAfter = value; }
            get { return valueAfter; }
        }
        public decimal Difference
        {
            set { difference = value; }
            get { return difference; }
        }
        public string Comment
        {
            set { comment = value; }
            get { return comment; }
        }
        public List<Matricules> Matricules
        {
            set { matricules = value; }
            get { return matricules; }
        }
    }
    class Matricules
    {
        private string id;
        private DateTime dtpaie;
        private DateTime dtalloc;
        private string idorga;
        private string societe;
        private string etablissement;
        private string matricule;
        private decimal periode;
        private decimal valueBefore;
        private decimal valueAfter;
        private decimal difference;

        public String Id
        {
            set { id = value; }
            get { return id; }
        }
        public DateTime Dtpaie
        {
            set { dtpaie = value; }
            get { return dtpaie; }
        }
        public DateTime Dtalloc
        {
            set { dtalloc = value; }
            get { return dtalloc; }
        }
        public string Idorga
        {
            set { idorga = value; }
            get { return idorga; }
        }
        public string Societe
        {
            set { societe = value; }
            get { return societe; }
        }
        public string Etablissement
        {
            set { etablissement = value; }
            get { return etablissement; }
        }
        public string Matricule
        {
            set { matricule = value; }
            get { return matricule; }
        }
        public decimal Periode
        {
            set { periode = value; }
            get { return periode; }
        }
        public decimal ValueBefore
        {
            set { valueBefore = value; }
            get { return valueBefore; }
        }
        public decimal ValueAfter
        {
            set { valueAfter = value; }
            get { return valueAfter; }
        }
        public decimal Difference
        {
            set { difference = value; }
            get { return difference; }
        }
    }

}
