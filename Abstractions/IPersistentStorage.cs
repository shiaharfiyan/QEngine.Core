namespace QEngine.Core.Abstractions
{
    public interface IPersistentStorage : IStorage
    {
        void Open();
        int Execute(string command);
        int Execute();
        void Close();
    }
}