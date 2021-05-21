# AzureDevOpsFunctions
This repository contains functions for automating Azure DevOps processes.

This repository is used in the following blog article. Full details on deploying this solution can be found in the article.

https://soltisweb.com/blog/detail/2021-05-21-automatingazuredevopsupdateswithazurefunctions

# Instructions
To use this repository:

1. Download/Clone the repository

2. In the [localsettings.json](https://github.com/BryanSoltis/AzureDevOpsFunctions/blob/master/local.settings.json) file, update the following values:

* **DevOpsOrgName**: [Your Azure DevOps Org Name]
* **DevOpsDefaultState**: [Your Azure DevOps state for work items. Typically this value is ‘New’]
* **DevOpsUpdateState**: “[Your Azure DevOps state for work items to be updated to. Typically this value is “In Progress”]
* **DevOpsPAT**: “[Your Azure Key Vault secret URL in the proper format. Example: @Microsoft.KeyVault(SecretUri=[Your Azure Key Vault Secrete URL)]”

# Helpful Links

* [Azure DevOps Client library sample snippets](https://github.com/microsoft/azure-devops-dotnet-samples/tree/main/ClientLibrary/Samples)
* [.NET Client Libraries - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops)
* [Get started with the REST APIs for Azure DevOps Services and Team Foundation Server - Azure DevOps Services REST API | Microsoft Docs](https://docs.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-6.1)
* [Authenticate with personal access tokens - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page)
* [Integrate with service hooks - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/service-hooks/overview?view=azure-devops)
* [Use Key Vault references - Azure App Service | Microsoft Docs](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references)
* [Assign an Azure Key Vault access policy (Portal) | Microsoft Docs](https://docs.microsoft.com/en-us/azure/key-vault/general/assign-access-policy-portal)
