namespace Reimers.Ihe.Communication
{
    using System;
    using System.Net;


    public class ClientConnectionEventArgs : EventArgs
    {
       
        public IPEndPoint EndPoint { get; set; }
    }
}
