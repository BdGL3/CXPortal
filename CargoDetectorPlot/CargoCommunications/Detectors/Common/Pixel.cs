using System;

namespace L3.Cargo.Communications.Detectors.Common
{
    public class Pixel: IDisposable
    {
        public Pixel() { Value = 0; }
        public Pixel(object value) { Value = System.Convert.ToUInt32(value); }
        public Pixel Clone() { return Convert(Value); }
        public static Pixel Convert(object value) { return new Pixel(value); }
        public void Dispose() { }
        public UInt32 Value { get; set; }
    }
}
