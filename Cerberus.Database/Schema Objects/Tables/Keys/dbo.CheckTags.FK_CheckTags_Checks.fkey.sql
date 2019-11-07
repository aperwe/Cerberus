/****** Object:  ForeignKey [FK_CheckTags_Checks]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[CheckTags]  WITH CHECK ADD  CONSTRAINT [FK_CheckTags_Checks] FOREIGN KEY([CheckID])
REFERENCES [dbo].[Checks] ([ID])


GO
ALTER TABLE [dbo].[CheckTags] CHECK CONSTRAINT [FK_CheckTags_Checks]

