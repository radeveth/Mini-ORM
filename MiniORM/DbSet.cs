namespace MiniORM
{
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    class DbSet<TEntity> : ICollection<TEntity>
        where TEntity : class, new()
    {
        internal DbSet(IEnumerable<TEntity> entities)
        {
            this.Entities = entities.ToList();
            this.ChangeTracker = new ChangeTracker<TEntity>(entities);
        }


        internal ChangeTracker<TEntity> ChangeTracker { get; set; }
        public IList<TEntity> Entities { get; set; }


        public int Count => this.Entities.Count;

        public bool IsReadOnly => this.Entities.IsReadOnly;

        public void Add(TEntity item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), ExceptionMessages.ItemNullException);
            }

            this.Entities.Add(item);
            this.ChangeTracker.Add(item);
        }

        public bool Remove(TEntity item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), ExceptionMessages.ItemNullException);
            }

            bool isremovedEntitySuccessfuly = this.Entities.Remove(item);

            if (isremovedEntitySuccessfuly)
            {
                this.ChangeTracker.Remove(item);
            }

            return isremovedEntitySuccessfuly;
        }

        public void Clear()
        {
            while (this.Entities.Any())
            {
                TEntity currentEntity = this.Entities.First();
                this.Remove(currentEntity);
            }
        }

        public bool Contains(TEntity item)
            => this.Entities.Contains(item);

        public void CopyTo(TEntity[] array, int arrayIndex)
            => this.Entities.CopyTo(array, arrayIndex);

        public void RemoveRange(ICollection<TEntity> entitiesToRemove)
        {
            foreach (TEntity entity in entitiesToRemove)
            {
                this.Remove(entity);
            }
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return this.Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}