using System.Collections.Generic;

namespace PunchIn.ViewModels.Converters
{
    public class ModelListToViewModelListConverter<TModel, TViewModel>
    {
        public List<TViewModel> Convert(IList<TModel> from, object state)
        {
            if (from == null) return null;
            List<TViewModel> list = new List<TViewModel>();
            foreach (var item in from)
            {
                list.Add((TViewModel)DbViewModelBase.ToViewModel<TModel, TViewModel>((TModel)item));
            }
            return list;
        }
    }

    public class ModelListToViewModelObservableElementCollectionConverter<TModel, TViewModel>
    {
        public ObservableElementCollection<TViewModel> Convert(IList<TModel> from, object state)
        {
            if (from == null) return null;
            List<TViewModel> list = new List<TViewModel>();
            foreach (var item in from)
            {
                list.Add((TViewModel)DbViewModelBase.ToViewModel<TModel, TViewModel>((TModel)item));
            }
            return new ObservableElementCollection<TViewModel>(list);
        }
    }
}
