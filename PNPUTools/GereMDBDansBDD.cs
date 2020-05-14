using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int AjouteFichiersMDBBDD(string[] sFichiers, decimal WORKFLOW_ID, string sDossierTempo, string sChaineDeConnexion, string CLIENT_ID = "")
        {
            try
            {
                sDossierTempo += "\\" + WORKFLOW_ID.ToString("0000000000");
                string sFichierZip = sDossierTempo + "\\" + WORKFLOW_ID.ToString("0000000000") + ".ZIP";
                if (Directory.Exists(sDossierTempo) == false)
                    Directory.CreateDirectory(sDossierTempo);
                
                if (ZIP.ManageZip.CompresseListeFichiers(sFichiers, sFichierZip) == -1)
                    return -1;

                if (AjouteZipBDD(sFichierZip, WORKFLOW_ID, sChaineDeConnexion,CLIENT_ID) == -1)
                    return -1;

                File.Delete(sFichierZip);
                Directory.Delete(sDossierTempo);

                return 0;
            }
            catch (Exception ex)
            {
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
        public int AjouteZipBDD(string sFichierZip, decimal WORKFLOW_ID, string sChaineDeConnexion, string CLIENT_ID = "")
        {
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(sFichierZip);
                string sRequete;

                using (var conn = new System.Data.SqlClient.SqlConnection(sChaineDeConnexion))
                {
                    conn.Open();
                    if (CLIENT_ID == "")
                        sRequete = "DELETE FROM PNPU_MDB WHERE ID_H_WORKFLOW = " + WORKFLOW_ID.ToString("#########0") + " AND CLIENT_ID IS NULL";
                    else
                        sRequete = "DELETE FROM PNPU_MDB WHERE ID_H_WORKFLOW = " + WORKFLOW_ID.ToString("#########0") + " AND CLIENT_ID = '" + CLIENT_ID +"'";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete,conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    if (CLIENT_ID == "")
                        sRequete = "INSERT INTO PNPU_MDB (ID_H_WORKFLOW,MDB) VALUES(" + WORKFLOW_ID.ToString("#########0") + ",@VALEUR)";
                    else
                        sRequete = "INSERT INTO PNPU_MDB (ID_H_WORKFLOW,MDB,CLIENT_ID) VALUES(" + WORKFLOW_ID.ToString("#########0") + ",@VALEUR,'" + CLIENT_ID + "')";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sRequete, conn))
                    {
                        var param = new System.Data.SqlClient.SqlParameter("@VALEUR", System.Data.SqlDbType.Binary)
                        {
                            Value = bytes
                        };

                        cmd.Parameters.Add(param);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected < 1)
                            return -1;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
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
        public int ExtraitFichiersMDBBDD(ref string[] sFichiers, decimal WORKFLOW_ID, string sDossierTempo, string sChaineDeConnexion, string CLIENT_ID = "")
        {
            try
            {
                sDossierTempo += "\\" + WORKFLOW_ID.ToString("0000000000");
                string sFichierZip = sDossierTempo + "\\" + WORKFLOW_ID.ToString("0000000000") + ".ZIP";

                if (Directory.Exists(sDossierTempo) == false)
                    Directory.CreateDirectory(sDossierTempo);
                else
                {
                    foreach (string sFichier in Directory.GetFiles(sDossierTempo))
                    {
                        File.Delete(sFichier);
                    }
                }

                if (ExtraitZipBDD(sFichierZip, WORKFLOW_ID, sChaineDeConnexion) == -1)
                    return -1;

                if (PNPUTools.ZIP.ManageZip.DecompresseDansDossier(sFichierZip, sDossierTempo) == -1)
                    return -1;

                File.Delete(sFichierZip);

                sFichiers = Directory.GetFiles(sDossierTempo);

                return 0;
            }
            catch (Exception ex)
            {
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
        public int ExtraitZipBDD(string sFichierZip, decimal WORKFLOW_ID, string sChaineDeConnexion, string CLIENT_ID = "")
        {
            string sRequete;
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(sChaineDeConnexion))
                {
                    if (CLIENT_ID == "")
                        sRequete = "SELECT MDB FROM PNPU_MDB WHERE ID_H_WORKFLOW = " + WORKFLOW_ID.ToString("#########0") + " AND CLIENT_ID IS NULL";
                    else
                        sRequete = "SELECT MDB FROM PNPU_MDB WHERE ID_H_WORKFLOW = " + WORKFLOW_ID.ToString("#########0") + " AND CLIENT_ID ='" + CLIENT_ID +" '";
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
                return -1;
            }
        }
    }
}
