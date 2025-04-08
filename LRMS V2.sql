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

DELETE FROM Department;

-- 1. Dữ liệu cho bảng Department (giữ nguyên)
INSERT INTO [Department] ([department_name])
VALUES 
('Information Technology'),
('Computer Science'),
('Software Engineering'),
('Artificial Intelligence'),
('Data Science'),
('Network Security');


-- Add Admin users
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('admin1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Admin One', 'admin1@example.com', '0111111111', 0, NULL, 1, GETDATE(), GETDATE(), 1),
('admin2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Admin Two', 'admin2@example.com', '0111111112', 0, NULL, 1, GETDATE(), GETDATE(), 1);

-- Add Lecturers (10 per department, 2 per level)
-- Department 1: Information Technology
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
-- Level 0 (Professor)
('lecturer1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor John Smith', 'lecturer1@example.com', '0123456001', 1, 0, 1, GETDATE(), GETDATE(), 1),
('lecturer2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Maria Johnson', 'lecturer2@example.com', '0123456002', 1, 0, 1, GETDATE(), GETDATE(), 1),
-- Level 1 (Associate Professor)
('lecturer3', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Robert Williams', 'lecturer3@example.com', '0123456003', 1, 1, 1, GETDATE(), GETDATE(), 1),
('lecturer4', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Jennifer Brown', 'lecturer4@example.com', '0123456004', 1, 1, 1, GETDATE(), GETDATE(), 1),
-- Level 2 (PhD)
('lecturer5', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Michael Davis', 'lecturer5@example.com', '0123456005', 1, 2, 1, GETDATE(), GETDATE(), 1),
('lecturer6', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Sarah Wilson', 'lecturer6@example.com', '0123456006', 1, 2, 1, GETDATE(), GETDATE(), 1),
-- Level 3 (Master)
('lecturer7', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. David Miller', 'lecturer7@example.com', '0123456007', 1, 3, 1, GETDATE(), GETDATE(), 1),
('lecturer8', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Elizabeth Taylor', 'lecturer8@example.com', '0123456008', 1, 3, 1, GETDATE(), GETDATE(), 1),
-- Level 4 (Bachelor)
('lecturer9', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Richard Anderson', 'lecturer9@example.com', '0123456009', 1, 4, 1, GETDATE(), GETDATE(), 1),
('lecturer10', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Patricia Thomas', 'lecturer10@example.com', '0123456010', 1, 4, 1, GETDATE(), GETDATE(), 1);

-- Department 2: Computer Science
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
-- Level 0 (Professor)
('lecturer11', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Charles Wilson', 'lecturer11@example.com', '0123456011', 1, 0, 1, GETDATE(), GETDATE(), 2),
('lecturer12', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Linda Jackson', 'lecturer12@example.com', '0123456012', 1, 0, 1, GETDATE(), GETDATE(), 2),
-- Level 1 (Associate Professor)
('lecturer13', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Daniel White', 'lecturer13@example.com', '0123456013', 1, 1, 1, GETDATE(), GETDATE(), 2),
('lecturer14', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Susan Harris', 'lecturer14@example.com', '0123456014', 1, 1, 1, GETDATE(), GETDATE(), 2),
-- Level 2 (PhD)
('lecturer15', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Thomas Martin', 'lecturer15@example.com', '0123456015', 1, 2, 1, GETDATE(), GETDATE(), 2),
('lecturer16', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Nancy Clark', 'lecturer16@example.com', '0123456016', 1, 2, 1, GETDATE(), GETDATE(), 2),
-- Level 3 (Master)
('lecturer17', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Kenneth Lewis', 'lecturer17@example.com', '0123456017', 1, 3, 1, GETDATE(), GETDATE(), 2),
('lecturer18', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Helen Walker', 'lecturer18@example.com', '0123456018', 1, 3, 1, GETDATE(), GETDATE(), 2),
-- Level 4 (Bachelor)
('lecturer19', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Paul Hall', 'lecturer19@example.com', '0123456019', 1, 4, 1, GETDATE(), GETDATE(), 2),
('lecturer20', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Deborah Allen', 'lecturer20@example.com', '0123456020', 1, 4, 1, GETDATE(), GETDATE(), 2);

-- Department 3: Software Engineering
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
-- Level 0 (Professor)
('lecturer21', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor William Turner', 'lecturer21@example.com', '0123456021', 1, 0, 1, GETDATE(), GETDATE(), 3),
('lecturer22', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Barbara Martinez', 'lecturer22@example.com', '0123456022', 1, 0, 1, GETDATE(), GETDATE(), 3),
-- Level 1 (Associate Professor)
('lecturer23', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. James Robinson', 'lecturer23@example.com', '0123456023', 1, 1, 1, GETDATE(), GETDATE(), 3),
('lecturer24', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Lisa Adams', 'lecturer24@example.com', '0123456024', 1, 1, 1, GETDATE(), GETDATE(), 3),
-- Level 2 (PhD)
('lecturer25', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Andrew Phillips', 'lecturer25@example.com', '0123456025', 1, 2, 1, GETDATE(), GETDATE(), 3),
('lecturer26', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Laura Rodriguez', 'lecturer26@example.com', '0123456026', 1, 2, 1, GETDATE(), GETDATE(), 3),
-- Level 3 (Master)
('lecturer27', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Steven Campbell', 'lecturer27@example.com', '0123456027', 1, 3, 1, GETDATE(), GETDATE(), 3),
('lecturer28', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Michelle Parker', 'lecturer28@example.com', '0123456028', 1, 3, 1, GETDATE(), GETDATE(), 3),
-- Level 4 (Bachelor)
('lecturer29', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Daniel Evans', 'lecturer29@example.com', '0123456029', 1, 4, 1, GETDATE(), GETDATE(), 3),
('lecturer30', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Linda Edwards', 'lecturer30@example.com', '0123456030', 1, 4, 1, GETDATE(), GETDATE(), 3);

-- Department 4: Artificial Intelligence
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
-- Level 0 (Professor)
('lecturer31', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Christopher Baker', 'lecturer31@example.com', '0123456031', 1, 0, 1, GETDATE(), GETDATE(), 4),
('lecturer32', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Karen Wright', 'lecturer32@example.com', '0123456032', 1, 0, 1, GETDATE(), GETDATE(), 4),
-- Level 1 (Associate Professor)
('lecturer33', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Anthony Nelson', 'lecturer33@example.com', '0123456033', 1, 1, 1, GETDATE(), GETDATE(), 4),
('lecturer34', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Sandra Mitchell', 'lecturer34@example.com', '0123456034', 1, 1, 1, GETDATE(), GETDATE(), 4),
-- Level 2 (PhD)
('lecturer35', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Kevin Roberts', 'lecturer35@example.com', '0123456035', 1, 2, 1, GETDATE(), GETDATE(), 4),
('lecturer36', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Carol Turner', 'lecturer36@example.com', '0123456036', 1, 2, 1, GETDATE(), GETDATE(), 4),
-- Level 3 (Master)
('lecturer37', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Jason Phillips', 'lecturer37@example.com', '0123456037', 1, 3, 1, GETDATE(), GETDATE(), 4),
('lecturer38', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Melissa Campbell', 'lecturer38@example.com', '0123456038', 1, 3, 1, GETDATE(), GETDATE(), 4),
-- Level 4 (Bachelor)
('lecturer39', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Brandon Scott', 'lecturer39@example.com', '0123456039', 1, 4, 1, GETDATE(), GETDATE(), 4),
('lecturer40', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Rebecca Stewart', 'lecturer40@example.com', '0123456040', 1, 4, 1, GETDATE(), GETDATE(), 4);

-- Department 5: Data Science
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
-- Level 0 (Professor)
('lecturer41', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Ronald Carter', 'lecturer41@example.com', '0123456041', 1, 0, 1, GETDATE(), GETDATE(), 5),
('lecturer42', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Kimberly Reed', 'lecturer42@example.com', '0123456042', 1, 0, 1, GETDATE(), GETDATE(), 5),
-- Level 1 (Associate Professor)
('lecturer43', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Gary Morris', 'lecturer43@example.com', '0123456043', 1, 1, 1, GETDATE(), GETDATE(), 5),
('lecturer44', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Amy Cook', 'lecturer44@example.com', '0123456044', 1, 1, 1, GETDATE(), GETDATE(), 5),
-- Level 2 (PhD)
('lecturer45', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Jeffrey Bennett', 'lecturer45@example.com', '0123456045', 1, 2, 1, GETDATE(), GETDATE(), 5),
('lecturer46', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Stephanie Wood', 'lecturer46@example.com', '0123456046', 1, 2, 1, GETDATE(), GETDATE(), 5),
-- Level 3 (Master)
('lecturer47', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Brian Rivera', 'lecturer47@example.com', '0123456047', 1, 3, 1, GETDATE(), GETDATE(), 5),
('lecturer48', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Christine Hughes', 'lecturer48@example.com', '0123456048', 1, 3, 1, GETDATE(), GETDATE(), 5),
-- Level 4 (Bachelor)
('lecturer49', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Edward Powell', 'lecturer49@example.com', '0123456049', 1, 4, 1, GETDATE(), GETDATE(), 5),
('lecturer50', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Sharon Long', 'lecturer50@example.com', '0123456050', 1, 4, 1, GETDATE(), GETDATE(), 5);

-- Department 6: Network Security
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
-- Level 0 (Professor)
('lecturer51', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Donald Murphy', 'lecturer51@example.com', '0123456051', 1, 0, 1, GETDATE(), GETDATE(), 6),
('lecturer52', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Professor Ruth Garcia', 'lecturer52@example.com', '0123456052', 1, 0, 1, GETDATE(), GETDATE(), 6),
-- Level 1 (Associate Professor)
('lecturer53', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Steven Russell', 'lecturer53@example.com', '0123456053', 1, 1, 1, GETDATE(), GETDATE(), 6),
('lecturer54', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Assoc. Prof. Brenda Coleman', 'lecturer54@example.com', '0123456054', 1, 1, 1, GETDATE(), GETDATE(), 6),
-- Level 2 (PhD)
('lecturer55', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Timothy Perry', 'lecturer55@example.com', '0123456055', 1, 2, 1, GETDATE(), GETDATE(), 6),
('lecturer56', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dr. Amanda Butler', 'lecturer56@example.com', '0123456056', 1, 2, 1, GETDATE(), GETDATE(), 6),
-- Level 3 (Master)
('lecturer57', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Gregory Foster', 'lecturer57@example.com', '0123456057', 1, 3, 1, GETDATE(), GETDATE(), 6),
('lecturer58', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Angela Simmons', 'lecturer58@example.com', '0123456058', 1, 3, 1, GETDATE(), GETDATE(), 6),
-- Level 4 (Bachelor)
('lecturer59', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mr. Lawrence Ward', 'lecturer59@example.com', '0123456059', 1, 4, 1, GETDATE(), GETDATE(), 6),
('lecturer60', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ms. Cynthia Brooks', 'lecturer60@example.com', '0123456060', 1, 4, 1, GETDATE(), GETDATE(), 6);


-- Add Students (5 per department)
-- Department 1: Information Technology
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'James Student', 'student1@example.com', '0123456101', 2, NULL, 1, GETDATE(), GETDATE(), 1),
('student2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mary Student', 'student2@example.com', '0123456102', 2, NULL, 1, GETDATE(), GETDATE(), 1),
('student3', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Robert Student', 'student3@example.com', '0123456103', 2, NULL, 1, GETDATE(), GETDATE(), 1),
('student4', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Patricia Student', 'student4@example.com', '0123456104', 2, NULL, 1, GETDATE(), GETDATE(), 1),
('student5', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'John Student', 'student5@example.com', '0123456105', 2, NULL, 1, GETDATE(), GETDATE(), 1);

-- Department 2: Computer Science
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student6', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Linda Student', 'student6@example.com', '0123456106', 2, NULL, 1, GETDATE(), GETDATE(), 2),
('student7', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Michael Student', 'student7@example.com', '0123456107', 2, NULL, 1, GETDATE(), GETDATE(), 2),
('student8', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Elizabeth Student', 'student8@example.com', '0123456108', 2, NULL, 1, GETDATE(), GETDATE(), 2),
('student9', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'William Student', 'student9@example.com', '0123456109', 2, NULL, 1, GETDATE(), GETDATE(), 2),
('student10', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Barbara Student', 'student10@example.com', '0123456110', 2, NULL, 1, GETDATE(), GETDATE(), 2);

-- Department 3: Software Engineering
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student11', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Donald Student', 'student11@example.com', '0123456111', 2, NULL, 1, GETDATE(), GETDATE(), 3),
('student12', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Margaret Student', 'student12@example.com', '0123456112', 2, NULL, 1, GETDATE(), GETDATE(), 3),
('student13', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Paul Student', 'student13@example.com', '0123456113', 2, NULL, 1, GETDATE(), GETDATE(), 3),
('student14', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Sandra Student', 'student14@example.com', '0123456114', 2, NULL, 1, GETDATE(), GETDATE(), 3),
('student15', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Mark Student', 'student15@example.com', '0123456115', 2, NULL, 1, GETDATE(), GETDATE(), 3);

-- Department 4: Artificial Intelligence
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student16', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Ashley Student', 'student16@example.com', '0123456116', 2, NULL, 1, GETDATE(), GETDATE(), 4),
('student17', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Steven Student', 'student17@example.com', '0123456117', 2, NULL, 1, GETDATE(), GETDATE(), 4),
('student18', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Kimberly Student', 'student18@example.com', '0123456118', 2, NULL, 1, GETDATE(), GETDATE(), 4),
('student19', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Andrew Student', 'student19@example.com', '0123456119', 2, NULL, 1, GETDATE(), GETDATE(), 4),
('student20', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Emily Student', 'student20@example.com', '0123456120', 2, NULL, 1, GETDATE(), GETDATE(), 4);

-- Department 5: Data Science
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student21', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Joshua Student', 'student21@example.com', '0123456121', 2, NULL, 1, GETDATE(), GETDATE(), 5),
('student22', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Amanda Student', 'student22@example.com', '0123456122', 2, NULL, 1, GETDATE(), GETDATE(), 5),
('student23', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Brandon Student', 'student23@example.com', '0123456123', 2, NULL, 1, GETDATE(), GETDATE(), 5),
('student24', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Melissa Student', 'student24@example.com', '0123456124', 2, NULL, 1, GETDATE(), GETDATE(), 5),
('student25', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Matthew Student', 'student25@example.com', '0123456125', 2, NULL, 1, GETDATE(), GETDATE(), 5);

-- Department 6: Network Security
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('student26', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Laura Student', 'student26@example.com', '0123456126', 2, NULL, 1, GETDATE(), GETDATE(), 6),
('student27', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Kevin Student', 'student27@example.com', '0123456127', 2, NULL, 1, GETDATE(), GETDATE(), 6),
('student28', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Michelle Student', 'student28@example.com', '0123456128', 2, NULL, 1, GETDATE(), GETDATE(), 6),
('student29', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Jason Student', 'student29@example.com', '0123456129', 2, NULL, 1, GETDATE(), GETDATE(), 6),
('student30', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Lisa Student', 'student30@example.com', '0123456130', 2, NULL, 1, GETDATE(), GETDATE(), 6);

-- Add Accounting Department Staff (2 per department)
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('accounting1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'George Accounting', 'accounting1@example.com', '0123456201', 3, NULL, 1, GETDATE(), GETDATE(), 1),
('accounting2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Jennifer Accounting', 'accounting2@example.com', '0123456202', 3, NULL, 1, GETDATE(), GETDATE(), 1),
('accounting3', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Thomas Accounting', 'accounting3@example.com', '0123456203', 3, NULL, 1, GETDATE(), GETDATE(), 2),
('accounting4', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Margaret Accounting', 'accounting4@example.com', '0123456204', 3, NULL, 1, GETDATE(), GETDATE(), 2);

-- Add Office Staff (2 per department)
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [level], [status], [created_at], [updated_at], [department_id])
VALUES 
('office1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Charles Office', 'office1@example.com', '0123456301', 4, NULL, 1, GETDATE(), GETDATE(), 1),
('office2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Susan Office', 'office2@example.com', '0123456302', 4, NULL, 1, GETDATE(), GETDATE(), 1),
('office3', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Joseph Office', 'office3@example.com', '0123456303', 4, NULL, 1, GETDATE(), GETDATE(), 2),
('office4', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Dorothy Office', 'office4@example.com', '0123456304', 4, NULL, 1, GETDATE(), GETDATE(), 2);





-- Update Admin users without department
UPDATE [Users] SET [department_id] = NULL WHERE [role] = 0;

-- Update Accounting staff without department
UPDATE [Users] SET [department_id] = NULL WHERE [role] = 3;

-- Update Office staff without department
UPDATE [Users] SET [department_id] = NULL WHERE [role] = 4;

-- Update lecturer emails (@lecturer)
-- UPDATE Users
-- SET email = username + '@lecturer.com'
-- WHERE role = 1;

-- Update student emails (@student)
-- UPDATE Users
-- SET email = username + '@student.com'
-- WHERE role = 2;

-- Update accounting department emails (@accounting)
-- UPDATE Users
-- SET email = username + '@accounting.com'
-- WHERE role = 3;

-- Update office emails (@office)
-- UPDATE Users
-- SET email = username + '@office.com'
-- WHERE role = 4;

-- Update admin emails (@admin)
-- UPDATE Users
-- SET email = username + '@admin.com'
-- WHERE role = 0;


-- 3. Dữ liệu cho bảng Groups (điều chỉnh lại)
-- INSERT INTO [Groups] ([group_type], [group_name], [max_member], [current_member], [status], [created_at], [created_by], [group_department])
-- VALUES 
-- (1, 'Group A', 10, 5, 1, GETDATE(), 1, 1),
-- (1, 'Group B', 8, 3, 1, GETDATE(), 2, 1),
-- (2, 'Group C', 15, 10, 1, GETDATE(), 3, 2);

-- 4. Dữ liệu cho bảng Group_Member (điều chỉnh lại)
-- INSERT INTO [Group_Member] ([role], [join_date], [status], [user_id], [group_id])
-- VALUES 
-- (0, GETDATE(), 1, 1, 1),
-- (1, GETDATE(), 1, 2, 1),
-- (0, GETDATE(), 1, 3, 2),
-- (1, GETDATE(), 1, 4, 2),
-- (0, GETDATE(), 1, 5, 3),
-- (1, GETDATE(), 1, 6, 3);

-- 5. Dữ liệu cho bảng Projects (điều chỉnh lại)
-- INSERT INTO [Projects] ([project_name], [project_type], [description], [approved_budget], [spent_budget], [status], [start_date], [end_date], 
-- [created_at], [updated_at], [methodlogy], [approved_by], [created_by], [group_id], [department_id])
-- VALUES 
-- (N'Project 1', 1, N'Mô tả dự án 1', 10000.00, 5000.00, 1, GETDATE(), DATEADD(MONTH, 1, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 1', 1, 1, 1, 1),
-- (N'Project 2', 1, N'Mô tả dự án 2', 20000.00, 10000.00, 1, GETDATE(), DATEADD(MONTH, 2, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 2', 2, 2, 1, 1),
-- (N'Project 3', 2, N'Mô tả dự án 3', 15000.00, 7000.00, 1, GETDATE(), DATEADD(MONTH, 3, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 3', 3, 3, 2, 2),
-- (N'Project 4', 2, N'Mô tả dự án 4', 25000.00, 12000.00, 1, GETDATE(), DATEADD(MONTH, 4, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 4', 4, 4, 2, 2),
-- (N'Project 5', 1, N'Mô tả dự án 5', 30000.00, 15000.00, 1, GETDATE(), DATEADD(MONTH, 5, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 5', 5, 5, 1, 1),
-- (N'Project 6', 1, N'Mô tả dự án 6', 18000.00, 9000.00, 1, GETDATE(), DATEADD(MONTH, 6, GETDATE()), GETDATE(), GETDATE(), N'Phương pháp 6', 6, 6, 1, 1);

-- 6. Dữ liệu cho bảng Quotas (điều chỉnh lại)
-- INSERT INTO [Quotas] ([allocated_budget], [status], [created_at], [update_at], [project_id], [allocated_by])
-- VALUES 
-- (5000.00, 1, GETDATE(), GETDATE(), 1, 1),
-- (10000.00, 1, GETDATE(), GETDATE(), 2, 2),
-- (7500.00, 1, GETDATE(), GETDATE(), 3, 3),
-- (12500.00, 1, GETDATE(), GETDATE(), 4, 4),
-- (15000.00, 1, GETDATE(), GETDATE(), 5, 5),
-- (20000.00, 1, GETDATE(), GETDATE(), 6, 6);

-- 7. Dữ liệu cho bảng Milestone (điều chỉnh lại)
-- INSERT INTO [Milestone] ([title], [description], [start_date], [end_date], [status], [assign_to], [assign_by], [project_id])
-- VALUES 
-- (N'Milestone 1', N'Mô tả milestone 1', GETDATE(), DATEADD(DAY, 10, GETDATE()), 1, 1, 1, 1),
-- (N'Milestone 2', N'Mô tả milestone 2', GETDATE(), DATEADD(DAY, 20, GETDATE()), 1, 2, 1, 2),
-- (N'Milestone 3', N'Mô tả milestone 3', GETDATE(), DATEADD(DAY, 30, GETDATE()), 1, 3, 2, 3),
-- (N'Milestone 4', N'Mô tả milestone 4', GETDATE(), DATEADD(DAY, 40, GETDATE()), 1, 4, 2, 4),
-- (N'Milestone 5', N'Mô tả milestone 5', GETDATE(), DATEADD(DAY, 50, GETDATE()), 1, 5, 3, 5),
-- (N'Milestone 6', N'Mô tả milestone 6', GETDATE(), DATEADD(DAY, 60, GETDATE()), 1, 6, 3, 6);

-- 8. Dữ liệu cho bảng Project_resources (điều chỉnh lại)
-- INSERT INTO [Project_resources] ([resource_name], [resource_type], [cost], [quantity], [acquired], [project_id])
-- VALUES 
-- (N'Resource 1', 1, 1000.00, 10, 1, 1),
-- (N'Resource 2', 2, 2000.00, 20, 1, 2),
-- (N'Resource 3', 3, 1500.00, 15, 1, 3),
-- (N'Resource 4', 4, 2500.00, 25, 1, 4),
-- (N'Resource 5', 5, 3000.00, 30, 1, 5),
-- (N'Resource 6', 6, 500.00, 5, 1, 6);

-- 9. Dữ liệu cho bảng Conference (điều chỉnh lại)
-- INSERT INTO [Conference] ([conference_name], [conference_ranking], [location], [presentation_date], [acceptance_date], [presentation_type], [project_id])
-- VALUES 
-- (N'Conference 1', 1, N'Hà Nội', GETDATE(), GETDATE(), 1, 1),
-- (N'Conference 2', 2, N'Đà Nẵng', GETDATE(), GETDATE(), 1, 2),
-- (N'Conference 3', 3, N'TP.HCM', GETDATE(), GETDATE(), 1, 3),
-- (N'Conference 4', 1, N'Hải Phòng', GETDATE(), GETDATE(), 1, 4),
-- (N'Conference 5', 2, N'Nha Trang', GETDATE(), GETDATE(), 1, 5),
-- (N'Conference 6', 3, N'Cần Thơ', GETDATE(), GETDATE(), 1, 6);

-- 10. Dữ liệu cho bảng Conference_expense (điều chỉnh lại)
-- INSERT INTO [Conference_expense] ([accomodation], [accomodation_expense], [travel], [travel_expense], [conference_id])
-- VALUES 
-- ('Hotel A', 100.00, 'Taxi', 50.00, 1),
-- ('Hotel B', 200.00, 'Bus', 100.00, 2),
-- ('Hotel C', 150.00, 'Train', 75.00, 3),
-- ('Hotel D', 250.00, 'Car', 125.00, 4),
-- ('Hotel E', 300.00, 'Plane', 150.00, 5),
-- ('Hotel F', 350.00, 'Bicycle', 175.00, 6);

-- 11. Dữ liệu cho bảng Journal (điều chỉnh lại)
-- INSERT INTO [Journal] ([journal_name], [publisher_name], [publisher_status], [doi_number], [acceptance_date], [publication_date], [submission_date], [reviewer_comments], [project_id])
-- VALUES 
-- ('Journal 1', 'Publisher 1', 1, '10.1234/journal1', GETDATE(), GETDATE(), GETDATE(), 'Comments 1', 1),
-- ('Journal 2', 'Publisher 2', 1, '10.1234/journal2', GETDATE(), GETDATE(), GETDATE(), 'Comments 2', 2),
-- ('Journal 3', 'Publisher 3', 1, '10.1234/journal3', GETDATE(), GETDATE(), GETDATE(), 'Comments 3', 3),
-- ('Journal 4', 'Publisher 4', 1, '10.1234/journal4', GETDATE(), GETDATE(), GETDATE(), 'Comments 4', 4),
-- ('Journal 5', 'Publisher 5', 1, '10.1234/journal5', GETDATE(), GETDATE(), GETDATE(), 'Comments 5', 5),
-- ('Journal 6', 'Publisher 6', 1, '10.1234/journal6', GETDATE(), GETDATE(), GETDATE(), 'Comments 6', 6);

-- 12. Dữ liệu cho bảng Author (điều chỉnh lại)
-- INSERT INTO [Author] ([role], [project_id], [user_id])
-- VALUES 
-- (0, 1, 1),
-- (1, 2, 2),
-- (0, 3, 3),
-- (1, 4, 4),
-- (0, 5, 5),
-- (1, 6, 6);

-- 13. Dữ liệu cho bảng Category (giữ nguyên)
-- INSERT INTO [Category] ([category_name], [status], [project_id])
-- VALUES 
-- ('Category 1', 1, 1),
-- ('Category 2', 1, 1),
-- ('Category 3', 1, 2),
-- ('Category 4', 1, 2),
-- ('Category 5', 1, 3),
-- ('Category 6', 1, 3);

-- 14. Dữ liệu cho bảng Timeline (điều chỉnh lại)
-- INSERT INTO [Timeline] ([start_date], [end_date], [event], [created_at], [update_at], [timeline_type], [created_by])
-- VALUES 
-- (GETDATE(), DATEADD(DAY, 10, GETDATE()), 'Event 1', GETDATE(), GETDATE(), 1, 1),
-- (GETDATE(), DATEADD(DAY, 20, GETDATE()), 'Event 2', GETDATE(), GETDATE(), 1, 2),
-- (GETDATE(), DATEADD(DAY, 30, GETDATE()), 'Event 3', GETDATE(), GETDATE(), 1, 3),
-- (GETDATE(), DATEADD(DAY, 40, GETDATE()), 'Event 4', GETDATE(), GETDATE(), 1, 4),
-- (GETDATE(), DATEADD(DAY, 50, GETDATE()), 'Event 5', GETDATE(), GETDATE(), 1, 5),
-- (GETDATE(), DATEADD(DAY, 60, GETDATE()), 'Event 6', GETDATE(), GETDATE(), 1, 6);

-- 15. Dữ liệu cho bảng Invitations (điều chỉnh lại)
-- INSERT INTO [Invitations] ([status], [message], [created_at], [updated_at], [group_id], [respond_date], [invited_role], [recieve_by], [sent_by])
-- VALUES 
-- (1, 'Invitation 1', GETDATE(), GETDATE(), 1, NULL, 1, 2, 1),
-- (1, 'Invitation 2', GETDATE(), GETDATE(), 1, NULL, 1, 3, 1),
-- (1, 'Invitation 3', GETDATE(), GETDATE(), 2, NULL, 1, 4, 2),
-- (1, 'Invitation 4', GETDATE(), GETDATE(), 2, NULL, 1, 5, 2),
-- (1, 'Invitation 5', GETDATE(), GETDATE(), 3, NULL, 1, 6, 3),
-- (1, 'Invitation 6', GETDATE(), GETDATE(), 3, NULL, 1, 1, 3);

-- 16. Dữ liệu cho bảng Notifications (điều chỉnh lại)
-- INSERT INTO [Notifications] ([created_at], [project_id], [title], [message], [status], [is_read], [invitation_id], [user_id])
-- VALUES 
-- (GETDATE(), 1, N'Thông báo 1', N'Nội dung thông báo 1', 1, 0, 1, 1),
-- (GETDATE(), 2, N'Thông báo 2', N'Nội dung thông báo 2', 1, 0, 2, 2),
-- (GETDATE(), 3, N'Thông báo 3', N'Nội dung thông báo 3', 1, 0, 3, 3),
-- (GETDATE(), 4, N'Thông báo 4', N'Nội dung thông báo 4', 1, 0, 4, 4),
-- (GETDATE(), 5, N'Thông báo 5', N'Nội dung thông báo 5', 1, 0, 5, 5),
-- (GETDATE(), 6, N'Thông báo 6', N'Nội dung thông báo 6', 1, 0, 6, 6);

-- 17. Dữ liệu cho bảng Documents (mới)
-- INSERT INTO [Documents] ([upload_at], [document_url], [file_name], [document_type], [project_id], [milestone_id], [project_resource_id], [conference_expense_id], [uploaded_by])
-- VALUES 
-- (GETDATE(), 'http://example.com/doc1.pdf', 'Document 1', 1, 1, 1, 1, 1, 1),
-- (GETDATE(), 'http://example.com/doc2.pdf', 'Document 2', 1, 2, 2, 2, 2, 2),
-- (GETDATE(), 'http://example.com/doc3.pdf', 'Document 3', 1, 3, 3, 3, 3, 3),
-- (GETDATE(), 'http://example.com/doc4.pdf', 'Document 4', 1, 4, 4, 4, 4, 4),
-- (GETDATE(), 'http://example.com/doc5.pdf', 'Document 5', 1, 5, 5, 5, 5, 5),
-- (GETDATE(), 'http://example.com/doc6.pdf', 'Document 6', 1, 6, 6, 6, 6, 6);

-- 18. Dữ liệu cho bảng Fund_Disbursement (mới)
-- INSERT INTO [Fund_Disbursement] ([fund_request], [status], [created_at], [update_at], [description], [supervisor_request], [author_request], [disburse_by], [project_id])
-- VALUES 
-- (5000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 1', 1, 1, 1, 1),
-- (10000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 2', 2, 2, 2, 2),
-- (7500.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 3', 3, 3, 3, 3),
-- (12500.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 4', 4, 4, 4, 4),
-- (15000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 5', 5, 5, 5, 5),
-- (9000.00, 1, GETDATE(), GETDATE(), N'Yêu cầu giải ngân 6', 6, 6, 6, 6);

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

--  20 Remember to run these SQL script when you pull the new code ( updated 4/4/2025)
ALTER TABLE Documents ALTER COLUMN conference_expense_id INT NULL;
ALTER TABLE Documents ALTER COLUMN project_resource_id INT NULL;
ALTER TABLE Documents ALTER COLUMN fund_disbursement_id INT NULL;

ALTER TABLE Documents DROP CONSTRAINT CHK_Documents_References;
ALTER TABLE Documents ADD CONSTRAINT CHK_Documents_References 
CHECK (project_resource_id IS NOT NULL);
