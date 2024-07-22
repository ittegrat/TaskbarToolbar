using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace TaskbarToolbar
{
  public static class Configuration
  {

    static Dictionary<string, string> options;

    public static T GetValue<T>(string key, T @default) {
      return options.ContainsKey(key) ? (T)Convert.ChangeType(options[key], typeof(T)) : @default;
    }
    public static void Refresh() {
      options = new Dictionary<string, string>();
      try {

        ConfigurationManager.RefreshSection("appSettings");
        if (ConfigurationManager.AppSettings is NameValueCollection appSettings) {
          foreach (var key in appSettings.AllKeys)
            options.Add(key, appSettings[key]);
        }

        var machine = Environment.MachineName.ToUpperInvariant();
        ConfigurationManager.RefreshSection(machine);
        if (ConfigurationManager.GetSection(machine) is NameValueCollection machineSection) {
          foreach (var key in machineSection.AllKeys)
            options[key] = machineSection[key];
        }

      }
      catch (Exception ex) {
        options["ERROR"] = ex.ToString();
      }
    }

    static Configuration() { Refresh(); }

  }
}
