USE LRMSDB;

/****** Object:  Index [IX_CompletionRequestDetails_request_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CompletionRequestDetails_request_id] ON [dbo].[CompletionRequestDetails]
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IDX_Documents_Type]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Documents_Type] ON [dbo].[Documents]
(
	[document_type] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Documents_conference_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Documents_conference_id] ON [dbo].[Documents]
(
	[conference_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Documents_journal_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Documents_journal_id] ON [dbo].[Documents]
(
	[journal_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Documents_request_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Documents_request_id] ON [dbo].[Documents]
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Fund_Disbursement_ConferenceId]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Fund_Disbursement_ConferenceId] ON [dbo].[Fund_Disbursement]
(
	[ConferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Fund_Disbursement_ExpenseId]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Fund_Disbursement_ExpenseId] ON [dbo].[Fund_Disbursement]
(
	[ExpenseId] ASC
)
WHERE ([ExpenseId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Fund_Disbursement_JournalId]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Fund_Disbursement_JournalId] ON [dbo].[Fund_Disbursement]
(
	[JournalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_approved_by]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_approved_by] ON [dbo].[ProjectRequests]
(
	[approved_by] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_assigned_council]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_assigned_council] ON [dbo].[ProjectRequests]
(
	[assigned_council] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_fund_disbursement_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_fund_disbursement_id] ON [dbo].[ProjectRequests]
(
	[fund_disbursement_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_phase_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_phase_id] ON [dbo].[ProjectRequests]
(
	[phase_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_project_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_project_id] ON [dbo].[ProjectRequests]
(
	[project_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_requested_by]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_requested_by] ON [dbo].[ProjectRequests]
(
	[requested_by] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProjectRequests_timeline_id]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProjectRequests_timeline_id] ON [dbo].[ProjectRequests]
(
	[timeline_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IDX_Projects_Status]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Projects_Status] ON [dbo].[Projects]
(
	[status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IDX_Users_Role]    Script Date: 5/8/2025 11:09:33 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Users_Role] ON [dbo].[Users]
(
	[role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CompletionRequestDetails] ADD  DEFAULT ((0)) FOR [budget_reconciled]
GO
ALTER TABLE [dbo].[ProjectPhase] ADD  DEFAULT ((0)) FOR [spent_budget]
GO
ALTER TABLE [dbo].[ProjectRequests] ADD  DEFAULT (getdate()) FOR [requested_at]
GO
ALTER TABLE [dbo].[Timeline] ADD  DEFAULT ((0)) FOR [status]
GO
ALTER TABLE [dbo].[TimelineSequence] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[TimelineSequence] ADD  DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[Author]  WITH NOCHECK ADD  CONSTRAINT [FK_Author_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Author] CHECK CONSTRAINT [FK_Author_Projects]
GO
ALTER TABLE [dbo].[Author]  WITH NOCHECK ADD  CONSTRAINT [FK_Author_Users] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Author] CHECK CONSTRAINT [FK_Author_Users]
GO
ALTER TABLE [dbo].[Category]  WITH NOCHECK ADD  CONSTRAINT [FK_Category_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_Category_Projects]
GO
ALTER TABLE [dbo].[CompletionRequestDetails]  WITH CHECK ADD  CONSTRAINT [FK_CompletionRequestDetails_ProjectRequests] FOREIGN KEY([request_id])
REFERENCES [dbo].[ProjectRequests] ([request_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CompletionRequestDetails] CHECK CONSTRAINT [FK_CompletionRequestDetails_ProjectRequests]
GO
ALTER TABLE [dbo].[Conference]  WITH NOCHECK ADD  CONSTRAINT [FK_Conference_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Conference] CHECK CONSTRAINT [FK_Conference_Projects]
GO
ALTER TABLE [dbo].[Conference_expense]  WITH NOCHECK ADD  CONSTRAINT [FK_ConferenceExpense_Conference] FOREIGN KEY([conference_id])
REFERENCES [dbo].[Conference] ([conference_id])
GO
ALTER TABLE [dbo].[Conference_expense] CHECK CONSTRAINT [FK_ConferenceExpense_Conference]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_Conference] FOREIGN KEY([conference_id])
REFERENCES [dbo].[Conference] ([conference_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_Conference]
GO
ALTER TABLE [dbo].[Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_Documents_ConferenceExpense] FOREIGN KEY([conference_expense_id])
REFERENCES [dbo].[Conference_expense] ([expense_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_ConferenceExpense]
GO
ALTER TABLE [dbo].[Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_Documents_FundDisbursement] FOREIGN KEY([fund_disbursement_id])
REFERENCES [dbo].[Fund_Disbursement] ([fund_disbursement_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_FundDisbursement]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_Journal] FOREIGN KEY([journal_id])
REFERENCES [dbo].[Journal] ([journal_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_Journal]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_ProjectPhase] FOREIGN KEY([project_phase_id])
REFERENCES [dbo].[ProjectPhase] ([project_phase_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_ProjectPhase]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_ProjectRequests] FOREIGN KEY([request_id])
REFERENCES [dbo].[ProjectRequests] ([request_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_ProjectRequests]
GO
ALTER TABLE [dbo].[Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_Documents_ProjectResources] FOREIGN KEY([project_resource_id])
REFERENCES [dbo].[Project_resources] ([project_resource_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_ProjectResources]
GO
ALTER TABLE [dbo].[Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_Documents_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_Projects]
GO
ALTER TABLE [dbo].[Documents]  WITH NOCHECK ADD  CONSTRAINT [FK_Documents_Users] FOREIGN KEY([uploaded_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_Users]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [FK_Fund_Disbursement_Conference_ConferenceId] FOREIGN KEY([ConferenceId])
REFERENCES [dbo].[Conference] ([conference_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_Fund_Disbursement_Conference_ConferenceId]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [FK_Fund_Disbursement_Journal_JournalId] FOREIGN KEY([JournalId])
REFERENCES [dbo].[Journal] ([journal_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_Fund_Disbursement_Journal_JournalId]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [FK_Fund_Disbursement_User] FOREIGN KEY([user_request])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_Fund_Disbursement_User]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [FK_FundDisbursement_ConferenceExpense] FOREIGN KEY([ExpenseId])
REFERENCES [dbo].[Conference_expense] ([expense_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_FundDisbursement_ConferenceExpense]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH NOCHECK ADD  CONSTRAINT [FK_FundDisbursement_GroupMember_Approved] FOREIGN KEY([appoved_by])
REFERENCES [dbo].[Group_Member] ([group_member_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_FundDisbursement_GroupMember_Approved]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [FK_FundDisbursement_ProjectPhase] FOREIGN KEY([project_phase_id])
REFERENCES [dbo].[ProjectPhase] ([project_phase_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_FundDisbursement_ProjectPhase]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH NOCHECK ADD  CONSTRAINT [FK_FundDisbursement_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_FundDisbursement_Projects]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [FK_FundDisbursement_Quota] FOREIGN KEY([quota_id])
REFERENCES [dbo].[Quotas] ([quota_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_FundDisbursement_Quota]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH NOCHECK ADD  CONSTRAINT [FK_FundDisbursement_Users] FOREIGN KEY([disburse_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [FK_FundDisbursement_Users]
GO
ALTER TABLE [dbo].[Group_Member]  WITH NOCHECK ADD  CONSTRAINT [FK_GroupMember_Groups] FOREIGN KEY([group_id])
REFERENCES [dbo].[Groups] ([group_id])
GO
ALTER TABLE [dbo].[Group_Member] CHECK CONSTRAINT [FK_GroupMember_Groups]
GO
ALTER TABLE [dbo].[Group_Member]  WITH NOCHECK ADD  CONSTRAINT [FK_GroupMember_Users] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Group_Member] CHECK CONSTRAINT [FK_GroupMember_Users]
GO
ALTER TABLE [dbo].[Groups]  WITH NOCHECK ADD  CONSTRAINT [FK_Groups_Department] FOREIGN KEY([group_department])
REFERENCES [dbo].[Department] ([department_id])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK_Groups_Department]
GO
ALTER TABLE [dbo].[Groups]  WITH NOCHECK ADD  CONSTRAINT [FK_Groups_Users] FOREIGN KEY([created_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK_Groups_Users]
GO
ALTER TABLE [dbo].[Invitations]  WITH NOCHECK ADD  CONSTRAINT [FK_Invitations_Groups] FOREIGN KEY([group_id])
REFERENCES [dbo].[Groups] ([group_id])
GO
ALTER TABLE [dbo].[Invitations] CHECK CONSTRAINT [FK_Invitations_Groups]
GO
ALTER TABLE [dbo].[Invitations]  WITH NOCHECK ADD  CONSTRAINT [FK_Invitations_Users_Receive] FOREIGN KEY([recieve_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Invitations] CHECK CONSTRAINT [FK_Invitations_Users_Receive]
GO
ALTER TABLE [dbo].[Invitations]  WITH NOCHECK ADD  CONSTRAINT [FK_Invitations_Users_Sent] FOREIGN KEY([sent_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Invitations] CHECK CONSTRAINT [FK_Invitations_Users_Sent]
GO
ALTER TABLE [dbo].[Journal]  WITH NOCHECK ADD  CONSTRAINT [FK_Journal_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Journal] CHECK CONSTRAINT [FK_Journal_Projects]
GO
ALTER TABLE [dbo].[Notifications]  WITH NOCHECK ADD  CONSTRAINT [FK_Notifications_Invitations] FOREIGN KEY([invitation_id])
REFERENCES [dbo].[Invitations] ([invitation_id])
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Invitations]
GO
ALTER TABLE [dbo].[Notifications]  WITH NOCHECK ADD  CONSTRAINT [FK_Notifications_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Projects]
GO
ALTER TABLE [dbo].[Notifications]  WITH NOCHECK ADD  CONSTRAINT [FK_Notifications_Users] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users]
GO
ALTER TABLE [dbo].[Project_resources]  WITH NOCHECK ADD  CONSTRAINT [FK_ProjectResources_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Project_resources] CHECK CONSTRAINT [FK_ProjectResources_Projects]
GO
ALTER TABLE [dbo].[ProjectPhase]  WITH NOCHECK ADD  CONSTRAINT [FK_Milestone_GroupMember_AssignBy] FOREIGN KEY([assign_by])
REFERENCES [dbo].[Group_Member] ([group_member_id])
GO
ALTER TABLE [dbo].[ProjectPhase] CHECK CONSTRAINT [FK_Milestone_GroupMember_AssignBy]
GO
ALTER TABLE [dbo].[ProjectPhase]  WITH NOCHECK ADD  CONSTRAINT [FK_Milestone_GroupMember_AssignTo] FOREIGN KEY([assign_to])
REFERENCES [dbo].[Group_Member] ([group_member_id])
GO
ALTER TABLE [dbo].[ProjectPhase] CHECK CONSTRAINT [FK_Milestone_GroupMember_AssignTo]
GO
ALTER TABLE [dbo].[ProjectPhase]  WITH NOCHECK ADD  CONSTRAINT [FK_Milestone_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[ProjectPhase] CHECK CONSTRAINT [FK_Milestone_Projects]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_Fund_Disbursement_fund_disbursement_id] FOREIGN KEY([fund_disbursement_id])
REFERENCES [dbo].[Fund_Disbursement] ([fund_disbursement_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_Fund_Disbursement_fund_disbursement_id]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_Groups_AssignedCouncil] FOREIGN KEY([assigned_council])
REFERENCES [dbo].[Groups] ([group_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_Groups_AssignedCouncil]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_ProjectPhase] FOREIGN KEY([phase_id])
REFERENCES [dbo].[ProjectPhase] ([project_phase_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_ProjectPhase]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_Projects]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_Timeline] FOREIGN KEY([timeline_id])
REFERENCES [dbo].[Timeline] ([timeline_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_Timeline]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_Users_ApprovedBy] FOREIGN KEY([approved_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_Users_ApprovedBy]
GO
ALTER TABLE [dbo].[ProjectRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProjectRequests_Users_RequestedBy] FOREIGN KEY([requested_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[ProjectRequests] CHECK CONSTRAINT [FK_ProjectRequests_Users_RequestedBy]
GO
ALTER TABLE [dbo].[Projects]  WITH NOCHECK ADD  CONSTRAINT [FK_Projects_Department] FOREIGN KEY([department_id])
REFERENCES [dbo].[Department] ([department_id])
GO
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_Department]
GO
ALTER TABLE [dbo].[Projects]  WITH NOCHECK ADD  CONSTRAINT [FK_Projects_GroupMember_Approved] FOREIGN KEY([approved_by])
REFERENCES [dbo].[Group_Member] ([group_member_id])
GO
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_GroupMember_Approved]
GO
ALTER TABLE [dbo].[Projects]  WITH NOCHECK ADD  CONSTRAINT [FK_Projects_Groups] FOREIGN KEY([group_id])
REFERENCES [dbo].[Groups] ([group_id])
GO
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_Groups]
GO
ALTER TABLE [dbo].[Projects]  WITH NOCHECK ADD  CONSTRAINT [FK_Projects_TimelineSequence] FOREIGN KEY([sequence_id])
REFERENCES [dbo].[TimelineSequence] ([sequence_id])
GO
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_TimelineSequence]
GO
ALTER TABLE [dbo].[Projects]  WITH NOCHECK ADD  CONSTRAINT [FK_Projects_Users_Created] FOREIGN KEY([created_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_Users_Created]
GO
ALTER TABLE [dbo].[Quotas]  WITH NOCHECK ADD  CONSTRAINT [FK_Quotas_Projects] FOREIGN KEY([project_id])
REFERENCES [dbo].[Projects] ([project_id])
GO
ALTER TABLE [dbo].[Quotas] CHECK CONSTRAINT [FK_Quotas_Projects]
GO
ALTER TABLE [dbo].[Quotas]  WITH NOCHECK ADD  CONSTRAINT [FK_Quotas_Users] FOREIGN KEY([allocated_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Quotas] CHECK CONSTRAINT [FK_Quotas_Users]
GO
ALTER TABLE [dbo].[Timeline]  WITH NOCHECK ADD  CONSTRAINT [FK_Timeline_TimelineSequence] FOREIGN KEY([sequence_id])
REFERENCES [dbo].[TimelineSequence] ([sequence_id])
GO
ALTER TABLE [dbo].[Timeline] CHECK CONSTRAINT [FK_Timeline_TimelineSequence]
GO
ALTER TABLE [dbo].[Timeline]  WITH NOCHECK ADD  CONSTRAINT [FK_Timeline_Users] FOREIGN KEY([created_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Timeline] CHECK CONSTRAINT [FK_Timeline_Users]
GO
ALTER TABLE [dbo].[TimelineSequence]  WITH NOCHECK ADD  CONSTRAINT [FK_TimelineSequence_Users] FOREIGN KEY([created_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[TimelineSequence] CHECK CONSTRAINT [FK_TimelineSequence_Users]
GO
ALTER TABLE [dbo].[Users]  WITH NOCHECK ADD  CONSTRAINT [FK_Users_Department] FOREIGN KEY([department_id])
REFERENCES [dbo].[Department] ([department_id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Department]
GO
ALTER TABLE [dbo].[Documents]  WITH NOCHECK ADD  CONSTRAINT [CHK_Documents_References] CHECK  (([project_resource_id] IS NOT NULL))
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [CHK_Documents_References]
GO
ALTER TABLE [dbo].[Fund_Disbursement]  WITH CHECK ADD  CONSTRAINT [CHK_Fund_Disbursement_User_Request] CHECK  (([user_request] IS NOT NULL))
GO
ALTER TABLE [dbo].[Fund_Disbursement] CHECK CONSTRAINT [CHK_Fund_Disbursement_User_Request]
GO
ALTER TABLE [dbo].[Groups]  WITH NOCHECK ADD  CONSTRAINT [CHK_Members] CHECK  (([current_member]<=[max_member]))
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [CHK_Members]
GO
ALTER TABLE [dbo].[Projects]  WITH NOCHECK ADD  CONSTRAINT [CHK_Budget] CHECK  (([approved_budget]>=[spent_budget]))
GO
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [CHK_Budget]
GO
USE [master]
GO
ALTER DATABASE [LRMSDB] SET  READ_WRITE 
GO
