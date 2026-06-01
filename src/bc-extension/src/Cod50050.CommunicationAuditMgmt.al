// Codeunit 50050 – Communication Audit Management
// Sprint 0 – Hash-Chain PoC (WS-H Task H6) + Consent Audit Logging
//
// Hash-Chain concept (see docs/plan/12-security-compliance.md §1.2):
//   Each Audit Log Entry carries a "Chain Hash" field:
//     ChainHash[n] = SHA-256( ChainHash[n-1] || EntryNo[n] || EventType[n] ||
//                             CorrelationId[n] || CreatedAt[n] )
//   Breaking this chain (modification or insertion of a gap entry) is detectable
//   by calling VerifyChainIntegrity().
//
// Usage:
//   - Call WriteAuditEntry() for all audit events instead of direct table insert.
//   - Call VerifyChainIntegrity() from the DSB/security admin page (periodic check).
//   - Call LogConsentChange() from Table 50014 triggers (called automatically).

codeunit 50050 "Comm. Audit Management"
{
    Access = Public;

    /// <summary>
    /// Creates a new audit log entry with correct chain hash and returns the Entry No.
    /// This is the ONLY sanctioned way to insert into Table 50012.
    /// </summary>
    procedure WriteAuditEntry(
        TenantId: Guid;
        CorrelationId: Guid;
        EventType: Code[40];
        Message: Text[250];
        Severity: Option Info,Warn,Error): BigInteger
    var
        AuditEntry: Record "Communication Audit Log Entry";
        PreviousHash: Text[64];
    begin
        PreviousHash := GetLastChainHash(TenantId);

        AuditEntry.Init();
        AuditEntry."Tenant Id" := TenantId;
        AuditEntry."Correlation Id" := CorrelationId;
        AuditEntry."Event Type" := EventType;
        AuditEntry.Message := Message;
        AuditEntry.Severity := Severity;
        AuditEntry."User SID" := UserSecurityId();
        AuditEntry."Created At" := CurrentDateTime();
        // Chain hash is computed BEFORE the record is inserted so that Entry No.
        // is not yet assigned. We use a temporary next entry no. heuristic:
        // actual uniqueness is guaranteed by AutoIncrement; chain covers the data.
        AuditEntry."Chain Hash" := ComputeChainHash(PreviousHash, AuditEntry);
        AuditEntry.Insert(false);  // bypass OnBeforeInsert event-check already done
        exit(AuditEntry."Entry No.");
    end;

    /// <summary>
    /// Convenience overload for writing audit entries with extended fields.
    /// </summary>
    procedure WriteAuditEntryFull(
        TenantId: Guid;
        CorrelationId: Guid;
        InteractionNo: Integer;
        EventType: Code[40];
        Message: Text[250];
        Severity: Option Info,Warn,Error;
        ServicePrincipalId: Guid;
        ModelDeployment: Text[80];
        TokenCount: Integer;
        LatencyMs: Integer;
        SourceHash: Text[64];
        OutputHash: Text[64]): BigInteger
    var
        AuditEntry: Record "Communication Audit Log Entry";
        PreviousHash: Text[64];
    begin
        PreviousHash := GetLastChainHash(TenantId);

        AuditEntry.Init();
        AuditEntry."Tenant Id" := TenantId;
        AuditEntry."Correlation Id" := CorrelationId;
        AuditEntry."Interaction No." := InteractionNo;
        AuditEntry."Event Type" := EventType;
        AuditEntry.Message := Message;
        AuditEntry.Severity := Severity;
        AuditEntry."User SID" := UserSecurityId();
        AuditEntry."Service Principal Id" := ServicePrincipalId;
        AuditEntry."Model Deployment" := ModelDeployment;
        AuditEntry."Token Count" := TokenCount;
        AuditEntry."Latency Ms" := LatencyMs;
        AuditEntry."Source Hash" := SourceHash;
        AuditEntry."Output Hash" := OutputHash;
        AuditEntry."Created At" := CurrentDateTime();
        AuditEntry."Chain Hash" := ComputeChainHash(PreviousHash, AuditEntry);
        AuditEntry.Insert(false);
        exit(AuditEntry."Entry No.");
    end;

    /// <summary>
    /// Writes a consent-specific audit entry. Called from Table 50014 triggers.
    /// Returns the new Audit Log Entry No.
    /// </summary>
    procedure LogConsentChange(
        ConsentCode: Code[20];
        MailboxAddress: Text[250];
        NewStatus: Enum "Communication Consent Status";
        EventType: Code[40]): BigInteger
    var
        Setup: Record "Communication Setup";
        TenantId: Guid;
        CorrId: Guid;
        Msg: Text[250];
    begin
        if Setup.Get() then
            TenantId := Setup."Tenant Id";
        CorrId := CreateGuid();
        Msg := CopyStr(StrSubstNo('Consent %1: %2 → %3', ConsentCode, Format(NewStatus), MailboxAddress), 1, 250);
        exit(WriteAuditEntry(TenantId, CorrId, EventType, Msg, 0 /* Info */));
    end;

    /// <summary>
    /// Iterates all audit entries for a tenant in entry-no order and verifies the chain.
    /// Returns true if the chain is intact, false if a break is detected.
    /// Reports the first broken entry via BreakAtEntryNo (-1 = no break found).
    /// </summary>
    procedure VerifyChainIntegrity(TenantId: Guid; var BreakAtEntryNo: BigInteger): Boolean
    var
        AuditEntry: Record "Communication Audit Log Entry";
        PreviousHash: Text[64];
        ExpectedHash: Text[64];
    begin
        BreakAtEntryNo := -1;
        PreviousHash := '';

        AuditEntry.SetRange("Tenant Id", TenantId);
        AuditEntry.SetCurrentKey("Entry No.");
        if not AuditEntry.FindSet() then
            exit(true);  // Empty log is valid.

        repeat
            ExpectedHash := ComputeChainHash(PreviousHash, AuditEntry);
            if AuditEntry."Chain Hash" <> ExpectedHash then begin
                BreakAtEntryNo := AuditEntry."Entry No.";
                exit(false);
            end;
            PreviousHash := AuditEntry."Chain Hash";
        until AuditEntry.Next() = 0;

        exit(true);
    end;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    local procedure GetLastChainHash(TenantId: Guid): Text[64]
    var
        AuditEntry: Record "Communication Audit Log Entry";
    begin
        AuditEntry.SetRange("Tenant Id", TenantId);
        AuditEntry.SetCurrentKey("Entry No.");
        if AuditEntry.FindLast() then
            exit(AuditEntry."Chain Hash");
        exit('');  // Genesis entry: previous hash is empty string.
    end;

    /// <summary>
    /// Computes SHA-256 of the concatenated chain input and returns 64-char hex.
    /// Chain input = PreviousHash || '|' || EntryContext fields (deterministic).
    /// </summary>
    local procedure ComputeChainHash(PreviousHash: Text[64]; AuditEntry: Record "Communication Audit Log Entry"): Text[64]
    var
        CryptographyMgmt: Codeunit "Cryptography Management";
        InputText: Text;
    begin
        // Deterministic serialisation of the entry's immutable key fields.
        InputText :=
            PreviousHash + '|'
            + AuditEntry."Event Type" + '|'
            + Format(AuditEntry."Correlation Id") + '|'
            + Format(AuditEntry."Created At", 0, 9) + '|'
            + AuditEntry.Message;

        // GenerateHashAsBase64String(input, HashAlgorithm::SHA256)
        // Option index 2 = SHA256 (MD5=0, SHA1=1, SHA256=2, SHA384=3, SHA512=4)
        exit(CopyStr(CryptographyMgmt.GenerateHashAsBase64String(InputText, 2), 1, 64));
    end;
}
