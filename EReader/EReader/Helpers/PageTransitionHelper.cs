using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace EReader.Helpers
{
    public static class PageTransitionHelper
    {
        public static void SetupTransition(this Page page, NavigationTransitionInfo transitionInfo)
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();
            theme.DefaultNavigationTransitionInfo = transitionInfo;
            collection.Add(theme);
            page.Transitions = collection;
        }
    }
}
