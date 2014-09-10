using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Communications.OPC
{
    public class OpcTagBase<T>
    {
        public string Name;

        public T Value;

        public OpcTagBase(string name)
        {
            Name = name;
        }

        public OpcTagBase(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
