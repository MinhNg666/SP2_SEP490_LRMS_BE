using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;
using LRMS_API;

namespace Service.Settings;
public class ProjectEmailTemplates
{
    private static string GetProjectTypeName(int? projectType)
    {
        return projectType switch
        {
            (int)ProjectTypeEnum.Research => "Research",
            (int)ProjectTypeEnum.Conference => "Conference",
            (int)ProjectTypeEnum.Journal => "Journal",
            _ => "Unknown"
        };
    }

    // Helper to render project phases as HTML (for Domain.DTO.Responses.ProjectPhaseResponse)
    private static string RenderProjectPhases(IEnumerable<Domain.DTO.Responses.ProjectPhaseResponse> phases)
    {
        if (phases == null || !phases.Any())
            return "<p>No project phases defined.</p>";
        var html = @"<table style='width:100%; border-collapse:collapse; margin-bottom:15px;'>
            <thead><tr style='background:#f2f2f2;'><th style='border:1px solid #ddd;padding:8px;'>Title</th><th style='border:1px solid #ddd;padding:8px;'>Start Date</th><th style='border:1px solid #ddd;padding:8px;'>End Date</th><th style='border:1px solid #ddd;padding:8px;'>Status</th><th style='border:1px solid #ddd;padding:8px;'>Spent Budget</th></tr></thead><tbody>";
        foreach (var phase in phases)
        {
            html += $"<tr><td style='border:1px solid #ddd;padding:8px;'>{phase.Title}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.StartDate:yyyy-MM-dd}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.EndDate:yyyy-MM-dd}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.Status}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.SpentBudget:N0}</td></tr>";
        }
        html += "</tbody></table>";
        return html;
    }

    // Helper to render project phases as HTML (for LRMS_API.ProjectPhase)
    private static string RenderProjectPhases(IEnumerable<LRMS_API.ProjectPhase> phases)
    {
        if (phases == null || !phases.Any())
            return "<p>No project phases defined.</p>";
        var html = @"<table style='width:100%; border-collapse:collapse; margin-bottom:15px;'>
            <thead><tr style='background:#f2f2f2;'><th style='border:1px solid #ddd;padding:8px;'>Title</th><th style='border:1px solid #ddd;padding:8px;'>Start Date</th><th style='border:1px solid #ddd;padding:8px;'>End Date</th><th style='border:1px solid #ddd;padding:8px;'>Status</th><th style='border:1px solid #ddd;padding:8px;'>Spent Budget</th></tr></thead><tbody>";
        foreach (var phase in phases)
        {
            html += $"<tr><td style='border:1px solid #ddd;padding:8px;'>{phase.Title}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.StartDate:yyyy-MM-dd}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.EndDate:yyyy-MM-dd}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.Status}</td><td style='border:1px solid #ddd;padding:8px;'>{phase.SpentBudget:N0}</td></tr>";
        }
        html += "</tbody></table>";
        return html;
    }

    // Helper to render documents as HTML (for Domain.DTO.Responses.DocumentResponse)
    private static string RenderDocuments(IEnumerable<Domain.DTO.Responses.DocumentResponse> documents)
    {
        if (documents == null || !documents.Any())
            return "<p>No documents attached.</p>";
        var html = @"<ul style='padding-left:20px;'>";
        foreach (var doc in documents)
        {
            html += $"<li><a href='{doc.DocumentUrl}' style='color:#00477e;'>{doc.FileName}</a> (Uploaded: {doc.UploadAt:yyyy-MM-dd})</li>";
        }
        html += "</ul>";
        return html;
    }

    // Helper to render documents as HTML (for LRMS_API.Document)
    private static string RenderDocuments(IEnumerable<LRMS_API.Document> documents)
    {
        if (documents == null || !documents.Any())
            return "<p>No documents attached.</p>";
        var html = @"<ul style='padding-left:20px;'>";
        foreach (var doc in documents)
        {
            html += $"<li><a href='{doc.DocumentUrl}' style='color:#00477e;'>{doc.FileName}</a> (Uploaded: {doc.UploadAt:yyyy-MM-dd})</li>";
        }
        html += "</ul>";
        return html;
    }

