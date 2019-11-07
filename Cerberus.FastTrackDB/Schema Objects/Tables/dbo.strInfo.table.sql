CREATE TABLE [dbo].[strInfo]
(
	strID int not null identity,
	build char(20),
	project varchar(50),
	locgroup varchar(50),
	lcxfile varchar(100),
	sourcestring varchar(max),
	comments varchar(8000),
	lsresid varchar(512),
	targetculture varchar(10),
	runID int
);
