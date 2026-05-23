using Api.Contracts.Collections;

namespace Api.Contracts
{
    public interface IDocumentsProvider
    {
        IPagedResourceCollection<IDocument> GetDocuments(string[] fields, int offSet, int limit);
    }
}