using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Core
{
    using AppFunc = Func<IDictionary<string, object>,Task>;
    public class LoggerModule
    {
        private readonly AppFunc _next;

        public LoggerModule(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Console.WriteLine("{0} {1}", environment["owin.RequestMethod"], environment["owin.RequestPath"]);
            Console.WriteLine("queryString: {0}", environment["owin.RequestQueryString"]);
            
            await _next.Invoke(environment);
            
            Console.WriteLine("StatusCode: {0}",environment["owin.ResponseStatusCode"]);
            //foreach (var o in environment)
            //{
            //    Console.WriteLine(string.Format("{0} {1}", o.Key, o.Value));
            //}
        }
    }
}
