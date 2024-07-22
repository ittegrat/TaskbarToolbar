using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace TaskbarToolbar
{
  public static class Icons
  {

    public static Bitmap ExtractIcon(string path, int index =0, bool small = false) {
      var ans = ExtractIconEx(path, index, out var lsih, out var ssih, 1);
      if (ans == uint.MaxValue)
        throw new Win32Exception();
      else {
        using (lsih) {
          using (ssih) {
            return Icon.FromHandle((small ? ssih : lsih).DangerousGetHandle()).ToBitmap();
          }
        }
      }
    }
    public static Bitmap GetFileIcon(string path, Flags flags = Flags.LARGEICON) {
      var uFlags = (uint)flags
        | SHGSI_ICON
      ;
      var sfi = new SHFileInfo();
      var ans = SHGetFileInfo(path, 0, ref sfi, SHFileInfoSz, uFlags);
      if (ans == IntPtr.Zero)
        throw new Win32Exception();
      else {
        using (var sih = new SafeIconHandle(sfi.hIcon)) {
          var bitmap = Icon.FromHandle(sih.DangerousGetHandle()).ToBitmap();
          return bitmap;
        }
      }
    }
    public static Bitmap GetStockIcon(IconID iconId, Flags flags = Flags.LARGEICON) {
      var sii = new SHStockIconInfo {
        cbSize = SHStockIconInfoSz
      };
      var uFlags = (uint)flags
        | SHGSI_ICON
      ;
      SHGetStockIconInfo((int)iconId, uFlags, ref sii);
      using (var sih = new SafeIconHandle(sii.hIcon)) {
        var bitmap = Icon.FromHandle(sih.DangerousGetHandle()).ToBitmap();
        return bitmap;
      }
    }


    const uint SHGSI_ICON = 0x100; // #define SHGSI_ICON SHGFI_ICON

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct SHFileInfo
    {
      public IntPtr hIcon;
      public int iIcon;
      public uint dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    }
    static readonly uint SHFileInfoSz = (uint)Marshal.SizeOf(typeof(SHFileInfo));

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHStockIconInfo
    {
      public uint cbSize;
      public IntPtr hIcon;
      public int iSysImageIndex;
      public int iIcon;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szPath;
    }
    static readonly uint SHStockIconInfoSz = (uint)Marshal.SizeOf(typeof(SHStockIconInfo));

    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern uint ExtractIconEx(string lpszFile, int nIconIndex, out SafeIconHandle phiconLarge, out SafeIconHandle phiconSmall, uint nIcons);
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFileInfo psfi, uint cbFileInfo, uint uFlags);
    [DllImport("shell32.dll", PreserveSig = false)]
    static extern void SHGetStockIconInfo(int siid, uint uFlags, ref SHStockIconInfo psii);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool DestroyIcon(IntPtr hIcon);

    sealed class SafeIconHandle : SafeHandle
    {
      public SafeIconHandle()
        : base(IntPtr.Zero, true) { }
      public SafeIconHandle(IntPtr hIcon)
        : this() { SetHandle(hIcon); }
      public override bool IsInvalid {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [PrePrepareMethod]
        get { return handle == IntPtr.Zero; }
      }
      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
      [PrePrepareMethod]
      protected override bool ReleaseHandle() {
        return DestroyIcon(handle);
      }
    }

    [Flags]
    public enum Flags : uint
    {
      LARGEICON = 0x0,
      SMALLICON = 0x1,
      SHELLICONSIZE = 0x4,
      LINKOVERLAY = 0x8000,
      SELECTED = 0x10000,
    }

    public enum IconID
    {
      DOCNOASSOC = 0,
      DOCASSOC = 1,
      APPLICATION = 2,
      FOLDER = 3,
      FOLDEROPEN = 4,
      DRIVE525 = 5,
      DRIVE35 = 6,
      DRIVEREMOVE = 7,
      DRIVEFIXED = 8,
      DRIVENET = 9,
      DRIVENETDISABLED = 10,
      DRIVECD = 11,
      DRIVERAM = 12,
      WORLD = 13,
      SERVER = 15,
      PRINTER = 16,
      MYNETWORK = 17,
      FIND = 22,
      HELP = 23,
      SHARE = 28,
      LINK = 29,
      SLOWFILE = 30,
      RECYCLER = 31,
      RECYCLERFULL = 32,
      MEDIACDAUDIO = 40,
      LOCK = 47,
      AUTOLIST = 49,
      PRINTERNET = 50,
      SERVERSHARE = 51,
      PRINTERFAX = 52,
      PRINTERFAXNET = 53,
      PRINTERFILE = 54,
      STACK = 55,
      MEDIASVCD = 56,
      STUFFEDFOLDER = 57,
      DRIVEUNKNOWN = 58,
      DRIVEDVD = 59,
      MEDIADVD = 60,
      MEDIADVDRAM = 61,
      MEDIADVDRW = 62,
      MEDIADVDR = 63,
      MEDIADVDROM = 64,
      MEDIACDAUDIOPLUS = 65,
      MEDIACDRW = 66,
      MEDIACDR = 67,
      MEDIACDBURN = 68,
      MEDIABLANKCD = 69,
      MEDIACDROM = 70,
      AUDIOFILES = 71,
      IMAGEFILES = 72,
      VIDEOFILES = 73,
      MIXEDFILES = 74,
      FOLDERBACK = 75,
      FOLDERFRONT = 76,
      SHIELD = 77,
      WARNING = 78,
      INFO = 79,
      ERROR = 80,
      KEY = 81,
      SOFTWARE = 82,
      RENAME = 83,
      DELETE = 84,
      MEDIAAUDIODVD = 85,
      MEDIAMOVIEDVD = 86,
      MEDIAENHANCEDCD = 87,
      MEDIAENHANCEDDVD = 88,
      MEDIAHDDVD = 89,
      MEDIABLURAY = 90,
      MEDIAVCD = 91,
      MEDIADVDPLUSR = 92,
      MEDIADVDPLUSRW = 93,
      DESKTOPPC = 94,
      MOBILEPC = 95,
      USERS = 96,
      MEDIASMARTMEDIA = 97,
      MEDIACOMPACTFLASH = 98,
      DEVICECELLPHONE = 99,
      DEVICECAMERA = 100,
      DEVICEVIDEOCAMERA = 101,
      DEVICEAUDIOPLAYER = 102,
      NETWORKCONNECT = 103,
      INTERNET = 104,
      ZIPFILE = 105,
      SETTINGS = 106,
      DRIVEHDDVD = 132,
      DRIVEBD = 133,
      MEDIAHDDVDROM = 134,
      MEDIAHDDVDR = 135,
      MEDIAHDDVDRAM = 136,
      MEDIABDROM = 137,
      MEDIABDR = 138,
      MEDIABDRE = 139,
      CLUSTEREDDRIVE = 140,
      MAX_ICONS = 181,
    }

  }
}
