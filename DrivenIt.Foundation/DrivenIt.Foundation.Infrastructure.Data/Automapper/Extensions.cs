using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DrivenIt.Foundation.Contracts;

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

        //viewmodel => domain task
        internal static TDomainTask ToTask<TDomainTask>(this IViewModel viewModel)
            where TDomainTask : IDomainTask
        {
            if (null == viewModel) return default(TDomainTask);

            return Mapper.Map<TDomainTask>(viewModel);
        }
    }
}
