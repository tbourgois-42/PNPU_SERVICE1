using HttpMultipartParser;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection.Emit;
using System.Net;

namespace PNPUTools
{
    public class Authentification
    {
        /// <summary>
        /// Connect user
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Return token string if user has been successfuly connected</returns>
        public static string AuthUser(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);

            string user = parser.GetParameterValue("user");
            string password = parser.GetParameterValue("password");

            string sToken = string.Empty;

            if (IsUserExist(user))
            {
                if (IsValidAuth(user, password))
                {
                    if (IsExpiredToken(user))
                    {
                        sToken = GenerateToken(user);
                    }
                    else
                    {
                        sToken = GetUserToken(user);
                    }
                }
            }

            return sToken;
        }

        /// <summary>
        /// Get user token
        /// </summary>
        /// <param name="User"></param>
        /// <returns>Return token string</returns>
        static string GetUserToken(string User)
        {
            string sToken = string.Empty;

            try
            {
                string sRequest = "SELECT PUTKN.TOKEN FROM PNPU_USER PUSER, PNPU_USER_TOKEN PUTKN WHERE PUSER.USER_ID = PUTKN.USER_ID AND PUTKN.TOKEN IS NOT NULL AND PUTKN.EXPIRED_DATE < SYSDATETIME() AND PUSER.USER_ID = '" + User + "'";
                DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

                if (result.Tables.Count > 0)
                {
                    sToken = result.Tables[0].Rows[0].ItemArray[0].ToString();
                }
            }
            catch (Exception)
            {
                return sToken;
            }
            return sToken;
        }

