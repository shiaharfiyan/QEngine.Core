namespace QEngine.Core
{
    public class QBinder
    {
        public QBinder()
        {
            CancellationTokenSource.Token.ThrowIfCancellationRequested();
        }
        public QOrigin Origin { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
    }
}
