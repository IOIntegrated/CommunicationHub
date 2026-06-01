// Table 50010 – Communication Setup (Singleton)
// Sprint 0 Skeleton – referenced by other objects (Consent List page, Audit Mgmt).

table 50010 "Communication Setup"
{
    Caption = 'Communication Setup';
    DataPerCompany = true;
    DataClassification = SystemMetadata;

    fields
    {
        field(1; "Primary Key"; Code[10])
        {
            Caption = 'Primary Key';
            DataClassification = SystemMetadata;
        }
        field(2; "Backend Base Url"; Text[250])
        {
            Caption = 'Backend Base URL';
            DataClassification = SystemMetadata;
        }
        field(3; "Tenant Id"; Guid)
        {
            Caption = 'Tenant Id';
            DataClassification = SystemMetadata;
        }
        field(4; "Pilot User Group Code"; Code[20])
        {
            Caption = 'Pilot User Group Code';
            DataClassification = SystemMetadata;
        }
        field(5; "Default Visibility Scope"; Enum "Communication Visibility Scope")
        {
            Caption = 'Default Visibility Scope';
            DataClassification = SystemMetadata;
            InitValue = "Owner Team";
        }
        field(6; "Default Sensitivity Level"; Enum "Communication Sensitivity Level")
        {
            Caption = 'Default Sensitivity Level';
            DataClassification = SystemMetadata;
            InitValue = Internal;
        }
        field(7; "Default Retention Days"; Integer)
        {
            Caption = 'Default Retention Days';
            DataClassification = SystemMetadata;
            InitValue = 730;  // 2 years default
        }
        field(8; "Allow Manual Capture"; Boolean)
        {
            Caption = 'Allow Manual Capture';
            DataClassification = SystemMetadata;
        }
        field(9; "Backend Service Principal Id"; Guid)
        {
            Caption = 'Backend Service Principal Id';
            DataClassification = SystemMetadata;
        }
        field(10; "Long Summary Blob Container"; Text[100])
        {
            Caption = 'Long Summary Blob Container';
            DataClassification = SystemMetadata;
        }
    }

    keys
    {
        key(PK; "Primary Key")
        {
            Clustered = true;
        }
    }
}
