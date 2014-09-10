using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace L3.Cargo.DetectorPlot
{    

    public struct LineData
    {
        public byte [] data;
    }

    public class LineDataEventArg : EventArgs
    {
        public LineDataEventArg (LineData item) {
            this.lineData = item;
        }

        public LineData lineData;

    }

    public class DataLines : List<LineData>
    {
        public delegate void NewLineEventHandler(object sender, LineDataEventArg fe);

        public event NewLineEventHandler OnLineAdded;

        public void Add(LineData item)
        {
            if (null != OnLineAdded)
            {
                LineDataEventArg fe = new LineDataEventArg(item);
                OnLineAdded(this, fe);
            }
            base.Add(item);
        }
    }

    public interface IDataAcq
    {
        void GetConfiguration();
        void SetConfiguration();
        void IsConnected();
        void Connect();
        void Start();
        void Stop();
        void Disconnect();
    }
}

