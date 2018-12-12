using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace RemotableObjects
{
	public class frmRServer : IObserver
	{
        public delegate void ReceivedMessage(string text);
        private MyRemotableObject remotableObject;
		private Container components = null;
        private ReceivedMessage gotMessage;

		public frmRServer(ReceivedMessage gotMessage)
		{
			remotableObject = new MyRemotableObject();
			
			//************************************* TCP *************************************//
			// using TCP protocol
			TcpChannel channel = new TcpChannel(8080);
			ChannelServices.RegisterChannel(channel, false);
			RemotingConfiguration.RegisterWellKnownServiceType(typeof(MyRemotableObject),"HelloWorld",WellKnownObjectMode.Singleton);
			//************************************* TCP *************************************//
			Cache.Attach(this);
            this.gotMessage = gotMessage;
		}
        
		public void Notify(string text)
		{
            gotMessage(text);
        }
	}
}
