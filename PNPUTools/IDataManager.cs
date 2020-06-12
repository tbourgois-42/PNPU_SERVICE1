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
    }
}
