using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace XUnitTest
{
    public class TestHelper
    {
        public static string SaveParamsToolbox(Dictionary<String, String> ParamInfo,  int idInstanceWF)
        {
            string ServerBefore = string.IsNullOrEmpty(ParamInfo["serverBefore"]) ? "" : ParamInfo["serverBefore"];
            string DatabaseBefore = string.IsNullOrEmpty(ParamInfo["databaseBefore"]) ? "" : ParamInfo["databaseBefore"];
            string PasswordBefore = string.IsNullOrEmpty(ParamInfo["passwordBefore"]) ? "" : ParamInfo["passwordBefore"];
            string ServerAfter = string.IsNullOrEmpty(ParamInfo["serverAfter"]) ? "" : ParamInfo["serverAfter"];
            string DatabaseAfter = string.IsNullOrEmpty(ParamInfo["databaseAfter"]) ? "" : ParamInfo["databaseAfter"];
            string PasswordAfter = string.IsNullOrEmpty(ParamInfo["passwordAfter"]) ? "" : ParamInfo["passwordAfter"];
            string DtPaie = string.IsNullOrEmpty(ParamInfo["dtPaie"]) ? "" : ParamInfo["dtPaie"];
            string ClientId = string.IsNullOrEmpty(ParamInfo["clientID"]) ? "" : ParamInfo["clientID"];
            string WorkflowId = string.IsNullOrEmpty(ParamInfo["workflowID"]) ? "" : ParamInfo["workflowID"];

            string[] sRequest = { "INSERT INTO PNPU_TMP_PARAM_TOOLBOX (SERVER_BEFORE, DATABASE_BEFORE, PASSWORD_BEFORE, SERVER_AFTER, DATABASE_AFTER, PASSWORD_AFTER, DT_PAIE, CLIENT_ID, WORKFLOW_ID, ID_H_WORKFLOW ) VALUES (@SERVER_BEFORE, @DATABASE_BEFORE, @PASSWORD_BEFORE, @SERVER_AFTER, @DATABASE_AFTER, @PASSWORD_AFTER, @DT_PAIE, @CLIENT_ID, @WORKFLOW_ID, @ID_H_WORKFLOW)" };
            string[] parameters = new string[] { "@SERVER_BEFORE", ServerBefore, "@DATABASE_BEFORE", DatabaseBefore, "@PASSWORD_BEFORE", PasswordBefore, "@SERVER_AFTER", ServerAfter, "@DATABASE_AFTER", DatabaseAfter, "@PASSWORD_AFTER", PasswordAfter, "@DT_PAIE", DtPaie, "@CLIENT_ID", ClientId, "@WORKFLOW_ID", WorkflowId, "@ID_H_WORKFLOW", idInstanceWF.ToString() };

            try
            {
                return DataManagerSQLServer.ExecuteSqlTransaction(sRequest, "PNPU_TMP_PARAM_TOOLBOX", parameters);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }


        public static void CreateParamToolboxForDedie(String WorkflowId, string _dtPaie, int IdHWorkflow)
        {
            Dictionary<string, string> ParamToolBoxInfo = new Dictionary<string, string>();
            ParamToolBoxInfo["serverBefore"] = "vm-pn-pusql-001";
            ParamToolBoxInfo["databaseBefore"] = "FRACUSQA1";
            ParamToolBoxInfo["passwordBefore"] = "FRACUSQA1";
            ParamToolBoxInfo["serverAfter"] = "vm-pn-pusql-001";
            ParamToolBoxInfo["databaseAfter"] = "FRACUSQA2";
            ParamToolBoxInfo["passwordAfter"] = "FRACUSQA2";
            ParamToolBoxInfo["dtPaie"] = _dtPaie;
            ParamToolBoxInfo["clientID"] = "999";
            ParamToolBoxInfo["workflowID"] = WorkflowId;

            SaveParamsToolbox(ParamToolBoxInfo, IdHWorkflow);
        }

        public static void CreateParamToolboxForMutualise(String WorkflowId, string _dtPaie, int IdHWorkflow)
        {
            Dictionary<string, string> ParamToolBoxInfo = new Dictionary<string, string>();
            ParamToolBoxInfo["serverBefore"] = "vm-pn-pusql-001";
            ParamToolBoxInfo["databaseBefore"] = "FRASTAQA1";
            ParamToolBoxInfo["passwordBefore"] = "FRASTAQA1";
            ParamToolBoxInfo["serverAfter"] = "vm-pn-pusql-001";
            ParamToolBoxInfo["databaseAfter"] = "FRASTAQA2";
            ParamToolBoxInfo["passwordAfter"] = "FRASTAQA2";
            ParamToolBoxInfo["dtPaie"] = _dtPaie;
            ParamToolBoxInfo["clientID"] = "997";
            ParamToolBoxInfo["workflowID"] = WorkflowId;
            SaveParamsToolbox(ParamToolBoxInfo, IdHWorkflow);


        }

        public void CreateParamToolboxForDesynchro(String WorkflowId, string _dtPaie, int IdHWorkflow)
        {
            Dictionary<string, string> ParamToolBoxInfo = new Dictionary<string, string>();
            ParamToolBoxInfo["serverBefore"] = "vm-pn-pusql-001";
            ParamToolBoxInfo["databaseBefore"] = "FRADERQA1";
            ParamToolBoxInfo["passwordBefore"] = "FRADERQA1";
            ParamToolBoxInfo["serverAfter"] = "vm-pn-pusql-001";
            ParamToolBoxInfo["databaseAfter"] = "FRADERQA2";
            ParamToolBoxInfo["passwordAfter"] = "FRADERQA2";
            ParamToolBoxInfo["dtPaie"] = _dtPaie;
            ParamToolBoxInfo["clientID"] = "998";
            ParamToolBoxInfo["workflowID"] = WorkflowId;

            SaveParamsToolbox(ParamToolBoxInfo, IdHWorkflow);
        }

        public static void AddInfoClientToParamAppli()
        {

            InfoClient infoClient0 = new InfoClient("999", "FRACUS", "Dédié", "256", "server=vm-PN-RDSQL-001;uid=FRACUSQA1;pwd=FRACUSQA1;database=FRACUSQA1;", "server=vm-PN-RDSQL-001;uid=FRACUSQA2;pwd=FRACUSQA2;database=FRACUSQA2;", false);
            ParamAppli.ListeInfoClient["999"] = infoClient0;
            infoClient0 = new InfoClient("998", "FRADER", "Désynchronisé", "258 ", "server=vm-PN-RDSQL-001;uid=FRADERQA1;pwd=FRADERQA1;database=FRADERQA1;", "server=vm-PN-RDSQL-001;uid=FRADERQA2;pwd=FRADERQA2;database=FRADERQA2;", false);
            ParamAppli.ListeInfoClient["998"] =  infoClient0;
            infoClient0 = new InfoClient("997", "FRASTA", "Mutualisé", "257 ", "server=vm-PN-RDSQL-001;uid=FRADERQA1;pwd=FRADERQA1;database=FRADERQA1;", "server=vm-PN-RDSQL-001;uid=FRADERQA2;pwd=FRADERQA2;database=FRADERQA2;", false);
            ParamAppli.ListeInfoClient["997"] = infoClient0;

        }
    }


}
