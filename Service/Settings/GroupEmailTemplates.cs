using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;
using LRMS_API;

namespace Service.Settings;
public static class GroupEmailTemplates
{
    // Template email for group creator
    public static string GetCreatorGroupCreationEmail(User creator, Group group)
    {
        if (creator == null || group == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Group Creation Successful</h2>
        <p>Dear <strong>{creator.FullName}</strong>,</p>
        <p>You have successfully created a new research group. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Group Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Group Name:</strong> {group.GroupName}</li>
                <li><strong>Group Type:</strong> {(group.GroupType == (int)GroupTypeEnum.Student ? "Student Research Group" : "Review_Council")}</li>
                <li><strong>Maximum Members:</strong> {group.MaxMember}</li>
                <li><strong>Creation Date:</strong> {group.CreatedAt:dd/MM/yyyy HH:mm}</li>
            </ul>
        </div>
        <p>Invitations have been sent to the members you added to the group. You can track their response status in the system.</p>
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

    // Template email inviting members to join the group
    public static string GetMemberInvitationEmail(User member, Group group, User inviter, int role)
    {
        if (member == null || group == null || inviter == null)
        {
            return "Unable to create email content due to missing information.";
        }

        string roleName = GetRoleName(role);

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Research Group Invitation</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>You have been invited to join a research group. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Group Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Group Name:</strong> {group.GroupName}</li>
                <li><strong>Group Type:</strong> {(group.GroupType == (int)GroupTypeEnum.Student ? "Student Research Group" : "Review_Council")}</li>
                <li><strong>Invited Role:</strong> {roleName}</li>
                <li><strong>Inviter:</strong> {inviter.FullName}</li>
            </ul>
        </div>
        <p>Please log in to the LRMS system to view and respond to this invitation.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the inviter or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderAddedEmail(User stakeholder, Group group, User creator)
    {
        if (stakeholder == null || group == null || creator == null)
        {
            return "Unable to create email content due to missing information.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Stakeholder Notification</h2>
        <p>Dear <strong>{stakeholder.FullName}</strong>,</p>
        <p>You have been added as a Stakeholder to a research group. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Group Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Group Name:</strong> {group.GroupName}</li>
                <li><strong>Creator:</strong> {creator.FullName}</li>
            </ul>
        </div>
        <p>As a Stakeholder, you will receive notifications about progress and important updates of projects in this group via email.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the group creator or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetMemberAssignmentEmail(User member, Group group, User creator, int role)
    {
        if (member == null || group == null || creator == null)
        {
            return "Unable to create email content due to missing information.";
        }

        string roleName = GetRoleName(role);

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Assessment Council Assignment</h2>
        <p>Dear <strong>{member.FullName}</strong>,</p>
        <p>You have been assigned as a member of an assessment council. Details are as follows:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Council Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Assessment Council Name:</strong> {group.GroupName}</li>
                <li><strong>Assigned Role:</strong> {roleName}</li>
                <li><strong>Assigned By:</strong> {creator.FullName}</li>
            </ul>
        </div>
        <p>Please log in to the LRMS system to view your assignment details and responsibilities.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the council chairman or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    // Helper method to get role name
    public static string GetRoleName(int role)
    {
        return role switch
        {
            (int)GroupMemberRoleEnum.Leader => "Group Leader",
            (int)GroupMemberRoleEnum.Member => "Member",
            (int)GroupMemberRoleEnum.Supervisor => "Supervisor",
            (int)GroupMemberRoleEnum.Stakeholder => "Stakeholder",
            (int)GroupMemberRoleEnum.Council_Chairman => "Review_Council Chairman",
            (int)GroupMemberRoleEnum.Secretary => "Review_Council Secretary",
            (int)GroupMemberRoleEnum.Council_Member => "Review_Council Member",
            _ => "Member"
        };
    }
}
