using Newtonsoft.Json;
using PNPUTools.DataManager;
using System;
using System.Data;

namespace PNPUCore.Database
{
    internal static class Class2
    {
        private static readonly string requestAllClient = "select CLI.CLIENT_ID, DATABASE_ID CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID";
        private static readonly string connectionStringSupport = "server=M4FRDB16.fr.meta4.com;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";

        private static string getAllInfoClient()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllClient, connectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            return json;
        }

        private static void Main(string[] args)
        {
            Console.WriteLine(Class2.getAllInfoClient());
        }

    }
}
