using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO.Compression;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using static BoombloxBootstrapper.Manager;

namespace BoombloxBootstrapper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Shown += Form1_Shown;
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            //accident
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void Form1_Shown(object sender, EventArgs e) 
        {
            await Task.Delay(500);

            if (Manager.HasArguments()) {
                if (Manager.CheckForArgument("-register")) {
                    label1.Text = "Registering Roblox...";
                    Manager.RegisterURI();
                    Form1.ActiveForm.Close();
                }
                if (Manager.CheckForArgument("-install")) {
                    label1.Text = "Installing Roblox...";
                    await Manager.Install();
                    Form1.ActiveForm.Close();
                }
            }

            if (!Manager.IsRegistered()) {
                var result = MessageBox.Show("Roblox is not registered, register?", "Registry Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    Manager.RegisterURI();
                }
                else {
                    Form1.ActiveForm.Close();
                }
            }

            if (!Manager.CanConnect())
            {
                string caption = "Cannot connect to Roblox website.\n\nIs antivirus software preventing Roblox from accessing the Internet?\n\nIf you continue, Roblox may not work properly.";
                string title = "Error";
                MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                MessageBoxIcon icon = MessageBoxIcon.Warning;

                var result = MessageBox.Show(
                    caption,
                    title,
                    buttons,
                    icon
                );

                if (result == DialogResult.Cancel)
                {
                    this.Close();
                    return;
                }

                if (!await RequiresInstall())
                {
                    /*if (Manager.Md5Check())
                     {*/
                        label1.Text = "Starting Roblox...";
                        Manager.StartClient(this);
                    /*{
                    else
                    {
                        MessageBox.Show("Do not tamper with the Roblox client.", "Roblox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Form1.ActiveForm.Close();
                    }*/
                }
                else {
                    MessageBox.Show("Roblox is not installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Form1.ActiveForm.Close();
                }
                
                
            }
            else {
                if (!await RequiresInstall())
                {
                    /*
                    if (Manager.Md5Check())
                    {*/
                        label1.Text = "Starting Roblox...";
                        Manager.StartClient(this);
                    /*}
                    else
                    {
                        MessageBox.Show("Do not tamper with the Roblox client.", "Roblox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Form1.ActiveForm.Close();
                    }*/
                }
                else
                {
                    label1.Text = "Installing Roblox...";
                    await Manager.Install();
                    label1.Text = "Starting Roblox...";
                    await Task.Delay(1000);
                    Manager.StartClient(this);
                }
            }

            Form1.ActiveForm.Close();
        }
        public static async Task<bool> RequiresInstall() {
            if (System.IO.File.Exists(clientPath))
            {
                string currentVersion = await Manager.GetCurrentVersion();
                string clientMd5 = await Manager.GetClientMd5();
                return currentVersion != clientMd5;
            }
            return true;
        }
    }

    public static class Manager
    {
        public static string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string folderName = "Boomblox";
        public static string siteUrl = "http://bmblox.xyz";
        public static string clientPath = localAppData + "/" + folderName + "/RobloxApp.exe";
        static readonly HttpClient web = new HttpClient();
        public static string currentVersion;
        private static string gameKey = "Boomblox";
        public static string currentMd5 = "3088843772ad9c0a";

        public enum OS {
            Windows,
            Linux,
            MacOS
        }

        public static async Task<string> GetClientMd5() {
            return currentMd5;
        }

        /// <summary>
        /// Begins an attempt to start the client
        /// </summary>
        public static void StartClient(Form mainForm)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                StartWindowsClient(mainForm);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                StartLinuxClient();
            }
        }

        /// <summary>
        /// Starts the discord rich presence background app
        /// </summary>
        private static Process StartDiscordRPC() {
            string[] args = Environment.GetCommandLineArgs();
            var process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = localAppData + "/" + folderName + "/content/fonts/BoombloxRPC.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = args[0];
            process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.StartInfo.UseShellExecute = false;
            return process;
        }

        /// <summary>
        /// Attempts to start the Windows client
        /// </summary>
        private static void StartWindowsClient(Form mainForm) {
            try
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = clientPath;

                process.Start();
                /*var rpc = StartDiscordRPC();
                rpc.Start();

                mainForm.Hide();

                process.WaitForExit();
                if (process.HasExited) {
                    rpc.Kill();
                    mainForm.Close();
                }*/
                
                Form.ActiveForm.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception: " + ex.Message);
                /*string caption = "CreateProcess\n\n" + clientPath + "\nfailed: The system cannot find the file specified.";
                string title = "Roblox";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Error;

                var result = MessageBox.Show(caption, title, buttons, icon);

                if (result == DialogResult.OK)
                {
                    Form1.ActiveForm.Close();
                }*/
            }
        }

        /// <summary>
        /// Attempts to start the Linux client
        /// </summary>
        private static void StartLinuxClient() {
            NotSupported(OS.Linux);
        }
        
        /// <summary>
        /// Informs the user that their OS is not supported for this application
        /// </summary>
        /// <param name="os">The OS that is being requested but isn't supported</param>
        public static void NotSupported(OS os) {
            string caption = os + " is not a supported OS";
            string title = "Error";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Warning;

            var result = MessageBox.Show(caption, title, buttons, icon);

            if (result == DialogResult.OK)
            {
                Form1.ActiveForm.Close();
            }
        }

        /// <summary>
        /// Determines whether the user can connect to the site or not
        /// </summary>
        /// <returns>A boolean of if the ping to the site succeeded or not</returns>
        public static bool CanConnect()
        {
            Ping ping = new Ping();
            try { 
                PingReply reply = ping.Send("bmblox.xyz");
                return reply.Status == IPStatus.Success;
            } catch (Exception) {
                return false;
            }
            
        }

        public static async Task<string> GetCurrentVersion() {
            string responseBody;

            try
            {
                Random rand = new Random();
                HttpResponseMessage response = await web.GetAsync("http://bmblox.xyz/version.txt?t=" + rand.Next(1, 50000));
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                responseBody = "NAN";
            }

            return responseBody;
        }

        /// <summary>
        /// Checks if an argument exists
        /// </summary>
        /// <param name="argument">Name of the argument</param>
        /// <returns></returns>
        public static bool CheckForArgument(string argument) {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) {
                if (arg == argument) {
                    return true;
                }   
            }

            return false;
        }

        /// <summary>
        /// Checks if the bootstrapper was started with arguments
        /// </summary>
        /// <returns></returns>
        public static bool HasArguments() {
            return Environment.GetCommandLineArgs().Length > 0;
        }

        /// <summary>
        /// Checks if the app is registered 
        /// </summary>
        /// <returns></returns>
        public static bool IsRegistered()
        {
            bool result = false;
            using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(gameKey))
            {
                if (registryKey != null) {
                    using (RegistryKey registryKey2 = registryKey.OpenSubKey("Identifier")) {
                        if (registryKey2 != null)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Registers the app
        /// </summary>
        public static void RegisterURI()
        {
            if (!Manager.IsAdministrator())
            {
                MessageBox.Show("Must be an administrator to register Roblox", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Form1.ActiveForm.Close();
            }

            string executablePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Boomblox/Roblox.exe";
            using (RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey(gameKey))
            {
                registryKey.SetValue("", "URL:Boomblox");
                registryKey.SetValue("URL Protocol", "");
                using (RegistryKey registryKey2 = registryKey.CreateSubKey("DefaultIcon"))
                {
                    registryKey2.SetValue("", "\"" + executablePath + "\",1");
                }
                using (RegistryKey registryKey3 = registryKey.CreateSubKey("shell\\open\\command"))
                {
                    registryKey3.SetValue("", "\"" + executablePath + "\"\"%1\"");
                }
                using (RegistryKey registryKey4 = registryKey.CreateSubKey("Identifier")) 
                {
                    registryKey4.SetValue("Version", "BoombloxV2");
                }
            }
            string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
            string value = text + "RobloxApp.exe";
            using (RegistryKey registryKey4 = Registry.ClassesRoot.CreateSubKey("TypeLib\\{03E1C8ED-C1C6-47BF-B9B9-A37B677318DD}"))
            {
                using (RegistryKey registryKey5 = registryKey4.CreateSubKey("1.2"))
                {
                    registryKey5.SetValue("", "Roblox 1.2 Type Library");
                    using (RegistryKey registryKey6 = registryKey5.CreateSubKey("0"))
                    {
                        using (RegistryKey registryKey7 = registryKey6.CreateSubKey("win32"))
                        {
                            registryKey7.SetValue("", value);
                        }
                    }
                    using (RegistryKey registryKey8 = registryKey5.CreateSubKey("FLAGS"))
                    {
                        registryKey8.SetValue("", "0");
                    }
                    using (RegistryKey registryKey9 = registryKey5.CreateSubKey("HELPDIR"))
                    {
                        registryKey9.SetValue("", text);
                    }
                }
                using (RegistryKey registryKey10 = registryKey4.CreateSubKey("1.3"))
                {
                    registryKey10.SetValue("", "Roblox 1.3 Type Library");
                    using (RegistryKey registryKey11 = registryKey10.CreateSubKey("0"))
                    {
                        using (RegistryKey registryKey12 = registryKey11.CreateSubKey("win32"))
                        {
                            registryKey12.SetValue("", value);
                        }
                    }
                    using (RegistryKey registryKey13 = registryKey10.CreateSubKey("FLAGS"))
                    {
                        registryKey13.SetValue("", "0");
                    }
                    using (RegistryKey registryKey14 = registryKey10.CreateSubKey("HELPDIR"))
                    {
                        registryKey14.SetValue("", text);
                    }
                }
            }
            MessageBox.Show("Succesfully registered!");
        }

        /// <summary>
        /// Checks if the user is running the bootstrapper as an administrator
        /// </summary>
        /// <returns></returns>
        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Checks if the current client's Md5 checksum matches the original
        /// </summary>
        /// <returns></returns>
        public static bool Md5Check() {
            return GetMd5Hash(clientPath) == currentMd5;
        }

        /// <summary>
        /// Gets the current client's Md5 checksum
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMd5Hash(string filePath)
        {
            string result;
            using (MD5 md = MD5.Create())
            {
                using (FileStream fileStream = System.IO.File.OpenRead(filePath))
                {
                    byte[] array = md.ComputeHash(fileStream);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (byte b in array)
                    {
                        stringBuilder.Append(b.ToString("x2"));
                    }
                    result = stringBuilder.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Installs the client
        /// </summary>
        /// <returns></returns>
        public static async Task Install() {
            string extractPath = Path.Combine(localAppData, "Boomblox");
            string zipPath = Path.Combine(extractPath, "temp.zip");
            Random randomTime = new Random();
            string location = "http://t0.bmblox.xyz/version-" + await GetCurrentVersion() + ".zip";

            if (Directory.Exists(extractPath)) {
                Directory.Delete(extractPath, true);
            }

            Directory.CreateDirectory(extractPath);

            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(location), zipPath);
            }

            ZipFile.ExtractToDirectory(zipPath, extractPath);

            System.IO.File.Delete(zipPath);

            Regserver();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            CreateShortcut("Play ROBLOX", Path.Combine(extractPath, "RobloxApp.exe"), "", desktopPath);
            CreateShortcut("ROBLOX Studio", Path.Combine(extractPath, "RobloxApp.exe"), "-script wait();game:Load(\"\")", desktopPath);
        }
        private static void Regserver() {
            ProcessStartInfo clientInfo = new ProcessStartInfo();
            clientInfo.WindowStyle = ProcessWindowStyle.Minimized;
            clientInfo.UseShellExecute = false;
            clientInfo.FileName = clientPath;
            clientInfo.Arguments = "/regserver";

            var process = Process.Start(clientInfo);
            System.Threading.Thread.Sleep(2000);

            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        private static void CreateShortcut(string shortcutName, string targetPath, string arguments, string shortcutDirectory)
        {
            string shortcutPath = Path.Combine(shortcutDirectory, shortcutName + ".lnk");

            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.Description = shortcutName;
            shortcut.TargetPath = targetPath;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Save();
        }
    }
}
