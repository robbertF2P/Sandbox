using Floor2Plan.Connectors.P6.Api.Models;
using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Api
{
    public interface IP6RestApi
    {
        public const string RestApiPath = "/p6ws/restapi";
        public const string LoginPath = RestApiPath + "/login";
        public const string LogoutPath = RestApiPath + "/logout";
        public const string ProjectPath = RestApiPath + "/project";
        public const string WbsPath = RestApiPath + "/wbs";
        public const string ActivityPath = RestApiPath + "/activity";
        public const string ResourcePath = RestApiPath + "/resource";
        public const string ResourceAssignmentPath = RestApiPath + "/resourceAssignment";
        public const string RelationshipPath = RestApiPath + "/relationship";
        public const string AuthTokenHeaderName = "authToken";
        public const string CookieHeaderName = "Cookie";
        public const string SessionCookieName = "JSESSIONID";
        public const string ProjectSummaryFields = "ObjectId,Id,Name,Status,LastUpdateDate,DataDate,SummaryActivityCount,SummaryCompletedActivityCount,SummaryInProgressActivityCount,SummaryNotStartedActivityCount";
        public const string ProjectSummaryOrderBy = null;
        public const string ProjectSummaryFilter = null;
        public const int ProjectSummaryPageSize = 500;
        public const int CatalogPageSize = 500;

        [Post(LoginPath)]
        Task<HttpResponseMessage> LoginAsync(
            [Header(AuthTokenHeaderName)] string authToken,
            [AliasAs("DatabaseName")] string databaseName,
            [Body] HttpContent body,
            CancellationToken cancellationToken = default);

        [Post(LogoutPath)]
        Task<HttpResponseMessage> LogoutAsync(
            [Header(CookieHeaderName)] string cookie,
            [Body] HttpContent body,
            CancellationToken cancellationToken = default);

        [Get(ProjectPath)]
        Task<List<P6ProjectRecord>> GetProjectsAsync(
            [Header(CookieHeaderName)] string cookie,
            [AliasAs("Fields")] string fields = ProjectSummaryFields,
            [AliasAs("OrderBy")] string orderBy = ProjectSummaryOrderBy,
            [AliasAs("Filter")] string filter = ProjectSummaryFilter,
            [AliasAs("Limit")] int limit = ProjectSummaryPageSize,
            [AliasAs("Offset")] int offset = 0,
            CancellationToken cancellationToken = default);

        [Get(WbsPath)]
        Task<List<P6WbsRecord>> GetWbsAsync(
            [Header(CookieHeaderName)] string cookie,
            [AliasAs("Fields")] string fields = "ObjectId,ProjectObjectId,Name",
            [AliasAs("Filter")] string filter = null,
            [AliasAs("Limit")] int limit = CatalogPageSize,
            [AliasAs("Offset")] int offset = 0,
            CancellationToken cancellationToken = default);

        [Get(ActivityPath)]
        Task<List<P6ActivityRecord>> GetActivitiesAsync(
            [Header(CookieHeaderName)] string cookie,
            [AliasAs("Fields")] string fields = "ObjectId,Id,Name,ProjectObjectId,StartDate,FinishDate",
            [AliasAs("Filter")] string filter = null,
            [AliasAs("Limit")] int limit = CatalogPageSize,
            [AliasAs("Offset")] int offset = 0,
            CancellationToken cancellationToken = default);

        [Get(ResourcePath)]
        Task<List<P6ResourceRecord>> GetResourcesAsync(
            [Header(CookieHeaderName)] string cookie,
            [AliasAs("Fields")] string fields = "ObjectId,Id,Name",
            [AliasAs("Limit")] int limit = CatalogPageSize,
            [AliasAs("Offset")] int offset = 0,
            CancellationToken cancellationToken = default);

        [Get(ResourceAssignmentPath)]
        Task<List<P6ResourceAssignmentRecord>> GetResourceAssignmentsAsync(
            [Header(CookieHeaderName)] string cookie,
            [AliasAs("Fields")] string fields = "ObjectId,ActivityObjectId,ResourceObjectId",
            [AliasAs("Filter")] string filter = null,
            [AliasAs("Limit")] int limit = CatalogPageSize,
            [AliasAs("Offset")] int offset = 0,
            CancellationToken cancellationToken = default);

        [Get(RelationshipPath)]
        Task<List<P6RelationshipRecord>> GetRelationshipsAsync(
            [Header(CookieHeaderName)] string cookie,
            [AliasAs("Fields")] string fields = "ObjectId,PredecessorActivityObjectId,SuccessorActivityObjectId,PredecessorProjectObjectId,SuccessorProjectObjectId",
            [AliasAs("Filter")] string filter = null,
            [AliasAs("Limit")] int limit = CatalogPageSize,
            [AliasAs("Offset")] int offset = 0,
            CancellationToken cancellationToken = default);
    }
}