        /// <summary>
        /// Check if user exist in database
        /// </summary>
        /// <param name="User"></param>
        /// <returns>Return true if exist, false if not</returns>
        static bool IsUserExist(string User)
        {
            try
            {
                string sRequest = "SELECT USER_ID FROM PNPU_USER WHERE USER_ID = '" + User + "'";
                DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);
                
                return result.Tables.Count > 0 ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Generate token for user
        /// </summary>
        /// <param name="User"></param>
        /// <returns>Return string token</returns>
        static string GenerateToken(string User)
        {
            string sToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            
            string[] sRequest = { "INSERT INTO PNPU_USER_TOKEN (USER_ID, TOKEN, EXPIRED_DATE) VALUES (@USER_ID, @TOKEN, @EXPIRED_DATE)" };
            string[] parameters = new string[] { "@USER_ID", User, "@TOKEN", sToken, "@EXPIRED_DATE", DateTime.UtcNow.AddHours(24).ToString("MM/dd/yyyy HH:mm:ss") };
           
            try
            {
                DataManagerSQLServer.ExecuteSqlTransaction(sRequest, "PNPU_USER_TOKEN", parameters);
            }
            catch (Exception)
            {
                sToken = string.Empty;
            }

            return sToken;
        }

        /// <summary>
        /// Check if token was expired
        /// </summary>
        /// <param name="User"></param>
        /// <returns>Return true if expired, false if not</returns>
        static bool IsExpiredToken(string User)
        {
            DateTime expiredTime;
            bool value = false;

            string sRequest = "SELECT EXPIRED_DATE FROM PNPU_USER_TOKEN WHERE USER_ID = '" + User + "'";
            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            if (result.Tables.Count > 0)
            {
                if (result.Tables[0].Rows.Count > 0)
                {
                    expiredTime = DateTime.Parse(result.Tables[0].Rows[0].ItemArray[0].ToString());
                    if (expiredTime > DateTime.UtcNow)
                    {
                        value = true;
                    }
                }
                else
                {
                    value = true;
                }
            }
            return value;
        }

        /// <summary>
        /// Check if user password is correct
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <returns>Return true if password is correct, false if not</returns>
        static bool IsValidAuth(string User, string Password)
        {
            string sRequest = "SELECT USER_ID, PASSWORD FROM PNPU_USER WHERE USER_ID = '" + User + "'";
            bool value = false;
            string passwordDecrypted = string.Empty;

            try
            {
                DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

                if (result.Tables.Count > 0)
                {
                    /*string user = result.Tables[0].Rows[0].ItemArray[0].ToString();
                    byte[] password = Encoding.ASCII.GetBytes(result.Tables[0].Rows[0].ItemArray[1].ToString());

                    using (Aes myAes = Aes.Create())
                    {
                        passwordDecrypted = DecryptStringToBytes_Aes(password, myAes.Key, myAes.IV);
                    }

                    return Password == passwordDecrypted ? true : false;*/
                    if (result.Tables[0].Rows.Count > 0)
                    {
                        return result.Tables[0].Rows[0].ItemArray[1].ToString() == Password ? true : false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return value;
        }

        /// <summary>
        /// Decrypt user password
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        static string DecryptStringToBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream crDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(crDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Connect user
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ConnectUser(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);

            string sToken = parser.GetParameterValue("token");

            return GetUserName(sToken);
        }

        /// <summary>
        /// Get username by user token
        /// </summary>
        /// <param name="sToken"></param>
        /// <returns></returns>
        static string GetUserName(string sToken)
        {
            string user = null;
            string sRequest = "SELECT PUSER.USER_ID FROM PNPU_USER PUSER, PNPU_USER_TOKEN PUTK WHERE PUSER.USER_ID = PUTK.USER_ID AND PUTK.TOKEN = '" + sToken + "'";
            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            if (result.Tables.Count > 0)
            {
                if (result.Tables[0].Rows.Count > 0)
                {
                    user = result.Tables[0].Rows[0].ItemArray[0].ToString();
                }
            }
            return user;
        }

        /// <summary>
        /// Disconnect user
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool SignOutUser(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);

            string user = parser.GetParameterValue("user");
            string token = parser.GetParameterValue("token");

            return DisconnectUser(user, token);
        }

        /// <summary>
        /// Delete user token from PNPU_USER_TOKEN
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        static bool DisconnectUser(string User, string Token)
        {
            string sRequest = "DELETE FROM PNPU_USER_TOKEN WHERE USER_ID = '" + User + "' AND TOKEN = '" + Token + "'";
            bool result = DataManagerSQLServer.DeleteDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return result == true ? true : false;
        }

        /// <summary>
        /// Get habilitation profil of user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetHabilitation(string user, string token)
        {
            user = user.Contains('\\') ? user.Replace(@"\", @"%") : user;
            string sRequest = "SELECT PROFIL_ID FROM PNPU_HABILITATION PHAB, PNPU_USER_TOKEN PUTKN WHERE PUTKN.USER_ID LIKE '%" + user + "%' AND PUTKN.TOKEN = '" + token + "' AND PUTKN.USER_ID = PHAB.USER_ID";
            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return HasData(result) ? result.Tables[0].Rows[0].ItemArray[0].ToString() : "";
        }

        /// <summary>
        /// Check if dataset have data
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private static bool HasData(DataSet datas)
        {
            if (datas.Tables.Count > 0)
            {
                return datas.Tables[0].Rows.Count > 0 ? true : false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get list of visible clients for a user
        /// </summary>
        /// <param name="sHabilitation"></param>
        /// <param name="sUser"></param>
        /// <returns>Return list of client</returns>
        public static IEnumerable<InfoClient> GetListClient(string sHabilitation, string sUser)
        {
            string[] lstClientKeys = ParamAppli.ListeInfoClient.Keys.ToArray<String>();
            sUser = sUser.Contains('\\') ? sUser.Replace(@"\", @"%") : sUser;
            List<string> lstClient = new List<string>();

            string sRequest = string.Empty;

            if (sHabilitation == "ADMIN")
            {
                sRequest = GenerateRequestGetListClient(lstClient, sHabilitation);
                DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.connectionStringSupport);
                if (HasData(result))
                {
                    return result.Tables[0].DataTableToList<InfoClient>();
                }
            }
            else
            {
                lstClient = GenerateListStringClient(lstClientKeys, sUser);
                sRequest = GenerateRequestGetListClient(lstClient, sHabilitation);

                DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.connectionStringSupport);
                if (HasData(result))
                {
                    return result.Tables[0].DataTableToList<InfoClient>();
                }
            }
            return null;
        }

        private static List<string> GenerateListStringClient(string[] lstClientKeys, string sUser)
        {
            string sRequest = "SELECT CLIENT_ID FROM HABILITATIONS WHERE USER_ID LIKE '%" + sUser + "%'";
            List<string> lstClient = new List<string>();

            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.connectionStringSupport);

            if (HasData(result))
            {
                foreach (string clientIDKeys in lstClientKeys)
                {
                    foreach (DataRow clientID in result.Tables[0].Rows)
                    {
                        if (clientIDKeys.ToString() == clientID.ItemArray[0].ToString())
                        {
                            lstClient.Add(clientIDKeys);
                        }
                    }
                }
            }

            return lstClient;
        }

        /// <summary>
        /// Generate request in order to Get list of clients
        /// </summary>
        /// <param name="lstClient"></param>
        /// <returns>Return string request</returns>
        private static string GenerateRequestGetListClient(List<string> lstClient, string sHabilitation)
        {
            string sSelect = "SELECT CLI.CLIENT_ID AS ID_CLIENT, CLI.CLIENT_NAME, CLI.SAAS AS TYPOLOGY_ID, COD.CODIFICATION_LIBELLE AS TYPOLOGY ";
            string sFrom = "FROM A_CLIENT CLI, A_CODIFICATION COD ";
            string sWhere = "WHERE COD.CODIFICATION_ID = CLI.SAAS ";
            string sRequest = sSelect + sFrom;
            string sAlias = "CLI";

            sWhere += BuildHabilitationWhereClause(lstClient, sHabilitation, sAlias);

            sRequest = sSelect + sFrom + sWhere;

            return sRequest;
        }

        /// <summary>
        /// Build habilitation where clause
        /// </summary>
        /// <param name="lstClient"></param>
        /// <param name="shabilitation"></param>
        /// <returns></returns>
        private static string BuildHabilitationWhereClause(List<string> lstClient, string sHabilitation, string sAlias)
        {
            string sWhere = string.Empty;
            bool bPremier = true;

            // If is'nt admin profil we generate where clause with list of client visible for the user
            if (sHabilitation != "ADMIN")
            {
                foreach (var clientID in lstClient)
                {
                    if (bPremier == true)
                    {
                        bPremier = false;
                        sWhere += "AND " + sAlias + ".CLIENT_ID IN (";
                    }
                    else
                    {
                        sWhere += ",";
                    }

                    sWhere += "'" + clientID + "'";
                }
                sWhere += ")";
            }
            return sWhere;
        }

        public static string GetHabilitationWhereClause(string sHabilitation, string sUser, string sAlias)
        {
            string[] lstClientKeys = ParamAppli.ListeInfoClient.Keys.ToArray<String>();
            List<string> lstClient  = null;

            if (sHabilitation == "ADMIN")
            {
                lstClient = new List<string>(lstClientKeys);
            } else
            {
                lstClient = GenerateListStringClient(lstClientKeys, sUser);
            }

            

            string sWhere = BuildHabilitationWhereClause(lstClient, sHabilitation, sAlias);

            return sWhere;
        }
    }
}
