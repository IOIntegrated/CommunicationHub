// Table 50014 – Communication Consent
// Sprint 0 Core Deliverable (F5, DoD-2, ADR-27/A16)
// Opt-in consent register per pilot mailbox/user (Stage-0 of ingestion pipeline).
// See: docs/plan/02-bc-data-model.md §3.15, docs/plan/12-security-compliance.md §10.3
//
// GDPR requirements:
//   - Every status change writes a Communication Audit Log Entry (50012).
//   - Withdrawal takes immediate effect (Stage 0 blocks further ingestion).
//   - Pilot-Until date is mandatory; expiry auto-set by daily job (see I4).
//   - No hard-delete; records are retained for audit for the legal hold period.

table 50014 "Communication Consent"
{
    Caption = 'Communication Consent';
    DataPerCompany = true;
    DataClassification = EndUserIdentifiableInformation;
    LookupPageId = "Communication Consent List";
    DrillDownPageId = "Communication Consent List";

    fields
    {
        field(1; "Code"; Code[20])
        {
            Caption = 'Code';
            DataClassification = SystemMetadata;
            NotBlank = true;
        }
        field(2; "Tenant Id"; Guid)
        {
            Caption = 'Tenant Id';
            DataClassification = SystemMetadata;
        }
        field(3; "Mailbox Address"; Text[250])
        {
            Caption = 'Mailbox Address';
            DataClassification = EndUserIdentifiableInformation;
            NotBlank = true;
        }
        field(4; "User Id"; Guid)
        {
            Caption = 'User Id';
            DataClassification = EndUserIdentifiableInformation;
        }
        field(5; "Consent Status"; Enum "Communication Consent Status")
        {
            Caption = 'Consent Status';
            DataClassification = SystemMetadata;
        }
        field(6; "Granted At"; DateTime)
        {
            Caption = 'Granted At';
            DataClassification = SystemMetadata;
            Editable = false;
        }
        field(7; "Withdrawn At"; DateTime)
        {
            Caption = 'Withdrawn At';
            DataClassification = SystemMetadata;
            Editable = false;
        }
        field(8; "Pilot Until"; Date)
        {
            Caption = 'Pilot Until';
            DataClassification = SystemMetadata;
            NotBlank = true;
        }
        field(9; "Document Version"; Code[20])
        {
            Caption = 'Document Version';
            DataClassification = SystemMetadata;
            NotBlank = true;
            // e.g. '2026-05-DE-v1.0'
        }
        field(10; "Audit Reference"; BigInteger)
        {
            Caption = 'Audit Reference';
            DataClassification = SystemMetadata;
            Editable = false;
            // FK → Communication Audit Log Entry (50012)."Entry No."
        }
        field(11; "Withdrawn Reason"; Text[250])
        {
            Caption = 'Withdrawn Reason';
            DataClassification = CustomerContent;
        }
        field(12; Language; Code[10])
        {
            Caption = 'Language';
            DataClassification = SystemMetadata;
        }
        field(13; "Last Modified By User SID"; Guid)
        {
            Caption = 'Last Modified By User SID';
            DataClassification = EndUserPseudonymousIdentifiers;
            Editable = false;
        }
        field(14; "Last Modified At"; DateTime)
        {
            Caption = 'Last Modified At';
            DataClassification = SystemMetadata;
            Editable = false;
        }
    }

    keys
    {
        key(PK; "Code")
        {
            Clustered = true;
        }
        key(SK1; "Tenant Id")
        {
        }
        key(SK2; "Tenant Id", "Mailbox Address", "Granted At")
        {
            // Stage 0 lookup: most-recent Granted record per mailbox.
        }
        key(SK4; "Tenant Id", "Consent Status", "Pilot Until")
        {
            // Daily expiry job: find Granted records where Pilot Until < TODAY.
        }
    }

    trigger OnBeforeInsert()
    var
        AuditMgmt: Codeunit "Comm. Audit Management";
    begin
        ValidateNewConsent();
        Rec."Last Modified By User SID" := UserSecurityId();
        Rec."Last Modified At" := CurrentDateTime();
        // Write audit entry – Entry No. is returned and stored for traceability.
        Rec."Audit Reference" := AuditMgmt.LogConsentChange(
            Rec."Code", Rec."Mailbox Address", Rec."Consent Status", 'consent.granted');
    end;

    trigger OnBeforeModify()
    var
        xRec2: Record "Communication Consent";
        AuditMgmt: Codeunit "Comm. Audit Management";
        EventType: Code[40];
    begin
        Rec."Last Modified By User SID" := UserSecurityId();
        Rec."Last Modified At" := CurrentDateTime();

        if xRec2.Get(Rec.Code) then
            if xRec2."Consent Status" <> Rec."Consent Status" then begin
                case Rec."Consent Status" of
                    "Communication Consent Status"::Granted:
                        begin
                            EventType := 'consent.granted';
                            Rec."Granted At" := CurrentDateTime();
                            Rec."Withdrawn At" := 0DT;
                        end;
                    "Communication Consent Status"::Withdrawn:
                        begin
                            EventType := 'consent.withdrawn';
                            Rec."Withdrawn At" := CurrentDateTime();
                        end;
                    "Communication Consent Status"::Expired:
                        EventType := 'consent.expired';
                    else
                        EventType := 'consent.status_changed';
                end;
                Rec."Audit Reference" := AuditMgmt.LogConsentChange(
                    Rec."Code", Rec."Mailbox Address", Rec."Consent Status", EventType);
            end;
    end;

    trigger OnBeforeDelete()
    begin
        // Append-only: consent records are never hard-deleted.
        // Retention-Job handles cleanup after the legal hold period.
        Error('Consent records cannot be deleted. Set Consent Status to Withdrawn instead.');
    end;

    /// <summary>
    /// Returns true if this mailbox has a currently valid (Granted, not expired) consent.
    /// Called by ingestion Stage 0 before any data is processed.
    /// </summary>
    procedure IsConsentValid(): Boolean
    begin
        if Rec."Consent Status" <> "Communication Consent Status"::Granted then
            exit(false);
        if (Rec."Pilot Until" <> 0D) and (Rec."Pilot Until" < Today()) then
            exit(false);
        exit(true);
    end;

    /// <summary>
    /// Finds the most-recent Granted consent for a given mailbox.
    /// Returns false if no valid consent exists.
    /// </summary>
    procedure FindActiveConsent(TenantId: Guid; MailboxAddress: Text[250]): Boolean
    begin
        Reset();
        SetRange("Tenant Id", TenantId);
        SetRange("Mailbox Address", MailboxAddress);
        SetRange("Consent Status", "Communication Consent Status"::Granted);
        SetFilter("Pilot Until", '>=%1', Today());
        if FindLast() then
            exit(true);
        exit(false);
    end;

    local procedure ValidateNewConsent()
    begin
        if Rec."Pilot Until" = 0D then
            Error('Pilot Until date is required.');
        if Rec."Pilot Until" > CalcDate('<+6M>', Today()) then
            Error('Pilot Until may not exceed 6 months from today (ADR-28).');
        if Rec."Document Version" = '' then
            Error('Document Version (consent form version) is required.');
        if Rec."Mailbox Address" = '' then
            Error('Mailbox Address is required.');
    end;
}
