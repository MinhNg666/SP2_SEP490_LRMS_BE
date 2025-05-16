using LRMS_API;

namespace Service.Settings;

public static class DepartmentQuotaAllocationEmailTemplates
{
    public static string GetCouncilMemberQuotaAllocationEmail(User councilMember, Department department, User allocator, Quota quota)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Department Quota Allocation Notification</title>
</head>
<body>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Department Quota Allocation Notification</h2>
        <p>Dear <strong>{councilMember.FullName}</strong>,</p>
        <p>A new budget quota has been allocated to your department. Details are as follows:</p>
        
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Quota Information:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Department:</strong> {department.DepartmentName}</li>
                <li><strong>Allocated Budget:</strong> {quota.AllocatedBudget:N0} VND</li>
                <li><strong>Quota Year:</strong> {quota.QuotaYear}</li>
                <li><strong>Allocated By:</strong> {allocator.FullName}</li>
                <li><strong>Allocation Date:</strong> {quota.CreatedAt:dd/MM/yyyy}</li>
            </ul>
        </div>

        <p>This quota will be available for research projects within your department for the specified year.</p>
        <p>Best regards,<br>LRMS System</p>
        
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the system administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }
}
