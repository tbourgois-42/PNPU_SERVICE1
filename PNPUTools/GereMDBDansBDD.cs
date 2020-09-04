using System;
using System.IO;

namespace PNPUTools
{
    public class GereMDBDansBDD
    {
        /// <summary>
        /// Methode qui zippe et insère des fichiers dans la BDD.
        /// </summary>
        /// <param name="sFichiers">Liste des fichiers à compresser et intégrer dans le BDD</param>
        /// <param name="WORKFLOW_ID">ID du workflow pour lequel on enregistre les fichiers.</param>
        /// <param name="sDossierTempo">Dossier de travail pour générer le fichier zip.</param>
        /// <param name="sChaineDeConnexion">Chaine de connexion de la base SQL Server.</param>
        /// <returns></returns>
        public int AjouteFichiersMDBBDD(string[] sFichiers, decimal WORKFLOW_ID, string sDossierTempo, string sChaineDeConnexion, int idInstanceWF, string CLIENT_ID = "", int iNiv = 0)
        {
            try
            {
                string sNom = WORKFLOW_ID.ToString("0000000000");
                if (CLIENT_ID != "")
                {
                    sNom += "_C" + CLIENT_ID + "_N" + iNiv.ToString();
                }

                sDossierTempo += "\\" + sNom;
                string sFichierZip = sDossierTempo + "\\" + sNom + ".ZIP";
                if (!Directory.Exists(sDossierTempo))
                {
                    Directory.CreateDirectory(sDossierTempo);
                }

                if (ZIP.ManageZip.CompresseListeFichiers(sFichiers, sFichierZip) == -1)
                {
                    return -1;
                }

                if (AjouteZipBDD(sFichierZip, WORKFLOW_ID, sChaineDeConnexion, idInstanceWF, CLIENT_ID, iNiv) == -1)
                {
                    return -1;
                }

                File.Delete(sFichierZip);
                Directory.Delete(sDossierTempo);

                return 0;
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Méthode qui insère un fichier ZIP dans la BDD.
        /// </summary>
        /// <param name="sFichierZip">Fichier Zip à insérer.</param>
        /// <param name="WORKFLOW_ID">ID du workflow pour lequel on enregistre le fichier ZIP.</param>
        /// <param name="sChaineDeConnexion">Chaine de connexion de la base SQL Server.</param>
        /// <returns>Retourne 0 si ok, -1 en cas d'erreur.</returns>
        public int AjouteZipBDD(string sFichierZip, decimal WORKFLOW_ID, string sChaineDeConnexion, int idInstanceWF, string CLIENT_ID = "", int iNiv = 0)
        {
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(sFichierZip);
                string sRequete;

                using (var conn = new System.Data.SqlClient.SqlConnection(sChaineDeConnexion))
                {
                    conn.Open();
                    sRequete = "DELETE FROM PNPU_MDB WHERE WORKFLOW_ID = " + WORKFLOW_ID.ToString("#########0") + " AND CLIENT_ID = '" + CLIENT_ID + "' AND NIV_DEP = '" + iNiv.ToString() + "' AND ID_H_WORKFLOW = " + idInstanceWF;

                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    sRequete = "INSERT INTO PNPU_MDB (WORKFLOW_ID, MDB, CLIENT_ID, NIV_DEP, ID_H_WORKFLOW) VALUES(" + WORKFLOW_ID.ToString("#########0") + ",@VALEUR,'" + CLIENT_ID + "','" + iNiv.ToString() + "'," + idInstanceWF + ")";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                    {
                        var param = new System.Data.SqlClient.SqlParameter("@VALEUR", System.Data.SqlDbType.Binary)
                        {
                            Value = bytes
                        };

                        cmd.Parameters.Add(param);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            return -1;
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ParamAppli.StatutError);
                throw;
            }
        }

        /// <summary>
        /// Methode qui extrait un ZIP de la BDD et le décompresse.
        /// </summary>
        /// <param name="sFichiers">Liste des fichiers à compresser et intégrer dans le BDD</param>
        /// <param name="WORKFLOW_ID">ID du workflow pour lequel on enregistre les fichiers.</param>
        /// <param name="sDossierTempo">Dossier de travail pour générer le fichier zip.</param>
        /// <param name="sChaineDeConnexion">Chaine de connexion de la base SQL Server.</param>
        /// <returns></returns>
        public int ExtraitFichiersMDBBDD(ref string[] sFichiers, decimal WORKFLOW_ID, string sDossierTempo, string sChaineDeConnexion, int idInstanceWF, string CLIENT_ID = "", int iNiv = 0)
        {
            try
            {
                string sNom = WORKFLOW_ID.ToString("0000000000");
                if (CLIENT_ID != "")
                {
                    sNom += "_C" + CLIENT_ID + "_N" + iNiv.ToString();
                }

                sDossierTempo += "\\" + sNom;
                string sFichierZip = sDossierTempo + "\\" + sNom + ".ZIP";

                if (!Directory.Exists(sDossierTempo))
                {
                    Directory.CreateDirectory(sDossierTempo);
                }
                else
                {
                    foreach (string sFichier in Directory.GetFiles(sDossierTempo))
                    {
                        File.Delete(sFichier);
                    }
                }

                if (ExtraitZipBDD(sFichierZip, WORKFLOW_ID, sChaineDeConnexion, idInstanceWF, CLIENT_ID, iNiv) == -1)
                {
                    return -1;
                }

                if (PNPUTools.ZIP.ManageZip.DecompresseDansDossier(sFichierZip, sDossierTempo) == -1)
                {
                    return -1;
                }

                File.Delete(sFichierZip);

                sFichiers = Directory.GetFiles(sDossierTempo);

                return 0;
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Méthode qui extrait un fichier ZIP de la BDD.
        /// </summary>
        /// <param name="sFichierZip">Fichier Zip à Extraire.</param>
        /// <param name="WORKFLOW_ID">ID du workflow pour lequel on enregistre le fichier ZIP.</param>
        /// <param name="sChaineDeConnexion">Chaine de connexion de la base SQL Server.</param>
        /// <returns>Retourne 0 si ok, -1 en cas d'erreur.</returns>
        public int ExtraitZipBDD(string sFichierZip, decimal WORKFLOW_ID, string sChaineDeConnexion, int idInstanceWF, string CLIENT_ID = "", int iNiv = 0)
        {
            string sRequete;
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(sChaineDeConnexion))
                {
                    sRequete = "SELECT MDB FROM PNPU_MDB WHERE WORKFLOW_ID = " + WORKFLOW_ID.ToString("#########0") + " AND CLIENT_ID ='" + CLIENT_ID + "' AND NIV_DEP = '" + iNiv.ToString() + "' AND ID_H_WORKFLOW = " + idInstanceWF + "";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                    {
                        conn.Open();
                        byte[] bytes2 = cmd.ExecuteScalar() as byte[];
                        System.IO.File.WriteAllBytes(sFichierZip, bytes2);
                    }

                }
                return 0;
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}
