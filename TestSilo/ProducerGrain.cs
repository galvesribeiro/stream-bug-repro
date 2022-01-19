using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;

namespace StreamTest;

public class ProducerGrain : Grain, IProducerGrain
{
    private readonly ILogger _logger;

    private IAsyncStream<int>? _implicitStream = null;
    private IAsyncStream<int>? _explicitStream = null;

    public ProducerGrain(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<ProducerGrain>();
    }

    public override Task OnActivateAsync()
    {
        var implicitProvider = this.GetStreamProvider(Constants.ImplicitStreamProvider);
        var explicitProvider = this.GetStreamProvider(Constants.ExplicitStreamProvider);

        this._implicitStream = implicitProvider.GetStream<int>(this.GetPrimaryKey(), Constants.ImplicitStreamNamespace);
        this._explicitStream = explicitProvider.GetStream<int>(this.GetPrimaryKey(), Constants.ExplicitStreamNamespace);

        return base.OnActivateAsync();
    }

    public async Task Produce(int i)
    {
        await this._implicitStream!.OnNextAsync(i);
        await this._explicitStream!.OnNextAsync(i);
        // return Task.WhenAll(
        //     this._implicitStream!.OnNextAsync(i),
        //     this._explicitStream!.OnNextAsync(i));
    }
}