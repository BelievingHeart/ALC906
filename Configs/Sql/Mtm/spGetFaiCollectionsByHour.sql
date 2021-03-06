create procedure spGetFaiCollectionsByHour
@Year int,
@Month int,
@Day int,
@Hour int

as begin

declare @yearText varchar(4);
declare @monthText varchar(2);
declare @dayText varchar(2);
declare @hourText varchar(2);
declare @nextHourText varchar(2);


set @yearText = convert(varchar(4), @Year)
set @monthText = CONVERT(varchar(2), @Month)
set @dayText = CONVERT(varchar(2), @Day)
set @hourText = CONVERT(varchar(2), @Hour)
set @nextHourText = CONVERT(varchar(2), @Hour+1)

if @Hour=23
select * from dbo.Mtm
	where CAST(InspectionTime as date)= @yearText+@monthText+@dayText and  CAST(InspectionTime as time)>= '23:00:00'
else
select * from dbo.Mtm
	where CAST(InspectionTime as date)= @yearText+@monthText+@dayText and  CAST(InspectionTime as time) between @hourText+':00:00' and @nextHourText+':00:00'
end



--exec dbo.spGetFaiCollectionsByHour 2019,12,17,21