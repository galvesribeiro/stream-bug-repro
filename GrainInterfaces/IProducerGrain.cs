using Orleans;

namespace GrainInterfaces;

public interface IProducerGrain : IGrainWithGuidKey
{
    Task Produce(int i);
}