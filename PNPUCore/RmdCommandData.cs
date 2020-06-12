using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNPUCore.Process
{
    class RmdCommandData
    {

        String idPackage;
        String idClass;
        String cmdCode;

        public RmdCommandData(string idPackage, string idClass, string cmdCode)
        {
            this.idPackage = idPackage;
            this.idClass = idClass;
            this.CmdCode = cmdCode;
        }

        public string IdPackage { get => idPackage; set => idPackage = value; }
        public string IdClass { get => idClass; set => idClass = value; }
        public string CmdCode { get => cmdCode; set => cmdCode = value; }
    }
}

