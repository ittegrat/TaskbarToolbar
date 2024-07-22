using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TaskbarToolbar
{

  public class LaunchInfo
  {
    public string FileName;
    public string TargetDir;
    public string WorkDir;
  }

  public class ItemBuilder : IDisposable
  {

    static Bitmap folderIcon;

    readonly dynamic wshShell;
    readonly Regex prefix;
    bool disposed = false;

    public ItemBuilder() {
      // 'Shell Object' WshShell coclass {72C24DD5-D70A-438B-8A42-98424B88AFB8}
      // Implements IWshShell3 interface {41904400-BE18-11D3-A28B-00104BD35090}
      // See 'Windows Script Host Object Model' TypeLibrary F935DC20-1CF0-11D0-ADB9-00C04FD58A0B
      var type = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
      wshShell = Activator.CreateInstance(type);
      prefix = new Regex(Configuration.GetValue("sort.prefix", @"^\d+_"));
    }

    public bool BuildItem(string path, out ToolStripItem item) {
      CheckDisposed();
      if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
        return BuildItemFromShortcut(path, out item);
      else if (Directory.Exists(path))
        return BuildItemFromFolder(path, out item);
      item = null;
      return false;
    }
    public void Dispose() {
      Marshal.FinalReleaseComObject(wshShell);
      disposed = true;
    }

    bool BuildItemFromFolder(string path, out ToolStripItem item) {

      var menu = new ToolStripMenuItem();

      foreach (var d in Directory.EnumerateDirectories(path)) {
        if (BuildItemFromFolder(d, out var mi)) {
          mi.Tag = new LaunchInfo { FileName = d };
          mi.Text = prefix.Replace(Path.GetFileNameWithoutExtension(d), String.Empty);
          mi.Image = GetFolderIcon();
          mi.MouseUp += Program.OnFolderClick;
          menu.DropDownItems.Add(mi);
        }
      }

      foreach (var f in Directory.EnumerateFiles(path, "*.lnk")) {
        if (BuildItemFromShortcut(f, out var mi))
          menu.DropDownItems.Add(mi);
      }

      if (menu.DropDownItems.Count > 0) {
        menu.Tag = new LaunchInfo { FileName = path };
        menu.Text = prefix.Replace(Path.GetFileNameWithoutExtension(path), String.Empty);
        menu.Image = GetFolderIcon();
        menu.MouseUp += Program.OnFolderClick;
        item = menu;
        return true;
      }
      else {
        menu?.Dispose();
        item = null;
        return false;
      }

    }
    bool BuildItemFromShortcut(string path, out ToolStripItem item) {
      dynamic wshShortcut = wshShell.CreateShortcut(path);
      var mi = new ToolStripMenuItem();
      try {

        var linfo = new LaunchInfo {
          FileName = path,
          WorkDir = Environment.ExpandEnvironmentVariables(wshShortcut.WorkingDirectory),
          TargetDir = Path.GetDirectoryName(wshShortcut.TargetPath),
        };
        if (String.IsNullOrEmpty(linfo.WorkDir))
          linfo.WorkDir = linfo.TargetDir;
        mi.Tag = linfo;

        var desc = (wshShortcut.Description as string)?.Trim();
        mi.Text = String.IsNullOrEmpty(desc)
          ? prefix.Replace(Path.GetFileNameWithoutExtension(path), String.Empty)
          : desc
        ;

        var iconInfo = ((string)wshShortcut.IconLocation).Split(',');
        if (iconInfo[0] != String.Empty) {
          mi.Image = Icons.ExtractIcon(iconInfo[0], Int32.Parse(iconInfo[1]), true);
        }
        else {
          var target = (wshShortcut.TargetPath as string)?.Trim();
          if (Directory.Exists(target))
            mi.Image = GetFolderIcon();
          else
            mi.Image = Icons.GetFileIcon(target, Icons.Flags.SMALLICON);
        }

        mi.MouseUp += Program.OnLinkClick;

        item = mi;
        return true;

      }
      catch {
        mi?.Dispose();
        item = null;
        return false;
      }
      finally {
        Marshal.FinalReleaseComObject(wshShortcut);
      }
    }
    void CheckDisposed() { if (disposed) throw new ObjectDisposedException(nameof(ItemBuilder)); }

    Bitmap GetFolderIcon() {
      if (folderIcon is null)
        folderIcon = Icons.GetStockIcon(Icons.IconID.FOLDER, Icons.Flags.SMALLICON);
      return folderIcon;
    }

  }

}
