using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureDevOpsFunctions
{
    public static class CheckBuildFailureHistoryFunction
    {
        [FunctionName("CheckBuildFailureHistoryFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            try
            {
                log.LogInformation("CheckBuildFailureHistoryFunction - STARTED");

                int failureCount = 0;
                bool overThreshold = false;

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
                    string projectid = await new StreamReader(req.Body).ReadToEndAsync();

                    // Get the history for the project builds
                    BuildHttpClient buildClient = connection.GetClient<BuildHttpClient>();
                    List<Build> builds = buildClient.GetBuildsAsync(projectid.Replace("\"","")).Result;
                    foreach (Build build in builds)
                    {
                        if (build.Result == BuildResult.Failed)
                        {
                            failureCount += 1;
                        }
                        if (failureCount == Int32.Parse(configurationBuilder["DevOpsRepeatedFailedBuildThreshold"]))
                        {
                            overThreshold = true;
                            break;
                        }
                    }
                }
                
                log.LogInformation("CheckBuildFailureHistoryFunction - COMPLETED");

                return new OkObjectResult(overThreshold);
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
