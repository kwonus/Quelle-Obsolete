using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Interop
{
    public interface IQClient
    {
        string Get(string endpoint, UInt16 maxResponseLength = 1024);
        (byte[] data, int length) Post(string endpoint, byte[] payload, string mimetype, UInt16 maxResponseLength = 1024);
    }
}
