using System.Collections;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public abstract class PluginInstance<T> : IDisposable
        where T : IPlugin
    {
        private T m_Instance = default(T);

        public T Instance
        {
            get
            {
                return m_Instance;
            }

            set
            {
                m_Instance = value;
            }
        }

        public void Dispose()
        {
            this.m_Instance.Dispose();
            this.m_Instance = default(T);
        }
    }
}
