using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace NutWinSvc;

[SupportedOSPlatform("windows")]
internal class RegistryConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// Gets or sets the registry root hive to use.<br/>
    /// Defaults to <see cref="RegistryHive.CurrentUser"/>.
    /// </summary>
    public RegistryHive RegistryHive { get; set; } = RegistryHive.CurrentUser;

    /// <summary>
    /// Gets or sets the registry key path to use.<br/>
    /// <b>Note</b>: The last key in the path will become the key for <see cref="ConfigurationManager.GetSection"/>.<br/>
    /// Example: Path=@"SOFTWARE\Company\Program" => .GetSection("Program")<br/>
    /// <b>Warning</b>: Avoid loading large registry paths (e.g. SOFTWARE), as all sub-keys and values will be loaded!
    /// </summary>
    public string Path { get; set; } = string.Empty;

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new RegistryConfigurationProvider(this);
}
