using System;
using System.Threading;
using GHIElectronics.NETMF.Native;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using L3.Cargo.Communications.APCS.Common;

namespace L3.Cargo.APCS
{
    public class PulseCounter : IDisposable
    {
        #region Declarations

        private RLP.Procedure initProcedure;
        private RLP.Procedure deinitProcedure;
        private RLP.Procedure queryProcedure;
        private RLP.Procedure updateFreqProcedure;
        private RLP.Procedure updatePulseProcedure;
        private RLP.Procedure resetProcedure;
        private RLP.Procedure stopProcedure;
        private RLP.Procedure startProcedure;
        private RLP.Procedure PWMStatusProcedure;

        private Thread thread;
        private bool terminated = false;

        private float CurrentSpeedMPH = 0.0f;

        private OperatingMode opMode;

        private float PortalInOutRatio = 1.0f;
        private float MobileInOutRatio = 1.0f;

        private short PortalMinFrequency = 100;
        private short MobileMinFrequency = 100;

        private short PortalMaxFrequency = 2000;
        private short MobileMaxFrequency = 2000;

        #endregion

        #region Construction / destruction

        /// <summary>
        /// Creates a new instance of the PulseCounter class.
        public PulseCounter(OperatingMode mode)
        {
            opMode = mode;
        }

        ~PulseCounter()
        {
            Dispose();
        }

        /// <summary>
        /// Release used resources.
        /// </summary>
        public void Dispose()
        {
            // Stop thread
            if (thread != null)
            {
                terminated = true;
                thread.Join();
                thread = null;
            }

            // Deinit RLP driver
            if (deinitProcedure != null)
            {
                deinitProcedure.Invoke();
                deinitProcedure = null;
                initProcedure = null;
                queryProcedure = null;
            }
        }       

        #endregion

        #region Public methods

        /// </summary>
        /// <param name="elfImage">elf image library</param>
        public void Initialize(byte[] elfImage)
        {
            // Load procedures
            initProcedure = RLP.GetProcedure(elfImage, "Init");
            deinitProcedure = RLP.GetProcedure(elfImage, "Deinit");
            queryProcedure = RLP.GetProcedure(elfImage, "Query");
            updateFreqProcedure = RLP.GetProcedure(elfImage, "PWMUpdateFreq");
            updatePulseProcedure = RLP.GetProcedure(elfImage, "PWMUpdatePulse");
            resetProcedure = RLP.GetProcedure(elfImage, "WDReset");
            stopProcedure = RLP.GetProcedure(elfImage, "PwmStop");
            startProcedure = RLP.GetProcedure(elfImage, "PWMRun");
            PWMStatusProcedure = RLP.GetProcedure(elfImage, "QueryPWMStatus");

            // Init RLP driver
            if (initProcedure.Invoke() != 0)
            {
                throw new Exception("Driver initialisation failed");
            }

            // Start thread
            thread = new Thread(ThreadProc);
            thread.Start();
        }

        public void ResetBoard()
        {
            resetProcedure.Invoke();
        }

        public void SetOperatingMode(OperatingMode mode) { opMode = mode; }

        public void SetInputToOutputRatio(OperatingMode mode, float ratio) 
        {
            if (mode == OperatingMode.AdaptiveMobile)
                MobileInOutRatio = ratio;
            else if (mode == OperatingMode.AdaptivePortal)
                PortalInOutRatio = ratio;
        }

        public void GetInputToOutputRatio(OperatingMode mode, out float ratio) 
        {
            ratio = 0.0f;

            if (mode == OperatingMode.AdaptiveMobile)
                ratio = MobileInOutRatio;
            else if (mode == OperatingMode.AdaptivePortal)
                ratio = PortalInOutRatio;
        }

        public void SetFrequencyRange(OperatingMode mode, short minFreq, short maxFreq)
        {
            if (mode == OperatingMode.AdaptiveMobile)
            {
                MobileMinFrequency = minFreq;
                MobileMaxFrequency = maxFreq;
            }
            else if (mode == OperatingMode.AdaptivePortal)
            {
                PortalMinFrequency = minFreq;
                PortalMaxFrequency = maxFreq;
            }
        }

        public void GetFrequencyRange(OperatingMode mode, out short minFreq, out short maxFreq) 
        {
            minFreq = maxFreq = 0;

            if (mode == OperatingMode.AdaptiveMobile)
            {
                minFreq = MobileMinFrequency;
                maxFreq = MobileMaxFrequency;
            }
            else if (mode == OperatingMode.AdaptivePortal)
            {
                minFreq = PortalMinFrequency;
                maxFreq = PortalMaxFrequency;
            }
        }
        
        public float GetCurrentSpeed() 
        {
            if ((opMode == OperatingMode.AdaptiveMobile) || (opMode == OperatingMode.AdaptivePortal))
                return CurrentSpeedMPH;
            else
                return 0.0f;
        }

        public void UpdatePWMFrequency(int freq) { updateFreqProcedure.Invoke(freq); }
        public void UpdatePWMPulseWidth(float width) { updatePulseProcedure.Invoke(width); }
        public void PWMOutputDisable() { stopProcedure.Invoke(); }
        public void PWMOutputEnable() { startProcedure.Invoke(); }
        public int GetPWMRunStatus() { return PWMStatusProcedure.Invoke(); }

        #endregion

        #region Private methods

        private void ThreadProc()
        {
            ushort[] data = new ushort[5];
            ushort edges = 0;
            float inputToOutputRatio;
            short MinPWMFrequency, MaxPWMFrequency;
                         
            while (!terminated)
            {
                queryProcedure.InvokeEx(data);

                if (data[0] > 0)
                {
                    if (opMode == OperatingMode.AdaptiveMobile)
                    {
                        inputToOutputRatio = MobileInOutRatio;
                        MinPWMFrequency = MobileMinFrequency;
                        MaxPWMFrequency = MobileMaxFrequency;
                        edges = data[1];
                    }
                    else
                    {
                        inputToOutputRatio = PortalInOutRatio;
                        MinPWMFrequency = PortalMinFrequency;
                        MaxPWMFrequency = PortalMaxFrequency;
                        edges = (data[2] > data[3]) ? data[2] : data[3];
                    }

                    // speed in miles per hour
                    CurrentSpeedMPH = ((float)edges / (float)data[0]) * 10.0f;
                                        
                    // update PWM rate
                    if ((opMode == OperatingMode.AdaptiveMobile) || (opMode == OperatingMode.AdaptivePortal))
                    {
                        int frequency = 
                            (int)System.Math.Round(CurrentSpeedMPH * 100.0f * inputToOutputRatio);

                        if (frequency < MinPWMFrequency) frequency = (int)MinPWMFrequency;
                        if (frequency > MaxPWMFrequency) frequency = (int)MaxPWMFrequency;

                        UpdatePWMFrequency(frequency);
                    }

                   // Debug.Print("Time: " + data[0].ToString() + "ms Edges: " + edges.ToString() + " Speed: " + CurrentSpeedMPH.ToString());
                }                   
                
                Thread.Sleep(300);
            }
        }

        #endregion
    }
}
