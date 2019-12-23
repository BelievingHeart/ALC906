create procedure dbo.spDeleteByDateTimeMtm
@DateToDelete datetime
as begin
set nocount on

delete from dbo.Mtm where InspectionTime=@DateToDelete
end