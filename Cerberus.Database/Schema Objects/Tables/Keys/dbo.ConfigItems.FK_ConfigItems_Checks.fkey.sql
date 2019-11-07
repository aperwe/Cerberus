/****** Object:  ForeignKey [FK_ConfigItems_Checks]    Script Date: 05/13/2009 15:49:20 ******/
ALTER TABLE [dbo].[ConfigItems]  WITH CHECK ADD  CONSTRAINT [FK_ConfigItems_Checks] FOREIGN KEY([CheckID])
REFERENCES [dbo].[Checks] ([ID])


GO
ALTER TABLE [dbo].[ConfigItems] CHECK CONSTRAINT [FK_ConfigItems_Checks]

