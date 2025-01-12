﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using static DBADashGUI.DBADashStatus;

namespace DBADashGUI.Backups
{
    public partial class BackupsControl : UserControl
    {

        public bool IncludeCritical
        {
            get
            {
                return criticalToolStripMenuItem.Checked;
            }
            set
            {
                criticalToolStripMenuItem.Checked = value;
            }
        }

        public bool IncludeWarning
        {
            get
            {
                return warningToolStripMenuItem.Checked;
            }
            set
            {
                warningToolStripMenuItem.Checked = value;
            }
        }
        public bool IncludeNA
        {
            get
            {
                return undefinedToolStripMenuItem.Checked;
            }
            set
            {
                undefinedToolStripMenuItem.Checked = value;
            }
        }
        public bool IncludeOK
        {
            get
            {
                return OKToolStripMenuItem.Checked;
            }
            set
            {
                OKToolStripMenuItem.Checked = value;
            }
        }

        public List<Int32> InstanceIDs { get; set; }
        private int databaseID { get; set; }

        private List<Int32> backupInstanceIDs;

        public void RefreshBackups()
        {
            databaseID = 0;
            dgvBackups.DataSource = null;
            dgvBackups.Columns.Clear();

            backupInstanceIDs = new List<int>();
            tsBack.Enabled = false;
            refresh();
        }

        private void refresh()
        {
            refreshSummary();
            if (InstanceIDs.Count>0 && (splitContainer1.SplitterDistance+200) > splitContainer1.Height) // Sumary is taking up all the room so don't show DB level data.
            {
                splitContainer1.Panel2Collapsed = true;
            }
            else
            {
                splitContainer1.Panel2Collapsed = false;
                if (databaseID > 0)
                {
                    refreshLastBackup();
                }
                else
                {
                    refreshBackups();
                }
            }
            
        }

