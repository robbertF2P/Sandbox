using System.Collections.Generic;
using AutoMapper;
using DrivenIt.Foundation.Contracts;

namespace DrivenIt.Foundation.Domain.Tools.Automapper
{
    public static class Extensions
    {
        public static TDomainModel To<TDomainModel>(this IViewModel model)
             where TDomainModel : IDomainModel
        {
            if (null == model) return default(TDomainModel);

            return Mapper.Map<TDomainModel>(model);
        }

        public static IList<TDomainModel> To<TDomainModel>(this IEnumerable<IViewModel> models)
            where TDomainModel : IDomainModel
        {
            return Mapper.Map<IList<TDomainModel>>(models);
        }

        public static TDomainTask ToTask<TDomainTask>(this IViewModel viewModel)
            where TDomainTask : IDomainTask
        {
            if (null == viewModel) return default(TDomainTask);

            return Mapper.Map<TDomainTask>(viewModel);
        }
    }
}
