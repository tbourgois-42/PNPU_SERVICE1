using PNPUCore.Process;
using System;
using System.Collections.Generic;
using System.Text;

namespace PNPUCore.Controle
{


    interface IControle
    {
        string MakeControl();
        IProcess GetProcessControle();
        void SetProcessControle(IProcess value);
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

        public string MakeControl()
        {
            throw new NotImplementedException();
        }
    }
}
