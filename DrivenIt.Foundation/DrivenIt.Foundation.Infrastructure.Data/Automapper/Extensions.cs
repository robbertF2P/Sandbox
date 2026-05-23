using AutoMapper;
using DrivenIt.Foundation.Contracts;
using System.Collections.Generic;

namespace DrivenIt.Foundation.Infrastructure.Data.Automapper
{
    public static class Extensions
    {
       public static TDomainModel To<TDomainModel>(this IDataModel model)
            where TDomainModel : IDomainModel
        {
            if (null == model) return default(TDomainModel);

            return Mapper.Map<TDomainModel>(model);
        }

        public static IList<TDomainModel> To<TDomainModel>(this IEnumerable<IDataModel> models)
            where TDomainModel : IDomainModel
        {
            return Mapper.Map<IList<TDomainModel>>(models);
        }

        
        public static TDataModel To<TDataModel,TIn>(this TIn task, TDataModel original)
            where TDataModel : IDataModel where TIn:IDomainTask
        {
            if (null == task) return original;

            return Mapper.Map<TIn,TDataModel>(task, original);
        }

        public static TDataModel To<TDataModel>(this IDomainTask task)
            where TDataModel : IDataModel
        {
            if (null == task) return default(TDataModel);

            return Mapper.Map<TDataModel>(task);
        }
    }
}
