namespace QEngine.Core
{
    public class QProperties
    {
        public bool Persistent { get; set; } = false;
        public bool AutoDelete { get; set; } = false;

        public QAcknowledgement Acknowledge { get; set; } = QAcknowledgement.SemiAutomatic;
        public QAcknowledgement Commit { get; set; } = QAcknowledgement.SemiAutomatic;

        public override string ToString()
        {
            var contentList = new List<string>
            {
                $"{(Persistent ? "Persistent" : "")}",
                $"{(AutoDelete ? "Auto Delete" : "")}",
                $"{Acknowledge}",
                $"{Commit}"
            };
            return string.Join(",", contentList.ToArray());
        }
    }
}
