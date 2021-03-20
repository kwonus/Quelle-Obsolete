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
            if (statement != null)
            {
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
            }
            // statement.Notify("warning", "There is no active search statement; This label will only preserve control variables if any are activated");

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
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, this.label.Replace(' ', '~') + ".yaml");

            using (StreamWriter writer = new StreamWriter(path))
            {
                if (search.host != null || search.domain != null || search.span != null || search.exact != null)
                {
                    writer.WriteLine("search:");
                    if (search.host != null)
                        writer.WriteLine("\thost: " +  search.host);
                    if (search.domain != null)
                        writer.WriteLine("\tdomain: " + search.domain);
                    if (search.span != null)
                        writer.WriteLine("\tspan: " + search.span.ToString());
                    if (search.exact != null)
                        writer.WriteLine("\texact: " + (search.exact.Value ? "true" : "false"));
                }
                if (display.heading != null || display.format != null || display.record != null)
                {
                    writer.WriteLine("display:");
                    if (display.heading != null)
                        writer.WriteLine("\theading: " + display.heading);
                    if (display.format != null) 
                        writer.WriteLine("\tformat: " + display.format);
                    if (display.record != null) 
                        writer.WriteLine("\trecord: " + display.record);
                }
                writer.WriteLine("definition:");
                writer.WriteLine("\tlabel: " + this.label     != null ? this.label     : "!!null");
                writer.WriteLine("\tmacro: " + this.expansion != null ? this.expansion : "!!null");
            }
            return this;
        }
    }
}
