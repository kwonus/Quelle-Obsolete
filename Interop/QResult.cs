using MessagePack;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QResult : IQuelleResult
    {
        public QResult() { /*for msgpack*/ }

        [Key(1)]
        public bool success { get; set; }
        [Key(2)]
        public string[] errors { get; set; }
        [Key(3)]
        public string[] warnings { get; set; }

        public QResult(IQuelleResult iresult)
        {
            this.success = iresult.success;
            this.errors = iresult.errors;
            this.warnings = iresult.warnings;
        }
    }
}
