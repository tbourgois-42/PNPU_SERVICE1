using PNPUCore.Control;
using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;

namespace PNPUCore.Process
{
    class ProcessMock : IProcess
    {
        private readonly object listOfMockControl;

        public void ExecuteMainProcess()
        {
            List<IControle> listControl =  ListControls.listOfMockControl;

            
        }

        public string FormatReport()
        {
            return "{OUAH MAIS QUEL TALENT!}";
        }

        internal static IProcess CreateProcess()
        {
            return new ProcessMock();
        }
    }
}
