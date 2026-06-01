// Permission Set – IOI_COMM_HUB_ADMIN  (Sprint 0 F4)
// DSB / Data-Protection-Officer admin access.
// Required for: Consent management, Setup page, Internal Domain list.
// Should NOT be assigned to regular business users.

permissionset 50062 "IOI_COMM_HUB_ADMIN"
{
    Assignable = true;
    Caption = 'CommHub - Admin', MaxLength = 30;
    IncludedPermissionSets = "IOI_COMM_HUB_EDIT";

    Permissions =
        tabledata "Communication Interaction" = RIMD,
        tabledata "Communication Consent" = RIMD,
        tabledata "Communication Audit Log Entry" = R,
        codeunit "Comm. Audit Management" = X;
}
