using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;

namespace StreamTest;


[ImplicitStreamSubscription(Constants.ImplicitStreamNamespace)]
public class ImplicitConsumerGrain : Grain, IImplicitConsumerGrain
{
    private readonly ILogger _logger;
    private int _count;

    public ImplicitConsumerGrain(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<ImplicitConsumerGrain>();
    }

    public Task<int> GetReceivedCount()
    {
        return Task.FromResult(this._count);
    }

    public override Task OnActivateAsync()
    {
        var implicitProvider = this.GetStreamProvider(Constants.ImplicitStreamProvider);
        var stream = implicitProvider.GetStream<int>(this.GetPrimaryKey(), Constants.ImplicitStreamNamespace);
        stream.SubscribeAsync(this.OnNext);

        return base.OnActivateAsync();
    }

    public Task OnNext(int item, StreamSequenceToken? token = null)
    {
        this._logger.LogInformation($"Received implicit: {item}");
        this._count++;
        return Task.CompletedTask;
    }
}

public class ExplicitConsumerGrain : Grain, IExplicitConsumerGrain
{
    private readonly ILogger _logger;
    private int _count;
    private IAsyncStream<int>? _stream = null;

    public ExplicitConsumerGrain(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<ExplicitConsumerGrain>();
    }

    public Task<int> GetReceivedCount()
    {
        return Task.FromResult(this._count);
    }

    public override async Task OnActivateAsync()
    {
        var explicitProvider = this.GetStreamProvider(Constants.ExplicitStreamProvider);
        this._stream = explicitProvider.GetStream<int>(this.GetPrimaryKey(), Constants.ExplicitStreamNamespace);

        var subs = await this._stream.GetAllSubscriptionHandles();
        if (subs.Count > 0)
        {
            foreach (var sub in subs)
            {
                await sub.ResumeAsync(this.OnNext);
            }
        }
    }

    public Task Subscribe()
    {
        return this._stream.SubscribeAsync(this.OnNext);
    }

    public Task OnNext(int item, StreamSequenceToken? token = null)
    {
        this._logger.LogInformation($"Received explicit: {item}");
        this._count++;
        return Task.CompletedTask;
    }
}
