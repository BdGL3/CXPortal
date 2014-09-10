using System;

namespace L3.Cargo.Communications.Common
{
    [Serializable()]
    public class CaseIdNotFoundException: System.Exception
    {
        public CaseIdNotFoundException() : base() { }
        public CaseIdNotFoundException(string message) : base(message) { }
        public CaseIdNotFoundException(string message, System.Exception inner) : base(message, inner) { }

        //Constructor needed for Serialization
        //when exception propagates from a remoting server to the client
        protected CaseIdNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
