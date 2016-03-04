using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Media3D;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition.Toolkit;

namespace CompositionBlendEffect.Helpers
{
    
    public class BlendEffect
    {
        private ArithmeticCompositeEffect _compositeEffect;
        private CompositionEffectBrush _effectBrush;

        private ContainerVisual _rootVisual;
        private SpriteVisual _spriteVisual;
        private FrameworkElement _element;
        private Size _imageSize;
        private double _imageAspectRatio;

        private ScalarKeyFrameAnimation _fadeInAnimation;
        private ScalarKeyFrameAnimation _fadeOutAnimation;

        public CompositionSurfaceBrush _lastImageBrush;

        public Compositor Compositor { get; set; }

        public BlendEffect(FrameworkElement element, string uri)
        {
            Compositor = element.GetContainerVisual().Compositor;

            Initialize(element);
            InitializeAnimations();

            SetParameters(uri);
        }

        private void Initialize(FrameworkElement element)
        {
            _element = element;
            _element.SizeChanged += ArithmeticEffectComposition_SizeChanged;
            _imageSize = new Size(element.ActualWidth, element.ActualHeight);

            _compositeEffect = new ArithmeticCompositeEffect
            {
                Name = "effect",
                ClampOutput = false,
                Source1 = new CompositionEffectSourceParameter("Source1"),
                Source2 = new CompositionEffectSourceParameter("Source2"),
                Source1Amount = 0.0f,
                Source2Amount = 0.0f,
                MultiplyAmount = 0.0f
            };

            _effectBrush = Compositor.CreateEffectFactory(_compositeEffect,
               new[]
               {
                    "effect.Source1Amount",
                    "effect.Source2Amount",
               }
             ).CreateBrush();

            _rootVisual = Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(_element, _rootVisual);
            _spriteVisual = Compositor.CreateSpriteVisual();
            _spriteVisual.Brush = _effectBrush;
            _rootVisual.Children.InsertAtBottom(_spriteVisual);
        }

        private void ArithmeticEffectComposition_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _imageSize = e.NewSize;
            _spriteVisual.ResizeSprite(_imageSize);
        }

        public void Clean()
        {
            _element.SizeChanged -= ArithmeticEffectComposition_SizeChanged;
            _element = null;
            _compositeEffect.Dispose();
            _effectBrush.Dispose();
            _lastImageBrush.Dispose();
        }

        private void InitializeAnimations()
        {
            _fadeInAnimation = Compositor.CreateScalarKeyFrameAnimation();
            _fadeInAnimation.InsertExpressionKeyFrame(0.0f, "0.0f");
            _fadeInAnimation.InsertExpressionKeyFrame(1.0f, "1.0f");

            _fadeOutAnimation = Compositor.CreateScalarKeyFrameAnimation();
            _fadeOutAnimation.InsertExpressionKeyFrame(0.0f, "1.0f");
            _fadeOutAnimation.InsertExpressionKeyFrame(1.0f, "0.0f");
        }

        public Task Play(float duration = 1500)
        {
            _fadeInAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            _fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(duration);

            _effectBrush.StopAnimation("effect.Source1Amount");
            _effectBrush.StopAnimation("effect.Source2Amount");

            return Task.WhenAll(
                _effectBrush.StartAsync("effect.Source1Amount", _fadeOutAnimation),
                _effectBrush.StartAsync("effect.Source2Amount", _fadeInAnimation));
        }

        public async Task Play(string uri, float duration = 1500)
        {
            await SetParameters(uri);
            await Play(duration);
        }

        private async Task SetParameters(string newUri)
        {
            var newImageBrush = await CreateImageBrush(newUri);

            _effectBrush.SetSourceParameter("Source2", newImageBrush);
            _effectBrush.SetSourceParameter("Source1", _lastImageBrush ?? newImageBrush);

            _lastImageBrush?.Dispose();
            _lastImageBrush = newImageBrush;
        }

        private Uri CreateUri(string input)
        {
            if (input.Contains("ms-appx"))
            {
                return new Uri(input);
            }
            if (input.Contains("http"))
            {
                return new Uri(input);
            }
            return new Uri($"ms-appx:///{input.TrimStart('/')}");
        }

        private async Task<CompositionSurfaceBrush> CreateImageBrush(string newUri)
        {
            var source = CreateUri(newUri);
            var imageBrushWithSize = await Compositor.CreateBrushFromAssetWithSize(source);
            var imageBrush = imageBrushWithSize.Brush;

            _spriteVisual.ResizeSprite(_imageSize);
            imageBrush.Stretch = CompositionStretch.UniformToFill;
            _imageAspectRatio = _imageSize.Width / _imageSize.Height;

            return imageBrush;
        }
    }
}
