using PNPUCore.Process;
using System;
using System.Net.Http.Headers;

namespace PNPUCore.Controle
{


    interface IControle
    {
        string MakeControl();
        IProcess GetProcessControle();
        void SetProcessControle(IProcess value);
        string GetLibControle();
        string GetTooltipControle();
    }

    public class PControle : IControle
    {
        public string ToolTipControle { get; set; }
        public string LibControle { get; set; }
        private IProcess processControle;
        protected string ResultatErreur;


        public IProcess GetProcessControle()
        {
            return processControle;
        }

        public string GetLibControle()
        { 
            return LibControle; 
        }

        public string GetTooltipControle()
        { 
            return ToolTipControle; 
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
