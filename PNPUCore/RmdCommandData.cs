namespace PNPUCore.Process
{
    internal class RmdCommandData
    {
        public RmdCommandData(string idPackage, string idClass, string idObject, string cmdCode, string cmdSequence, IProcess process, string idCCTTask)
        {
            IdPackage = idPackage;
            IdClass = idClass;
            CmdCode = cmdCode;
            IdObject = idObject;
            CmdSequence = cmdSequence;
            Process = process;
            IdCCTTask = idCCTTask;
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

