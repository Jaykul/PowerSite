using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerSite.DataModel
{
    public interface IIdentityObject
    {
        string Id { get; set; }
    }


    public class IdentityCollection<T> : KeyedCollection<string, T> where T : IIdentityObject
    {
        public static IdentityCollection<T> Create(IEnumerable<T> collection)
        {
            var namedCollection = new IdentityCollection<T>();

            foreach (var layout in collection)
            {
                namedCollection.Add(layout);
            }

            return namedCollection;
        }
	
        protected override string GetKeyForItem(T item)
        {
            return item.Id;
        }
    }
}
