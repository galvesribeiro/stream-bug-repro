using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Streams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamTest;

var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(log => log.AddConsole())
    .UseOrleans((ctx, siloBuilder) =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.AddMemoryGrainStorage("PubSubStore", opt => opt.NumStorageGrains = 100);
        siloBuilder.AddMemoryGrainStorageAsDefault(opt => opt.NumStorageGrains = 100);

        siloBuilder.AddEventHubStreams(Constants.ExplicitStreamProvider, configurator =>
        {
            configurator.ConfigureEventHub(ev => ev.Configure(opt =>
            {
                opt.ConnectionString = Constants.ExplicitEHConnectionString;
                opt.ConsumerGroup = Constants.ExplicitConsumerGroup;
                opt.Path = Constants.ExplicitEH;
            }));

            configurator.UseAzureTableCheckpointer(atc => atc.Configure(opt =>
            {
                opt.ConnectionString = Constants.StorageConnectionString;
                opt.TableName = Constants.ExplicitCheckpoint;
            }));

            configurator.UseLeaseBasedQueueBalancer();
        });

        siloBuilder.AddEventHubStreams(Constants.ImplicitStreamProvider, configurator =>
        {
            configurator.ConfigureEventHub(ev => ev.Configure(opt =>
            {
                opt.ConnectionString = Constants.ImplicitEHConnectionString;
                opt.ConsumerGroup = Constants.ImplicitConsumerGroup;
                opt.Path = Constants.ImplicitEH;
            }));

            configurator.UseAzureTableCheckpointer(atc => atc.Configure(opt =>
            {
                opt.ConnectionString = Constants.StorageConnectionString;
                opt.TableName = Constants.ImplicitCheckpoint;
            }));

            configurator.UseLeaseBasedQueueBalancer();
            configurator.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
        });

        siloBuilder.UseAzureBlobLeaseProvider(configurator => configurator.Configure(opt =>
        {
            opt.DataConnectionString = Constants.StorageConnectionString;
            opt.BlobContainerName = Constants.LeasesBlob;
        }));
    }).Build();

await host.RunAsync();
