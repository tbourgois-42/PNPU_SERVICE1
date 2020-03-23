using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleParamAppli : IControle
    {
        private string sPathMdb = string.Empty;
        string sCLE = string.Empty;
        string sSECTION = string.Empty;

        public ControleParamAppli(string sPPathMdb, string spCLE, string spSECTION)
        {
            sPathMdb = sPPathMdb;
            sCLE = spCLE;
            sSECTION = spSECTION;
         }

        public bool makeControl()
        {
            bool bResultat = true;
            string sCommandPack = string.Empty;


             DataManagerAccess dmaManagerAccess = null;
            try
            {
                if (sCLE != string.Empty)
                {
                    sCommandPack = "ID_KEY IN (" + sCLE + ")";
                }

                if (sSECTION != string.Empty)
                {
                    if (sCommandPack != string.Empty)
                        sCommandPack += " OR ";
                    sCommandPack += "ID_SECTION IN(" + sSECTION + ")";
                }

                if (sCommandPack != string.Empty)
                {
                    sCommandPack = "SELECT DISTINCT ID_SECTION + '\\' + ID_KEY AS CLE FROM  M4RAV_APP_VAL_LG1 WHERE " + sCommandPack;
                    dmaManagerAccess = new DataManagerAccess(sPathMdb);
                    DataSet dsDataSet = dmaManagerAccess.GetData(sCommandPack);

                    if (dsDataSet.Tables[0].Rows.Count > 0)
                    {

                        foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                        {
                            // TODO Loguer les paramètres applicatyif livrés à tort
                            bResultat = false;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO, loguer l'exception
                bResultat = false;
            }

            return bResultat;

        }
    }
}
