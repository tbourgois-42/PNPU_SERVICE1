using PNPUCore.Database;
using PNPUTools;
using PNPUTools.DataManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "clients/dashboard/{workflowId}/{idInstanceWF}")]
        IEnumerable<InfoClientStep> GetInfoAllClient(string workflowId, string idInstanceWF);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "clients/{ClientName}")]
        string GetInfoOneClient(string ClientName);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "process")]
        IEnumerable<PNPU_PROCESS> GetAllProcesses();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "workflow/historic")]
        IEnumerable<PNPU_H_WORKFLOW> GetHWorkflow();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "process/{processId}")]
        PNPU_PROCESS getProcess(string processId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "workflow")]
        IEnumerable<PNPU_WORKFLOW> GetAllWorkFLow();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "workflow/{workflowId}")]
        PNPU_WORKFLOW getWorkflow(string workflowId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "report/{workflowId}/{idInstanceWF}/{idProcess}/{clientId}")]
        IEnumerable<PNPU_H_REPORT> getReport(string idProcess, string workflowId, string clientId, string idInstanceWF);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "workflow/{workflowId}/processus")]
        IEnumerable<PNPU_WORKFLOWPROCESSES> GetWorkflowProcesses(string workflowId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "localisation/workflow/{workflowId}/{idInstanceWF}/{clientId}")]
        string GetNbLocalisation(string workflowId, string idInstanceWF, string clientId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "processuscritique")]
        string GetProcessusCritiquesAllCLient();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "processuscritique/{ClientName}")]
        string GetProcessusCritiquesOneClient(string ClientName);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "idorga")]
        string GetIdOrgaAllClient();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "idorga/{ClientName}")]
        string GetIdOrgaOneClient(string ClientName);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "typologie")]
        string GetTypoAllClient();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "typologie/{ClientName}")]
        string GetTypoOneClient(string ClientName);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "Workflow/{WorkflowName}/Run")]
        string RunWorkflow(string WorkflowName);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "Workflow/CreateWorkflow/")]
        string CreateWorkflow(PNPU_WORKFLOW input);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "Workflow/{workflowID}")]
        string AffectWorkflowsProcesses(PNPU_STEP input, string workflowID);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "Workflow/{workflowID}/maxstep")]
        string GetMaxStep(string workflowID);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "process/CreateProcess/")]
        string CreateProcess(PNPU_PROCESS input);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "Workflow/{workflowID}")]
        string ModifyWorkflow(PNPU_WORKFLOW input, string workflowID);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "process/{processID}")]
        string ModifyProcessus(PNPU_PROCESS input, string processID);

        [OperationContract]
        [WebInvoke(Method = "DELETE",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "Workflow/{workflowID}/Delete")]
        string DeleteWorkflow(string workflowID);

        [OperationContract]
        [WebInvoke(Method = "DELETE",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "process/{processID}/Delete")]
        string DeleteProcess(string processID);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "worflow/{WorkflowId}/uploadFile")]
        void UploadFile(Stream stream, string WorkflowId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "clientsByTypo/{TypologyId}")]
        IEnumerable<InfoClient> getListClientsByTypo( string TypologyId);
        
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "clientsByTypo")]
        IEnumerable<InfoClient> getListClients();

        [OperationContract]
        [WebGet(UriTemplate = "clients/livraison/{workflowId}/{idInstanceWF}/{clientId}")]
        Stream GetMdbLivraison(string workflowId, string idInstanceWF, string clientId);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "livraison/availablePack/{workflowId}/{idInstanceWF}/{clientId}")]
        int GetNbAvailablePack(string workflowId, string idInstanceWF, string clientId);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "auth/signin")]
        string AuthUser(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "auth/me")]
        string ConnectUser(Stream stream);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "auth/signout")]
        string SignOutUser(Stream stream);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "auth/habilitation/?user={user}&token={token}")]
        string GetHabilitation(string user, string token);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            UriTemplate = "auth/habilitation/clients/?user={user}&habilitation={habilitation}")]
        string[] GetListClients(string user, string habilitation);

        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json)]
        void preflightRequest();

    }
}
