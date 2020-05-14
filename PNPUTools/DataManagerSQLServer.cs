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
        /// <summary>
        /// Gestion des transactions SQL.
        /// </summary>
        /// <param name="transactionName">Nom de la transaction.</param>
        /// <param name="reqInsert">Tableau de requêtes à traiter.</param>
        /// <param name="sTable">Nom de la table impactée par la requête</param>
        /// <param name="parameters">Paramètres nécessaire à la requête</param>
        /// <returns>Retourne le dernier ID auto incrément en cas d'INSERT, autrement "Requête traité avec succès".</returns>
        public static string ExecuteSqlTransaction(string[] reqInsert, string sTable, string[] parameters, bool getKeyAutoIndent)
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
    }
}
