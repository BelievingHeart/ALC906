create procedure spSelectFaiCollectionByIntervalAlps
@TimeStart varchar(50),
@TimeEnd varchar(50)

as begin


select * from dbo.Alps
	where InspectionTime between @TimeStart and @TimeEnd
end



--exec dbo.spGetFaiCollectionsByHour 2019,12,17,21