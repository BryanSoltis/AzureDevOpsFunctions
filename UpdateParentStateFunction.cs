using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureDevOpsFunctions
{
    public static class UpdateParentStateFunction
    {
        [FunctionName("UpdateParentStateFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

                // Get the settings
                string orgName = configurationBuilder["DevOpsOrgName"];
                string defaultState = configurationBuilder["DevOpsDefaultState"]; // New work item state 
                string updateState = configurationBuilder["DevOpsUpdateState"]; // Work item state to change parent item to  
                string pat = configurationBuilder["DevOpsPAT"]; // DevOps Personal Access Token (from Azure Key Vault) 

                log.LogInformation("PAT:" + pat);

                // Get request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                JObject data = (JObject)JsonConvert.DeserializeObject(requestBody);

                // Check the child state
                string currentState = data["resource"]["revision"]["fields"]["System.State"].ToString();

                if (currentState != defaultState)
                {
                    // Get the parent id
                    int parentId;
                    // Check if there is a parent work item
                    if (data["resource"]["revision"]["fields"]["System.Parent"] != null)
                    {
                        if (Int32.TryParse(data["resource"]["revision"]["fields"]["System.Parent"].ToString(), out parentId))
                        {
                            string Url = string.Format(
                                @"https://dev.azure.com/{0}",
                                orgName);

                            // Create instance of VssConnection using Personal Access Token
                            VssConnection connection = new VssConnection(new Uri(Url), new VssBasicCredential(string.Empty, pat));

                            // Get the parent work item
                            WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();

                            string[] fieldNames = new string[] {
                        "System.State",
                        };

                            WorkItem parentworkitem = workItemTrackingClient.GetWorkItemAsync(parentId, fieldNames).Result;

                            // Check the parent state to make sure it needs to be modified
                            if (parentworkitem.Fields["System.State"].ToString() != updateState)
                            {
                                // Update the parent to the default state
                                JsonPatchDocument patchDocument = new JsonPatchDocument();
                                patchDocument.Add(
                                    new JsonPatchOperation()
                                    {
                                        Operation = Operation.Add,
                                        Path = "/fields/System.State",
                                        Value = updateState
                                    }
                                );
                                WorkItem result = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, parentId).Result;
                            }
                        }
                    }
                }
                return new OkObjectResult("Success");
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.ToString());
                log.LogInformation(ex.StackTrace);
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
