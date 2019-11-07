CREATE PROCEDURE [dbo].[NewStrInfo]
	@build char(20),
	@project varchar(50),
	@locgroup varchar(50),
	@lcxfile varchar(100),
	@sourcestring varchar(max),
	@comments varchar(8000),
	@lsresid varchar(512),
	@targetCultrue varchar(10),
	@runID int,
	@StrID int output
AS
	if exists (select strid from strinfo where lsresid = @lsresid and runid = @runid)
		begin
			select @strID = (select strid as [NewID] from strinfo where lsresid = @lsresid and runid = @runid)
		end
	else
		begin
			insert into strinfo (build, project, locgroup, lcxfile, sourcestring, comments, lsresid, targetCulture, runid)
				values (@build, @project, @locgroup, @lcxfile, @sourcestring, @comments, @lsresid, @targetCultrue, @runid)
			select @strID = (SELECT @@IDENTITY AS [NewID])
		end								
RETURN 0;