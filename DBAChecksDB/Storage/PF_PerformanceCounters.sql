﻿CREATE PARTITION FUNCTION [PF_PerformanceCounters](DATETIME2 (2))
    AS RANGE RIGHT
    FOR VALUES ();

