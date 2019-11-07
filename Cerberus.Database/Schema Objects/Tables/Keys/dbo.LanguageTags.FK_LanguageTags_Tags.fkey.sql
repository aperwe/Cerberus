/****** Object:  ForeignKey [FK_LanguageTags_Tags]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[LanguageTags]  WITH CHECK ADD  CONSTRAINT [FK_LanguageTags_Tags] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tags] ([ID])


GO
ALTER TABLE [dbo].[LanguageTags] CHECK CONSTRAINT [FK_LanguageTags_Tags]

