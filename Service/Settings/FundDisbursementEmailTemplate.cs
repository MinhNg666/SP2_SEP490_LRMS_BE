using System;
using LRMS_API;

namespace Service.Settings;
public static class FundDisbursementEmailTemplate
{
    public static string GetApprovalEmail(User recipient, Project project, FundDisbursement fundDisbursement, User approver)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #27ae60;'>Fund Disbursement Approved</h2>
        <p>Dear <strong>{recipient.FullName}</strong>,</p>
        <p>Your fund disbursement request for the project <strong>{project.ProjectName}</strong> has been <strong>approved</strong>.</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Disbursement Details:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Requested Amount:</strong> {fundDisbursement.FundRequest:C}</li>
                <li><strong>Approved By:</strong> {approver.FullName}</li>
                <li><strong>Approval Date:</strong> {DateTime.Now:dd/MM/yyyy}</li>
            </ul>
        </div>
        <p>Please check your account or contact the office for further details.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the project leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }

    public static string GetRejectionEmail(User recipient, Project project, FundDisbursement fundDisbursement, User rejector, string rejectionReason)
    {
        return $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
        <h2 style='color: #c94c4c;'>Fund Disbursement Rejected</h2>
        <p>Dear <strong>{recipient.FullName}</strong>,</p>
        <p>Your fund disbursement request for the project <strong>{project.ProjectName}</strong> has been <strong>rejected</strong>.</p>
        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
            <h3 style='margin-top: 0; color: #00477e;'>Disbursement Details:</h3>
            <ul style='padding-left: 20px;'>
                <li><strong>Project Name:</strong> {project.ProjectName}</li>
                <li><strong>Requested Amount:</strong> {fundDisbursement.FundRequest:C}</li>
                <li><strong>Rejected By:</strong> {rejector.FullName}</li>
                <li><strong>Rejection Date:</strong> {DateTime.Now:dd/MM/yyyy}</li>
                <li><strong>Reason:</strong> {rejectionReason}</li>
            </ul>
        </div>
        <p>You may review the reason above and submit a new request if needed.</p>
        <p>Best regards,<br>LRMS System</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
        <p style='font-size: 12px; color: #777;'>
            <em>Note: This is an automated email, please do not reply.<br>
            If you have any questions, please contact the project leader or administrator for assistance.</em>
        </p>
    </div>
</body>
</html>";
    }
}
