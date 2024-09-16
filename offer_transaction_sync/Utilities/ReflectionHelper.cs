using offer_transaction_sync.Attributes;
using System.Data.SqlClient;
using System.Reflection;

namespace offer_transaction_sync.Utilities
{
    public static class ReflectionHelper
    {
        public static IEnumerable<PropertyInfo> GetProperties<T>(bool? isKey = null)
        {
            return typeof(T).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<DataSourceAttribute>();
                return attr != null && (!isKey.HasValue || attr.IsKey == isKey.Value);
            });
        }
        public static object GetValueFromReader(SqlDataReader reader, PropertyInfo property, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return property.PropertyType.Name switch
            {
                "Decimal" => reader.GetDecimal(ordinal),
                "Boolean" => reader.GetBoolean(ordinal),
                "Byte" => reader.GetByte(ordinal),
                "Int32" => reader.GetInt32(ordinal),
                "String" => reader.GetString(ordinal),
                "DateTime" => reader.GetDateTime(ordinal),
                _ => throw new NotSupportedException($"Type {property.PropertyType.Name} is not supported. Columnname : {columnName} Property : {property.GetCustomAttribute<DataSourceAttribute>().SourceName}")
            };
        }
    }
}
