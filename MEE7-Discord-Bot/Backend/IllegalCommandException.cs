using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Backend
{
    public class IllegalCommandException : Exception
    {
        public IllegalCommandException(string message) : base(message)
        {

        }
    }
}
