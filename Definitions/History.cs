using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Definitions
{
    //  This provides universal expansion of {macro} labels and {1} review identifiers *which is the motivation for combining them behind a single interface
    public class History: IExpand
    {
        public static Dictionary<UInt32, string> Review = new Dictionary<UInt32, string>(); 
        public UInt32 id { get; private set; }
        public string label { get => id.ToString(); }
        public string expansion { get; private set; }

        public History(UInt32 id, HMIStatement statement)   // Persist
        {
            this.id = id;
            this.expansion = statement != null && statement.statement != null ? HMIStatement.SquenchText(statement.statement) : "";
            this.Persist();
        }
        public History(UInt32 id)   // Retrieve
        {
            this.id = id;
            this.expansion = Review[this.id];
        }
        private IExpand Persist()
        {
            Review.Add(this.id, this.expansion);    // THis should eventually go to a file, but this is okay for now
            return this;
        } 
    }

}