    public static string GetMemberProjectCreationEmail(User member, Project project, User creator, Group group, Department department)
    {
        if (member == null || project == null || creator == null || group == null || department == null)
        {
            return "Unable to create email content due to missing information.";
        }

        var phasesHtml = RenderProjectPhases(project.ProjectPhases);
        var documentsHtml = RenderDocuments(project.Documents);

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>New Project Creation Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>Your group has created a new project. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Project Type:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Department:</strong> {department.DepartmentName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
                <li><strong>Creator:</strong> {creator.FullName}</li>
                <li><strong>Created At:</strong> {project.CreatedAt:dd/MM/yyyy}</li>
                <li><strong>Approved Budget:</strong> {project.ApprovedBudget:N0} VND</li>
            </ul>
            <h3 style='color: #00477e;'>Project Phases:</h3>
            {phasesHtml}
            <h3 style='color: #00477e;'>Attached Documents:</h3>
            {documentsHtml}
        </div>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderProjectCreationEmail(User stakeholder, Project project, User creator, Group group, Department department)
    {
        if (stakeholder == null || project == null || creator == null || group == null || department == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>New Project Supervision Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p>A new research project has been created under your supervision. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Project Type:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Creator:</strong> {creator.FullName}</li>
                <li><strong>Department:</strong> {department.DepartmentName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
            </ul>
            <h3 style='color: #00477e;'>Timeline Details:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Start Date:</strong> {project.StartDate:dd/MM/yyyy}</li>
                <li><strong>End Date:</strong> {project.EndDate:dd/MM/yyyy}</li>
            </ul>
        </div>
        <p>As a stakeholder, you will receive periodic updates about the project's progress.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberProjectApprovalEmail(User member, Project project, User approver, Group group, Department department, string documentUrl)
    {
        if (member == null || project == null || approver == null || group == null || department == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Project Approval Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>Your group's project has been approved by the council. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Project Type:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Department:</strong> {department.DepartmentName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
                <li><strong>Approver:</strong> {approver.FullName}</li>
                <li><strong>Approval Date:</strong> {DateTime.Now:dd/MM/yyyy}</li>
                <li><strong>Approved Budget:</strong> {project.ApprovedBudget:N0} VND</li>
            </ul>
        </div>
        <p><strong>Review_Council Meeting Minutes:</strong> <a href='{documentUrl}' style='color: #00477e;'>View here</a></p>
        <p>Your group can now begin implementing the project as proposed.</p>
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

    public static string GetStakeholderProjectApprovalEmail(User stakeholder, Project project, User approver, Group group, Department department, string documentUrl)
    {
        if (stakeholder == null || project == null || approver == null || group == null || department == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Project Approval Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p>Your group's project has been approved by the council. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Project Type:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Department:</strong> {department.DepartmentName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
                <li><strong>Approver:</strong> {approver.FullName}</li>
                <li><strong>Approval Date:</strong> {DateTime.Now:dd/MM/yyyy}</li>
                <li><strong>Approved Budget:</strong> {project.ApprovedBudget:N0} VND</li>
            </ul>
        </div>
        <p><strong>Review_Council Meeting Minutes:</strong> <a href='{documentUrl}' style='color: #00477e;'>View here</a></p>
        <p>As a stakeholder, you will continue to receive updates about the project's progress.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderProjectRejectionEmail(User stakeholder, Project project, Group group, string documentUrl)
    {
        if (stakeholder == null || project == null || group == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Project Rejection Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p>Your group's project has been rejected. Details are as follows:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
            </ul>
        </div>
        
        <p><strong>Review_Council Meeting Minutes/Rejection Reason:</strong> <a href='{documentUrl}' style='color: #00477e;'>View here</a></p>
        <p>Please review the rejection reason and make necessary adjustments before resubmitting.</p>
        <p>Best regards,<br>LRMS System</p>
        
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberProjectRejectionEmail(User member, Project project, Group group, string documentUrl)
    {
        if (member == null || project == null || group == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Project Rejection Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>Your group's project has been rejected. Details are as follows:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
            </ul>
        </div>
        
        <p><strong>Review_Council Meeting Minutes/Rejection Reason:</strong> <a href='{documentUrl}' style='color: #00477e;'>View here</a></p>
        <p>Please coordinate with your group members to make necessary adjustments before resubmitting.</p>
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

    public static string GetStakeholderDocumentUploadEmail(User stakeholder, Project project, User uploader, Group group, string fileName, string documentUrl)
    {
        if (stakeholder == null || project == null || uploader == null || group == null)
        {
            return "Unable to create email content due to missing information.";
        }

        var phasesHtml = RenderProjectPhases(project.ProjectPhases);
        var documentsHtml = RenderDocuments(project.Documents);

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>New Document Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p>A new document has been uploaded to the project you are supervising. Details are as follows:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
            </ul>
            
            <h3 style='color: #00477e;'>Document Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Document Name:</strong> {fileName}</li>
                <li><strong>Uploader:</strong> {uploader.FullName}</li>
                <li><strong>Upload Time:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
            </ul>

            <h3 style='color: #00477e;'>Project Phases:</h3>
            {phasesHtml}
            
            <h3 style='color: #00477e;'>All Project Documents:</h3>
            {documentsHtml}
        </div>
        
        <p><a href='{documentUrl}' style='display: inline-block; background-color: #00477e; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px;'>View New Document</a></p>
        <p>Please access the system to view the document details.</p>
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

    public static string GetMemberDocumentUploadEmail(User member, Project project, User uploader, Group group, string fileName, string documentUrl)
    {
        if (member == null || project == null || uploader == null || group == null)
        {
            return "Unable to create email content due to missing information.";
        }

        var phasesHtml = RenderProjectPhases(project.ProjectPhases);
        var documentsHtml = RenderDocuments(project.Documents);

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>New Document Notification</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>A new document has been uploaded to the project of your group. Details are as follows:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Project Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Research Group:</strong> {group.GroupName}</li>
            </ul>
            
            <h3 style='color: #00477e;'>Document Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Document Name:</strong> {fileName}</li>
                <li><strong>Uploader:</strong> {uploader.FullName}</li>
                <li><strong>Upload Time:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
            </ul>

            <h3 style='color: #00477e;'>Project Phases:</h3>
            {phasesHtml}
            
            <h3 style='color: #00477e;'>All Project Documents:</h3>
            {documentsHtml}
        </div>
        
        <p><a href='{documentUrl}' style='display: inline-block; background-color: #00477e; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px;'>View New Document</a></p>
        <p>Please access the system to view the document details.</p>
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