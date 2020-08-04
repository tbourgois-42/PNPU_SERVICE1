using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

namespace PNPUTools.DataManager
{
    public class DataManagerSQLServer : IDataManager
    {
        // Voir comment on gère la chaine de connexion. Normalement dans les infos clients
        string GetConnectionString()
        {
            return String.Empty;
        }

        public override DataSet GetData(string sRequest, string sConnectionString)
        {
            DataSet dataSet = null;
            string sTableName = string.Empty;


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
                    sTableName = GetTableName(sRequest);
                    if (sTableName == string.Empty)
                        adapter.Fill(dataSet);
                    else
                        adapter.Fill(dataSet, sTableName);
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

        public static DataSet GetDatasWithParams(string sRequest, string sConnectionString, decimal idProcess, decimal workflowId, string clientId, int idInstanceWF)
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
                    command.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int, 15).Value = idInstanceWF;
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

        /// <summary>
        /// Gestion des transactions SQL.
        /// </summary>
        /// <param name="transactionName">Nom de la transaction.</param>
        /// <param name="reqInsert">Tableau de requêtes à traiter.</param>
        /// <param name="sTable">Nom de la table impactée par la requête</param>
        /// <param name="parameters">Paramètres nécessaire à la requête</param>
        /// <param name="getKeyAutoIndent">Saisir true pour récupérer la clé autogénérée</param>
        /// <returns>Retourne le dernier ID auto incrément en cas d'INSERT, autrement "Requête traité avec succès".</returns>
        public static string ExecuteSqlTransaction(string[] reqInsert, string sTable, string[] parameters, bool getKeyAutoIndent = false)
        {
            string ReturnValue = null;

            using (SqlConnection connection = new SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {

                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                string UniqueID = Guid.NewGuid().ToString().Substring(0,32);

                transaction = connection.BeginTransaction(UniqueID);

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    foreach(string request in reqInsert)
                    {
                        // Requete d'insertion
                        for (int i = 0; i < parameters.Count(); i += 2)
                        {
                            command.Parameters.AddWithValue(parameters[i], parameters[i + 1]);
                        }
                        command.CommandText = request;
                        command.ExecuteNonQuery();
                        // Récupère la dernière clé auto incrément
                        if (reqInsert.Length == 1 && getKeyAutoIndent == true)
                        {
                            // TODO : Retourner un tableau key value des clé auto incrément en cas d'insertion multiple
                            command.CommandText = "SELECT IDENT_CURRENT('" + sTable + "') AS [IDENT_CURRENT]";
                            ReturnValue = command.ExecuteScalar().ToString();
                        }
                    }
                    transaction.Commit();
                    // Si on n'est pas sur une requête d'insertion on renvoi une chaine de caractère
                    ReturnValue = (ReturnValue == null) ? "Requête traité avec succès" : ReturnValue;
                } 
                catch (Exception ex)
                {
                    ReturnValue = ex.ToString();

                    // En cas d'erreur on Rollback la transaction
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exRlbk)
                    {
                        // Le Rollback a échoué. Exemple, coupure de connexion avec le serveur
                        ReturnValue = exRlbk.ToString();
                    }
                }
            }
            return ReturnValue;
        }

        /// <summary>
        /// Gestion des transactions SQL.
        /// </summary>
        /// <param name="transactionName">Nom de la transaction.</param>
        /// <param name="reqInsert">Tableau de requêtes à traiter.</param>
         /// <param name="parameters">Paramètres nécessaire à la requête</param>
        /// <returns>Retourne le dernier ID auto incrément en cas d'INSERT, autrement "Requête traité avec succès".</returns>
        public static string ExecuteSqlTransaction(string[] reqInsert, string[] parameters, string sConnectionString)
        {
            string ReturnValue = "OK";

            using (SqlConnection connection = new SqlConnection(sConnectionString))
            {

                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                string UniqueID = Guid.NewGuid().ToString().Substring(0, 32);

                transaction = connection.BeginTransaction(UniqueID);

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    for (int i = 0; i < parameters.Count(); i += 2)
                    {
                        command.Parameters.AddWithValue(parameters[i], parameters[i + 1]);
                    }
                    foreach (string request in reqInsert)
                    {
                        // Requete d'insertion
                       
                        command.CommandText = request;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    // Si on n'est pas sur une requête d'insertion on renvoi une chaine de caractère
                    ReturnValue = (ReturnValue == null) ? "Requête traité avec succès" : ReturnValue;
                }
                catch (Exception ex)
                {
                    ReturnValue = ex.ToString();

                    // En cas d'erreur on Rollback la transaction
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exRlbk)
                    {
                        // Le Rollback a échoué. Exemple, coupure de connexion avec le serveur
                        ReturnValue = exRlbk.ToString();
                    }
                }
            }
            return ReturnValue;
        }

        /// <summary>
        /// Check if exist
        /// </summary>
        /// <param name="sRequest">Request</param>
        /// <param name="sConnectionString">Connection string</param>
        /// <returns></returns>
        public static string SelectCount(string sRequest, string sConnectionString)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(sConnectionString))
            {
                string value = "";
                try
                {
                    conn.Open();
                    SqlCommand command = conn.CreateCommand();
                    command.CommandText = sRequest;
                    value = command.ExecuteScalar().ToString();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("La requete : " + sRequest + " a échoué.");
                    return ex.ToString();
                }
                return value;
            }
        }

        internal static bool DeleteDatas(string sRequest, string connectionString)
        {
            bool result = false;
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand command = conn.CreateCommand();
                    command.CommandText = sRequest;
                    command.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("La requete : " + sRequest + " a échoué.");
                }
            }
            return result;
        }

        /// <summary>
        /// Get list of compressed file from the database
        /// </summary>
        /// <param name="sRequest"></param>
        /// <param name="connectionStringBaseAppli"></param>
        /// <returns>Return list of byte array</returns>
        internal static List<byte[]> ReadBinaryDatas(string sRequest, string connectionStringBaseAppli)
        {
            byte[] MDB = null;
            DataSet dataSet = null;
            List<byte[]> lstMDB = new List<byte[]>();

            try
            {
                using (var conn = new SqlConnection(connectionStringBaseAppli))
                {
                    using (var cmd = new SqlCommand(sRequest, conn))
                    {
                        conn.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = cmd;

                        dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        foreach (DataRow drRow in dataSet.Tables[0].Rows)
                        {
                            lstMDB.Add((byte[])drRow["MDB"]);
                        }
                    }

                }
                return lstMDB;
            }
            catch (Exception ex)
            {
                Console.WriteLine("La requete : " + sRequest + " a échoué.");
                return lstMDB;
            }
        }
    }
}
