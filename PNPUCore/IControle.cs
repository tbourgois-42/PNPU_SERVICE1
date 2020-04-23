using PNPUCore.Process;
using System;
using System.Collections.Generic;
using System.Text;

namespace PNPUCore.Controle
{


    interface IControle
    {

        Boolean MakeControl();
    }

    public class PControle : IControle
    {
        private IProcess processControle;

        public IProcess GetProcessControle()
        {
            return processControle;
        }

        public void SetProcessControle(IProcess value)
        {
            processControle = value;
        }

        public bool MakeControl()
        {
            throw new NotImplementedException();
        }
    }
}
