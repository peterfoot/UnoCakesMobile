using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoCakesMobile.Services
{
    public interface INavigationService
    {
        bool CanGoBack { get; }

        void GoBack();

        bool Navigate(Type sourcePageType, object parameter = null);
    }
}
