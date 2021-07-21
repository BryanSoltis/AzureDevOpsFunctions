# Azure DevOps Functions
This repository contains functions for automating Azure DevOps processes.

This repository is used in the following blog article. Full details on deploying this solution can be found in the article.

https://soltisweb.com/blog/detail/2021-05-21-automatingazuredevopsupdateswithazurefunctions

# Prerequisites
* **Create an Azure Function**  
Azure Functions will be used to update Azure DevOps when Service Hooks are enabled within Azure DevOps. Luckily, there’s a .NET Core SDK for the Azure DevOps REST API to simplify the integration. Depending on the automation requirements, Azure Functions can vary greatly.

* **Create an Azure Function Managed Identity**  
This will allow the Azure Function to be assigned permissions to the Azure Key Vault.

* **Create an Azure Key Vault**  
This will be used to store sensitive information, like the Azure DevOps PAT.

* **Assign an Azure Key Vault access policy (Portal) | Microsoft Docs**  
This process allows the Azure Function to access the Azure Key Vault to retrieve the PAT.


# Instructions
To use this repository:

1. Download/Clone the repository

2. In the [localsettings.json](https://github.com/BryanSoltis/AzureDevOpsFunctions/blob/master/local.settings.json) file, update the following values (if running locally):

* **DevOpsOrgName**: [Your Azure DevOps Org Name]
* **DevOpsDefaultState**: [Your Azure DevOps state for work items. Typically this value is ‘New’]
* **DevOpsUpdateState**: “[Your Azure DevOps state for work items to be updated to. Typically this value is “In Progress”]
* **DevOpsPAT**: “[Your Azure Key Vault secret URL in the proper format. Example: @Microsoft.KeyVault(SecretUri=[Your Azure Key Vault Secrete URL)]”
* **DevOpsRepeatedFailedBuildThreshold**: “[Number of times a build can fail before being flagged]”

3. Deploy the project to Azure Functions.

4. Create the App Settings for the configuration values.

5. In Azure DevOps, create a service hook.
    a. Select "Web hooks" for the Service
    b. Select "Work item updated" for the Event
    c. Select "State" for the Field
    d. Enter the Azure Function URL for the Action

6. In Azure DevOps, create new Personal Access Token (PAT).

7. In Azure Key Vault, create a new secret for the DevOpsPAT value.

8. In Azure DevOps, update a work item status.

9. Confirm the service hook is called and the Azure Function is exectued to update the parent work item status.

# Helpful Links

* [Azure DevOps Client library sample snippets](https://github.com/microsoft/azure-devops-dotnet-samples/tree/main/ClientLibrary/Samples)
* [.NET Client Libraries - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops)
* [Get started with the REST APIs for Azure DevOps Services and Team Foundation Server - Azure DevOps Services REST API | Microsoft Docs](https://docs.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-6.1)
* [Authenticate with personal access tokens - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page)
* [Integrate with service hooks - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/service-hooks/overview?view=azure-devops)
* [Use Key Vault references - Azure App Service | Microsoft Docs](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references)
* [Assign an Azure Key Vault access policy (Portal) | Microsoft Docs](https://docs.microsoft.com/en-us/azure/key-vault/general/assign-access-policy-portal)
