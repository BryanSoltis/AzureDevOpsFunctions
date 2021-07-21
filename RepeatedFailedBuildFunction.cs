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
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureDevOpsFunctions
{
    public static class RepeatedFailedBuildFunction
    {
        [FunctionName("RepeatedFailedBuildFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {
                log.LogInformation("RepeatedFailedBuildFunction - STARTED");

                bool createBug = false;

                var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

                // Get the settings
                string orgName = configurationBuilder["DevOpsOrgName"];
                string pat = configurationBuilder["DevOpsPAT"]; // DevOps Personal Access Token (from Azure Key Vault) 

                // Create instance of VssConnection using Personal Access Token
                string Url = string.Format(
                    @"https://dev.azure.com/{0}",
                    orgName);

                using (VssConnection connection = new VssConnection(new Uri(Url), new VssBasicCredential(string.Empty, pat)))
                {
                    // Get request body
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    JObject data = (JObject)JsonConvert.DeserializeObject(requestBody);

                    // Get the build id
                    string buildid = data["resource"]["id"].ToString();

                    // Get the project id
                    string projectid = data["resourceContainers"]["project"]["id"].ToString();

                    // Get the history for the project builds
                    // Create a mock HttpRequest for the projectid
                    // Call the standalone function 
                    var ms = new MemoryStream();
                    var sw = new StreamWriter(ms);
                    var projectidjson = JsonConvert.SerializeObject(projectid);
                    sw.Write(projectidjson);
                    sw.Flush();
                    ms.Position = 0;
                    var req2 = new Mock<HttpRequest>();
                    req2.Setup(x => x.Body).Returns(ms);
                    ObjectResult result = (ObjectResult)CheckBuildFailureHistoryFunction.Run(req2.Object, log, context).Result;
                    // Ensure the function call was successful
                    if (result.StatusCode == 200)
                    {
                        createBug = Convert.ToBoolean(result.Value);
                    }

                    // Check if a bug should be created
                    if (createBug)
                    {
                        log.LogInformation("Bug Work Item Creation - STARTED");
                        // Construct the object containing field values required for the new work item
                        JsonPatchDocument patchDocument = new JsonPatchDocument();

                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                                Path = "/fields/System.Title",
                                Value = "Repeated Build Failure"
                            }
                        );

                        WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();

                        // Create the new work item
                        WorkItem newWorkItem = workItemTrackingClient.CreateWorkItemAsync(patchDocument, projectid, "Bug").Result;
                        log.LogInformation("Bug Work Item Creation - COMPLETED");
                    }
                    else
                    {
                        log.LogInformation("Bug Work Item Creation not required");
                    }

                    log.LogInformation("RepeatedFailedBuildFunction - COMPLETED");
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