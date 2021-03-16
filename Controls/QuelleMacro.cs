using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Controls
{
    public class QuelleMacro
    {
        public CTLSearch search { get; private set; }
        public CTLDisplay display { get; private set; }
        public CTLSystem system { get; private set; }

        public string label { get; private set; }
        public string macro { get; private set; }

        private QuelleMacro(string label, string macro)
        {
            ;
        }
        public static QuelleMacro Create(string label, string macro)
        {
            return new QuelleMacro(label, macro);
        }
        public static QuelleMacro Read(string label)
        {
            return null;
        }
    }
}
