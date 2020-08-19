using PNPUCore;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Data;
using Xunit;

namespace XUnitTest
{

    public class InitialisationProcessDependance : IDisposable
    {
        private string clientId { get; set; }
        private int workflowId { get; set; }


        public InitialisationProcessDependance()
        {
            // ... initialize data in the test database ...
            string listClientId = "49"; //
            int workflowId = 32; //TODO
            int process = ParamAppli.ProcessGestionDependance;
            int idInstanceWF = 0;
            clientId = "49";

            string sRequest = "SELECT ID_PROCESS FROM PNPU_STEP PS INNER JOIN PNPU_WORKFLOW PHW ON PHW.WORKFLOW_ID = PS.WORKFLOW_ID  WHERE PHW.WORKFLOW_ID = " + workflowId + " AND PS.ORDER_ID = 0 AND PHW.IS_TOOLBOX = 1";

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

            var launcher = new Launcher();
            launcher.Launch(listClientId, workflowId, process, idInstanceWF);

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
            string requestCheckIfAReportIsPresent = "select JSON_TEMPLATE from PNPU_H_REPORT where WORKFLOW_ID = " + workflowId + " AND ID_PROCESS = " + process + " AND CLIENT_ID = " + listClientId + "";
            DataSet dsDataSet = DataManagerSQLServer.GetDatas(requestCheckIfAReportIsPresent, ParamAppli.ConnectionStringBaseAppli);
            bool isReportIsPresent = false;
            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                DataRow drRow = dsDataSet.Tables[0].Rows[0];
                isReportIsPresent = String.IsNullOrEmpty((string)drRow[0]) && dsDataSet.Tables[0].Rows.Count == 1;

            }

            Assert.True(isReportIsPresent);

        }

        [Fact]
        public void ShouldGetMDBWhenLaunchDependanceProcess()
        {
            bool isMDBIsPresent = false;
            string requestCheckIfAReportIsPresent = "select MDB from PNPU_MDB where WORKFLOW_ID = AND CLIENT_ID = ";
            DataSet dsDataSet = DataManagerSQLServer.GetDatas(requestCheckIfAReportIsPresent, ParamAppli.ConnectionStringBaseAppli);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                DataRow drRow = dsDataSet.Tables[0].Rows[0];
                isMDBIsPresent = dsDataSet.Tables[0].Rows.Count >= 1;
            }

            Assert.True(isMDBIsPresent);
        }
    }
}
