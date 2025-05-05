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
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        string departmentName = group.GroupDepartmentNavigation?.DepartmentName ?? "Chưa có thông tin";

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Lời mời tham gia nhóm nghiên cứu</h2>
        <p>Kính gửi <strong>{invitedUser.FullName}</strong>,</p>
        <p>Bạn đã được mời tham gia nhóm nghiên cứu. Chi tiết như sau:</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin nhóm:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên nhóm:</strong> {group.GroupName}</li>
                <li><strong>Loại nhóm:</strong> {(group.GroupType == 1 ? "Nhóm nghiên cứu sinh viên" : "Hội đồng")}</li>
                <li><strong>Trưởng nhóm:</strong> {sender.FullName}</li>
                <li><strong>Khoa/Phòng ban:</strong> {departmentName}</li>
            </ul>
        </div>
        <p>Vui lòng đăng nhập vào hệ thống LRMS để xem chi tiết và phản hồi lời mời này.</p>
        <p>Trân trọng,<br>Hệ thống LRMS</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetAcceptInvitationEmail(User member, Group group, User sender)
    {
        if (member == null || group == null || sender == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        string departmentName = group.GroupDepartmentNavigation?.DepartmentName ?? "Chưa có thông tin";

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo chấp nhận lời mời</h2>
        <p>Kính gửi <strong>{sender.FullName}</strong>,</p>
        <p><strong>{member.FullName}</strong> đã chấp nhận lời mời tham gia nhóm nghiên cứu của bạn.</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin nhóm:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên nhóm:</strong> {group.GroupName}</li>
                <li><strong>Khoa/Phòng ban:</strong> {departmentName}</li>
            </ul>
        </div>
        <p>Vui lòng truy cập hệ thống LRMS để xem chi tiết và cập nhật thông tin nhóm.</p>
        <p>Trân trọng,<br>Hệ thống LRMS</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetRejectInvitationEmail(User member, Group group, User sender)
    {
        if (member == null || group == null || sender == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        string departmentName = group.GroupDepartmentNavigation?.DepartmentName ?? "Chưa có thông tin";

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Thông báo từ chối lời mời</h2>
        <p>Kính gửi <strong>{sender.FullName}</strong>,</p>
        <p><strong>{member.FullName}</strong> đã từ chối lời mời tham gia nhóm nghiên cứu của bạn.</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin nhóm:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên nhóm:</strong> {group.GroupName}</li>
                <li><strong>Khoa/Phòng ban:</strong> {departmentName}</li>
            </ul>
        </div>
        <p>Vui lòng truy cập hệ thống LRMS để xem chi tiết và cập nhật thông tin nhóm.</p>
        <p>Trân trọng,<br>Hệ thống LRMS</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.</em>
        </p>
    </div>
</body>
</html>";
    }
}