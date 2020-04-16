using System;

namespace MEE7.Backend
{
    public class IllegalCommandException : Exception
    {
        public IllegalCommandException(string message) : base(message)
        {

        }
    }
}
