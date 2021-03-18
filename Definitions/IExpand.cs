using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Definitions
{
    public interface IExpand
    {
        string label { get; }
        string expansion { get; }

        static IExpand Create(string label, HMIStatement statement)
        {
            if (string.IsNullOrEmpty(label) || (statement == null || statement.segmentation.Count < 1))
                return null;

            if (char.IsLetter(label[0]))
                return new Macro(label, statement);   // Persist

            var isNumeric = false;
            for (int c = 0; c < label.Length; c++)
            {
                isNumeric = char.IsDigit(label[c]);
                if (!isNumeric)
                    break;
            }
            return isNumeric ? new History(UInt32.Parse(label), statement) : null;
        }
        public static IExpand Expand(string label)
        {
            if (string.IsNullOrEmpty(label))
                return null;

            if (char.IsLetter(label[0]))
                return new Macro(label);   // Retrieve

            var isNumeric = false;
            for (int c = 0; c < label.Length; c++)
            {
                isNumeric = char.IsDigit(label[c]);
                if (!isNumeric)
                    return null;
            }
            return new History(UInt32.Parse(label)); // Retrieve
        }
    }
}
