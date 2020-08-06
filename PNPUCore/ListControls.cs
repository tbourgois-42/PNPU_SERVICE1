using System.Collections.Generic;

namespace PNPUCore.Controle
{
    static class ListControls
    {
        internal static List<IControle> listOfMockControl = new List<IControle>() { new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock(), new ControleMock() };
    }
}