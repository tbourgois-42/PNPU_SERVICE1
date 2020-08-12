using PNPUTools;
using Xunit;
using PNPUCore;
using System.Data;
using PNPUTools.DataManager;
using System;

namespace XUnitTest
{

    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            // ... initialize data in the test database ...

        }

        public void Dispose()
        {
            // ... clean up test data from the database ...
        }

    }


    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Database collection")]
    public class ParamToolboxTest
    {
        DatabaseFixture fixture;

        public ParamToolboxTest(DatabaseFixture fixture)
        {
            this.fixture = fixture;
        }


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

        

        [Fact]
        public void ShouldGetReportWhenLaunchDependanceProcess()
        {

            string listClientId = "";
            int workflowId = 0;
            int process = ParamAppli.ProcessGestionDependance;
            int idInstanceWF = 0;
            bool isReportIsPresent = false;
            var launcher = new Launcher();
            launcher.Launch(listClientId, workflowId, process, idInstanceWF);

            string requestCheckIfAReportIsPresent = "select JSON_TEMPLATE from PNPU_H_REPORT where WORKFLOW_ID = " + workflowId + " AND ID_PROCESS = " + process + " AND CLIENT_ID = " + listClientId + "";
            DataSet dsDataSet = DataManagerSQLServer.GetDatas(requestCheckIfAReportIsPresent, ParamAppli.ConnectionStringBaseAppli);

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

            string listClientId = "";
            int workflowId = 0;
            int process = ParamAppli.ProcessGestionDependance;
            int idInstanceWF = 0;
            bool isMDBIsPresent = false;
            var launcher = new Launcher();
            launcher.Launch(listClientId, workflowId, process, idInstanceWF);

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
    public class ContextGeneratorTool
    {


        public static void initWorkflowWithMdb()
        {

            string clientId = "";
            int workflowId = 0;

            string sRequest = "SELECT ID_PROCESS FROM PNPU_STEP PS INNER JOIN PNPU_WORKFLOW PHW ON PHW.WORKFLOW_ID = PS.WORKFLOW_ID  WHERE PHW.WORKFLOW_ID = " + workflowId + " AND PS.ORDER_ID = 0 AND PHW.IS_TOOLBOX = 1";



            DataSet dsDataSet = DataManagerSQLServer.GetDatas(sRequest, ParamAppli.ConnectionStringBaseAppli);

            if ((dsDataSet != null) && (dsDataSet.Tables[0].Rows.Count > 0))
            {
                DataRow drRow = dsDataSet.Tables[0].Rows[0];
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

                int idInstanceWF = int.Parse(RequestTool.CreateUpdateWorkflowHistoric(historicWorkflow));

            }

        }
    }
}
