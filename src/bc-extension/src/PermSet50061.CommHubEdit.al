// Permission Set – IOI_COMM_HUB_EDIT  (Sprint 0 F4)
// For pilot users and BC agents who can view and correct matching/entity links.
// Inherits read permissions; adds Insert/Modify on Interaction and related tables.

permissionset 50061 "IOI_COMM_HUB_EDIT"
{
    Assignable = true;
    Caption = 'CommHub - Edit', MaxLength = 30;
    IncludedPermissionSets = "IOI_COMM_HUB_VIEW";

    Permissions =
        tabledata "Communication Interaction" = RIM,
        tabledata "Communication Consent" = R,          // Edit forbidden; DSB only (ADMIN)
        tabledata "Communication Audit Log Entry" = R,  // Append-only via codeunit
        codeunit "Comm. Audit Management" = X;
}
