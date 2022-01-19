
using GrainInterfaces;
using Orleans;

var client = new ClientBuilder().UseLocalhostClustering().Build();
await client.Connect();

var count = 100;
var id = Guid.NewGuid();
var implicitConsumer = client.GetGrain<IImplicitConsumerGrain>(id);
var explicitConsumer = client.GetGrain<IExplicitConsumerGrain>(id);

await explicitConsumer.Subscribe();

var producer = client.GetGrain<IProducerGrain>(id);

var tasks = new Task[count];
for (var i = 0; i < count; i++)
{
    tasks[i] = producer.Produce(i);
}

await Task.WhenAll(tasks);

await Task.Delay(TimeSpan.FromSeconds(60));

var implicitCount = await implicitConsumer.GetReceivedCount();        
var explicitCount = await explicitConsumer.GetReceivedCount();

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine($"-- Sent {count} messages to {id} --");
Console.WriteLine($"Implicit received: {implicitCount}");
Console.WriteLine($"Explicit received: {explicitCount}");