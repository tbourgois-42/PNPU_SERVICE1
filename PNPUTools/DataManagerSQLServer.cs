using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace PNPUTools.DataManager
{
    public class DataManagerSQLServer : IDataManager
    {
        // Voir comment on gère la chaine de connexion. Normalement dans les infos clients
        string GetConnectionString()
        {
            return String.Empty;
        }

        public DataSet GetData(string sRequest, string sConnectionString)
        {
            DataSet dataSet = null;

            try 
            {
                using (SqlConnection connection =
                 new SqlConnection(sConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter adapter =
                        new SqlDataAdapter(sRequest, connection);

                    // Open the connection and fill the DataSet.


                    dataSet = new DataSet();
                    adapter.Fill(dataSet);
                }
            }
            catch (Exception ex)
            {
                // A gérer la mise à jour du log
                //Console.WriteLine(ex.Message);
                dataSet = null;
            }
             return dataSet;
        }

        public static DataSet GetDatas(string sRequest, string sConnectionString)
        {
            DataSet dataSet = null;

            try
            {
                using (SqlConnection connection =
                 new SqlConnection(sConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter adapter =
                        new SqlDataAdapter(sRequest, connection);

                    // Open the connection and fill the DataSet.


                    dataSet = new DataSet();
                    adapter.Fill(dataSet);
                }
            }
            catch (Exception ex)
            {
                // A gérer la mise à jour du log
                //Console.WriteLine(ex.Message);
                dataSet = null;
            }
            return dataSet;
        }

    }
}
