// Permission Set – IOI_COMM_HUB_VIEW  (Sprint 0 F4)
// Read-only access for business users viewing communication history in BC.

permissionset 50060 "IOI_COMM_HUB_VIEW"
{
    Assignable = true;
    Caption = 'CommHub - Read Only', MaxLength = 30;

    Permissions =
        tabledata "Communication Interaction" = R,
        tabledata "Communication Audit Log Entry" = R,
        tabledata "Communication Consent" = R,
        codeunit "Comm. Audit Management" = X;
}
