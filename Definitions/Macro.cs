using QuelleHMI.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Definitions
{
    public class Macro: IExpand
    {
        public IQuelleSearchControls search { get; private set; }
        public IQuelleDisplayControls display { get; private set; }

        public string label { get; private set; }
        public string expansion { get; private set; }
        public Macro(string label, HMIStatement statement)  // Persist
        {
            this.search = QuelleControlConfig.search.CreateCopy;
            this.display = QuelleControlConfig.display.CreateCopy;

            this.label = HMIStatement.SquenchText(label);
            this.expansion = "";
            var searches = (from key in statement.segmentation.Keys orderby key select statement.segmentation[key]);
            foreach (var clause in searches)
            {
                if (clause.verb == Search.FIND)
                {
                    var find = new QClauseSearch((Search)clause);
                    if (find.segment.Length > 0)
                    {
                        if (this.expansion.Length > 0)
                            this.expansion += " ; ";
                        if (find.polarity == '-')
                            this.expansion += "-- ";
                        this.expansion += HMIStatement.SquenchText(find.segment);
                    }
                }
            }
            this.Persist();
        }
        public Macro(string label)  // Retrieve
        {
            //  TODO: Read yaml
            QuelleControlConfig.search.Update("http://127.0.0.1:1611", null, 0, false);     // these need to be read from yaml
            QuelleControlConfig.display.Update(null, null, null);                           // these need to be read from yaml
        }
        private IExpand Persist()
        {
            string folder = Path.Combine(QuelleControlConfig.QuelleHome, "Labels");
            string path = Path.Combine(folder, this.label.Replace(' ', '~') + ".yaml");

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("search:");
                writer.WriteLine("\thost: "   + search.host != null   ? search.host   : "!!null");
                writer.WriteLine("\tdomain: " + search.domain != null ? search.domain : "!!null");
                writer.WriteLine("\tspan: "   + search.span != null   ? search.span.ToString() : "!!null");
                writer.WriteLine("\texact: "  + search.exact != null  ? (search.exact.Value ? "true" : "false") : "!!null");

                writer.WriteLine("search:");
                writer.WriteLine("\thost: "   + display.heading != null ? display.heading : "!!null");
                writer.WriteLine("\tdomain: " + display.format  != null ? display.format  : "!!null");
                writer.WriteLine("\tdomain: " + display.record  != null ? display.record  : "!!null");

                writer.WriteLine("definition:");
                writer.WriteLine("\tlabel: " + this.label     != null ? this.label     : "!!null");
                writer.WriteLine("\tmacro: " + this.expansion != null ? this.expansion : "!!null");
            }
            return this;
        }
    }
}
