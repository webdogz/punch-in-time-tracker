
namespace PunchIn.ViewModels
{
    public interface IDbModelViewModel
    {
        TModel ToModel<TViewModel, TModel>(IGuidPK viewModel);
    }
}
