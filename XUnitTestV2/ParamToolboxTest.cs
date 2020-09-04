using PNPUCore;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;
using Xunit;

namespace XUnitTest
{

    public class ParamToolboxTest
    {



        [Fact]
        public void SouldGetClientTrigramGLS()
        {
            ParamToolbox paramToolbox = new ParamToolbox();

            Assert.Equal("GLS", paramToolbox.GetClientTrigram("26"));
        }

        [Fact]
        public void SouldGetClientTrigramNotEqual()
        {
            ParamToolbox paramToolbox = new ParamToolbox();

            Assert.NotEqual("GLS2", paramToolbox.GetClientTrigram("26"));
        }

        [Fact]
        public void SouldGetClientTrigramNotFound()
        {
            ParamToolbox paramToolbox = new ParamToolbox();

            Assert.Equal("Client trigram does not exist.", paramToolbox.GetClientTrigram("1234"));
        }

    }

}
