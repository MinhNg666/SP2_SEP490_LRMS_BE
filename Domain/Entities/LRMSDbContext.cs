using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LRMS_API;

public partial class LRMSDbContext : DbContext
{
    public LRMSDbContext()
    {
    }

    public LRMSDbContext(DbContextOptions<LRMSDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Conference> Conferences { get; set; }

    public virtual DbSet<ConferenceExpense> ConferenceExpenses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<FundDisbursement> FundDisbursements { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<Journal> Journals { get; set; }

    public virtual DbSet<ProjectPhase> ProjectPhases { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectResource> ProjectResources { get; set; }

    public virtual DbSet<Quota> Quotas { get; set; }

    public virtual DbSet<Timeline> Timelines { get; set; }

    public virtual DbSet<TimelineSequence> TimelineSequence { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ProjectRequest> ProjectRequests { get; set; }

    public virtual DbSet<CompletionRequestDetail> CompletionRequestDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Author__86516BCFD3E29AFB");

            entity.ToTable("Author");

            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Project).WithMany(p => p.Authors)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Author_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.Authors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Author_Users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__D54EE9B45213372A");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(200)
                .HasColumnName("category_name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Project).WithMany(p => p.Categories)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Category_Projects");
        });

        modelBuilder.Entity<Conference>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("PK__Conferen__DC92030881F39E56");

            entity.ToTable("Conference");

            entity.Property(e => e.ConferenceId).HasColumnName("conference_id");
            entity.Property(e => e.AcceptanceDate)
                .HasColumnType("datetime")
                .HasColumnName("acceptance_date");
            entity.Property(e => e.ConferenceName)
                .HasMaxLength(255)
                .HasColumnName("conference_name");
            entity.Property(e => e.ConferenceRanking).HasColumnName("conference_ranking");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
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
            entity.HasKey(e => e.ExpenseId).HasName("PK__Conferen__404B6A6BE00ED890");

            entity.ToTable("Conference_expense");

            entity.Property(e => e.ExpenseId).HasColumnName("expense_id");
            entity.Property(e => e.Accomodation)
                .HasMaxLength(255)
                .HasColumnName("accomodation");
            entity.Property(e => e.AccomodationExpense)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("accomodation_expense");
            entity.Property(e => e.ConferenceId).HasColumnName("conference_id");
            entity.Property(e => e.Travel)
                .HasMaxLength(255)
                .HasColumnName("travel");
            entity.Property(e => e.TravelExpense)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("travel_expense");

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceExpenses)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ConferenceExpense_Conference");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C2232422ADB9B56B");

            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(200)
                .HasColumnName("department_name");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__9666E8ACE9AEE755");

            entity.HasIndex(e => e.DocumentType, "IDX_Documents_Type");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.ConferenceExpenseId).HasColumnName("conference_expense_id");
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.DocumentUrl)
                .HasMaxLength(500)
                .HasColumnName("document_url");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FundDisbursementId).HasColumnName("fund_disbursement_id");
            entity.Property(e => e.ProjectPhaseId).HasColumnName("project_phase_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ProjectResourceId).HasColumnName("project_resource_id");
            entity.Property(e => e.UploadAt)
                .HasColumnType("datetime")
                .HasColumnName("upload_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");

            entity.HasOne(d => d.ConferenceExpense).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ConferenceExpenseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documents_ConferenceExpense");

            entity.HasOne(d => d.FundDisbursement).WithMany(p => p.Documents)
                .HasForeignKey(d => d.FundDisbursementId)
                .HasConstraintName("FK_Documents_FundDisbursement");

            entity.HasOne(d => d.ProjectPhase).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectPhaseId)
                .HasConstraintName("FK_Documents_ProjectPhase");

            entity.HasOne(d => d.Project).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Documents_Projects");

            entity.HasOne(d => d.ProjectResource).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documents_ProjectResources");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploadedBy)
                .HasConstraintName("FK_Documents_Users");
        });

        modelBuilder.Entity<FundDisbursement>(entity =>
        {
            entity.HasKey(e => e.FundDisbursementId).HasName("PK__Fund_Dis__7FED8C5489F7F33D");

            entity.ToTable("Fund_Disbursement");

            entity.Property(e => e.FundDisbursementId).HasColumnName("fund_disbursement_id");
            entity.Property(e => e.UserRequest).HasColumnName("user_request");
            entity.Property(e => e.AppovedBy).HasColumnName("appoved_by");
            entity.Property(e => e.DisburseBy).HasColumnName("disburse_by");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.QuotaId).HasColumnName("quota_id");
            entity.Property(e => e.ProjectPhaseId).HasColumnName("project_phase_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FundRequest)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("fund_request");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");

            entity.HasOne(d => d.AppovedByNavigation)
                .WithMany(p => p.FundDisbursementAppovedByNavigations)
                .HasForeignKey(d => d.AppovedBy)
                .HasConstraintName("FK_FundDisbursement_GroupMember_Approved");

            entity.HasOne(d => d.UserRequestNavigation)
                .WithMany(p => p.FundDisbursementsAsRequester)
                .HasForeignKey(d => d.UserRequest)
                .HasConstraintName("FK_FundDisbursement_User_Request");

            entity.HasOne(d => d.DisburseByNavigation).WithMany(p => p.FundDisbursements)
                .HasForeignKey(d => d.DisburseBy)
                .HasConstraintName("FK_FundDisbursement_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.FundDisbursements)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_FundDisbursement_Projects");

            entity.HasOne(d => d.Quota)
                .WithMany(p => p.FundDisbursements)
                .HasForeignKey(d => d.QuotaId)
                .HasConstraintName("FK_FundDisbursement_Quota");

            entity.HasOne(d => d.ProjectPhase)
                .WithMany(p => p.FundDisbursements)
                .HasForeignKey(d => d.ProjectPhaseId)
                .HasConstraintName("FK_FundDisbursement_ProjectPhase");

            entity.Ignore("AuthorId");
            entity.Ignore("GroupMemberId");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__D57795A03A6C1236");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CurrentMember).HasColumnName("current_member");
            entity.Property(e => e.GroupDepartment).HasColumnName("group_department");
            entity.Property(e => e.GroupName)
                .HasMaxLength(200)
                .HasColumnName("group_name");
            entity.Property(e => e.GroupType).HasColumnName("group_type");
            entity.Property(e => e.MaxMember).HasColumnName("max_member");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Groups_Users");

            entity.HasOne(d => d.GroupDepartmentNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.GroupDepartment)
                .HasConstraintName("FK_Groups_Department");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId).HasName("PK__Group_Me__F3C66B8CE62B0790");

            entity.ToTable("Group_Member");

            entity.Property(e => e.GroupMemberId).HasColumnName("group_member_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.JoinDate)
                .HasColumnType("datetime")
                .HasColumnName("join_date");
            entity.Property(e => e.Role).HasColumnName("role");
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
            entity.HasKey(e => e.InvitationId).HasName("PK__Invitati__94B74D7C436FE83E");

            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.InvitedRole).HasColumnName("invited_role");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.RecieveBy).HasColumnName("recieve_by");
            entity.Property(e => e.RespondDate)
                .HasColumnType("datetime")
                .HasColumnName("respond_date");
            entity.Property(e => e.SentBy).HasColumnName("sent_by");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Group).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Invitations_Groups");

            entity.HasOne(d => d.RecieveByNavigation).WithMany(p => p.InvitationRecieveByNavigations)
                .HasForeignKey(d => d.RecieveBy)
                .HasConstraintName("FK_Invitations_Users_Receive");

            entity.HasOne(d => d.SentByNavigation).WithMany(p => p.InvitationSentByNavigations)
                .HasForeignKey(d => d.SentBy)
                .HasConstraintName("FK_Invitations_Users_Sent");
        });

        modelBuilder.Entity<Journal>(entity =>
        {
            entity.HasKey(e => e.JournalId).HasName("PK__Journal__9894D298AFDA5EC3");

            entity.ToTable("Journal");

            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.AcceptanceDate)
                .HasColumnType("datetime")
                .HasColumnName("acceptance_date");
            entity.Property(e => e.DoiNumber)
                .HasMaxLength(100)
                .HasColumnName("doi_number");
            entity.Property(e => e.JournalName)
                .HasMaxLength(255)
                .HasColumnName("journal_name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.PublicationDate)
                .HasColumnType("datetime")
                .HasColumnName("publication_date");
            entity.Property(e => e.PublisherName)
                .HasMaxLength(255)
                .HasColumnName("publisher_name");
            entity.Property(e => e.PublisherStatus).HasColumnName("publisher_status");
            entity.Property(e => e.ReviewerComments).HasColumnName("reviewer_comments");
            entity.Property(e => e.SubmissionDate)
                .HasColumnType("datetime")
                .HasColumnName("submission_date");

            entity.HasOne(d => d.Project).WithMany(p => p.Journals)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Journal_Projects");
        });

        modelBuilder.Entity<ProjectPhase>(entity =>
        {
            entity.HasKey(e => e.ProjectPhaseId).HasName("PK__ProjectP__67592EB79C6F9174");

            entity.ToTable("ProjectPhase");

            entity.Property(e => e.ProjectPhaseId).HasColumnName("project_phase_id");
            entity.Property(e => e.AssignBy).HasColumnName("assign_by");
            entity.Property(e => e.AssignTo).HasColumnName("assign_to");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.SpentBudget)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("spent_budget")
                .HasDefaultValue(0);

            entity.HasOne(d => d.AssignByNavigation).WithMany(p => p.ProjectPhaseAssignByNavigations)
                .HasForeignKey(d => d.AssignBy)
                .HasConstraintName("FK_ProjectPhase_GroupMember_AssignBy");

            entity.HasOne(d => d.AssignToNavigation).WithMany(p => p.ProjectPhaseAssignToNavigations)
                .HasForeignKey(d => d.AssignTo)
                .HasConstraintName("FK_ProjectPhase_GroupMember_AssignTo");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectPhases)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_ProjectPhase_Projects");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FD5E27EE0");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
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
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__BC799E1FB00E4B70");

            entity.HasIndex(e => e.Status, "IDX_Projects_Status");

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ApprovedBudget)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("approved_budget");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Methodlogy).HasColumnName("methodlogy");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("project_name");
            entity.Property(e => e.ProjectType).HasColumnName("project_type");
            entity.Property(e => e.SpentBudget)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("spent_budget");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.SequenceId).HasColumnName("sequence_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_Projects_GroupMember_Approved");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Projects_Users_Created");

            entity.HasOne(d => d.Department).WithMany(p => p.Projects)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Projects_Department");

            entity.HasOne(d => d.Group).WithMany(p => p.Projects)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Projects_Groups");

            entity.HasOne(d => d.Sequence)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.SequenceId)
                .HasConstraintName("FK_Project_TimelineSequence");
        });

        modelBuilder.Entity<ProjectResource>(entity =>
        {
            entity.HasKey(e => e.ProjectResourceId).HasName("PK__Project___A2029D37E43DB5DB");

            entity.ToTable("Project_resources");

            entity.Property(e => e.ProjectResourceId).HasColumnName("project_resource_id");
            entity.Property(e => e.Acquired).HasColumnName("acquired");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ResourceName)
                .HasMaxLength(200)
                .HasColumnName("resource_name");
            entity.Property(e => e.ResourceType).HasColumnName("resource_type");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectResources)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_ProjectResources_Projects");
        });

        modelBuilder.Entity<Quota>(entity =>
        {
            entity.HasKey(e => e.QuotaId).HasName("PK__Quotas__FF9A8B250935C34D");

            entity.Property(e => e.QuotaId).HasColumnName("quota_id");
            entity.Property(e => e.AllocatedBudget)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("allocated_budget");
            entity.Property(e => e.AllocatedBy).HasColumnName("allocated_by");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");

            entity.HasOne(d => d.AllocatedByNavigation).WithMany(p => p.Quota)
                .HasForeignKey(d => d.AllocatedBy)
                .HasConstraintName("FK_Quotas_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.Quota)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Quotas_Projects");
        });

        modelBuilder.Entity<Timeline>(entity =>
        {
            entity.HasKey(e => e.TimelineId);

            entity.ToTable("Timeline", "dbo");

            entity.Property(e => e.TimelineId).HasColumnName("timeline_id");
            entity.Property(e => e.SequenceId).HasColumnName("sequence_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnName("end_date").HasColumnType("datetime");
            entity.Property(e => e.Event).HasColumnName("event").HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("datetime");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("datetime");
            entity.Property(e => e.TimelineType).HasColumnName("timeline_type");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasOne(d => d.Sequence)
                .WithMany(p => p.Timelines)
                .HasForeignKey(d => d.SequenceId)
                .HasConstraintName("FK_Timeline_TimelineSequence");

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Timelines)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Timeline_Users");
        });

        modelBuilder.Entity<TimelineSequence>(entity =>
        {
            entity.HasKey(e => e.SequenceId);
            
            entity.ToTable("TimelineSequence", "dbo");

            entity.Property(e => e.SequenceId).HasColumnName("sequence_id");
            entity.Property(e => e.SequenceName).HasColumnName("sequence_name").HasMaxLength(255);
            entity.Property(e => e.SequenceDescription).HasColumnName("sequence_description").HasColumnType("nvarchar(MAX)");
            entity.Property(e => e.SequenceColor).HasColumnName("sequence_color").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_TimelineSequence_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FCE651109");

            entity.HasIndex(e => e.Role, "IDX_Users_Role");

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
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Users_Department");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
