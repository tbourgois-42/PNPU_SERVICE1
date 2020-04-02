using Newtonsoft.Json;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PNPUTools
{
    public class RequestTool
    {

        static string requestAllClient = "select CLI.CLIENT_ID, DATABASE_ID, CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID";
        static string requestOneClient = "select CLI.CLIENT_ID, DATABASE_ID, CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID AND TRIGRAMME = ";
        static string connectionStringSupport = "server=M4FRDB16;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";

        public static string GetAllInfoClient()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllClient, connectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            var regex = new Regex(Regex.Escape("Table"));
            var newJson = regex.Replace(json, "Clients", 1);
            return newJson;
        }

        public static string GetInfoOneClient(string clientName)
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestOneClient + "'" + clientName + "'", connectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            var regex = new Regex(Regex.Escape("Table"));
            var newJson = regex.Replace(json, "Client", 1);
            return newJson;
        }

    }
}
