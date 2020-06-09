using PNPUCore.RapportTNR;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PNPUCore.Controle
{
    class ControleTNR : PControle, IControle
    {
        private PNPUCore.Process.ProcessTNR Process;

        /// <summary>  
        /// Constructeur de la classe. 
        /// </summary>  
        /// <param name="pProcess">Process qui a lancé le contrôle. Permet d'accéder aux méthodes et attributs publics de l'objet lançant le contrôle.</param>
        public ControleTNR(PNPUCore.Process.IProcess pProcess)
        {
            Process = (PNPUCore.Process.ProcessTNR)pProcess;
        }

        /// <summary>
        /// Add item with difference value into the rapportEcarts class
        /// </summary>
        /// <param name="rapportEcarts"></param>
        /// <param name="drRow"></param>
        /// <param name="index"></param>
        /// <param name="sDate"></param>
        /// <param name="sNewOrDelete"></param>
        internal void AddItemEcart(Ecarts rapportEcarts, DataRow drRow, int index, DateTime sDate, string sNewOrDelete)
        {
            rapportEcarts.Name = drRow[4] + " - " + drRow[3];
            rapportEcarts.ValueAfter = GetAgregateItemValue(drRow[1].ToString(), drRow[2].ToString(), sDate, ParamAppli.ConnectionStringBaseQA2);
            rapportEcarts.ValueBefore = GetAgregateItemValue(drRow[1].ToString(), drRow[2].ToString(), sDate, ParamAppli.ConnectionStringBaseQA1);
            rapportEcarts.Difference = GetAgregateDifferenceItemValue(rapportEcarts.ValueBefore, rapportEcarts.ValueAfter);
            rapportEcarts.Comment = (index == -1) ? sNewOrDelete : "";
        }

        /// <summary>
        /// Set CORRECT statut to the classification if the statut was not already set to WARNING statut
        /// </summary>
        /// <param name="rapportClassification"></param>
        internal void SetStatusCorrect(Classification rapportClassification)
        {
            // If result has been already set to WARNING we don't do anything
            if (rapportClassification.Result != ParamAppli.TranscoSatut["WARNING"])
            {
                rapportClassification.Result = ParamAppli.TranscoSatut["CORRECT"];
            }
        }

        internal Dictionary<string, string> GetListOfCumulativeTable(DataSet ItemsNoeudReadTNR)
        {
            int index = 2;
            Dictionary<string, string> lstCumulativeTable = new Dictionary<string, string>();

            foreach (DataRow drRow in ItemsNoeudReadTNR.Tables[0].Rows)
            {
                if (lstCumulativeTable.ContainsValue(drRow[2].ToString()) == false && drRow[2].ToString() != "M4SCO_VE_AC_HR_PERIOD")
                {
                    lstCumulativeTable.Add(SetAlias(index), drRow[2].ToString());
                }
                index ++;
            }
            return lstCumulativeTable;
        }

        /// <summary>
        /// Set WARNING statut to the classification and all parent's class
        /// </summary>
        /// <param name="globalResult"></param>
        /// <param name="rapportDomaine"></param>
        /// <param name="rapportSousDomaine"></param>
        /// <param name="rapportSousDomaineParts"></param>
        /// <param name="rapportClassification"></param>
        /// <returns></returns>
        internal void SetStatusWarning(RTNR RapportTNR, Domaine rapportDomaine, SousDomaine rapportSousDomaine, SousDomaineParts rapportSousDomaineParts, Classification rapportClassification)
        {
            RapportTNR.Result = ParamAppli.TranscoSatut["WARNING"];
            rapportDomaine.Result = ParamAppli.TranscoSatut["WARNING"];
            rapportSousDomaine.Result = ParamAppli.TranscoSatut["WARNING"];
            rapportSousDomaineParts.Result = ParamAppli.TranscoSatut["WARNING"];
            rapportClassification.Result = ParamAppli.TranscoSatut["WARNING"];
        }

        /// <summary>
        /// MoveTo the existing classification
        /// </summary>
        /// <param name="rapportClassification"></param>
        /// <param name="lstClassification"></param>
        /// <param name="drRow"></param>
        /// <returns>Return the classification of the item</returns>
        internal Classification MoveToClassification(Classification rapportClassification, Dictionary<string, Classification> lstClassification, DataRow drRow)
        {
            return rapportClassification = lstClassification[drRow[6].ToString()];
        }

        /// <summary>
        /// Create a new classification
        /// </summary>
        /// <param name="rapportClassification"></param>
        /// <param name="drRow"></param>
        /// <param name="lstClassification"></param>
        /// <param name="RapportSousDomaineParts"></param>
        internal void CreateClassification(Classification rapportClassification, DataRow drRow, Dictionary<string, Classification> lstClassification, SousDomaineParts RapportSousDomaineParts)
        {
            string ClassificationResult = ParamAppli.StatutOk;

            rapportClassification.Name = CheckClassificationName(drRow[6].ToString());
            rapportClassification.Result = ClassificationResult;
            lstClassification.Add(drRow[6].ToString(), rapportClassification);
            RapportSousDomaineParts.Classification.Add(rapportClassification);
        }

        /// <summary>
        /// Check difference by cumulative line (id hr / id period) between TNR database and REF database.
        /// Build Matricule object with list of matricules by item.
        /// </summary>
        /// <param name="RapportEcarts"></param>
        /// <param name="itemValuesBaseQA1"></param>
        /// <param name="itemValuesBaseQA2"></param>
        /// <param name="drRow"></param>
        /// <param name="sDate"></param>
        internal void CheckDifference(Ecarts RapportEcarts, DataSet itemValuesBaseQA1, DataSet itemValuesBaseQA2, DataRow drRow, DateTime sDate)
        {
            if (isEmptyDataSet(itemValuesBaseQA1) == false && isEmptyDataSet(itemValuesBaseQA2) == false)
            {
                int indexTNR = 0;
                
                foreach (DataRow drRowQA2 in itemValuesBaseQA2.Tables[0].Rows)
                {
                    DataRow drRowQA1 = itemValuesBaseQA1.Tables[0].Rows[indexTNR];

                    if (drRowQA1.ItemArray[7].ToString() != drRowQA2.ItemArray[7].ToString())
                    {
                        Matricules RapportMatricules = new Matricules();

                        RapportMatricules = AddEmloyeeData(RapportMatricules, sDate, drRowQA1, drRowQA2);

                        RapportEcarts.Matricules.Add(RapportMatricules);
                    }
                    indexTNR++;
                }
            }
        }

        private Matricules AddEmloyeeData(Matricules RapportMatricules, DateTime sDate, DataRow drRowQA1, DataRow drRowQA2)
        {
            RapportMatricules.Dtpaie = sDate;
            RapportMatricules.Dtalloc = Convert.ToDateTime(drRowQA2[1]);
            RapportMatricules.Idorga = drRowQA2[2].ToString();
            RapportMatricules.Societe = drRowQA2[3].ToString();
            RapportMatricules.Etablissement = drRowQA2[4].ToString();
            RapportMatricules.Matricule = drRowQA2[5].ToString();
            RapportMatricules.Periode = Convert.ToDecimal(drRowQA2[6]);
            RapportMatricules.ValueBefore = Decimal.Round(Convert.ToDecimal(drRowQA1[7]), 2);
            RapportMatricules.ValueAfter = Decimal.Round(Convert.ToDecimal(drRowQA2[7]), 2);
            RapportMatricules.Difference = Decimal.Round(RapportMatricules.ValueAfter - RapportMatricules.ValueBefore, 2);

            return RapportMatricules;
        }

        /// <summary>
        /// Check if DataSet is empty or not.
        /// </summary>
        /// <param name="itemValues"></param>
        /// <returns>Return true if DataSet is Empty, false if not.</returns>
        private bool isEmptyDataSet(DataSet itemValues)
        {
            return (itemValues == null) ? true : false;
        }

        /// <summary>  
        /// Check if item into TNR read node exist into REF read node. 
        /// </summary>  
        /// <param name="dataTableREF">REF Read Node</param>
        /// <param name="dataTableTNR">TNR Read Node</param>
        /// <param name="item">Searched item</param>
        /// <returns>Return the index of the item or -1 if not exist.</returns>
        internal int FindIndexOfBaseRef(string item, DataSet ItemsNoeudReadREF, DataSet ItemsNoeudReadTNR)
        {
            int index = -1;

            DataTable dataTableREF = ItemsNoeudReadREF.Tables[0];
            DataTable dataTableTNR = ItemsNoeudReadTNR.Tables[0];

            dataTableREF.PrimaryKey = new[] { dataTableREF.Columns["ID_REAL_FIELD"] };
            dataTableTNR.PrimaryKey = new[] { dataTableTNR.Columns["ID_REAL_FIELD"] };

            index = dataTableREF.Rows.IndexOf(dataTableREF.Rows.Find(item));

            return index;
        }

        /// <summary>  
        /// Check if item into REF read node exist into TNR read node. 
        /// </summary>  
        /// <param name="dataTableREF">REF Read Node</param>
        /// <param name="dataTableTNR">TNR Read Node</param>
        /// <param name="item">Searched item</param>
        /// <returns>Return the index of the item or -1 if not exist.</returns>
        internal int FindIndexOfBaseTnr(string item, DataSet ItemsNoeudReadREF, DataSet ItemsNoeudReadTNR)
        {
            int index = -1;

            DataTable dataTableREF = ItemsNoeudReadREF.Tables[0];
            DataTable dataTableTNR = ItemsNoeudReadTNR.Tables[0];

            dataTableREF.PrimaryKey = new[] { dataTableREF.Columns["ID_REAL_FIELD"] };
            dataTableTNR.PrimaryKey = new[] { dataTableTNR.Columns["ID_REAL_FIELD"] };

            index = dataTableTNR.Rows.IndexOf(dataTableTNR.Rows.Find(item));

            return index;
        }

        /// <summary>
        /// Check if classification is null or empty
        /// </summary>
        /// <param name="sClassification"></param>
        /// <returns>Return "SANS CLASSIFICATION" if the classification equals to null or empty</returns>
        internal string CheckClassificationName(string sClassification)
        {
            return (string.IsNullOrEmpty(sClassification)) ? "SANS CLASSIFICATION" : sClassification;
        }

        /// <summary>
        /// Set Alias Table
        /// </summary>
        /// <param name="sTable"></param>
        /// <returns></returns>
        public string SetAlias(int index)
        {
            return "CO" + index;
        }

        public string GetAlias(string table, Dictionary<string, string>  lstCumulativeTable)
        {
            string sAlias = string.Empty;

            foreach (KeyValuePair<string, string> CumulativeTable in lstCumulativeTable)
            {
                if (CumulativeTable.Value == table)
                {
                    sAlias = CumulativeTable.Key;
                }
            }
            return sAlias;
        }

        /// <summary>
        /// Get item's agregate value from the database passed in parameters
        /// </summary>
        /// <param name="sItem"></param>
        /// <param name="sTable"></param>
        /// <param name="sDate"></param>
        /// <param name="sConnexionString"></param>
        /// <returns>Return item's agregate value</returns>
        public decimal GetAgregateItemValue(string sItem, string sTable, DateTime sDate, string sConnexionString)
        {
            decimal sValue = 0;
            string sRequest = string.Empty;

            DataManagerSQLServer dataManagerSQLServer;

            sRequest = "SELECT SUM(" + sItem + ") FROM " + sTable + " WHERE SCO_DT_PAY = {d'" + sDate.ToString("yyyy-MM-dd") + "'}";

            try
            {
                dataManagerSQLServer = new DataManagerSQLServer();

                DataSet result = dataManagerSQLServer.GetData(sRequest, sConnexionString);
                sValue = result != null ? Decimal.Round(Convert.ToDecimal(result.Tables[0].Rows[0].ItemArray[0]), 2) : 0;
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
                sValue = 0;
            }

            return sValue;
        }

        /// <summary>
        /// Get difference value between before and after value
        /// </summary>
        /// <param name="valueBefore"></param>
        /// <param name="valueAfter"></param>
        /// <returns></returns>
        public decimal GetAgregateDifferenceItemValue(decimal valueBefore, decimal valueAfter)
        {
            decimal sDifference;

            sDifference = valueAfter - valueBefore;

            return sDifference;
        }

        /// <summary>
        /// Get all items into read node from the database passed in parameters
        /// </summary>
        /// <param name="sConnectionString"></param>
        /// <returns>Return DataSet which contains all items from AUX_AC_HRPERIOD Node</returns>
        public DataSet GetItemsNoeudRead(string sConnectionString)
        {
            string sRequest = string.Empty;
            DataSet dsDataSet = null;

            DataManagerSQLServer dataManagerSQLServer;

            try
            {
                // Load list of items from AUX_AC_HRPERIOD Node
                dataManagerSQLServer = new DataManagerSQLServer();
                sRequest = "SELECT REQ2.ID_ITEM, ";
                sRequest += "REQ2.ID_REAL_FIELD, ";
                sRequest += "REQ2.ID_REAL_OBJECT, ";
                sRequest += "REQ2.ID_TRANS_ITEMFRA, ";
                sRequest += "REQ2.ID_SYNONYM, ";
                sRequest += "REQ1.EXE_GROUP, ";
                sRequest += "REQ2.ID_CLASSIFICATION ";
                sRequest += "FROM ";
                sRequest += "( SELECT MAX(I2.EXE_GROUP) as c1,'-1' as c2,'ZZZZ' as c3,'-1' as c4,'ZZZZ' as C5,MAX(I2.EXE_GROUP)  EXE_GROUP , I2.ID_ITEM  ID_ITEM FROM M4RCH_ITEMS I2 WHERE I2.ID_TI LIKE '%HRPERIOD_CALC%' GROUP BY I2.ID_ITEM  )  REQ1 , ";
                sRequest += "( SELECT I.ID_ITEM  ID_ITEM, ";
                sRequest += "F.ID_REAL_FIELD  ID_REAL_FIELD, ";
                sRequest += "F.ID_REAL_OBJECT  ID_REAL_OBJECT, ";
                sRequest += "I.ID_M4_TYPE  ID_M4_TYPE, ";
                sRequest += "I.ID_TRANS_ITEMFRA  ID_TRANS_ITEMFRA, ";
                sRequest += "I.ID_SYNONYM  ID_SYNONYM, ";
                sRequest += "MIN(P.ID_CLASSIFICATION)  ID_CLASSIFICATION ";
                sRequest += "FROM M4VCH_ITEMS_READ  I ";
                sRequest += "INNER JOIN M4RDC_REAL_FIELDS  F ";
                sRequest += "ON I.ID_READ_FIELD = F.ID_FIELD and I.ID_READ_OBJECT = F.ID_OBJECT ";
                sRequest += "LEFT JOIN M4RCH_PICOMPONENTS  C ";
                sRequest += "ON C.ID_ITEM = I.ID_ITEM ";
                sRequest += "LEFT JOIN M4RCH_PAYROLL_ITEM  P ";
                sRequest += "ON P.ID_PAYROLL_ITEM = C.ID_PAYROLL_ITEM ";
                sRequest += "WHERE I.ID_TI Like '%AUX_AC_HRPERIOD' ";
                sRequest += "and F.ID_REAL_FIELD NOT IN ('SCO_OR_HR_PERIOD','SCO_ID_HR','ID_ORGANIZATION','SCO_DT_PAY','SCO_DT_ALLOC','SCO_DT_START_SLICE','SCO_PAY_FREQ_PAY','SCO_PAY_FREQ_ALLOC','ID_CURRENCY') ";
                sRequest += "and I.ID_M4_TYPE IN (6,8,9) ";
                sRequest += "GROUP BY I.ID_ITEM, F.ID_REAL_FIELD, F.ID_REAL_OBJECT, I.ID_M4_TYPE,I.ID_TRANS_ITEMFRA,I.ID_SYNONYM ";
                sRequest += ")  REQ2 ";
                sRequest += "WHERE REQ1.ID_ITEM = REQ2.ID_ITEM ";
                sRequest += "ORDER BY  REQ1.EXE_GROUP ASC ";

                dsDataSet = dataManagerSQLServer.GetData(sRequest, sConnectionString);
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
            }
            return dsDataSet;
        }

        /// <summary>
        /// Get values for item passed in parameters
        /// </summary>
        /// <param name="sItem"></param>
        /// <param name="sTable"></param>
        /// <param name="sDate"></param>
        /// <param name="sConnexionString"></param>
        /// <returns></returns>
        public DataSet GetItemValues(DataRow drRow, DateTime sDate, string sConnexionString, Dictionary<string, string> lstCumulativeTable)
        {
            string sRequest = string.Empty;
            DataSet result = null;
            string sTable = drRow[2].ToString();
            string sAlias = GetAlias(sTable, lstCumulativeTable);
            string sItem = drRow[1].ToString();

            DataManagerSQLServer dataManagerSQLServer;

            sRequest = "SELECT CO1.SCO_DT_PAY, CO1.SCO_DT_ALLOC, CO1.ID_ORGANIZATION, CO1.SCO_ID_LEG_ENT, CO1.SFR_ID_SUB_LEG_ENT, CO1.SCO_ID_HR, CO1.SCO_OR_HR_PERIOD, ";
            sRequest += sAlias + "." + sItem + " ";
            sRequest += "FROM M4SCO_AC_HR_PERIOD CO1 ";

            sRequest += BuildInnerJoinRequest(sAlias, sTable);

            sRequest += "WHERE CO1.SCO_DT_PAY = { d'" + sDate.ToString("yyyy-MM-dd") + "'} ";
            sRequest += "GROUP BY CO1.SCO_DT_PAY, CO1.SCO_DT_ALLOC, CO1.ID_ORGANIZATION, CO1.SCO_ID_LEG_ENT, CO1.SFR_ID_SUB_LEG_ENT, CO1.SCO_ID_HR, CO1.SCO_OR_HR_PERIOD, ";
            sRequest += sAlias + "." + sItem + " ";
            sRequest += "ORDER BY CO1.SCO_DT_PAY DESC, CO1.SCO_DT_ALLOC DESC,  CO1.SCO_ID_HR DESC, CO1.SCO_OR_HR_PERIOD DESC ";

            try
            {
                dataManagerSQLServer = new DataManagerSQLServer();

                result = dataManagerSQLServer.GetData(sRequest, sConnexionString);
            }
            catch (Exception ex)
            {
                Logger.Log(Process, this, ParamAppli.StatutError, ex.Message);
                result = null;
            }

            return result;
        }

        private string BuildInnerJoinRequest(string sAlias, string sTable)
        {
            string sRequest = string.Empty;

            sRequest = "INNER JOIN " + sTable + " " + sAlias + " ON CO1.ID_ORGANIZATION = " + sAlias + ".ID_ORGANIZATION ";
            sRequest += "AND CO1.SCO_ID_HR = " + sAlias + ".SCO_ID_HR ";
            sRequest += "AND CO1.SCO_OR_HR_PERIOD = " + sAlias + ".SCO_OR_HR_PERIOD ";
            sRequest += "AND CO1.SCO_DT_PAY = " + sAlias + ".SCO_DT_PAY ";
            sRequest += "AND CO1.SCO_DT_ALLOC = " + sAlias + ".SCO_DT_ALLOC ";
            sRequest += "AND CO1.SCO_DT_START_SLICE = " + sAlias + ".SCO_DT_START_SLICE ";
            sRequest += "AND CO1.SCO_PAY_FREQ_PAY = " + sAlias + ".SCO_PAY_FREQ_PAY ";
            sRequest += "AND CO1.SCO_PAY_FREQ_ALLOC = " + sAlias + ".SCO_PAY_FREQ_ALLOC ";
            sRequest += "AND CO1.ID_CURRENCY = " + sAlias + ".ID_CURRENCY ";

            return sRequest;
        }
    }
}
