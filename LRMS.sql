CREATE TABLE [Users] (
	[user_id] INTEGER,
	[username] NVARCHAR,
	[password] NVARCHAR,
	[full_name] NVARCHAR,
	[email] NVARCHAR,
	[phone] NVARCHAR,
	[role_id] INTEGER,
	[department_id] INTEGER,
	[status] INTEGER,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	[group_id] INTEGER,
	PRIMARY KEY([user_id])
);
GO

CREATE TABLE [Departments] (
	[department_id] INTEGER,
	[name] NVARCHAR,
	[description] NVARCHAR,
	[head_id] INTEGER,
	[quota_count] INTEGER,
	[status] BIT,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	PRIMARY KEY([department_id])
);
GO

CREATE TABLE [Research_Projects] (
	[project_id] INTEGER,
	[title] NVARCHAR,
	[description] TEXT,
	[lecturer_id] INTEGER,
	[department_id] INTEGER,
	[budget] DECIMAL,
	[start_date] DATETIME,
	[end_date] DATETIME,
	[status] INTEGER,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	[paper_id] INTEGER,
	[document] VARBINARY,
	[group_id] INTEGER,
	PRIMARY KEY([project_id])
);
GO

CREATE TABLE [Research_Progress] (
	[research_progress_id] INTEGER,
	[project_id] INTEGER,
	[speciality] NVARCHAR,
	[description] TEXT,
	[percentage_complete] INTEGER,
	[attachment_url] NVARCHAR,
	[reported_by] INTEGER,
	[reported_at] DATETIME,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	PRIMARY KEY([research_progress_id])
);
GO

CREATE TABLE [Quotas] (
	[quota_id] INTEGER,
	[academic_year] NVARCHAR,
	[department_id] INTEGER,
	[quota_amount] INTEGER,
	[used_amount] INTEGER,
	[allocated_by] INTEGER,
	[allocated_at] DATETIME,
	[created_at] DATETIME,
	[project_id] INTEGER,
	[updated_at] DATETIME,
	[budget] INTEGER,
	PRIMARY KEY([quota_id])
);
GO

CREATE TABLE [Notifications] (
	[notification_id] INTEGER,
	[user_id] INTEGER,
	[project_id] INTEGER,
	[title] NVARCHAR,
	[content] TEXT,
	[is_read] BIT,
	[created_at] DATETIME,
	[request_id] INTEGER,
	PRIMARY KEY([notification_id])
);
GO

CREATE TABLE [Paper] (
	[paper_id] INTEGER NOT NULL IDENTITY UNIQUE,
	[department_id] INTEGER,
	[lecturer_id] INTEGER,
	[group_id] INTEGER,
	[budget] INTEGER,
	[document] VARBINARY,
	[type] DECIMAL,
	[volume] INTEGER,
	[number_pages] INTEGER,
	[title] VARCHAR,
	[location] VARCHAR,
	[time] DATETIME,
	[status] DECIMAL,
	[abstract] VARCHAR,
	[purpose] VARCHAR,
	[category_id] INTEGER,
	[milestone_title] NVARCHAR,
	[milestone_deadline] DATETIME,
	[project_dur_startdate] DATETIME,
	[project_enddate] DATETIME,
	PRIMARY KEY([paper_id])
);
GO

CREATE TABLE [Magazine] (
	[magazine_id] INTEGER NOT NULL IDENTITY UNIQUE,
	[volume] INTEGER,
	[number_pages] INTEGER,
	[years] DATETIME,
	[title] NVARCHAR,
	[lecturer_id] INTEGER,
	[paper_id] INTEGER,
	[abstract] NVARCHAR,
	[status] INTEGER,
	PRIMARY KEY([magazine_id])
);
GO

