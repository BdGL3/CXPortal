using System.ServiceModel;
using System;

namespace L3.Cargo.Communications.Common
{
    public class TCPBinding : NetTcpBinding
    {
        public TCPBinding() :
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
        }
    }
}
