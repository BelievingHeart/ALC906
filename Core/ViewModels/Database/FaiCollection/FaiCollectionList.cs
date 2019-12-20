using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Core.ViewModels.Database.FaiCollection
{
    public class FaiCollectionList : Collection<IFaiCollection>, ITypedList
    {
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return TypeDescriptor.GetProperties(Count > 0 ? this[0].GetType() : typeof(IFaiCollection));
        }
        
        public FaiCollectionList(IList<IFaiCollection> source) : base(source) { }

    }
}