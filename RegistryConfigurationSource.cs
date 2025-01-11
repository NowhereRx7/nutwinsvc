using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutWinSvc
{
    internal class RegistryConfigurationSource : IConfigurationSource
    {
        public RegistryHive RegistryHive { get; set; }

        public string Path { get; set; } = string.Empty;

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new RegistryConfigurationProvider(this);
    }
}
