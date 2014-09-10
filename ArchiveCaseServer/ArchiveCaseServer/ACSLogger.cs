using System;
using L3.Cargo.Communications.Common;

namespace L3.Cargo.ArchiveCaseServer
{
    public class ACSLogger: Logger
    {
        MainWindow log;

        public ACSLogger(MainWindow logWindow)
        {
            log = logWindow;
        }

        public override void Print(String message)
        {
            log.LogMessage(message);
        }

        public override void PrintLine(String message)
        {
            log.LogMessage(message + "\r");
        }

        public override void PrintLine()
        {
            log.LogMessage("\r");
        }

        public override void PrintInfoLine(String message)
        {
            DateTime now = DateTime.Now;
            log.LogMessage(now + ":" + now.Millisecond + ": " + message + "\r");
        }

        public override void RefreshCaseList()
        {
            //log.RefreshCaseList();
        }
        
    }
}
