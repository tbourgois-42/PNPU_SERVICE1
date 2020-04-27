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

        public static DataSet GetDatasWithParams(string sRequest, string sConnectionString, decimal idProcess, decimal workflowId, string clientId)
        {
            DataSet dataSet = null;

            try
            {
                using (SqlConnection connection =
                 new SqlConnection(sConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    // Open the connection and fill the DataSet.
                    SqlCommand command = new SqlCommand(sRequest, connection);
                    command.Parameters.Add("@ID_PROCESS", SqlDbType.Int, 15).Value = idProcess;
                    command.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int, 15).Value = workflowId;
                    command.Parameters.Add("@CLIENT_ID", SqlDbType.VarChar, 64).Value = clientId;
                    adapter.SelectCommand = command;

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

        public static string GetLastInsertedPK(string sTable, string sConnectionString)
        {
            DataSet dataSet = null;
            string LastInsertedPK = "";
            string sRequest = "SELECT IDENT_CURRENT('" + sTable + "') AS [IDENT_CURRENT]";

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
                    // On récupère la dernière PK généré en automatique par la BDD
                    LastInsertedPK = dataSet.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                // A gérer la mise à jour du log
                //Console.WriteLine(ex.Message);
                dataSet = null;
            }
            return LastInsertedPK;
        }

        /// <summary>
        /// Créer l'enregistrement en base en récupérant la valeur de la clé primaire créée automatiquement.
        /// </summary>
        /// <param string="sRequest">Chemin du dossier à ziper.</param>
        /// <param PNPU_PROCESS="input">Paramétres récupérés depuis le Web Service</param>
        /// <param string="sTable">Nom de la table impacté par l'insertion</param>
        /// <returns>Retourne 0 si ok, -1 en cas de problème.</returns>
        public static string SendTransactionWithGetLastPKid(string sRequest, PNPU_PROCESS input, string sTable)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                string LastInsertedPK = "";
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = conn.BeginTransaction("CreateProcess");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = conn;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = sRequest;
                    /*for (int i = 0; i < sParameters.Length; i++)
                    {
                        sParameters[i].ToString();
                        command.Parameters.Add("@" + sParameters[i].ToString(), SqlDbType.VarChar, 254).Value = sParameters[i].ToString();
                    }*/
                    command.Parameters.Add("@PROCESS_LABEL", SqlDbType.VarChar, 254).Value = input.PROCESS_LABEL;
                    command.Parameters.Add("@IS_LOOPABLE", SqlDbType.VarChar, 254).Value = input.IS_LOOPABLE;
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT IDENT_CURRENT('" + sTable + "') AS [IDENT_CURRENT]";
                    LastInsertedPK = command.ExecuteScalar().ToString();
                    transaction.Commit();
                    Console.WriteLine(LastInsertedPK);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);
                    try
                    {
                        transaction.Rollback();
                        return "Commit Exception Type: {0}" + ex.GetType();
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                        return "Rollback Exception Type: {0}" + ex2.GetType();
                    }
                }
                return LastInsertedPK;
            }
        }
    }
}
