namespace QEngine.Core
{
    public class QRoute
    {
        public string Route { get; set; } = "/";
        public Delegate Delegate { get; set; } = () => "";
    }
}
