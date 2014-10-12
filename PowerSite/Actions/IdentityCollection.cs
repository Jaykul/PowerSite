using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSite.Actions
{
    public interface IIdentityObject
    {
        string Id { get; set; }
    }


    public class IdentityCollection : KeyedCollection<string, IIdentityObject>
    {
        public static IdentityCollection Create(IEnumerable<IIdentityObject> collection)
        {
            var namedCollection = new IdentityCollection();

            foreach (var layout in collection)
            {
                namedCollection.Add(layout);
            }

            return namedCollection;
        }

        protected override string GetKeyForItem(IIdentityObject item)
        {
            return item.Id;
        }
    }
}
