using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutWinSvc
{
    internal class RegistryConfigurationProvider : ConfigurationProvider
    {
        private readonly RegistryConfigurationSource source;

        public RegistryConfigurationProvider(RegistryConfigurationSource source)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(source.Path, nameof(source) + "." + nameof(source.Path));
            this.source = source;
        }

        public override void Load()
        {
            //Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
        }

    }
}
