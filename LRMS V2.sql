USE master;

-- Drop the database if it exists
DROP DATABASE IF EXISTS LRMSDB;
GO

-- Create the database
CREATE DATABASE LRMSDB;
GO

-- Use the created database
USE LRMSDB;
GO

CREATE TABLE [Users] (
    [user_id] INTEGER IDENTITY(1,1),
    [username] NVARCHAR(100),
    [password] NVARCHAR(255),
    [full_name] NVARCHAR(200),
    [email] NVARCHAR(200),
    [phone] NVARCHAR(20),
    [role] INTEGER,
    [level] INTEGER,
    [status] INTEGER,
    [created_at] DATETIME,
    [updated_at] DATETIME,
    [department_id] INTEGER,
    PRIMARY KEY([user_id])
);
GO

CREATE TABLE [Projects] (
    [project_id] INTEGER IDENTITY(1,1),
    [project_name] NVARCHAR(255),
    [project_type] INTEGER,
    [description] NVARCHAR(MAX),
    [approved_budget] DECIMAL(18,2),
    [spent_budget] DECIMAL(18,2) NOT NULL,
    [status] INTEGER,
    [start_date] DATETIME,
    [end_date] DATETIME,
    [created_at] DATETIME,
    [updated_at] DATETIME,
    [methodlogy] NVARCHAR(MAX) NOT NULL,
    [approved_by] INTEGER,
    [created_by] INTEGER,
    [group_id] INTEGER,
    [department_id] INTEGER,
    PRIMARY KEY([project_id])
);
GO

CREATE TABLE [Quotas] (
    [quota_id] INTEGER IDENTITY(1,1),
    [allocated_budget] DECIMAL(18,2),
    [status] INTEGER,
    [created_at] DATETIME,
    [update_at] DATETIME,
    [project_id] INTEGER,
    [allocated_by] INTEGER,
    PRIMARY KEY([quota_id])
);
GO

CREATE TABLE [Notifications] (
    [notification_id] INTEGER IDENTITY(1,1),
    [created_at] DATETIME,
    [project_id] INTEGER,
    [title] NVARCHAR(255),
    [message] NVARCHAR(MAX),
    [status] INTEGER,
    [is_read] BIT,
    [invitation_id] INTEGER,
    [user_id] INTEGER,
    PRIMARY KEY([notification_id])
);
GO

CREATE TABLE [Groups] (
    [group_id] INTEGER IDENTITY(1,1),
    [group_type] INTEGER,
    [group_name] NVARCHAR(200),
    [max_member] INTEGER,
    [current_member] INTEGER,
    [status] INTEGER,
    [created_at] DATETIME,
    [updated_at] DATETIME,
    [created_by] INTEGER,
    [group_department] INTEGER,
    PRIMARY KEY([group_id])
);
GO

CREATE TABLE [Category] (
    [category_id] INTEGER IDENTITY(1,1),
    [category_name] NVARCHAR(200),
    [status] INTEGER,
    [project_id] INTEGER,
    PRIMARY KEY([category_id])
);
GO

CREATE TABLE [Group_Member] (
    [group_member_id] INTEGER IDENTITY(1,1),
    [role] INTEGER,
    [join_date] DATETIME,
    [status] INTEGER,
    [user_id] INTEGER,
    [group_id] INTEGER,
    PRIMARY KEY([group_member_id])
);
GO

CREATE TABLE [Project_resources] (
    [project_resource_id] INTEGER IDENTITY(1,1),
    [resource_name] NVARCHAR(200) NOT NULL,
    [resource_type] INTEGER,
    [cost] DECIMAL(18,2),
    [quantity] INTEGER,
    [acquired] BIT,
    [project_id] INTEGER,
    PRIMARY KEY([project_resource_id])
);
GO

CREATE TABLE [Milestone] (
    [milestone_id] INTEGER IDENTITY(1,1),
    [title] NVARCHAR(255),
    [description] NVARCHAR(MAX),
    [start_date] DATETIME,
    [end_date] DATETIME,
    [status] INTEGER,
    [assign_to] INTEGER,
    [assign_by] INTEGER,
    [project_id] INTEGER,
    PRIMARY KEY([milestone_id])
);
GO

