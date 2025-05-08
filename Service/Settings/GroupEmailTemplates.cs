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
    // Template email cho người tạo nhóm
    public static string GetCreatorGroupCreationEmail(User creator, Group group)
    {
        if (creator == null || group == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo tạo nhóm thành công</h2>
        <p>Kính gửi <strong>{creator.FullName}</strong>,</p>
        <p>Bạn đã tạo thành công nhóm nghiên cứu mới. Chi tiết như sau:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin nhóm:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên nhóm:</strong> {group.GroupName}</li>
                <li><strong>Loại nhóm:</strong> {(group.GroupType == (int)GroupTypeEnum.Student ? "Nhóm nghiên cứu sinh viên" : "Hội đồng")}</li>
                <li><strong>Số thành viên tối đa:</strong> {group.MaxMember}</li>
                <li><strong>Ngày tạo:</strong> {group.CreatedAt:dd/MM/yyyy HH:mm}</li>
            </ul>
        </div>
        <p>Lời mời đã được gửi đến các thành viên bạn đã thêm vào nhóm. Bạn có thể theo dõi trạng thái phản hồi của họ trong hệ thống.</p>
        <p>Trân trọng,<br>Hệ thống LRMS</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với quản trị viên để được hỗ trợ.</em>
        </p>
    </div>
</body>
</html>";
    }

    // Template email mời thành viên tham gia nhóm
    public static string GetMemberInvitationEmail(User member, Group group, User inviter, int role)
    {
        if (member == null || group == null || inviter == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        string roleName = GetRoleName(role);

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Lời mời tham gia nhóm nghiên cứu</h2>
        <p>Kính gửi <strong>{member.FullName}</strong>,</p>
        <p>Bạn đã được mời tham gia nhóm nghiên cứu. Chi tiết như sau:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin nhóm:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên nhóm:</strong> {group.GroupName}</li>
                <li><strong>Loại nhóm:</strong> {(group.GroupType == (int)GroupTypeEnum.Student ? "Nhóm nghiên cứu sinh viên" : "Hội đồng")}</li>
                <li><strong>Vai trò được mời:</strong> {roleName}</li>
                <li><strong>Người mời:</strong> {inviter.FullName}</li>
            </ul>
        </div>
        <p>Vui lòng đăng nhập vào hệ thống LRMS để xem và phản hồi lời mời này.</p>
        <p>Trân trọng,<br>Hệ thống LRMS</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với người mời hoặc quản trị viên để được hỗ trợ.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderAddedEmail(User stakeholder, Group group, User creator)
    {
        if (stakeholder == null || group == null || creator == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo vai trò Stakeholder</h2>
        <p>Kính gửi <strong>{stakeholder.FullName}</strong>,</p>
        <p>Bạn đã được thêm làm Stakeholder của nhóm nghiên cứu. Chi tiết như sau:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin nhóm:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên nhóm:</strong> {group.GroupName}</li>
                <li><strong>Người tạo:</strong> {creator.FullName}</li>
            </ul>
        </div>
        <p>Với vai trò Stakeholder, bạn sẽ nhận được các thông báo về tiến độ và cập nhật quan trọng của các dự án thuộc nhóm này qua email.</p>
        <p>Trân trọng,<br>Hệ thống LRMS</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với người tạo nhóm hoặc quản trị viên để được hỗ trợ.</em>
        </p>
    </div>
</body>
</html>";
    }

    // Phương thức trợ giúp để lấy tên vai trò
    public static string GetRoleName(int role)
    {
        return role switch
        {
            (int)GroupMemberRoleEnum.Leader => "Trưởng nhóm",
            (int)GroupMemberRoleEnum.Member => "Thành viên",
            (int)GroupMemberRoleEnum.Supervisor => "Giáo viên hướng dẫn",
            (int)GroupMemberRoleEnum.Stakeholder => "Stakeholder",
            (int)GroupMemberRoleEnum.Council_Chairman => "Chủ tịch hội đồng",
            (int)GroupMemberRoleEnum.Secretary => "Thư ký hội đồng",
            (int)GroupMemberRoleEnum.Council_Member => "Thành viên hội đồng",
            _ => "Thành viên"
        };
    }
}
