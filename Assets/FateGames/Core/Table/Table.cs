using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FateGames.Core
{
    public abstract class Table<T> : ScriptableObject where T : TableEntity
    {
        [SerializeField] protected List<T> entities;
        protected Dictionary<string, T> table;

        public T this[string key]
        {
            get => table[key];
        }

        public List<T> Values { get => table.Values.ToList(); }

        public bool ContainsKey(string key) => table.ContainsKey(key);

        private void OnEnable()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            table = new();
            for (int i = 0; i < entities.Count; i++)
            {
                T entity = entities[i];
                table.Add(entity.Tag, entity);
            }
        }
    }
    public abstract class TableEntity : ScriptableObject
    {
        [SerializeField] private string tag;

        public string Tag { get => tag; }
    }
}
