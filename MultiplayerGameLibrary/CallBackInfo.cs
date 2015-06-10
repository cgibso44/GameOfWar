using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace MultiplayerGameLibrary
{
    [DataContract]
    public class CallbackInfo
    {
        [DataMember]
        public int NumCards { get; private set; }
        [DataMember]
        public int NumDecks { get; private set; }
        [DataMember]
        public bool Reset { get; private set; }

        public CallbackInfo(int c, int d, bool r)
        {
            NumCards = c;
            NumDecks = d;
            Reset = r;
        }
    }
}
