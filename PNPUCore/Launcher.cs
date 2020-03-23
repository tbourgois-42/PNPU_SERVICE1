﻿using PNPUCore.Database;
using PNPUCore.Process;
using System;
using System.Collections.Generic;

namespace PNPUCore
{
    public class Launcher
    {
        List<InfoClient> listClient;


        void launchProcess(IProcess process)
        {
            process.executeMainProcess();
            String json = process.formatReport();
            Console.WriteLine(json);
        }


        public void Launch(String clientName, String processName)
        {
            IProcess process = createProcess(processName, clientName);

            launchProcess(process);
        }


        IProcess createProcess(String process, String client)
        {
            return ProcessMock.createProcess();
        }

    }
}
