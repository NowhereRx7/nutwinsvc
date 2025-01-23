using Microsoft.Win32;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Versioning;
using System.Xml;
using System.Xml.Linq;

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

    private static bool CanHandle(RegistryValueKind valueKind)
    {
        switch (valueKind)
        {
            case RegistryValueKind.DWord:
            case RegistryValueKind.ExpandString:
            case RegistryValueKind.QWord:
            case RegistryValueKind.String:
                return true;
            //case RegistryValueKind.None:
            //case RegistryValueKind.Unknown:
            //case RegistryValueKind.Binary:
            //case RegistryValueKind.MultiString:
            default:
                return false;
        }
    }

    private static void ProcessValues(RegistryKey? key, string prefix, List<string> results)
    {
        if (key == null) return;
        foreach (var name in key.GetValueNames())
        {
            try
            {
                if (CanHandle(key.GetValueKind(name)))
                    results.Add(prefix + ConfigurationPath.KeyDelimiter + name);
            }
            catch { }
        }
    }

    private static void ProcessKey(RegistryKey? key, string prefix, List<string> results)
    {
        if (key == null) return;
        ProcessValues(key, prefix, results);
        foreach (var name in key.GetSubKeyNames())
        {
            try
            {
                using RegistryKey? subKey = key.OpenSubKey(name);
                if (subKey == null) continue;
                string subPrefix = prefix + ConfigurationPath.KeyDelimiter + name;
                ProcessValues(subKey, subPrefix, results);
                ProcessKey(subKey, subPrefix, results);
            }
            catch { }
        }
    }

    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        List<string> results = new(base.GetChildKeys(earlierKeys, parentPath));
        try
        {
            using RegistryKey root = RegistryKey.OpenBaseKey(source.RegistryHive, RegistryView.Default);
            using RegistryKey? key = root.OpenSubKey(source.Path);
            ProcessKey(key, prefix, results);
        }
        catch { }
        bool added = (results.Count > 0);
        results.AddRange(earlierKeys);
        if (added) results.Sort(ConfigurationKeyComparer.Instance.Compare);
        return results;
    }

    public override bool TryGet(string key, out string? value) => (base.TryGet(key, out value) || TryGetRegistry(key, out value));

    private bool TryGetRegistry(string key, out string? value)
    {
        value = null;
        try
        {
            string[] parts = key.Split(ConfigurationPath.KeyDelimiter);
            if (parts.Length < 2) return false;
            if (parts[0] != prefix) return false;
            using RegistryKey root = RegistryKey.OpenBaseKey(source.RegistryHive, RegistryView.Default);
            using RegistryKey? rootKey = root.OpenSubKey(source.Path);
            if (rootKey == null) return false;
            if (parts.Length == 2)
            {
                if (!CanHandle(rootKey.GetValueKind(parts[^1]))) return false;
                object? v = rootKey.GetValue(parts[^1]);
                if (v == null) return false;
                value = v.ToString();
                return true; 
            }
            else
            {
                using RegistryKey? registryKey = root.OpenSubKey(String.Join('\\', parts[1..^1]));
                if (registryKey == null) return false;
                if (!CanHandle(registryKey.GetValueKind(parts[^1]))) return false;
                object? v2 = registryKey.GetValue(parts[^1]);
                if (v2 == null) return false;
                value = v2.ToString();
                return true;
            }
        }
        catch { }
        return false;
    }

    public override void Load() => Data.Clear();
}
