// Page 50001 – Communication Consent Card  (Sprint 0 F5, DoD-2)
// Detail card for a single pilot-user consent record.

page 50001 "Communication Consent Card"
{
    Caption = 'Communication Consent';
    PageType = Card;
    SourceTable = "Communication Consent";
    ApplicationArea = All;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            group(General)
            {
                Caption = 'General';

                field("Code"; Rec."Code")
                {
                    ApplicationArea = All;
                    ToolTip = 'Unique consent identifier.';
                    Editable = Rec."Consent Status" = "Communication Consent Status"::Pending;
                }
                field("Mailbox Address"; Rec."Mailbox Address")
                {
                    ApplicationArea = All;
                    ToolTip = 'UPN of the pilot mailbox that gave consent.';
                }
                field("User Id"; Rec."User Id")
                {
                    ApplicationArea = All;
                    ToolTip = 'AAD Object ID of the consenting employee.';
                }
                field(Language; Rec.Language)
                {
                    ApplicationArea = All;
                    ToolTip = 'Language of the consent form shown (de / en).';
                }
                field("Document Version"; Rec."Document Version")
                {
                    ApplicationArea = All;
                    ToolTip = 'Version of the consent form (e.g. 2026-05-DE-v1.0).';
                }
            }
            group(ConsentDetails)
            {
                Caption = 'Consent Status';

                field("Consent Status"; Rec."Consent Status")
                {
                    ApplicationArea = All;
                    ToolTip = 'Current consent status. Use actions to change it.';
                    Editable = false;
                }
                field("Pilot Until"; Rec."Pilot Until")
                {
                    ApplicationArea = All;
                    ToolTip = 'Consent validity end date (max. 6 months from grant date per ADR-28).';
                }
                field("Granted At"; Rec."Granted At")
                {
                    ApplicationArea = All;
                    Editable = false;
                    ToolTip = 'Timestamp when consent was granted.';
                }
                field("Withdrawn At"; Rec."Withdrawn At")
                {
                    ApplicationArea = All;
                    Editable = false;
                    ToolTip = 'Timestamp when consent was withdrawn.';
                }
                field("Withdrawn Reason"; Rec."Withdrawn Reason")
                {
                    ApplicationArea = All;
                    MultiLine = true;
                    ToolTip = 'Optional reason for withdrawal (not required by law).';
                }
            }
            group(Audit)
            {
                Caption = 'Audit';

                field("Audit Reference"; Rec."Audit Reference")
                {
                    ApplicationArea = All;
                    Editable = false;
                    ToolTip = 'Entry No. of the last Audit Log entry for this consent.';
                }
                field("Last Modified By User SID"; Rec."Last Modified By User SID")
                {
                    ApplicationArea = All;
                    Editable = false;
                    ToolTip = 'BC User Security ID that last changed this record.';
                }
                field("Last Modified At"; Rec."Last Modified At")
                {
                    ApplicationArea = All;
                    Editable = false;
                    ToolTip = 'When this record was last changed.';
                }
            }
        }
    }

    actions
    {
        area(Processing)
        {
            action(GrantConsent)
            {
                Caption = 'Grant Consent';
                ApplicationArea = All;
                Image = Approve;
                Enabled = Rec."Consent Status" <> "Communication Consent Status"::Granted;
                ToolTip = 'Mark this consent as Granted.';

                trigger OnAction()
                begin
                    Rec.Validate("Consent Status", "Communication Consent Status"::Granted);
                    Rec."Granted At" := CurrentDateTime();
                    Rec.Modify(true);
                    CurrPage.Update(false);
                end;
            }
            action(WithdrawConsent)
            {
                Caption = 'Withdraw Consent';
                ApplicationArea = All;
                Image = Cancel;
                Enabled = Rec."Consent Status" = "Communication Consent Status"::Granted;
                ToolTip = 'Withdraw consent immediately. Ingestion will be blocked.';

                trigger OnAction()
                begin
                    if not Confirm('Withdraw consent for %1? Ingestion will stop immediately.', false, Rec."Mailbox Address") then
                        exit;
                    Rec.Validate("Consent Status", "Communication Consent Status"::Withdrawn);
                    Rec.Modify(true);
                    CurrPage.Update(false);
                end;
            }
        }
    }
}
