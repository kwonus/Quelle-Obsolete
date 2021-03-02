using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Interop
{
    class QMD5
    {
        public UInt64 part_1;
        public UInt64 part_2;

        public QMD5(Guid md5)
        {
            this.part_1 = 0;
            this.part_2 = 0;
        }
        public QMD5(string str)
        {
            this.part_1 = 0;
            this.part_2 = 0;
        }
        public QMD5(UInt64 part_1, UInt64 part_2)
        {
            this.part_1 = part_1;
            this.part_2 = part_2;
        }
    }
}
