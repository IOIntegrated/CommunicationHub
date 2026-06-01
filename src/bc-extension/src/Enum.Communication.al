// Customer Communication Copilot – Enumerations
// Pilot Object Range: 50000–50099  (see docs/plan/02-bc-data-model.md §2)
// All enums are Extensible = true to enable third-party channel integrations.

enum 50000 "Communication Channel"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; Email) { Caption = 'Email'; }
    value(2; "Teams Chat") { Caption = 'Teams Chat'; }
    value(3; "Teams Channel") { Caption = 'Teams Channel'; }
    value(4; "Teams Meeting") { Caption = 'Teams Meeting'; }
    value(5; Manual) { Caption = 'Manual'; }
    value(6; Other) { Caption = 'Other'; }
}

enum 50001 "Communication Direction"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; Inbound) { Caption = 'Inbound'; }
    value(2; Outbound) { Caption = 'Outbound'; }
    value(3; Internal) { Caption = 'Internal'; }
}

enum 50002 "Communication Capture Method"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; "Server Ingestion") { Caption = 'Server Ingestion'; }
    value(2; "Outlook Plugin") { Caption = 'Outlook Plugin'; }
    value(3; "Teams Plugin") { Caption = 'Teams Plugin'; }
    value(4; Manual) { Caption = 'Manual'; }
    value(5; "BC API") { Caption = 'BC API'; }
}

enum 50003 "Communication Processing Status"
{
    Extensible = true;
    value(0; Pending) { Caption = 'Pending'; }
    value(1; Processing) { Caption = 'Processing'; }
    value(2; Completed) { Caption = 'Completed'; }
    value(3; Failed) { Caption = 'Failed'; }
    value(4; Excluded) { Caption = 'Excluded'; }
}

enum 50004 "Communication Visibility Scope"
{
    Extensible = true;
    value(0; Owner) { Caption = 'Owner'; }
    value(1; "Owner Team") { Caption = 'Owner Team'; }
    value(2; Company) { Caption = 'Company'; }
}

enum 50005 "Communication Sensitivity Level"
{
    Extensible = true;
    value(0; Public) { Caption = 'Public'; }
    value(1; Internal) { Caption = 'Internal'; }
    value(2; Confidential) { Caption = 'Confidential'; }
    value(3; "Strictly Confidential") { Caption = 'Strictly Confidential'; }
    value(4; Restricted) { Caption = 'Restricted'; }
}

enum 50006 "Communication Participant Role"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; From) { Caption = 'From'; }
    value(2; "To") { Caption = 'To'; }
    value(3; CC) { Caption = 'CC'; }
    value(4; BCC) { Caption = 'BCC'; }
    value(5; Member) { Caption = 'Member'; }
    value(6; Organizer) { Caption = 'Organizer'; }
    value(7; Presenter) { Caption = 'Presenter'; }
    value(8; Attendee) { Caption = 'Attendee'; }
}

enum 50007 "Communication Storage Location"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; SharePoint) { Caption = 'SharePoint'; }
    value(2; OneDrive) { Caption = 'OneDrive'; }
    value(3; "Azure Blob") { Caption = 'Azure Blob'; }
    value(4; "External URL") { Caption = 'External URL'; }
}

enum 50008 "Communication Entity Link Source"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; "AI Suggested") { Caption = 'AI Suggested'; }
    value(2; Rule) { Caption = 'Rule'; }
    value(3; Manual) { Caption = 'Manual'; }
    value(4; "External API") { Caption = 'External API'; }
}

enum 50009 "Communication Summary Type"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; "Single Message") { Caption = 'Single Message'; }
    value(2; Thread) { Caption = 'Thread'; }
    value(3; "Customer Briefing") { Caption = 'Customer Briefing'; }
    value(4; "Project Briefing") { Caption = 'Project Briefing'; }
    value(5; "Meeting Briefing") { Caption = 'Meeting Briefing'; }
    value(6; Topic) { Caption = 'Topic'; }
    value(7; Chronological) { Caption = 'Chronological'; }
}

enum 50010 "Communication Action Item Source"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; "AI Suggested") { Caption = 'AI Suggested'; }
    value(2; "User Created") { Caption = 'User Created'; }
    value(3; "External Imported") { Caption = 'External Imported'; }
}

enum 50011 "Communication Action Item Status"
{
    Extensible = true;
    value(0; Open) { Caption = 'Open'; }
    value(1; "In Progress") { Caption = 'In Progress'; }
    value(2; Done) { Caption = 'Done'; }
    value(3; Cancelled) { Caption = 'Cancelled'; }
    value(4; Rejected) { Caption = 'Rejected'; }
}

enum 50012 "Communication Source System"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; "M365 Exchange") { Caption = 'M365 Exchange'; }
    value(2; "M365 Teams") { Caption = 'M365 Teams'; }
    value(3; "M365 SharePoint") { Caption = 'M365 SharePoint'; }
    value(4; "Business Central") { Caption = 'Business Central'; }
    value(5; External) { Caption = 'External'; }
}

enum 50013 "Communication Source Reference Type"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; "Search Doc") { Caption = 'Search Doc'; }
    value(2; "SharePoint Item") { Caption = 'SharePoint Item'; }
    value(3; "BC Record") { Caption = 'BC Record'; }
    value(4; "Interaction Message") { Caption = 'Interaction Message'; }
    value(5; "Web URL") { Caption = 'Web URL'; }
    value(6; "Blob Object") { Caption = 'Blob Object'; }
}

enum 50014 "Communication Source Used For"
{
    Extensible = true;
    value(0; " ") { Caption = ' '; }
    value(1; Summary) { Caption = 'Summary'; }
    value(2; Reply) { Caption = 'Reply'; }
    value(3; Match) { Caption = 'Match'; }
    value(4; Classification) { Caption = 'Classification'; }
}

enum 50015 "Communication AI Summary Status"
{
    Extensible = true;
    value(0; Draft) { Caption = 'Draft'; }
    value(1; Generated) { Caption = 'Generated'; }
    value(2; Edited) { Caption = 'Edited'; }
    value(3; Approved) { Caption = 'Approved'; }
    value(4; Stale) { Caption = 'Stale'; }
}

enum 50016 "Communication Consent Status"
{
    Extensible = true;
    value(0; Pending) { Caption = 'Pending'; }
    value(1; Granted) { Caption = 'Granted'; }
    value(2; Withdrawn) { Caption = 'Withdrawn'; }
    value(3; Expired) { Caption = 'Expired'; }
}
