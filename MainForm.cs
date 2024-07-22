using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TaskbarToolbar
{
  internal class MainForm : Form
  {

    ContextMenuStrip contextMenu;

    public MainForm() {

      BackColor = Color.Black;
      FormBorderStyle = FormBorderStyle.None;
      Icon = Program.GetAppIcon();
      MaximizeBox = false;
      MinimizeBox = false;
      Size = new Size(0, 0);
      WindowState = FormWindowState.Minimized;

      Resize += OnResize;

      if (!Configuration.GetValue("lazy", true))
        contextMenu = BuildContextMenu();

    }

    public void Rebuild() {
      contextMenu?.Dispose();
      if (Configuration.GetValue("lazy", true))
        contextMenu = null;
      else
        contextMenu = BuildContextMenu();
    }

    void OnResize(object sender, EventArgs e) {
      if (WindowState == FormWindowState.Minimized)
        return;
      WindowState = FormWindowState.Minimized;
      ShowMenu();
    }

    ContextMenuStrip BuildContextMenu() {

      contextMenu = new ContextMenuStrip();
      contextMenu.MouseWheel += (s, e) => (s as ContextMenuStrip).Close();
      contextMenu.PreviewKeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) (s as ContextMenuStrip).Close(); };
      ContextMenuStrip = contextMenu;

      using (var builder = new ItemBuilder()) {
        foreach (var fse in Directory.EnumerateFileSystemEntries(Program.RootPath)) {
          if (builder.BuildItem(fse, out var item))
            contextMenu.Items.Add(item);
        }
      }

      contextMenu.PerformLayout();

      return contextMenu;

    }
    void ShowMenu() {
      var bar = contextMenu ?? BuildContextMenu();
      var point = Cursor.Position;
      Program.SetForegroundWindow(bar.Handle);
      bar.Show(point);
    }

  }
}
