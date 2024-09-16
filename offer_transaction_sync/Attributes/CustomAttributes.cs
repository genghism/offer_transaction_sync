namespace offer_transaction_sync.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataSourceAttribute(string _SourceName, bool _IsKey = false) : Attribute 
    {
        public string SourceName { get; } = _SourceName;
        public bool IsKey { get; } = _IsKey;
    }
}
