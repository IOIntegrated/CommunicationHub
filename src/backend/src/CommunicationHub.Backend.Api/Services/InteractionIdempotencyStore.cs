using CommunicationHub.Backend.Core.Models;
using System.Collections.Concurrent;

namespace CommunicationHub.Backend.Api.Services;

public interface IInteractionIdempotencyStore
{
    bool TryGet(string key, out InteractionSaveResult existing);
    void Store(string key, InteractionSaveResult result);
}

public sealed class InMemoryInteractionIdempotencyStore : IInteractionIdempotencyStore
{
    private readonly ConcurrentDictionary<string, InteractionSaveResult> _entries = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string key, out InteractionSaveResult existing)
    {
        if (_entries.TryGetValue(key, out var value))
        {
            existing = value;
            return true;
        }

        existing = default!;
        return false;
    }

    public void Store(string key, InteractionSaveResult result)
    {
        _entries.TryAdd(key, result);
    }
}
