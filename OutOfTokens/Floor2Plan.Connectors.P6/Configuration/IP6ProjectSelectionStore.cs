using System.Collections.Generic;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Configuration
{
    /// <summary>
    /// Persists which P6 projects an administrator selected on the connector configuration screen.
    /// </summary>
    public interface IP6ProjectSelectionStore
    {
        Task<IReadOnlyCollection<string>> GetSelectedProjectIdsAsync();

        Task SaveSelectedProjectIdsAsync(IEnumerable<string> projectIds);
    }
}
