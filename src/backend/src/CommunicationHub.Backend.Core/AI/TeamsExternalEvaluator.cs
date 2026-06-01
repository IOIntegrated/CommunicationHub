namespace CommunicationHub.Backend.Core.AI;

public static class TeamsExternalEvaluator
{
    public static bool HasExternalParticipation(
        string mailboxUpn,
        string? senderUpn,
        IEnumerable<string>? participantUpns)
    {
        var internalDomain = ExtractDomain(mailboxUpn);
        if (string.IsNullOrWhiteSpace(internalDomain))
            return true;

        if (IsExternal(senderUpn, internalDomain))
            return true;

        foreach (var participant in participantUpns ?? [])
        {
            if (IsExternal(participant, internalDomain))
                return true;
        }

        return false;
    }

    private static bool IsExternal(string? upn, string internalDomain)
    {
        var domain = ExtractDomain(upn);
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        return !string.Equals(domain, internalDomain, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractDomain(string? upn)
    {
        if (string.IsNullOrWhiteSpace(upn))
            return string.Empty;

        var at = upn.LastIndexOf('@');
        if (at < 0 || at + 1 >= upn.Length)
            return string.Empty;

        return upn[(at + 1)..].Trim().ToLowerInvariant();
    }
}
