﻿CREATE TABLE dbo.AzureDBResourceGovernanceHistory(
	InstanceID INT NOT NULL,
	ValidFrom DATETIME2(2) NOT NULL,
	ValidTo DATETIME2(2) NOT NULL,
	database_id INT NOT NULL,
    logical_database_guid UNIQUEIDENTIFIER NOT NULL,
    physical_database_guid UNIQUEIDENTIFIER NOT NULL,
    server_name NVARCHAR(128) NOT NULL,
    database_name NVARCHAR(128) NOT NULL,
    slo_name NVARCHAR(128) NOT NULL,
    dtu_limit INT NULL,
    cpu_limit INT NULL,
    min_cpu TINYINT NOT NULL,
    max_cpu TINYINT NOT NULL,
    cap_cpu TINYINT NOT NULL,
    min_cores SMALLINT NOT NULL,
    max_dop SMALLINT NOT NULL,
    min_memory INT NOT NULL,
    max_memory INT NOT NULL,
    max_sessions INT NOT NULL,
    max_memory_grant INT NOT NULL,
    max_db_memory INT NULL,
    govern_background_io BIT NOT NULL,
    min_db_max_size_in_mb BIGINT NULL,
    max_db_max_size_in_mb BIGINT NULL,
    default_db_max_size_in_mb BIGINT NULL,
    db_file_growth_in_mb BIGINT NULL,
    initial_db_file_size_in_mb BIGINT NULL,
    log_size_in_mb BIGINT NULL,
    instance_cap_cpu INT NULL,
    instance_max_log_rate BIGINT NULL,
    instance_max_worker_threads INT NULL,
    replica_type INT NOT NULL,
    max_transaction_size BIGINT NULL,
    checkpoint_rate_mbps INT NULL,
    checkpoint_rate_io INT NULL,
    last_updated_date_utc DATETIME NULL,
    primary_group_id INT NULL,
    primary_group_max_workers INT NOT NULL,
    primary_min_log_rate BIGINT NOT NULL,
    primary_max_log_rate BIGINT NOT NULL,
    primary_group_min_io INT NOT NULL,
    primary_group_max_io INT NOT NULL,
    primary_group_min_cpu FLOAT(8) NOT NULL,
    primary_group_max_cpu FLOAT(8) NOT NULL,
    primary_log_commit_fee INT NOT NULL,
    primary_pool_max_workers INT NOT NULL,
    pool_max_io INT NOT NULL,
    govern_db_memory_in_resource_pool BIT NOT NULL,
    volume_local_iops INT NULL,
    volume_managed_xstore_iops INT NULL,
    volume_external_xstore_iops INT NULL,
    volume_type_local_iops INT NULL,
    volume_type_managed_xstore_iops INT NULL,
    volume_type_external_xstore_iops INT NULL,
    volume_pfs_iops INT NULL,
    volume_type_pfs_iops INT NULL,
    user_data_directory_space_quota_mb INT NULL,
    user_data_directory_space_usage_mb INT NULL,
	CONSTRAINT PK_AzureDBResourceGovernanceHistory PRIMARY KEY(InstanceID,ValidTo),
	CONSTRAINT FK_AzureDBResourceGovernanceHistory_Instances FOREIGN KEY(InstanceID) REFERENCES dbo.Instances(InstanceID)
)
GO
CREATE NONCLUSTERED INDEX FIX_AzureDBResourceGovernanceHistory_InstanceID
ON AzureDBResourceGovernanceHistory(InstanceID)
WHERE ValidTo ='9999-12-31'