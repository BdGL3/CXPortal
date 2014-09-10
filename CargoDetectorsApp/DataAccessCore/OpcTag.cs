
namespace L3.Cargo.Detectors.DataAccessCore
{
    public class OpcTag
    {
        public string Name;

        public int Value;

        public OpcTag(string name)
        {
            Name = name;
            Value = -1;
        }

        public OpcTag (string name, int value)
        {
            Name = name;
            Value = value;
        }
    }
}
