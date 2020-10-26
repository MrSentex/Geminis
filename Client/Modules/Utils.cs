using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geminis.Client.Modules
{
    class Utils : InterconnectClient
    {
        public bool CompareLists(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count) { return false; }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
