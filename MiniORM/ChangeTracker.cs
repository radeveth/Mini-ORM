namespace MiniORM
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    // Entity classes must be reference types and instance
    internal class ChangeTracker<TEntity>
        where TEntity : class, new()
    {
        // All entities
        // Copy of the original entities
        private readonly IList<TEntity> allEntities;

        // Added, but not change saved
        private readonly IList<TEntity> added;

        // Removed, but not change saved
        private readonly IList<TEntity> removed;


        private ChangeTracker()
        {
            this.added = new List<TEntity>();
            this.removed = new List<TEntity>();
        }

        public ChangeTracker(IEnumerable<TEntity> entities)
            : this()
        {
            this.allEntities = CloneEntities(entities);
        }

        public IReadOnlyCollection<TEntity> AllEntities => (IReadOnlyCollection<TEntity>)this.allEntities;
        public IReadOnlyCollection<TEntity> Added => (IReadOnlyCollection<TEntity>)this.added;
        public IReadOnlyCollection<TEntity> Removed => (IReadOnlyCollection<TEntity>)this.removed;

        public void Add(TEntity entity)
            => this.added.Add(entity);

        // This method physically not deleting
        // Add entity -> to collection removed
        // Say that this entities needs to be deleted when method SaveChanges() is invoked
        public void Remove(TEntity entity)
            => this.removed.Remove(entity);

        public IEnumerable<TEntity> GetModifiedEntities(DbSet<TEntity> dbSet)
        {
            IList<TEntity> modifiedEntities = new List<TEntity>();

            PropertyInfo[] primaryKeys = typeof(TEntity)
                .GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .ToArray();

            foreach (TEntity proxyEntity in this.AllEntities)
            {
                IEnumerable<object> proxyEntitiesPrimaryKeysValues = 
                    GetPrimaryKeysValues(primaryKeys, proxyEntity).ToArray();

                TEntity originalEntity = dbSet.Entities
                    .Single(e => GetPrimaryKeysValues(primaryKeys, e)
                    .SequenceEqual(proxyEntitiesPrimaryKeysValues));

                bool isModified = IsModified(originalEntity, proxyEntity);

                if (isModified)
                {
                    modifiedEntities.Add(originalEntity);
                }
            }

            return modifiedEntities;
        }

        private List<TEntity> CloneEntities(IEnumerable<TEntity> entitties)
        {
            var cloneEntities = new List<TEntity>();

            PropertyInfo[] propertiesToClone = typeof(TEntity)
                .GetProperties()
                .Where(pI => DbContext.AllowedSqlTypes.Contains(pI.PropertyType))
                .ToArray();

            foreach (TEntity originalEntity in entitties)
            {
                TEntity clonedEntity = Activator.CreateInstance<TEntity>();

                foreach (PropertyInfo property in propertiesToClone)
                {
                    object value = property.GetValue(originalEntity);
                    property.SetValue(clonedEntity, value);
                }

                cloneEntities.Add(clonedEntity);
            }

            return cloneEntities;
        }

        private static IEnumerable<object> GetPrimaryKeysValues(IEnumerable<PropertyInfo> primatyKeys, TEntity entity)
        {
            return primatyKeys.Select(pk => pk.GetValue(entity));
        }

        private static bool IsModified(TEntity originalEntity, TEntity proxy)
        {
            IEnumerable<PropertyInfo> monitoredProperty = typeof(TEntity)
                .GetProperties().Where(pi => DbContext.AllowedSqlTypes.Contains(pi.PropertyType));

            ICollection<PropertyInfo> modifiedProperties = monitoredProperty
                .Where(pi => !Equals(pi.GetValue(originalEntity), pi.GetValue(proxy)))
                .ToArray();

            return modifiedProperties.Any();
        }
    }
}