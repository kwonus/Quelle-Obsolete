using QuelleHMI.Definitions;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QSearchControls
    {
        public QSearchControls()    /* for serialization*/
        {
            ;
        }
        public QSearchControls(bool initialize) : base()    /* for serialization*/
        {
            if (initialize)
            {
                this.xdomain = QuelleControlConfig.search.domain;
                this.xexact  = QuelleControlConfig.search.exact.Value;
                this.xspan   = QuelleControlConfig.search.span.Value;
                this.xhost   = QuelleControlConfig.search.host;
                this.setControlDefaults();
            }
        }
        private void setControlDefaults()
        {
            if (this.xdomain == null)
                this.xdomain = CTLSearch.defaultDomain;
            if (this.xexact == null)
                this.xexact = CTLSearch.defaultExact;
            if (this.xspan == null)
                this.xspan = CTLSearch.defaultSpan;
            if (this.xhost == null)
                this.xhost = CTLSearch.defaultHost;
        }

        private string xhost { get; set; }
        private string xdomain { get; set; }
        private uint? xspan { get; set; }
        private bool? xexact { get; set; }

        [DataMember]
        public string host
        {
            get => xhost != null ? xhost : CTLSearch.defaultHost;
            set => xhost = value;
        }
        [DataMember]
        public string domain
        {
            get => xdomain != null ? xdomain : CTLSearch.defaultDomain;
            set => xdomain = value;
        }
        [DataMember]
        public uint span
        {
            get => xspan.HasValue ? xspan.Value : CTLSearch.defaultSpan;
            set => xspan = value;
        }
        [DataMember]
        public bool exact
        {
            get => xexact.HasValue ? xexact.Value : CTLSearch.defaultExact;
            set => xexact = value;
        }
        public QSearchControls(IQuelleSearchControls icontrols)
        {
            this.xhost   = icontrols.host;
            this.xdomain = icontrols.domain;
            this.xspan   = icontrols.span.Value;
            this.xexact  = icontrols.exact.Value;
        }
    }
}
