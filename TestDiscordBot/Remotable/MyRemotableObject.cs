using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace RemotableObjects
{
	public class MyRemotableObject : MarshalByRefObject
	{
		public MyRemotableObject()
		{
		
		}

		public void SetMessage(string message)
		{
			Cache.GetInstance().MessageString = message;
		}
	}
}
