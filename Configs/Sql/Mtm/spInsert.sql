-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE spInsertMtm
-- Add the parameters for the stored procedure here
	@InspectionTime datetime,
	@Cavity int,
	@Result varchar(6),
	@FAI1__1A float, 
@FAI1__1B float, 
@FAI1__1C float, 
@FAI1__2A float, 
@FAI1__2B float, 
@FAI1__2C float, 
@FAI2__1A float, 
@FAI2__1B float, 
@FAI2__1C float, 
@FAI2__2A float, 
@FAI2__2B float, 
@FAI2__2C float, 
@FAI9__1 float, 
@FAI9__2 float, 
@FAI10__1 float, 
@FAI10__2 float, 
@FAI23__A float, 
@FAI23__C float, 
@FAI24__A float, 
@FAI24__B float, 
@FAI24__C float, 
@FAI33__1 float, 
@FAI33__2 float, 
@FAI34__1 float, 
@FAI34__2 float, 
@FAI43__A float, 
@FAI43__B float, 
@FAI43__C float, 
@FAI46__A float, 
@FAI46__B float, 
@FAI46__C float, 
@FAI__52 float, 
@FAI11__1 float, 
@FAI11__2 float, 
@FAI11__3 float, 
@FAI11__4 float, 
@FAI11__5 float, 
@FAI11__6 float, 
@FAI11__7 float, 
@FAI11__8 float, 
@FAI16__1 float, 
@FAI16__2 float, 
@FAI17__1 float, 
@FAI17__2 float, 
@FAI17__3 float, 
@FAI17__4 float, 
@FAI18C float, 
@FAI18M float, 
@FAI19__1 float, 
@FAI19__2 float, 
@FAI19__3 float, 
@FAI19__4 float, 
@FAI19__5 float, 
@FAI19__6 float, 
@FAI19__7 float, 
@FAI19__8 float, 
@FAI19__9 float, 
@FAI19__10 float, 
@FAI19__11 float, 
@FAI19__12 float, 
@FAI20__1 float, 
@FAI20__2 float, 
@FAI20__3 float, 
@FAI20__4 float, 
@FAI21 float, 
@FAI22 float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into dbo.Mtm(InspectionTime, Cavity, Result, FAI1__1A,FAI1__1B,FAI1__1C,FAI1__2A,FAI1__2B,FAI1__2C,FAI2__1A,FAI2__1B,FAI2__1C,FAI2__2A,FAI2__2B,FAI2__2C,FAI9__1,FAI9__2,FAI10__1,FAI10__2,FAI23__A,FAI23__C,FAI24__A,FAI24__B,FAI24__C,FAI33__1,FAI33__2,FAI34__1,FAI34__2,FAI43__A,FAI43__B,FAI43__C,FAI46__A,FAI46__B,FAI46__C,FAI__52,FAI11__1,FAI11__2,FAI11__3,FAI11__4,FAI11__5,FAI11__6,FAI11__7,FAI11__8,FAI16__1,FAI16__2,FAI17__1,FAI17__2,FAI17__3,FAI17__4,FAI18C,FAI18M,FAI19__1,FAI19__2,FAI19__3,FAI19__4,FAI19__5,FAI19__6,FAI19__7,FAI19__8,FAI19__9,FAI19__10,FAI19__11,FAI19__12,FAI20__1,FAI20__2,FAI20__3,FAI20__4,FAI21,FAI22) 
	values(@InspectionTime, @Cavity, @Result, @FAI1__1A,@FAI1__1B,@FAI1__1C,@FAI1__2A,@FAI1__2B,@FAI1__2C,@FAI2__1A,@FAI2__1B,@FAI2__1C,@FAI2__2A,@FAI2__2B,@FAI2__2C,@FAI9__1,@FAI9__2,@FAI10__1,@FAI10__2,@FAI23__A,@FAI23__C,@FAI24__A,@FAI24__B,@FAI24__C,@FAI33__1,@FAI33__2,@FAI34__1,@FAI34__2,@FAI43__A,@FAI43__B,@FAI43__C,@FAI46__A,@FAI46__B,@FAI46__C,@FAI__52,@FAI11__1,@FAI11__2,@FAI11__3,@FAI11__4,@FAI11__5,@FAI11__6,@FAI11__7,@FAI11__8,@FAI16__1,@FAI16__2,@FAI17__1,@FAI17__2,@FAI17__3,@FAI17__4,@FAI18C,@FAI18M,@FAI19__1,@FAI19__2,@FAI19__3,@FAI19__4,@FAI19__5,@FAI19__6,@FAI19__7,@FAI19__8,@FAI19__9,@FAI19__10,@FAI19__11,@FAI19__12,@FAI20__1,@FAI20__2,@FAI20__3,@FAI20__4,@FAI21,@FAI22)
END
GO

--declare @currentTime as datetime
--set @currentTime=GETDATE();
--exec dbo.spInsert @InspectionTime=@currentTime, @Fai1=0, @Result='Ng'