        private void refreshLastBackup()
        {
            using (var cn = new SqlConnection(Common.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.LastBackup_Get", cn) { CommandType = CommandType.StoredProcedure })
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("DatabaseID", databaseID);
                DataTable dtBackups = new DataTable();
                Common.ConvertUTCToLocal(ref dtBackups);
                da.Fill(dtBackups);
                dgvBackups.DataSource = null;
                dgvBackups.AutoGenerateColumns =false;
                dgvBackups.Columns.Clear();
                dgvBackups.Columns.AddRange(
                    new DataGridViewTextBoxColumn { HeaderText = "Database", DataPropertyName = "name", SortMode = DataGridViewColumnSortMode.Automatic, Frozen = Common.FreezeKeyColumn },
                    new DataGridViewTextBoxColumn() { HeaderText = "Recovery Model", DataPropertyName = "recovery_model" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Type", DataPropertyName = "backup_type_desc" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Start Date", DataPropertyName = "backup_start_date_utc" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Finish Date", DataPropertyName = "backup_finish_date_utc" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Duration", DataPropertyName = "BackupDuration" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Backup Size (GB)", DataPropertyName = "BackupSizeGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Backup MB/sec", DataPropertyName = "BackupMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Backup Size (Compressed) (GB)", DataPropertyName = "BackupSizeCompressedGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Compression Saving %", DataPropertyName = "CompressionSavingPct", DefaultCellStyle = new DataGridViewCellStyle() { Format = "P1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Backup Write MB/sec", DataPropertyName = "BackupWriteMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Partner Backup", DataPropertyName = "IsPartnerBackup" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Partner", DataPropertyName = "Partner" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Damaged", DataPropertyName = "is_damaged" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Checksum", DataPropertyName = "has_backup_checksums" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Compressed", DataPropertyName = "IsCompressed" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Password Protected", DataPropertyName = "is_password_protected" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Encrypted", DataPropertyName = "IsEncrypted" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Snapshot Backup", DataPropertyName = "is_snapshot" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Has Bulk Logged Data", DataPropertyName = "has_bulk_logged_data" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Readonly", DataPropertyName = "is_readonly" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Force Offline", DataPropertyName = "is_force_offline" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Single User", DataPropertyName = "is_single_user" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Key Algorythm", DataPropertyName = "key_algorithm" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Encryptor_Type", DataPropertyName = "encryptor_type" }
                );

                dgvBackups.DataSource = dtBackups;
                dgvBackups.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            }

        }
        

        private void refreshBackups()
        {
            UseWaitCursor = true;
            configureInstanceThresholdsToolStripMenuItem.Enabled = (InstanceIDs.Count == 1);

            using (var cn = new SqlConnection(Common.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.Backups_Get", cn) { CommandType = CommandType.StoredProcedure })
            {
                cn.Open();
                cmd.Parameters.AddWithValue("InstanceIDs", string.Join(",", InstanceIDs));
                cmd.Parameters.AddWithValue("IncludeCritical", IncludeCritical);
                cmd.Parameters.AddWithValue("IncludeWarning", IncludeWarning);
                cmd.Parameters.AddWithValue("IncludeNA", IncludeNA);
                cmd.Parameters.AddWithValue("IncludeOK", IncludeOK);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dtBackups = new DataTable();
                da.Fill(dtBackups);
                Common.ConvertUTCToLocal(ref dtBackups);
                dgvBackups.AutoGenerateColumns = false;
                dgvBackups.DataSource = null;
                dgvBackups.Columns.Clear();
                dgvBackups.Columns.AddRange(
                    new DataGridViewTextBoxColumn() { HeaderText = "Instance", DataPropertyName = "InstanceDisplayName", Name = "Instance", Frozen=Common.FreezeKeyColumn },
                    new DataGridViewLinkColumn { HeaderText = "Database", DataPropertyName = "name", SortMode = DataGridViewColumnSortMode.Automatic, LinkColor = DashColors.LinkColor, Frozen = Common.FreezeKeyColumn },
                    new DataGridViewTextBoxColumn() { HeaderText = "Created", DataPropertyName = "create_date_utc" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Recovery Model", DataPropertyName = "recovery_model_desc" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Full", DataPropertyName = "LastFull", Name = "LastFull" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Diff", DataPropertyName = "LastDiff", Name = "LastDiff" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Log", DataPropertyName = "LastLog", Name = "LastLog" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Filegroup", DataPropertyName = "LastFG", Name = "LastFG" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Filegroup Diff", DataPropertyName = "LastFGDiff", Name = "LastFGDiff" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Partial", DataPropertyName = "LastPartial", Name = "LastPartial" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Partial Diff", DataPropertyName = "LastPartial", Name = "LastPartialDiff" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Partner Backup", DataPropertyName = "IsPartnerBackup" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Full Damaged", DataPropertyName = "IsFullDamaged" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Diff Damaged", DataPropertyName = "IsDiffDamaged" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Is Log Damaged", DataPropertyName = "IsLogDamaged" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Snapshot Date", DataPropertyName = "SnapshotDate" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Snapshot Age", DataPropertyName = "SnapshotAge", Name = "SnapshotAge" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Threshold Configured Level", DataPropertyName = "ThresholdsConfiguredLevel" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Excluded Reason", DataPropertyName = "FullBackupExcludedReason" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Excluded Reason", DataPropertyName = "DiffBackupExcludedReason" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Log Backup Excluded Reason", DataPropertyName = "LogBackupExcludedReason" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Log Backup Warning Threshold", DataPropertyName = "LogBackupWarningThreshold" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Log Backup Critical Threshold", DataPropertyName = "LogBackupCriticalThreshold" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Warning Threshold", DataPropertyName = "FullBackupWarningThreshold" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Critical Threshold", DataPropertyName = "FullBackupCriticalThreshold" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Warning Threshold", DataPropertyName = "DiffBackupWarningThreshold" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Critical Threshold", DataPropertyName = "DiffBackupCriticalThreshold" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Consider Partial Backups", DataPropertyName = "ConsiderPartialBackups" },
                    new DataGridViewCheckBoxColumn() { HeaderText = "Consider FG Backups", DataPropertyName = "ConsiderFGBackups" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Last Full Duration", DataPropertyName = "LastFullDuration" } ,
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Size (GB)", DataPropertyName = "FullBackupSizeGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup MB/sec", DataPropertyName = "FullBackupMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Size (Compressed) (GB)", DataPropertyName = "FullBackupSizeCompressedGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Compression Saving %", DataPropertyName = "FullCompressionSavingPct", DefaultCellStyle = new DataGridViewCellStyle() { Format = "P1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Write MB/sec", DataPropertyName = "FullBackupWriteMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Size (GB)", DataPropertyName = "DiffBackupSizeGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } } ,
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup MB/sec", DataPropertyName = "DiffBackupMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } } ,
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Size (Compressed) (GB)", DataPropertyName = "DiffBackupSizeCompressedGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } } ,
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Compression Saving %", DataPropertyName = "DiffCompressionSavingPct", DefaultCellStyle = new DataGridViewCellStyle() { Format = "P1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Write MB/sec", DataPropertyName = "DiffBackupWriteMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                    new DataGridViewTextBoxColumn() { HeaderText = "Checksums", DataPropertyName = "Checksums" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Is Compressed", DataPropertyName = "IsCompressed" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Password Protected", DataPropertyName = "PasswordProtected" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Encrypted", DataPropertyName = "IsEncrypted" },
                    new DataGridViewTextBoxColumn() { HeaderText = "Snapshot Backup", DataPropertyName = "SnapshotBackups" },
                    new DataGridViewLinkColumn() { HeaderText = "Configure", Text = "Configure", UseColumnTextForLinkValue = true, SortMode = DataGridViewColumnSortMode.NotSortable, Name = "Configure", LinkColor = DashColors.LinkColor }
                );               
                
                dgvBackups.DataSource = new DataView(dtBackups);
                dgvBackups.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            }

            UseWaitCursor = false;
        }

