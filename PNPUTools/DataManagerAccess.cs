using System;
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
            string sTableName = string.Empty;

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
                    sTableName = GetTableName(sRequest);
                    if (sTableName == string.Empty)
                    {
                        adapter.Fill(dataSet);
                    }
                    else
                    {
                        adapter.Fill(dataSet, sTableName);
                    }
                }
                catch (Exception ex)
                {
                    //TODO LOG
                    Console.WriteLine(ex.Message);
                    dataSet = null;
                }

            }

            return dataSet;
        }


    }
}