CREATE TABLE [Conference] (
    [conference_id] INTEGER IDENTITY(1,1),
    [conference_name] NVARCHAR(255),
    [conference_ranking] INTEGER,
    [location] NVARCHAR(255),
    [presentation_date] DATETIME,
    [acceptance_date] DATETIME,
    [presentation_type] INTEGER,
    [project_id] INTEGER,
    PRIMARY KEY([conference_id])
);
GO

CREATE TABLE [Journal] (
    [journal_id] INTEGER IDENTITY(1,1),
    [journal_name] NVARCHAR(255),
    [publisher_name] NVARCHAR(255),
    [publisher_status] INTEGER,
    [doi_number] NVARCHAR(100),
    [acceptance_date] DATETIME,
    [publication_date] DATETIME,
    [submission_date] DATETIME,
    [reviewer_comments] NVARCHAR(MAX),
    [project_id] INTEGER,
    PRIMARY KEY([journal_id])
);
GO

CREATE TABLE [Conference_expense] (
    [expense_id] INTEGER IDENTITY(1,1),
    [accomodation] NVARCHAR(255),
    [accomodation_expense] DECIMAL(18,2),
    [travel] NVARCHAR(255),
    [travel_expense] DECIMAL(18,2),
    [conference_id] INTEGER,
    PRIMARY KEY([expense_id])
);
GO

CREATE TABLE [Author] (
    [author_id] INTEGER IDENTITY(1,1),
    [role] INTEGER,
    [project_id] INTEGER NOT NULL,
    [user_id] INTEGER,
    PRIMARY KEY([author_id])
);
GO

CREATE TABLE [Department] (
    [department_id] INTEGER IDENTITY(1,1),
    [department_name] NVARCHAR(200),
    PRIMARY KEY([department_id])
);
GO

CREATE TABLE [Timeline] (
    [timeline_id] INTEGER IDENTITY(1,1),
    [start_date] DATETIME,
    [end_date] DATETIME,
    [event] NVARCHAR(255),
    [created_at] DATETIME,
    [update_at] DATETIME,
    [timeline_type] INTEGER,
    [created_by] INTEGER,
    PRIMARY KEY([timeline_id])
);
GO

CREATE TABLE [Invitations] (
    [invitation_id] INTEGER IDENTITY(1,1),
    [status] INTEGER,
    [message] NVARCHAR(MAX),
    [created_at] DATETIME,
    [updated_at] DATETIME,
    [group_id] INTEGER,
    [respond_date] DATETIME,
    [invited_role] INTEGER,
    [recieve_by] INTEGER,
    [sent_by] INTEGER,
    PRIMARY KEY([invitation_id])
);
GO

CREATE TABLE [Documents] (
    [document_id] INTEGER IDENTITY(1,1),
    [upload_at] DATETIME,
    [document_url] NVARCHAR(500),
    [file_name] NVARCHAR(255),
    [document_type] INTEGER,
    [project_id] INTEGER,
    [milestone_id] INTEGER,
    [project_resource_id] INTEGER NOT NULL,
    [conference_expense_id] INTEGER NOT NULL,
    [fund_disbursement_id] INTEGER,
    [uploaded_by] INTEGER,
    PRIMARY KEY([document_id])
);
GO

CREATE TABLE [Fund_Disbursement] (
    [fund_disbursement_id] INTEGER IDENTITY(1,1),
    [fund_request] DECIMAL(18,2),
    [status] INTEGER,
    [created_at] DATETIME,
    [update_at] DATETIME,
    [description] NVARCHAR(MAX),
    [supervisor_request] INTEGER NOT NULL,
    [author_request] INTEGER NOT NULL,
    [appoved_by] INTEGER,
    [disburse_by] INTEGER,
    [project_id] INTEGER,
    PRIMARY KEY([fund_disbursement_id])
);
GO

-- Add Foreign Key Constraints
ALTER TABLE [Users]
ADD CONSTRAINT FK_Users_Department
FOREIGN KEY([department_id]) REFERENCES [Department]([department_id]);
GO

ALTER TABLE [Groups]
ADD CONSTRAINT FK_Groups_Department
FOREIGN KEY([group_department]) REFERENCES [Department]([department_id]);
GO

ALTER TABLE [Groups]
ADD CONSTRAINT FK_Groups_Users
FOREIGN KEY([created_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Group_Member]
ADD CONSTRAINT FK_GroupMember_Groups
FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id]);
GO

