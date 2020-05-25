using PNPUTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            RamdlTool test = new RamdlTool("110", 3);
            test.AnalyseMdbRAMDL();

        }
    }
}
