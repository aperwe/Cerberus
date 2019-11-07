CREATE PROCEDURE [dbo].[NewStrResult]
	@strID int,
	@message varchar(8000),
	@result char(10),
	@checkname char(100),
	@strResultID int output
AS
	insert into strResult (strid, message, result, checkname) 
	values (@strid, @message, @result, @checkname)
	
	select @strResultID = (select @@IDENTITY)
RETURN 0;