        private void refreshSummary()
        {
            UseWaitCursor = true;
            using (var cn = new SqlConnection(Common.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.BackupSummary_Get", cn) { CommandType = CommandType.StoredProcedure })
            {
                cn.Open();
                cmd.Parameters.AddWithValue("InstanceIDs", InstanceIDs.AsDataTable());
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Common.ConvertUTCToLocal(ref dt);
                dgvSummary.AutoGenerateColumns = false;
                if (dgvSummary.Columns.Count == 0)
                {
                    dgvSummary.Columns.AddRange(
                        new DataGridViewLinkColumn() { HeaderText = "Instance", DataPropertyName = "InstanceDisplayName", SortMode = DataGridViewColumnSortMode.Automatic, Name = "Instance", LinkColor = DashColors.LinkColor, Frozen=Common.FreezeKeyColumn },
                        new DataGridViewTextBoxColumn() { HeaderText = "Database Count", DataPropertyName = "DatabaseCount" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup OK", DataPropertyName = "FullOK", Name = "FullOK" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup N/A", DataPropertyName = "FullNA", Name = "FullNA" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Warning", DataPropertyName = "FullWarning", Name = "FullWarning" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Critical", DataPropertyName = "FullCritical", Name = "FullCritical" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup OK", DataPropertyName = "DiffOK", Name = "DiffOK" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup N/A", DataPropertyName = "DiffNA", Name = "DiffNA" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Warning", DataPropertyName = "DiffWarning", Name = "DiffWarning" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Critical", DataPropertyName = "DiffCritical", Name = "DiffCritical" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Backup OK", DataPropertyName = "LogOK", Name = "LogOK" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Backup N/A", DataPropertyName = "LogNA", Name = "LogNA" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Backup Warning", DataPropertyName = "LogWarning", Name = "LogWarning" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Backup Critical", DataPropertyName = "LogCritical", Name = "LogCritical" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Recovery Count", DataPropertyName = "FullRecoveryCount" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Bulk Logged Count", DataPropertyName = "BulkLoggedCount" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Simple Count", DataPropertyName = "SimpleCount" },
                        new DataGridViewCheckBoxColumn() { HeaderText = "Is Partner Backup", DataPropertyName = "IsPartnerBackup" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Snapshot Date", DataPropertyName = "SnapshotDate" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Snapshot Age", DataPropertyName = "SnapshotAge", Name = "SnapshotAge" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Oldest Full", DataPropertyName = "OldestFull", ToolTipText = "Date of oldest full backup, excluding databases without a threshold configured" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Oldest Diff", DataPropertyName = "OldestDiff", ToolTipText = "Date of oldest diff backup, excluding databases without a threshold configured" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Oldest Log", DataPropertyName = "OldestLog", ToolTipText = "Date of oldest log backup, excluding databases without a threshold configured" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Size (GB)", DataPropertyName = "FullBackupSizeGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup MB/sec", DataPropertyName = "FullBackupMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Size (Compressed) (GB)", DataPropertyName = "FullBackupSizeCompressedGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Compression Saving %", DataPropertyName = "FullCompressionSavingPct", DefaultCellStyle = new DataGridViewCellStyle() { Format = "P1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Backup Write MB/sec", DataPropertyName = "FullBackupWriteMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Size (GB)", DataPropertyName = "DiffBackupSizeGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup MB/sec", DataPropertyName = "DiffBackupMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Size (Compressed) (GB)", DataPropertyName = "DiffBackupSizeCompressedGB", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Compression Saving %", DataPropertyName = "DiffCompressionSavingPct", DefaultCellStyle = new DataGridViewCellStyle() { Format = "P1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Backup Write MB/sec", DataPropertyName = "DiffBackupWriteMBsec", DefaultCellStyle = new DataGridViewCellStyle() { Format = "N1" } },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Checksums", DataPropertyName = "FullChecksum" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Checksums", DataPropertyName = "DiffChecksum" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Checksums", DataPropertyName = "LogChecksum" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Compressed", DataPropertyName = "FullCompressed" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Compressed", DataPropertyName = "DiffCompressed" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Compressed", DataPropertyName = "LogCompressed" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Password Protected", DataPropertyName = "FullPasswordProtected" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Password Protected", DataPropertyName = "DiffPasswordProtected" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Password Protected", DataPropertyName = "LogPasswordProtected" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Full Encrypted", DataPropertyName = "FullEncrypted" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Diff Encrypted", DataPropertyName = "DiffEncrypted" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Log Encrypted", DataPropertyName = "LogEncrypted" },
                        new DataGridViewTextBoxColumn() { HeaderText = "Snapshot Backups", DataPropertyName = "SnapshotBackups" },
                        new DataGridViewTextBoxColumn() { HeaderText = "DB Level Threshold Config", DataPropertyName = "DBThresholdConfiguration" },
                        new DataGridViewLinkColumn() { HeaderText = "Configure", Text = "Configure", UseColumnTextForLinkValue = true, SortMode = DataGridViewColumnSortMode.NotSortable, Name = "Configure", LinkColor = DashColors.LinkColor }
                        );
                }
                dgvSummary.Columns[0].Frozen = Common.FreezeKeyColumn;
                dgvSummary.DataSource = new DataView(dt);               
                splitContainer1.SplitterDistance = (dgvSummary.Rows.Count * 24) + dgvSummary.ColumnHeadersHeight+24; // Set size based on row count, header size and scrollbar
                dgvSummary.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            }
            UseWaitCursor = false;
        }

        public BackupsControl()
        {
            InitializeComponent();
        }



        private void dgvBackups_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dgvBackups.Columns.Contains("LastFull"))
            {
                for (Int32 idx = e.RowIndex; idx < e.RowIndex + e.RowCount; idx += 1)
                {
                    var row = (DataRowView)dgvBackups.Rows[idx].DataBoundItem;
                    var fullBackupStatus = (DBADashStatus.DBADashStatusEnum)row["FullBackupStatus"];
                    var diffBackupStatus = (DBADashStatus.DBADashStatusEnum)row["DiffBackupStatus"];
                    var logBackupStatus = (DBADashStatus.DBADashStatusEnum)row["LogBackupStatus"];
                    var snapshotStatus = (DBADashStatus.DBADashStatusEnum)row["SnapshotAgeStatus"];
                    dgvBackups.Rows[idx].Cells["LastFull"].SetStatusColor(fullBackupStatus);
                    dgvBackups.Rows[idx].Cells["LastDiff"].SetStatusColor(diffBackupStatus);
                    dgvBackups.Rows[idx].Cells["LastLog"].SetStatusColor(logBackupStatus);
                    if ((bool)row["ConsiderPartialBackups"])
                    {
                        dgvBackups.Rows[idx].Cells["LastPartial"].SetStatusColor(fullBackupStatus);
                        dgvBackups.Rows[idx].Cells["LastPartialDiff"].SetStatusColor(diffBackupStatus);
                    }
                    else
                    {
                        dgvBackups.Rows[idx].Cells["LastPartial"].SetStatusColor(DBADashStatusEnum.NA);
                        dgvBackups.Rows[idx].Cells["LastPartialDiff"].SetStatusColor(DBADashStatusEnum.NA);
                    }
                    if ((bool)row["ConsiderFGBackups"])
                    {
                        dgvBackups.Rows[idx].Cells["LastFG"].SetStatusColor(fullBackupStatus);
                        dgvBackups.Rows[idx].Cells["LastFGDiff"].SetStatusColor(diffBackupStatus);
                    }
                    else
                    {
                        dgvBackups.Rows[idx].Cells["LastFG"].SetStatusColor(DBADashStatusEnum.NA);
                        dgvBackups.Rows[idx].Cells["LastFGDiff"].SetStatusColor(DBADashStatusEnum.NA);
                    }
                    dgvBackups.Rows[idx].Cells["SnapshotAge"].SetStatusColor(snapshotStatus);
                    if ((string)row["ThresholdsConfiguredLevel"] == "Database")
                    {
                        dgvBackups.Rows[idx].Cells["Configure"].Style.Font = new Font(dgvBackups.Font, FontStyle.Bold);
                    }
                    else
                    {
                        dgvBackups.Rows[idx].Cells["Configure"].Style.Font = new Font(dgvBackups.Font, FontStyle.Regular);
                    }
                }
            }

        }

        private void dgvBackups_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBackups.Columns.Contains("Configure"))
            {
                var row = (DataRowView)dgvBackups.Rows[e.RowIndex].DataBoundItem;
                if (dgvBackups.Columns[e.ColumnIndex].Name == "Configure")
                {                   
                    ConfigureThresholds((Int32)row["InstanceID"], (Int32)row["DatabaseID"]);
                }
                else if (dgvBackups.Columns[e.ColumnIndex].HeaderText == "Database")
                {
                    databaseID = (Int32)row["DatabaseID"];
                    tsBack.Enabled = true;
                    refresh();
                }
            }
        }

        private void ConfigureThresholds(Int32 InstanceID, Int32 DatabaseID)
        {
            var frm = new BackupThresholdsConfig
            {
                InstanceID = InstanceID,
                DatabaseID = DatabaseID
            };
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {
                refresh();
            }

        }

        private void configureRootThresholdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigureThresholds(-1, -1);
        }

