using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Communications.Common
{
    public struct DiscoveryMetadata
    {
        public string Name;
        public string Value;

        public DiscoveryMetadata (string metadataName, string metadataValue)
        {
            Name = metadataName;
            Value = metadataValue;
        }
    }
}
