/****** Object:  ForeignKey [FK_ProjectTags_Tags]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[ProjectTags]  WITH CHECK ADD  CONSTRAINT [FK_ProjectTags_Tags] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tags] ([ID])


GO
ALTER TABLE [dbo].[ProjectTags] CHECK CONSTRAINT [FK_ProjectTags_Tags]

