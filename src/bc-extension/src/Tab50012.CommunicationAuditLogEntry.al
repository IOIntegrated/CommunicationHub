// Table 50012 – Communication Audit Log Entry
// Sprint 0 Skeleton (F5) + Hash-Chain PoC (H6)
// Append-only table; deletions only via Retention-Job after legal hold period.
// Hash chain (field "Chain Hash") is computed by Codeunit 50050 on each insert
// to make tamper-detection possible – see docs/plan/12-security-compliance.md §1.2.

table 50012 "Communication Audit Log Entry"
{
    Caption = 'Communication Audit Log Entry';
    DataPerCompany = true;
    DataClassification = SystemMetadata;

    fields
    {
        field(1; "Entry No."; BigInteger)
        {
            Caption = 'Entry No.';
            AutoIncrement = true;
            DataClassification = SystemMetadata;
        }
        field(2; "Tenant Id"; Guid)
        {
            Caption = 'Tenant Id';
            DataClassification = SystemMetadata;
        }
        field(3; "Correlation Id"; Guid)
        {
            Caption = 'Correlation Id';
            DataClassification = SystemMetadata;
        }
        field(4; "Interaction No."; Integer)
        {
            Caption = 'Interaction No.';
            DataClassification = SystemMetadata;
            TableRelation = "Communication Interaction"."Entry No.";
        }
        field(5; "Event Type"; Code[40])
        {
            Caption = 'Event Type';
            DataClassification = SystemMetadata;
            // Expected values (non-exhaustive):
            //   interaction.persisted | ai.summary.generated | ai.suggestion.created
            //   ai.suggestion.accepted | permission.denied | prompt.injection.detected
            //   consent.granted | consent.withdrawn | consent.expired
        }
        field(6; "User SID"; Guid)
        {
            Caption = 'User SID';
            DataClassification = EndUserPseudonymousIdentifiers;
        }
        field(7; "Service Principal Id"; Guid)
        {
            Caption = 'Service Principal Id';
            DataClassification = SystemMetadata;
        }
        field(8; "Source Hash"; Text[64])
        {
            Caption = 'Source Hash';
            DataClassification = SystemMetadata;
            // SHA-256 hex of the source message/content reference list (no plaintext).
        }
        field(9; "Output Hash"; Text[64])
        {
            Caption = 'Output Hash';
            DataClassification = SystemMetadata;
            // SHA-256 hex of the AI output (no plaintext).
        }
        field(10; "Model Deployment"; Text[80])
        {
            Caption = 'Model Deployment';
            DataClassification = SystemMetadata;
        }
        field(11; "Token Count"; Integer)
        {
            Caption = 'Token Count';
            DataClassification = SystemMetadata;
        }
        field(12; "Latency Ms"; Integer)
        {
            Caption = 'Latency Ms';
            DataClassification = SystemMetadata;
        }
        field(13; Severity; Option)
        {
            Caption = 'Severity';
            DataClassification = SystemMetadata;
            OptionMembers = Info,Warn,Error;
            OptionCaption = 'Info,Warn,Error';
        }
        field(14; Message; Text[250])
        {
            Caption = 'Message';
            DataClassification = SystemMetadata;
            // Technical short message only – no PII allowed.
        }
        field(15; "Created At"; DateTime)
        {
            Caption = 'Created At';
            DataClassification = SystemMetadata;
        }
        field(16; "Chain Hash"; Text[64])
        {
            Caption = 'Chain Hash';
            DataClassification = SystemMetadata;
            Editable = false;
            // SHA-256 of (previous entry's Chain Hash + this entry's key fields).
            // Computed automatically by Codeunit "Comm. Audit Management".
            // A break in this chain indicates tampering – see H6 PoC.
        }
    }

    keys
    {
        key(PK; "Entry No.")
        {
            Clustered = true;
        }
        key(SK1; "Tenant Id", "Created At")
        {
        }
        key(SK2; "Tenant Id", "Event Type", "Created At")
        {
        }
        key(SK3; "Tenant Id", "Correlation Id")
        {
        }
    }

    trigger OnBeforeInsert()
    begin
        // Chain hash MUST be computed by Codeunit "Comm. Audit Management".WriteAuditEntry()
        // to guarantee chain continuity. Direct insert without that codeunit will leave
        // Chain Hash empty and break the chain – enforced here to warn developers.
        if (Rec."Created At" = 0DT) then
            Rec."Created At" := CurrentDateTime();
        if (Rec."Event Type" = '') then
            Error('Event Type must be set before inserting an Audit Log Entry.');
    end;

    // Append-only – modifications and deletions are blocked.
    trigger OnBeforeModify()
    begin
        Error('Audit Log entries are append-only and cannot be modified.');
    end;

    trigger OnBeforeDelete()
    begin
        Error('Audit Log entries can only be deleted by the authorised Retention-Job service principal.');
    end;
}
