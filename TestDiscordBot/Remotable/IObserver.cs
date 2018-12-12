using System;

namespace RemotableObjects
{
	public interface IObserver
	{
		void Notify(string text);
	}
}
