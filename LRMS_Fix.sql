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

-- Create tables with proper data types and constraints
CREATE TABLE [Users] (
	[user_id] INTEGER IDENTITY(1,1),
	[username] NVARCHAR(100),
	[password] NVARCHAR(255),
	[full_name] NVARCHAR(200),
	[email] NVARCHAR(200),
	[phone] NVARCHAR(20),
	[role] INTEGER,
	[department_id] INTEGER,
	[status] INTEGER,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	[group_id] INTEGER,
	[level] INTEGER,
	PRIMARY KEY([user_id])
);
GO

CREATE TABLE [Groups] (
	[group_id] INTEGER NOT NULL IDENTITY(1,1),
	[group_name] NVARCHAR(200),
	[max_member] INTEGER,
	[current_member] INTEGER,
	[status] INTEGER,
	[created_at] DATETIME,
	[group_type] INTEGER,
	PRIMARY KEY([group_id])
);
GO

CREATE TABLE [Group_Member] (
	[group_member_id] INTEGER NOT NULL IDENTITY(1,1),
	[group_id] INTEGER,
	[member_name] NVARCHAR(200),
	[member_email] NVARCHAR(200),
	[role] INTEGER,
	[user_id] INTEGER,
	[status] INTEGER,
	[join_date] DATETIME,
	PRIMARY KEY([group_member_id])
);
GO

CREATE TABLE [Projects] (
	[project_id] INTEGER IDENTITY(1,1),
	[title] NVARCHAR(255),
	[description] NVARCHAR(MAX),
	[user_id] INTEGER,
	[start_date] DATETIME,
	[end_date] DATETIME,
	[status] INTEGER,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	[group_id] INTEGER,
	[project_type] INTEGER,
	[approved_by] INTEGER,
	[objective] NVARCHAR(MAX),
	[methodology] NVARCHAR(MAX),
	[type] INTEGER,
	[approved_budget] DECIMAL(18,2),
	[spent_budget] DECIMAL(18,2),
	PRIMARY KEY([project_id])
);
GO

CREATE TABLE [Quotas] (
	[quota_id] INTEGER IDENTITY(1,1),
	[quota_amount] INTEGER,
	[allocated_by] INTEGER,
	[allocated_at] DATETIME,
	[project_id] INTEGER,
	[limit_value] DECIMAL(18,2),
	[current_value] DECIMAL(18,2),
	PRIMARY KEY([quota_id])
);
GO

CREATE TABLE [Milestone] (
	[milestone_id] INTEGER NOT NULL IDENTITY(1,1),
	[project_id] INTEGER,
	[title] NVARCHAR(255),
	[description] NVARCHAR(MAX),
	[start_date] DATETIME,
	[end_date] DATETIME,
	[status] INTEGER,
	[assign_to] INTEGER,
	[progress_percentage] DECIMAL(5,2),
	PRIMARY KEY([milestone_id])
);
GO

CREATE TABLE [Documents] (
	[document_id] INTEGER NOT NULL IDENTITY(1,1),
	[project_id] INTEGER,
	[milestone_id] INTEGER,
	[document_url] NVARCHAR(500),
	[file_name] NVARCHAR(255),
	[document_type] INTEGER,
	[upload_at] DATETIME,
	[upload_by] INTEGER,
	PRIMARY KEY([document_id])
);
GO

CREATE TABLE [Notifications] (
	[notification_id] INTEGER IDENTITY(1,1),
	[user_id] INTEGER,
	[project_id] INTEGER,
	[title] NVARCHAR(255),
	[message] NVARCHAR(MAX),
	[status] INTEGER,
	[created_at] DATETIME,
	[invitation_id] INTEGER,
	PRIMARY KEY([notification_id])
);
GO

CREATE TABLE [Category] (
	[category_id] INTEGER NOT NULL IDENTITY(1,1),
	[category_name] NVARCHAR(200),
	[project_id] INTEGER,
	PRIMARY KEY([category_id])
);
GO

CREATE TABLE [Project_resources] (
	[resource_id] INTEGER NOT NULL IDENTITY(1,1),
	[project_id] INTEGER,
	[resource_type] INTEGER,
	[quantity] INTEGER,
	[acquired] BIT,
	[estimated_cost] DECIMAL(18,2),
	PRIMARY KEY([resource_id])
);
GO

