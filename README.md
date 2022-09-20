# Dapr Data Management

  Welcome to the Part 2 of the [Dapr Series](https://github.com/SiddyHub/Dapr/tree/eshop_daprized).
  
  The advantage of Microservices is that each Microservice can choose it's own type of Storage. Through this repo, we will check our services use multiple Data Storing technologies.
  Also in the process look at Dapr State Management, Secrets Configuration etc.

  The [main](https://github.com/SiddyHub/DaprDataManagement) branch, is the code base from [Part 1](https://github.com/SiddyHub/Dapr/tree/eshop_daprized) of the [Dapr Series](https://github.com/SiddyHub/Dapr/tree/eshop_daprized),
and [daprDataManagement](https://github.com/SiddyHub/DaprDataManagement/tree/daprDataManagement) branch is the refactored code base covering Dapr State Management and other concepts.

This version of the code uses **Dapr 1.7**

## Pre-Requisites to Run the Application

- VS Code
  - with [Dapr Extension](https://docs.dapr.io/developing-applications/ides/vscode/vscode-dapr-extension/)
  - and with [Azure Cache Extension](https://marketplace.visualstudio.com/items?itemName=ms-azurecache.vscode-azurecache)
- .NET Core 3.1 SDK
- Docker installed (e.g. Docker Desktop for Windows)
- [Dapr CLI installed](https://docs.dapr.io/getting-started/install-dapr-cli/)
- Access to an Azure subscription

## Architecture Overview

![dapr_state_mgmnt_arch](https://user-images.githubusercontent.com/84964657/191175832-7ce251c3-af91-4ef0-baeb-6006ddab2e31.png)

In our [Part 1](https://github.com/SiddyHub/Dapr/tree/eshop_daprized) we used  ShoppingBasket as one of our API service endpoint, to add items, remove or update our line items in the shopping basket.
Also our EventCatalog service used to get Events and Categories from a relational SQL Database.

In this repo, we would be replacing the Shopping Basket service with an Azure Redis Cache for our State Management so that there is no need to persist the data in the database, but need fast access, and also may need data to be expired or be deleted.

Also for our Event Catalog service we would be replacing relational SQL Database with Azure Cosmos DB database.

The Cosmos DB Endpoint, Key, DatabaseName and Azure Redis Cache Key would be retrieved from the Dapr secrets store.

## Running the app locally   

   Once VS Code with [Dapr Extension](https://docs.dapr.io/developing-applications/ides/vscode/vscode-dapr-extension/) has been installed, we can leverage it to scaffold the configuration for us, instead of manually configuring **launch.json**.

   A **tasks.json** file also gets prepared by the Dapr extension task.

   Follow [this link](https://docs.dapr.io/developing-applications/ides/vscode/vscode-how-to-debug-multiple-dapr-apps/#prerequisites) to know more about configuring `launch.json and tasks.json`

   In VS Code go to Run and Debug, and Run All Projects at the same time or Individual project.

 ![debug](https://user-images.githubusercontent.com/84964657/190982955-b0a69850-4795-444a-aaf3-e2d6120dc1b2.jpg)
 
  All the projects which have build successfully and running, can be viewed in the Call Stack window.

![callstack](https://user-images.githubusercontent.com/84964657/190982330-5724fbae-2caa-49ec-a87a-db425db661c5.jpg)

   Once the application and side car is running, we can also apply breakpoint to debug the code. Check [this link](https://code.visualstudio.com/docs/editor/debugging#_breakpoints) for more info.

   ![breakpoint](https://user-images.githubusercontent.com/84964657/191080455-2aa1a8f9-a051-410b-9a42-617184b5ee39.jpg)

   The Darp extension added also provides information about the applications running and the corresponding components loaded for that application.

   ![dapr_extension_components](https://user-images.githubusercontent.com/84964657/190985678-5b7d24c8-095d-43e5-86fe-0002a5d985ee.png)

## Dapr Building Blocks Covered

**1. State Management**
   
   *We would be using Azure Redis Cache for our State Management, but if Azure Subscription is not available, one can use Redis Cache container spun up by Dapr.

   Refer [this link](https://docs.dapr.io/developing-applications/building-blocks/state-management/state-management-overview/) to know more.

   We have an Interface `IShoppingBasketService` which specifies all capabilities of our Shopping Basket i.e. Add, Update, Get or Remove items from the basket.
   
   Follow [this link](https://docs.dapr.io/reference/components-reference/supported-state-stores/setup-redis/#component-format) to check more metadata fields for a Redis Component.

   To set up Azure Redis follow [this link](https://docs.dapr.io/reference/components-reference/supported-state-stores/setup-redis/#setup-redis) and make sure to select "Azure" tab.

   Once the code has been run via VS Code, we can test the Redis Cache values with the help of [Azure Cache Extension](https://marketplace.visualstudio.com/items?itemName=ms-azurecache.vscode-azurecache).

   |Initial Run|After Item added to Shopping Basket|
   |-----------|-----------------------------------|
   |![initial_cache](https://user-images.githubusercontent.com/84964657/191172204-d28d2616-8a55-4d8d-87ac-e700ac86a38b.jpg)|  ![cache_values](https://user-images.githubusercontent.com/84964657/191172284-c7d755d1-ca03-4616-9910-6c9278509528.jpg)|


**2. Secrets Management**

   Refer [this link](https://docs.dapr.io/developing-applications/building-blocks/secrets/secrets-overview/) to know about more about dapr Secrets Management.
   
        The Dapr .Net SDK features a .Net Configuration Provider. It loads specified secrets into underlying [.Net Configuration API](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration).
     
   Our application can then reference secrets from the IConfiguration dictionary that is registered in ASP.NET Core dependency injection.
     
   The secrets configuration provider is available from the [Dapr.Extensions.Configuration](https://www.nuget.org/packages/Dapr.Extensions.Configuration) Nuget
   package.
   The provider can be registered in the `Program.cs` of an ASP.NET Web API application.

   We use the secrets configuration provider to load our Cosmos DB Endpoint, Key and DatabaseName from `secrets.json` file, like below:

   ```
   .ConfigureAppConfiguration(config => 
   {
      var daprClient = new DaprClientBuilder().Build();                  
      var secretDescriptors = new List<DaprSecretDescriptor>
      {
          new DaprSecretDescriptor("CosmosDb:Endpoint"),
          new DaprSecretDescriptor("CosmosDb:Key"),
          new DaprSecretDescriptor("CosmosDb:DatabaseName")
     };
     config.AddDaprSecretStore("secretstore", secretDescriptors, daprClient);
   })
   ```

   Now secrets are loaded into the application configuration. You can retrieve them by calling the indexer of the IConfiguration instance in
   `Startup.cs ConfigureServices`

   ```
   services.AddDbContext<EventCatalogCosmosDbContext>(options =>
      options.UseCosmos(Configuration["CosmosDb:Endpoint"],
      Configuration["CosmosDb:Key"],
      Configuration["CosmosDb:DatabaseName"])
   );
   ```

## Troubleshooting notes

- If not able to load Dapr projects when running from VS Code, check if Docker Engine is running, so that it can load all components.
- If using Azure Service Bus as a Pub Sub Message broker make sure to enter primary connection string in `secrets.json`
- If using Cosmos DB make sure to enter Endpoint, Key and DatabaseName in `secrets.json` file
- If using Azure Redis Cache make sure to enter Key in `secrets.json` file
- If mail binding is not working, make sure `maildev`image is running. Refer [this link](https://github.com/maildev/maildev) for more info.
- For any more service issues, we can check Zipkin trace logs.
