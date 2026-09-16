using System.IO;

namespace Floor2Plan.Connectors.P6.Messages
{
    public sealed record P6RawData(MemoryStream Content, string FileName, string MimeType);
}
