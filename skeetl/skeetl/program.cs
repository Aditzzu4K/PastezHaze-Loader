using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Loader
{
    // 
    public static class ResourceLoader
    {
        private static string extractFolder = "";

        public static void Initialize()
        {
            extractFolder = Path.Combine(Path.GetTempPath(), "LoaderExtract");
            if (!Directory.Exists(extractFolder))
                Directory.CreateDirectory(extractFolder);
        }

        public static Image LoadLogo()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resources = assembly.GetManifestResourceNames();

                foreach (string res in resources)
                {
                    if (res.EndsWith(".png") || res.EndsWith(".jpg") || res.EndsWith(".bmp"))
                    {
                        using (Stream stream = assembly.GetManifestResourceStream(res))
                        {
                            if (stream != null)
                                return Image.FromStream(stream);
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static string ExtractDLL(string dllName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resources = assembly.GetManifestResourceNames();

                string resourceName = null;
                foreach (string res in resources)
                {
                    if (res.EndsWith(dllName))
                    {
                        resourceName = res;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(resourceName))
                {
                    string dllNameNoExt = Path.GetFileNameWithoutExtension(dllName);
                    foreach (string res in resources)
                    {
                        if (res.Contains(dllNameNoExt) && res.EndsWith(".dll"))
                        {
                            resourceName = res;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(resourceName))
                    return null;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    string extractPath = Path.Combine(extractFolder, dllName);
                    File.WriteAllBytes(extractPath, buffer);

                    return extractPath;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void ExtractAllDLLs(List<string> dllNames)
        {
            foreach (string dllName in dllNames)
            {
                ExtractDLL(dllName);
            }
        }

        public static string GetExtractPath(string dllName)
        {
            return Path.Combine(extractFolder, dllName);
        }
    }

    // 
    public class LoginForm : Form
    {
        private TextBox passwordBox;
        private Label errorLabel;
        private const string CORRECT_PASSWORD = "bbclover";

        public LoginForm()
        {
            this.Text = "";
            this.Size = new Size(380, 260);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;

            PictureBox logoBox = new PictureBox();
            logoBox.Location = new Point(90, 15);
            logoBox.Size = new Size(180, 60);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;

            Image logo = ResourceLoader.LoadLogo();
            if (logo != null)
                logoBox.Image = logo;
            else
                logoBox.Visible = false;
            this.Controls.Add(logoBox);

            Label title = new Label();
            title.Text = "🔐 Authentication Required";
            title.ForeColor = Color.FromArgb(200, 150, 255);
            title.BackColor = Color.FromArgb(30, 30, 35);
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            title.Location = new Point(20, 85);
            title.Size = new Size(320, 30);
            title.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(title);

            Label status = new Label();
            status.Text = "Enter password to unlock loader";
            status.ForeColor = Color.FromArgb(180, 180, 190);
            status.BackColor = Color.FromArgb(30, 30, 35);
            status.Font = new Font("Segoe UI", 11);
            status.Location = new Point(20, 120);
            status.Size = new Size(320, 25);
            status.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(status);

            passwordBox = new TextBox();
            passwordBox.Location = new Point(70, 155);
            passwordBox.Size = new Size(220, 25);
            passwordBox.BackColor = Color.FromArgb(40, 40, 48);
            passwordBox.ForeColor = Color.FromArgb(220, 220, 230);
            passwordBox.Font = new Font("Consolas", 12);
            passwordBox.PasswordChar = '●';
            passwordBox.TextAlign = HorizontalAlignment.Center;
            passwordBox.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) Login(); };
            this.Controls.Add(passwordBox);

            errorLabel = new Label();
            errorLabel.Text = "";
            errorLabel.ForeColor = Color.FromArgb(255, 80, 80);
            errorLabel.BackColor = Color.FromArgb(30, 30, 35);
            errorLabel.Font = new Font("Segoe UI", 10);
            errorLabel.Location = new Point(20, 185);
            errorLabel.Size = new Size(320, 25);
            errorLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(errorLabel);

            Button loginBtn = new Button();
            loginBtn.Text = "🔓 Unlock";
            loginBtn.Location = new Point(110, 210);
            loginBtn.Size = new Size(100, 32);
            loginBtn.BackColor = Color.FromArgb(40, 40, 48);
            loginBtn.ForeColor = Color.FromArgb(220, 220, 230);
            loginBtn.FlatStyle = FlatStyle.Flat;
            loginBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 70);
            loginBtn.Click += (s, e) => Login();
            this.Controls.Add(loginBtn);

            Button exitBtn = new Button();
            exitBtn.Text = "✖ Exit";
            exitBtn.Location = new Point(220, 210);
            exitBtn.Size = new Size(80, 32);
            exitBtn.BackColor = Color.FromArgb(40, 40, 48);
            exitBtn.ForeColor = Color.FromArgb(220, 220, 230);
            exitBtn.FlatStyle = FlatStyle.Flat;
            exitBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 70);
            exitBtn.Click += (s, e) => Application.Exit();
            this.Controls.Add(exitBtn);

            this.Shown += (s, e) => passwordBox.Focus();
        }

        private void Login()
        {
            if (passwordBox.Text.Trim() == CORRECT_PASSWORD)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                errorLabel.Text = "❌ Invalid password! Try again.";
                passwordBox.Text = "";
                passwordBox.Focus();
                passwordBox.BackColor = Color.FromArgb(60, 30, 30);
                System.Timers.Timer timer = new System.Timers.Timer(300);
                timer.Elapsed += (s, args) => {
                    passwordBox.Invoke(new Action(() => {
                        passwordBox.BackColor = Color.FromArgb(40, 40, 48);
                    }));
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                startPoint = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
        }
    }

    //
    public class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint MEM_RELEASE = 0x8000;
        public const uint PAGE_READWRITE = 0x04;
    }

    // 
    public class StandardInjector
    {
        public static bool Inject(string dllPath, string processName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    MessageBox.Show($" {processName} don't run!", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                Process target = processes[0];

                if (!File.Exists(dllPath))
                {
                    MessageBox.Show($"DLL doesn't exist: {dllPath}", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 
                IntPtr hProcess = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, (uint)target.Id);
                if (hProcess == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show($"OpenProcess failed! Error code: {error}\nRun as Administator.", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 
                byte[] dllPathBytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
                IntPtr pRemoteMemory = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllPathBytes.Length + 1, WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_READWRITE);
                if (pRemoteMemory == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show($"VirtualAllocEx failed! Error code: {error}", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WinAPI.CloseHandle(hProcess);
                    return false;
                }

                // 
                IntPtr bytesWritten;
                if (!WinAPI.WriteProcessMemory(hProcess, pRemoteMemory, dllPathBytes, (uint)dllPathBytes.Length + 1, out bytesWritten))
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show($"WriteProcessMemory failed! Error code: {error}", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WinAPI.VirtualFreeEx(hProcess, pRemoteMemory, 0, WinAPI.MEM_RELEASE);
                    WinAPI.CloseHandle(hProcess);
                    return false;
                }

                // 
                IntPtr hKernel32 = WinAPI.GetModuleHandle("kernel32.dll");
                IntPtr pLoadLibrary = WinAPI.GetProcAddress(hKernel32, "LoadLibraryA");
                if (pLoadLibrary == IntPtr.Zero)
                {
                    MessageBox.Show("LoadLibracy can't be find!", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WinAPI.VirtualFreeEx(hProcess, pRemoteMemory, 0, WinAPI.MEM_RELEASE);
                    WinAPI.CloseHandle(hProcess);
                    return false;
                }

                // 
                IntPtr hThread = WinAPI.CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibrary, pRemoteMemory, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show($"CreateRemoteThread failed! Error code: {error}", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WinAPI.VirtualFreeEx(hProcess, pRemoteMemory, 0, WinAPI.MEM_RELEASE);
                    WinAPI.CloseHandle(hProcess);
                    return false;
                }

                // 
                WinAPI.WaitForSingleObject(hThread, 5000);

                // 
                WinAPI.CloseHandle(hThread);
                WinAPI.VirtualFreeEx(hProcess, pRemoteMemory, 0, WinAPI.MEM_RELEASE);
                WinAPI.CloseHandle(hProcess);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Injection Error: {ex.Message}", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

    // ===== MAIN FORM =====
    public class MainForm : Form
    {
        private ListBox cheatList;
        private Label statusLabel;
        private Label lineLabel;
        private Timer animationTimer;
        private float hue = 0.0f;
        private List<CheatInfo> cheats = new List<CheatInfo>();
        private List<string> dllNames = new List<string>();

        public class CheatInfo
        {
            public string Name { get; set; }
            public string ResourceName { get; set; }
            public string Updated { get; set; }
        }

        public MainForm()
        {
            this.Text = "";
            this.Size = new Size(410, 450);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;

            ResourceLoader.Initialize();

            // Logo
            PictureBox logoBox = new PictureBox();
            logoBox.Location = new Point(115, 5);
            logoBox.Size = new Size(160, 50);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;

            Image logo = ResourceLoader.LoadLogo();
            if (logo != null)
                logoBox.Image = logo;
            else
                logoBox.Visible = false;
            this.Controls.Add(logoBox);

            Label title = new Label();
            title.Text = "CS2 Loader";
            title.ForeColor = Color.FromArgb(200, 150, 255);
            title.BackColor = Color.FromArgb(30, 30, 35);
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.Location = new Point(20, 10);
            title.Size = new Size(350, 30);
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Visible = (logo == null);
            this.Controls.Add(title);

            // 
            Label userLabel = new Label();
            string pcName = Environment.MachineName;
            userLabel.Text = $"👤 Welcome, {pcName}";
            userLabel.ForeColor = Color.FromArgb(100, 255, 150);
            userLabel.BackColor = Color.FromArgb(30, 30, 35);
            userLabel.Font = new Font("Segoe UI", 11);
            userLabel.Location = new Point(20, 55);
            userLabel.Size = new Size(350, 25);
            userLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(userLabel);

            Label fsHeader = new Label();
            fsHeader.Text = "Cheatz";
            fsHeader.ForeColor = Color.FromArgb(220, 220, 230);
            fsHeader.BackColor = Color.FromArgb(30, 30, 35);
            fsHeader.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            fsHeader.Location = new Point(20, 85);
            fsHeader.Size = new Size(100, 20);
            this.Controls.Add(fsHeader);

            cheatList = new ListBox();
            cheatList.Location = new Point(20, 105);
            cheatList.Size = new Size(350, 110);
            cheatList.BackColor = Color.FromArgb(40, 40, 48);
            cheatList.ForeColor = Color.FromArgb(220, 220, 230);
            cheatList.Font = new Font("Segoe UI", 12);
            cheatList.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(cheatList);

            // 
            Button refreshBtn = new Button();
            refreshBtn.Text = "🔄 Refresh";
            refreshBtn.Location = new Point(20, 225);
            refreshBtn.Size = new Size(80, 28);
            refreshBtn.BackColor = Color.FromArgb(40, 40, 48);
            refreshBtn.ForeColor = Color.FromArgb(220, 220, 230);
            refreshBtn.FlatStyle = FlatStyle.Flat;
            refreshBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 70);
            refreshBtn.Click += (s, e) => UpdateStatus();
            this.Controls.Add(refreshBtn);

            Button injectBtn = new Button();
            injectBtn.Text = "📥 Load";
            injectBtn.Location = new Point(110, 225);
            injectBtn.Size = new Size(80, 28);
            injectBtn.BackColor = Color.FromArgb(40, 40, 48);
            injectBtn.ForeColor = Color.FromArgb(220, 220, 230);
            injectBtn.FlatStyle = FlatStyle.Flat;
            injectBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 70);
            injectBtn.Click += InjectBtn_Click;
            this.Controls.Add(injectBtn);

            Button exitBtn = new Button();
            exitBtn.Text = "✖ Exit";
            exitBtn.Location = new Point(200, 225);
            exitBtn.Size = new Size(80, 28);
            exitBtn.BackColor = Color.FromArgb(40, 40, 48);
            exitBtn.ForeColor = Color.FromArgb(220, 220, 230);
            exitBtn.FlatStyle = FlatStyle.Flat;
            exitBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 70);
            exitBtn.Click += (s, e) => Application.Exit();
            this.Controls.Add(exitBtn);

            lineLabel = new Label();
            lineLabel.Location = new Point(20, 218);
            lineLabel.Size = new Size(350, 3);
            this.Controls.Add(lineLabel);

            statusLabel = new Label();
            statusLabel.Location = new Point(20, 260);
            statusLabel.Size = new Size(350, 130);
            statusLabel.ForeColor = Color.FromArgb(220, 220, 230);
            statusLabel.BackColor = Color.FromArgb(30, 30, 35);
            statusLabel.Font = new Font("Consolas", 11);
            this.Controls.Add(statusLabel);

            animationTimer = new Timer();
            animationTimer.Interval = 30;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            LoadCheats();
            UpdateStatus();
            ExtractAllDLLs();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            hue += 0.01f;
            if (hue > 1.0f) hue = 0.0f;
            lineLabel.BackColor = HSVToRGB(hue, 1.0f, 1.0f);
        }

        private Color HSVToRGB(float hue, float saturation, float value)
        {
            int hi = (int)Math.Floor(hue * 6) % 6;
            float f = hue * 6 - (float)Math.Floor(hue * 6);
            float v = value;
            float p = value * (1 - saturation);
            float q = value * (1 - f * saturation);
            float t = value * (1 - (1 - f) * saturation);

            float r, g, b;
            switch (hi)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private void ExtractAllDLLs()
        {
            try
            {
                foreach (string dllName in dllNames)
                {
                    string path = ResourceLoader.ExtractDLL(dllName);
                }
            }
            catch { }
        }

        private void LoadCheats()
        {
            cheats.Clear();
            dllNames.Clear();

            cheats.Add(new CheatInfo { Name = "NeverPaste", ResourceName = "neverpaste.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "Bankroll (Crashes all time)", ResourceName = "bankroll.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "Invincible.win", ResourceName = "invincible.win.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "ResolveCheat", ResourceName = "resolveheat.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "Cynth-", ResourceName = "cynth-.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "Velocity.cat", ResourceName = "velocity.cat.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "AttackWare", ResourceName = "attackware.dll", Updated = "" });
            cheats.Add(new CheatInfo { Name = "Nexoriabeta", ResourceName = "nexoriabeta.dll", Updated = "" });

            foreach (var cheat in cheats)
            {
                dllNames.Add(cheat.ResourceName);
            }

            cheatList.Items.Clear();
            foreach (var cheat in cheats)
            {
                cheatList.Items.Add($"  {cheat.Name}");
            }
            if (cheatList.Items.Count > 0) cheatList.SelectedIndex = 0;
        }

        private void UpdateStatus()
        {
            bool steamRunning = Process.GetProcessesByName("steam").Length > 0;
            bool cs2Running = Process.GetProcessesByName("cs2").Length > 0;
            Random rand = new Random();
            int days = rand.Next(1, 366);

            statusLabel.Text = $"Status:\n" +
                (steamRunning ? "Steam: ✅ Connected\n" : "Steam: ❌ Not running\n") +
                (cs2Running ? "CS2: ✅ Running\n" : "CS2: ❌ Not running\n") +
                $"Random ({days} days remaining)\n" +
                "Welcome back, user\n" +
                "kkk >,< credits to -- and --";
        }

        private void InjectBtn_Click(object sender, EventArgs e)
        {
            if (cheatList.SelectedIndex < 0 || cheatList.SelectedIndex >= cheats.Count)
            {
                MessageBox.Show("Select a cheat first!", "Erorr", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CheatInfo selected = cheats[cheatList.SelectedIndex];

            if (Process.GetProcessesByName("steam").Length == 0)
            {
                MessageBox.Show("Steam is not open!", "Careful", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Process.GetProcessesByName("cs2").Length == 0)
            {
                MessageBox.Show("CS2 doesn't run!\n\nOpen CS2 before injection.", "Careful", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string dllPath = ResourceLoader.GetExtractPath(selected.ResourceName);

            if (!File.Exists(dllPath))
            {
                dllPath = ResourceLoader.ExtractDLL(selected.ResourceName);
                if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
                {
                    MessageBox.Show($"Can't extract dlls:\n{selected.ResourceName}",
                        "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (StandardInjector.Inject(dllPath, "cs2"))
            {
                MessageBox.Show($"✅ {selected.Name} Injected succesful!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus();
            }
        }

        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                startPoint = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }
            base.OnFormClosed(e);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (LoginForm login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
