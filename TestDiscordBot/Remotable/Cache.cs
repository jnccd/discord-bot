using System;

namespace RemotableObjects
{
	public class Cache
	{
		private static Cache myInstance;
		public static IObserver Observer;

		private Cache()
		{

		}
		
		public static void Attach(IObserver observer)
		{
			Observer = observer;
		}
		public static Cache GetInstance()
		{
			if(myInstance==null)
			{
				myInstance = new Cache();
			}
			return myInstance;
		}
		public string MessageString
		{
			set 
			{
				Observer.Notify(value);
			}
		}
	}
}
