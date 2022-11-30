namespace QEngine.Core
{
    public struct QConsumeResult
    {
        public QConsumeResult()
        {

        }

        public bool IsSucceeded { get; set; } = false;
        public QItem Item { get; set; } = default!;
    }
}
