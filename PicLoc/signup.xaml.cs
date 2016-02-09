using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PicLoc
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class signup : Page
    {
        public signup()
        {
            this.InitializeComponent();
        }

        private void signup_button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Debug.WriteLine("HERE WHAT THE FUCK TAPPED");
        }

        private void signup_button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HERE WHAT THE FUCK CLICK");
        }
    }
}
