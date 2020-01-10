create procedure dbo.spDeleteOutdatedMtm
as begin
set nocount on

declare @ninetydaysago datetime
declare @now datetime
set @now = getdate()
set @ninetydaysago = dateadd(day,-90,@now)

delete from dbo.Mtm where InspectionTime < @ninetydaysago
end