CREATE TABLE [Conference] (
	[conference_id] INTEGER NOT NULL IDENTITY(1,1),
	[project_id] INTEGER,
	[conference_name] VARCHAR(255),
	[conference_ranking] INTEGER,
	[location] VARCHAR(255),
	[presentation_date] DATETIME,
	[acceptance_date] DATETIME,
	[conference_url] VARCHAR(500),
	[presentation_type] INTEGER,
	[conference_proceedings] VARCHAR(500),
	PRIMARY KEY([conference_id])
);
GO

CREATE TABLE [Journal] (
	[journal_id] INTEGER NOT NULL IDENTITY(1,1),
	[project_id] INTEGER,
	[publisher_approval] BIT,
	[doi_number] VARCHAR(100),
	[pages] VARCHAR(50),
	[submission_date] DATETIME,
	[acceptance_date] DATETIME,
	[publication_date] DATETIME,
	[journal_name] VARCHAR(255),
	[reviewer_comments] NVARCHAR(MAX),
	[revision_history] NVARCHAR(MAX),
	PRIMARY KEY([journal_id])
);
GO

CREATE TABLE [Conference_expense] (
	[expense_id] INTEGER NOT NULL IDENTITY(1,1),
	[conference_id] INTEGER,
	[travel_expense] DECIMAL(18,2),
	[transportation_expense] DECIMAL(18,2),
	[transportation] VARCHAR(100),
	[accomodation] VARCHAR(100),
	PRIMARY KEY([expense_id])
);
GO

CREATE TABLE [Author] (
	[author_id] INTEGER NOT NULL IDENTITY(1,1),
	[project_id] INTEGER,
	[user_id] INTEGER,
	[role] INTEGER,
	[email] NVARCHAR(200),
	PRIMARY KEY([author_id])
);
GO

CREATE TABLE [Department] (
	[department_id] INTEGER NOT NULL IDENTITY(1,1),
	[department_name] NVARCHAR(200),
	[project_id] INTEGER,
	[user_id] INTEGER,
	PRIMARY KEY([department_id])
);
GO

CREATE TABLE [Timeline] (
	[timeline_id] INTEGER NOT NULL IDENTITY(1,1),
	[start_date] DATETIME,
	[end_date] DATETIME,
	[created_by] INTEGER,
	[created_at] DATETIME,
	[timeline_type] INTEGER,
	PRIMARY KEY([timeline_id])
);
GO

CREATE TABLE [Invitations] (
	[invitation_id] INTEGER NOT NULL IDENTITY(1,1),
	[status] INTEGER,
	[content] NVARCHAR(MAX),
	[created_at] DATETIME,
	[group_id] INTEGER,
	[invited_user_id] INTEGER,
	[invited_by] INTEGER,
	[invited_role] INTEGER,
	[respond_date] DATETIME,
	PRIMARY KEY([invitation_id])
);
GO

-- Add foreign keys (corrected)

