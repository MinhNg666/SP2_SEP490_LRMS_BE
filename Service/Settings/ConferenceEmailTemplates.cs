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
<strong>Chi phí dự kiến:</strong><br/>
- Chi phí đi lại: {expense.TravelExpense:N0} VND ({expense.Travel})<br/>
- Chi phí lưu trú: {expense.AccomodationExpense:N0} VND ({expense.Accomodation})
</p>";
    }

    public static string GetStakeholderConferenceCreationEmail(User stakeholder, Project project, Conference conference, Group group, User creator)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tạo Conference</title>
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
            <p>Dự án nghiên cứu của nhóm đã được chuyển đổi thành Conference. Chi tiết như sau:</p>
            
            <p><strong>Thông tin dự án gốc:</strong><br/>
            - Tên dự án: {project.ProjectName}<br/>
            - Nhóm nghiên cứu: {group.GroupName}<br/>
            - Trạng thái: Đang chờ phê duyệt</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Hình thức: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
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

    public static string GetMemberConferenceCreationEmail(User member, Project project, Conference conference, Group group, User creator)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tạo Conference</title>
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
            <p>Dự án nghiên cứu của nhóm đã được chuyển đổi thành Conference. Chi tiết như sau:</p>
            
            <p><strong>Thông tin dự án gốc:</strong><br/>
            - Tên dự án: {project.ProjectName}<br/>
            - Nhóm nghiên cứu: {group.GroupName}<br/>
            - Trạng thái: Đang chờ phê duyệt</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Hình thức: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
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

    public static string GetStakeholderConferenceApprovalEmail(User stakeholder, Project project, Conference conference, Group group, User approver, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo phê duyệt Conference</title>
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
            <p>Conference của nhóm đã được hội đồng phê duyệt. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Hình thức: {conference.PresentationType}<br/>
            - Người phê duyệt: {approver.FullName}</p>
            
            {GetExpenseInfo(conference)}
            
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

    public static string GetMemberConferenceApprovalEmail(User member, Project project, Conference conference, Group group, User approver, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo phê duyệt Conference</title>
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
            <p>Conference của nhóm đã được hội đồng phê duyệt. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Hình thức: {conference.PresentationType}<br/>
            - Người phê duyệt: {approver.FullName}</p>
            
            {GetExpenseInfo(conference)}
            
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

    public static string GetStakeholderConferenceRejectionEmail(User stakeholder, Project project, Conference conference, Group group, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo từ chối Conference</title>
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
            <p class=""rejection""><strong>Conference của nhóm đã bị hội đồng từ chối.</strong> Chi tiết như sau:</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Hình thức: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
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

    public static string GetMemberConferenceRejectionEmail(User member, Project project, Conference conference, Group group, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo từ chối Conference</title>
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
            <p class=""rejection""><strong>Conference của nhóm đã bị hội đồng từ chối.</strong> Chi tiết như sau:</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}<br/>
            - Hình thức: {conference.PresentationType}</p>
            
            {GetExpenseInfo(conference)}
            
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

    public static string GetStakeholderConferenceDocumentEmail(User stakeholder, Project project, Conference conference, User uploader, string fileName, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tài liệu Conference mới</title>
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
            <p>Một tài liệu mới đã được tải lên cho Conference của nhóm. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}</p>
            
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

    public static string GetMemberConferenceDocumentEmail(User member, Project project, Conference conference, User uploader, string fileName, string documentUrl)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo tài liệu Conference mới</title>
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
            <p>Một tài liệu mới đã được tải lên cho Conference của nhóm. Chi tiết như sau:</p>
            
            <p><strong>Thông tin Conference:</strong><br/>
            - Tên Conference: {conference.ConferenceName}<br/>
            - Địa điểm: {conference.Location}<br/>
            - Xếp hạng: {conference.ConferenceRanking}<br/>
            - Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}</p>
            
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