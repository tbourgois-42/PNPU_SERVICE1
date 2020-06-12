using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;

namespace PNPUTools.DataManager
{
    public class DataManagerAccess : IDataManager
    {
    
        public string GetConnectionString(string sMdbPath)
        {
            return "Driver={Microsoft Access Driver (*.mdb)};Dbq="
                + sMdbPath
                + ";Uid=Admin;Pwd=;";
        }

        public override DataSet GetData(string sRequest, string sMdbPath)
        {
            DataSet dataSet = null;

             using (OdbcConnection connection =
              new OdbcConnection(GetConnectionString(sMdbPath)))
            {
                OdbcDataAdapter adapter =
                    new OdbcDataAdapter(sRequest, connection);

                // Open the connection and fill the DataSet.
                try
                {
                    connection.Open();
                    dataSet = new DataSet();
                    adapter.Fill(dataSet);
                }
                catch (Exception ex)
                {
                    // A gérer la mise à jour du log
                    //Console.WriteLine(ex.Message);
                    dataSet = null;
                }

            }

             return dataSet;
        }

        
    }
}
