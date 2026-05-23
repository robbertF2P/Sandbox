using System.Web;
using System.Web.Mvc;

namespace DrivenIt.Foundation.Sample.WebSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
