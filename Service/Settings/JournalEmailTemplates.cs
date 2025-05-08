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
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tạo Journal</title>
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
            <p>Kính gửi {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p>Dự án nghiên cứu của nhóm đã được chuyển đổi thành Journal. Chi tiết như sau:</p>
            
            <p><strong>Thông tin dự án gốc:</strong><br/>
            - Tên dự án: {project.ProjectName}<br/>
            - Nhóm nghiên cứu: {group.GroupName}<br/>
            - Trạng thái: Đang chờ phê duyệt</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}<br/>
            - Ngày nộp: {journal.SubmissionDate:dd/MM/yyyy}</p>
            
            <p>Vui lòng truy cập hệ thống để xem thêm chi tiết và theo dõi quá trình phê duyệt.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalCreationEmail(User member, Project project, Journal journal, Group group, User creator)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tạo Journal</title>
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
            <p>Kính gửi {member.FullName},</p>
        </div>
        <div class=""content"">
            <p>Dự án nghiên cứu của nhóm đã được chuyển đổi thành Journal. Chi tiết như sau:</p>
            
            <p><strong>Thông tin dự án gốc:</strong><br/>
            - Tên dự án: {project.ProjectName}<br/>
            - Nhóm nghiên cứu: {group.GroupName}<br/>
            - Trạng thái: Đang chờ phê duyệt</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}<br/>
            - Ngày nộp: {journal.SubmissionDate:dd/MM/yyyy}</p>
            
            <p>Vui lòng truy cập hệ thống để xem thêm chi tiết và theo dõi quá trình phê duyệt.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderJournalApprovalEmail(User stakeholder, Project project, Journal journal, Group group, User approver, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo phê duyệt Journal</title>
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
            <p>Kính gửi {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p>Journal của nhóm đã được hội đồng phê duyệt. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}<br/>
            - Ngày nộp: {journal.SubmissionDate:dd/MM/yyyy}<br/>
            - Người phê duyệt: {approver.FullName}</p>
            
            <div class=""document-link"">
                <p><strong>Biên bản họp hội đồng xét duyệt:</strong><br/>
                <a href=""{documentUrl}"">Xem biên bản</a></p>
            </div>
            
            <p>Vui lòng truy cập hệ thống để xem thêm chi tiết.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalApprovalEmail(User member, Project project, Journal journal, Group group, User approver, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo phê duyệt Journal</title>
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
            <p>Kính gửi {member.FullName},</p>
        </div>
        <div class=""content"">
            <p>Journal của nhóm đã được hội đồng phê duyệt. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}<br/>
            - Ngày nộp: {journal.SubmissionDate:dd/MM/yyyy}<br/>
            - Người phê duyệt: {approver.FullName}</p>
            
            <div class=""document-link"">
                <p><strong>Biên bản họp hội đồng xét duyệt:</strong><br/>
                <a href=""{documentUrl}"">Xem biên bản</a></p>
            </div>
            
            <p>Vui lòng truy cập hệ thống để xem thêm chi tiết.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderJournalRejectionEmail(User stakeholder, Project project, Journal journal, Group group, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo từ chối Journal</title>
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
            <p>Kính gửi {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p class=""rejection""><strong>Journal của nhóm đã bị hội đồng từ chối.</strong> Chi tiết như sau:</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}<br/>
            - Ngày nộp: {journal.SubmissionDate:dd/MM/yyyy}</p>
            
            <div class=""document-link"">
                <p><strong>Biên bản họp hội đồng xét duyệt (bao gồm lý do từ chối):</strong><br/>
                <a href=""{documentUrl}"">Xem biên bản</a></p>
            </div>
            
            <p>Vui lòng xem xét các ý kiến từ hội đồng và thực hiện các điều chỉnh cần thiết trước khi nộp lại.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalRejectionEmail(User member, Project project, Journal journal, Group group, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo từ chối Journal</title>
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
            <p>Kính gửi {member.FullName},</p>
        </div>
        <div class=""content"">
            <p class=""rejection""><strong>Journal của nhóm đã bị hội đồng từ chối.</strong> Chi tiết như sau:</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}<br/>
            - Ngày nộp: {journal.SubmissionDate:dd/MM/yyyy}</p>
            
            <div class=""document-link"">
                <p><strong>Biên bản họp hội đồng xét duyệt (bao gồm lý do từ chối):</strong><br/>
                <a href=""{documentUrl}"">Xem biên bản</a></p>
            </div>
            
            <p>Vui lòng phối hợp với các thành viên trong nhóm để xem xét các ý kiến từ hội đồng và thực hiện các điều chỉnh cần thiết trước khi nộp lại.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetStakeholderJournalDocumentEmail(User stakeholder, Project project, Journal journal, User uploader, string fileName, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tài liệu Journal mới</title>
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
            <p>Kính gửi {stakeholder.FullName},</p>
        </div>
        <div class=""content"">
            <p>Một tài liệu mới đã được tải lên cho Journal của nhóm. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}</p>
            
            <div class=""document-info"">
                <p><strong>Thông tin tài liệu:</strong><br/>
                - Tên tài liệu: {fileName}<br/>
                - Người tải lên: {uploader.FullName}<br/>
                - Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm}<br/>
                - <a href=""{documentUrl}"">Xem tài liệu</a></p>
            </div>
            
            <p>Vui lòng truy cập hệ thống để xem chi tiết tài liệu.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetMemberJournalDocumentEmail(User member, Project project, Journal journal, User uploader, string fileName, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tài liệu Journal mới</title>
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
            <p>Kính gửi {member.FullName},</p>
        </div>
        <div class=""content"">
            <p>Một tài liệu mới đã được tải lên cho Journal của nhóm. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Journal:</strong><br/>
            - Tên Journal: {journal.JournalName}<br/>
            - Nhà xuất bản: {journal.PublisherName}<br/>
            - Mã DOI: {journal.DoiNumber}</p>
            
            <div class=""document-info"">
                <p><strong>Thông tin tài liệu:</strong><br/>
                - Tên tài liệu: {fileName}<br/>
                - Người tải lên: {uploader.FullName}<br/>
                - Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm}<br/>
                - <a href=""{documentUrl}"">Xem tài liệu</a></p>
            </div>
            
            <p>Vui lòng truy cập hệ thống để xem chi tiết tài liệu.</p>
        </div>
        <div class=""footer"">
            <p>Trân trọng,<br/>
            Hệ thống LRMS</p>
            
            <p><em>Lưu ý: Đây là email tự động, vui lòng không phản hồi email này.<br/>
            Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với trưởng nhóm hoặc quản trị viên để được hỗ trợ.</em></p>
        </div>
    </div>
</body>
</html>";
    }
}