using Microsoft.Win32;
using System.Runtime.Versioning;

namespace NutWinSvc;

/// <summary>
/// Windows registry implementation of <see cref="ConfigurationProvider"/>.
/// </summary>
[SupportedOSPlatform("windows")]
internal class RegistryConfigurationProvider : ConfigurationProvider
{
    private readonly RegistryConfigurationSource source;
    private readonly string prefix;

    public RegistryConfigurationProvider(RegistryConfigurationSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(source.Path);
        this.source = source;
        prefix = Path.GetFileName(Path.TrimEndingDirectorySeparator(source.Path));
    }

    private void ProcessValues(RegistryKey key, string prefix)
    {
        foreach (var name in key.GetValueNames())
        {
            try
            {
                var value = key.GetValue(name);
                if (Convert.ChangeType(value, typeof(string)) is string str)
                    Data.Add(prefix + ConfigurationPath.KeyDelimiter + name, str);
            }
            catch { }
        }
    }

    private void ProcessKey(RegistryKey key, string prefix)
    {
        ProcessValues(key, prefix);
        foreach (var name in key.GetSubKeyNames())
        {
            try
            {
                using RegistryKey? subKey = key.OpenSubKey(name);
                if (subKey != null)
                {
                    string subPrefix = prefix + ConfigurationPath.KeyDelimiter + name;
                    ProcessValues(subKey, subPrefix);
                    ProcessKey(subKey, subPrefix);
                }
            }
            catch { }
        }
    }

    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return base.GetChildKeys(earlierKeys, parentPath);
    }

    public override void Load()
    {
        Data.Clear();
        try
        {
            using RegistryKey root = RegistryKey.OpenBaseKey(source.RegistryHive, RegistryView.Default);
            using RegistryKey? key = root.OpenSubKey(source.Path);
            if (key != null)
                ProcessKey(key, prefix);
        }
        catch { }
    }

}
