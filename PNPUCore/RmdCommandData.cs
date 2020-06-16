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
        String idObject;
        IProcess process;

        public RmdCommandData(string idPackage, string idClass, string idObject, string cmdCode, IProcess process)
        {
            this.idPackage = idPackage;
            this.idClass = idClass;
            this.cmdCode = cmdCode;
            this.idObject = idObject;
            this.process = process;
        }

        public string IdPackage { get => idPackage; set => idPackage = value; }
        public string IdClass { get => idClass; set => idClass = value; }
        public string CmdCode { get => cmdCode; set => cmdCode = value; }
        public string IdObject { get => cmdCode; set => cmdCode = value; }
        public IProcess Process { get => process; set => process = value; }
    }
}

