// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc


using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public partial class Form1 : Form
    {
        private AppSettings _settings = new AppSettings();
        private AppSettings _lastLoggedSettingsSnapshot = new AppSettings();
        private readonly System.Windows.Forms.Timer _autoBackupCountdownTimer = new System.Windows.Forms.Timer();
        private readonly Dictionary<string, DateTime> _nextAutoBackupRunsByPair = new Dictionary<string, DateTime>();
        private bool _isRefreshingConfiguredPaths;
        private readonly ToolTip _mainToolTip = new ToolTip();
        private readonly bool _startMinimizedToSystray;
        private readonly ModernTheme.ModernScrollBar _configuredPathsVerticalScrollBar = new ModernTheme.ModernScrollBar
        {
            Name = "configuredPathsVerticalScrollBar",
            Orientation = Orientation.Vertical,
            Visible = false
        };
        private bool _isUpdatingConfiguredPathsScrollBar;
        private bool _isApplyingWindowSettings;
        private bool _isBackupRunning;
        private bool _isExplicitExitRequested;
        private readonly Dictionary<string, BackupProgressDisplayState> _backupProgressByPair =
            new Dictionary<string, BackupProgressDisplayState>(
                StringComparer.OrdinalIgnoreCase);
        private readonly System.Windows.Forms.Timer _backupProgressAnimationTimer =
            new System.Windows.Forms.Timer
            {
                Interval = 100
            };

        private Panel? _modernTitleBarPanel;
        private Label? _modernTitleLabel;
        private Label? _activeDataLossWarningLabel;
        private Label? _debugModeWarningLabel;
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        // # Title Refresh Interval
        private const int TitleRefreshIntervalMilliseconds = 1000;
        private string _baseWindowTitle = string.Empty;



        private sealed class BackupProgressDisplayState
        {
            public BackupFileProgressArea Area { get; set; }
            public int Percentage { get; set; }
            public bool IsIndeterminate { get; set; }
            public int PulseOffset { get; set; }
        }

        public Form1(bool startMinimizedToSystray = false)
        {
            _startMinimizedToSystray = startMinimizedToSystray;

            InitializeComponent();

            _baseWindowTitle = Text;

            using Stream? iconStream = typeof(Form1).Assembly.GetManifestResourceStream("EasyVersionBackup.Ressources.favicon2.ico");

            if (iconStream != null)
            {
                Icon = new Icon(iconStream);
            }
            else
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }

            notifyIconMain.Icon = Icon;

            // VISUAL IMPROVEMENTS
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            BackColor = ModernTheme.WindowBackColor;
            ModernWindowFrame.Apply(this);
            SizeGripStyle = SizeGripStyle.Hide;
            InitializeModernTitleBar();
            InitializeResizeGripPanel();
            InitializeMainToolbar();

            buttonBackup.FlatStyle = FlatStyle.Flat;
            buttonBackup.FlatAppearance.BorderSize = 0;
            buttonBackup.BackColor = ModernTheme.AccentColor;
            buttonBackup.ForeColor = ModernTheme.DarkTextColor;
            buttonBackup.Cursor = Cursors.Hand;

            buttonBackup.MouseEnter += (s, e) => buttonBackup.BackColor = ModernTheme.AccentHoverColor;
            buttonBackup.MouseLeave += (s, e) => buttonBackup.BackColor = ModernTheme.AccentColor;

            dataGridViewConfiguredPaths.BorderStyle = BorderStyle.None;
            dataGridViewConfiguredPaths.BackgroundColor = ModernTheme.WindowBackColor;
            dataGridViewConfiguredPaths.GridColor = ModernTheme.ControlBackColor;
            dataGridViewConfiguredPaths.EnableHeadersVisualStyles = false;
            dataGridViewConfiguredPaths.ShowCellToolTips = true;
            dataGridViewConfiguredPaths.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridViewConfiguredPaths.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.BackColor = ModernTheme.ControlBackColor;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.HeaderFontSize, FontStyle.Bold);

            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.BackColor;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.ForeColor;

            dataGridViewConfiguredPaths.DefaultCellStyle.BackColor = ModernTheme.TitleBarBackColor;
            dataGridViewConfiguredPaths.DefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewConfiguredPaths.DefaultCellStyle.SelectionBackColor = ModernTheme.AccentColor;
            dataGridViewConfiguredPaths.DefaultCellStyle.SelectionForeColor = ModernTheme.DarkTextColor;

            dataGridViewConfiguredPaths.AlternatingRowsDefaultCellStyle.BackColor = ModernTheme.WindowBackColor;
            dataGridViewConfiguredPaths.AlternatingRowsDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewConfiguredPaths.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

            dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceDirectory"].ReadOnly = false;
            dataGridViewConfiguredPaths.Columns["ColumnConfiguredTargetDirectory"].ReadOnly = false;

            InitializeAutoBackupTimerColumn();
            InitializeBackupInfoColumn();
            InitializeConfiguredPathActionColumns();

            dataGridViewConfiguredPaths.ScrollBars = ScrollBars.None;
            dataGridViewConfiguredPaths.AllowDrop = true;
            dataGridViewConfiguredPaths.CellContentClick += dataGridViewConfiguredPaths_CellContentClick;
            dataGridViewConfiguredPaths.CellPainting += dataGridViewConfiguredPaths_CellPainting;
            dataGridViewConfiguredPaths.CellToolTipTextNeeded += dataGridViewConfiguredPaths_CellToolTipTextNeeded;
            dataGridViewConfiguredPaths.MouseDown += dataGridViewConfiguredPaths_MouseDown;
            dataGridViewConfiguredPaths.MouseMove += dataGridViewConfiguredPaths_MouseMove;
            dataGridViewConfiguredPaths.MouseUp += dataGridViewConfiguredPaths_MouseUp;
            dataGridViewConfiguredPaths.DragOver += dataGridViewConfiguredPaths_DragOver;
            dataGridViewConfiguredPaths.DragDrop += dataGridViewConfiguredPaths_DragDrop;
            InitializeConfiguredPathsScrollBar();

            _autoBackupCountdownTimer.Interval = TitleRefreshIntervalMilliseconds;
            _autoBackupCountdownTimer.Tick += autoBackupCountdownTimer_Tick;

            _backupProgressAnimationTimer.Tick +=
                backupProgressAnimationTimer_Tick;

            LoadSettings();

            if ((!DebugMode.Current.Enabled ||
                 !DebugMode.Current.SkipInitialDisclaimer) &&
                !_settings.InitialDisclaimerAccepted)
            {
                DialogResult disclaimerResult =
                    ModernConfirmationDialog.ShowInitialDisclaimer(
                        this);

                if (disclaimerResult != DialogResult.OK)
                {
                    _isExplicitExitRequested = true;
                    Shown += (sender, e) => Close();
                    return;
                }

                _settings.InitialDisclaimerAccepted = true;
                SettingsStorage.Save(
                    _settings);
            }

            if ((!DebugMode.Current.Enabled ||
                 !DebugMode.Current.SkipRecurringDataLossWarning) &&
                IsRecurringDataLossWarningDue())
            {
                DialogResult warningResult =
                    ModernConfirmationDialog.ShowRecurringDataLossWarning(
                        this,
                        _settings.AutoPurgeEnabled,
                        _settings.SourceCleanupEnabled);

                if (warningResult != DialogResult.OK)
                {
                    _isExplicitExitRequested = true;
                    Shown += (sender, e) => Close();
                    return;
                }

                _settings.LastDataLossWarningUtc =
                    DateTime.UtcNow;
                SettingsStorage.Save(
                    _settings);
            }

            _lastLoggedSettingsSnapshot =
                CloneSettings(
                    _settings);

            _isApplyingWindowSettings = true;

            try
            {
                ApplyMainWindowHeightForThreeRows();
                ApplyWindowSettings();
            }
            finally
            {
                _isApplyingWindowSettings = false;
            }

            RefreshConfiguredPaths();
            RestartAutoBackupCountdown();
        }
        private void BeginBackupProgress(
            BackupPathPair pair)
        {
            ReportBackupProgress(
                pair,
                new BackupFileProgress(
                    BackupFileProgressArea.Source,
                    1,
                    true));
        }

        private void ReportBackupProgress(
            BackupPathPair pair,
            BackupFileProgress progress)
        {
            if (InvokeRequired)
            {
                BeginInvoke(
                    new Action(
                        () => ReportBackupProgress(
                            pair,
                            progress)));
                return;
            }

            string pairKey =
                SettingsStorage.CreatePairKey(
                    pair.SourceDirectory,
                    pair.TargetDirectory);

            if (!_backupProgressByPair.TryGetValue(
                    pairKey,
                    out BackupProgressDisplayState? state))
            {
                state =
                    new BackupProgressDisplayState();

                _backupProgressByPair[pairKey] =
                    state;
            }

            state.Area = progress.Area;
            state.Percentage =
                Math.Max(
                    state.Percentage,
                    progress.Percentage);
            state.IsIndeterminate =
                progress.IsIndeterminate;

            if (!_backupProgressAnimationTimer.Enabled)
            {
                _backupProgressAnimationTimer.Start();
            }

            InvalidateBackupProgressRow(pair);
            RefreshAutoBackupTimerColumn();
        }

        private void EndBackupProgress(
            BackupPathPair pair)
        {
            if (InvokeRequired)
            {
                BeginInvoke(
                    new Action(
                        () => EndBackupProgress(pair)));
                return;
            }

            string pairKey =
                SettingsStorage.CreatePairKey(
                    pair.SourceDirectory,
                    pair.TargetDirectory);

            _backupProgressByPair.Remove(pairKey);

            if (_backupProgressByPair.Count == 0)
            {
                _backupProgressAnimationTimer.Stop();
            }

            InvalidateBackupProgressRow(pair);
            RefreshAutoBackupTimerColumn();
        }

        private void backupProgressAnimationTimer_Tick(
            object? sender,
            EventArgs e)
        {
            foreach (BackupProgressDisplayState state in
                     _backupProgressByPair.Values)
            {
                if (state.IsIndeterminate)
                {
                    state.PulseOffset =
                        (state.PulseOffset + 12) % 240;
                }
            }

            dataGridViewConfiguredPaths.Invalidate();
        }

        private bool TryGetBackupProgressForRow(
            int rowIndex,
            out BackupProgressDisplayState? state)
        {
            state = null;

            if (rowIndex < 0 ||
                rowIndex >= _settings.BackupPathPairs.Count)
            {
                return false;
            }

            BackupPathPair pair =
                _settings.BackupPathPairs[rowIndex];

            string pairKey =
                SettingsStorage.CreatePairKey(
                    pair.SourceDirectory,
                    pair.TargetDirectory);

            return _backupProgressByPair.TryGetValue(
                pairKey,
                out state);
        }

        private void InvalidateBackupProgressRow(
            BackupPathPair pair)
        {
            int rowIndex =
                _settings.BackupPathPairs.IndexOf(pair);

            if (rowIndex < 0 ||
                rowIndex >= dataGridViewConfiguredPaths.Rows.Count)
            {
                return;
            }

            dataGridViewConfiguredPaths.InvalidateRow(
                rowIndex);
        }

        private void PaintBackupProgressCell(
            DataGridViewCellPaintingEventArgs e,
            BackupProgressDisplayState state)
        {
            e.PaintBackground(
                e.CellBounds,
                true);

            Rectangle contentBounds =
                Rectangle.Inflate(
                    e.CellBounds,
                    -1,
                    -1);

            if (state.IsIndeterminate)
            {
                int pulseWidth =
                    Math.Max(
                        24,
                        contentBounds.Width / 4);

                int travelWidth =
                    contentBounds.Width +
                    pulseWidth;

                int pulseLeft =
                    contentBounds.Left -
                    pulseWidth +
                    (state.PulseOffset %
                     Math.Max(1, travelWidth));

                Rectangle pulseBounds =
                    new Rectangle(
                        pulseLeft,
                        contentBounds.Top,
                        pulseWidth,
                        contentBounds.Height);

                using SolidBrush pulseBrush =
                    new SolidBrush(
                        ModernTheme.BackupProgressPulseColor);

                e.Graphics.FillRectangle(
                    pulseBrush,
                    Rectangle.Intersect(
                        contentBounds,
                        pulseBounds));
            }
            else
            {
                int fillWidth =
                    (int)Math.Round(
                        contentBounds.Width *
                        state.Percentage /
                        100D);

                if (fillWidth > 0)
                {
                    using SolidBrush progressBrush =
                        new SolidBrush(
                            ModernTheme.BackupProgressFillColor);

                    e.Graphics.FillRectangle(
                        progressBrush,
                        new Rectangle(
                            contentBounds.Left,
                            contentBounds.Top,
                            fillWidth,
                            contentBounds.Height));
                }
            }

            e.Paint(
                e.CellBounds,
                DataGridViewPaintParts.Border |
                DataGridViewPaintParts.ContentForeground |
                DataGridViewPaintParts.ErrorIcon |
                DataGridViewPaintParts.Focus);

            e.Handled = true;
        }

        private void dataGridViewConfiguredPaths_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }

            string columnName =
                dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (e.RowIndex >= 0 &&
                columnName == "ColumnConfiguredSourceSettings" &&
                e.RowIndex < _settings.BackupPathPairs.Count)
            {
                e.ToolTipText =
                    GetConfiguredSettingsToolTipText(
                        _settings.BackupPathPairs[e.RowIndex]);
                return;
            }

            e.ToolTipText =
                GetConfiguredPathActionColumnToolTipText(
                    e.ColumnIndex);
        }
        private string GetConfiguredPathActionColumnToolTipText(int columnIndex)
        {
            string columnName = dataGridViewConfiguredPaths.Columns[columnIndex].Name;

            if (columnName == "ColumnConfiguredSourceBrowse")
            {
                return "Browse source directory";
            }

            if (columnName == "ColumnConfiguredSourceSettings")
            {
                return "Backup pair settings";
            }

            if (columnName == "ColumnConfiguredSourceExclusions")
            {
                return "Edit excluded source paths";
            }

            if (columnName == "ColumnConfiguredTargetBrowse")
            {
                return "Browse target directory";
            }

            return string.Empty;
        }

        private string GetConfiguredSettingsToolTipText(
            BackupPathPair pair)
        {
            bool retentionActive =
                IsRetentionActive(pair);
            bool sourceCleanupActive =
                IsSourceCleanupActive(pair);

            List<string> lines =
                new List<string>
                {
                    "Backup pair settings",
                    string.Empty,
                    $"Retention: {(retentionActive ? "Active" : "Inactive")}"
                };

            if (retentionActive)
            {
                if (pair.RetentionKeepLastEnabled)
                {
                    lines.Add(
                        $"Keep last backups: {pair.RetentionKeepLastCount}");
                }

                if (pair.RetentionKeepDaysEnabled)
                {
                    lines.Add(
                        $"Keep backups for: {pair.RetentionKeepDaysCount} days");
                }

                if (pair.RetentionKeepLastEnabled &&
                    pair.RetentionKeepDaysEnabled)
                {
                    string retentionMode =
                        string.Equals(
                            pair.RetentionMode,
                            BackupHelper.RetentionModeAll,
                            StringComparison.OrdinalIgnoreCase)
                            ? "AND"
                            : "OR";

                    lines.Add(
                        $"Retention mode: {retentionMode}");
                }

                string retentionExclusions =
                    pair.RetentionExcludedTags != null &&
                    pair.RetentionExcludedTags.Any(tag =>
                        !string.IsNullOrWhiteSpace(tag))
                        ? string.Join(
                            ", ",
                            pair.RetentionExcludedTags.Where(tag =>
                                !string.IsNullOrWhiteSpace(tag)))
                        : "None";

                lines.Add(
                    $"Retention exclusions: {retentionExclusions}");
            }

            lines.Add(string.Empty);
            lines.Add(
                $"Source cleanup: {(sourceCleanupActive ? "Active" : "Inactive")}");

            if (sourceCleanupActive)
            {
                string cleanupRules =
                    pair.SourceCleanupFileExtensions != null &&
                    pair.SourceCleanupFileExtensions.Any(rule =>
                        !string.IsNullOrWhiteSpace(rule))
                        ? string.Join(
                            ", ",
                            pair.SourceCleanupFileExtensions.Where(rule =>
                                !string.IsNullOrWhiteSpace(rule)))
                        : "None";

                lines.Add(
                    $"Files to clean up: {cleanupRules}");

                if (string.Equals(
                        pair.SourceCleanupMode,
                        SourceCleanupService.ModeKeepAfterDate,
                        StringComparison.OrdinalIgnoreCase))
                {
                    lines.Add(
                        $"Files before: {pair.SourceCleanupKeepAfterDate}");
                }
                else
                {
                    lines.Add(
                        $"Keep newest files: {pair.SourceCleanupKeepLastCount}");
                }
            }

            return string.Join(
                Environment.NewLine,
                lines);
        }

        private bool IsRetentionActive(
            BackupPathPair pair)
        {
            return _settings.AutoPurgeEnabled &&
                (pair.RetentionKeepLastEnabled ||
                 pair.RetentionKeepDaysEnabled);
        }

        private bool IsSourceCleanupActive(
            BackupPathPair pair)
        {
            return _settings.SourceCleanupEnabled &&
                pair.SourceCleanupEnabled &&
                pair.SourceCleanupFileExtensions != null &&
                pair.SourceCleanupFileExtensions.Any(rule =>
                    !string.IsNullOrWhiteSpace(rule));
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_startMinimizedToSystray && (_settings.MinimizeToSystray || _settings.CloseToSystray))
            {
                StartMinimizedToSystray();
            }

            if (_settings.AutoUpdateCheck)
            {
                _ = CheckForUpdatesOnStartupAsync();
            }
        }
        private async Task CheckForUpdatesOnStartupAsync()
        {
            VersionHelperGitResult result = await VersionHelperGit.CheckForUpdateAsync(ApplicationVersionHelper.GetApplicationVersionText());

            if (IsDisposed)
            {
                return;
            }

            if (!result.CanConnectToGitHub)
            {
                return;
            }

            if (!result.UpdateAvailable)
            {
                return;
            }

            DialogResult dialogResult = ModernConfirmationDialog.Show(
                this,
                "Update available",
                "Update available: " + result.LatestVersion + Environment.NewLine + Environment.NewLine +
                "Do you want to open the GitHub download page?");

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(result.DownloadUrl))
            {
                return;
            }

            OpenUrl(result.DownloadUrl);
        }


        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch (Exception exception)
            {
                ModernMessageDialog.Show(
                    this,
                    "Error",
                    $"The link could not be opened:{Environment.NewLine}{url}{Environment.NewLine}{Environment.NewLine}{exception.Message}");
            }
        }

        private void StartMinimizedToSystray()
        {
            RefreshNotifyIconText();

            ShowInTaskbar = false;
            notifyIconMain.Visible = true;
            Hide();
        }
        private void InitializeModernTitleBar()
        {
            _modernTitleBarPanel = new Panel
            {
                Name = "panelModernTitleBar",
                Dock = DockStyle.Top,
                Height = ModernTheme.TitleBarHeight,
                BackColor = ModernTheme.TitleBarBackColor
            };

            PictureBox pictureBoxModernTitleIcon = new PictureBox
            {
                Name = "pictureBoxModernTitleIcon",
                Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            _modernTitleLabel = new Label
            {
                Name = "labelModernTitle",
                Text = _baseWindowTitle,
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(ClientSize.Width - 102, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular)
            };

            Button buttonModernMinimize = CreateModernTitleBarButton("buttonModernMinimize", "Minimize", new Point(ClientSize.Width - 72, 0));
            buttonModernMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonModernMinimize.Click += (sender, e) => WindowState = FormWindowState.Minimized;

            Button buttonModernClose = CreateModernTitleBarButton("buttonModernClose", "Close", new Point(ClientSize.Width - 36, 0));
            buttonModernClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonModernClose.MouseEnter += (sender, e) => buttonModernClose.BackColor = ModernTheme.CloseButtonHoverColor;
            buttonModernClose.MouseLeave += (sender, e) => buttonModernClose.BackColor = ModernTheme.TitleBarBackColor;
            buttonModernClose.Click += (sender, e) => Close();

            _modernTitleBarPanel.MouseDown += ModernTitleBar_MouseDown;
            pictureBoxModernTitleIcon.MouseDown += ModernTitleBar_MouseDown;
            _modernTitleLabel.MouseDown += ModernTitleBar_MouseDown;

            _modernTitleBarPanel.Controls.Add(pictureBoxModernTitleIcon);
            _modernTitleBarPanel.Controls.Add(_modernTitleLabel);
            _modernTitleBarPanel.Controls.Add(buttonModernMinimize);
            _modernTitleBarPanel.Controls.Add(buttonModernClose);

            buttonModernMinimize.BringToFront();
            buttonModernClose.BringToFront();

            _modernTitleBarPanel.Resize += (sender, e) =>
            {
                buttonModernClose.Location = new Point(_modernTitleBarPanel.ClientSize.Width - ModernTheme.TitleBarButtonSize.Width, 0);
                buttonModernMinimize.Location = new Point(_modernTitleBarPanel.ClientSize.Width - (ModernTheme.TitleBarButtonSize.Width * 2), 0);

                if (_modernTitleLabel != null)
                {
                    _modernTitleLabel.Size = new Size(_modernTitleBarPanel.ClientSize.Width - 102, ModernTheme.TitleBarHeight);
                }

                buttonModernMinimize.Invalidate();
                buttonModernClose.Invalidate();
            };

            Controls.Add(_modernTitleBarPanel);
            _modernTitleBarPanel.BringToFront();
        }
        protected override void WndProc(ref Message m)
        {
            const int wmNchittest = 0x84;
            const int htClient = 1;
            const int htLeft = 10;
            const int htRight = 11;
            const int htTop = 12;
            const int htTopLeft = 13;
            const int htTopRight = 14;
            const int htBottom = 15;
            const int htBottomLeft = 16;
            const int htBottomRight = 17;
            const int resizeBorderSize = 6;

            base.WndProc(ref m);

            if (m.Msg != wmNchittest)
            {
                return;
            }

            if ((int)m.Result != htClient)
            {
                return;
            }

            Point cursorPosition = PointToClient(Cursor.Position);
            Control? controlAtCursor = GetChildAtPoint(cursorPosition);

            if (controlAtCursor is Button)
            {
                return;
            }

            bool isLeft = cursorPosition.X <= resizeBorderSize;
            bool isRight = cursorPosition.X >= ClientSize.Width - resizeBorderSize;
            bool isTop = cursorPosition.Y <= resizeBorderSize;
            bool isBottom = cursorPosition.Y >= ClientSize.Height - resizeBorderSize;

            if (isTop && isLeft)
            {
                m.Result = htTopLeft;
                return;
            }

            if (isTop && isRight)
            {
                m.Result = htTopRight;
                return;
            }

            if (isBottom && isLeft)
            {
                m.Result = htBottomLeft;
                return;
            }

            if (isBottom && isRight)
            {
                m.Result = htBottomRight;
                return;
            }

            if (isLeft)
            {
                m.Result = htLeft;
                return;
            }

            if (isRight)
            {
                m.Result = htRight;
                return;
            }

            if (isTop)
            {
                m.Result = htTop;
                return;
            }

            if (isBottom)
            {
                m.Result = htBottom;
            }
        }
        private Button CreateModernTitleBarButton(string name, string text, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = string.Empty,
                Tag = text,
                Size = ModernTheme.TitleBarButtonSize,
                Location = location,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            button.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using Pen pen = new Pen(ModernTheme.TextColor, 1.4F)
                {
                    StartCap = System.Drawing.Drawing2D.LineCap.Square,
                    EndCap = System.Drawing.Drawing2D.LineCap.Square
                };

                if (button.Tag?.ToString() == "Minimize")
                {
                    int y = button.ClientRectangle.Top + button.ClientRectangle.Height / 2 + 5;
                    e.Graphics.DrawLine(pen, 13, y, 23, y);
                }

                if (button.Tag?.ToString() == "Close")
                {
                    e.Graphics.DrawLine(pen, 13, 11, 23, 21);
                    e.Graphics.DrawLine(pen, 23, 11, 13, 21);
                }
            };

            return button;
        }
        private void ModernTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            const int wmNclbuttondown = 0xA1;
            const int htCaption = 0x2;

            ReleaseCapture();
            SendMessage(Handle, wmNclbuttondown, htCaption, 0);
        }
        private void InitializeMainToolbar()
        {
            menuStrip1.Visible = false;
            labelConfiguredPaths.Visible = false;

            int toolbarTop = 44;
            int buttonSize = 32;
            int buttonSpacing = 6;
            int left = 12;

            dataGridViewConfiguredPaths.Location = new Point(12, 88);
            dataGridViewConfiguredPaths.Size = new Size(ClientSize.Width - 24, ClientSize.Height - dataGridViewConfiguredPaths.Top - 12);

            buttonBackup.Size = new Size(buttonBackup.Width, buttonSize);
            buttonBackup.Location = new Point(ClientSize.Width - buttonBackup.Width - 12 - ModernTheme.DataGridViewScrollBarSize, toolbarTop);
            buttonBackup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _mainToolTip.SetToolTip(buttonBackup, "Start backup");

            Button buttonExit = CreateToolbarButton("buttonExit", string.Empty, "Exit", "Exit", new Point(left, toolbarTop));
            buttonExit.Click += exitToolStripMenuItem_Click;
            left += buttonSize + buttonSpacing;

            Button buttonAddConfiguredPath = CreateToolbarButton("buttonAddConfiguredPath", "+", string.Empty, "Add backup path", new Point(left, toolbarTop));
            buttonAddConfiguredPath.Click += buttonAddConfiguredPath_Click;
            left += buttonSize + buttonSpacing;

            Button buttonRemoveConfiguredPath = CreateToolbarButton("buttonRemoveConfiguredPath", "−", string.Empty, "Remove selected backup path", new Point(left, toolbarTop));
            buttonRemoveConfiguredPath.Click += buttonRemoveConfiguredPath_Click;
            left += buttonSize + buttonSpacing;

            Button buttonMoveConfiguredPathUp = CreateToolbarButton("buttonMoveConfiguredPathUp", "↑", string.Empty, "Move selected backup path up", new Point(left, toolbarTop));
            buttonMoveConfiguredPathUp.Click += buttonMoveConfiguredPathUp_Click;
            left += buttonSize + buttonSpacing;

            Button buttonMoveConfiguredPathDown = CreateToolbarButton("buttonMoveConfiguredPathDown", "↓", string.Empty, "Move selected backup path down", new Point(left, toolbarTop));
            buttonMoveConfiguredPathDown.Click += buttonMoveConfiguredPathDown_Click;
            left += buttonSize + buttonSpacing;

            Button buttonModernSettings = CreateToolbarButton("buttonModernSettings", string.Empty, "Settings", "Settings", new Point(left, toolbarTop));
            buttonModernSettings.Click += (sender, e) =>
            {
                using ModernSettings form = new ModernSettings(_settings);

                DialogResult settingsResult =
                    form.ShowDialog(this);

                if (settingsResult == DialogResult.OK)
                {
                    _settings = form.ResultSettings;
                    SaveSettings();
                    RestartAutoBackupCountdown();
                }

                RefreshConfiguredPaths();
            };
            left += buttonSize + buttonSpacing;

            Button buttonAbout = CreateToolbarButton("buttonAbout", "?", string.Empty, "About EasyVersionBackup", new Point(left, toolbarTop));
            buttonAbout.Click += helpToolStripMenuItem_Click;
            left += buttonSize + buttonSpacing;

            _activeDataLossWarningLabel = new Label
            {
                Name = "labelActiveDataLossWarning",
                Text = "Retention/Cleanup active - Data loss possible!",
                AutoSize = false,
                Location = new Point(left, toolbarTop),
                Size = new Size(286, buttonSize + 1),
                BackColor = Color.Red,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(
                    ModernTheme.FontFamilyName,
                    ModernTheme.DefaultFontSize,
                    FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 4, 0, 0),
                Visible = false
            };

            _mainToolTip.SetToolTip(
                _activeDataLossWarningLabel,
                "WARNING: Retention and Source Cleanup are" +
                Environment.NewLine +
                "experimental features." +
                Environment.NewLine +
                Environment.NewLine +
                "Software defects, incorrect configuration, or" +
                Environment.NewLine +
                "incorrect source and destination paths can cause" +
                Environment.NewLine +
                "permanent and irreversible data loss." +
                Environment.NewLine +
                Environment.NewLine +
                "Their use is strongly discouraged and should be" +
                Environment.NewLine +
                "limited to experimental purposes." +
                Environment.NewLine +
                Environment.NewLine +
                "Do not use these features for important or" +
                Environment.NewLine +
                "irreplaceable data." +
                Environment.NewLine +
                Environment.NewLine +
                "Before using them, always ensure that complete," +
                Environment.NewLine +
                "verified, and recoverable backups exist in a" +
                Environment.NewLine +
                "separate backup solution that EasyVersionBackup" +
                Environment.NewLine +
                "cannot modify or delete.");

            Controls.Add(buttonExit);
            Controls.Add(buttonAddConfiguredPath);
            Controls.Add(buttonRemoveConfiguredPath);
            Controls.Add(buttonMoveConfiguredPathUp);
            Controls.Add(buttonMoveConfiguredPathDown);
            Controls.Add(buttonModernSettings);
            Controls.Add(buttonAbout);
            Controls.Add(_activeDataLossWarningLabel);

            _debugModeWarningLabel = new Label
            {
                Name = "labelDebugModeWarning",
                Text = "Safety mode: Off",
                AutoSize = true,
                Location = new Point(
                    _activeDataLossWarningLabel.Left,
                    _activeDataLossWarningLabel.Top + 8),
                BackColor = Color.Transparent,
                ForeColor = ModernTheme.DisabledTextColor,
                BorderStyle = BorderStyle.None,
                Font = new Font(
                    ModernTheme.FontFamilyName,
                    ModernTheme.DefaultFontSize - 1,
                    FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            _mainToolTip.SetToolTip(
                _debugModeWarningLabel,
                "Debug Mode is active." +
                Environment.NewLine +
                "One or more user-facing safety warnings or confirmations are disabled." +
                Environment.NewLine +
                "Technical deletion protections remain active.");

            Controls.Add(_debugModeWarningLabel);

            buttonExit.BringToFront();
            buttonAddConfiguredPath.BringToFront();
            buttonRemoveConfiguredPath.BringToFront();
            buttonMoveConfiguredPathUp.BringToFront();
            buttonMoveConfiguredPathDown.BringToFront();
            buttonModernSettings.BringToFront();
            buttonAbout.BringToFront();
            _activeDataLossWarningLabel.BringToFront();
            buttonBackup.BringToFront();

            if (_modernTitleBarPanel != null)
            {
                _modernTitleBarPanel.BringToFront();
            }
        }
        private void MoveConfiguredPathRow(int sourceRowIndex, int targetRowIndex)
        {
            if (sourceRowIndex == targetRowIndex)
            {
                return;
            }

            if (sourceRowIndex < 0 || sourceRowIndex >= _settings.BackupPathPairs.Count)
            {
                return;
            }

            if (targetRowIndex < 0 || targetRowIndex >= _settings.BackupPathPairs.Count)
            {
                return;
            }

            dataGridViewConfiguredPaths.EndEdit();
            SyncEnabledPairsFromGrid();

            BackupPathPair pair = _settings.BackupPathPairs[sourceRowIndex];

            _settings.BackupPathPairs.RemoveAt(sourceRowIndex);
            _settings.BackupPathPairs.Insert(targetRowIndex, pair);

            SaveSettings();
            RefreshConfiguredPaths();

            if (targetRowIndex >= 0 && targetRowIndex < dataGridViewConfiguredPaths.Rows.Count)
            {
                dataGridViewConfiguredPaths.ClearSelection();
                dataGridViewConfiguredPaths.Rows[targetRowIndex].Selected = true;
                dataGridViewConfiguredPaths.CurrentCell = dataGridViewConfiguredPaths.Rows[targetRowIndex].Cells["ColumnConfiguredSourceDirectory"];
            }
        }
        private void dataGridViewConfiguredPaths_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (dataGridViewConfiguredPaths.Tag is not Tuple<int, Point> dragInfo)
            {
                return;
            }

            Rectangle dragRectangle = new Rectangle(
                dragInfo.Item2.X - SystemInformation.DragSize.Width / 2,
                dragInfo.Item2.Y - SystemInformation.DragSize.Height / 2,
                SystemInformation.DragSize.Width,
                SystemInformation.DragSize.Height);

            if (dragRectangle.Contains(e.Location))
            {
                return;
            }

            dataGridViewConfiguredPaths.DoDragDrop(dragInfo.Item1, DragDropEffects.Move);
        }
        private void dataGridViewConfiguredPaths_MouseUp(object? sender, MouseEventArgs e)
        {
            dataGridViewConfiguredPaths.Tag = null;
        }
        private void dataGridViewConfiguredPaths_DragOver(object? sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(int)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            Point clientPoint = dataGridViewConfiguredPaths.PointToClient(new Point(e.X, e.Y));
            DataGridView.HitTestInfo hitTestInfo = dataGridViewConfiguredPaths.HitTest(clientPoint.X, clientPoint.Y);

            e.Effect = hitTestInfo.RowIndex >= 0 && hitTestInfo.RowIndex < _settings.BackupPathPairs.Count
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }
        private void dataGridViewConfiguredPaths_DragDrop(object? sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(int)))
            {
                return;
            }

            int sourceRowIndex = (int)e.Data.GetData(typeof(int))!;
            Point clientPoint = dataGridViewConfiguredPaths.PointToClient(new Point(e.X, e.Y));
            DataGridView.HitTestInfo hitTestInfo = dataGridViewConfiguredPaths.HitTest(clientPoint.X, clientPoint.Y);

            if (hitTestInfo.RowIndex < 0 || hitTestInfo.RowIndex >= _settings.BackupPathPairs.Count)
            {
                return;
            }

            MoveConfiguredPathRow(sourceRowIndex, hitTestInfo.RowIndex);
            dataGridViewConfiguredPaths.Tag = null;
        }
        private void dataGridViewConfiguredPaths_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                dataGridViewConfiguredPaths.Tag = null;
                return;
            }

            DataGridView.HitTestInfo hitTestInfo = dataGridViewConfiguredPaths.HitTest(e.X, e.Y);

            if (hitTestInfo.RowIndex < 0 || hitTestInfo.RowIndex >= _settings.BackupPathPairs.Count)
            {
                dataGridViewConfiguredPaths.Tag = null;
                return;
            }

            dataGridViewConfiguredPaths.Tag = Tuple.Create(hitTestInfo.RowIndex, new Point(e.X, e.Y));
        }
        private void buttonMoveConfiguredPathDown_Click(object? sender, EventArgs e)
        {
            if (dataGridViewConfiguredPaths.CurrentRow == null)
            {
                return;
            }

            MoveConfiguredPathRow(dataGridViewConfiguredPaths.CurrentRow.Index, dataGridViewConfiguredPaths.CurrentRow.Index + 1);
        }
        private void buttonMoveConfiguredPathUp_Click(object? sender, EventArgs e)
        {
            if (dataGridViewConfiguredPaths.CurrentRow == null)
            {
                return;
            }

            MoveConfiguredPathRow(dataGridViewConfiguredPaths.CurrentRow.Index, dataGridViewConfiguredPaths.CurrentRow.Index - 1);
        }
        private Button CreateToolbarButton(string name, string text, string iconType, string toolTipText, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = text,
                Size = ModernTheme.MainToolbarButtonSize,
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 2),
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            if (!string.IsNullOrWhiteSpace(toolTipText))
            {
                _mainToolTip.SetToolTip(button, toolTipText);
            }

            if (!string.IsNullOrWhiteSpace(iconType))
            {
                button.Paint += (sender, e) =>
                {
                    if (iconType == "Exit")
                    {
                        DrawToolbarExitIcon(e.Graphics, button.ClientRectangle);
                    }

                    if (iconType == "Settings")
                    {
                        DrawToolbarSettingsIcon(e.Graphics, button.ClientRectangle);
                    }
                };
            }

            return button;
        }
        private void DrawToolbarExitIcon(Graphics graphics, Rectangle bounds)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(ModernTheme.TextColor, 1.2F)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Square,
                EndCap = System.Drawing.Drawing2D.LineCap.Square,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
            };

            using SolidBrush brush = new SolidBrush(ModernTheme.TextColor);

            int iconLeft = bounds.Left - 1;
            int iconTop = bounds.Top + 7;
            int iconBottom = bounds.Top + 25;
            int iconCenterY = bounds.Top + 16;

            graphics.DrawLine(pen, iconLeft + 10, iconTop + 1, iconLeft + 17, iconTop + 1);
            graphics.DrawLine(pen, iconLeft + 10, iconTop + 1, iconLeft + 10, iconCenterY - 2);
            graphics.DrawLine(pen, iconLeft + 10, iconCenterY + 3, iconLeft + 10, iconBottom - 1);
            graphics.DrawLine(pen, iconLeft + 10, iconBottom - 1, iconLeft + 17, iconBottom - 1);

            Point[] doorPoints =
            {
        new Point(iconLeft + 18, iconTop),
        new Point(iconLeft + 25, iconTop + 3),
        new Point(iconLeft + 25, iconBottom - 3),
        new Point(iconLeft + 18, iconBottom)
    };

            graphics.FillPolygon(brush, doorPoints);

            graphics.DrawLine(pen, iconLeft + 6, iconCenterY, iconLeft + 15, iconCenterY);
            graphics.DrawLine(pen, iconLeft + 12, iconCenterY - 2, iconLeft + 15, iconCenterY);
            graphics.DrawLine(pen, iconLeft + 12, iconCenterY + 2, iconLeft + 15, iconCenterY);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        private void DrawToolbarSettingsIcon(Graphics graphics, Rectangle bounds)
        {
            ModernTheme.DrawSettingsIcon(graphics, bounds, ModernTheme.TextColor, 1.2F);
        }
        private void buttonAddConfiguredPath_Click(object? sender, EventArgs e)
        {
            SyncEnabledPairsFromGrid();

            BackupPathPair pair = new BackupPathPair
            {
                IsEnabled = true,
                SourceDirectory = string.Empty,
                TargetDirectory = string.Empty,
                Versioning = _settings.DefaultVersioning,
                IgnoreCopyErrors = _settings.IgnoreCopyErrors,
                SkipDialogs = false,
                RetentionKeepLastEnabled = false,
                RetentionKeepLastCount = 10,
                RetentionKeepDaysEnabled = false,
                RetentionKeepDaysCount = 14,
                RetentionMode = BackupHelper.RetentionModeAny,
                RetentionExcludedTags = new List<string>(),
                ExcludedPaths = new List<string>()
            };

            _settings.BackupPathPairs.Add(pair);

            SaveSettings();
            RefreshConfiguredPaths();

            int rowIndex = dataGridViewConfiguredPaths.Rows.Count - 1;

            if (rowIndex >= 0)
            {
                dataGridViewConfiguredPaths.ClearSelection();
                dataGridViewConfiguredPaths.Rows[rowIndex].Selected = true;
                dataGridViewConfiguredPaths.CurrentCell = dataGridViewConfiguredPaths.Rows[rowIndex].Cells["ColumnConfiguredSourceDirectory"];
                dataGridViewConfiguredPaths.BeginEdit(true);
            }
        }

        private void buttonRemoveConfiguredPath_Click(object? sender, EventArgs e)
        {
            if (dataGridViewConfiguredPaths.CurrentRow == null)
            {
                return;
            }

            int rowIndex = dataGridViewConfiguredPaths.CurrentRow.Index;

            if (rowIndex < 0 || rowIndex >= _settings.BackupPathPairs.Count)
            {
                return;
            }

            BackupPathPair pair = _settings.BackupPathPairs[rowIndex];

            if (ConfiguredPathContainsData(pair))
            {
                DialogResult result = ModernConfirmationDialog.Show(
                    this,
                    "Remove path",
                    "The selected path contains configured data. Do you really want to remove it?");

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            _settings.BackupPathPairs.RemoveAt(rowIndex);
            _settings.LastUsedVersionsByPair.Remove(pairKey);
            _settings.BackupStatusesByPair.Remove(pairKey);
            _nextAutoBackupRunsByPair.Remove(pairKey);

            SaveSettings();
            RefreshConfiguredPaths();
            RestartAutoBackupCountdown();
        }
        private void LoadSettings()
        {
            _settings = SettingsStorage.Load();
            BackupLogger.SetLogLevel(_settings.LogLevel);
            BackupLogger.CleanupOldLogFiles(30);

            foreach (BackupPathPair pair in _settings.BackupPathPairs)
            {
                if (string.IsNullOrWhiteSpace(pair.Versioning))
                {
                    pair.Versioning = _settings.DefaultVersioning;
                }
            }

            if (!string.IsNullOrWhiteSpace(
                SettingsStorage.LastLoadErrorMessage))
            {
                Shown += (sender, e) =>
                {
                    ModernMessageDialog.Show(
                        this,
                        "Settings",
                        SettingsStorage.LastLoadErrorMessage);
                };
            }
        }

        private void helpToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using AboutForm form = new AboutForm();
            form.ShowDialog(this);
        }
        private bool ConfiguredPathContainsData(BackupPathPair pair)
        {
            if (!string.IsNullOrWhiteSpace(pair.SourceDirectory))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(pair.TargetDirectory))
            {
                return true;
            }

            if (pair.ExcludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath)))
            {
                return true;
            }

            string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            if (_settings.LastUsedVersionsByPair.ContainsKey(pairKey))
            {
                return true;
            }

            if (_settings.BackupStatusesByPair.ContainsKey(pairKey))
            {
                return true;
            }

            return false;
        }
        private void ApplyWindowSettings()
        {
            StartPosition = FormStartPosition.Manual;

            if (_settings.MainWindowWidth > 0 && _settings.MainWindowHeight > 0)
            {
                Size = new Size(_settings.MainWindowWidth, _settings.MainWindowHeight);
            }

            if (_settings.MainWindowLeft >= 0 && _settings.MainWindowTop >= 0)
            {
                Location = new Point(_settings.MainWindowLeft, _settings.MainWindowTop);
            }
            else
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
        }
        private void ApplyMainWindowHeightForThreeRows()
        {
            int gridHeight = dataGridViewConfiguredPaths.ColumnHeadersHeight + (dataGridViewConfiguredPaths.RowTemplate.Height * 3) + 2;
            int clientHeight = dataGridViewConfiguredPaths.Top + gridHeight + 12;

            MinimumSize = SizeFromClientSize(new Size(500, clientHeight));

            if (_settings.MainWindowWidth <= 0 || _settings.MainWindowHeight <= 0)
            {
                ClientSize = new Size(ClientSize.Width, clientHeight);
                return;
            }

            if (Width < MinimumSize.Width || Height < MinimumSize.Height)
            {
                Size = new Size(
                    Math.Max(Width, MinimumSize.Width),
                    Math.Max(Height, MinimumSize.Height));
            }
        }

        private void SaveWindowSettings()
        {
            if (_isApplyingWindowSettings || WindowState == FormWindowState.Minimized)
            {
                return;
            }

            Rectangle bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            if (_settings.MainWindowWidth == bounds.Width &&
                _settings.MainWindowHeight == bounds.Height &&
                _settings.MainWindowLeft == bounds.Left &&
                _settings.MainWindowTop == bounds.Top)
            {
                return;
            }

            _settings.MainWindowWidth = bounds.Width;
            _settings.MainWindowHeight = bounds.Height;
            _settings.MainWindowLeft = bounds.Left;
            _settings.MainWindowTop = bounds.Top;

            SaveSettings();
        }

        private void SaveSettings()
        {
            BackupLogger.SetLogLevel(
                _settings.LogLevel);

            SettingsStorage.Save(
                _settings);

            SystemLogger.WriteSettingsChanges(
                _lastLoggedSettingsSnapshot,
                _settings);

            _lastLoggedSettingsSnapshot =
                CloneSettings(
                    _settings);
        }

        private void UpdateActiveDataLossWarning()
        {
            if (_activeDataLossWarningLabel == null)
            {
                return;
            }

            bool retentionActive =
                _settings.AutoPurgeEnabled &&
                _settings.BackupPathPairs.Any(pair =>
                    pair.RetentionKeepLastEnabled ||
                    pair.RetentionKeepDaysEnabled);

            bool sourceCleanupActive =
                _settings.SourceCleanupEnabled &&
                _settings.BackupPathPairs.Any(pair =>
                    pair.SourceCleanupEnabled);

            bool debugModeActive =
                DebugMode.Current.Enabled;

            _activeDataLossWarningLabel.Visible =
                !debugModeActive &&
                !DebugMode.Current.HideActiveDataLossWarning &&
                (retentionActive ||
                 sourceCleanupActive);

            if (_debugModeWarningLabel != null)
            {
                _debugModeWarningLabel.Visible =
                    debugModeActive;

                if (_debugModeWarningLabel.Visible)
                {
                    _debugModeWarningLabel.BringToFront();
                }
            }

            if (_activeDataLossWarningLabel.Visible)
            {
                _activeDataLossWarningLabel.BringToFront();
            }
        }

        private bool IsRecurringDataLossWarningDue()
        {
            if (!_settings.AutoPurgeEnabled &&
                !_settings.SourceCleanupEnabled)
            {
                return false;
            }

            int warningIntervalDays =
                Math.Max(
                    1,
                    _settings.DataLossWarningIntervalDays);

            if (!_settings.LastDataLossWarningUtc.HasValue)
            {
                return true;
            }

            return DateTime.UtcNow -
                _settings.LastDataLossWarningUtc.Value.ToUniversalTime() >=
                TimeSpan.FromDays(
                    warningIntervalDays);
        }

        private static AppSettings CloneSettings(
            AppSettings settings)
        {
            string json =
                JsonSerializer.Serialize(
                    settings);

            return JsonSerializer.Deserialize<AppSettings>(
                       json) ??
                   new AppSettings();
        }

        private void RefreshConfiguredPaths()
        {
            UpdateActiveDataLossWarning();

            _isRefreshingConfiguredPaths = true;

            try
            {
                foreach (DataGridViewRow existingRow in dataGridViewConfiguredPaths.Rows)
                {
                    if (existingRow.Cells["ColumnConfiguredBackupInfo"].Value is Image oldImage)
                    {
                        oldImage.Dispose();
                    }
                }

                dataGridViewConfiguredPaths.Rows.Clear();

                if (_settings.BackupPathPairs.Count == 0)
                {
                    buttonBackup.Enabled = false;
                    return;
                }

                foreach (BackupPathPair pair in _settings.BackupPathPairs)
                {
                    int rowIndex = dataGridViewConfiguredPaths.Rows.Add();
                    DataGridViewRow row = dataGridViewConfiguredPaths.Rows[rowIndex];

                    row.Cells["ColumnConfiguredIsEnabled"].Value = pair.IsEnabled;
                    row.Cells["ColumnConfiguredAutoBackupTimer"].Value = string.Empty;
                    row.Cells["ColumnConfiguredSourceDirectory"].Value = pair.SourceDirectory;
                    row.Cells["ColumnConfiguredSourceBrowse"].ToolTipText = "Browse source directory";
                    row.Cells["ColumnConfiguredSourceSettings"].ToolTipText =
                        GetConfiguredSettingsToolTipText(pair);
                    row.Cells["ColumnConfiguredSourceExclusions"].ToolTipText = "Edit excluded source paths";
                    row.Cells["ColumnConfiguredTargetDirectory"].Value = pair.TargetDirectory;
                    row.Cells["ColumnConfiguredTargetBrowse"].ToolTipText = "Browse target directory";
                    row.Cells["ColumnConfiguredBackupInfo"].Value = GetBackupInfoIcon(pair);
                    row.Cells["ColumnConfiguredBackupInfo"].ToolTipText = GetBackupInfoToolTipText(pair);

                    row.Tag = new List<string>(pair.ExcludedPaths);
                    UpdateConfiguredExclusionButtonStyle(rowIndex);
                }

                buttonBackup.Enabled = _settings.BackupPathPairs.Any(p => p.IsEnabled);
                RefreshAutoBackupTimerColumn();
            }
            finally
            {
                _isRefreshingConfiguredPaths = false;
                UpdateConfiguredPathsScrollBar();
            }
        }

        private void InitializeConfiguredPathsScrollBar()
        {
            _configuredPathsVerticalScrollBar.ScrollValueChanged += configuredPathsVerticalScrollBar_ScrollValueChanged;

            dataGridViewConfiguredPaths.Scroll += dataGridViewConfiguredPaths_Scroll;
            dataGridViewConfiguredPaths.RowsAdded += dataGridViewConfiguredPaths_ContentChanged;
            dataGridViewConfiguredPaths.RowsRemoved += dataGridViewConfiguredPaths_ContentChanged;
            dataGridViewConfiguredPaths.Resize += dataGridViewConfiguredPaths_ContentChanged;
            dataGridViewConfiguredPaths.MouseWheel += dataGridViewConfiguredPaths_MouseWheel;

            Controls.Add(_configuredPathsVerticalScrollBar);
            _configuredPathsVerticalScrollBar.BringToFront();

            UpdateConfiguredPathsScrollBarBounds();
        }

        private void UpdateConfiguredPathsScrollBarBounds()
        {
            int scrollBarSize = ModernTheme.DataGridViewScrollBarSize;
            int rightMargin = 12;
            int bottomMargin = 12;
            int gridRight = ClientSize.Width - rightMargin - scrollBarSize;
            int gridBottom = ClientSize.Height - bottomMargin;

            dataGridViewConfiguredPaths.Size = new Size(
                Math.Max(100, gridRight - dataGridViewConfiguredPaths.Left),
                Math.Max(60, gridBottom - dataGridViewConfiguredPaths.Top));

            int visibleTableHeight = dataGridViewConfiguredPaths.ColumnHeadersHeight;

            foreach (DataGridViewRow row in dataGridViewConfiguredPaths.Rows)
            {
                if (row.Visible)
                {
                    visibleTableHeight += row.Height;
                }
            }

            visibleTableHeight = Math.Min(dataGridViewConfiguredPaths.Height, Math.Max(dataGridViewConfiguredPaths.ColumnHeadersHeight, visibleTableHeight));

            _configuredPathsVerticalScrollBar.Location = new Point(dataGridViewConfiguredPaths.Right, dataGridViewConfiguredPaths.Top);
            _configuredPathsVerticalScrollBar.Size = new Size(scrollBarSize, visibleTableHeight);
            _configuredPathsVerticalScrollBar.BringToFront();

            UpdateConfiguredPathsScrollBar();
        }

        private void UpdateConfiguredPathsScrollBar()
        {
            if (_isUpdatingConfiguredPathsScrollBar)
            {
                return;
            }

            _isUpdatingConfiguredPathsScrollBar = true;

            try
            {
                int rowCount = dataGridViewConfiguredPaths.Rows.Count;
                int visibleRowCount = Math.Max(1, dataGridViewConfiguredPaths.DisplayedRowCount(false));
                int maximumFirstDisplayedRowIndex = Math.Max(0, rowCount - visibleRowCount);
                int firstDisplayedRowIndex = GetConfiguredPathsFirstDisplayedScrollingRowIndex();

                _configuredPathsVerticalScrollBar.Minimum = 0;
                _configuredPathsVerticalScrollBar.Maximum = maximumFirstDisplayedRowIndex;
                _configuredPathsVerticalScrollBar.LargeChange = visibleRowCount;
                _configuredPathsVerticalScrollBar.Visible = true;
                _configuredPathsVerticalScrollBar.Value = Math.Min(maximumFirstDisplayedRowIndex, Math.Max(0, firstDisplayedRowIndex));
                _configuredPathsVerticalScrollBar.Invalidate();
            }
            finally
            {
                _isUpdatingConfiguredPathsScrollBar = false;
            }
        }

        private int GetConfiguredPathsFirstDisplayedScrollingRowIndex()
        {
            try
            {
                return dataGridViewConfiguredPaths.FirstDisplayedScrollingRowIndex;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        private void configuredPathsVerticalScrollBar_ScrollValueChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingConfiguredPathsScrollBar || dataGridViewConfiguredPaths.Rows.Count == 0)
            {
                return;
            }

            try
            {
                dataGridViewConfiguredPaths.FirstDisplayedScrollingRowIndex = Math.Min(_configuredPathsVerticalScrollBar.Value, dataGridViewConfiguredPaths.Rows.Count - 1);
            }
            catch (InvalidOperationException exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Configured paths could not be scrolled: " +
                    exception.Message);
            }
        }

        private void dataGridViewConfiguredPaths_Scroll(object? sender, ScrollEventArgs e)
        {
            UpdateConfiguredPathsScrollBar();
        }

        private void dataGridViewConfiguredPaths_ContentChanged(object? sender, EventArgs e)
        {
            UpdateConfiguredPathsScrollBar();
        }

        private void dataGridViewConfiguredPaths_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!_configuredPathsVerticalScrollBar.Visible)
            {
                return;
            }

            _configuredPathsVerticalScrollBar.Value += e.Delta > 0 ? -1 : 1;
        }

        private void exitToolStripMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            _isExplicitExitRequested = true;
            notifyIconMain.Visible = false;
            Close();
        }

        private void generalToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using ModernSettings form = new ModernSettings(_settings);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _settings = form.ResultSettings;
                SaveSettings();
                RefreshConfiguredPaths();
                RestartAutoBackupCountdown();
            }
        }

        private async void buttonBackup_Click(
            object sender,
            EventArgs e)
        {
            if (_isBackupRunning)
            {
                return;
            }

            _isBackupRunning = true;
            buttonBackup.Enabled = false;

            try
            {
                SyncEnabledPairsFromGrid();

                List<BackupPathPair> activePairs =
                    _settings.BackupPathPairs
                        .Where(pair =>
                            pair.IsEnabled &&
                            !string.IsNullOrWhiteSpace(
                                pair.SourceDirectory) &&
                            !string.IsNullOrWhiteSpace(
                                pair.TargetDirectory))
                        .ToList();

                if (activePairs.Count == 0)
                {
                    ModernMessageDialog.Show(
                        this,
                        "Error",
                        "No active paths selected.");
                    return;
                }

                List<BackupPathPair> runnablePairs =
                    new List<BackupPathPair>();

                int prevalidationFailedBackups = 0;
                int prevalidationCanceledBackups = 0;

                Dictionary<BackupPathPair, string> versionsByPair =
                    new Dictionary<BackupPathPair, string>();

                foreach (BackupPathPair pair in activePairs)
                {
                    string? pathValidationError =
                        BackupHelper.GetBackupPathValidationError(
                            pair,
                            activePairs);

                    if (!string.IsNullOrWhiteSpace(
                        pathValidationError))
                    {
                        SetBackupStatus(
                            pair,
                            BackupPathStatus.StatusError,
                            pathValidationError);

                        if (!pair.SkipDialogs)
                        {
                            ModernMessageDialog.Show(
                                this,
                                "Error",
                                $"{pair.SourceDirectory}{Environment.NewLine}{Environment.NewLine}{pathValidationError}");
                        }

                        prevalidationFailedBackups++;
                        continue;
                    }

                    if (!Directory.Exists(pair.SourceDirectory))
                    {
                        string errorMessage =
                            $"Source directory not found: {pair.SourceDirectory}";

                        SetBackupStatus(
                            pair,
                            BackupPathStatus.StatusError,
                            errorMessage);

                        if (!pair.SkipDialogs)
                        {
                            ModernMessageDialog.Show(
                                this,
                                "Error",
                                $"Source directory not found:{Environment.NewLine}{pair.SourceDirectory}");
                        }

                        prevalidationFailedBackups++;
                        continue;
                    }

                    if (pair.SourceCleanupEnabled &&
                        !SourceCleanupService.TryValidateSettings(
                            pair,
                            out _,
                            out string sourceCleanupValidationError))
                    {
                        SetBackupStatus(
                            pair,
                            BackupPathStatus.StatusError,
                            sourceCleanupValidationError);

                        if (!pair.SkipDialogs)
                        {
                            ModernMessageDialog.Show(
                                this,
                                "Error",
                                $"{pair.SourceDirectory}{Environment.NewLine}{Environment.NewLine}{sourceCleanupValidationError}");
                        }

                        prevalidationFailedBackups++;
                        continue;
                    }

                    if (!EnsureTargetDirectoryExistsForManualBackup(
                            pair,
                            out bool targetPreparationFailed))
                    {
                        if (targetPreparationFailed)
                        {
                            prevalidationFailedBackups++;
                        }
                        else
                        {
                            prevalidationCanceledBackups++;
                        }

                        continue;
                    }

                    try
                    {
                        versionsByPair[pair] =
                            VersionHelper.GetSuggestedVersion(
                                _settings,
                                pair);

                        runnablePairs.Add(pair);
                    }
                    catch (Exception exception)
                    {
                        SetBackupStatus(
                            pair,
                            BackupPathStatus.StatusError,
                            exception.Message);

                        if (!pair.SkipDialogs)
                        {
                            ModernMessageDialog.Show(
                                this,
                                "Error",
                                $"Version could not be determined:{Environment.NewLine}{pair.SourceDirectory}{Environment.NewLine}{Environment.NewLine}{exception.Message}");
                        }

                        prevalidationFailedBackups++;
                    }
                }

                SaveSettings();
                RefreshBackupInfoColumn();

                if (runnablePairs.Count == 0)
                {
                    return;
                }

                Dictionary<BackupPathPair, string> tagsByPair =
                    runnablePairs.ToDictionary(
                        pair => pair,
                        pair => string.Empty);

                Dictionary<BackupPathPair, bool> ignoreErrorsByPair =
                    runnablePairs.ToDictionary(
                        pair => pair,
                        pair => pair.IgnoreCopyErrors);

                List<BackupPathPair> dialogPairs =
                    runnablePairs
                        .Where(pair => !pair.SkipDialogs)
                        .ToList();

                if (dialogPairs.Count > 0)
                {
                    List<BackupVersionItem> versionItems =
                        dialogPairs
                            .Select(pair =>
                                new BackupVersionItem
                                {
                                    SourceDirectory =
                                        pair.SourceDirectory,
                                    TargetDirectory =
                                        pair.TargetDirectory,
                                    SourceName =
                                        new DirectoryInfo(
                                            pair.SourceDirectory).Name,
                                    Version =
                                        versionsByPair[pair],
                                    Tag =
                                        tagsByPair[pair]
                                })
                            .ToList();

                    using VersionInputForm versionForm =
                        new VersionInputForm(
                            versionItems,
                            _settings.IgnoreCopyErrors,
                            _settings.Tags ?? new List<string>(),
                            new Size(
                                _settings.BackupVersionDialogWidth,
                                _settings.BackupVersionDialogHeight));

                    DialogResult versionDialogResult =
                        versionForm.ShowDialog(this);

                    if (versionForm.DialogSize.Width > 0 &&
                        versionForm.DialogSize.Height > 0)
                    {
                        _settings.BackupVersionDialogWidth =
                            versionForm.DialogSize.Width;

                        _settings.BackupVersionDialogHeight =
                            versionForm.DialogSize.Height;

                        SaveSettings();
                    }

                    if (versionDialogResult != DialogResult.OK)
                    {
                        return;
                    }

                    _settings.IgnoreCopyErrors =
                        versionForm.IgnoreCopyErrors;

                    for (int i = 0;
                         i < dialogPairs.Count;
                         i++)
                    {
                        BackupPathPair dialogPair =
                            dialogPairs[i];

                        string selectedVersion =
                            versionForm.ResultItems[i].Version;

                        versionsByPair[dialogPair] =
                            selectedVersion;

                        tagsByPair[dialogPair] =
                            versionForm.ResultItems[i].Tag;

                        ignoreErrorsByPair[dialogPair] =
                            versionForm.IgnoreCopyErrors;

                    }

                    SaveSettings();
                }

                int successfulBackups = 0;
                int failedBackups = prevalidationFailedBackups;
                int canceledBackups = prevalidationCanceledBackups;
                int skippedPaths = 0;
                int purgedBackups = 0;

                List<string> destinationActions =
                    new List<string>();

                foreach (BackupPathPair pair in runnablePairs)
                {
                    BackupPairRunResult runResult =
                        await ExecuteBackupPairAsync(
                            pair,
                            versionsByPair[pair],
                            tagsByPair[pair],
                            ignoreErrorsByPair[pair],
                            "MANUAL");

                    if (runResult.Successful)
                    {
                        successfulBackups++;
                        skippedPaths +=
                            runResult.SkippedPaths;
                        purgedBackups +=
                            runResult.PurgedBackups;

                        destinationActions.Add(
                            runResult.DestinationAction);

                        string selectedVersion =
                            versionsByPair[pair];

                        if (!VersionPatternHelper.IsDatePattern(
                                pair.Versioning) &&
                            !string.Equals(
                                pair.Versioning,
                                selectedVersion,
                                StringComparison.Ordinal))
                        {
                            pair.Versioning =
                                selectedVersion;
                        }
                    }
                    else if (runResult.Canceled)
                    {
                        canceledBackups++;

                        if (!pair.SkipDialogs &&
                            runResult.Error != null)
                        {
                            BackupDialogHelper.ShowBackupCanceledBecauseDestinationExists(
                                this,
                                runResult.Error.Message);
                        }
                    }
                    else
                    {
                        failedBackups++;

                        if (!pair.SkipDialogs &&
                            runResult.Error != null)
                        {
                            ModernMessageDialog.Show(
                                this,
                                "Error",
                                $"Backup failed:{Environment.NewLine}{pair.SourceDirectory}{Environment.NewLine}{Environment.NewLine}{runResult.Error.Message}");
                        }
                    }
                }

                notifyIconMain.Visible = true;
                notifyIconMain.BalloonTipTitle =
                    "EasyVersionBackup";

                bool onlyCanceled =
                    successfulBackups == 0 &&
                    failedBackups == 0 &&
                    canceledBackups > 0;

                if (onlyCanceled)
                {
                    notifyIconMain.BalloonTipText =
                        canceledBackups == 1
                            ? "Backup canceled. No files were created or deleted."
                            : $"Backups canceled: {canceledBackups}. No files were created or deleted.";

                    notifyIconMain.BalloonTipIcon =
                        ToolTipIcon.Info;
                }
                else
                {
                    notifyIconMain.BalloonTipText =
                        $"Backup finished. Successful: {successfulBackups}, failed: {failedBackups}, canceled: {canceledBackups}, skipped files: {skippedPaths}.{BackupHelper.FormatDestinationActionSummary(destinationActions)}{BackupHelper.FormatRetentionSummary(purgedBackups)}";

                    notifyIconMain.BalloonTipIcon =
                        failedBackups > 0
                            ? ToolTipIcon.Warning
                            : canceledBackups > 0
                                ? ToolTipIcon.Info
                                : ToolTipIcon.None;
                }

                notifyIconMain.ShowBalloonTip(5000);
            }
            finally
            {
                _isBackupRunning = false;
                buttonBackup.Enabled =
                    _settings.BackupPathPairs.Any(
                        pair => pair.IsEnabled);

                RefreshAutoBackupTimerColumn();
            }
        }

        private bool EnsureTargetDirectoryExistsForManualBackup(
            BackupPathPair pair,
            out bool failed)
        {
            failed = false;

            if (Directory.Exists(pair.TargetDirectory))
            {
                return true;
            }

            DialogResult result = ModernConfirmationDialog.Show(
                this,
                "Create target directory",
                $"Target directory does not exist:{Environment.NewLine}{pair.TargetDirectory}{Environment.NewLine}{Environment.NewLine}Do you want to create it?");

            if (result != DialogResult.Yes)
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(pair.TargetDirectory);
                return true;
            }
            catch (Exception exception)
            {
                failed = true;
                SetBackupStatus(
                    pair,
                    BackupPathStatus.StatusError,
                    exception.Message);
                SaveSettings();
                RefreshBackupInfoColumn();

                ModernMessageDialog.Show(
                    this,
                    "Error",
                    $"Target directory could not be created:{Environment.NewLine}{pair.TargetDirectory}{Environment.NewLine}{Environment.NewLine}{exception.Message}");

                return false;
            }
        }

        private async Task<(BackupFileOperationResult FileResult, string DestinationAction)> ExecuteBackupAsync(
            BackupPathPair pair,
            string version,
            string tag,
            bool ignoreCopyErrors,
            Action<BackupFileProgress>? progressHandler)
        {
            string sourceName =
                new DirectoryInfo(pair.SourceDirectory).Name;

            string versionedName =
                VersionHelper.BuildVersionedName(
                    sourceName,
                    version);

            if (!string.IsNullOrWhiteSpace(tag))
            {
                versionedName += "_" + tag.Trim();
            }

            string conflictHandling =
                BackupHelper.NormalizeDestinationConflictHandling(
                    _settings.BackupDestinationConflictHandling);

            bool createZip = _settings.ZipDestinationFiles;
            string destinationPath = createZip
                ? Path.Combine(
                    pair.TargetDirectory,
                    $"{versionedName}.zip")
                : Path.Combine(
                    pair.TargetDirectory,
                    versionedName);

            string destinationAction =
                BackupHelper.DestinationActionCreated;

            bool overwriteExisting = false;

            if (BackupHelper.DestinationExists(destinationPath))
            {
                if (conflictHandling ==
                    BackupHelper.DestinationConflictAsk)
                {
                    DialogResult result =
                        BackupDialogHelper.ShowDestinationConflictDialog(
                            this,
                            destinationPath,
                            out string selectedConflictAction);

                    if (result != DialogResult.OK)
                    {
                        throw new OperationCanceledException(
                            destinationPath);
                    }

                    conflictHandling =
                        BackupHelper.NormalizeDestinationConflictHandling(
                            selectedConflictAction);
                }

                if (conflictHandling ==
                    BackupHelper.DestinationConflictAppend)
                {
                    destinationPath =
                        BackupHelper.GetNumberedDestinationPath(
                            destinationPath);

                    destinationAction =
                        BackupHelper.DestinationActionAppended;
                }
                else if (conflictHandling ==
                    BackupHelper.DestinationConflictCancel)
                {
                    throw new OperationCanceledException(
                        destinationPath);
                }
                else
                {
                    if (BackupHelper.IsProtectedByRetentionExcludedTag(
                        destinationPath,
                        pair))
                    {
                        throw new OperationCanceledException(
                            destinationPath);
                    }

                    overwriteExisting = true;
                    destinationAction =
                        BackupHelper.DestinationActionOverwritten;
                }
            }

            BackupFileOperationResult fileResult =
                await Task.Run(
                    () => BackupFileService.CreateBackup(
                        pair.SourceDirectory,
                        destinationPath,
                        createZip,
                        overwriteExisting,
                        pair.ExcludedPaths,
                        ignoreCopyErrors,
                        ShowFileErrorActionDialog,
                        progressHandler));

            BackupLogger.WriteLine(
                $"BACKUP CREATED | source=\"{pair.SourceDirectory}\" | target=\"{pair.TargetDirectory}\" | backup=\"{fileResult.DestinationFileName}\" | destinationAction={destinationAction} | skippedFiles={fileResult.SkippedCount}");

            return (fileResult, destinationAction);
        }


        private async Task<BackupPairRunResult> ExecuteBackupPairAsync(
            BackupPathPair pair,
            string version,
            string tag,
            bool ignoreCopyErrors,
            string backupKind)
        {
            BackupPairRunResult runResult =
                new BackupPairRunResult();

            string logResult = "FAILED";

            BackupLogger.WriteLine(
                $"{backupKind} BACKUP START | source=\"{pair.SourceDirectory}\" | target=\"{pair.TargetDirectory}\"");

            try
            {
                BeginBackupProgress(
                    pair);

                List<string> sourceCleanupPreviewPaths =
                    new List<string>();

                if (_settings.SourceCleanupEnabled &&
                    pair.SourceCleanupEnabled)
                {
                    sourceCleanupPreviewPaths =
                        SourceCleanupService.GetDeletePreviewPaths(
                            pair);
                }

                List<string> retentionPreviewPaths =
                    new List<string>();

                if (ShouldRunRetentionForPair(pair))
                {
                    retentionPreviewPaths =
                        BackupHelper.GetRetentionPurgePreviewPathsForNextBackup(
                            pair,
                            _settings.ZipDestinationFiles);
                }

                bool applySourceCleanup =
                    sourceCleanupPreviewPaths.Count > 0;

                if (applySourceCleanup)
                {
                    DialogResult sourceCleanupConfirmationResult =
                        TryConfirmSourceCleanupWarningDialogue(
                            pair,
                            sourceCleanupPreviewPaths);

                    if (sourceCleanupConfirmationResult ==
                        DialogResult.Cancel)
                    {
                        SetBackupStatus(
                            pair,
                            BackupPathStatus.StatusWarning,
                            "Backup canceled by user before any files were created or deleted.");

                        runResult.Canceled = true;
                        logResult = "CANCELED";
                        return runResult;
                    }

                    applySourceCleanup =
                        sourceCleanupConfirmationResult ==
                        DialogResult.Yes;
                }

                bool applyRetention =
                    retentionPreviewPaths.Count > 0;

                if (applyRetention)
                {
                    DialogResult retentionConfirmationResult =
                        TryConfirmRetentionWarningDialogue(
                            pair,
                            retentionPreviewPaths);

                    if (retentionConfirmationResult ==
                        DialogResult.Cancel)
                    {
                        SetBackupStatus(
                            pair,
                            BackupPathStatus.StatusWarning,
                            "Backup canceled by user before any files were created or deleted.");

                        runResult.Canceled = true;
                        logResult = "CANCELED";
                        return runResult;
                    }

                    applyRetention =
                        retentionConfirmationResult ==
                        DialogResult.Yes;
                }

                (
                    BackupFileOperationResult fileResult,
                    string destinationAction
                ) = await ExecuteBackupAsync(
                    pair,
                    version,
                    tag,
                    ignoreCopyErrors,
                    progress =>
                        ReportBackupProgress(
                            pair,
                            progress));

                string pairKey =
                    SettingsStorage.CreatePairKey(
                        pair.SourceDirectory,
                        pair.TargetDirectory);

                _settings.LastUsedVersionsByPair[pairKey] =
                    version;

                SourceCleanupResult sourceCleanupResult =
                    new SourceCleanupResult();

                string sourceCleanupErrorMessage =
                    string.Empty;

                if (applySourceCleanup &&
                    _settings.SourceCleanupEnabled &&
                    pair.SourceCleanupEnabled)
                {
                    ReportBackupProgress(
                        pair,
                        new BackupFileProgress(
                            BackupFileProgressArea.Source,
                            90,
                            true));

                    try
                    {
                        sourceCleanupResult =
                            await Task.Run(
                                () => SourceCleanupService.Apply(
                                    pair,
                                    sourceCleanupPreviewPaths));
                    }
                    catch (Exception cleanupException)
                    {
                        sourceCleanupErrorMessage =
                            cleanupException.Message;

                        BackupLogger.WriteLine(
                            $"SOURCE CLEANUP ERROR | source=\"{pair.SourceDirectory}\" | error=\"{cleanupException.Message}\"");
                    }
                }

                int purgedForPair = 0;
                List<string> purgedPaths =
                    new List<string>();

                if (applyRetention &&
                    ShouldRunRetentionForPair(pair))
                {
                    ReportBackupProgress(
                        pair,
                        new BackupFileProgress(
                            BackupFileProgressArea.Target,
                            97,
                            true));

                    List<string> actualPurgePaths =
                        BackupHelper.GetRetentionPurgePreviewPaths(
                            pair,
                            _settings.ZipDestinationFiles);

                    purgedForPair =
                        BackupHelper.ApplyRetention(
                            pair,
                            _settings.ZipDestinationFiles,
                            actualPurgePaths,
                            out purgedPaths);
                }
                else
                {
                    BackupHelper.ApplyRetentionDisabled(
                        out purgedPaths);
                }

                List<string> statusMessages =
                    new List<string>();

                if (fileResult.SkippedCount > 0)
                {
                    statusMessages.Add(
                        FormatSkippedFilesMessage(
                            fileResult.SkippedCount,
                            fileResult.SkippedPaths));
                }

                if (sourceCleanupResult.DeletedCount > 0)
                {
                    statusMessages.Add(
                        $"{sourceCleanupResult.DeletedCount} source file(s) deleted after backup.");
                }

                if (sourceCleanupResult.FailedPaths.Count > 0)
                {
                    statusMessages.Add(
                        $"{sourceCleanupResult.FailedPaths.Count} source file(s) could not be deleted.");
                }

                if (!string.IsNullOrWhiteSpace(
                    sourceCleanupErrorMessage))
                {
                    statusMessages.Add(
                        $"Source cleanup was not completed: {sourceCleanupErrorMessage}");
                }

                if (purgedForPair > 0)
                {
                    statusMessages.Add(
                        BackupHelper.FormatRetentionStatusMessage(
                            purgedPaths));
                }

                SetBackupStatus(
                    pair,
                    sourceCleanupResult.FailedPaths.Count == 0 &&
                    string.IsNullOrWhiteSpace(
                        sourceCleanupErrorMessage)
                        ? BackupPathStatus.StatusOk
                        : BackupPathStatus.StatusWarning,
                    string.Join(
                        Environment.NewLine +
                        Environment.NewLine,
                        statusMessages.Where(message =>
                            !string.IsNullOrWhiteSpace(
                                message))),
                    fileResult.DestinationFileName);

                runResult.Successful = true;
                runResult.SkippedPaths =
                    fileResult.SkippedCount;
                runResult.PurgedBackups =
                    purgedForPair;
                runResult.DestinationAction =
                    destinationAction;
                logResult = "SUCCESS";
            }
            catch (OperationCanceledException exception)
            {
                SetBackupStatus(
                    pair,
                    BackupPathStatus.StatusWarning,
                    BackupHelper.FormatBackupCanceledBecauseDestinationExistsMessage(
                        exception.Message));

                runResult.Canceled = true;
                runResult.Error = exception;
                logResult = "CANCELED";
            }
            catch (Exception exception)
            {
                SetBackupStatus(
                    pair,
                    BackupPathStatus.StatusError,
                    exception.Message);

                runResult.Error = exception;
                logResult = "FAILED";
            }
            finally
            {
                EndBackupProgress(
                    pair);

                BackupLogger.WriteLine(
                    $"{backupKind} BACKUP END | source=\"{pair.SourceDirectory}\" | target=\"{pair.TargetDirectory}\" | result={logResult}");

                SaveSettings();
                RefreshBackupInfoColumn();
            }

            return runResult;
        }

        private string FormatSkippedFilesMessage(
            int skippedFiles,
            List<string> skippedFileList)
        {
            if (skippedFileList.Count == 0)
            {
                return $"{skippedFiles} file(s) skipped.";
            }

            int skippedFolderCount =
                skippedFileList
                    .Select(filePath =>
                        Path.GetDirectoryName(filePath) ??
                        string.Empty)
                    .Where(directoryPath =>
                        !string.IsNullOrWhiteSpace(directoryPath))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

            string folderText =
                skippedFolderCount == 1
                    ? "1 folder"
                    : $"{skippedFolderCount} folders";

            return $"{skippedFiles} file(s) skipped in {folderText}:{Environment.NewLine}" +
                string.Join(
                    Environment.NewLine,
                    skippedFileList);
        }
        
        
        private BackupFileErrorAction ShowFileErrorActionDialog(
            string filePath,
            Exception exception)
        {
            if (InvokeRequired)
            {
                return (BackupFileErrorAction)Invoke(
                    new Func<BackupFileErrorAction>(
                        () => ShowFileErrorActionDialog(
                            filePath,
                            exception)));
            }

            using FileErrorDialog dialog =
                new FileErrorDialog(
                    filePath,
                    exception.Message);

            DialogResult result =
                dialog.ShowDialog(this);

            if (result == DialogResult.Retry)
            {
                return BackupFileErrorAction.Retry;
            }

            if (result == DialogResult.Ignore)
            {
                return BackupFileErrorAction.Skip;
            }

            if (result == DialogResult.Yes)
            {
                return BackupFileErrorAction.IgnoreAll;
            }

            return BackupFileErrorAction.Abort;
        }

        private async Task ExecuteAutomaticBackupAsync(
            List<BackupPathPair> automaticBackupPairs)
        {
            List<BackupPathPair> validPairs =
                automaticBackupPairs
                    .Where(pair =>
                        pair.IsEnabled &&
                        !string.IsNullOrWhiteSpace(
                            pair.SourceDirectory) &&
                        !string.IsNullOrWhiteSpace(
                            pair.TargetDirectory))
                    .ToList();

            if (validPairs.Count == 0)
            {
                return;
            }

            int successfulBackups = 0;
            int failedBackups = 0;
            int canceledBackups = 0;
            int skippedPaths = 0;
            int purgedBackups = 0;

            List<string> destinationActions =
                new List<string>();

            foreach (BackupPathPair pair in validPairs)
            {
                string? pathValidationError =
                    BackupHelper.GetBackupPathValidationError(
                        pair,
                        _settings.BackupPathPairs);

                if (!string.IsNullOrWhiteSpace(
                    pathValidationError))
                {
                    SetBackupStatus(
                        pair,
                        BackupPathStatus.StatusError,
                        pathValidationError);

                    SaveSettings();
                    RefreshBackupInfoColumn();
                    failedBackups++;
                    continue;
                }

                if (!Directory.Exists(
                    pair.SourceDirectory))
                {
                    SetBackupStatus(
                        pair,
                        BackupPathStatus.StatusError,
                        $"Source directory not found: {pair.SourceDirectory}");

                    SaveSettings();
                    RefreshBackupInfoColumn();
                    failedBackups++;
                    continue;
                }

                if (_settings.SourceCleanupEnabled &&
                    pair.SourceCleanupEnabled &&
                    !SourceCleanupService.TryValidateSettings(
                        pair,
                        out _,
                        out string sourceCleanupValidationError))
                {
                    SetBackupStatus(
                        pair,
                        BackupPathStatus.StatusError,
                        sourceCleanupValidationError);

                    SaveSettings();
                    RefreshBackupInfoColumn();
                    failedBackups++;
                    continue;
                }

                try
                {
                    if (!Directory.Exists(
                        pair.TargetDirectory))
                    {
                        Directory.CreateDirectory(
                            pair.TargetDirectory);
                    }

                    string automaticVersion =
                        VersionHelper.GetSuggestedVersion(
                            _settings,
                            pair);

                    BackupPairRunResult runResult =
                        await ExecuteBackupPairAsync(
                            pair,
                            automaticVersion,
                            string.Empty,
                            pair.IgnoreCopyErrors,
                            "AUTOMATIC");

                    if (runResult.Successful)
                    {
                        successfulBackups++;
                        skippedPaths +=
                            runResult.SkippedPaths;
                        purgedBackups +=
                            runResult.PurgedBackups;

                        destinationActions.Add(
                            runResult.DestinationAction);
                    }
                    else if (runResult.Canceled)
                    {
                        canceledBackups++;
                    }
                    else
                    {
                        failedBackups++;
                    }
                }
                catch (Exception exception)
                {
                    SetBackupStatus(
                        pair,
                        BackupPathStatus.StatusError,
                        exception.Message);

                    BackupLogger.WriteLine(
                        $"AUTOMATIC BACKUP ERROR | source=\"{pair.SourceDirectory}\" | target=\"{pair.TargetDirectory}\" | result=FAILED_BEFORE_START | error=\"{exception.Message}\"");

                    SaveSettings();
                    RefreshBackupInfoColumn();
                    failedBackups++;
                }
            }

            notifyIconMain.Visible = true;
            notifyIconMain.BalloonTipTitle =
                "EasyVersionBackup";

            notifyIconMain.BalloonTipText =
                successfulBackups > 0
                    ? $"Auto-Backup finished. Successful: {successfulBackups}, failed: {failedBackups}, canceled: {canceledBackups}, skipped files: {skippedPaths}.{BackupHelper.FormatDestinationActionSummary(destinationActions)}{BackupHelper.FormatRetentionSummary(purgedBackups)}"
                    : $"Auto-Backup failed. Successful: 0, failed: {failedBackups}, canceled: {canceledBackups}.";

            notifyIconMain.BalloonTipIcon =
                failedBackups > 0 ||
                successfulBackups == 0
                    ? ToolTipIcon.Warning
                    : ToolTipIcon.None;

            notifyIconMain.ShowBalloonTip(5000);
        }
        private async void autoBackupCountdownTimer_Tick(
            object? sender,
            EventArgs e)
        {
            RefreshAutoBackupTimerColumn();
            RefreshWindowTitleCountdown();
            RefreshNotifyIconText();

            if (!_settings.AutoBackupEnabled ||
                _isBackupRunning)
            {
                return;
            }

            DateTime now = DateTime.Now;

            List<BackupPathPair> duePairs =
                _settings.BackupPathPairs
                    .Where(pair =>
                        pair.IsEnabled &&
                        !string.IsNullOrWhiteSpace(
                            pair.SourceDirectory) &&
                        !string.IsNullOrWhiteSpace(
                            pair.TargetDirectory))
                    .Where(pair =>
                    {
                        string pairKey =
                            SettingsStorage.CreatePairKey(
                                pair.SourceDirectory,
                                pair.TargetDirectory);

                        if (!_nextAutoBackupRunsByPair.TryGetValue(
                            pairKey,
                            out DateTime nextRun))
                        {
                            _nextAutoBackupRunsByPair[pairKey] =
                                now.AddSeconds(
                                    GetAutoBackupIntervalSeconds(
                                        pair));

                            return false;
                        }

                        return now >= nextRun;
                    })
                    .ToList();

            if (duePairs.Count == 0)
            {
                return;
            }

            _isBackupRunning = true;
            buttonBackup.Enabled = false;

            try
            {
                await ExecuteAutomaticBackupAsync(
                    duePairs);
            }
            finally
            {
                DateTime nextBaseTime = DateTime.Now;

                foreach (BackupPathPair pair in duePairs)
                {
                    string pairKey =
                        SettingsStorage.CreatePairKey(
                            pair.SourceDirectory,
                            pair.TargetDirectory);

                    _nextAutoBackupRunsByPair[pairKey] =
                        nextBaseTime.AddSeconds(
                            GetAutoBackupIntervalSeconds(
                                pair));
                }

                _isBackupRunning = false;
                buttonBackup.Enabled =
                    _settings.BackupPathPairs.Any(
                        pair => pair.IsEnabled);

                RefreshAutoBackupTimerColumn();
                RefreshWindowTitleCountdown();
                RefreshNotifyIconText();
            }
        }
        private void RefreshAutoBackupTimerColumn()
        {
            DateTime now = DateTime.Now;

            for (int i = 0;
                 i < dataGridViewConfiguredPaths.Rows.Count &&
                 i < _settings.BackupPathPairs.Count;
                 i++)
            {
                DataGridViewRow row =
                    dataGridViewConfiguredPaths.Rows[i];

                BackupPathPair pair =
                    _settings.BackupPathPairs[i];

                string progressPairKey =
                    SettingsStorage.CreatePairKey(
                        pair.SourceDirectory,
                        pair.TargetDirectory);

                if (_backupProgressByPair.TryGetValue(
                        progressPairKey,
                        out BackupProgressDisplayState? progressState))
                {
                    row.Cells[
                        "ColumnConfiguredAutoBackupTimer"
                    ].Value =
                        $"{progressState.Percentage} %";

                    continue;
                }

                if (!_settings.AutoBackupEnabled ||
                    !pair.IsEnabled ||
                    string.IsNullOrWhiteSpace(
                        pair.SourceDirectory) ||
                    string.IsNullOrWhiteSpace(
                        pair.TargetDirectory))
                {
                    row.Cells[
                        "ColumnConfiguredAutoBackupTimer"
                    ].Value = string.Empty;

                    continue;
                }

                string pairKey =
                    SettingsStorage.CreatePairKey(
                        pair.SourceDirectory,
                        pair.TargetDirectory);

                if (!_nextAutoBackupRunsByPair.TryGetValue(
                    pairKey,
                    out DateTime nextRun))
                {
                    nextRun = now.AddSeconds(
                        GetAutoBackupIntervalSeconds(pair));

                    _nextAutoBackupRunsByPair[pairKey] =
                        nextRun;
                }

                TimeSpan remaining = nextRun - now;

                if (remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                row.Cells[
                    "ColumnConfiguredAutoBackupTimer"
                ].Value = FormatAutoBackupInterval(
                    remaining);
            }
        }
        private void RefreshWindowTitleCountdown()
        {
            if (!_settings.AutoBackupEnabled || !TryGetNextAutoBackupRun(out DateTime nextAutoBackupRun))
            {
                Text = _baseWindowTitle;

                if (_modernTitleLabel != null)
                {
                    _modernTitleLabel.Text = Text;
                }

                return;
            }

            TimeSpan remaining = nextAutoBackupRun - DateTime.Now;

            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            int totalSeconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
            Text = $"{_baseWindowTitle} (Backup in: {totalSeconds} sek)";

            if (_modernTitleLabel != null)
            {
                _modernTitleLabel.Text = Text;
            }
        }
        private string FormatAutoBackupInterval(
            TimeSpan interval)
        {
            int totalSeconds = Math.Max(
                0,
                (int)Math.Ceiling(
                    Math.Min(
                        interval.TotalSeconds,
                        int.MaxValue)));

            return AutoBackupIntervalHelper.Format(
                totalSeconds);
        }
        private int GetAutoBackupIntervalSeconds()
        {
            return Math.Max(
                1,
                _settings.AutoBackupIntervalSeconds);
        }

        private int GetAutoBackupIntervalSeconds(BackupPathPair pair)
        {
            if (pair.AutoBackupIntervalSeconds > 0)
            {
                return pair.AutoBackupIntervalSeconds;
            }

            return GetAutoBackupIntervalSeconds();
        }

        private bool TryGetNextAutoBackupRun(out DateTime nextAutoBackupRun)
        {
            nextAutoBackupRun = DateTime.MaxValue;

            foreach (BackupPathPair pair in _settings.BackupPathPairs)
            {
                if (!pair.IsEnabled ||
                    string.IsNullOrWhiteSpace(pair.SourceDirectory) ||
                    string.IsNullOrWhiteSpace(pair.TargetDirectory))
                {
                    continue;
                }

                string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

                if (!_nextAutoBackupRunsByPair.TryGetValue(pairKey, out DateTime pairNextRun))
                {
                    continue;
                }

                if (pairNextRun < nextAutoBackupRun)
                {
                    nextAutoBackupRun = pairNextRun;
                }
            }

            return nextAutoBackupRun != DateTime.MaxValue;
        }
        private void RestartAutoBackupCountdown()
        {
            _autoBackupCountdownTimer.Stop();
            _nextAutoBackupRunsByPair.Clear();

            if (!_settings.AutoBackupEnabled)
            {
                RefreshAutoBackupTimerColumn();
                RefreshWindowTitleCountdown();
                RefreshNotifyIconText();
                return;
            }

            DateTime now = DateTime.Now;

            foreach (BackupPathPair pair in _settings.BackupPathPairs)
            {
                if (!pair.IsEnabled ||
                    string.IsNullOrWhiteSpace(pair.SourceDirectory) ||
                    string.IsNullOrWhiteSpace(pair.TargetDirectory))
                {
                    continue;
                }

                string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
                _nextAutoBackupRunsByPair[pairKey] = now.AddSeconds(GetAutoBackupIntervalSeconds(pair));
            }

            RefreshAutoBackupTimerColumn();
            RefreshWindowTitleCountdown();
            RefreshNotifyIconText();

            if (_nextAutoBackupRunsByPair.Count > 0)
            {
                _autoBackupCountdownTimer.Start();
            }
        }



        private void RefreshBackupInfoColumn()
        {
            for (int i = 0;
                 i < dataGridViewConfiguredPaths.Rows.Count &&
                 i < _settings.BackupPathPairs.Count;
                 i++)
            {
                BackupPathPair pair =
                    _settings.BackupPathPairs[i];

                DataGridViewCell infoCell =
                    dataGridViewConfiguredPaths.Rows[i]
                        .Cells["ColumnConfiguredBackupInfo"];

                if (infoCell.Value is Image oldImage)
                {
                    oldImage.Dispose();
                }

                infoCell.Value = GetBackupInfoIcon(pair);
                infoCell.ToolTipText =
                    GetBackupInfoToolTipText(pair);
            }
        }
        private string GetBackupInfoToolTipText(BackupPathPair pair)
        {
            string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            if (!_settings.BackupStatusesByPair.TryGetValue(key, out BackupPathStatus? status))
            {
                return "Last Backup: -";
            }

            string text = $"Last Backup: {status.LastBackupDateTime}";

            if (!string.IsNullOrWhiteSpace(status.LastBackupFileName))
            {
                text += $"{Environment.NewLine}File: {status.LastBackupFileName}";
            }

            if (!string.IsNullOrWhiteSpace(status.LastBackupErrorMessage))
            {
                if (status.LastBackupStatus == BackupPathStatus.StatusError)
                {
                    text += $"{Environment.NewLine}Error: {status.LastBackupErrorMessage}";
                }
                else if (status.LastBackupStatus == BackupPathStatus.StatusWarning)
                {
                    text += $"{Environment.NewLine}{status.LastBackupErrorMessage}";
                }
            }

            return text;
        }
        private Color GetBackupInfoColor(BackupPathPair pair)
        {
            string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            if (!_settings.BackupStatusesByPair.TryGetValue(key, out BackupPathStatus? status))
            {
                return ModernTheme.BackupInfoDefaultColor;
            }

            if (status.LastBackupStatus == BackupPathStatus.StatusOk)
            {
                return ModernTheme.BackupInfoOkColor;
            }

            if (status.LastBackupStatus == BackupPathStatus.StatusWarning)
            {
                return ModernTheme.BackupInfoWarningColor;
            }

            if (status.LastBackupStatus == BackupPathStatus.StatusError)
            {
                return ModernTheme.BackupInfoErrorColor;
            }

            return ModernTheme.BackupInfoDefaultColor;
        }
        private Bitmap GetBackupInfoIcon(BackupPathPair pair)
        {
            Color color = GetBackupInfoColor(pair);
            Bitmap bitmap = new Bitmap(16, 16);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using SolidBrush brush = new SolidBrush(color);

            // circle size settings start
            graphics.FillEllipse(brush, 0, 0, 16, 16);
            // circle size settings end

            // "i" color settings start
            Color infoTextColor = ModernTheme.BackupInfoTextColor;
            // "i" color settings end

            using SolidBrush infoBrush = new SolidBrush(infoTextColor);

            // "i" dot position settings start
            graphics.FillEllipse(infoBrush, 7, 3, 2, 2);
            // "i" dot position settings end

            // "i" line position settings start
            graphics.FillRectangle(infoBrush, 7, 7, 2, 6);
            // "i" line position settings end

            return bitmap;
        }
        private void SetBackupStatus(
            BackupPathPair pair,
            string status,
            string errorMessage,
            string backupFileName = "")
        {
            string key = SettingsStorage.CreatePairKey(
                pair.SourceDirectory,
                pair.TargetDirectory);

            _settings.BackupStatusesByPair[key] =
                new BackupPathStatus
                {
                    LastBackupDateTime =
                        DateTime.Now.ToString(
                            "dd.MM.yyyy, HH:mm"),
                    LastBackupStatus = status,
                    LastBackupFileName =
                        string.Equals(
                            status,
                            BackupPathStatus.StatusError,
                            StringComparison.OrdinalIgnoreCase)
                            ? string.Empty
                            : backupFileName,
                    LastBackupErrorMessage = errorMessage
                };
        }

        private void InitializeBackupInfoColumn()
        {
            if (dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredBackupInfo"))
            {
                return;
            }

            DataGridViewImageColumn columnConfiguredBackupInfo = new DataGridViewImageColumn
            {
                HeaderText = "ℹ",
                Name = "ColumnConfiguredBackupInfo",
                ReadOnly = true,
                Width = 35,
                MinimumWidth = 35,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ImageLayout = DataGridViewImageCellLayout.Normal
            };

            int targetColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredTargetDirectory"].Index;
            dataGridViewConfiguredPaths.Columns.Insert(targetColumnIndex + 1, columnConfiguredBackupInfo);
        }
        private void InitializeAutoBackupTimerColumn()
        {
            if (dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredAutoBackupTimer"))
            {
                return;
            }

            DataGridViewTextBoxColumn columnConfiguredAutoBackupTimer = new DataGridViewTextBoxColumn
            {
                HeaderText = "Timer",
                Name = "ColumnConfiguredAutoBackupTimer",
                ReadOnly = true,
                Width = 60,
                MinimumWidth = 60,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            dataGridViewConfiguredPaths.Columns.Insert(1, columnConfiguredAutoBackupTimer);
        }
        private void SyncEnabledPairsFromGrid()
        {
            for (int i = 0;
                 i < _settings.BackupPathPairs.Count &&
                 i < dataGridViewConfiguredPaths.Rows.Count;
                 i++)
            {
                DataGridViewRow row =
                    dataGridViewConfiguredPaths.Rows[i];

                BackupPathPair pair =
                    _settings.BackupPathPairs[i];

                string oldPairKey = SettingsStorage.CreatePairKey(
                    pair.SourceDirectory,
                    pair.TargetDirectory);

                object? value =
                    row.Cells["ColumnConfiguredIsEnabled"].Value;

                bool isEnabled = false;

                if (value != null)
                {
                    bool.TryParse(
                        value.ToString(),
                        out isEnabled);
                }

                pair.IsEnabled = isEnabled;
                pair.SourceDirectory =
                    row.Cells["ColumnConfiguredSourceDirectory"]
                        .Value?.ToString()?.Trim() ??
                    string.Empty;

                pair.TargetDirectory =
                    row.Cells["ColumnConfiguredTargetDirectory"]
                        .Value?.ToString()?.Trim() ??
                    string.Empty;

                pair.ExcludedPaths =
                    row.Tag is List<string> excludedPaths
                        ? new List<string>(excludedPaths)
                        : new List<string>();

                string newPairKey = SettingsStorage.CreatePairKey(
                    pair.SourceDirectory,
                    pair.TargetDirectory);

                if (!string.Equals(
                    oldPairKey,
                    newPairKey,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MigratePairStateKey(
                        oldPairKey,
                        newPairKey);
                }
            }

            buttonBackup.Enabled =
                !_isBackupRunning &&
                _settings.BackupPathPairs.Any(
                    pair => pair.IsEnabled);

            RefreshAutoBackupTimerColumn();
            SaveSettings();
        }

        private void MigratePairStateKey(
            string oldPairKey,
            string newPairKey)
        {
            if (_settings.LastUsedVersionsByPair.TryGetValue(
                oldPairKey,
                out string? lastUsedVersion))
            {
                _settings.LastUsedVersionsByPair.Remove(
                    oldPairKey);

                _settings.LastUsedVersionsByPair[newPairKey] =
                    lastUsedVersion;
            }

            if (_settings.BackupStatusesByPair.TryGetValue(
                oldPairKey,
                out BackupPathStatus? status))
            {
                _settings.BackupStatusesByPair.Remove(
                    oldPairKey);

                _settings.BackupStatusesByPair[newPairKey] =
                    status;
            }

            if (_nextAutoBackupRunsByPair.TryGetValue(
                oldPairKey,
                out DateTime nextRun))
            {
                _nextAutoBackupRunsByPair.Remove(oldPairKey);
                _nextAutoBackupRunsByPair[newPairKey] = nextRun;
            }
        }

        private void dataGridViewConfiguredPaths_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewConfiguredPaths.IsCurrentCellDirty)
            {
                dataGridViewConfiguredPaths.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        private void InitializeConfiguredPathActionColumns()
        {
            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceBrowse"))
            {
                DataGridViewButtonColumn columnConfiguredSourceBrowse = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredSourceBrowse",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Browse source directory"
                };

                int sourceDirectoryColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceDirectory"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(sourceDirectoryColumnIndex + 1, columnConfiguredSourceBrowse);
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceSettings"))
            {
                DataGridViewButtonColumn columnConfiguredSourceSettings = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredSourceSettings",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Backup pair settings"
                };

                int sourceBrowseColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceBrowse"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(sourceBrowseColumnIndex + 1, columnConfiguredSourceSettings);
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceExclusions"))
            {
                DataGridViewButtonColumn columnConfiguredSourceExclusions = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredSourceExclusions",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Edit excluded source paths"
                };

                int sourceSettingsColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceSettings"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(sourceSettingsColumnIndex + 1, columnConfiguredSourceExclusions);
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredTargetBrowse"))
            {
                DataGridViewButtonColumn columnConfiguredTargetBrowse = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredTargetBrowse",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Browse target directory"
                };

                int targetDirectoryColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredTargetDirectory"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(targetDirectoryColumnIndex + 1, columnConfiguredTargetBrowse);
            }
        }



        private void dataGridViewConfiguredPaths_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (columnName == "ColumnConfiguredBackupInfo")
            {
                if (e.RowIndex >= _settings.BackupPathPairs.Count)
                {
                    return;
                }

                ShowBackupInfoDialog(_settings.BackupPathPairs[e.RowIndex]);
            }

            if (columnName == "ColumnConfiguredSourceBrowse")
            {
                string currentValue = dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredSourceDirectory"].Value?.ToString() ?? string.Empty;

                if (ModernFolderBrowserDialog.Show(this, "Select source directory", currentValue, out string selectedPath))
                {
                    dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredSourceDirectory"].Value = selectedPath;
                    SyncEnabledPairsFromGrid();
                }
            }

            if (columnName == "ColumnConfiguredSourceSettings")
            {
                if (e.RowIndex >= _settings.BackupPathPairs.Count)
                {
                    return;
                }

                SyncEnabledPairsFromGrid();

                BackupPathPair pair = _settings.BackupPathPairs[e.RowIndex];
                string previousVersioning = string.IsNullOrWhiteSpace(pair.Versioning)
                    ? _settings.DefaultVersioning
                    : pair.Versioning;

                using BackupPairSettingsDialog dialog = new BackupPairSettingsDialog(this, pair, _settings.DefaultVersioning, _settings.ZipDestinationFiles && _settings.AutoPurgeEnabled, _settings.SourceCleanupEnabled, _settings.Tags);

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string newVersioning = dialog.ResultVersioning;

                    if (!string.Equals(previousVersioning, newVersioning, StringComparison.OrdinalIgnoreCase))
                    {
                        string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
                        _settings.LastUsedVersionsByPair.Remove(key);
                    }

                    pair.Versioning = newVersioning;
                    pair.IgnoreCopyErrors = dialog.ResultIgnoreCopyErrors;
                    pair.SkipDialogs = dialog.ResultSkipDialogs;
                    pair.AutoBackupIntervalSeconds = dialog.ResultAutoBackupIntervalSeconds;
                    pair.RetentionKeepLastEnabled = dialog.ResultRetentionKeepLastEnabled;
                    pair.RetentionKeepLastCount = dialog.ResultRetentionKeepLastCount;
                    pair.RetentionKeepDaysEnabled = dialog.ResultRetentionKeepDaysEnabled;
                    pair.RetentionKeepDaysCount = dialog.ResultRetentionKeepDaysCount;
                    pair.RetentionMode = dialog.ResultRetentionMode;
                    pair.RetentionExcludedTags = new List<string>(dialog.ResultRetentionExcludedTags);
                    pair.SourceCleanupEnabled = dialog.ResultSourceCleanupEnabled;
                    pair.SourceCleanupRelativeDirectory = dialog.ResultSourceCleanupRelativeDirectory;
                    pair.SourceCleanupFileExtensions = new List<string>(dialog.ResultSourceCleanupFileExtensions);
                    pair.SourceCleanupMode = dialog.ResultSourceCleanupMode;
                    pair.SourceCleanupKeepLastCount = dialog.ResultSourceCleanupKeepLastCount;
                    pair.SourceCleanupKeepAfterDate = dialog.ResultSourceCleanupKeepAfterDate;

                    SaveSettings();
                    UpdateActiveDataLossWarning();
                    RestartAutoBackupCountdown();

                    DataGridViewCell settingsCell =
                        dataGridViewConfiguredPaths.Rows[e.RowIndex]
                            .Cells["ColumnConfiguredSourceSettings"];

                    settingsCell.ToolTipText =
                        GetConfiguredSettingsToolTipText(pair);

                    dataGridViewConfiguredPaths.InvalidateCell(
                        settingsCell);
                }
            }

            if (columnName == "ColumnConfiguredSourceExclusions")
            {
                List<string> excludedPaths = dataGridViewConfiguredPaths.Rows[e.RowIndex].Tag is List<string> rowExcludedPaths
                    ? new List<string>(rowExcludedPaths)
                    : new List<string>();

                if (ShowConfiguredExclusionsDialog(excludedPaths, out List<string> resultExcludedPaths))
                {
                    dataGridViewConfiguredPaths.Rows[e.RowIndex].Tag = resultExcludedPaths;
                    UpdateConfiguredExclusionButtonStyle(e.RowIndex);
                    SyncEnabledPairsFromGrid();
                }
            }

            if (columnName == "ColumnConfiguredTargetBrowse")
            {
                string currentValue = dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredTargetDirectory"].Value?.ToString() ?? string.Empty;

                if (ModernFolderBrowserDialog.Show(this, "Select target directory", currentValue, out string selectedPath))
                {
                    dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredTargetDirectory"].Value = selectedPath;
                    SyncEnabledPairsFromGrid();
                }
            }
        }

        private void ShowBackupInfoDialog(BackupPathPair pair)
        {
            string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
            string lastBackupFileName = _settings.BackupStatusesByPair.TryGetValue(key, out BackupPathStatus? status)
                ? status.LastBackupFileName
                : string.Empty;

            List<BackupLogEntry> logEntries = BackupLogger.ReadBackupPairEntries(
                pair.SourceDirectory,
                pair.TargetDirectory,
                lastBackupFileName,
                500);

            using ModernBackupInfoDialog dialog = new ModernBackupInfoDialog(
                this,
                GetBackupInfoToolTipText(pair),
                logEntries,
                new Size(_settings.BackupInfoDialogWidth, _settings.BackupInfoDialogHeight));

            dialog.ShowDialog(this);

            _settings.BackupInfoDialogWidth = dialog.DialogSize.Width;
            _settings.BackupInfoDialogHeight = dialog.DialogSize.Height;
            SaveSettings();
        }
        private void dataGridViewConfiguredPaths_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.Graphics == null)
            {
                return;
            }

            string columnName = dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (e.RowIndex == -1 && columnName == "ColumnConfiguredSourceSettings")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                DrawConfiguredSettingsIcon(e.Graphics, e.CellBounds, ModernTheme.TextColor, 1.8F);
                e.Handled = true;
                return;
            }

            if (e.RowIndex == -1 && columnName == "ColumnConfiguredSourceExclusions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                DrawConfiguredFilterIcon(e.Graphics, e.CellBounds, false, ModernTheme.TextColor);
                e.Handled = true;
                return;
            }

            if (e.RowIndex < 0)
            {
                return;
            }

            if ((columnName == "ColumnConfiguredSourceDirectory" ||
                 columnName == "ColumnConfiguredTargetDirectory") &&
                TryGetBackupProgressForRow(
                    e.RowIndex,
                    out BackupProgressDisplayState? progressState) &&
                ((columnName == "ColumnConfiguredSourceDirectory" &&
                  progressState.Area == BackupFileProgressArea.Source) ||
                 (columnName == "ColumnConfiguredTargetDirectory" &&
                  progressState.Area == BackupFileProgressArea.Target)))
            {
                PaintBackupProgressCell(
                    e,
                    progressState);
                return;
            }

            if (columnName != "ColumnConfiguredSourceBrowse" &&
                columnName != "ColumnConfiguredSourceSettings" &&
                columnName != "ColumnConfiguredSourceExclusions" &&
                columnName != "ColumnConfiguredTargetBrowse")
            {
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
            Color outlineColor = isSelected ? ModernTheme.DarkTextColor : ModernTheme.TextColor;

            if (columnName == "ColumnConfiguredSourceBrowse" || columnName == "ColumnConfiguredTargetBrowse")
            {
                DrawConfiguredBrowseIcon(e.Graphics, e.CellBounds, outlineColor);
            }

            if (columnName == "ColumnConfiguredSourceSettings")
            {
                bool retentionActive =
                    e.RowIndex < _settings.BackupPathPairs.Count &&
                    IsRetentionActive(
                        _settings.BackupPathPairs[e.RowIndex]);

                bool sourceCleanupActive =
                    e.RowIndex < _settings.BackupPathPairs.Count &&
                    IsSourceCleanupActive(
                        _settings.BackupPathPairs[e.RowIndex]);

                DrawConfiguredSettingsIcon(
                    e.Graphics,
                    e.CellBounds,
                    outlineColor,
                    1.1F,
                    retentionActive,
                    sourceCleanupActive);
            }

            if (columnName == "ColumnConfiguredSourceExclusions")
            {
                bool hasExclusions = dataGridViewConfiguredPaths.Rows[e.RowIndex].Tag is List<string> excludedPaths
                    && excludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath));

                DrawConfiguredFilterIcon(e.Graphics, e.CellBounds, hasExclusions, outlineColor);
            }

            e.Handled = true;
        }
        
        private void DrawConfiguredSettingsIcon(
            Graphics graphics,
            Rectangle cellBounds,
            Color outlineColor,
            float penWidth,
            bool retentionActive = false,
            bool sourceCleanupActive = false)
        {
            ModernTheme.DrawSettingsIcon(
                graphics,
                cellBounds,
                outlineColor,
                penWidth);

            if (retentionActive)
            {
                ModernTheme.DrawSettingsStatusMarker(
                    graphics,
                    cellBounds,
                    "R",
                    ModernTheme.ActiveRetentionColor,
                    ModernTheme.BackupInfoTextColor,
                    true);
            }

            if (sourceCleanupActive)
            {
                ModernTheme.DrawSettingsStatusMarker(
                    graphics,
                    cellBounds,
                    "C",
                    ModernTheme.ActiveSourceCleanupColor,
                    ModernTheme.DarkTextColor,
                    false);
            }
        }

        private void DrawConfiguredBrowseIcon(Graphics graphics, Rectangle cellBounds, Color outlineColor)
        {
            int centerX = cellBounds.Left + cellBounds.Width / 2;
            int centerY = cellBounds.Top + cellBounds.Height / 2 - 1;

            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(outlineColor, 1.6F);

            graphics.DrawEllipse(pen, centerX - 6, centerY - 6, 9, 9);
            graphics.DrawLine(pen, centerX + 1, centerY + 1, centerX + 7, centerY + 7);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        private void DrawConfiguredFilterIcon(Graphics graphics, Rectangle cellBounds, bool hasExclusions, Color outlineColor)
        {
            int centerX = cellBounds.Left + cellBounds.Width / 2;
            int centerY = cellBounds.Top + cellBounds.Height / 2;

            Point[] funnelPoints =
            {
        new Point(centerX - 8, centerY - 7),
        new Point(centerX + 8, centerY - 7),
        new Point(centerX + 3, centerY - 1),
        new Point(centerX + 1, centerY + 7),
        new Point(centerX - 1, centerY + 7),
        new Point(centerX - 3, centerY - 1)
    };

            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(hasExclusions ? ModernTheme.DarkTextColor : outlineColor, 1F);

            if (hasExclusions)
            {
                using SolidBrush brush = new SolidBrush(ModernTheme.ActiveExclusionColor);
                graphics.FillPolygon(brush, funnelPoints);
            }

            graphics.DrawPolygon(pen, funnelPoints);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        private void UpdateConfiguredExclusionButtonStyle(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridViewConfiguredPaths.Rows.Count)
            {
                return;
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceExclusions"))
            {
                return;
            }

            dataGridViewConfiguredPaths.InvalidateCell(dataGridViewConfiguredPaths.Rows[rowIndex].Cells["ColumnConfiguredSourceExclusions"]);
        }

        private bool ShowConfiguredExclusionsDialog(List<string> excludedPaths, out List<string> resultExcludedPaths)
        {
            resultExcludedPaths = excludedPaths;

            using ExclusionsDialog dialog = new ExclusionsDialog(excludedPaths);

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return false;
            }

            resultExcludedPaths = dialog.ResultExcludedPaths;
            return true;
        }
        private void dataGridViewConfiguredPaths_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isRefreshingConfiguredPaths)
            {
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (columnName == "ColumnConfiguredIsEnabled" ||
                columnName == "ColumnConfiguredSourceDirectory" ||
                columnName == "ColumnConfiguredTargetDirectory")
            {
                SyncEnabledPairsFromGrid();
            }
        }

        private void Form1_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            if (!_isExplicitExitRequested &&
                _settings.CloseToSystray &&
                e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                SaveWindowSettings();
                RefreshNotifyIconText();

                Hide();
                ShowInTaskbar = false;
                notifyIconMain.Visible = true;
                return;
            }

            notifyIconMain.Visible = false;
            SaveWindowSettings();
        }

        private bool ShouldRunRetentionForPair(BackupPathPair pair)
        {
            return _settings.AutoPurgeEnabled &&
                (pair.RetentionKeepLastEnabled || pair.RetentionKeepDaysEnabled);
        }

        private DialogResult TryConfirmSourceCleanupWarningDialogue(
            BackupPathPair pair,
            List<string> sourceCleanupPreviewPaths)
        {
            if (DebugMode.Current.Enabled &&
                DebugMode.Current.SkipDeletionConfirmationDialogs)
            {
                BackupLogger.WriteLine(
                    $"SOURCE CLEANUP CONFIRMATION BYPASSED BY DEBUG MODE | source=\"{pair.SourceDirectory}\" | files={sourceCleanupPreviewPaths.Count}");

                return DialogResult.Yes;
            }

            List<string> deletionPreviewLines =
                new List<string>
                {
                    "SOURCE DIRECTORY:",
                    pair.SourceDirectory,
                    string.Empty
                };

            deletionPreviewLines.AddRange(
                sourceCleanupPreviewPaths.Select(
                    FormatDeletionPreviewPath));

            DialogResult result =
                ShowDeletionConfirmation(
                    "Confirm Source Cleanup",
                    "Source Cleanup will permanently and irreversibly delete these files from the source directory:",
                    "Continue without Source Cleanup",
                    "Continue (delete!)",
                    deletionPreviewLines);

            BackupLogger.WriteLine(
                $"SOURCE CLEANUP CONFIRMATION | source=\"{pair.SourceDirectory}\" | files={sourceCleanupPreviewPaths.Count} | result={result}");

            return result;
        }

        private DialogResult TryConfirmRetentionWarningDialogue(
            BackupPathPair pair,
            List<string> retentionPreviewPaths)
        {
            if (DebugMode.Current.Enabled &&
                DebugMode.Current.SkipDeletionConfirmationDialogs)
            {
                BackupLogger.WriteLine(
                    $"RETENTION CONFIRMATION BYPASSED BY DEBUG MODE | target=\"{pair.TargetDirectory}\" | files={retentionPreviewPaths.Count}");

                return DialogResult.Yes;
            }

            List<string> deletionPreviewLines =
                new List<string>
                {
                    "DESTINATION DIRECTORY:",
                    pair.TargetDirectory,
                    string.Empty
                };

            deletionPreviewLines.AddRange(
                retentionPreviewPaths.Select(
                    FormatDeletionPreviewPath));

            DialogResult result =
                ShowDeletionConfirmation(
                    "Confirm Retention Cleanup",
                    "Retention will permanently and irreversibly delete these backup files from the destination directory:",
                    "Continue without Destination Cleanup",
                    "Continue (delete!)",
                    deletionPreviewLines);

            BackupLogger.WriteLine(
                $"RETENTION CONFIRMATION | target=\"{pair.TargetDirectory}\" | files={retentionPreviewPaths.Count} | result={result}");

            return result;
        }

        private DialogResult ShowDeletionConfirmation(
            string title,
            string warningText,
            string continueWithoutText,
            string deleteText,
            List<string> purgePreviewLines)
        {
            using Form form = new Form();

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ClientSize = new Size(760, 520);
            form.BackColor = ModernTheme.WindowBackColor;
            form.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            form.ShowInTaskbar = false;
            form.Icon = Icon;
            ModernWindowFrame.Apply(form);

            Panel panelModernTitleBar = new Panel
            {
                Name = "panelModernTitleBar",
                Dock = DockStyle.Top,
                Height = ModernTheme.TitleBarHeight,
                BackColor = ModernTheme.TitleBarBackColor
            };

            PictureBox pictureBoxModernTitleIcon = new PictureBox
            {
                Name = "pictureBoxModernTitleIcon",
                Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = title,
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(form.ClientSize.Width - 66, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = new Button
            {
                Name = "buttonModernClose",
                Text = string.Empty,
                Size = ModernTheme.TitleBarButtonSize,
                Location = new Point(form.ClientSize.Width - ModernTheme.TitleBarButtonSize.Width, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            buttonModernClose.FlatAppearance.BorderSize = 0;
            buttonModernClose.FlatAppearance.MouseOverBackColor = ModernTheme.CloseButtonHoverColor;
            buttonModernClose.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            buttonModernClose.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using Pen pen = new Pen(ModernTheme.TextColor, 1.4F)
                {
                    StartCap = System.Drawing.Drawing2D.LineCap.Square,
                    EndCap = System.Drawing.Drawing2D.LineCap.Square
                };

                e.Graphics.DrawLine(pen, 13, 11, 23, 21);
                e.Graphics.DrawLine(pen, 23, 11, 13, 21);
            };

            buttonModernClose.Click += (sender, e) =>
            {
                form.DialogResult = DialogResult.Cancel;
                form.Close();
            };

            panelModernTitleBar.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                const int wmNclbuttondown = 0xA1;
                const int htCaption = 0x2;

                ReleaseCapture();
                SendMessage(form.Handle, wmNclbuttondown, htCaption, 0);
            };

            pictureBoxModernTitleIcon.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                const int wmNclbuttondown = 0xA1;
                const int htCaption = 0x2;

                ReleaseCapture();
                SendMessage(form.Handle, wmNclbuttondown, htCaption, 0);
            };

            labelModernTitle.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                const int wmNclbuttondown = 0xA1;
                const int htCaption = 0x2;

                ReleaseCapture();
                SendMessage(form.Handle, wmNclbuttondown, htCaption, 0);
            };

            panelModernTitleBar.Controls.Add(pictureBoxModernTitleIcon);
            panelModernTitleBar.Controls.Add(labelModernTitle);
            panelModernTitleBar.Controls.Add(buttonModernClose);

            Label labelMessage = new Label
            {
                Text = warningText,
                AutoSize = false,
                Location = new Point(18, 52),
                Size = new Size(form.ClientSize.Width - 36, 28),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel panelPurgePreview = new Panel
            {
                Name = "panelPurgePreview",
                Location = new Point(18, 86),
                Size = new Size(form.ClientSize.Width - 36, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ModernTheme.ControlBackColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            Panel panelPurgePreviewViewport = new Panel
            {
                Name = "panelPurgePreviewViewport",
                Location = Point.Empty,
                BackColor = ModernTheme.ControlBackColor
            };

            Label labelPurgePreviewContent = new Label
            {
                Name = "labelPurgePreviewContent",
                Text = string.Join(Environment.NewLine, purgePreviewLines),
                AutoSize = false,
                Location = new Point(4, 4),
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Font = form.Font,
                TextAlign = ContentAlignment.TopLeft
            };

            ModernTheme.ModernScrollBar verticalPurgePreviewScrollBar =
                new ModernTheme.ModernScrollBar
                {
                    Name = "verticalPurgePreviewScrollBar",
                    Orientation = Orientation.Vertical,
                    Width = ModernTheme.DataGridViewScrollBarSize,
                    Visible = false
                };

            ModernTheme.ModernScrollBar horizontalPurgePreviewScrollBar =
                new ModernTheme.ModernScrollBar
                {
                    Name = "horizontalPurgePreviewScrollBar",
                    Orientation = Orientation.Horizontal,
                    Height = ModernTheme.DataGridViewScrollBarSize,
                    Visible = false
                };

            panelPurgePreviewViewport.Controls.Add(
                labelPurgePreviewContent);
            panelPurgePreview.Controls.Add(
                panelPurgePreviewViewport);
            panelPurgePreview.Controls.Add(
                verticalPurgePreviewScrollBar);
            panelPurgePreview.Controls.Add(
                horizontalPurgePreviewScrollBar);

            void UpdatePurgePreviewContentPosition()
            {
                labelPurgePreviewContent.Location =
                    new Point(
                        4 - horizontalPurgePreviewScrollBar.Value,
                        4 - verticalPurgePreviewScrollBar.Value);
            }

            void UpdatePurgePreviewLayout()
            {
                int scrollBarSize =
                    ModernTheme.DataGridViewScrollBarSize;

                string[] previewLines =
                    purgePreviewLines.Count == 0
                        ? new[] { string.Empty }
                        : purgePreviewLines.ToArray();

                int contentWidth =
                    previewLines
                        .Select(line =>
                            TextRenderer.MeasureText(
                                line,
                                labelPurgePreviewContent.Font,
                                Size.Empty,
                                TextFormatFlags.NoPadding |
                                TextFormatFlags.NoPrefix)
                                .Width)
                        .DefaultIfEmpty(0)
                        .Max() + 8;

                int contentHeight =
                    previewLines.Length *
                    labelPurgePreviewContent.Font.Height + 8;

                int availableWidth =
                    Math.Max(
                        1,
                        panelPurgePreview.ClientSize.Width);

                int availableHeight =
                    Math.Max(
                        1,
                        panelPurgePreview.ClientSize.Height);

                bool horizontalVisible =
                    contentWidth > availableWidth;
                bool verticalVisible =
                    contentHeight > availableHeight;

                if (verticalVisible)
                {
                    availableWidth -= scrollBarSize;
                }

                if (horizontalVisible)
                {
                    availableHeight -= scrollBarSize;
                }

                if (!horizontalVisible &&
                    contentWidth > availableWidth)
                {
                    horizontalVisible = true;
                    availableHeight -= scrollBarSize;
                }

                if (!verticalVisible &&
                    contentHeight > availableHeight)
                {
                    verticalVisible = true;
                    availableWidth -= scrollBarSize;
                }

                availableWidth =
                    Math.Max(1, availableWidth);
                availableHeight =
                    Math.Max(1, availableHeight);

                panelPurgePreviewViewport.Location =
                    Point.Empty;
                panelPurgePreviewViewport.Size =
                    new Size(
                        availableWidth,
                        availableHeight);

                verticalPurgePreviewScrollBar.Location =
                    new Point(
                        availableWidth,
                        0);
                verticalPurgePreviewScrollBar.Size =
                    new Size(
                        scrollBarSize,
                        availableHeight);
                verticalPurgePreviewScrollBar.Minimum = 0;
                verticalPurgePreviewScrollBar.Maximum =
                    Math.Max(
                        0,
                        contentHeight - availableHeight);
                verticalPurgePreviewScrollBar.LargeChange =
                    availableHeight;
                verticalPurgePreviewScrollBar.Visible =
                    verticalVisible;

                horizontalPurgePreviewScrollBar.Location =
                    new Point(
                        0,
                        availableHeight);
                horizontalPurgePreviewScrollBar.Size =
                    new Size(
                        availableWidth,
                        scrollBarSize);
                horizontalPurgePreviewScrollBar.Minimum = 0;
                horizontalPurgePreviewScrollBar.Maximum =
                    Math.Max(
                        0,
                        contentWidth - availableWidth);
                horizontalPurgePreviewScrollBar.LargeChange =
                    availableWidth;
                horizontalPurgePreviewScrollBar.Visible =
                    horizontalVisible;

                labelPurgePreviewContent.Size =
                    new Size(
                        Math.Max(
                            contentWidth,
                            availableWidth - 8),
                        Math.Max(
                            contentHeight,
                            availableHeight - 8));

                UpdatePurgePreviewContentPosition();
            }

            verticalPurgePreviewScrollBar.ScrollValueChanged +=
                (sender, e) =>
                    UpdatePurgePreviewContentPosition();

            horizontalPurgePreviewScrollBar.ScrollValueChanged +=
                (sender, e) =>
                    UpdatePurgePreviewContentPosition();

            panelPurgePreviewViewport.MouseWheel +=
                (sender, e) =>
                {
                    if (!verticalPurgePreviewScrollBar.Visible)
                    {
                        return;
                    }

                    int scrollStep =
                        Math.Max(
                            labelPurgePreviewContent.Font.Height,
                            SystemInformation.MouseWheelScrollLines *
                            labelPurgePreviewContent.Font.Height);

                    verticalPurgePreviewScrollBar.Value +=
                        e.Delta > 0
                            ? -scrollStep
                            : scrollStep;
                };

            labelPurgePreviewContent.MouseWheel +=
                (sender, e) =>
                {
                    if (!verticalPurgePreviewScrollBar.Visible)
                    {
                        return;
                    }

                    int scrollStep =
                        Math.Max(
                            labelPurgePreviewContent.Font.Height,
                            SystemInformation.MouseWheelScrollLines *
                            labelPurgePreviewContent.Font.Height);

                    verticalPurgePreviewScrollBar.Value +=
                        e.Delta > 0
                            ? -scrollStep
                            : scrollStep;
                };

            panelPurgePreview.Resize +=
                (sender, e) =>
                    UpdatePurgePreviewLayout();

            UpdatePurgePreviewLayout();

            Button buttonCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 28),
                Location = new Point(form.ClientSize.Width - 549, 470),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true,
                UseVisualStyleBackColor = false
            };

            buttonCancel.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonCancel.FlatAppearance.BorderSize = 1;
            buttonCancel.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonCancel.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            Button buttonContinueWithoutDeleting = new Button
            {
                Text = continueWithoutText,
                Size = new Size(285, 30),
                Location = new Point(form.ClientSize.Width - 443, 469),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.No,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true,
                UseVisualStyleBackColor = false
            };

            buttonContinueWithoutDeleting.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonContinueWithoutDeleting.FlatAppearance.BorderSize = 1;
            buttonContinueWithoutDeleting.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonContinueWithoutDeleting.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            Button buttonContinuePurge = new Button
            {
                Text = deleteText,
                Size = new Size(146, 30),
                Location = new Point(form.ClientSize.Width - 152, 469),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Yes,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.DestructiveActionColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true,
                UseVisualStyleBackColor = false
            };

            buttonContinuePurge.FlatAppearance.BorderSize = 0;
            buttonContinuePurge.FlatAppearance.MouseOverBackColor = ModernTheme.DestructiveActionHoverColor;
            buttonContinuePurge.FlatAppearance.MouseDownBackColor = ModernTheme.DestructiveActionPressedColor;

            form.Controls.Add(panelModernTitleBar);
            form.Controls.Add(labelMessage);
            form.Controls.Add(panelPurgePreview);
            form.Controls.Add(buttonCancel);
            form.Controls.Add(buttonContinueWithoutDeleting);
            form.Controls.Add(buttonContinuePurge);

            form.AcceptButton = buttonContinuePurge;
            form.CancelButton = buttonCancel;

            return form.ShowDialog(this);
        }

        private string FormatDeletionPreviewPath(
            string deletionPreviewPath)
        {
            if (File.Exists(deletionPreviewPath))
            {
                return $"{deletionPreviewPath} ({File.GetLastWriteTime(deletionPreviewPath):yyyy-MM-dd HH:mm:ss})";
            }

            return $"{deletionPreviewPath} (file not found)";
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                SaveWindowSettings();
            }
        }
        private string FormatNotifyIconRemainingText(TimeSpan remaining)
        {
            int totalSeconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));

            if (totalSeconds < 60)
            {
                return totalSeconds == 1 ? "1 second" : totalSeconds + " seconds";
            }

            int totalMinutes = Math.Max(1, (int)Math.Ceiling(totalSeconds / 60.0));

            if (totalMinutes < 60)
            {
                return totalMinutes == 1 ? "1 minute" : totalMinutes + " minutes";
            }

            int totalHours = Math.Max(1, (int)Math.Ceiling(totalMinutes / 60.0));
            return totalHours == 1 ? "1 hour" : totalHours + " hours";
        }
        private void RefreshNotifyIconText()
        {
            if (!_settings.AutoBackupEnabled || !TryGetNextAutoBackupRun(out DateTime nextAutoBackupRun))
            {
                notifyIconMain.Text = "No backup scheduled";
                return;
            }

            TimeSpan remaining = nextAutoBackupRun - DateTime.Now;

            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            notifyIconMain.Text = $"Next backup in {FormatNotifyIconRemainingText(remaining)} ({nextAutoBackupRun:HH:mm})";
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                UpdateConfiguredPathsScrollBarBounds();
            }

            if (!_settings.MinimizeToSystray)
            {
                return;
            }

            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            RefreshNotifyIconText();

            Hide();
            ShowInTaskbar = false;
            notifyIconMain.Visible = true;
        }
        private void InitializeResizeGripPanel()
        {
            Panel panelResizeGrip = new Panel
            {
                Name = "panelResizeGrip",
                Size = new Size(18, 18),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 18, ClientSize.Height - 18),
                Cursor = Cursors.SizeNWSE,
                BackColor = ModernTheme.WindowBackColor
            };

            panelResizeGrip.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using Pen penDark = new Pen(ModernTheme.AccentColor, 1F);
                using Pen penLight = new Pen(ModernTheme.ControlBackColor, 1F);

                for (int offset = 4; offset <= 14; offset += 4)
                {
                    e.Graphics.DrawLine(penLight, 18 - offset + 1, 18, 18, 18 - offset + 1);
                    e.Graphics.DrawLine(penDark, 18 - offset, 18, 18, 18 - offset);
                }
            };

            panelResizeGrip.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                const int wmNclbuttondown = 0xA1;
                const int htBottomRight = 17;

                ReleaseCapture();
                SendMessage(Handle, wmNclbuttondown, htBottomRight, 0);
            };

            Controls.Add(panelResizeGrip);
            panelResizeGrip.BringToFront();
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                SaveWindowSettings();
            }
        }

        private void notifyIconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromSystray();
        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            RestoreFromSystray();
        }

        private void toolStripMenuItemBackup_Click(object sender, EventArgs e)
        {
            buttonBackup_Click(sender, e);
        }

        private void toolStripMenuItemExitTray_Click(
            object sender,
            EventArgs e)
        {
            _isExplicitExitRequested = true;
            notifyIconMain.Visible = false;
            Close();
        }

        private void RestoreFromSystray()
        {
            Show();

            if (_settings.MinimizeToSystray || _settings.CloseToSystray)
            {
                ShowInTaskbar = false;
                notifyIconMain.Visible = true;
            }
            else
            {
                ShowInTaskbar = true;
                notifyIconMain.Visible = false;
            }

            WindowState = FormWindowState.Normal;
            Activate();
        }


        private sealed class BackupPairRunResult
        {
            public bool Successful { get; set; }
            public bool Canceled { get; set; }
            public bool RetentionCanceled { get; set; }
            public Exception? Error { get; set; }
            public int SkippedPaths { get; set; }
            public int PurgedBackups { get; set; }
            public string DestinationAction { get; set; } =
                BackupHelper.DestinationActionCreated;
            public string DestinationFileName { get; set; } =
                string.Empty;
        }

    }
}