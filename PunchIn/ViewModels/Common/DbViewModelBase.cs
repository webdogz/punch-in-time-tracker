using EmitMapper;
using EmitMapper.MappingConfiguration;
using System;
using System.Collections.Generic;

namespace PunchIn.ViewModels
{
    public abstract class DbViewModelBase : ViewModelBase
    {
        #region Methods
        public static TViewModel ToViewModel<TModel, TViewModel>(TModel model)
        {
            return ObjectMapperManager.DefaultInstance.GetMapper<TModel, TViewModel>().Map(model);
        }
        public static TViewModel ToViewModel<TModel, TViewModel>(TModel model, ObjectsMapper<TModel, TViewModel> mapper)
        {
            return mapper.Map(model);
        }
        public TModel ToModel<TViewModel, TModel>(IGuidPK viewModel)
        {
            var vm = (TViewModel)viewModel;
            var m = (TModel)Activator.CreateInstance(typeof(TModel), viewModel.Id);
            return ObjectMapperManager.DefaultInstance.GetMapper<TViewModel, TModel>().Map(vm, m);
        }
        #endregion
    }
}
