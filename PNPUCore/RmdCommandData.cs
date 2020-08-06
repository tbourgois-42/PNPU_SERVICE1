using System;

namespace PNPUCore.Process
{
    class RmdCommandData
    {
        public RmdCommandData(string idPackage, string idClass, string idObject, string cmdCode, string cmdSequence, IProcess process, string idCCTTask)
        {
            this.IdPackage = idPackage;
            this.IdClass = idClass;
            this.CmdCode = cmdCode;
            this.IdObject = idObject;
            this.CmdSequence = cmdSequence;
            this.Process = process;
            this.IdCCTTask = idCCTTask;
        }

        public string IdPackage { get; set; }
        public string IdClass { get; set; }
        public string CmdCode { get; set; }
        public string IdObject { get; set; }
        public string CmdSequence { get; set; }
        public IProcess Process { get; set; }
        public string IdCCTTask { get; set; }
    }
}

