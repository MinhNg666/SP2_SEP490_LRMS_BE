using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Service.Implementations;

namespace Service.Settings;

public static class InvitationEmailTemplates
{
    public static string GetInvitationEmail(User invitedUser, Group group, User sender)
    {
        if (invitedUser == null || group == null || sender == null)
        {
            return "Unable to create email content due to missing information.";
        }

        string departmentName = group.GroupDepartmentNavigation?.DepartmentName ?? "Information Not Available";

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Research Group Invitation</h2>
        <p>Dear <strong>{invitedUser.FullName}</strong>,</p>
        <p>You have been invited to join a research group. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Group Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Group Name:</strong> {group.GroupName}</li>
                <li><strong>Group Type:</strong> {(group.GroupType == 1 ? "Student Research Group" : "Review_Council")}</li>
                <li><strong>Group Leader:</strong> {sender.FullName}</li>
                <li><strong>Department/Unit:</strong> {departmentName}</li>
            </ul>
        </div>
        <p>Please log in to the LRMS system to view details and respond to this invitation.</p>
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

    public static string GetAcceptInvitationEmail(User member, Group group, User sender)
    {
        if (member == null || group == null || sender == null)
        {
            return "Unable to create email content due to missing information.";
        }

        string departmentName = group.GroupDepartmentNavigation?.DepartmentName ?? "Information Not Available";

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Invitation Acceptance Notification</h2>
        <p>Dear <strong>{sender.FullName}</strong>,</p>
        <p><strong>{member.FullName}</strong> has accepted the invitation to join your research group.</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Group Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Group Name:</strong> {group.GroupName}</li>
                <li><strong>Department/Unit:</strong> {departmentName}</li>
            </ul>
        </div>
        <p>Please access the LRMS system to view details and update group information.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetRejectInvitationEmail(User member, Group group, User sender)
    {
        if (member == null || group == null || sender == null)
        {
            return "Unable to create email content due to missing information.";
        }

        string departmentName = group.GroupDepartmentNavigation?.DepartmentName ?? "Information Not Available";

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Invitation Rejection Notification</h2>
        <p>Dear <strong>{sender.FullName}</strong>,</p>
        <p><strong>{member.FullName}</strong> has declined the invitation to join your research group.</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Group Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Group Name:</strong> {group.GroupName}</li>
                <li><strong>Department/Unit:</strong> {departmentName}</li>
            </ul>
        </div>
        <p>Please access the LRMS system to view details and update group information.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.</em>
        </p>
    </div>
</body>
</html>";
    }
}