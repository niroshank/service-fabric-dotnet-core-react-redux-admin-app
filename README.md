---
languages:
- csharp
- javascript
products:
- azure service fabric
- dotnet core
- angular
page_type: iot product
description: "This sample project demonstrates a multi-tenant IoT solution using Azure Service Fabric."
---

# Azure Sevice Fabric Dotnet Core Angular Sample

This project demonstrates a multi-tenant IoT solution using Azure Service Fabric.

## Setup

 1. [Install Visual Studio 2017](https://www.visualstudio.com/). Any version will do. Make sure you have the following workloads installed:
    - Azure development
    - .NET Core cross-platform development

## Configure Service Fabric

One of the greate feature of Service Fabric is the ability to create change and delete applications and services at runtime. To get started, we can first create a new Service Fabric - Stateless MVC application calling Admin Application and Admin Web Servce. In this project we will have to application types which will create instances of stateless services. In order to create a new instance of the fabric client we can simple call the following code below.

First of all we need to create an instance to Connect to the cluster by creating a FabricClient instance. To connect to the local development cluster, run the following example inside the webService Constructor:

```csharp
using System.Fabric;

// Further down in our constructor.
var fabricClient = new FabricClient();
```

After creating the serviceFabric instance we should add dependency injection inside the service class.

```csharp
//add dependency injection for fabricClient
.AddSingleton<FabricClient>(this.fabricClient))
```
