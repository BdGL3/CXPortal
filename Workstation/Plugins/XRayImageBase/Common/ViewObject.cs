using System;
using System.Collections.Generic;
using System.Windows;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Common
{
    public class ViewObject : IDisposable
    {
        #region Private Members

        private string m_Name;

        private uint m_ImageIndex;

        private uint m_MaxDetectorsPerBoard;

        private uint m_BitsPerPixel;

        private uint m_SamplingSpeed;

        private float m_SamplingSpace;

        private SourceObject m_HighEnergy;

        private SourceObject m_LowEnergy;

        private ViewType m_ViewType;

        private List<AnnotationInfo> m_Annotations;

        private List<System.Windows.Rect> m_TIPMarkings;

        #endregion Private Members


        #region Public Members

        public bool IsValid
        {
            get
            {
                return (HighEnergy != null || LowEnergy != null) ? true : false;
            }
        }

        public uint ImageIndex
        {
            get
            {
                return m_ImageIndex;
            }
        }

        public uint MaxDetectorsPerBoard
        {
            get
            {
                return m_MaxDetectorsPerBoard;
            }
        }

        public uint BitsPerPixel
        {
            get
            {
                return m_BitsPerPixel;
            }
        }

        public uint SamplingSpeed
        {
            get
            {
                return m_SamplingSpeed;
            }
        }

        public float SamplingSpace
        {
            get
            {
                return m_SamplingSpace;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public SourceObject HighEnergy
        {
            get
            {
                return m_HighEnergy;
            }
        }

        public SourceObject LowEnergy
        {
            get
            {
                return m_LowEnergy;
            }
        }

        public ViewType ViewType
        {
            get
            {
                return m_ViewType;
            }
        }

        public List<AnnotationInfo> Annotations
        {
            get
            {
                return m_Annotations;
            }

            set
            {
                m_Annotations = value;
            }
        }

        public List<System.Windows.Rect> TIPMarkings
        {
            get
            {
                return m_TIPMarkings;
            }
            set
            {
                m_TIPMarkings = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public ViewObject (string name, uint index, ViewType viewType, SourceObject highEnergy, SourceObject lowEnergy,
             uint maxDetectorsPerBoard, uint bitsPerPixel,  uint samplingSpeed, float samplingSpace, List<AnnotationInfo> annotations)
        {
            m_Name = name;
            m_ImageIndex = index;
            m_ViewType = viewType;
            m_HighEnergy = highEnergy;
            m_LowEnergy = lowEnergy;
            m_MaxDetectorsPerBoard = maxDetectorsPerBoard;
            m_BitsPerPixel = bitsPerPixel;
            m_SamplingSpeed = samplingSpeed;
            m_SamplingSpace = samplingSpace;
            m_Annotations = annotations;
            m_TIPMarkings = null;
        }

        #endregion Constructors


        #region Public Methods

        public void Dispose()
        {
            if (HighEnergy != null)
            {
                HighEnergy.Dispose();
            }

            if (LowEnergy != null)
            {
                LowEnergy.Dispose();
            }
        }

        #endregion Public Methods
    }
}
