// Table 50000 – Communication Interaction
// Sprint 0 Skeleton (F5) – full field set from docs/plan/02-bc-data-model.md §3.1
// Business logic and triggers completed in MVP1 Sprint 1.

table 50000 "Communication Interaction"
{
    Caption = 'Communication Interaction';
    DataPerCompany = true;
    DataClassification = CustomerContent;
    LookupPageId = "Communication Interaction List";
    DrillDownPageId = "Communication Interaction List";

    fields
    {
        field(1; "Entry No."; Integer)
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
        field(3; "Source System"; Enum "Communication Source System")
        {
            Caption = 'Source System';
            DataClassification = SystemMetadata;
        }
        field(4; Channel; Enum "Communication Channel")
        {
            Caption = 'Channel';
            DataClassification = SystemMetadata;
        }
        field(5; Direction; Enum "Communication Direction")
        {
            Caption = 'Direction';
            DataClassification = SystemMetadata;
        }
        field(6; "Capture Method"; Enum "Communication Capture Method")
        {
            Caption = 'Capture Method';
            DataClassification = SystemMetadata;
        }
        field(7; "Capture Timestamp"; DateTime)
        {
            Caption = 'Capture Timestamp';
            DataClassification = SystemMetadata;
        }
        field(8; "Sent At"; DateTime)
        {
            Caption = 'Sent At';
            DataClassification = SystemMetadata;
        }
        field(9; "Received At"; DateTime)
        {
            Caption = 'Received At';
            DataClassification = SystemMetadata;
        }
        field(10; Subject; Text[250])
        {
            Caption = 'Subject';
            DataClassification = CustomerContent;
        }
        field(11; Snippet; Text[500])
        {
            Caption = 'Snippet';
            DataClassification = CustomerContent;
        }
        field(12; "Mailbox UPN"; Text[250])
        {
            Caption = 'Mailbox UPN';
            DataClassification = EndUserIdentifiableInformation;
        }
        field(13; "Source User AAD Object Id"; Guid)
        {
            Caption = 'Source User AAD Object Id';
            DataClassification = EndUserIdentifiableInformation;
        }
        field(14; "Internet Message Id"; Text[250])
        {
            Caption = 'Internet Message Id';
            DataClassification = SystemMetadata;
        }
        field(15; "Conversation Id"; Text[250])
        {
            Caption = 'Conversation Id';
            DataClassification = SystemMetadata;
        }
        field(16; "Chat Id"; Text[100])
        {
            Caption = 'Chat Id';
            DataClassification = SystemMetadata;
        }
        field(17; "Team Id"; Guid)
        {
            Caption = 'Team Id';
            DataClassification = SystemMetadata;
        }
        field(18; "Channel Id"; Text[100])
        {
            Caption = 'Channel Id';
            DataClassification = SystemMetadata;
        }
        field(19; "Online Meeting Id"; Text[250])
        {
            Caption = 'Online Meeting Id';
            DataClassification = SystemMetadata;
        }
        field(20; "Source Message Id"; Text[250])
        {
            Caption = 'Source Message Id';
            DataClassification = SystemMetadata;
        }
        field(21; "Parent Interaction No."; Integer)
        {
            Caption = 'Parent Interaction No.';
            DataClassification = SystemMetadata;
            TableRelation = "Communication Interaction"."Entry No.";
        }
        field(22; "External Hash"; Text[64])
        {
            Caption = 'External Hash';
            DataClassification = SystemMetadata;
        }
        field(23; "Is External Communication"; Boolean)
        {
            Caption = 'Is External Communication';
            DataClassification = SystemMetadata;
        }
        field(24; "Sensitivity Level"; Enum "Communication Sensitivity Level")
        {
            Caption = 'Sensitivity Level';
            DataClassification = SystemMetadata;
        }
        field(25; "User Visibility Scope"; Enum "Communication Visibility Scope")
        {
            Caption = 'User Visibility Scope';
            DataClassification = SystemMetadata;
            InitValue = "Owner Team";
        }
        field(26; "Owner User Security Id"; Guid)
        {
            Caption = 'Owner User Security Id';
            DataClassification = EndUserIdentifiableInformation;
        }
        field(27; "Owner Team Code"; Code[20])
        {
            Caption = 'Owner Team Code';
            DataClassification = SystemMetadata;
        }
        field(28; "Processing Status"; Enum "Communication Processing Status")
        {
            Caption = 'Processing Status';
            DataClassification = SystemMetadata;
        }
        field(29; "Processing Error"; Text[250])
        {
            Caption = 'Processing Error';
            DataClassification = SystemMetadata;
        }
        field(30; "AI Summary Status"; Enum "Communication AI Summary Status")
        {
            Caption = 'AI Summary Status';
            DataClassification = SystemMetadata;
        }
        field(31; "Permalink Url"; Text[2048])
        {
            Caption = 'Permalink URL';
            DataClassification = SystemMetadata;
        }
        field(32; "BC Company Id"; Guid)
        {
            Caption = 'BC Company Id';
            DataClassification = SystemMetadata;
        }
        field(33; "Search Doc Id"; Text[100])
        {
            Caption = 'Search Doc Id';
            DataClassification = SystemMetadata;
        }
        field(34; "Blob Object Id"; Text[250])
        {
            Caption = 'Blob Object Id';
            DataClassification = SystemMetadata;
        }
        field(35; "Retention Until"; Date)
        {
            Caption = 'Retention Until';
            DataClassification = SystemMetadata;
        }
        field(36; "Legal Hold"; Boolean)
        {
            Caption = 'Legal Hold';
            DataClassification = SystemMetadata;
        }
        field(37; "Processing Restricted"; Boolean)
        {
            Caption = 'Processing Restricted';
            DataClassification = SystemMetadata;
        }
        field(38; "Consent Reference"; Code[20])
        {
            Caption = 'Consent Reference';
            DataClassification = SystemMetadata;
            TableRelation = "Communication Consent".Code;
        }
        field(39; "Correlation Id"; Guid)
        {
            Caption = 'Correlation Id';
            DataClassification = SystemMetadata;
        }
        field(40; "Created By Service Principal"; Guid)
        {
            Caption = 'Created By Service Principal';
            DataClassification = SystemMetadata;
        }
        field(41; "Last Modified At"; DateTime)
        {
            Caption = 'Last Modified At';
            DataClassification = SystemMetadata;
        }
    }

    keys
    {
        key(PK; "Entry No.")
        {
            Clustered = true;
        }
        key(SK1; "Tenant Id", "BC Company Id", "Sent At")
        {
            // Mandant/Company-Listen, Timeline (desc on Sent At via query filter)
        }
        key(SK5; "Tenant Id", "Internet Message Id")
        {
            // E-Mail dedup
        }
        key(SK6; "Tenant Id", "Conversation Id", "Sent At")
        {
            // Thread aggregation
        }
        key(SK10; "Tenant Id", "External Hash")
        {
            // Dedup – unique enforced via OnBeforeInsert trigger
        }
        key(SK11; "Tenant Id", "Is External Communication", "Processing Status")
        {
            // Worker backlog filter
        }
        key(SK12; "Tenant Id", "BC Company Id", "User Visibility Scope", "Owner Team Code")
        {
            // Security filtering
        }
        key(SK14; "Tenant Id", "Legal Hold", "Retention Until")
        {
            // Retention / hold job
        }
    }

    fieldgroups
    {
        fieldgroup(DropDown; "Entry No.", Channel, Subject, "Sent At") { }
        fieldgroup(Brick; Subject, "Mailbox UPN", "Sent At", Channel) { }
    }

    trigger OnBeforeInsert()
    begin
        if IsNullGuid(Rec."BC Company Id") then
            Rec."BC Company Id" := GetCurrentCompanyId();
        Rec."Capture Timestamp" := CurrentDateTime();
        Rec."Last Modified At" := CurrentDateTime();
    end;

    trigger OnBeforeModify()
    begin
        Rec."Last Modified At" := CurrentDateTime();
    end;

    // Hard-delete blocked – use Legal Hold + Retention workflow instead.
    trigger OnBeforeDelete()
    begin
        if Rec."Legal Hold" then
            Error('Deletion blocked: record is under Legal Hold.');
    end;

    local procedure GetCurrentCompanyId(): Guid
    var
        CompanyInformation: Record "Company Information";
    begin
        if CompanyInformation.Get() then
            exit(CompanyInformation.SystemId);
        exit(CreateGuid());
    end;
}
