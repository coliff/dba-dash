﻿CREATE TABLE dbo.AvailabilityGroups(
	   InstanceID INT NOT NULL,
	   group_id UNIQUEIDENTIFIER NOT NULL,
       name NVARCHAR(128) NOT NULL,
       resource_id NVARCHAR(40) NOT NULL,
       resource_group_id NVARCHAR(40) NOT NULL,
       failure_condition_level INT NOT NULL,
       health_check_timeout INT NOT NULL,
       automated_backup_preference TINYINT NOT NULL,
       version SMALLINT  NOT NULL,
       basic_features BIT  NOT NULL,
       dtc_support BIT NOT NULL, 
       db_failover BIT NOT NULL,
       is_distributed BIT NOT NULL,
       cluster_type TINYINT NULL,
       required_synchronized_secondaries_to_commit INT NULL,
       sequence_number BIGINT NULL,
       is_contained BIT NULL,
       automated_backup_preference_desc  AS (CASE automated_backup_preference WHEN (0) THEN N'PRIMARY' WHEN (1) THEN N'SECONDARY_ONLY' WHEN (2) THEN N'SECONDARY' WHEN (3) THEN N'NONE' ELSE CONVERT(NVARCHAR(60),automated_backup_preference) END),
       CONSTRAINT FK_AvailabilityGroups_Instances FOREIGN KEY(InstanceID) REFERENCES dbo.Instances(InstanceID),
       CONSTRAINT PK_AvailabilityGroups PRIMARY KEY(InstanceID,group_id)
)