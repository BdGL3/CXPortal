using System.Runtime.Serialization;

namespace L3.Cargo.Communications.DetectorPlot.Common
{
    [DataContract]
    public class DetectorConfig
    {
        #region Private Members

        private int _PixelsPerColumn;

        private int _BytesPerPixel;

        private int _DetectorsPerBoard;

        #endregion Private Members


        #region Public Members

        [DataMember]
        public int PixelsPerColumn
        {
            get
            {
                return _PixelsPerColumn;
            }
            set
            {
                _PixelsPerColumn = value;
            }
        }

        [DataMember]
        public int BytesPerPixel
        {
            get
            {
                return _BytesPerPixel;
            }
            set
            {
                _BytesPerPixel = value;
            }
        }

        [DataMember]
        public int DetectorsPerBoard
        {
            get
            {
                return _DetectorsPerBoard;
            }
            set
            {
                _DetectorsPerBoard = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public DetectorConfig(int pixelsPerColumn, int bytesPerPixel, int detectorsPerBoard)
        {
            _PixelsPerColumn = pixelsPerColumn;
            _BytesPerPixel = bytesPerPixel;
            _DetectorsPerBoard = detectorsPerBoard;
        }

        #endregion Constructors
    }
}
