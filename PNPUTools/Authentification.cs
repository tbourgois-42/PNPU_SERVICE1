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

namespace PNPUTools
{
    public class Authentification
    {
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

        public static string ConnectUser(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);

            string sToken = parser.GetParameterValue("token");

            return GetUserName(sToken);
        }

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

        public static bool SignOutUser(Stream stream)
        {
            var parser = MultipartFormDataParser.Parse(stream);

            string user = parser.GetParameterValue("user");
            string token = parser.GetParameterValue("token");

            return DisconnectUser(user, token);
        }

        static bool DisconnectUser(string User, string Token)
        {
            string sRequest = "DELETE FROM PNPU_USER_TOKEN WHERE USER_ID = '" + User + "' AND TOKEN = '" + Token + "'";
            bool result = DataManagerSQLServer.DeleteDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return result == true ? true : false;
        }

        public static string GetHabilitation(string user, string token)
        {
            user = user.Contains('\\') ? user.Replace(@"\", @"%") : user;
            string sRequest = "SELECT PROFIL_ID FROM PNPU_HABILITATION PHAB, PNPU_USER_TOKEN PUTKN WHERE PUTKN.USER_ID LIKE '%" + user + "%' AND PUTKN.TOKEN = '" + token + "' AND PUTKN.USER_ID = PHAB.USER_ID";
            DataSet result = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            return HasData(result) ? result.Tables[0].Rows[0].ItemArray[0].ToString() : "";
        }

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

        public static string[] GetListClient(string sHabilitation, string sUser)
        {
            string[] lstClientKeys = ParamAppli.ListeInfoClient.Keys.ToArray<String>();
            sUser = sUser.Contains('\\') ? sUser.Replace(@"\", @"%") : sUser;
            List<string> lstClient = null;

            string sRequest = "SELECT CLIENT_ID FROM HABILITATIONS WHERE USER_ID LIKE '%" + sUser + "%'";

            if (sHabilitation == "ADMIN")
            {
                return lstClientKeys;
            }
            else
            {
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
                return lstClient != null ? lstClient.ToArray() : null;
            }
        }
    }
}
