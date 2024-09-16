using offer_transaction_sync.Utilities;
using System.Linq.Expressions;
using offer_transaction_sync.Models;
using offer_transaction_sync.Attributes;
using System.Data.SqlClient;
using System.Reflection;

namespace offer_transaction_sync.Services
{
    public class EntityService
    {
        public Entity FindExistingEntity(Entity entity, IQueryable<Entity> existingEntities)
        {
            var keyProperties = ReflectionHelper.GetProperties<Entity>(isKey:true).ToList();

            if (!keyProperties.Any())
            {
                throw new InvalidOperationException("No key properties found for Entity");
            }

            var parameter = Expression.Parameter(typeof(Entity), "e");

            var equalExpressions = keyProperties.Select(prop =>
                Expression.Equal(
                    Expression.Property(parameter, prop),
                    Expression.Constant(prop.GetValue(entity), prop.PropertyType)
                )
            );

            var combinedExpression = equalExpressions.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<Entity, bool>>(combinedExpression, parameter);

            return existingEntities.FirstOrDefault(lambda);
        }

        public static List<Entity> ReadEntitiesDynamic(SqlCommand command)
        {
            var entities = new List<Entity>();
            var properties = typeof(Entity).GetProperties()
                .Where(p => p.GetCustomAttribute<DataSourceAttribute>() != null)
                .ToList();

            using SqlDataReader reader = command.ExecuteReader();
            try { 
            while (reader.Read())
            {
                var entityValues = new object[properties.Count];

                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var attribute = property.GetCustomAttribute<DataSourceAttribute>();
                    var columnName = attribute.SourceName;

                    entityValues[i] = ReflectionHelper.GetValueFromReader(reader, property, columnName);
                }

                var entity = (Entity)Activator.CreateInstance(typeof(Entity), entityValues);
                entities.Add(entity);
            }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return entities;
        }

        public bool EntitiesAreDifferent(Entity entity1, Entity entity2)
        {
            var properties = ReflectionHelper.GetProperties<Entity>(isKey: false);

            foreach (var property in properties)
            {
                var value1 = property.GetValue(entity1);
                var value2 = property.GetValue(entity2);

                if (!Equals(value1, value2))
                {
                    return true;
                }
            }

            return false;
        }

    }

}