-- 1. Projects references Users
ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_Users 
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 2. Projects references Users
ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_Approver
FOREIGN KEY([approved_by]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 3. Projects references Groups
ALTER TABLE [Projects]
ADD CONSTRAINT FK_Projects_Groups
FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 4. Group_Member references Groups
ALTER TABLE [Group_Member]
ADD CONSTRAINT FK_GroupMember_Groups
FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 5. Group_Member references Users
ALTER TABLE [Group_Member]
ADD CONSTRAINT FK_GroupMember_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 6. Project resources references Projects
ALTER TABLE [Project_resources]
ADD CONSTRAINT FK_Resources_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 7. Milestone references Projects
ALTER TABLE [Milestone]
ADD CONSTRAINT FK_Milestone_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 8. Milestone references Users
ALTER TABLE [Milestone]
ADD CONSTRAINT FK_Milestone_Users
FOREIGN KEY([assign_to]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 9. Conference_expense references Conference
ALTER TABLE [Conference_expense]
ADD CONSTRAINT FK_Expense_Conference
FOREIGN KEY([conference_id]) REFERENCES [Conference]([conference_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 10. Quotas references Users
ALTER TABLE [Quotas]
ADD CONSTRAINT FK_Quotas_Users
FOREIGN KEY([allocated_by]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 11. Quotas references Projects
ALTER TABLE [Quotas]
ADD CONSTRAINT FK_Quotas_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 12. Notifications references Users
ALTER TABLE [Notifications]
ADD CONSTRAINT FK_Notifications_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 13. Notifications references Projects
ALTER TABLE [Notifications]
ADD CONSTRAINT FK_Notifications_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 14. Conference references Projects
ALTER TABLE [Conference]
ADD CONSTRAINT FK_Conference_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 15. Journal references Projects
ALTER TABLE [Journal]
ADD CONSTRAINT FK_Journal_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 16. Author references Users
ALTER TABLE [Author]
ADD CONSTRAINT FK_Author_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 17. Author references Projects
ALTER TABLE [Author]
ADD CONSTRAINT FK_Author_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 18. Department references Projects
ALTER TABLE [Department]
ADD CONSTRAINT FK_Department_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 20. Timeline references Users
ALTER TABLE [Timeline]
ADD CONSTRAINT FK_Timeline_Users
FOREIGN KEY([created_by]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 21. Documents references Projects
ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 22. Documents references Milestone
ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_Milestone
FOREIGN KEY([milestone_id]) REFERENCES [Milestone]([milestone_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 23. Documents references Users
ALTER TABLE [Documents]
ADD CONSTRAINT FK_Documents_Users
FOREIGN KEY([upload_by]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 24. Invitations references Groups
ALTER TABLE [Invitations]
ADD CONSTRAINT FK_Invitations_Groups
FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 25. Invitations references Users (người mời)
ALTER TABLE [Invitations]
ADD CONSTRAINT FK_Invitations_InvitedBy
FOREIGN KEY([invited_by]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 26. Invitations references Users (người được mời)
ALTER TABLE [Invitations]
ADD CONSTRAINT FK_Invitations_InvitedUser
FOREIGN KEY([invited_user_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 27. Notifications references Invitations
ALTER TABLE [Notifications]
ADD CONSTRAINT FK_Notifications_Invitations
FOREIGN KEY([invitation_id]) REFERENCES [Invitations]([invitation_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- 28. Category references Projects
ALTER TABLE [Category]
ADD CONSTRAINT FK_Category_Projects
FOREIGN KEY([project_id]) REFERENCES [Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- Dữ liệu mẫu cho bảng Users
INSERT INTO [Users] ([username], [password], [full_name], [email], [phone], [role], [department_id], [status], [created_at], [updated_at], [group_id], [level])
VALUES 
('user1', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Nguyễn Văn A', 'user1@example.com', '0123456789', 1, 1, 1, GETDATE(), GETDATE(), 1, 0),
('user2', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Trần Thị B', 'user2@example.com', '0123456788', 2, 1, 1, GETDATE(), GETDATE(), 1, 1),
('user3', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Lê Văn C', 'user3@example.com', '0123456787', 1, 2, 1, GETDATE(), GETDATE(), 2, 2),
('user4', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Phạm Thị D', 'user4@example.com', '0123456786', 2, 2, 1, GETDATE(), GETDATE(), 2, 3),
('user5', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Nguyễn Văn E', 'user5@example.com', '0123456785', 1, 1, 1, GETDATE(), GETDATE(), 1, 4),
('user6', '$2a$11$ioVSpEVBZDeUgJLi0399K.g7O5CYHzablfqJESjL7FW0SJuGrbyWC', 'Trần Thị F', 'user6@example.com', '0123456784', 2, 1, 1, GETDATE(), GETDATE(), 1, 2);

-- Dữ liệu mẫu cho bảng Groups
INSERT INTO [Groups] ([group_name], [max_member], [current_member], [status], [created_at], [group_type])
VALUES 
('Group A', 10, 5, 1, GETDATE(), 1),
('Group B', 8, 3, 1, GETDATE(), 1),
('Group C', 15, 10, 1, GETDATE(), 2);

-- Dữ liệu mẫu cho bảng Group_Member
INSERT INTO [Group_Member] ([group_id], [member_name], [member_email], [role], [user_id], [status], [join_date])
VALUES 
(1, 'Nguyễn Văn A', 'user1@example.com', 0, 1, 1, GETDATE()),
(1, 'Trần Thị B', 'user2@example.com', 1, 2, 1, GETDATE()),
(2, 'Lê Văn C', 'user3@example.com', 1, 3, 1, GETDATE()),
(2, 'Phạm Thị D', 'user4@example.com', 1, 4, 1, GETDATE()),
(3, 'Nguyễn Văn E', 'user5@example.com', 1, 5, 1, GETDATE()),
(3, 'Trần Thị F', 'user6@example.com', 1, 6, 1, GETDATE());

-- Dữ liệu mẫu cho bảng Projects
INSERT INTO [Projects] ([title], [description], [user_id], [start_date], [end_date], [status], [created_at], [updated_at], [group_id], [project_type], [approved_by], [objective], [methodology], [type], [approved_budget], [spent_budget])
VALUES 
('Project 1', 'Mô tả dự án 1', 1, GETDATE(), DATEADD(MONTH, 1, GETDATE()), 1, GETDATE(), GETDATE(), 1, 1, 1, 'Mục tiêu 1', 'Phương pháp 1', 0, 10000.00, 5000.00),
('Project 2', 'Mô tả dự án 2', 2, GETDATE(), DATEADD(MONTH, 2, GETDATE()), 1, GETDATE(), GETDATE(), 1, 1, 1, 'Mục tiêu 2', 'Phương pháp 2', 0, 20000.00, 10000.00),
('Project 3', 'Mô tả dự án 3', 3, GETDATE(), DATEADD(MONTH, 3, GETDATE()), 1, GETDATE(), GETDATE(), 2, 2, 1, 'Mục tiêu 3', 'Phương pháp 3', 1, 15000.00, 7000.00),
('Project 4', 'Mô tả dự án 4', 4, GETDATE(), DATEADD(MONTH, 4, GETDATE()), 1, GETDATE(), GETDATE(), 2, 2, 1, 'Mục tiêu 4', 'Phương pháp 4', 1, 25000.00, 12000.00),
('Project 5', 'Mô tả dự án 5', 5, GETDATE(), DATEADD(MONTH, 5, GETDATE()), 1, GETDATE(), GETDATE(), 1, 1, 1, 'Mục tiêu 5', 'Phương pháp 5', 2, 30000.00, 15000.00),
('Project 6', 'Mô tả dự án 6', 6, GETDATE(), DATEADD(MONTH, 6, GETDATE()), 1, GETDATE(), GETDATE(), 1, 1, 1, 'Mục tiêu 6', 'Phương pháp 6', 3, 18000.00, 9000.00);

-- Dữ liệu mẫu cho bảng Quotas
INSERT INTO [Quotas] ([quota_amount], [allocated_by], [allocated_at], [project_id], [limit_value], [current_value])
VALUES 
(100, 1, GETDATE(), 1, 5000.00, 2000.00),
(200, 2, GETDATE(), 2, 10000.00, 5000.00),
(150, 3, GETDATE(), 3, 7500.00, 3000.00),
(250, 4, GETDATE(), 4, 12500.00, 6000.00),
(300, 5, GETDATE(), 5, 15000.00, 8000.00),
(350, 6, GETDATE(), 6, 20000.00, 10000.00);

-- Dữ liệu mẫu cho bảng Milestone
INSERT INTO [Milestone] ([project_id], [title], [description], [start_date], [end_date], [status], [assign_to], [progress_percentage])
VALUES 
(1, 'Milestone 1', 'Mô tả milestone 1', GETDATE(), DATEADD(DAY, 10, GETDATE()), 1, 1, 50.00),
(2, 'Milestone 2', 'Mô tả milestone 2', GETDATE(), DATEADD(DAY, 20, GETDATE()), 1, 2, 30.00),
(3, 'Milestone 3', 'Mô tả milestone 3', GETDATE(), DATEADD(DAY, 30, GETDATE()), 1, 3, 70.00),
(4, 'Milestone 4', 'Mô tả milestone 4', GETDATE(), DATEADD(DAY, 40, GETDATE()), 1, 4, 20.00),
(5, 'Milestone 5', 'Mô tả milestone 5', GETDATE(), DATEADD(DAY, 50, GETDATE()), 1, 5, 90.00),
(6, 'Milestone 6', 'Mô tả milestone 6', GETDATE(), DATEADD(DAY, 60, GETDATE()), 1, 6, 10.00);

-- Dữ liệu mẫu cho bảng Documents
INSERT INTO [Documents] ([project_id], [milestone_id], [document_url], [file_name], [document_type], [upload_at], [upload_by])
VALUES 
(1, 1, 'http://example.com/doc1.pdf', 'Document 1', 1, GETDATE(), 1),
(2, 2, 'http://example.com/doc2.pdf', 'Document 2', 1, GETDATE(), 2),
(3, 3, 'http://example.com/doc3.pdf', 'Document 3', 1, GETDATE(), 3),
(4, 4, 'http://example.com/doc4.pdf', 'Document 4', 1, GETDATE(), 4),
(5, 5, 'http://example.com/doc5.pdf', 'Document 5', 1, GETDATE(), 5),
(6, 6, 'http://example.com/doc6.pdf', 'Document 6', 1, GETDATE(), 6);

-- Dữ liệu mẫu cho bảng Notifications
INSERT INTO [Notifications] ([user_id], [project_id], [title], [message], [status], [created_at], [invitation_id])
VALUES 
(1, 1, 'Thông báo 1', 'Nội dung thông báo 1', 1, GETDATE(), NULL),
(2, 2, 'Thông báo 2', 'Nội dung thông báo 2', 1, GETDATE(), NULL),
(3, 3, 'Thông báo 3', 'Nội dung thông báo 3', 1, GETDATE(), NULL),
(4, 4, 'Thông báo 4', 'Nội dung thông báo 4', 1, GETDATE(), NULL),
(5, 5, 'Thông báo 5', 'Nội dung thông báo 5', 1, GETDATE(), NULL),
(6, 6, 'Thông báo 6', 'Nội dung thông báo 6', 1, GETDATE(), NULL);

-- Dữ liệu mẫu cho bảng Category
INSERT INTO [Category] ([category_name], [project_id])
VALUES 
('Category 1', 1),
('Category 2', 1),
('Category 3', 2),
('Category 4', 2),
('Category 5', 3),
('Category 6', 3);

-- Dữ liệu mẫu cho bảng Project_resources
INSERT INTO [Project_resources] ([project_id], [resource_type], [quantity], [acquired], [estimated_cost])
VALUES 
(1, 1, 10, 1, 1000.00),
(2, 2, 20, 1, 2000.00),
(3, 3, 15, 1, 1500.00),
(4, 4, 25, 1, 2500.00),
(5, 5, 30, 1, 3000.00),
(6, 6, 5, 1, 500.00);

-- Dữ liệu mẫu cho bảng Conference
INSERT INTO [Conference] ([project_id], [conference_name], [conference_ranking], [location], [presentation_date], [acceptance_date], [conference_url], [presentation_type], [conference_proceedings])
VALUES 
(1, 'Conference 1', 1, 'Hà Nội', GETDATE(), GETDATE(), 'http://example.com/conference1', 1, 'Proceedings 1'),
(2, 'Conference 2', 2, 'Đà Nẵng', GETDATE(), GETDATE(), 'http://example.com/conference2', 1, 'Proceedings 2'),
(3, 'Conference 3', 3, 'TP.HCM', GETDATE(), GETDATE(), 'http://example.com/conference3', 1, 'Proceedings 3'),
(4, 'Conference 4', 1, 'Hải Phòng', GETDATE(), GETDATE(), 'http://example.com/conference4', 1, 'Proceedings 4'),
(5, 'Conference 5', 2, 'Nha Trang', GETDATE(), GETDATE(), 'http://example.com/conference5', 1, 'Proceedings 5'),
(6, 'Conference 6', 3, 'Cần Thơ', GETDATE(), GETDATE(), 'http://example.com/conference6', 1, 'Proceedings 6');

-- Dữ liệu mẫu cho bảng Journal
INSERT INTO [Journal] ([project_id], [publisher_approval], [doi_number], [pages], [submission_date], [acceptance_date], [publication_date], [journal_name], [reviewer_comments], [revision_history])
VALUES 
(1, 1, '10.1234/journal1', '1-10', GETDATE(), GETDATE(), GETDATE(), 'Journal 1', 'Comments 1', 'Revision 1'),
(2, 1, '10.1234/journal2', '11-20', GETDATE(), GETDATE(), GETDATE(), 'Journal 2', 'Comments 2', 'Revision 2'),
(3, 1, '10.1234/journal3', '21-30', GETDATE(), GETDATE(), GETDATE(), 'Journal 3', 'Comments 3', 'Revision 3'),
(4, 1, '10.1234/journal4', '31-40', GETDATE(), GETDATE(), GETDATE(), 'Journal 4', 'Comments 4', 'Revision 4'),
(5, 1, '10.1234/journal5', '41-50', GETDATE(), GETDATE(), GETDATE(), 'Journal 5', 'Comments 5', 'Revision 5'),
(6, 1, '10.1234/journal6', '51-60', GETDATE(), GETDATE(), GETDATE(), 'Journal 6', 'Comments 6', 'Revision 6');

-- Dữ liệu mẫu cho bảng Conference_expense
INSERT INTO [Conference_expense] ([conference_id], [travel_expense], [transportation_expense], [transportation], [accomodation])
VALUES 
(1, 100.00, 50.00, 'Taxi', 'Hotel A'),
(2, 200.00, 100.00, 'Bus', 'Hotel B'),
(3, 150.00, 75.00, 'Train', 'Hotel C'),
(4, 250.00, 125.00, 'Car', 'Hotel D'),
(5, 300.00, 150.00, 'Plane', 'Hotel E'),
(6, 350.00, 175.00, 'Bicycle', 'Hotel F');

-- Dữ liệu mẫu cho bảng Author
INSERT INTO [Author] ([project_id], [user_id], [role], [email])
VALUES 
(1, 1, 0, 'author1@example.com'),
(2, 2, 0, 'author2@example.com'),
(3, 3, 0, 'author3@example.com'),
(4, 4, 0, 'author4@example.com'),
(5, 5, 0, 'author5@example.com'),
(6, 6, 0, 'author6@example.com');

-- Dữ liệu mẫu cho bảng Department
INSERT INTO [Department] ([department_name], [project_id])
VALUES 
('Department 1', 1),
('Department 2', 2),
('Department 3', 3),
('Department 4', 4),
('Department 5', 5),
('Department 6', 6);

-- Dữ liệu mẫu cho bảng Timeline
INSERT INTO [Timeline] ([start_date], [end_date], [created_by], [created_at], [timeline_type])
VALUES 
(GETDATE(), DATEADD(DAY, 10, GETDATE()), 1, GETDATE(), 1),
(GETDATE(), DATEADD(DAY, 20, GETDATE()), 2, GETDATE(), 1),
(GETDATE(), DATEADD(DAY, 30, GETDATE()), 3, GETDATE(), 1),
(GETDATE(), DATEADD(DAY, 40, GETDATE()), 4, GETDATE(), 1),
(GETDATE(), DATEADD(DAY, 50, GETDATE()), 5, GETDATE(), 1),
(GETDATE(), DATEADD(DAY, 60, GETDATE()), 6, GETDATE(), 1);

-- Dữ liệu mẫu cho bảng Invitations
INSERT INTO [Invitations] ([status], [content], [created_at], [group_id], [invited_user_id], [invited_by], [invited_role], [respond_date])
VALUES 
(1, 'Invitation 1', GETDATE(), 1, 1, 1, 1, NULL),
(1, 'Invitation 2', GETDATE(), 1, 2, 1, 1, NULL),
(1, 'Invitation 3', GETDATE(), 2, 3, 1, 1, NULL),
(1, 'Invitation 4', GETDATE(), 2, 4, 1, 1, NULL),
(1, 'Invitation 5', GETDATE(), 3, 5, 1, 1, NULL),
(1, 'Invitation 6', GETDATE(), 3, 6, 1, 1, NULL);