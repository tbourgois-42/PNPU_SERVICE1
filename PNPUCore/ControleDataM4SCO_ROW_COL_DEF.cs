using PNPUCore.Process;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUCore.Rapport;

namespace PNPUCore.Controle
{
    class ControleDataM4SCO_ROW_COL_DEF
    {
        private const string TableTraitee = "M4SCO_ROW_COL_DEF";

        public ControleDataM4SCO_ROW_COL_DEF()
        {
           
        }

        public void AnalyzeCommand(RmdCommandData rmdCommandData, CommandData commandData)
        {
            string sTable;
            string sFilter;
            string sID_OrgaOrg;
            string sRequeteRef;
            string sRequeteClient;
            DataSet dsDataSetRef;
            DataSet dsDataSetClient;
            string sConnectionString;
            DataManagerSQLServer dmsDataManager = new DataManagerSQLServer();
            List<string> lID_Orga;
            string sFiltreRef;
            string sFiltreClient;
            ControleCommandData controleCommandData;


            ((ProcessAnalyseImpact)rmdCommandData.Process).ExtractTableFilter(rmdCommandData.CmdCode, out sTable, out sFilter);
            
            if (TableTraitee != sTable) return;
            lID_Orga = ParamAppli.ListeInfoClient[((ProcessAnalyseImpact)rmdCommandData.Process).CLIENT_ID].listID_ORGA;
            sConnectionString = ParamAppli.ListeInfoClient[((ProcessAnalyseImpact)rmdCommandData.Process).CLIENT_ID].ConnectionStringQA1;

            // On traite une commande de propagation
            if (rmdCommandData.CmdCode.IndexOf("M4SFR_COPY_DATA_ORG") >= 0)
            {
                foreach (string sOrgaCour in lID_Orga)
                {
                    if (sFilter.IndexOf("ID_ORGA") >= 0)
                    {
                        sFiltreRef = sFilter;
                        sFiltreClient = ((ProcessAnalyseImpact)rmdCommandData.Process).ReplaceID_ORGA(sFilter, "0001", sOrgaCour);
                    }
                    else
                    {
                        sFiltreRef = sFilter + " AND ID_ORGANIZATION='0001'";
                        sFiltreClient = sFilter + " A ID_ORGANIZATION='" + sOrgaCour + "'";
                    }

                    sRequeteRef = "SELECT * FROM " + sTable + " WHERE " + sFiltreRef;
                    sRequeteClient = "SELECT * FROM " + sTable + " WHERE " + sFiltreClient;
                    dsDataSetRef = dmsDataManager.GetData(sRequeteRef, sConnectionString);
                    dsDataSetClient = dmsDataManager.GetData(sRequeteClient, sConnectionString);

                    // Pas le même nombre de ligne, à traiter à la main.
                    if (dsDataSetRef.Tables[0].Rows.Count != dsDataSetClient.Tables[0].Rows.Count)
                    {
                        controleCommandData = new ControleCommandData();
                        controleCommandData.Message = "Nombre différents d'enregistrements entre référentiel et client. Commande à traiter à la main.";
                        controleCommandData.Result = ParamAppli.StatutWarning;
                        commandData.listControleCommandData.Add(controleCommandData);
                    }

                    for (int iIndex = 0; iIndex < dsDataSetRef.Tables[0].Rows.Count; iIndex++)
                    {
                       // if (dmsDataManager.CompareRows(dsDataSetRef.Tables[0].Rows[iIndex], dsDataSetRef.Tables[0], dsDataSetClient.Tables[0].Rows[iIndex], dsDataSetClient.Tables[0]) == false)

                    }
                }
            }
        }
    }
}
