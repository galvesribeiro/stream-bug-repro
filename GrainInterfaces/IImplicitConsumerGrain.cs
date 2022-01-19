using Orleans;

namespace GrainInterfaces;

public interface IImplicitConsumerGrain : IGrainWithGuidKey
{
    Task<int> GetReceivedCount();
}
