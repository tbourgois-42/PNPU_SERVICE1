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
            // MDU - 17/06/2020 - Ajout de ID_H_WORKFLOW = 1 pour la compilation (à modifier par la personne ayant créé cette classe)
            RamdlTool test = new RamdlTool("110", 3, 1);
            test.AnalyseMdbRAMDL();

        }
    }
}
