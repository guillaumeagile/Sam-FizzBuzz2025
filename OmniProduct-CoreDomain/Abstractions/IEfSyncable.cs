namespace OmniProduct_CoreDomain.Abstractions;

// Persistence concerns: keeping domain fields and flattened EF columns in sync.
public interface IEfSyncable
{
    void SyncEfColumns();
    void HydrateFromEfColumns();
}
