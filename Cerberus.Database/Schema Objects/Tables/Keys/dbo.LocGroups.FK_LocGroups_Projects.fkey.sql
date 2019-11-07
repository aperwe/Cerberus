/****** Object:  ForeignKey [FK_LocGroups_Projects]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[LocGroups]  WITH CHECK ADD  CONSTRAINT [FK_LocGroups_Projects] FOREIGN KEY([ProjectID])
REFERENCES [dbo].[Projects] ([ID])


GO
ALTER TABLE [dbo].[LocGroups] CHECK CONSTRAINT [FK_LocGroups_Projects]

