using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNPUCore.RapportTNR
{
    class RTNR
    {
        public RTNR()
        {
            Domaine = new List<Domaine>();
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
            jw.WriteValue(rTNR.Name);
            jw.WritePropertyName("result");
            jw.WriteValue(rTNR.Result);
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

        public String Id { set; get; }

        public String Name { set; get; }

        public DateTime Debut { set; get; }

        public DateTime Fin { set; get; }

        public String Result { set; get; }
        public List<Domaine> Domaine { set; get; }
        public string IdClient { get; internal set; }
    }

    class Domaine
    {
        private string result { get; set; }

        public Domaine()
        {
            SousDomaine = new List<SousDomaine>();
        }


        public String Id { set; get; }

        public String Name { set; get; }

        public List<SousDomaine> SousDomaine { set; get; }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }

    }

    class SousDomaine
    {
        private string result { get; set; }

        public SousDomaine()
        {
            SousDomaineParts = new List<SousDomaineParts>();
        }

        public String Id { set; get; }

        public String Name { set; get; }

        public List<SousDomaineParts> SousDomaineParts { set; get; }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class SousDomaineParts
    {
        private string result { get; set; }

        public SousDomaineParts()
        {
            Classification = new List<Classification>();
        }

        public String Id { set; get; }

        public String Name { set; get; }

        public List<Classification> Classification { set; get; }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class Classification
    {
        private string result { get; set; }

        public Classification()
        {
            Ecarts = new List<Ecarts>();
        }

        public String Id { set; get; }

        public String Name { set; get; }

        public List<Ecarts> Ecarts { set; get; }

        public string Result
        {
            set { result = value; }
            get { return result; }
        }
    }

    class Ecarts
    {
        private List<Matricules> matricules { get; set; }

        public Ecarts()
        {
            matricules = new List<Matricules>();
        }

        public String Id { set; get; }
        public String Name { set; get; }
        public decimal ValueBefore { set; get; }
        public decimal ValueAfter { set; get; }
        public decimal Difference { set; get; }
        public string Comment { set; get; }
        public List<Matricules> Matricules
        {
            set { matricules = value; }
            get { return matricules; }
        }
    }
    class Matricules
    {
        public String Id { set; get; }
        public DateTime Dtpaie { set; get; }
        public DateTime Dtalloc { set; get; }
        public string Idorga { set; get; }
        public string Societe { set; get; }
        public string Etablissement { set; get; }
        public string Matricule { set; get; }
        public decimal Periode { set; get; }
        public decimal ValueBefore { set; get; }
        public decimal ValueAfter { set; get; }
        public decimal Difference { set; get; }
    }

}
