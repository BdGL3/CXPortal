using System;

namespace L3.Cargo.Workstation.Plugins.Common.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }

        string Version { get; }

        void Initialize (Object passedObj);

        void Dispose ();
    }
}
