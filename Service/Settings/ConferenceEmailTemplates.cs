using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;

namespace Service.Settings;
public static class ConferenceEmailTemplates
{
    private static string GetExpenseInfo(Conference conference)
    {
        var expense = conference.ConferenceExpenses.FirstOrDefault();
        if (expense == null) return "";

        return $@"<p>
<strong>Estimated Expenses:</strong><br/>
- Travel Expenses: {expense.TravelExpense:N0} USD ({expense.Travel})<br/>
- Accommodation Expenses: {expense.AccomodationExpense:N0} USD ({expense.Accomodation})
</p>";
    }

    public static string GetStakeholderConferenceCreationEmail(User stakeholder, Project project, Conference conference, Group group, User creator)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Conference Creation Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p>The research project of the group has been converted to a Conference. Details are as follows:</p>
            
            <p><strong>Original Project Information:</strong><br/>
            - Project Name: {project.ProjectName}<br/>
            - Research Group: {group.GroupName}<br/>
            - Status: Pending Approval</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Presentation Type: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
            <p>Please access the system to view more details and track the approval process.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberConferenceCreationEmail(User member, Project project, Conference conference, Group group, User creator)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Conference Creation Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {member.FullName},</p>
        </div>
        <div class=""content"">
            <p>The research project of the group has been converted to a Conference. Details are as follows:</p>
            
            <p><strong>Original Project Information:</strong><br/>
            - Project Name: {project.ProjectName}<br/>
            - Research Group: {group.GroupName}<br/>
            - Status: Pending Approval</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Presentation Type: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
            <p>Please access the system to view more details and track the approval process.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderConferenceApprovalEmail(User stakeholder, Project project, Conference conference, Group group, User approver, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Conference Approval Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
        .document-link {{ padding: 10px; background-color: #f5f5f5; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p>The group's Conference has been approved by the council. Details are as follows:</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Presentation Type: {conference.PresentationType}<br/>
            - Approver: {approver.FullName}</p>
            
            {GetExpenseInfo(conference)}
            
            <div class=""document-link"">
                <p><strong>Review_Council Meeting Minutes:</strong><br/>
                <a href=""{documentUrl}"">View Minutes</a></p>
            </div>
            
            <p>Please access the system to view more details.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberConferenceApprovalEmail(User member, Project project, Conference conference, Group group, User approver, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Conference Approval Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
        .document-link {{ padding: 10px; background-color: #f5f5f5; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {member.FullName},</p>
        </div>
        <div class=""content"">
            <p>The group's Conference has been approved by the council. Details are as follows:</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Presentation Type: {conference.PresentationType}<br/>
            - Approver: {approver.FullName}</p>
            
            {GetExpenseInfo(conference)}
            
            <div class=""document-link"">
                <p><strong>Review_Council Meeting Minutes:</strong><br/>
                <a href=""{documentUrl}"">View Minutes</a></p>
            </div>
            
            <p>Please access the system to view more details.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderConferenceRejectionEmail(User stakeholder, Project project, Conference conference, Group group, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Conference Rejection Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
        .document-link {{ padding: 10px; background-color: #f5f5f5; margin: 15px 0; }}
        .rejection {{ color: #d9534f; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p class=""rejection""><strong>The group's Conference has been rejected by the council.</strong> Details are as follows:</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Presentation Type: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
            <div class=""document-link"">
                <p><strong>Review_Council Meeting Minutes (including rejection reasons):</strong><br/>
                <a href=""{documentUrl}"">View Minutes</a></p>
            </div>
            
            <p>Please review the council's comments and make necessary adjustments before resubmitting.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberConferenceRejectionEmail(User member, Project project, Conference conference, Group group, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Conference Rejection Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
        .document-link {{ padding: 10px; background-color: #f5f5f5; margin: 15px 0; }}
        .rejection {{ color: #d9534f; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {member.FullName},</p>
        </div>
        <div class=""content"">
            <p class=""rejection""><strong>The group's Conference has been rejected by the council.</strong> Details are as follows:</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Presentation Type: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
            <div class=""document-link"">
                <p><strong>Review_Council Meeting Minutes (including rejection reasons):</strong><br/>
                <a href=""{documentUrl}"">View Minutes</a></p>
            </div>
            
            <p>Please collaborate with group members to review the council's comments and make necessary adjustments before resubmitting.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderConferenceDocumentEmail(User stakeholder, Project project, Conference conference, User uploader, string fileName, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New Conference Document Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
        .document-info {{ padding: 10px; background-color: #f5f5f5; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p>A new document has been uploaded for the group's Conference. Details are as follows:</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}</p>
            
            <div class=""document-info"">
                <p><strong>Document Information:</strong><br/>
                - Document Name: {fileName}<br/>
                - Uploaded By: {uploader.FullName}<br/>
                - Time: {DateTime.Now:dd/MM/yyyy HH:mm}<br/>
                - <a href=""{documentUrl}"">View Document</a></p>
            </div>
            
            <p>Please access the system to view document details.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberConferenceDocumentEmail(User member, Project project, Conference conference, User uploader, string fileName, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New Conference Document Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ margin-bottom: 20px; }}
        .content {{ margin-bottom: 30px; }}
        .footer {{ font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }}
        .document-info {{ padding: 10px; background-color: #f5f5f5; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <p>Dear {member.FullName},</p>
        </div>
        <div class=""content"">
            <p>A new document has been uploaded for the group's Conference. Details are as follows:</p>
            
            <p><strong>Conference Information:</strong><br/>
            - Conference Name: {conference.ConferenceName}<br/>
            - Location: {conference.Location}<br/>
            - Ranking: {conference.ConferenceRanking}<br/>
            - Presentation Date: {conference.PresentationDate:dd/MM/yyyy}</p>
            
            <div class=""document-info"">
                <p><strong>Document Information:</strong><br/>
                - Document Name: {fileName}<br/>
                - Uploaded By: {uploader.FullName}<br/>
                - Time: {DateTime.Now:dd/MM/yyyy HH:mm}<br/>
                - <a href=""{documentUrl}"">View Document</a></p>
            </div>
            
            <p>Please access the system to view document details.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br/>
            LRMS System</p>
            
            <p><em>Note: This is an automated email, please do not reply.<br/>
            If you have any questions, please contact the group leader or administrator for assistance.</em></p>
        </div>
    </div>
</body>
</html>";
    }
}