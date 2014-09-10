using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace L3.Cargo.Subsystem.DataAccessCore
{
    public class UpdateTcpBinding : NetTcpBinding
    {
        public UpdateTcpBinding() :
            base()
        {
            Int32 buffSize = 268435456;

            base.Security.Mode = SecurityMode.None;
            base.MaxBufferPoolSize = buffSize;
            base.MaxBufferSize = buffSize;
            base.MaxReceivedMessageSize = buffSize;
            base.ReaderQuotas.MaxArrayLength = buffSize;
            base.ReaderQuotas.MaxStringContentLength = buffSize;
            base.ReaderQuotas.MaxDepth = buffSize;
            base.ReaderQuotas.MaxBytesPerRead = buffSize;
            base.ReaderQuotas.MaxNameTableCharCount = buffSize;
            base.OpenTimeout = TimeSpan.FromMilliseconds(5000);
            base.SendTimeout = TimeSpan.FromMilliseconds(5000);
            base.CloseTimeout = TimeSpan.FromMilliseconds(5000);
        }
    }
}
