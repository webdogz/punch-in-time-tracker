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
            return ObjectMapperManager.DefaultInstance
                .GetMapper<TViewModel, TModel>(
                    new DefaultMapConfig().ConstructBy<TModel>(() => (TModel)Activator.CreateInstance(typeof(TModel), viewModel.Id))
                ).Map((TViewModel)viewModel);
        }

        public static TViewModel ToViewModelWithList<TModel, TViewModel, TChildModel, TChildViewModel>(TModel model)
        {
            var mapper = EmitMapper.ObjectMapperManager.DefaultInstance
                .GetMapper<TModel, TViewModel>(
                    new EmitMapper.MappingConfiguration.DefaultMapConfig().ConvertGeneric(typeof(IList<TChildModel>), typeof(IList<TChildViewModel>),
                        new EmitMapper.MappingConfiguration.DefaultCustomConverterProvider(typeof(Converters.ModelListToViewModelListConverter<TChildModel, TChildViewModel>))
                    )
                );
            return mapper.Map(model);
        }
        #endregion
    }
}
