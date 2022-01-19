using Orleans;

namespace GrainInterfaces;

public interface IExplicitConsumerGrain : IGrainWithGuidKey
{
    Task<int> GetReceivedCount();
    Task Subscribe();
}