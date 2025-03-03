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
	[role_id] INTEGER,
	[department_id] INTEGER,
	[status] INTEGER,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	[group_id] INTEGER,
	[level] NVARCHAR(50),
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
	[role] NVARCHAR(100),
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
	[type] NVARCHAR(100),
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
	[role] NVARCHAR(100),
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

-- 19. Department references Users
ALTER TABLE [Department]
ADD CONSTRAINT FK_Department_Users
FOREIGN KEY([user_id]) REFERENCES [Users]([user_id])
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