CREATE TABLE [Conference] (
	[conference_id] INTEGER NOT NULL IDENTITY UNIQUE,
	[location] NVARCHAR,
	[time] DATETIME,
	[paper_id] INTEGER,
	[status] INTEGER,
	[abstract] NVARCHAR,
	[methodology] NVARCHAR,
	[title] VARCHAR,
	[expected_outcome] NVARCHAR,
	[keyword] NVARCHAR,
	[type] DECIMAL,
	[journal] NVARCHAR,
	[transportation] DECIMAL,
	[accomodation] DECIMAL,
	[daparture_date] DATETIME,
	[return_date] DATETIME,
	[travel_cost] INTEGER,
	[note] NVARCHAR,
	PRIMARY KEY([conference_id])
);
GO

CREATE TABLE [Groups] (
	[group_id] INTEGER NOT NULL IDENTITY UNIQUE,
	[group_name] NVARCHAR,
	[max_member] INTEGER,
	[current_member] INTEGER,
	PRIMARY KEY([group_id])
);
GO

CREATE TABLE [Category] (
	[category_id] INTEGER NOT NULL IDENTITY UNIQUE,
	[category_name] NVARCHAR,
	[paper_id] INTEGER,
	[project_id] INTEGER,
	[department_id] INTEGER,
	PRIMARY KEY([category_id])
);
GO

CREATE TABLE [Request] (
	[request_id] INTEGER NOT NULL IDENTITY UNIQUE,
	[user_id] INTEGER,
	[status] INTEGER,
	[project_id] INTEGER,
	[paper_id] INTEGER,
	PRIMARY KEY([request_id])
);
GO

ALTER TABLE [Users]
ADD FOREIGN KEY([department_id]) REFERENCES [Departments]([department_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Users]
ADD FOREIGN KEY([user_id]) REFERENCES [Departments]([head_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Projects]
ADD FOREIGN KEY([lecturer_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Projects]
ADD FOREIGN KEY([department_id]) REFERENCES [Departments]([department_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Progress]
ADD FOREIGN KEY([project_id]) REFERENCES [Research_Projects]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Quotas]
ADD FOREIGN KEY([department_id]) REFERENCES [Departments]([department_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Notifications]
ADD FOREIGN KEY([user_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Progress]
ADD FOREIGN KEY([project_id]) REFERENCES [Notifications]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Quotas]
ADD FOREIGN KEY([project_id]) REFERENCES [Research_Progress]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Paper]
ADD FOREIGN KEY([paper_id]) REFERENCES [Magazine]([paper_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Paper]
ADD FOREIGN KEY([paper_id]) REFERENCES [Magazine]([years])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Users]
ADD FOREIGN KEY([group_id]) REFERENCES [Conference]([conference_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Paper]
ADD FOREIGN KEY([department_id]) REFERENCES [Departments]([department_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Paper]
ADD FOREIGN KEY([group_id]) REFERENCES [Conference]([conference_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Paper]
ADD FOREIGN KEY([lecturer_id]) REFERENCES [Users]([user_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Projects]
ADD FOREIGN KEY([paper_id]) REFERENCES [Paper]([paper_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Projects]
ADD FOREIGN KEY([project_id]) REFERENCES [Groups]([current_member])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Groups]
ADD FOREIGN KEY([max_member]) REFERENCES [Paper]([paper_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Users]
ADD FOREIGN KEY([user_id]) REFERENCES [Category]([category_name])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Projects]
ADD FOREIGN KEY([project_id]) REFERENCES [Category]([project_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Category]
ADD FOREIGN KEY([department_id]) REFERENCES [Paper]([paper_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Notifications]
ADD FOREIGN KEY([request_id]) REFERENCES [Category]([category_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Research_Projects]
ADD FOREIGN KEY([group_id]) REFERENCES [Groups]([group_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Category]
ADD FOREIGN KEY([department_id]) REFERENCES [Departments]([department_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO
ALTER TABLE [Paper]
ADD FOREIGN KEY([category_id]) REFERENCES [Category]([category_id])
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO