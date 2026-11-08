using Lincsolution.UI.Base.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTester
{
    public sealed class DesignDashboardViewModel : DashboardViewModel
    {
        public DesignDashboardViewModel() 
            : base(new DashboardModel(), new DesignSharedModel(), new DummyNavigationService())
        {
            
        }
    }
}
