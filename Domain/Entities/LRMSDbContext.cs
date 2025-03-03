using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LRMS_API;

public partial class LRMSDbContext : DbContext
{
    public LRMSDbContext()
    {
    }

    public LRMSDbContext(DbContextOptions<LRMSDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Conference> Conferences { get; set; }

    public virtual DbSet<ConferenceExpense> ConferenceExpenses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<Journal> Journals { get; set; }

    public virtual DbSet<Milestone> Milestones { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectResource> ProjectResources { get; set; }

    public virtual DbSet<Quota> Quotas { get; set; }

    public virtual DbSet<Timeline> Timelines { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=(local);database=LRMSDB;uid=sa;pwd=12345;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Author__86516BCF8720C4AA");

            entity.ToTable("Author");

            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .HasColumnName("role");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Project).WithMany(p => p.Authors)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Author_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.Authors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Author_Users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__D54EE9B45C495105");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(200)
                .HasColumnName("category_name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.HasOne(d => d.Project).WithMany(p => p.Categories)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Category_Projects");
        });

        modelBuilder.Entity<Conference>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("PK__Conferen__DC9203083BF2142E");

            entity.ToTable("Conference");

            entity.Property(e => e.ConferenceId).HasColumnName("conference_id");
            entity.Property(e => e.AcceptanceDate)
                .HasColumnType("datetime")
                .HasColumnName("acceptance_date");
            entity.Property(e => e.ConferenceName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("conference_name");
            entity.Property(e => e.ConferenceProceedings)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("conference_proceedings");
            entity.Property(e => e.ConferenceRanking).HasColumnName("conference_ranking");
            entity.Property(e => e.ConferenceUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("conference_url");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("location");
            entity.Property(e => e.PresentationDate)
                .HasColumnType("datetime")
                .HasColumnName("presentation_date");
            entity.Property(e => e.PresentationType).HasColumnName("presentation_type");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.HasOne(d => d.Project).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Conference_Projects");
        });

        modelBuilder.Entity<ConferenceExpense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Conferen__404B6A6B1B43C464");

            entity.ToTable("Conference_expense");

