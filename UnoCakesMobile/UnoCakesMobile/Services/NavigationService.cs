using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoCakesMobile.Services
{
    public sealed class NavigationService : INavigationService
    {
        private Frame _frame;
        internal NavigationService(Frame frame)
        {
            _frame = frame;
        }

        public bool CanGoBack => _frame.CanGoBack;

        public void GoBack()
        {
            _frame.GoBack();
        }

        public bool Navigate(Type sourcePageType, object parameter = null)
        {
            return _frame.Navigate(sourcePageType, parameter);
        }
    }
}