ALTER TABLE [Group_Member]
ADD CONSTRAINT FK_GroupMember_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_Users_Created
FOREIGN KEY([created_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_GroupMember_Approved
FOREIGN KEY([approved_by]) REFERENCES [Group_Member]([group_member_id]);
GO

ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_Groups
FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id]);
GO

ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_Department
FOREIGN KEY([department_id]) REFERENCES [Department]([department_id]);
GO

ALTER TABLE [Milestone]
ADD CONSTRAINT FK_Milestone_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Milestone]
ADD CONSTRAINT FK_Milestone_GroupMember_AssignTo
FOREIGN KEY([assign_to]) REFERENCES [Group_Member]([group_member_id]);
GO

ALTER TABLE [Milestone]
ADD CONSTRAINT FK_Milestone_GroupMember_AssignBy
FOREIGN KEY([assign_by]) REFERENCES [Group_Member]([group_member_id]);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_Milestone
FOREIGN KEY([milestone_id]) REFERENCES [Milestone]([milestone_id]);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_ProjectResources
FOREIGN KEY([project_resource_id]) REFERENCES [Project_resources]([project_resource_id]);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_ConferenceExpense
FOREIGN KEY([conference_expense_id]) REFERENCES [Conference_expense]([expense_id]);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_Users
FOREIGN KEY([uploaded_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_FundDisbursement
FOREIGN KEY([fund_disbursement_id]) REFERENCES [Fund_Disbursement]([fund_disbursement_id]);
GO

ALTER TABLE [Project_resources]
ADD CONSTRAINT FK_ProjectResources_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Conference]
ADD CONSTRAINT FK_Conference_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Conference_expense]
ADD CONSTRAINT FK_ConferenceExpense_Conference
FOREIGN KEY([conference_id]) REFERENCES [Conference]([conference_id]);
GO

ALTER TABLE [Journal]
ADD CONSTRAINT FK_Journal_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Author]
ADD CONSTRAINT FK_Author_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Author]
ADD CONSTRAINT FK_Author_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Category]
ADD CONSTRAINT FK_Category_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Quotas]
ADD CONSTRAINT FK_Quotas_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Quotas]
ADD CONSTRAINT FK_Quotas_Users
FOREIGN KEY([allocated_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Timeline]
ADD CONSTRAINT FK_Timeline_Users
FOREIGN KEY([created_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Invitations]
ADD CONSTRAINT FK_Invitations_Groups
FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id]);
GO

ALTER TABLE [Invitations]
ADD CONSTRAINT FK_Invitations_Users_Receive
FOREIGN KEY([recieve_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Invitations]
ADD CONSTRAINT FK_Invitations_Users_Sent
FOREIGN KEY([sent_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Notifications]
ADD CONSTRAINT FK_Notifications_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Notifications]
ADD CONSTRAINT FK_Notifications_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Notifications]
ADD CONSTRAINT FK_Notifications_Invitations
FOREIGN KEY([invitation_id]) REFERENCES [Invitations]([invitation_id]);
GO

ALTER TABLE [Fund_Disbursement]
ADD CONSTRAINT FK_FundDisbursement_GroupMember
FOREIGN KEY([supervisor_request]) REFERENCES [Group_Member]([group_member_id]);
GO

ALTER TABLE [Fund_Disbursement]
ADD CONSTRAINT FK_FundDisbursement_Author
FOREIGN KEY([author_request]) REFERENCES [Author]([author_id]);
GO

ALTER TABLE [Fund_Disbursement]
ADD CONSTRAINT FK_FundDisbursement_Users
FOREIGN KEY([disburse_by]) REFERENCES [Users]([user_id]);
GO

ALTER TABLE [Fund_Disbursement]
ADD CONSTRAINT FK_FundDisbursement_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id]);
GO

ALTER TABLE [Fund_Disbursement]
ADD CONSTRAINT FK_FundDisbursement_GroupMember_Approved
FOREIGN KEY([appoved_by]) REFERENCES [Group_Member]([group_member_id]);
GO

-- Add Check Constraints
ALTER TABLE [Projects]
ADD CONSTRAINT CHK_Budget 
CHECK (approved_budget >= spent_budget);
GO

ALTER TABLE [Groups]
ADD CONSTRAINT CHK_Members 
CHECK (current_member <= max_member);
GO

ALTER TABLE [Documents]
ADD CONSTRAINT CHK_Documents_References
CHECK (
    (project_resource_id IS NOT NULL AND conference_expense_id IS NOT NULL)
);
GO

ALTER TABLE [Fund_Disbursement]
ADD CONSTRAINT CHK_Fund_Disbursement_Requests
CHECK (
    (supervisor_request IS NOT NULL AND author_request IS NOT NULL)
);
GO

-- Create Indexes
CREATE INDEX IDX_Projects_Status ON Projects(status);
CREATE INDEX IDX_Users_Role ON Users(role);
CREATE INDEX IDX_Documents_Type ON Documents(document_type);
GO

-- 1. Dữ liệu cho bảng Department (giữ nguyên)
INSERT INTO [Department] ([department_name])
VALUES 
('Department 1'),
('Department 2'),
('Department 3'),
('Department 4'),
('Department 5'),
('Department 6');

-- 2. Dữ liệu cho bảng Users (điều chỉnh lại)
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('user1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', N'Nguyễn Văn A', 'lecturer1@example.com', '0123456789', 1, 0, 1, GETDATE(), GETDATE(), 1),
('user2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', N'Trần Thị B', 'student1@example.com', '0123456788', 2, 1, 1, GETDATE(), GETDATE(), 1),
('user3', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', N'Lê Văn C', 'lecturer2@example.com', '0123456787', 1, 2, 1, GETDATE(), GETDATE(), 2),
('user4', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', N'Phạm Thị D', 'office1@example.com', '0123456786', 4, 3, 1, GETDATE(), GETDATE(), 2),
('user5', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', N'Nguyễn Văn E', 'accountingDep1@example.com', '0123456785', 3, 4, 1, GETDATE(), GETDATE(), 1),
('user6', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', N'Trần Thị F', 'accountingDep2@example.com', '0123456784', 3, 2, 1, GETDATE(), GETDATE(), 1);

-- Add more lecturers
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('Johnson', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Michael Johnson', 'johnson@example.com', '0123487111', 1, 2, 1, GETDATE(), GETDATE(), 1),
('Smith', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Sarah Smith', 'smith@example.com', '0123487222', 1, 1, 1, GETDATE(), GETDATE(), 2),
('Davis', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Robert Davis', 'davis@example.com', '0123487333', 1, 3, 1, GETDATE(), GETDATE(), 1),
('Wilson', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Emily Wilson', 'wilson@example.com', '0123487444', 1, 2, 1, GETDATE(), GETDATE(), 3);

-- Add more students
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student_brown', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'James Brown', 'brown@example.com', '0123476111', 2, NULL, 1, GETDATE(), GETDATE(), 1),
('student_miller', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Jennifer Miller', 'miller@example.com', '0123476222', 2, NULL, 1, GETDATE(), GETDATE(), 2),
('student_taylor', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'David Taylor', 'taylor@example.com', '0123476333', 2, NULL, 1, GETDATE(), GETDATE(), 1),
('student_anderson', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Emma Anderson', 'anderson@example.com', '0123476444', 2, NULL, 1, GETDATE(), GETDATE(), 3),
('student_thomas', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'William Thomas', 'thomas@example.com', '0123476555', 2, NULL, 1, GETDATE(), GETDATE(), 2),
('student_jackson', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Olivia Jackson', 'jackson@example.com', '0123476666', 2, NULL, 1, GETDATE(), GETDATE(), 1);

-- Update lecturer emails (@lecturer)
UPDATE Users
SET email = username + '@lecturer.com'
WHERE role = 1;

-- Update student emails (@student)
UPDATE Users
SET email = username + '@student.com'
WHERE role = 2;

-- Update accounting department emails (@accounting)
UPDATE Users
SET email = username + '@accounting.com'
WHERE role = 3;

-- Update office emails (@office)
UPDATE Users
SET email = username + '@office.com'
WHERE role = 4;

-- Update admin emails (@admin)
UPDATE Users
SET email = username + '@admin.com'
WHERE role = 0;


-- 3. Dữ liệu cho bảng Groups (điều chỉnh lại)
INSERT INTO [Groups] ([group_type], [group_name], [max_member], [current_member], [status], [created_at], [created_by], [group_department])
VALUES 
(1, 'Group A', 10, 5, 1, GETDATE(), 1, 1),
(1, 'Group B', 8, 3, 1, GETDATE(), 2, 1),
(2, 'Group C', 15, 10, 1, GETDATE(), 3, 2);

-- 4. Dữ liệu cho bảng Group_Member (điều chỉnh lại)
INSERT INTO [Group_Member] ([role], [join_date], [status], [user_id], [group_id])
VALUES 
(0, GETDATE(), 1, 1, 1),
(1, GETDATE(), 1, 2, 1),
(0, GETDATE(), 1, 3, 2),
(1, GETDATE(), 1, 4, 2),
(0, GETDATE(), 1, 5, 3),
(1, GETDATE(), 1, 6, 3);

-- 5. Dữ liệu cho bảng Projects (điều chỉnh lại)
INSERT INTO [Projects] ([project_name], [project_type], [description], [approved_budget], [spent_budget], [status], [start_date], [end_date], 
[created_at], [updated_at], [methodlogy], [approved_by], [created_by], [group_id], [department_id])
VALUES 
(N'Project 1', 1, N'Mô tả dự án 1', 10000.00, 5000.00, 1, GETDATE(), DATEADD(MONTH, 1, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 1', 1, 1, 1, 1),
(N'Project 2', 1, N'Mô tả dự án 2', 20000.00, 10000.00, 1, GETDATE(), DATEADD(MONTH, 2, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 2', 2, 2, 1, 1),
(N'Project 3', 2, N'Mô tả dự án 3', 15000.00, 7000.00, 1, GETDATE(), DATEADD(MONTH, 3, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 3', 3, 3, 2, 2),
(N'Project 4', 2, N'Mô tả dự án 4', 25000.00, 12000.00, 1, GETDATE(), DATEADD(MONTH, 4, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 4', 4, 4, 2, 2),
(N'Project 5', 1, N'Mô tả dự án 5', 30000.00, 15000.00, 1, GETDATE(), DATEADD(MONTH, 5, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 5', 5, 5, 1, 1),
(N'Project 6', 1, N'Mô tả dự án 6', 18000.00, 9000.00, 1, GETDATE(), DATEADD(MONTH, 6, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 6', 6, 6, 1, 1);

-- 6. Dữ liệu cho bảng Quotas (điều chỉnh lại)
INSERT INTO [Quotas] ([allocated_budget], [status], [created_at], [update_at], [project_id], [allocated_by])
VALUES 
(5000.00, 1, GETDATE(), GETDATE(), 1, 1),
(10000.00, 1, GETDATE(), GETDATE(), 2, 2),
(7500.00, 1, GETDATE(), GETDATE(), 3, 3),
(12500.00, 1, GETDATE(), GETDATE(), 4, 4),
(15000.00, 1, GETDATE(), GETDATE(), 5, 5),
(20000.00, 1, GETDATE(), GETDATE(), 6, 6);

-- 7. Dữ liệu cho bảng Milestone (điều chỉnh lại)
INSERT INTO [Milestone] ([title], [description], [start_date], [end_date], [status], [assign_to], [assign_by], [project_id])
VALUES 
(N'Milestone 1', N'Mô tả milestone 1', GETDATE(), DATEADD(DAY, 10, GETDATE()), 1, 1, 1, 1),
(N'Milestone 2', N'Mô tả milestone 2', GETDATE(), DATEADD(DAY, 20, GETDATE()), 1, 2, 1, 2),
(N'Milestone 3', N'Mô tả milestone 3', GETDATE(), DATEADD(DAY, 30, GETDATE()), 1, 3, 2, 3),
(N'Milestone 4', N'Mô tả milestone 4', GETDATE(), DATEADD(DAY, 40, GETDATE()), 1, 4, 2, 4),
(N'Milestone 5', N'Mô tả milestone 5', GETDATE(), DATEADD(DAY, 50, GETDATE()), 1, 5, 3, 5),
(N'Milestone 6', N'Mô tả milestone 6', GETDATE(), DATEADD(DAY, 60, GETDATE()), 1, 6, 3, 6);

-- 8. Dữ liệu cho bảng Project_resources (điều chỉnh lại)
INSERT INTO [Project_resources] ([resource_name], [resource_type], [cost], [quantity], [acquired], [project_id])
VALUES 
(N'Resource 1', 1, 1000.00, 10, 1, 1),
(N'Resource 2', 2, 2000.00, 20, 1, 2),
(N'Resource 3', 3, 1500.00, 15, 1, 3),
(N'Resource 4', 4, 2500.00, 25, 1, 4),
(N'Resource 5', 5, 3000.00, 30, 1, 5),
(N'Resource 6', 6, 500.00, 5, 1, 6);

-- 9. Dữ liệu cho bảng Conference (điều chỉnh lại)
INSERT INTO [Conference] ([conference_name], [conference_ranking], [location], [presentation_date], [acceptance_date], [presentation_type], [project_id])
VALUES 
(N'Conference 1', 1, N'Hà Nội', GETDATE(), GETDATE(), 1, 1),
(N'Conference 2', 2, N'Đà Nẵng', GETDATE(), GETDATE(), 1, 2),
(N'Conference 3', 3, N'TP.HCM', GETDATE(), GETDATE(), 1, 3),
(N'Conference 4', 1, N'Hải Phòng', GETDATE(), GETDATE(), 1, 4),
(N'Conference 5', 2, N'Nha Trang', GETDATE(), GETDATE(), 1, 5),
(N'Conference 6', 3, N'Cần Thơ', GETDATE(), GETDATE(), 1, 6);

-- 10. Dữ liệu cho bảng Conference_expense (điều chỉnh lại)
INSERT INTO [Conference_expense] ([accomodation], [accomodation_expense], [travel], [travel_expense], [conference_id])
VALUES 
('Hotel A', 100.00, 'Taxi', 50.00, 1),
('Hotel B', 200.00, 'Bus', 100.00, 2),
('Hotel C', 150.00, 'Train', 75.00, 3),
('Hotel D', 250.00, 'Car', 125.00, 4),
('Hotel E', 300.00, 'Plane', 150.00, 5),
('Hotel F', 350.00, 'Bicycle', 175.00, 6);

-- 11. Dữ liệu cho bảng Journal (điều chỉnh lại)
INSERT INTO [Journal] ([journal_name], [publisher_name], [publisher_status], [doi_number], [acceptance_date], [publication_date], [submission_date], [reviewer_comments], [project_id])
VALUES 
('Journal 1', 'Publisher 1', 1, '10.1234/journal1', GETDATE(), GETDATE(), GETDATE(), 'Comments 1', 1),
('Journal 2', 'Publisher 2', 1, '10.1234/journal2', GETDATE(), GETDATE(), GETDATE(), 'Comments 2', 2),
('Journal 3', 'Publisher 3', 1, '10.1234/journal3', GETDATE(), GETDATE(), GETDATE(), 'Comments 3', 3),
('Journal 4', 'Publisher 4', 1, '10.1234/journal4', GETDATE(), GETDATE(), GETDATE(), 'Comments 4', 4),
('Journal 5', 'Publisher 5', 1, '10.1234/journal5', GETDATE(), GETDATE(), GETDATE(), 'Comments 5', 5),
('Journal 6', 'Publisher 6', 1, '10.1234/journal6', GETDATE(), GETDATE(), GETDATE(), 'Comments 6', 6);

-- 12. Dữ liệu cho bảng Author (điều chỉnh lại)
INSERT INTO [Author] ([role], [project_id], [user_id])
VALUES 
(0, 1, 1),
(1, 2, 2),
(0, 3, 3),
(1, 4, 4),
(0, 5, 5),
(1, 6, 6);

-- 13. Dữ liệu cho bảng Category (giữ nguyên)
INSERT INTO [Category] ([category_name], [status], [project_id])
VALUES 
('Category 1', 1, 1),
('Category 2', 1, 1),
('Category 3', 1, 2),
('Category 4', 1, 2),
('Category 5', 1, 3),
('Category 6', 1, 3);

-- 14. Dữ liệu cho bảng Timeline (điều chỉnh lại)
INSERT INTO [Timeline] ([start_date], [end_date], [event], [created_at], [update_at], [timeline_type], [created_by])
VALUES 
(GETDATE(), DATEADD(DAY, 10, GETDATE()), 'Event 1', GETDATE(), GETDATE(), 1, 1),
(GETDATE(), DATEADD(DAY, 20, GETDATE()), 'Event 2', GETDATE(), GETDATE(), 1, 2),
(GETDATE(), DATEADD(DAY, 30, GETDATE()), 'Event 3', GETDATE(), GETDATE(), 1, 3),
(GETDATE(), DATEADD(DAY, 40, GETDATE()), 'Event 4', GETDATE(), GETDATE(), 1, 4),
(GETDATE(), DATEADD(DAY, 50, GETDATE()), 'Event 5', GETDATE(), GETDATE(), 1, 5),
(GETDATE(), DATEADD(DAY, 60, GETDATE()), 'Event 6', GETDATE(), GETDATE(), 1, 6);

-- 15. Dữ liệu cho bảng Invitations (điều chỉnh lại)
INSERT INTO [Invitations] ([status], [message], [created_at], [updated_at], [group_id], [respond_date], [invited_role], [recieve_by], [sent_by])
VALUES 
(1, 'Invitation 1', GETDATE(), GETDATE(), 1, NULL, 1, 2, 1),
(1, 'Invitation 2', GETDATE(), GETDATE(), 1, NULL, 1, 3, 1),
(1, 'Invitation 3', GETDATE(), GETDATE(), 2, NULL, 1, 4, 2),
(1, 'Invitation 4', GETDATE(), GETDATE(), 2, NULL, 1, 5, 2),
(1, 'Invitation 5', GETDATE(), GETDATE(), 3, NULL, 1, 6, 3),
(1, 'Invitation 6', GETDATE(), GETDATE(), 3, NULL, 1, 1, 3);

-- 16. Dữ liệu cho bảng Notifications (điều chỉnh lại)
INSERT INTO [Notifications] ([created_at], [project_id], [title], [message], [status], [is_read], [invitation_id], [user_id])
VALUES 
(GETDATE(), 1, N'Thông báo 1', N'Nội dung thông báo 1', 1, 0, 1, 1),
(GETDATE(), 2, N'Thông báo 2', N'Nội dung thông báo 2', 1, 0, 2, 2),
(GETDATE(), 3, N'Thông báo 3', N'Nội dung thông báo 3', 1, 0, 3, 3),
(GETDATE(), 4, N'Thông báo 4', N'Nội dung thông báo 4', 1, 0, 4, 4),
(GETDATE(), 5, N'Thông báo 5', N'Nội dung thông báo 5', 1, 0, 5, 5),
(GETDATE(), 6, N'Thông báo 6', N'Nội dung thông báo 6', 1, 0, 6, 6);

-- 17. Dữ liệu cho bảng Documents (mới)
INSERT INTO [Documents] ([upload_at], [document_url], [file_name], [document_type], [project_id], [milestone_id], [project_resource_id], [conference_expense_id], [uploaded_by])
VALUES 
(GETDATE(), 'http://example.com/doc1.pdf', 'Document 1', 1, 1, 1, 1, 1, 1),
(GETDATE(), 'http://example.com/doc2.pdf', 'Document 2', 1, 2, 2, 2, 2, 2),
(GETDATE(), 'http://example.com/doc3.pdf', 'Document 3', 1, 3, 3, 3, 3, 3),
(GETDATE(), 'http://example.com/doc4.pdf', 'Document 4', 1, 4, 4, 4, 4, 4),
(GETDATE(), 'http://example.com/doc5.pdf', 'Document 5', 1, 5, 5, 5, 5, 5),
(GETDATE(), 'http://example.com/doc6.pdf', 'Document 6', 1, 6, 6, 6, 6, 6);

-- 18. Dữ liệu cho bảng Fund_Disbursement (mới)
INSERT INTO [Fund_Disbursement] ([fund_request], [status], [created_at], [update_at], [description], [supervisor_request], [author_request], [disburse_by], [project_id])
VALUES 
(5000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 1', 1, 1, 1, 1),
(10000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 2', 2, 2, 2, 2),
(7500.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 3', 3, 3, 3, 3),
(12500.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 4', 4, 4, 4, 4),
(15000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 5', 5, 5, 5, 5),
(9000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 6', 6, 6, 6, 6);

--  19 Remember to run these SQL script when you pull the new code ( updated 4/4/2025)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RefreshToken')
BEGIN
    ALTER TABLE Users ADD RefreshToken nvarchar(max) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RefreshTokenExpiryTime')
BEGIN
    ALTER TABLE Users ADD RefreshTokenExpiryTime datetime2 NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastLogin')
BEGIN
    ALTER TABLE Users ADD LastLogin datetime2 NULL;
END





