using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionBlendEffect.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CompositionBlendEffect
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BlendEffect _blendEffect;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _blendEffect = new BlendEffect(BackgroundContainer, "ms-appx:///Assets/Blend1.jpg");
            await _blendEffect.Play();

            bool alternate = false;

            while (true)
            {
                await Task.Delay(2000);

                alternate = !alternate;
                await _blendEffect.Play(alternate ? "ms-appx:///Assets/Blend2.jpg" : "ms-appx:///Assets/Blend1.jpg");
            }
        }
    }
}
