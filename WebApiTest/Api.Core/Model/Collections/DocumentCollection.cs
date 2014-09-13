using System.Collections.Generic;

namespace Api.Core.Model.Collections
{
    public class DocumentCollection : Collection<DocumentReference>
    {
        public DocumentCollection(IEnumerable<DocumentReference> items, int total) : base(items, total)
        {
        }

    }
}