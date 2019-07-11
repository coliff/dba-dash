﻿CREATE PROC [Report].[Summary_Get](@InstanceIDs VARCHAR(MAX)=NULL)
AS
DECLARE @Instances TABLE(
	InstanceID INT PRIMARY KEY
)
IF @InstanceIDs IS NULL
BEGIN
	INSERT INTO @Instances
	(
	    InstanceID
	)
	SELECT InstanceID 
	FROM dbo.Instances 
	WHERE IsActive=1
END 
ELSE 
BEGIN
	INSERT INTO @Instances
	(
		InstanceID
	)
	SELECT Item
	FROM dbo.SplitStrings(@InstanceIDs,',')
END;

WITH LS AS (
	SELECT InstanceID,MIN(Status) as LogShippingStatus
	FROM LogShippingStatus
	WHERE Status<>3
	GROUP BY InstanceID
)
, B as (
	SELECT InstanceID,
			MIN(NULLIF(FullBackupStatus,3)) as FullBackupStatus,
			MIN(NULLIF(LogBackupStatus,3)) as LogBackupStatus,
			MIN(NULLIF(DiffBackupStatus,3)) as DiffBackupStatus
	FROM dbo.BackupStatus
	GROUP BY InstanceID
)
, D AS (
	SELECT InstanceID, MIN(Status) as DriveStatus
	FROM dbo.DriveStatus
	WHERE Status<>3
	GROUP BY InstanceID
),
 F AS (
	SELECT InstanceID,MIN(FreeSpaceStatus) AS FileFreeSpaceStatus
	FROM dbo.DBFileStatus
	WHERE FreeSpaceStatus<>3
	GROUP BY InstanceID
)
SELECT I.InstanceID,
	I.Instance,
	STUFF((SELECT ',' + Tag FROM dbo.InstanceTag IT JOIN dbo.Tags T ON T.TagID = IT.TagID WHERE IT.InstanceID = I.InstanceID AND IT.TagID <> -1 ORDER BY T.Tag FOR XML PATH(''),TYPE).value('.','NVARCHAR(MAX)'),1,1,'') AS Tags,
	LS.LogShippingStatus,
	B.FullBackupStatus,
	B.LogBackupStatus,
	B.DiffBackupStatus,
	D.DriveStatus,
	F.FileFreeSpaceStatus
FROM dbo.Instances I 
LEFT JOIN LS ON I.InstanceID = LS.InstanceID
LEFT JOIN B ON I.InstanceID = B.InstanceID
LEFT JOIN D ON I.InstanceID = D.InstanceID
LEFT JOIN F ON I.InstanceID = F.InstanceID
WHERE EXISTS(SELECT 1 FROM @Instances t WHERE I.InstanceID = t.InstanceID)
AND I.IsActive=1