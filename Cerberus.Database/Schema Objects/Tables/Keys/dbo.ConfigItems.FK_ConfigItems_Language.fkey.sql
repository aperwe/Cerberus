/****** Object:  ForeignKey [FK_ConfigItems_Language]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[ConfigItems]  WITH CHECK ADD  CONSTRAINT [FK_ConfigItems_Language] FOREIGN KEY([LanguageID])
REFERENCES [dbo].[Language] ([Id])


GO
ALTER TABLE [dbo].[ConfigItems] CHECK CONSTRAINT [FK_ConfigItems_Language]

