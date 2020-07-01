using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNPUCore.Process
{
    class RmdCommandDataROWCOLDEF : RmdCommandData
    {

        public RmdCommandDataROWCOLDEF(string idPackage, string idClass, string cmdCode, IProcess process) : base (idPackage, idClass, cmdCode, process)
        {
            string sTable;
            string sFiltre;

            ((ProcessAnalyseImpact)process).ExtractTableFilter(cmdCode, out sTable, out sFiltre);

            
        }

    }
}

