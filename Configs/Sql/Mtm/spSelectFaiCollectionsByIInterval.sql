create procedure spSelectFaiCollectionByIntervalMtm
@TimeStart varchar(50),
@TimeEnd varchar(50)

as begin


select * from dbo.Mtm
	where InspectionTime between @TimeStart and @TimeEnd
end



--exec dbo.spGetFaiCollectionsByHour 2019,12,17,21