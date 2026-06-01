// Page 50000 – Communication Consent List  (Sprint 0 F5, DoD-2)
// Admin page for DSB/HR to manage pilot-user consent records.
// Requires permission set IOI_COMM_HUB_ADMIN.

page 50000 "Communication Consent List"
{
    Caption = 'Communication Consent';
    PageType = List;
    SourceTable = "Communication Consent";
    ApplicationArea = All;
    UsageCategory = Administration;
    CardPageId = "Communication Consent Card";
    InsertAllowed = true;
    ModifyAllowed = true;
    DeleteAllowed = false;  // Append-only – see Tab50014 OnBeforeDelete

    layout
    {
        area(Content)
        {
            repeater(ConsentLines)
            {
                field("Code"; Rec."Code")
                {
                    ApplicationArea = All;
                    ToolTip = 'Unique consent identifier (e.g. CC-0001).';
                }
                field("Mailbox Address"; Rec."Mailbox Address")
                {
                    ApplicationArea = All;
                    ToolTip = 'UPN of the pilot mailbox that gave consent.';
                }
                field("Consent Status"; Rec."Consent Status")
                {
                    ApplicationArea = All;
                    ToolTip = 'Current consent status.';
                    StyleExpr = ConsentStatusStyle;
                }
                field("Pilot Until"; Rec."Pilot Until")
                {
                    ApplicationArea = All;
                    ToolTip = 'Consent is valid until this date (max. 6 months).';
                }
                field("Document Version"; Rec."Document Version")
                {
                    ApplicationArea = All;
                    ToolTip = 'Version of the consent form presented to the user.';
                }
                field("Granted At"; Rec."Granted At")
                {
                    ApplicationArea = All;
                    ToolTip = 'When consent was granted.';
                }
                field("Withdrawn At"; Rec."Withdrawn At")
                {
                    ApplicationArea = All;
                    ToolTip = 'When consent was withdrawn (if applicable).';
                }
            }
        }
        area(FactBoxes)
        {
            systempart(Links; Links) { ApplicationArea = RecordLinks; }
            systempart(Notes; Notes) { ApplicationArea = Notes; }
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
                ToolTip = 'Set status to Granted for the selected record.';

                trigger OnAction()
                begin
                    SetConsentStatus("Communication Consent Status"::Granted);
                end;
            }
            action(WithdrawConsent)
            {
                Caption = 'Withdraw Consent';
                ApplicationArea = All;
                Image = Cancel;
                ToolTip = 'Set status to Withdrawn. Takes immediate effect – ingestion is blocked.';

                trigger OnAction()
                begin
                    if not Confirm('Withdraw consent for %1? Ingestion will stop immediately.', false, Rec."Mailbox Address") then
                        exit;
                    SetConsentStatus("Communication Consent Status"::Withdrawn);
                end;
            }
            action(VerifyAuditChain)
            {
                Caption = 'Verify Audit Chain';
                ApplicationArea = All;
                Image = CheckRulesSyntax;
                ToolTip = 'Verify the tamper-detection hash chain in the Audit Log (H6 PoC).';

                trigger OnAction()
                var
                    AuditMgmt: Codeunit "Comm. Audit Management";
                    Setup: Record "Communication Setup";
                    BreakAt: BigInteger;
                begin
                    if not Setup.Get() then
                        Error('Communication Setup not configured.');
                    if AuditMgmt.VerifyChainIntegrity(Setup."Tenant Id", BreakAt) then
                        Message('Audit chain is intact. No tampering detected.')
                    else
                        Error('Chain break detected at Entry No. %1. Contact the security team.', BreakAt);
                end;
            }
        }
        area(Navigation)
        {
            action(AuditLog)
            {
                Caption = 'Audit Log';
                ApplicationArea = All;
                Image = Log;
                ToolTip = 'View audit log entries for this consent record.';
                RunObject = Page "Communication Audit Log Entry";
            }
        }
    }

    trigger OnAfterGetRecord()
    begin
        SetConsentStatusStyle();
    end;

    var
        ConsentStatusStyle: Text;

    local procedure SetConsentStatus(NewStatus: Enum "Communication Consent Status")
    begin
        Rec.Validate("Consent Status", NewStatus);
        Rec.Modify(true);
        CurrPage.Update(false);
    end;

    local procedure SetConsentStatusStyle()
    begin
        case Rec."Consent Status" of
            "Communication Consent Status"::Granted:
                ConsentStatusStyle := 'Favorable';
            "Communication Consent Status"::Withdrawn,
            "Communication Consent Status"::Expired:
                ConsentStatusStyle := 'Unfavorable';
            "Communication Consent Status"::Pending:
                ConsentStatusStyle := 'Ambiguous';
            else
                ConsentStatusStyle := 'Standard';
        end;
    end;
}
