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
        String cmdSequence;
        IProcess process;
  
        public RmdCommandData(string idPackage, string idClass, string idObject, string cmdCode, string cmdSequence, IProcess process)
        {
            this.idPackage = idPackage;
            this.idClass = idClass;
            this.cmdCode = cmdCode;
            this.idObject = idObject;
            this.cmdSequence = cmdSequence;
            this.process = process;
        }

        public string IdPackage { get => idPackage; set => idPackage = value; }
        public string IdClass { get => idClass; set => idClass = value; }
        public string CmdCode { get => cmdCode; set => cmdCode = value; }
        public string IdObject { get => idObject; set => idObject = value; }
        public string CmdSequence { get => cmdSequence; set => cmdSequence = value; }
        public IProcess Process { get => process; set => process = value; }
    }
}

