CREATE TABLE [dbo].[Run]
(
	id int not null identity,
	runDate datetime not null,
	[user] nvarchar(50) not null
);
