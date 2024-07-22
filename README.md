# TaskbarToolbar
Windows 11 removed the 'Toolbars' (e.g. Quick Launch) functionality from the Taskbar and restructured the 'Start Menu' in a way I don't like. This simple app try to fill the gap, at least for me.

Inspired by [SystemTrayMenu](https://github.com/Hofknecht/SystemTrayMenu) and [TrayToolbar](https://github.com/rojarsmith/TrayToolbar), but rewritten from scratch.

The program enumerates the shortcuts (a.k.a. link files) starting from a root folder, analyzes their properties and builds a ContextMenuStrip that is displayed by clicking on the taskbar icon.

## Configuration
Only shortcuts and the folder structure is considered; other files are ignored.
The program is portable.
The configuration file contains at least the `appSettings` section.
Per PC overrides can be specified adding a section named after the PC.

The `appSettings` section specify:
  - `root`: the root path [REQUIRED].
  - `sort.prefix`: a regex prefix, eventually stripped from folder and shortcut names, to manage the sorting order [OPTIONAL, default `^\d+_`]
  - `lazy`: delays context menu creation until first use [OPTIONAL, default `true`]

The root folder is enumerated using `Directory.EnumerateFileSystemEntries`, so you can mix shortcuts and folders. In subfolders, folders and shortcuts are enumerated separately (folders first).

## Actions
  - Shortcut:
    - Left Click --> start shortcut
    - Shift + LClick --> start shortcut as Administrator
    - Right Click --> open workdir or targetdir
    - Shift + RClick --> open targetdir
    - Ctrl + RClick --> open shortcut properties
  - Folder:
    - Shift + Click --> runas
    - Right Click --> open folder

Roll the mouse wheel or press the Esc button to dismiss the context menu without clicking on an item.