        private void configureInstanceThresholdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (InstanceIDs.Count == 1)
            {
                ConfigureThresholds(InstanceIDs[0], -1);
            }
        }

        private void criticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshBackups();

        }

        private void tsFilter_Click(object sender, EventArgs e)
        {
            refresh();
        }


        private void tsRefresh_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private void tsCopy_Click(object sender, EventArgs e)
        {
            dgvSummary.Columns["Configure"].Visible = false;
            Common.CopyDataGridViewToClipboard(dgvSummary);
            dgvSummary.Columns["Configure"].Visible = true;
        }

        private void tsExcel_Click(object sender, EventArgs e)
        {
            dgvSummary.Columns["Configure"].Visible = false;
            Common.PromptSaveDataGridView(ref dgvSummary);
            dgvSummary.Columns["Configure"].Visible =true;
        }

        private void dgvSummary_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (Int32 idx = e.RowIndex; idx < e.RowIndex + e.RowCount; idx += 1)
            {

                var row = (DataRowView)dgvSummary.Rows[idx].DataBoundItem;
                var snapshotStatus = (DBADashStatus.DBADashStatusEnum)row["SnapshotAgeStatus"];

                dgvSummary.Rows[idx].Cells["SnapshotAge"].SetStatusColor(snapshotStatus);

                var okCols = new string[] { "FullOK", "DiffOK", "LogOK" };
                foreach (string col in okCols)
                {
                    int value = Convert.ToInt32(dgvSummary.Rows[idx].Cells[col].Value);
                    dgvSummary.Rows[idx].Cells[col].SetStatusColor(value > 0 ? DBADashStatusEnum.OK : DBADashStatusEnum.NA);
                }
                var warningCols = new string[] { "FullWarning", "DiffWarning", "LogWarning" };
                foreach (string col in warningCols)
                {
                    int value = Convert.ToInt32(dgvSummary.Rows[idx].Cells[col].Value);
                    dgvSummary.Rows[idx].Cells[col].SetStatusColor(value > 0 ? DBADashStatusEnum.Warning : DBADashStatusEnum.NA);
                }
                var criticalCols = new string[] { "FullCritical", "DiffCritical", "LogCritical" };
                foreach (string col in criticalCols)
                {
                    int value = Convert.ToInt32(dgvSummary.Rows[idx].Cells[col].Value);
                    dgvSummary.Rows[idx].Cells[col].SetStatusColor(value > 0 ? DBADashStatusEnum.Critical : DBADashStatusEnum.NA);
                }
                dgvSummary.Rows[idx].Cells["Configure"].Style.Font = Convert.ToBoolean(row["InstanceThresholdConfiguration"]) ? new Font(dgvSummary.Font, FontStyle.Bold) : new Font(dgvSummary.Font, FontStyle.Regular);
                
            }
        }

        private void dgvSummary_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dgvSummary.Columns[e.ColumnIndex].Name == "Instance" && backupInstanceIDs.Count == 0)
                {
                    databaseID = 0;
                    DataRowView row = (DataRowView)dgvSummary.Rows[e.RowIndex].DataBoundItem;
                    backupInstanceIDs = InstanceIDs;
                    InstanceIDs = new List<int>() { (int)row["InstanceID"] };
                    IncludeCritical = true;
                    IncludeWarning = true;
                    IncludeOK = true;
                    IncludeNA = true;
                    tsBack.Enabled = true;
                    refresh();
                }
                else if (dgvSummary.Columns[e.ColumnIndex].Name == "Configure")
                {
                    var row = (DataRowView)dgvSummary.Rows[e.RowIndex].DataBoundItem;
                    ConfigureThresholds((Int32)row["InstanceID"], -1);
                }
            }
        }

    

        private void tsBack_Click(object sender, EventArgs e)
        {
            if (databaseID > 0)
            {
                databaseID = 0;
            }
            else
            {
                InstanceIDs = backupInstanceIDs;
                backupInstanceIDs = new List<int>();
            }
            tsBack.Enabled = backupInstanceIDs.Count>0;
            IncludeCritical = true;
            IncludeWarning = true;
            IncludeOK = InstanceIDs.Count==1;
            IncludeNA = InstanceIDs.Count==1;
            refresh();
        }

        private void tsExcelDetail_Click(object sender, EventArgs e)
        {
            dgvBackups.Columns["Configure"].Visible = false;
            Common.PromptSaveDataGridView(ref dgvBackups);
            dgvBackups.Columns["Configure"].Visible = true;
        }

        private void tsCopyDetail_Click(object sender, EventArgs e)
        {
            dgvBackups.Columns["Configure"].Visible = false;
            Common.CopyDataGridViewToClipboard(dgvBackups);
            dgvBackups.Columns["Configure"].Visible = true;
        }

        private void tsCols_Click(object sender, EventArgs e)
        {
            using (var frm = new SelectColumns() { Columns = dgvSummary.Columns })
            {
                frm.ShowDialog(this);
            }
        }

        private void tsDetailCols_Click(object sender, EventArgs e)
        {
            using (var frm = new SelectColumns() { Columns = dgvBackups.Columns })
            {
                frm.ShowDialog(this);
            }
        }
    }
}
