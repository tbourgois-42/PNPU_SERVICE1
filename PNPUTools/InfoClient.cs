using System;
using System.Collections.Generic;
using System.Text;

namespace PNPUTools
{

    public class InfoClient
    {
        public string ID_CLIENT { get; set; }
        public string CLIENT_NAME { get; set; }
        public string TYPOLOGY { get; set; }
        public string TYPOLOGY_ID { get; set; }
        public string ConnectionStringQA1 { get; set; }
        public string ConnectionStringQA2 { get; set; }

        public InfoClient(string iID_CLIENT, string sCLIENT_NAME, string sTypology, string sTypology_ID, string sConnectionStringQA1, string sConnectionStringQA2)
        {
            ID_CLIENT = iID_CLIENT;
            CLIENT_NAME = sCLIENT_NAME;
            TYPOLOGY = sTypology;
            TYPOLOGY_ID = sTypology_ID;
            ConnectionStringQA1 = sConnectionStringQA1;
            ConnectionStringQA2 = sConnectionStringQA2;
        }

        public InfoClient()
        {
        }
    }


}
