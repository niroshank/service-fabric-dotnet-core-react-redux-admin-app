using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IoT.Common;
using Iot.Admin.WebService.ViewModels;
using Iot.Admin.WebService.Models;
using System.Fabric.Description;
using Iot.Common;
using System.Collections.Specialized;

namespace IoT.Admin.WebService.Controllers
{
    [Route("api/[Controller]")]
    public class IngestionController : Controller
    {
        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        private readonly FabricClient fabricClient;
        private readonly IApplicationLifetime appLifetime;

        public IngestionController(FabricClient fabricClient, IApplicationLifetime appLifetime)
        {
            this.fabricClient = fabricClient;
            this.appLifetime = appLifetime;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ApplicationList applications = await this.fabricClient.QueryManager.GetApplicationListAsync();

            return this.Ok(
                applications
                    .Where(x => x.ApplicationTypeName == Names.IngestionApplicationTypeName)
                    .Select(
                        x =>
                            new ApplicationViewModel(
                                x.ApplicationName.ToString(),
                                x.ApplicationStatus.ToString(),
                                x.ApplicationTypeVersion,
                                x.ApplicationParameters)));
        }

        [HttpPost]
        [Route("{name}")]
        public async Task<IActionResult> Post([FromRoute] string name, [FromBody] IngestionApplicationParams parameters)
        {
            try
            {
                // Application parameters are passed to the Ingestion application instance.
                NameValueCollection appInstanceParameterCollection = new NameValueCollection();
                appInstanceParameterCollection["IotHubConnectionString"] = parameters.IotHubConnectionString;

                ApplicationDescription application = new ApplicationDescription(
                new Uri($"{Names.IngestionApplicationPrefix}/{name}"),
                Names.IngestionApplicationTypeName,
                parameters.Version,
                appInstanceParameterCollection);

                // Create a named application instance
                await this.fabricClient.ApplicationManager.CreateApplicationAsync(application, this.operationTimeout, this.appLifetime.ApplicationStopping);

                // Next, create named instances of the services that run in the application.
                ServiceUriBuilder serviceNameUriBuilder = new ServiceUriBuilder(application.ApplicationName.ToString(), Names.IngestionTelemetryServiceName);

                StatefulServiceDescription service = new StatefulServiceDescription()
                {
                    ApplicationName = application.ApplicationName,
                    HasPersistedState = true,
                    MinReplicaSetSize = 1,
                    TargetReplicaSetSize = 1,
                    //PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(eventHubInfo.PartitionCount, 0, eventHubInfo.PartitionCount - 1),
                    PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(parameters.PartitionCount, Int64.MinValue, Int64.MaxValue),
                    //PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                    ServiceName = serviceNameUriBuilder.Build(),
                    ServiceTypeName = Names.IngestionTelemetryServiceTypeName,
                    

                };

                await this.fabricClient.ServiceManager.CreateServiceAsync(service, this.operationTimeout, this.appLifetime.ApplicationStopping);

                return this.Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}