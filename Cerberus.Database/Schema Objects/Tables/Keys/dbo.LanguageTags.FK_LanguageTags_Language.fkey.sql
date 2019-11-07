/****** Object:  ForeignKey [FK_LanguageTags_Language]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[LanguageTags]  WITH CHECK ADD  CONSTRAINT [FK_LanguageTags_Language] FOREIGN KEY([LanguageID])
REFERENCES [dbo].[Language] ([Id])


GO
ALTER TABLE [dbo].[LanguageTags] CHECK CONSTRAINT [FK_LanguageTags_Language]

