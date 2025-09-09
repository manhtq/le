using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SharpMonoInjectorGUI
{
    public partial class MainForm : Form
    {
        private ListView processListView;
        private TextBox dllPathTextBox;
        private TextBox namespaceTextBox;
        private TextBox classNameTextBox;
        private TextBox methodNameTextBox;
        private Button browseButton;
        private Button refreshButton;
        private Button injectButton;
        private RichTextBox logTextBox;
        private Timer refreshTimer;
        private Label statusLabel;

        public MainForm()
        {
            InitializeComponent();
            LoadProcesses();
            SetupTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "SharpMono Injector GUI - AntiCheat DLL Injector";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Title Label
            var titleLabel = new Label
            {
                Text = "🎯 SHARPMONO INJECTOR",
                Font = new Font("Consolas", 20, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(20, 15),
                Size = new Size(400, 35),
                AutoSize = false
            };

            // Process List Group
            var processGroupBox = new GroupBox
            {
                Text = "Target Process",
                Location = new Point(20, 60),
                Size = new Size(850, 250),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            processListView = new ListView
            {
                Location = new Point(10, 25),
                Size = new Size(830, 180),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9)
            };
            processListView.Columns.Add("PID", 80);
            processListView.Columns.Add("Process Name", 250);
            processListView.Columns.Add("Window Title", 300);
            processListView.Columns.Add("Architecture", 100);
            processListView.Columns.Add("Mono", 80);

            refreshButton = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(10, 210),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            refreshButton.Click += RefreshButton_Click;

            processGroupBox.Controls.Add(processListView);
            processGroupBox.Controls.Add(refreshButton);

            // DLL Settings Group
            var dllGroupBox = new GroupBox
            {
                Text = "DLL Settings",
                Location = new Point(20, 320),
                Size = new Size(850, 180),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var dllLabel = new Label
            {
                Text = "DLL Path:",
                Location = new Point(10, 30),
                Size = new Size(80, 20),
                ForeColor = Color.White
            };

            dllPathTextBox = new TextBox
            {
                Location = new Point(100, 28),
                Size = new Size(600, 25),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                Text = @"C:\AntiCheat.dll"
            };

            browseButton = new Button
            {
                Text = "📁 Browse",
                Location = new Point(710, 27),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            browseButton.Click += BrowseButton_Click;

            var namespaceLabel = new Label
            {
                Text = "Namespace:",
                Location = new Point(10, 65),
                Size = new Size(80, 20),
                ForeColor = Color.White
            };

            namespaceTextBox = new TextBox
            {
                Location = new Point(100, 63),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                Text = "AntiCheatSystem"
            };

            var classLabel = new Label
            {
                Text = "Class:",
                Location = new Point(420, 65),
                Size = new Size(50, 20),
                ForeColor = Color.White
            };

            classNameTextBox = new TextBox
            {
                Location = new Point(480, 63),
                Size = new Size(330, 25),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                Text = "AntiCheatBootstrap"
            };

            var methodLabel = new Label
            {
                Text = "Method:",
                Location = new Point(10, 100),
                Size = new Size(80, 20),
                ForeColor = Color.White
            };

            methodNameTextBox = new TextBox
            {
                Location = new Point(100, 98),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                Text = "Initialize"
            };

            injectButton = new Button
            {
                Text = "💉 INJECT DLL",
                Location = new Point(420, 95),
                Size = new Size(390, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            injectButton.Click += InjectButton_Click;

            dllGroupBox.Controls.Add(dllLabel);
            dllGroupBox.Controls.Add(dllPathTextBox);
            dllGroupBox.Controls.Add(browseButton);
            dllGroupBox.Controls.Add(namespaceLabel);
            dllGroupBox.Controls.Add(namespaceTextBox);
            dllGroupBox.Controls.Add(classLabel);
            dllGroupBox.Controls.Add(classNameTextBox);
            dllGroupBox.Controls.Add(methodLabel);
            dllGroupBox.Controls.Add(methodNameTextBox);
            dllGroupBox.Controls.Add(injectButton);

            // Log Group
            var logGroupBox = new GroupBox
            {
                Text = "Log Output",
                Location = new Point(20, 510),
                Size = new Size(850, 120),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            logTextBox = new RichTextBox
            {
                Location = new Point(10, 20),
                Size = new Size(830, 90),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            logGroupBox.Controls.Add(logTextBox);

            // Status Bar
            statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(20, 640),
                Size = new Size(850, 20),
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9)
            };

            // Add all controls to form
            this.Controls.Add(titleLabel);
            this.Controls.Add(processGroupBox);
            this.Controls.Add(dllGroupBox);
            this.Controls.Add(logGroupBox);
            this.Controls.Add(statusLabel);

            // Initial log
            Log("SharpMono Injector initialized successfully.", Color.Cyan);
            Log("Select a Unity/Mono process and click Inject to begin.", Color.Yellow);
        }

        private void SetupTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 5000; // Refresh every 5 seconds
            refreshTimer.Tick += (s, e) => LoadProcesses();
            refreshTimer.Start();
        }

        private void LoadProcesses()
        {
            processListView.Items.Clear();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    // Check if it's a Unity or Mono process
                    bool isMonoProcess = IsMonoProcess(process);
                    
                    // Skip system processes and non-Mono processes for clarity
                    if (process.Id == 0 || process.Id == 4) continue;
                    
                    var item = new ListViewItem(process.Id.ToString());
                    item.SubItems.Add(process.ProcessName);
                    item.SubItems.Add(process.MainWindowTitle);
                    item.SubItems.Add(Is64BitProcess(process) ? "x64" : "x86");
                    item.SubItems.Add(isMonoProcess ? "✓" : "");
                    
                    if (isMonoProcess)
                    {
                        item.BackColor = Color.FromArgb(40, 60, 40);
                    }
                    
                    item.Tag = process;
                    processListView.Items.Add(item);
                }
                catch { }
            }

            statusLabel.Text = $"Found {processListView.Items.Count} processes, {processListView.Items.Cast<ListViewItem>().Count(i => i.SubItems[4].Text == "✓")} Mono/Unity";
        }

        private bool IsMonoProcess(Process process)
        {
            try
            {
                foreach (ProcessModule module in process.Modules)
                {
                    string moduleName = module.ModuleName.ToLower();
                    if (moduleName.Contains("mono") || 
                        moduleName.Contains("unity") ||
                        moduleName == "gameassembly.dll")
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private bool Is64BitProcess(Process process)
        {
            try
            {
                bool wow64Process;
                if (!IsWow64Process(process.Handle, out wow64Process))
                    return IntPtr.Size == 8; // Default to system architecture
                
                return !wow64Process && IntPtr.Size == 8;
            }
            catch
            {
                return IntPtr.Size == 8;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadProcesses();
            Log("Process list refreshed.", Color.Cyan);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*";
                dialog.Title = "Select DLL to Inject";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dllPathTextBox.Text = dialog.FileName;
                    Log($"Selected DLL: {dialog.FileName}", Color.Cyan);
                }
            }
        }

        private void InjectButton_Click(object sender, EventArgs e)
        {
            if (processListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a target process.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(dllPathTextBox.Text))
            {
                MessageBox.Show("DLL file not found. Please check the path.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedItem = processListView.SelectedItems[0];
            var process = (Process)selectedItem.Tag;

            try
            {
                Log($"Starting injection into process: {process.ProcessName} (PID: {process.Id})", Color.Yellow);
                
                var injector = new MonoInjector();
                bool success = injector.Inject(
                    process,
                    dllPathTextBox.Text,
                    namespaceTextBox.Text,
                    classNameTextBox.Text,
                    methodNameTextBox.Text
                );

                if (success)
                {
                    Log($"✓ Successfully injected into {process.ProcessName}!", Color.Lime);
                    Log($"  Namespace: {namespaceTextBox.Text}", Color.LightGray);
                    Log($"  Class: {classNameTextBox.Text}", Color.LightGray);
                    Log($"  Method: {methodNameTextBox.Text}", Color.LightGray);
                    
                    MessageBox.Show("DLL injected successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Log($"✗ Failed to inject into {process.ProcessName}", Color.Red);
                    MessageBox.Show("Injection failed. Check the log for details.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Exception during injection: {ex.Message}", Color.Red);
                MessageBox.Show($"Injection error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string message, Color color)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => Log(message, color)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = Color.Gray;
            logTextBox.AppendText($"[{timestamp}] ");
            logTextBox.SelectionColor = color;
            logTextBox.AppendText(message + Environment.NewLine);
            logTextBox.ScrollToCaret();
        }
    }
}