            entity.Property(e => e.ExpenseId).HasColumnName("expense_id");
            entity.Property(e => e.Accomodation)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("accomodation");
            entity.Property(e => e.ConferenceId).HasColumnName("conference_id");
            entity.Property(e => e.Transportation)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("transportation");
            entity.Property(e => e.TransportationExpense)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("transportation_expense");
            entity.Property(e => e.TravelExpense)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("travel_expense");

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceExpenses)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_Expense_Conference");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C22324227A1FB6EF");

            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(200)
                .HasColumnName("department_name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Project).WithMany(p => p.Departments)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Department_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.Departments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Department_Users");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__9666E8ACC4E1E9EC");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.DocumentUrl)
                .HasMaxLength(500)
                .HasColumnName("document_url");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.MilestoneId).HasColumnName("milestone_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.UploadAt)
                .HasColumnType("datetime")
                .HasColumnName("upload_at");
            entity.Property(e => e.UploadBy).HasColumnName("upload_by");

            entity.HasOne(d => d.Milestone).WithMany(p => p.Documents)
                .HasForeignKey(d => d.MilestoneId)
                .HasConstraintName("FK_Documents_Milestone");

            entity.HasOne(d => d.Project).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Documents_Projects");

            entity.HasOne(d => d.UploadByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploadBy)
                .HasConstraintName("FK_Documents_Users");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__D57795A08B306820");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentMember).HasColumnName("current_member");
            entity.Property(e => e.GroupName)
                .HasMaxLength(200)
                .HasColumnName("group_name");
            entity.Property(e => e.GroupType).HasColumnName("group_type");
            entity.Property(e => e.MaxMember).HasColumnName("max_member");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId).HasName("PK__Group_Me__F3C66B8C5D072371");

            entity.ToTable("Group_Member");

            entity.Property(e => e.GroupMemberId).HasColumnName("group_member_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.JoinDate)
                .HasColumnType("datetime")
                .HasColumnName("join_date");
            entity.Property(e => e.MemberEmail)
                .HasMaxLength(200)
                .HasColumnName("member_email");
            entity.Property(e => e.MemberName)
                .HasMaxLength(200)
                .HasColumnName("member_name");
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .HasColumnName("role");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_GroupMember_Groups");

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_GroupMember_Users");
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PK__Invitati__94B74D7C682CB14D");

            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.InvitedBy).HasColumnName("invited_by");
            entity.Property(e => e.InvitedRole).HasColumnName("invited_role");
            entity.Property(e => e.InvitedUserId).HasColumnName("invited_user_id");
            entity.Property(e => e.RespondDate)
                .HasColumnType("datetime")
                .HasColumnName("respond_date");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Group).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Invitations_Groups");

            entity.HasOne(d => d.InvitedByNavigation).WithMany(p => p.InvitationInvitedByNavigations)
                .HasForeignKey(d => d.InvitedBy)
                .HasConstraintName("FK_Invitations_InvitedBy");

            entity.HasOne(d => d.InvitedUser).WithMany(p => p.InvitationInvitedUsers)
                .HasForeignKey(d => d.InvitedUserId)
                .HasConstraintName("FK_Invitations_InvitedUser");
        });

        modelBuilder.Entity<Journal>(entity =>
        {
            entity.HasKey(e => e.JournalId).HasName("PK__Journal__9894D298CBAEA3E4");

            entity.ToTable("Journal");

            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.AcceptanceDate)
                .HasColumnType("datetime")
                .HasColumnName("acceptance_date");
            entity.Property(e => e.DoiNumber)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("doi_number");
            entity.Property(e => e.JournalName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("journal_name");
            entity.Property(e => e.Pages)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("pages");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.PublicationDate)
                .HasColumnType("datetime")
                .HasColumnName("publication_date");
            entity.Property(e => e.PublisherApproval).HasColumnName("publisher_approval");
            entity.Property(e => e.ReviewerComments).HasColumnName("reviewer_comments");
            entity.Property(e => e.RevisionHistory).HasColumnName("revision_history");
            entity.Property(e => e.SubmissionDate)
                .HasColumnType("datetime")
                .HasColumnName("submission_date");

            entity.HasOne(d => d.Project).WithMany(p => p.Journals)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Journal_Projects");
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.HasKey(e => e.MilestoneId).HasName("PK__Mileston__67592EB75043504B");

            entity.ToTable("Milestone");

            entity.Property(e => e.MilestoneId).HasColumnName("milestone_id");
            entity.Property(e => e.AssignTo).HasColumnName("assign_to");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ProgressPercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress_percentage");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.AssignToNavigation).WithMany(p => p.Milestones)
                .HasForeignKey(d => d.AssignTo)
                .HasConstraintName("FK_Milestone_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.Milestones)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Milestone_Projects");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F09DA854D");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Invitation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.InvitationId)
                .HasConstraintName("FK_Notifications_Invitations");

            entity.HasOne(d => d.Project).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Notifications_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__BC799E1F7C43913B");

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ApprovedBudget)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("approved_budget");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Methodology).HasColumnName("methodology");
            entity.Property(e => e.Objective).HasColumnName("objective");
            entity.Property(e => e.ProjectType).HasColumnName("project_type");
            entity.Property(e => e.SpentBudget)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("spent_budget");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ProjectApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_Projects_Approver");

            entity.HasOne(d => d.Group).WithMany(p => p.Projects)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Projects_Groups");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Projects_Users");
        });

        modelBuilder.Entity<ProjectResource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__Project___4985FC73F1CB07CD");

            entity.ToTable("Project_resources");

            entity.Property(e => e.ResourceId).HasColumnName("resource_id");
            entity.Property(e => e.Acquired).HasColumnName("acquired");
            entity.Property(e => e.EstimatedCost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("estimated_cost");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ResourceType).HasColumnName("resource_type");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectResources)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Resources_Projects");
        });

        modelBuilder.Entity<Quota>(entity =>
        {
            entity.HasKey(e => e.QuotaId).HasName("PK__Quotas__FF9A8B255E79CE1A");

            entity.Property(e => e.QuotaId).HasColumnName("quota_id");
            entity.Property(e => e.AllocatedAt)
                .HasColumnType("datetime")
                .HasColumnName("allocated_at");
            entity.Property(e => e.AllocatedBy).HasColumnName("allocated_by");
            entity.Property(e => e.CurrentValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("current_value");
            entity.Property(e => e.LimitValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("limit_value");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.QuotaAmount).HasColumnName("quota_amount");

            entity.HasOne(d => d.AllocatedByNavigation).WithMany(p => p.Quota)
                .HasForeignKey(d => d.AllocatedBy)
                .HasConstraintName("FK_Quotas_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.Quota)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Quotas_Projects");
        });

        modelBuilder.Entity<Timeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId).HasName("PK__Timeline__DC6F55B00B621F68");

            entity.ToTable("Timeline");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.TimelineType).HasColumnName("timeline_type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Timelines)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Timeline_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F44D8C626");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasColumnName("level");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
