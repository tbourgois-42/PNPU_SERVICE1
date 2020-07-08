using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace PNPUTools.DataManager
{
    public abstract class IDataManager
    {

        abstract public DataSet GetData(string sRequest, string sConnexion);


        /*public DataSet GetData(string sRequest, string sMdbPath)
        {
            return null;
        }*/
   
        /// <summary>
        /// Donne la valeur convertie en chaine de caractères du champ passé en paramètre.
        /// </summary>
        /// <param name="pDataRow">Ligne sur laquelle on veut lire la valeur.</param>
        /// <param name="pDataTable">Table lue.</param>
        /// <param name="pFieldName">Nom du champ dont on veut connaitre la valeur.</param>
        /// <returns>Valeur convertie en chaine.</returns>
        public string GetFieldValue(DataRow pDataRow, DataTable pDataTable, string pFieldName)
        {
            string sResultat = string.Empty;

            try
            {
                int iIndex = pDataTable.Columns.IndexOf(pFieldName);
                if (iIndex >= 0)
                    sResultat = pDataRow[iIndex].ToString();
            }
            catch (Exception)
            {
                sResultat = string.Empty;
            }

            return sResultat;
        }

        /// <summary>
        /// Compare les colonnes de 2 tables.
        /// </summary>
        /// <param name="Table1">Première table de la comparaison.</param>
        /// <param name="Table2">Deuxième table de la comparaison.</param>
        /// <returns>Retourne vrai si même colonnes, faux sinon.</returns>
        public bool CompareColumns(DataTable Table1, DataTable Table2)
        {
            bool bResulat = true;
            int iCpt = 0;

            try
            {
                if (Table1.Columns.Count != Table2.Columns.Count)
                    bResulat = false;
                else
                {
                    while ((iCpt < Table1.Columns.Count) && (bResulat == true))
                    {
                        if (Table1.Columns[iCpt].ColumnName != Table2.Columns[iCpt].ColumnName)
                            bResulat = false;
                        else
                            iCpt++;
                    }
                }
            }
            catch (Exception)
            {
                bResulat = false;
            }
            return (bResulat);
        }

        /// <summary>
        /// Compare les valeurs des champs de 2 tables.
        /// </summary>
        /// <param name="pDataRow1">Ligne de la première table.</param>
        /// <param name="Table1">Première table.</param>
        /// <param name="pDataRow2">Ligne de la deuxième table.</param>
        /// <param name="Table2">Deuxième table</param>
        /// <param name="dListeTablesFieldsIgnore">Liste des champs techniques à ignorer pour la comparaison</param>
        /// <returns>Retourne vrai si tous les champs des 2 tables ont la même valeur, faux sinon</returns>
        public bool CompareRows(DataRow pDataRow1, DataTable Table1, DataRow pDataRow2, DataTable Table2, Dictionary<string, List<string>> dListeTablesFieldsIgnore)
        {
            int iCpt = 0;
            bool bResulat = true;
            string sValeur1;
            string sValeur2;
            string sNomTable;
            string sNomChamp;

            try
            {
                if (CompareColumns(Table1, Table2) == true)
                {
                    sNomTable = Table1.TableName;
                    while ((iCpt < Table1.Columns.Count) && (bResulat == true))
                    {
                        sNomChamp = Table1.Columns[iCpt].ColumnName;
                        if (dListeTablesFieldsIgnore[sNomTable].Contains(sNomChamp) == false)
                        {
                            sValeur1 = pDataRow1[iCpt].ToString();
                            sValeur2 = pDataRow2[iCpt].ToString();
                            if (sValeur1 != sValeur2)
                                bResulat = false;
                        }
                        iCpt++;
                    }
                }
                else
                    bResulat = false;
            }
            catch(Exception)
            {
                bResulat = false;
            }
            return (bResulat);
        }
        /// <summary>
        /// Compare les valeurs des champs de 2 tables.
        /// </summary>
        /// <param name="pDataRow1">Ligne de la première table.</param>
        /// <param name="Table1">Première table.</param>
        /// <param name="pDataRow2">Ligne de la deuxième table.</param>
        /// <param name="Table2">Deuxième table</param>
        /// <param name="dListeTablesFieldsIgnore">Liste des champs techniques à ignorer pour la comparaison</param>
        /// <param name="ListColumnsDif">Au retour contient la liste des champs en différences</param>
        /// <returns>Retourne vrai si tous les champs des 2 tables ont la même valeur, faux sinon</returns>
        public bool CompareRows(DataRow pDataRow1, DataTable Table1, DataRow pDataRow2, DataTable Table2, Dictionary<string, List<string>> dListeTablesFieldsIgnore, ref List<string> ListColumnsDif)
        {
            int iCpt = 0;
            bool bResulat = true;
            string sValeur1;
            string sValeur2;
            string sNomTable;
            string sNomChamp;

            try
            {
                ListColumnsDif.Clear();
                if (CompareColumns(Table1, Table2) == true)
                {
                    sNomTable = Table1.TableName;
                    while (iCpt < Table1.Columns.Count)
                    {
                        sNomChamp = Table1.Columns[iCpt].ColumnName;
                        if (dListeTablesFieldsIgnore[sNomTable].Contains(sNomChamp) == false)
                        {
                            sValeur1 = pDataRow1[iCpt].ToString();
                            sValeur2 = pDataRow2[iCpt].ToString();
                            if (sValeur1 != sValeur2)
                            {
                                bResulat = false;
                                ListColumnsDif.Add(sNomChamp);
                            }
                        }
                        iCpt++;
                    }
                }
                else
                    bResulat = false;
            }
            catch (Exception)
            {
                bResulat = false;
            }
            return (bResulat);
        }

        public Dictionary<string, List<string>> GetIgnoredFields(string sConnectionInfo)
        {
            Dictionary<string, List<string>> dListeTables = new Dictionary<string, List<string>>();
            DataSet dsDataSet;
            string sTableCourrante = string.Empty;
            List<string> sListeChamps = new List<string>();
            string sRequete = "select A.ID_REAL_OBJECT, A.ID_REAL_FIELD from M4RDC_REAL_FIELDS A,M4RDC_FIELDS B where A.ID_OBJECT=B.ID_OBJECT AND A.ID_FIELD=B.ID_FIELD AND B.OWNER_FLAG = 1 AND B.ID_INTERNAL_FIELD IN('6', '30', '31', '64') ORDER BY 1,2";


            dsDataSet = this.GetData(sRequete, sConnectionInfo);
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    if (sTableCourrante != drRow[0].ToString())
                    {
                        if (sListeChamps.Count > 0)
                        {
                            dListeTables.Add(sTableCourrante, sListeChamps);
                            sListeChamps.Clear();
                        }
                        sTableCourrante = drRow[0].ToString();
                    }
                    sListeChamps.Add(drRow[1].ToString());
                }
            }

            return dListeTables;
        }

        public List<string> GetPersonnalTables(string sConnectionInfo)
        {
            List<string> lListPersoTables = new List<string>();

            DataSet dsDataSet = this.GetData("select DISTINCT ID_REAL_OBJECT from M4RDC_REAL_FIELDS where ID_REAL_FIELD LIKE '%ID_HR' OR ID_REAL_FIELD LIKE '%ID_PERSON'", sConnectionInfo);
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    lListPersoTables.Add(drRow[0].ToString());
                }
            }

            return lListPersoTables;
        }

        public string GetTableName(string sRequest)
        {
            string sResultat = string.Empty;
            int iIndex1;
            int iIndex2;

            try
            {
                sRequest = sRequest.ToUpper();
                iIndex1 = sRequest.IndexOf("FROM");
                if (iIndex1 >= 0)
                {
                    iIndex1 += "FROM".Length;
                    while (char.IsWhiteSpace(sRequest[iIndex1]) == true) iIndex1++;
                    iIndex2 = iIndex1 + 1;
                    while ((char.IsLetterOrDigit(sRequest[iIndex2]) == true) || (sRequest[iIndex2] == '_')) iIndex2++;
                    sResultat = sRequest.Substring(iIndex1, iIndex2 - iIndex1);
                }
            }
            catch(Exception)
            {
                sResultat = string.Empty;
            }
            return (sResultat);

        }

        public string GenerateReplace(string sTable, string sFilter, string sOrgaOrg, string sOrgaDest)
        {
            string sResultat = string.Empty;

            if (sFilter.Contains("ID_ORGANIZATION") == false)
                sFilter = " AND ID_ORGANIZATION='" + sOrgaDest + "'";
            sResultat = "Replace " + sTable + "  from origin to destination where \"" + sFilter + "\"";
            sResultat = ReplaceID_ORGA(sResultat, sOrgaOrg, sOrgaDest);

            return sResultat;
        }


        public void GetPKFields(string sTable, string sConnectionString, ref List<string> lPKFields)
        {
            DataManagerSQLServer dataManagerSQL = new DataManagerSQLServer();
            DataSet dsDataSet;
            string sRequete = "SELECT B.ID_REAL_FIELD FROM M4RDC_FIELDS A,M4RDC_REAL_FIELDS B where B.ID_REAL_OBJECT = '" + sTable + "' AND B.ID_OBJECT=A.ID_OBJECT AND B.ID_FIELD=A.ID_FIELD AND A.POS_PK>0 AND A.ID_TYPE NOT LIKE '%DATE%' order by A.POS_PK";
            lPKFields.Clear();

            dsDataSet = dataManagerSQL.GetData(sRequete, sConnectionString);
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                {
                    lPKFields.Add(drRow[0].ToString());
                }
            }

        }

        public bool ExistsField(DataTable dtTable, string sField)
        {
            bool bResult = false;

            foreach (DataColumn dcColumn in dtTable.Columns)
            {
                if (dcColumn.ColumnName == sField)
                {
                    bResult = true;
                    break;
                }
            }

            return bResult;
        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'un script SQL
        /// </summary>
        /// <param name="sCommand">Script SQL à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est le script</param>
        /// <param name="sFilter">En sortie contient le filtre contenue dans le script</param>
        public void ExtractTableFilter(string sCommand, ref string sTable, ref string sFilter, ref List<string> lColumnsList)
        {
            lColumnsList.Clear();
            if (sCommand.Contains("M4SFR_COPY_DATA_ORG") == true)
            {
                ExtractTableFilterCOPY_DATA(sCommand, ref sTable, ref sFilter);
            }
            else if (sCommand.Contains("REPLACE") == true)
            {
                ExtractTableFilterREPLACE(sCommand, ref sTable, ref sFilter);
            }
            else if (sCommand.ToUpper().Contains("DELETE") == true)
            {
                ExtractTableFilterDELETE(sCommand, ref sTable, ref sFilter);
            }
            else if (sCommand.ToUpper().Contains("UPDATE") == true)
            {
                ExtractTableFilterUPDATE(sCommand, ref sTable, ref sFilter, ref lColumnsList);
            }
            else
            {
                sTable = String.Empty;
                sFilter = string.Empty;
            }
        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'une commande REPLACE
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la suppression</param>
        /// <param name="sFilter">En sortie contien le filtre du script</param>
        public void ExtractTableFilterREPLACE(string sCommand, ref string sTable, ref string sFilter)
        {
            int iIndex1;
            int iIndex2;
            string sCommandMAJ = sCommand.ToUpper();

            sTable = String.Empty;
            sFilter = String.Empty;

            iIndex1 = sCommandMAJ.IndexOf("REPLACE");
            if (iIndex1 >= 0)
            {
                
                iIndex1 += "REPLACE".Length;

                // Récupération du nom de la table
                while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                    iIndex1++;

                iIndex2 = iIndex1;
                while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                    iIndex2++;

                sTable = sCommand.Substring(iIndex1, iIndex2 - iIndex1);

                // Récupération du filtre
                iIndex1 = sCommandMAJ.IndexOf("WHERE", iIndex2);
                if (iIndex1 >= 0)
                {
                    iIndex2 += "WHERE".Length + 1;
                    iIndex1 = sCommandMAJ.IndexOf("\"", iIndex2) + 1;
                    iIndex2 = sCommandMAJ.IndexOf("\"", iIndex1);
                    sFilter = sCommand.Substring(iIndex1, iIndex2- iIndex1);
                    sFilter = sFilter.Trim();
                }

            }

        }
        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'un script UPDATE
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la suppression</param>
        /// <param name="sFilter">En sortie contien le filtre du script</param>
        public void ExtractTableFilterUPDATE(string sCommand, ref string sTable, ref string sFilter, ref List<string> lColumnsList)
        {
            int iIndex1;
            int iIndex2;
            string sCommandMAJ = sCommand.ToUpper();
            string sColumn;

            sTable = String.Empty;
            sFilter = String.Empty;

            iIndex1 = sCommandMAJ.IndexOf("UPDATE");
            if (iIndex1 >= 0)
            {
                iIndex1 += "UPDATE".Length;
                // Récupération du nom de la table
                while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                    iIndex1++;

                iIndex2 = iIndex1;
                while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                    iIndex2++;

                sTable = sCommand.Substring(iIndex1, iIndex2 - iIndex1);

                // Rechcerche des champs mis à jour
                iIndex1 = sCommandMAJ.IndexOf("SET", iIndex2);
                while (iIndex1 >= 0)
                {
                    iIndex1 += "SET".Length;
                    while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                        iIndex1++;

                    iIndex2 = iIndex1;
                    while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                        iIndex2++;
                    sColumn = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                    lColumnsList.Add(sColumn);
                    iIndex1 = sCommandMAJ.IndexOf("SET", iIndex2);
                }



                // Récupération du filtre
                iIndex1 = sCommandMAJ.IndexOf("WHERE", iIndex2);
                if (iIndex1 >= 0)
                {
                    // Vérification de la présence d'un FROM
                    iIndex2 = sCommandMAJ.IndexOf("FROM", iIndex2);
                    if ((iIndex2 >= 0) && (iIndex2 < iIndex1))
                    {
                        int iIndex3;
                        iIndex2 += "FROM".Length;
                        while (Char.IsWhiteSpace(sCommandMAJ[iIndex2]) == true)
                            iIndex2++;
                        iIndex3 = iIndex2;
                        while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex3]) == true) || (sCommandMAJ[iIndex3] == '_'))
                            iIndex3++;

                        sTable = sCommand.Substring(iIndex2, iIndex3 - iIndex2);
                    }

                    iIndex1 += "WHERE".Length + 1;
                    sFilter = sCommand.Substring(iIndex1);
                }
                else // Vérification de la présence d'un FROM
                {
                    iIndex2 = sCommandMAJ.IndexOf("FROM", iIndex2);
                    if (iIndex2 >= 0)
                    {
                        int iIndex3;
                        iIndex2 += "FROM".Length;
                        while (Char.IsWhiteSpace(sCommandMAJ[iIndex2]) == true)
                            iIndex2++;
                        iIndex3 = iIndex2;
                        while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex3]) == true) || (sCommandMAJ[iIndex3] == '_'))
                            iIndex3++;

                        sTable = sCommand.Substring(iIndex2, iIndex3 - iIndex2);
                    }
                }

            }

        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'un script DELETE
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la suppression</param>
        /// <param name="sFilter">En sortie contien le filtre du script</param>
        public void ExtractTableFilterDELETE(string sCommand, ref string sTable, ref string sFilter)
        {
            int iIndex1;
            int iIndex2;
            string sCommandMAJ = sCommand.ToUpper();

            sTable = String.Empty;
            sFilter = String.Empty;

            iIndex1 = sCommandMAJ.IndexOf("DELETE");
            if (iIndex1 >= 0)
            {
                // Gestion de la présence de FROM
                iIndex2 = sCommandMAJ.IndexOf("FROM", iIndex1);
                if (iIndex2 >= 0)
                    iIndex1 = iIndex2 + "FROM".Length;
                else
                    iIndex1 += "DELETE".Length;

                // Récupération du nom de la table
                while (Char.IsWhiteSpace(sCommandMAJ[iIndex1]) == true)
                    iIndex1++;

                iIndex2 = iIndex1;
                while ((Char.IsLetterOrDigit(sCommandMAJ[iIndex2]) == true) || (sCommandMAJ[iIndex2] == '_'))
                    iIndex2++;

                sTable = sCommand.Substring(iIndex1, iIndex2 - iIndex1);

                // Récupération du filtre
                iIndex1 = sCommandMAJ.IndexOf("WHERE", iIndex2);
                if (iIndex1 >= 0)
                {
                    iIndex1 += "WHERE".Length + 1;
                    sFilter = sCommand.Substring(iIndex1);
                }

            }

        }

        /// <summary>
        /// Cette méthode permet de récupérer la table et le filtre à partir d'une commande de propagation
        /// </summary>
        /// <param name="sCommand">Commande de propagation à analyser</param>
        /// <param name="sTable">En sortie contient la table sur laquelle est faite la propagation</param>
        /// <param name="sFilter">En sortie contien le filtre de la propagation</param>
        public void ExtractTableFilterCOPY_DATA(string sCommand, ref string sTable, ref string sFilter)
        {
            if (sCommand.ToUpper().IndexOf("EXEC") >= 0)
            {
                sTable = ExtractParameterCOPY_DATA(sCommand, "@table");
                sFilter = ExtractParameterCOPY_DATA(sCommand, "@opt_where").Replace("''", "'");
            }
            else
            {
                sTable = ExtractParameterCOPY_DATA(sCommand, 1);
                sFilter = ExtractParameterCOPY_DATA(sCommand, 4).Replace("''", "'");
            }

        }

        /// <summary>
        /// Cette méthode permet de récupérer la valeur d'un paramètre passé à une procédure stockée type SQL server (EXEC...)
        /// </summary>
        /// <param name="sCommand">Commande à analyser</param>
        /// <param name="sParameterName">Nom du paramètre à extraire</param>
        public string ExtractParameterCOPY_DATA(string sCommand, string sParameterName)
        {
            int iIndex1, iIndex2;
            string sResultat = String.Empty;
            bool bContinue = true;

            try
            {
                iIndex1 = sCommand.IndexOf(sParameterName);
                if (iIndex1 >= 0)
                {
                    iIndex1 += sParameterName.Length;
                    while (sCommand[iIndex1] != '\'') iIndex1++;
                    iIndex1++;
                    iIndex2 = iIndex1;
                    while (bContinue == true)
                    {
                        if (iIndex2 >= sCommand.Length)
                            return (String.Empty);

                        if (sCommand[iIndex2] == '\'')
                        {
                            if (sCommand.Substring(iIndex2, 2) == "''")
                                iIndex2 += 2;
                            else
                                bContinue = false;
                        }
                        else
                            iIndex2++;
                    }

                    sResultat = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                }

            }
            catch (Exception)
            {
                sResultat = String.Empty;
            }
            return sResultat;
        }

        /// <summary>
        /// Cette méthode permet de récupérer la valeur d'un paramètre passé à une procédure stockée type Oracle (CALL...)
        /// </summary>
        /// <param name="sCommand">Commande à analyser</param>
        /// <param name="iParameterPos">Position du paramètre à extraire</param>
        public string ExtractParameterCOPY_DATA(string sCommand, int iParameterPos)
        {
            int iIndex1 = 0;
            int iIndex2 = 0;
            string sResultat = String.Empty;
            bool bContinue = true;
            int iNumeroParam = 1;

            try
            {
                iIndex1 = sCommand.IndexOf("M4SFR_COPY_DATA_ORG");
                if (iIndex1 >= 0)
                {
                    iIndex1 += "M4SFR_COPY_DATA_ORG".Length;
                    while (sCommand[iIndex1] != '\'') iIndex1++;
                    iIndex1++;
                    while (iNumeroParam < iParameterPos)
                    {
                        iIndex1 = sCommand.IndexOf(',', iIndex1);
                        if (iIndex1 == -1)
                            return String.Empty;

                        while (sCommand[iIndex1] != '\'') iIndex1++;
                        iIndex1++;

                        // Test si on est sur '', dans le filtre par exemple
                        if (sCommand[iIndex1] != '\'')
                            iNumeroParam++;
                    }

                    if (iNumeroParam == iParameterPos)
                    {
                        iIndex2 = iIndex1;
                        while (bContinue == true)
                        {
                            if (iIndex2 >= sCommand.Length)
                                return (String.Empty);

                            if (sCommand[iIndex2] == '\'')
                            {
                                if (sCommand.Substring(iIndex2, 2) == "''")
                                    iIndex2 += 2;
                                else
                                    bContinue = false;
                            }
                            else
                                iIndex2++;
                        }
                    }

                    sResultat = sCommand.Substring(iIndex1, iIndex2 - iIndex1);
                }

            }
            catch (Exception)
            {
                sResultat = String.Empty;
            }
            return sResultat;
        }


       

        /// <summary>
        /// Methode permettant de gérer les scripts contenant un test sur la base de destination (Oracle). Elle retourne uniquement
        /// les commandes concernant le client en fonction du paramètre bOracle
        /// </summary>
        /// <param name="sCommande">Commande à analyser</param>
        /// <param name="bOracle">Vrai si la base du client est sous Oracle, faux sinon</param>
        /// <returns>Retourne la chaine de commande ne contenant que les lignes concernant le client</returns>
        public string GereOracle(string sCommande, bool bOracle)
        {
            int iIndex1 = 0;
            int iIndex2;
            string sResultat = string.Empty;
            bool bScriptOracle;
            bool bContinue = true;
            string sCommandeMaj = sCommande.ToUpper();
            string sEtiquette;


            while (bContinue == true)
            {
                iIndex1 = sCommandeMaj.IndexOf("DBMS(", iIndex1);
                if (iIndex1 >= 0)
                {
                    while ((sCommandeMaj[iIndex1] != '=') && (iIndex1 < sCommandeMaj.Length) && ((sCommandeMaj[iIndex1] != '<') || (sCommandeMaj[iIndex1 + 1] != '>')))
                        iIndex1++;

                    if (iIndex1 < sCommandeMaj.Length)
                    {
                        if (sCommandeMaj[iIndex1] == '=')
                            bScriptOracle = false;
                        else
                            bScriptOracle = true;

                        iIndex1 = sCommandeMaj.IndexOf("GOTO", iIndex1);
                        if (iIndex1 >= 0)
                        {
                            iIndex1 += "GOTO".Length;
                            while (Char.IsWhiteSpace(sCommandeMaj[iIndex1]) == true) iIndex1++;
                            iIndex2 = iIndex1;
                            while ((char.IsLetterOrDigit(sCommandeMaj[iIndex2]) == true) || (sCommandeMaj[iIndex2] == '_')) iIndex2++;
                            sEtiquette = sCommandeMaj.Substring(iIndex1, iIndex2 - iIndex1);

                            iIndex2 = sCommandeMaj.IndexOf("->" + sEtiquette, iIndex2);
                            iIndex2 = sCommandeMaj.IndexOf("*/", iIndex2);
                            iIndex2 += "*/".Length;
                            // Si on doit garder ce code
                            if (bOracle == bScriptOracle)
                            {
                                iIndex1 = sCommandeMaj.IndexOf("\\", iIndex1);
                                iIndex1++;
                                sResultat += sCommande.Substring(iIndex1, iIndex2 - iIndex1);
                            }
                            iIndex1 = iIndex2;
                            if (iIndex1 == sCommandeMaj.Length)
                                bContinue = false;
                        }
                    }
                    else
                        bContinue = false;
                }
                else
                    bContinue = false;
            }

            return sResultat;
        }

        /// <summary>
        /// Remplace dans un script SQL une ID_ORGA par une autre
        /// </summary>
        /// <param name="sCommand">Script SQL à modifier</param>
        /// <param name="sID_OrgaOrg">ID_ORGA à remplacer</param>
        /// <param name="sID_OrgaDest">Nouvel ID_ORGA</param>
        /// <returns>Retourne le script modifié</returns>
        public string ReplaceID_ORGA(string sCommand, string sID_OrgaOrg, string sID_OrgaDest)
        {
            List<string> lID_Orga = new List<string>();
            lID_Orga.Add(sID_OrgaDest);
            return (ReplaceID_ORGA(sCommand, sID_OrgaOrg, lID_Orga));
        }

        /// <summary>
        /// Remplace dans un script SQL une ID_ORGA par une liste d'ID_ORGA
        /// </summary>
        /// <param name="sCommand">Script SQL à modifier</param>
        /// <param name="sID_OrgaOrg">ID_ORGA à remplacer</param>
        /// <param name="sID_OrgaDest">Liste des nouveaux ID_ORGA</param>
        /// <returns>Retourne le script modifié</returns>
        public string ReplaceID_ORGA(string sCommand, string sID_OrgaOrg, List<string> sID_OrgaDest)
        {
            string sORGA_COPY = string.Empty;
            string sORGA_SCRIPT = string.Empty;
            bool bPremierElement = true;
            string sResultat = sCommand;

            foreach (string orga in sID_OrgaDest)
            {
                if (bPremierElement == true)
                {
                    bPremierElement = false;
                    sORGA_COPY = "'";
                    sORGA_SCRIPT = "'";
                }
                else
                {
                    sORGA_COPY += ",";
                    sORGA_SCRIPT += ",'";
                }
                sORGA_COPY += orga;
                sORGA_SCRIPT += orga + "'";
            }
            if (sORGA_COPY.Length > 0)
                sORGA_COPY += "'";

            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "(|\\s+),(|\\s+)'" + sID_OrgaOrg + "'", "," + sORGA_COPY, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "@id_orgas_dest(|\\s+)=(|\\s+)'" + sID_OrgaOrg + "'", "@id_orgas_dest = " + sORGA_COPY, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION(|\\s+)=(|\\s+)'" + sID_OrgaOrg + "'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION\\s+LIKE(|\\s+)'" + sID_OrgaOrg + "'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sResultat = System.Text.RegularExpressions.Regex.Replace(sResultat, "ID_ORGANIZATION\\s+IN(|\\s+)\\((|\\s+)'" + sID_OrgaOrg + "'(|\\s+)\\)", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return sResultat;
        }

        public string[] splitCmdCodeData(string cmdCode)
        {
            //We use an ugly text to replace \\\\ for come back to this after the split on \\
            string uglyString = "A?DJUEBISKJLERPDLZMLERJD?ZADLKJZDKD.§ZDI";
            cmdCode = cmdCode.Replace("\\\\", uglyString);
            string[] result = cmdCode.Split('\\');



            for (int i = 0; i < result.Length; i++)
            {



                result[i] = result[i].Replace(uglyString, "\\\\");
                result[i] = result[i].Trim();
                if (String.IsNullOrEmpty(result[i]))
                {
                    RemoveAt(ref result, i);
                    i--;
                }
            }
            return result;
        }



        public string removeCommentOnCommand(string cmdCode)
        {



            int indexOpenComment = cmdCode.IndexOf("/*");
            int indexCloseComment = -1;
            if (indexOpenComment != -1)
            {
                indexCloseComment = cmdCode.IndexOf("*/", indexOpenComment);
            }
            while (indexOpenComment != -1)
            {
                cmdCode = cmdCode.Remove(indexOpenComment, (indexCloseComment - indexOpenComment) + 2); //+2 to remove the / at the end of the comment
                indexOpenComment = cmdCode.IndexOf("/*");
                if (indexOpenComment != -1)
                {
                    indexCloseComment = cmdCode.IndexOf("*/", indexOpenComment);
                }
            }




            indexOpenComment = cmdCode.IndexOf("//");
            if (indexOpenComment != -1)
            {
                indexCloseComment = cmdCode.IndexOf(System.Environment.NewLine, indexOpenComment);
            }
            while (indexOpenComment != -1)
            {
                cmdCode = cmdCode.Remove(indexOpenComment, (indexCloseComment - indexOpenComment) + 2); //+2 to remove the / at the end of the comment
                indexOpenComment = cmdCode.IndexOf("//");
                if (indexOpenComment != -1)
                {
                    indexCloseComment = cmdCode.IndexOf(System.Environment.NewLine, indexOpenComment);
                }
            }



            return cmdCode;
        }



        public void RemoveAt<T>(ref T[] arr, int index)
        {
            for (int a = index; a < arr.Length - 1; a++)
            {
                // moving elements downwards, to fill the gap at [index]
                arr[a] = arr[a + 1];
            }
            // finally, let's decrement Array's size by one
            Array.Resize(ref arr, arr.Length - 1);
        }

       
    }
}

