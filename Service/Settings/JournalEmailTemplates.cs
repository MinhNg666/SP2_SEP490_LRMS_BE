using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;

namespace Service.Settings;
public static class JournalEmailTemplates
{
    public static string GetStakeholderJournalCreationEmail(User stakeholder, Project project, Journal journal, Group group, User creator)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Journal Creation Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p>The research project of the group has been converted to a Journal. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Original Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
                <li><strong>Status:</strong> Pending Approval</li>
            </ul>
            <h3 style='color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
                <li><strong>Submission Date:</strong> {journal.SubmissionDate:dd/MM/yyyy}</li>
            </ul>
        </div>
            <p>Please access the system to view more details and track the approval process.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalCreationEmail(User member, Project project, Journal journal, Group group, User creator)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Journal Creation Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>The research project of the group has been converted to a Journal. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Original Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
                <li><strong>Status:</strong> Pending Approval</li>
            </ul>
            <h3 style='color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
                <li><strong>Submission Date:</strong> {journal.SubmissionDate:dd/MM/yyyy}</li>
            </ul>
        </div>
            <p>Please access the system to view more details and track the approval process.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderJournalApprovalEmail(User stakeholder, Project project, Journal journal, Group group, User approver, string documentUrl)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Journal Approval Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
            <p>The group's Journal has been approved by the council. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
                <li><strong>Submission Date:</strong> {journal.SubmissionDate:dd/MM/yyyy}</li>
                <li><strong>Approver:</strong> {approver.FullName}</li>
            </ul>
            <div style='padding: 10px; background-color: #f5f5f5; margin: 15px 0;'>
                <p><strong>Review_Council Meeting Minutes:</strong> <a href='{documentUrl}' style='color: #00477e;'>View Minutes</a></p>
            </div>
        </div>
        <p>Please access the system to view more details.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalApprovalEmail(User member, Project project, Journal journal, Group group, User approver, string documentUrl)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Journal Approval Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
            <p>The group's Journal has been approved by the council. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
                <li><strong>Submission Date:</strong> {journal.SubmissionDate:dd/MM/yyyy}</li>
                <li><strong>Approver:</strong> {approver.FullName}</li>
            </ul>
            <div style='padding: 10px; background-color: #f5f5f5; margin: 15px 0;'>
                <p><strong>Review_Council Meeting Minutes:</strong> <a href='{documentUrl}' style='color: #00477e;'>View Minutes</a></p>
            </div>
        </div>
        <p>Please access the system to view more details.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderJournalRejectionEmail(User stakeholder, Project project, Journal journal, Group group, string documentUrl)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Journal Rejection Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p style='color: #d9534f;'><strong>The group's Journal has been rejected by the council.</strong> Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
                <li><strong>Submission Date:</strong> {journal.SubmissionDate:dd/MM/yyyy}</li>
            </ul>
            <div style='padding: 10px; background-color: #f5f5f5; margin: 15px 0;'>
                <p><strong>Review_Council Meeting Minutes (including rejection reasons):</strong> <a href='{documentUrl}' style='color: #00477e;'>View Minutes</a></p>
            </div>
        </div>
        <p>Please review the council's comments and make necessary adjustments before resubmitting.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalRejectionEmail(User member, Project project, Journal journal, Group group, string documentUrl)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Journal Rejection Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p style='color: #d9534f;'><strong>The group's Journal has been rejected by the council.</strong> Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
                <li><strong>Submission Date:</strong> {journal.SubmissionDate:dd/MM/yyyy}</li>
            </ul>
            <div style='padding: 10px; background-color: #f5f5f5; margin: 15px 0;'>
                <p><strong>Review_Council Meeting Minutes (including rejection reasons):</strong> <a href='{documentUrl}' style='color: #00477e;'>View Minutes</a></p>
            </div>
        </div>
        <p>Please collaborate with group members to review the council's comments and make necessary adjustments before resubmitting.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderJournalDocumentEmail(User stakeholder, Project project, Journal journal, User uploader, string fileName, string documentUrl)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>New Journal Document Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
            <p>A new document has been uploaded for the group's Journal. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
            </ul>
            <div style='padding: 10px; background-color: #f5f5f5; margin: 15px 0;'>
                <p><strong>Document Information:</strong></p>
                <ul style='padding-left: 20px;'>
                    <li><strong>Document Name:</strong> {fileName}</li>
                    <li><strong>Uploaded By:</strong> {uploader.FullName}</li>
                    <li><strong>Time:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                    <li><a href='{documentUrl}' style='color: #00477e;'>View Document</a></li>
                </ul>
            </div>
        </div>
        <p>Please access the system to view document details.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalDocumentEmail(User member, Project project, Journal journal, User uploader, string fileName, string documentUrl)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>New Journal Document Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
            <p>A new document has been uploaded for the group's Journal. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Journal Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Journal Name:</strong> {journal.JournalName}</li>
                <li><strong>Publisher:</strong> {journal.PublisherName}</li>
                <li><strong>DOI Number:</strong> {journal.DoiNumber}</li>
            </ul>
            <div style='padding: 10px; background-color: #f5f5f5; margin: 15px 0;'>
                <p><strong>Document Information:</strong></p>
                <ul style='padding-left: 20px;'>
                    <li><strong>Document Name:</strong> {fileName}</li>
                    <li><strong>Uploaded By:</strong> {uploader.FullName}</li>
                    <li><strong>Time:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                    <li><a href='{documentUrl}' style='color: #00477e;'>View Document</a></li>
                </ul>
            </div>
        </div>
        <p>Please access the system to view document details.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }
}