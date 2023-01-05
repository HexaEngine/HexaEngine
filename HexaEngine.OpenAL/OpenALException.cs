namespace HexaEngine.OpenAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class OpenALException : Exception
    {
        internal OpenALException(string msg) : base(msg)
        {
        }
    }
}