using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

/* TODO
 - deploy release
 -? config: single/multiple instances
 -? revise OpenConfig
 -? custom editor for config
 -? custom program to open folders (e.g. explorer++)
 -? command line args
*/

/* CONSIDER
 - AVAILABLE KEYS: { None, Shift, Ctrl, Shift+Ctrl } x { Left, Right }
*/

namespace TaskbarToolbar
{

  internal static class Program
  {

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    static readonly NotifyIcon ni = new NotifyIcon();
    static Icon appIcon;

    public static string RootPath { get; private set; }

    [STAThread]
    static void Main(string[] args) {

      var configErr = Configuration.GetValue("ERROR", String.Empty);
      if (configErr != String.Empty) {
        MessageBox.Show($"Configuration error:\n{configErr}", "TaskbarToolbar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      // Only a single instace is allowed to run
      Mutex singleInstance;
      if (Configuration.GetValue("single.instance", true)) {
        singleInstance = new Mutex(true, "TbTbSingleInstance", out bool granted);
        if (!granted) {
          Debug.WriteLine("Taskbar Toolbar is already running. Exiting.");
          return;
        }
      }

      if (!SetRootPath())
        return;

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Application.ApplicationExit += (s, e) => {
        ni.Visible = false;
        ni.Dispose();
      };

      // NotifyIcon ContextMenu
      ni.ContextMenu = new ContextMenu();
      ni.ContextMenu.MenuItems.Add(new MenuItem("Help", (s, e) => Help()));
      ni.ContextMenu.MenuItems.Add(new MenuItem("Open Root", (s, e) => Launch(RootPath)));
      ni.ContextMenu.MenuItems.Add(new MenuItem("Open Config", (s, e) => OpenConfig()));
      ni.ContextMenu.MenuItems.Add(new MenuItem("Rebuild", (s, e) => Rebuild()));
      ni.ContextMenu.MenuItems.Add(new MenuItem("About", (s, e) => About()));
      ni.ContextMenu.MenuItems.Add(new MenuItem("Exit", (s, e) => Application.Exit()));

      ni.Icon = GetAppIcon();
      ni.Visible = true;

      Application.Run(new MainForm { Name = "MainForm" });

    }

    public static void OnFolderClick(object sender, MouseEventArgs args) {
      var keys = Control.ModifierKeys;
      var item = sender as ToolStripMenuItem;
      var linfo = item.Tag as LaunchInfo;
      //MessageBox.Show($"{args.Button}: {item.Tag}\nKeys: {Control.ModifierKeys}");
      if (args.Button == MouseButtons.Left) {
        if ((keys & Keys.Shift) == Keys.Shift) {
          Launch(linfo.FileName, "runas");
        }
      }
      else if (args.Button == MouseButtons.Right) {
        Launch(linfo.FileName);
      }
    }
    public static void OnLinkClick(object sender, MouseEventArgs args) {
      var keys = Control.ModifierKeys;
      var item = sender as ToolStripMenuItem;
      var linfo = item.Tag as LaunchInfo;
      //MessageBox.Show($"{args.Button}: {item.Tag}\nKeys: {Control.ModifierKeys}");
      if (args.Button == MouseButtons.Left) {
        if ((keys & Keys.Shift) == Keys.Shift)
          Launch(linfo.FileName, "runas");
        else
          Launch(linfo.FileName);
      }
      else if (args.Button == MouseButtons.Right) {
        if ((keys & Keys.Shift) == Keys.Shift)
          Launch(linfo.TargetDir);
        else if ((keys & Keys.Control) == Keys.Control)
          Launch(linfo.FileName, "properties");
        else
          Launch(linfo.WorkDir);
      }

    }

    public static Icon GetAppIcon() {
      if (appIcon is null) {
        var icoName = $"{Assembly.GetExecutingAssembly().GetName().Name}.TbTb.ico";
        var asm = Assembly.GetExecutingAssembly();
        using (var mrs = asm.GetManifestResourceStream(icoName)) {
          appIcon = new Icon(mrs);
        }
      }
      return appIcon;
    }

    static void About() {
      var msg = new StringBuilder();
      var mbi = MessageBoxIcon.Information;
      try {
        var asm = Assembly.GetExecutingAssembly();
        var ad = (AssemblyDescriptionAttribute)asm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute)).First();
        msg.AppendLine(ad.Description);
        var ac = (AssemblyCopyrightAttribute)asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute)).First();
        msg.Append(ac.Copyright).AppendLine(", MIT License");
        var af = (AssemblyFileVersionAttribute)asm.GetCustomAttributes(typeof(AssemblyFileVersionAttribute)).First();
        msg.Append("File version: ").AppendLine(af.Version);
        var av = (AssemblyInformationalVersionAttribute)asm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute)).First();
        msg.Append("Version: ").AppendLine(av.InformationalVersion);
        var am = asm.GetCustomAttributes(typeof(AssemblyMetadataAttribute))
          .Cast<AssemblyMetadataAttribute>()
          .Where(ca => ca.Key == "RepositoryUrl")
          .First()
        ;
        msg.Append(am.Key).Append(": ").AppendLine(am.Value);
      }
      catch (Exception ex) {
        msg.Append(ex.Message);
        mbi = MessageBoxIcon.Error;
      }
      MessageBox.Show(msg.ToString(), "TaskbarToolbar", MessageBoxButtons.OK, mbi);
    }
    static void Help() {
      var msg = new StringBuilder();
      msg.AppendLine(" * Item Click *");
      msg.AppendLine("   - L-Click: launch shortcut");
      msg.AppendLine("   - SHIFT+L-Click: launch shortcut as admin");
      msg.AppendLine("   - R-Click: open working dir");
      msg.AppendLine("   - SHIFT+R-Click: open target dir");
      msg.AppendLine("   - CTRL+R-Click: open shortcut props");
      msg.AppendLine("\n * Folder Click *");
      msg.AppendLine("   - SHIFT+L-Click: open admin terminal in folder");
      msg.AppendLine("   - R-Click: open folder");
      MessageBox.Show(msg.ToString(), "TaskbarToolbar", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    static void Launch(string fileName, string verb = "") {
      var pinfo = new ProcessStartInfo {
        FileName = fileName,
        UseShellExecute = true,
      };
      if (!String.IsNullOrEmpty(verb)) pinfo.Verb = verb;
      try { Process.Start(pinfo); }
      catch (Exception ex) {
        MessageBox.Show($"Process.Start => {ex.GetType().Name}:\n{ex.Message}", "TaskbarToolbar", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
    static void OpenConfig() {
      Launch(System.IO.Path.GetDirectoryName(Application.ExecutablePath));
      // var fileName = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
      // choose editor & launch
      // call refresh config
    }
    static void Rebuild() {
      Configuration.Refresh();
      if (!SetRootPath())
        return;
      (Application.OpenForms["MainForm"] as MainForm)?.Rebuild();
    }
    static bool SetRootPath() {
      RootPath = Environment.ExpandEnvironmentVariables(
        Configuration.GetValue("root", String.Empty)
      );
      if (String.IsNullOrWhiteSpace(RootPath) || !System.IO.Directory.Exists(RootPath)) {
        MessageBox.Show("Invalid root path!", "TaskbarToolbar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        OpenConfig();
        return false;
      }
      return true;
    }

  }

}
