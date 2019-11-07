SET  ARITHABORT, CONCAT_NULL_YIELDS_NULL, ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, QUOTED_IDENTIFIER ON 
SET  NUMERIC_ROUNDABORT OFF
GO
:setvar DatabaseName "Cerberus.FastTrackDB"
:setvar PrimaryFilePhysicalName "c:\temp\Cerberus.FastTrackDB.mdf"
:setvar PrimaryLogFilePhysicalName "c:\temp\Cerberus.FastTrackDB_log.ldf"

USE [master]

GO

:on error exit

IF  (DB_ID(N'$(DatabaseName)') IS NOT NULL
    AND DATABASEPROPERTYEX(N'$(DatabaseName)','Status') <> N'ONLINE')
BEGIN
    RAISERROR(N'The state of the target database, %s, is not set to ONLINE. To deploy to this database, its state must be set to ONLINE.', 16, 127,N'$(DatabaseName)') WITH NOWAIT
    RETURN
END
GO

IF (DB_ID(N'$(DatabaseName)') IS NOT NULL)
BEGIN
    IF ((SELECT CAST(value AS nvarchar(128))
	    FROM 
		    [$(DatabaseName)]..fn_listextendedproperty('microsoft_database_tools_deploystamp', null, null, null, null, null, null )) 
	    = CAST(N'4edc2420-b368-4065-9b4e-38b9b4448328' AS nvarchar(128)))
    BEGIN
	    RAISERROR(N'Deployment has been skipped because the script has already been deployed to the target server.', 16 ,100) WITH NOWAIT
	    RETURN
    END
END
GO


:on error exit

CREATE DATABASE [$(DatabaseName)] ON ( NAME = N'PrimaryFileName', FILENAME = N'$(PrimaryFilePhysicalName)') LOG ON ( NAME = N'PrimaryLogFileName', FILENAME = N'$(PrimaryLogFilePhysicalName)') COLLATE Latin1_General_CI_AS 

GO

:on error resume
     
EXEC sp_dbcmptlevel N'$(DatabaseName)', 90

GO

IF EXISTS (SELECT 1 FROM [sys].[databases] WHERE [name] = N'$(DatabaseName)') 
    ALTER DATABASE [$(DatabaseName)] SET  
	ALLOW_SNAPSHOT_ISOLATION OFF
GO

IF EXISTS (SELECT 1 FROM [sys].[databases] WHERE [name] = N'$(DatabaseName)') 
    ALTER DATABASE [$(DatabaseName)] SET  
	READ_COMMITTED_SNAPSHOT OFF
GO

IF EXISTS (SELECT 1 FROM [sys].[databases] WHERE [name] = N'$(DatabaseName)') 
    ALTER DATABASE [$(DatabaseName)] SET  
	MULTI_USER,
	CURSOR_CLOSE_ON_COMMIT OFF,
	CURSOR_DEFAULT LOCAL,
	AUTO_CLOSE OFF,
	AUTO_CREATE_STATISTICS ON,
	AUTO_SHRINK OFF,
	AUTO_UPDATE_STATISTICS ON,
	AUTO_UPDATE_STATISTICS_ASYNC ON,
	ANSI_NULL_DEFAULT ON,
	ANSI_NULLS ON,
	ANSI_PADDING ON,
	ANSI_WARNINGS ON,
	ARITHABORT ON,
	CONCAT_NULL_YIELDS_NULL ON,
	NUMERIC_ROUNDABORT OFF,
	QUOTED_IDENTIFIER ON,
	RECURSIVE_TRIGGERS OFF,
	RECOVERY FULL,
	PAGE_VERIFY NONE,
	DISABLE_BROKER,
	PARAMETERIZATION SIMPLE
	WITH ROLLBACK IMMEDIATE
GO

IF IS_SRVROLEMEMBER ('sysadmin') = 1
BEGIN

IF EXISTS (SELECT 1 FROM [sys].[databases] WHERE [name] = N'$(DatabaseName)') 
    EXEC sp_executesql N'
    ALTER DATABASE [$(DatabaseName)] SET  
	DB_CHAINING OFF,
	TRUSTWORTHY OFF'

END
ELSE
BEGIN
    RAISERROR(N'Unable to modify the database settings for DB_CHAINING or TRUSTWORTHY. You must be a SysAdmin in order to apply these settings.',0,1)
END

GO

USE [$(DatabaseName)]

GO
/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script	
 Use SQLCMD syntax to include a file into the pre-deployment script			
 Example:      :r .\filename.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/








GO

:on error exit

:on error resume
GO
PRINT N'Creating [dbo].[Run]'
GO
CREATE TABLE [dbo].[Run]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[runDate] [datetime] NOT NULL,
[user] [nvarchar] (50) NOT NULL
) ON [PRIMARY]
GO
PRINT N'Creating [dbo].[NewRun]'
GO
CREATE PROCEDURE [dbo].[NewRun]
	@user nvarchar(50),
	@runID int output
AS
	insert into run(rundate, [user]) 
	values (getdate(), @user)
	select @runid = (select @@IDENTITY AS NewID)
GO
PRINT N'Creating [dbo].[strResult]'
GO
CREATE TABLE [dbo].[strResult]
(
[strID] [int] NULL,
[message] [varchar] (8000) NULL,
[result] [char] (10) NULL,
[checkname] [char] (100) NULL
) ON [PRIMARY]
GO
PRINT N'Creating [dbo].[NewStrResult]'
GO
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
GO
PRINT N'Creating [dbo].[strInfo]'
GO
CREATE TABLE [dbo].[strInfo]
(
[strID] [int] NOT NULL IDENTITY(1, 1),
[build] [char] (20) NULL,
[project] [varchar] (50) NULL,
[locgroup] [varchar] (50) NULL,
[lcxfile] [varchar] (100) NULL,
[sourcestring] [varchar] (max) NULL,
[comments] [varchar] (8000) NULL,
[lsresid] [varchar] (512) NULL,
[targetculture] [varchar] (10) NULL,
[runID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
PRINT N'Creating [dbo].[NewStrInfo]'
GO
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
GO

GO
/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script		
 Use SQLCMD syntax to include a file into the post-deployment script			
 Example:      :r .\filename.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/










USE [$(DatabaseName)]
IF ((SELECT COUNT(*) 
	FROM 
		::fn_listextendedproperty( 'microsoft_database_tools_deploystamp', null, null, null, null, null, null )) 
	> 0)
BEGIN
	EXEC [dbo].sp_dropextendedproperty 'microsoft_database_tools_deploystamp'
END
EXEC [dbo].sp_addextendedproperty 'microsoft_database_tools_deploystamp', N'4edc2420-b368-4065-9b4e-38b9b4448328'
GO

