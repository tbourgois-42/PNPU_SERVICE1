using System;
using System.Data;
using System.Data.SqlClient;

namespace PNPUTools
{
    static public class Logger
    {

        public static void Log(string id_process, int iteration, int workflowId, string message, string statutMessage, string idControle, string isControle, string server, string baseSQL, int idInstanceWF)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();

                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_LOG (ID_PROCESS, ITERATION, WORKFLOW_ID, MESSAGE, STATUT_MESSAGE, ID_CONTROLE, IS_CONTROLE, DATE_LOG, SERVER, BASE, NIVEAU_LOG, ID_H_WORKFLOW) VALUES (@ID_PROCESS, @ITERATION, @WORKFLOW_ID, @MESSAGE, @STATUT_MESSAGE, @ID_CONTROLE, @IS_CONTROLE, @DATE_LOG, @SERVER, @BASE, @NIVEAU_LOG, @ID_H_WORKFLOW)", conn))
                    {
                        //TODO vérif if all element is not null
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.VarChar, 50).Value = id_process;
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = iteration;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = workflowId;
                        cmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 250).Value = message;
                        cmd.Parameters.Add("@STATUT_MESSAGE", SqlDbType.VarChar, 50).Value = statutMessage;
                        cmd.Parameters.Add("@ID_CONTROLE", SqlDbType.VarChar, 50).Value = idControle;
                        cmd.Parameters.Add("@IS_CONTROLE", SqlDbType.VarChar, 50).Value = isControle;
                        cmd.Parameters.Add("@DATE_LOG", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@SERVER", SqlDbType.VarChar, 50).Value = server;//process.SERVER;
                        cmd.Parameters.Add("@BASE", SqlDbType.VarChar, 50).Value = baseSQL;//process.BASE;
                        cmd.Parameters.Add("@NIVEAU_LOG", SqlDbType.VarChar, 50).Value = ParamAppli.LogLevel;
                        cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int).Value = idInstanceWF;

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }



        public static void Log(string id_process, int iteration, int workflowId, string message, string statutMessage, string server, string baseSQL, int idInstanceWF)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();

                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_LOG (ID_PROCESS, ITERATION, WORKFLOW_ID, MESSAGE, STATUT_MESSAGE, ID_CONTROLE, IS_CONTROLE, DATE_LOG, SERVER, BASE, NIVEAU_LOG, ID_H_WORKFLOW) VALUES (@ID_PROCESS, @ITERATION, @WORKFLOW_ID, @MESSAGE, @STATUT_MESSAGE, @ID_CONTROLE, @IS_CONTROLE, @DATE_LOG, @SERVER, @BASE, @NIVEAU_LOG, @ID_H_WORKFLOW)", conn))
                    {
                        //TODO vérif if all element is not null
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.VarChar, 50).Value = id_process;
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = iteration;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = workflowId;
                        cmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 250).Value = message;
                        cmd.Parameters.Add("@STATUT_MESSAGE", SqlDbType.VarChar, 50).Value = statutMessage;
                        cmd.Parameters.Add("@ID_CONTROLE", SqlDbType.VarChar, 50).Value = "";
                        cmd.Parameters.Add("@IS_CONTROLE", SqlDbType.VarChar, 50).Value = "N";
                        cmd.Parameters.Add("@DATE_LOG", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@SERVER", SqlDbType.VarChar, 50).Value = server;
                        cmd.Parameters.Add("@BASE", SqlDbType.VarChar, 50).Value = baseSQL;
                        cmd.Parameters.Add("@NIVEAU_LOG", SqlDbType.VarChar, 50).Value = ParamAppli.LogLevel;
                        cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int).Value = idInstanceWF;

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Log(string message, string statutMessage)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();

                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_LOG (ID_PROCESS, ITERATION, WORKFLOW_ID, MESSAGE, STATUT_MESSAGE, ID_CONTROLE, IS_CONTROLE, DATE_LOG, SERVER, BASE, NIVEAU_LOG, ID_H_WORKFLOW) VALUES (@ID_PROCESS, @ITERATION, @WORKFLOW_ID, @MESSAGE, @STATUT_MESSAGE, @ID_CONTROLE, @IS_CONTROLE, @DATE_LOG, @SERVER, @BASE, @NIVEAU_LOG, @ID_H_WORKFLOW)", conn))
                    {
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.VarChar, 50).Value = "";
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = -1;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = -1;
                        cmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 250).Value = message;
                        cmd.Parameters.Add("@STATUT_MESSAGE", SqlDbType.VarChar, 50).Value = statutMessage;
                        cmd.Parameters.Add("@ID_CONTROLE", SqlDbType.VarChar, 50).Value = "";
                        cmd.Parameters.Add("@IS_CONTROLE", SqlDbType.VarChar, 50).Value = "N";
                        cmd.Parameters.Add("@DATE_LOG", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@SERVER", SqlDbType.VarChar, 50).Value = "";
                        cmd.Parameters.Add("@BASE", SqlDbType.VarChar, 50).Value = "";
                        cmd.Parameters.Add("@NIVEAU_LOG", SqlDbType.VarChar, 50).Value = ParamAppli.LogLevel;
                        cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int).Value = -1;

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
