using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleCmdInterdites : IControle
    {
        private string sPathMdb = string.Empty;
        private List<string> lL_INTERDIT;

        public ControleCmdInterdites(string sPPathMdb, List<string> L_INTERDIT)
        {
            sPathMdb = sPPathMdb;
            lL_INTERDIT = L_INTERDIT;
         }

        public bool makeControl()
        {
            bool bResultat = true;
            string sCommandPack = string.Empty;
            int iCpt = 0;
            bool bTrouve;

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess(sPathMdb);
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_PACKAGE,CMD_SEQUENCE,CMD_CODE FROM M4RDL_PACK_CMDS WHERE CMD_ACTIVE =-1");

                if (dsDataSet.Tables[0].Rows.Count > 0)
                {

                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        sCommandPack = drRow[2].ToString().ToUpper().Trim();

                        // Je remplace les espaces et tabulation par un seul espace.
                        sCommandPack = System.Text.RegularExpressions.Regex.Replace(sCommandPack, "\\s+", " ");

                        iCpt = 0;
                        bTrouve = false;
                        while ((iCpt < lL_INTERDIT.Count()) && (bTrouve == false))
                        {
                            if (sCommandPack.IndexOf(lL_INTERDIT[iCpt++]) >= 0)
                            {
                                bTrouve = true;
                                bResultat = false;
                                // TODO indiquer que le pack contient des commandes interdites
                            }
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
