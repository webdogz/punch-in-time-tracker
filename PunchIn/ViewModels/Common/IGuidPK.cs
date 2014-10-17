using System;

namespace PunchIn.ViewModels
{
    public interface IGuidPK
    {
        Guid Id { get; set; }
    }
    /// <summary>
    /// Simple marker interface indicating ViewModel has IList(Model) that can be converted to IList(ViewModel)
    /// </summary>
    public interface IDbPunchHasConvertableList { }
}
