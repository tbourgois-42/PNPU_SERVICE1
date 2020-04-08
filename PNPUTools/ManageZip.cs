using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace PNPUTools.ZIP
{
    /// <summary>
    /// Cette classe permet de gérer simplement la compression et décompression de fichiers ZIP.
    /// </summary>
    public class ManageZip
    {

        /// <summary>
        /// Compression des fichiers d'un dossier dans un fichier zip.
        /// </summary>
        /// <param name="pDossierAZiper">Chemin du dossier à ziper.</param>
        /// <param name="pNomFichierZip">Nom du fichier zip à générer</param>
        /// <param name="pFiltre">Filtre à appliquer sur les noms de fichiers. Par défaut prend tous les fichiers.</param>
        /// <returns>Retourne 0 si ok, -1 en cas de problème.</returns>
        static public int CompresseDossier(string pDossierAZiper, string pNomFichierZip, string pFiltre="*.*")
        {
            try
            {
                string[] tsListeFichiers = Directory.GetFiles(pDossierAZiper,pFiltre);

                return (CompresseListeFichiers(tsListeFichiers, pNomFichierZip));

            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Compression des fichiers donnés dans un tableau dans un fichier zip.
        /// </summary>
        /// <param name="pListeFichiers">Liste des fichiers à ziper.</param>
        /// <param name="pNomFichierZip">Nom du fichier zip à générer</param>
        /// <returns>Retourne 0 si ok, -1 en cas de problème.</returns>
        static public int CompresseListeFichiers(string[] pListeFichiers, string pNomFichierZip)
        {
            try
            {
                 if (File.Exists(pNomFichierZip) == true)
                    File.Delete(pNomFichierZip);

                using (ZipOutputStream ZipStream = new ZipOutputStream(File.Create(pNomFichierZip)))
                {
                    string sNomFichierDansZip;
                    ZipStream.SetLevel(5); // Compression moyenne

                    // Parcours des fichiers contenus dans le dossier
                    foreach (string sNomFichier in pListeFichiers)
                    {
                        sNomFichierDansZip = Path.GetFileName(sNomFichier); // Pour l'archive on ne garde que le nom, pas le chemin.
                        ZipEntry zipEntry = new ZipEntry(sNomFichierDansZip);
                        ZipStream.PutNextEntry(zipEntry);

                        // Lecture du fichier et écriture dans le ZIP
                        using (FileStream fs = File.OpenRead(sNomFichier))
                        {
                            int iLongueur = 0;
                            byte[] tbBuffer = new byte[4096];
                            do
                            {
                                iLongueur = fs.Read(tbBuffer, 0, 4096);
                                ZipStream.Write(tbBuffer, 0, iLongueur);
                            } while (iLongueur > 0);

                            fs.Close();
                        }
                    }
                    ZipStream.Finish();
                    ZipStream.Close();
                }
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------
        // Compression des fichiers d'un dossier dans un fichier zip.
        //
        //
        /// <summary>
        /// Décompression des fichiers d'un fichier zip dans un dossier.
        /// </summary>
        /// <param name="pNomFichierZip">Nom du fichier zip à décompresser.</param>
        /// <param name="pDossierDestination">Nom du dossier dans lequel les fichiers du ZIP doivent être copiés.</param>
        /// <returns>Retourne 0 si ok, -1 en cas de problème.</returns>
        static public int DecompresseDansDossier(string pNomFichierZip, string pDossierDestination)
        {
            try
            {
                if (Directory.Exists(pDossierDestination) == false)
                    Directory.CreateDirectory(pDossierDestination);

                using (ZipInputStream ZipStream = new ZipInputStream(File.OpenRead(pNomFichierZip)))
                {
                    ZipEntry zipEntry = ZipStream.GetNextEntry();
                    while (zipEntry != null)
                    {
                        if (zipEntry.IsFile == true)
                        {
                            FileInfo fiFileInfo = new FileInfo(pDossierDestination + "\\" + zipEntry.Name);
                            Directory.CreateDirectory(fiFileInfo.DirectoryName);

                            using (FileStream fs = new FileStream(fiFileInfo.FullName, FileMode.Create))
                            {
                                int iLongueur = 0;
                                byte[] tbBuffer = new byte[4096];
                                do
                                {
                                    iLongueur = ZipStream.Read(tbBuffer, 0, 4096);
                                    fs.Write(tbBuffer, 0, iLongueur);
                                } while (iLongueur > 0);
                                fs.Flush();
                                fs.Close();

                            }

                        }

                        zipEntry = ZipStream.GetNextEntry();
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
