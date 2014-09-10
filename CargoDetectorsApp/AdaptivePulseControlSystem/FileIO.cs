using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.IO;

namespace L3.Cargo.APCS
{
    public class FileIO
    {
        public static void Test()
        {

            // Create new Thread that runs the ExampleThreadFunction
            Thread ExampleThread = new Thread(new ThreadStart(ExampleThreadFunction));

            // SD stuff is in
            PersistentStorage sdPS = new PersistentStorage("SD");

            // Led stuff is in 
            OutputPort LED;
            LED = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, true);

            // Button stuff in
            InputPort Button;
            Button = new InputPort((Cpu.Pin)FEZ_Pin.Digital.LDR, false,
                                   Port.ResistorMode.PullUp);


            while (true)
            {
                //Led status at the beginning is off
                LED.Write(false);

                if (Button.Read())
                {

                    while (Button.Read()) ;   // wait while busy

                    //Led is on
                    LED.Write(true);

                    // Mount
                    sdPS.MountFileSystem();

                    // Start our new Thread
                    ExampleThread.Start();

                    while (Button.Read()) ;   // wait while busy

                    //Led is off
                    LED.Write(true);

                    // Abort our new Thread
                    ExampleThread.Abort();

                    // Unmount
                    sdPS.UnmountFileSystem();
                }

            }



        }


        public static void ExampleThreadFunction()
        {

            // bit rate change acording your GPS
            SerialPort serialPort = new SerialPort("COM3", 4800);
            serialPort.Open();

            //here we create file in SD card main folder
            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
            FileStream FileHandle = new FileStream(rootDirectory + @"\gps.txt", FileMode.Create);


            while (true)
            {

                int bytesToRead = serialPort.BytesToRead;
                if (bytesToRead > 0)
                {

                    // all struff from GPS streams in to file
                    byte[] buffer = new byte[bytesToRead];
                    serialPort.Read(buffer, 0, buffer.Length);
                    Debug.Print(new String(System.Text.Encoding.UTF8.GetChars(buffer)) + "\n");
                    FileHandle.Write(buffer, 0, buffer.Length);
                    Thread.Sleep(500);
                }
                // Cleaning 
                Debug.GC(true);
                Debug.EnableGCMessages(false);

            }

        }
    }

}