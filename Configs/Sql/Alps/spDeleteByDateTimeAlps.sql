create procedure dbo.spDeleteByDateTimeAlps
@DateToDelete datetime
as begin
set nocount on

delete from dbo.Alps where InspectionTime=@DateToDelete
end