/****** Object:  ForeignKey [FK_CheckTags_Tags]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[CheckTags]  WITH CHECK ADD  CONSTRAINT [FK_CheckTags_Tags] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tags] ([ID])


GO
ALTER TABLE [dbo].[CheckTags] CHECK CONSTRAINT [FK_CheckTags_Tags]

