using System;

namespace Api.Dto.Models.Base
{
    internal static class UrlFactory
    {
        public static string BaseUrl = "http://localhost/";
        public const string DocumentResourcePrefix = "documents";
        public const string CompanyResourcePrefix = "companies";
        public const string TransactionProposalResourcePrefix = "transactionProposals";

        public static string GetPrefix(Type objecType)
        {
            if (objecType == typeof (Document) || objecType == typeof (DocumentReference))
            {
                return DocumentResourcePrefix;
            }
            if (objecType == typeof(Company) || objecType == typeof(CompanyReference))
            {
                return CompanyResourcePrefix;
            }
            return string.Empty;
        }
    }
}