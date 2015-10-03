USE [Test]
GO
/****** Object:  StoredProcedure [dbo].[SelectSectionStructuresInBounds]    Script Date: 9/15/2015 5:27:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[SelectSectionStructuresAndLinksInBounds]
	-- Add the parameters for the stored procedure here
	@Z float,
	@BBox geometry,
	@MinRadius float,
	@QueryDate datetime
AS
BEGIN 
		IF OBJECT_ID('tempdb..#SectionLocationsInBounds') IS NOT NULL DROP TABLE #SectionLocationsInBounds
		IF OBJECT_ID('tempdb..#SectionStructuresInBounds') IS NOT NULL DROP TABLE #SectionStructuresInBounds
		select * into #SectionLocationsInBounds from Location where (@bbox.STIntersects(VolumeShape) = 1) and Z = @Z AND Radius >= @MinRadius order by ParentID
		select * into #SectionStructuresInBounds from Structure where ID in (select distinct ParentID from #SectionLocationsInBounds)

		IF @QueryDate IS NOT NULL
			select * from #SectionStructuresInBounds where LastModified >= @QueryDate
		ELSE
			select * from #SectionStructuresInBounds
			  
		Select * from StructureLink L
		where (L.TargetID in (Select ID from #SectionStructuresInBounds))
			OR (L.SourceID in (Select ID from #SectionStructuresInBounds)) 
END
