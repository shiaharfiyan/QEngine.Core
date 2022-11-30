namespace QEngine.Core
{
    public struct QProduceResult
    {
        public QProduceResult()
        {

        }

        public bool IsSucceeded { get; set; } = false;
        public QItem Item { get; set; } = default!;
        public QThrottling Throttling { get; set; } = QThrottling.Block;
    }
}
