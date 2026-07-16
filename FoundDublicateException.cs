using System;
using System.Collections.Generic;
using System.Text;

namespace telegram_Dictionary
{
    class FoundDublicateException : Exception
    {
        public required String MethodName { get; set; }
    }
}
