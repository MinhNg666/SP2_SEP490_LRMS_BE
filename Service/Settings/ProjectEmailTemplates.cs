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
            (int)ProjectTypeEnum.Research => "Nghiên cứu",
            (int)ProjectTypeEnum.Conference => "Hội nghị",
            (int)ProjectTypeEnum.Journal => "Tạp chí",
            _ => "Không xác định"
        };
    }

    public static string GetMemberProjectCreationEmail(User member, Project project, User creator, Group group, Department department)
    {
        if (member == null || project == null || creator == null || group == null || department == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo dự án mới</h2>
        <p>Kính gửi <strong>{member.FullName}</strong>,</p>
        <p>Bạn đã được thêm vào dự án nghiên cứu mới với vai trò thành viên nhóm. Dưới đây là thông tin chi tiết:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Loại dự án:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Người khởi tạo:</strong> {creator.FullName}</li>
                <li><strong>Khoa/Phòng ban:</strong> {department.DepartmentName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
            </ul>
            
            <h3 style='color: #00477e;'>Chi tiết thời gian:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Ngày bắt đầu:</strong> {project.StartDate:dd/MM/yyyy}</li>
                <li><strong>Ngày kết thúc:</strong> {project.EndDate:dd/MM/yyyy}</li>
            </ul>
        </div>
        
        <p>Vui lòng truy cập hệ thống để xem thêm chi tiết và bắt đầu công việc của bạn.</p>
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

    public static string GetStakeholderProjectCreationEmail(User stakeholder, Project project, User creator, Group group, Department department)
    {
        if (stakeholder == null || project == null || creator == null || group == null || department == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo dự án mới cần giám sát</h2>
        <p>Kính gửi <strong>{stakeholder.FullName}</strong>,</p>
        <p>Một dự án nghiên cứu mới đã được khởi tạo dưới sự giám sát của bạn. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Loại dự án:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Người khởi tạo:</strong> {creator.FullName}</li>
                <li><strong>Khoa/Phòng ban:</strong> {department.DepartmentName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
            </ul>
            
            <h3 style='color: #00477e;'>Chi tiết thời gian:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Ngày bắt đầu:</strong> {project.StartDate:dd/MM/yyyy}</li>
                <li><strong>Ngày kết thúc:</strong> {project.EndDate:dd/MM/yyyy}</li>
            </ul>
        </div>
        
        <p>Với vai trò stakeholder, bạn sẽ nhận được các cập nhật định kỳ về tiến độ dự án.</p>
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

    public static string GetMemberProjectApprovalEmail(User member, Project project, User approver, Group group, Department department, string documentUrl)
    {
        if (member == null || project == null || approver == null || group == null || department == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Thông báo phê duyệt dự án</h2>
        <p>Kính gửi <strong>{member.FullName}</strong>,</p>
        <p>Dự án của nhóm bạn đã được hội đồng phê duyệt. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Loại dự án:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Khoa/Phòng ban:</strong> {department.DepartmentName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
                <li><strong>Người phê duyệt:</strong> {approver.FullName}</li>
                <li><strong>Ngày phê duyệt:</strong> {DateTime.Now:dd/MM/yyyy}</li>
                <li><strong>Kinh phí được duyệt:</strong> {project.ApprovedBudget:N0} VNĐ</li>
            </ul>
        </div>
        
        <p><strong>Biên bản họp hội đồng:</strong> <a href='{documentUrl}' style='color: #00477e;'>Xem tại đây</a></p>
        <p>Nhóm có thể bắt đầu triển khai dự án theo kế hoạch đã đề xuất.</p>
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

    public static string GetStakeholderProjectApprovalEmail(User stakeholder, Project project, User approver, Group group, Department department, string documentUrl)
    {
        if (stakeholder == null || project == null || approver == null || group == null || department == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Thông báo phê duyệt dự án</h2>
        <p>Kính gửi <strong>{stakeholder.FullName}</strong>,</p>
        <p>Dự án dưới sự giám sát của bạn đã được hội đồng phê duyệt. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Loại dự án:</strong> {GetProjectTypeName(project.ProjectType)}</li>
                <li><strong>Khoa/Phòng ban:</strong> {department.DepartmentName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
                <li><strong>Người phê duyệt:</strong> {approver.FullName}</li>
                <li><strong>Ngày phê duyệt:</strong> {DateTime.Now:dd/MM/yyyy}</li>
                <li><strong>Kinh phí được duyệt:</strong> {project.ApprovedBudget:N0} VNĐ</li>
            </ul>
        </div>
        
        <p><strong>Biên bản họp hội đồng:</strong> <a href='{documentUrl}' style='color: #00477e;'>Xem tại đây</a></p>
        <p>Với vai trò stakeholder, bạn sẽ tiếp tục nhận được các cập nhật về tiến độ triển khai dự án.</p>
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

    public static string GetStakeholderProjectRejectionEmail(User stakeholder, Project project, Group group, string documentUrl)
    {
        if (stakeholder == null || project == null || group == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Thông báo từ chối dự án</h2>
        <p>Kính gửi <strong>{stakeholder.FullName}</strong>,</p>
        <p>Dự án nghiên cứu của nhóm đã bị từ chối. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
            </ul>
        </div>
        
        <p><strong>Biên bản họp hội đồng/Lý do từ chối:</strong> <a href='{documentUrl}' style='color: #00477e;'>Xem tại đây</a></p>
        <p>Vui lòng xem xét lý do từ chối và thực hiện các điều chỉnh cần thiết trước khi nộp lại.</p>
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

    public static string GetMemberProjectRejectionEmail(User member, Project project, Group group, string documentUrl)
    {
        if (member == null || project == null || group == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Thông báo từ chối dự án</h2>
        <p>Kính gửi <strong>{member.FullName}</strong>,</p>
        <p>Dự án nghiên cứu của nhóm đã bị từ chối. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
            </ul>
        </div>
        
        <p><strong>Biên bản họp hội đồng/Lý do từ chối:</strong> <a href='{documentUrl}' style='color: #00477e;'>Xem tại đây</a></p>
        <p>Vui lòng phối hợp với các thành viên trong nhóm để thực hiện các điều chỉnh cần thiết trước khi nộp lại.</p>
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

    public static string GetStakeholderDocumentUploadEmail(User stakeholder, Project project, User uploader, Group group, string fileName, string documentUrl)
    {
        if (stakeholder == null || project == null || uploader == null || group == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo tài liệu mới</h2>
        <p>Kính gửi <strong>{stakeholder.FullName}</strong>,</p>
        <p>Một tài liệu mới đã được tải lên trong dự án mà bạn đang giám sát. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
            </ul>
            
            <h3 style='color: #00477e;'>Thông tin tài liệu:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên tài liệu:</strong> {fileName}</li>
                <li><strong>Người tải lên:</strong> {uploader.FullName}</li>
                <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
            </ul>
        </div>
        
        <p><a href='{documentUrl}' style='display: inline-block; background-color: #00477e; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px;'>Xem tài liệu</a></p>
        <p>Vui lòng truy cập hệ thống để xem chi tiết tài liệu.</p>
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

    public static string GetMemberDocumentUploadEmail(User member, Project project, User uploader, Group group, string fileName, string documentUrl)
    {
        if (member == null || project == null || uploader == null || group == null)
        {
            return "Không thể tạo nội dung email do thiếu thông tin.";
        }

        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #00477e;'>Thông báo tài liệu mới</h2>
        <p>Kính gửi <strong>{member.FullName}</strong>,</p>
        <p>Một tài liệu mới đã được tải lên trong dự án của nhóm bạn. Chi tiết như sau:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Thông tin dự án:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên dự án:</strong> {project.ProjectName}</li>
                <li><strong>Nhóm nghiên cứu:</strong> {group.GroupName}</li>
            </ul>
            
            <h3 style='color: #00477e;'>Thông tin tài liệu:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Tên tài liệu:</strong> {fileName}</li>
                <li><strong>Người tải lên:</strong> {uploader.FullName}</li>
                <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
            </ul>
        </div>
        
        <p><a href='{documentUrl}' style='display: inline-block; background-color: #00477e; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px;'>Xem tài liệu</a></p>
        <p>Vui lòng truy cập hệ thống để xem chi tiết tài liệu.</p>
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
}