/****** Object:  ForeignKey [FK_ConfigItems_LocGroups]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[ConfigItems]  WITH CHECK ADD  CONSTRAINT [FK_ConfigItems_LocGroups] FOREIGN KEY([LocGroupID])
REFERENCES [dbo].[LocGroups] ([ID])


GO
ALTER TABLE [dbo].[ConfigItems] CHECK CONSTRAINT [FK_ConfigItems_LocGroups]

