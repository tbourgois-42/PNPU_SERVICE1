using PNPUCore.Controle;
using PNPUCore.Process;
using PNPUTools;
using System;
using System.Data;
using System.Data.SqlClient;

namespace PNPUCore
{
    static class Logger
    {

        public static void Log(IProcess process, IControle controle, string statutMessage, string message)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();

                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_LOG (ID_PROCESS, ITERATION, WORKFLOW_ID, MESSAGE, STATUT_MESSAGE, ID_CONTROLE, IS_CONTROLE, DATE_LOG, SERVER, BASE, NIVEAU_LOG, ID_H_WORKFLOW) VALUES (@ID_PROCESS, @ITERATION, @WORKFLOW_ID, @MESSAGE, @STATUT_MESSAGE, @ID_CONTROLE, @IS_CONTROLE, @DATE_LOG, @SERVER, @BASE, @NIVEAU_LOG, @ID_H_WORKFLOW)", conn))
                    {
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.VarChar, 50).Value = process.ToString();
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = 1;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = process.WORKFLOW_ID;
                        cmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 250).Value = message;
                        cmd.Parameters.Add("@STATUT_MESSAGE", SqlDbType.VarChar, 50).Value = statutMessage;
                        cmd.Parameters.Add("@ID_CONTROLE", SqlDbType.VarChar, 50).Value = controle.ToString();
                        cmd.Parameters.Add("@IS_CONTROLE", SqlDbType.VarChar, 50).Value = "Y";
                        cmd.Parameters.Add("@DATE_LOG", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@SERVER", SqlDbType.VarChar, 50).Value = "SERVER";//process.SERVER;
                        cmd.Parameters.Add("@BASE", SqlDbType.VarChar, 50).Value = "BASE";//process.BASE;
                        cmd.Parameters.Add("@NIVEAU_LOG", SqlDbType.VarChar, 50).Value = ParamAppli.LogLevel;
                        cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int).Value = process.ID_INSTANCEWF;

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }



        public static void Log(IProcess process, string statutMessage, string message)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ParamAppli.ConnectionStringBaseAppli))
            {
                try
                {

                    conn.Open();

                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO PNPU_LOG (ID_PROCESS, ITERATION, WORKFLOW_ID, MESSAGE, STATUT_MESSAGE, ID_CONTROLE, IS_CONTROLE, DATE_LOG, SERVER, BASE, NIVEAU_LOG, ID_H_WORKFLOW) VALUES (@ID_PROCESS, @ITERATION, @WORKFLOW_ID, @MESSAGE, @STATUT_MESSAGE, @ID_CONTROLE, @IS_CONTROLE, @DATE_LOG, @SERVER, @BASE, @NIVEAU_LOG, @ID_H_WORKFLOW)", conn))
                    {
                        cmd.Parameters.Add("@ID_PROCESS", SqlDbType.VarChar, 50).Value = process.ToString();
                        cmd.Parameters.Add("@ITERATION", SqlDbType.Int).Value = 1;
                        cmd.Parameters.Add("@WORKFLOW_ID", SqlDbType.Int).Value = process.WORKFLOW_ID;
                        cmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 250).Value = message;
                        cmd.Parameters.Add("@STATUT_MESSAGE", SqlDbType.VarChar, 50).Value = statutMessage;
                        cmd.Parameters.Add("@ID_CONTROLE", SqlDbType.VarChar, 50).Value = "";
                        cmd.Parameters.Add("@IS_CONTROLE", SqlDbType.VarChar, 50).Value = "N";
                        cmd.Parameters.Add("@DATE_LOG", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@SERVER", SqlDbType.VarChar, 50).Value = "SERVER";//process.SERVER;
                        cmd.Parameters.Add("@BASE", SqlDbType.VarChar, 50).Value = "BASE";// process.BASE;
                        cmd.Parameters.Add("@NIVEAU_LOG", SqlDbType.VarChar, 50).Value = ParamAppli.LogLevel;
                        cmd.Parameters.Add("@ID_H_WORKFLOW", SqlDbType.Int).Value = process.ID_INSTANCEWF;

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
