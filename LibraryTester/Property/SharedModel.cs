using System;
using Lincsolution.UI.Base.Shared;

namespace LibraryTester.Property
{
    public interface ISharedModel : IBaseSharedModel<Alarm>
    {
        
    }
    public partial class SharedModel : BaseSharedModel<Alarm>, ISharedModel
    {
        
    }
}
