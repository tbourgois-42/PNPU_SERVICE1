using Newtonsoft.Json;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNPUCore.Database
{
    class Class2
    {

        static string requestAllClient = "select CLI.CLIENT_ID, DATABASE_ID CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID";
        static string requestOneClient = "select CLI.CLIENT_ID, DATABASE_ID CLIENT_NAME, TRIGRAMME, HOST, USER_ACCOUNT, USER_PASSWORD from DBS DATA, A_CLIENT CLI where CLI.CLIENT_ID = DATA.CLIENT_ID AND TRIGRAMME = ";
        static string connectionStringSupport = "server=M4FRDB16;uid=META4_DOCSUPPREAD;pwd=META4_DOCSUPPREAD;database=META4_DOCSUPP;";





        static string getAllInfoClient()
        {
            DataSet result = DataManagerSQLServer.GetDatas(requestAllClient, connectionStringSupport);
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            return json;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Class2.getAllInfoClient());
        }

    }
}
