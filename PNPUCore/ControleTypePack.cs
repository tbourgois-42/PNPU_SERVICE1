using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleTypeack : IControle
    {
        private string sPathMdb = string.Empty;
        private List<string> lCMD_L;
        private List<string> lCMD_D;
        private List<string> lCMD_F;
        private List<string> lCMD_B;

        public ControleTypeack(string sPPathMdb, List<string> CMD_L, List<string> CMD_D, List<string> CMD_F, List<string> CMD_B)
        {
            sPathMdb = sPPathMdb;
            lCMD_L = CMD_L;
            lCMD_D = CMD_D;
            lCMD_F = CMD_F;
            lCMD_B = CMD_B;
        }

        public bool makeControl()
        {
            bool bResultat = true;
            int iID_SYNONYM;

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess(sPathMdb);
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE ID_TI LIKE '%HR%CALC' AND ID_TI NOT LIKE '%DIF%' AND ID_SYNONYM <> 0");

                if (dsDataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        
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

        private bool ControleUnTypack(string TypePack, List<string> CMD)
        {
            bool bResultat = true;
            DataManagerAccess dmaManagerAccess = null;

            try
            {
                dmaManagerAccess = new DataManagerAccess(sPathMdb);
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_PACKAGE,CMD_SEQUENCE,CMD_CODE FROM M4RDL_PACK_CMDS WHERE ID_PACKAGE LIKE '%" + TypePack + "' AND CMD_ACTIVE =-1");

                if (dsDataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        
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
