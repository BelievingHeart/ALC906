create procedure dbo.spDeleteOutdatedAlps
as begin
set nocount on

declare @ninetydaysago datetime
declare @now datetime
set @now = getdate()
set @ninetydaysago = dateadd(day,-90,@now)

delete from dbo.Alps where InspectionTime < @ninetydaysago
end