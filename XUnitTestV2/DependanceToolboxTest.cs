﻿using PNPUCore;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace XUnitTest
{

    public class InitialisationProcessDependance : IDisposable
    {
        public string clientId { get; set; }
        public int workflowId { get; set; }
        public int idInstanceWF { get; set; }


        public InitialisationProcessDependance()
        {
            // ... initialize data in the test database ...

            TestHelper.AddInfoClientToParamAppli();

            workflowId = 32;
            int process = ParamAppli.ProcessGestionDependance;
            clientId = "999";

            //OLD CODE string sRequest = "SELECT ID_PROCESS FROM PNPU_STEP PS INNER JOIN PNPU_WORKFLOW PHW ON PHW.WORKFLOW_ID = PS.WORKFLOW_ID  WHERE PHW.WORKFLOW_ID = " + workflowId + " AND PS.ORDER_ID = 0 AND PHW.IS_TOOLBOX = 1";

            // We generate instance of workflow in PNPU_H_WORKFLOW 
            PNPU_H_WORKFLOW historicWorkflow = new PNPU_H_WORKFLOW
            {
                WORKFLOW_ID = workflowId,
                CLIENT_ID = clientId,
                LAUNCHING_DATE = DateTime.Now,
                ENDING_DATE = new DateTime(1800, 1, 1),
                STATUT_GLOBAL = ParamAppli.StatutInProgress,
                INSTANCE_NAME = "Toolbox Workflow #" + workflowId
            };

            idInstanceWF = int.Parse(RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow));

            TestHelper.CreateParamToolboxForDedie(workflowId.ToString(), null, idInstanceWF);

            GereMDBDansBDD gestionMDBdansBDD = new GereMDBDansBDD();
            // Add zip into database
            gestionMDBdansBDD.AjouteZipBDD("C:\\TEMPO\\MDB_TEST\\TEST_DEPENDANCE_WF32.zip", workflowId, ParamAppli.ConnectionStringBaseAppli, idInstanceWF);

            var launcher = new Launcher();
            launcher.Launch(clientId, workflowId, process, idInstanceWF);

        }

        public void Dispose()
        {
            // ... clean up test data from the database ...

        }

    }


    [CollectionDefinition("Database collection")]
    public class InitialisationProcessDependanceCollection : ICollectionFixture<InitialisationProcessDependance>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Database collection")]
    public class ProcessDependanceTest
    {
        private readonly InitialisationProcessDependance fixture;

        public ProcessDependanceTest(InitialisationProcessDependance fixture)
        {
            this.fixture = fixture;
        }



        [Fact]
        public void ShouldGetReportWhenLaunchDependanceProcess()
        {
            string requestCheckIfAReportIsPresent = String.Format("select JSON_TEMPLATE from PNPU_H_REPORT where WORKFLOW_ID = {0} AND CLIENT_ID = '{1}' AND ID_H_WORKFLOW = {2} ", fixture.workflowId, fixture.clientId, fixture.idInstanceWF);
            DataSet dsDataSet = DataManagerSQLServer.GetDatas(requestCheckIfAReportIsPresent, ParamAppli.ConnectionStringBaseAppli);
            bool isReportIsPresent = false;
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                DataRow drRow = dsDataSet.Tables[0].Rows[0];
                isReportIsPresent = !String.IsNullOrEmpty((string)drRow[0]) && dsDataSet.Tables[0].Rows.Count == 1;

            }

            Assert.True(isReportIsPresent);

        }

        [Fact]
        public void ShouldNotGetMDBWhenLaunchDependanceProcess()
        {
            bool isMDBIsPresent = false;
            string requestCheckIfAReportIsPresent = String.Format("select MDB from PNPU_H_REPORT where WORKFLOW_ID = {0} AND CLIENT_ID = '{1}' AND ID_H_WORKFLOW = {2} ", fixture.workflowId, fixture.clientId, fixture.idInstanceWF);
            DataSet dsDataSet = DataManagerSQLServer.GetDatas(requestCheckIfAReportIsPresent, ParamAppli.ConnectionStringBaseAppli);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                //UNUSED CODE Removed? DataRow drRow = dsDataSet.Tables[0].Rows[0];
                isMDBIsPresent = dsDataSet.Tables[0].Rows.Count >= 1;
            }

            Assert.True(!isMDBIsPresent);
        }
    }
}
