CREATE PROCEDURE [dbo].[NewRun]
	@user nvarchar(50),
	@runID int output
AS
	insert into run(rundate, [user]) 
	values (getdate(), @user)
	select @runid = (select @@IDENTITY AS NewID)