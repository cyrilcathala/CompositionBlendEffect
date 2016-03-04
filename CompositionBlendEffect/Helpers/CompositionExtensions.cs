using Microsoft.UI.Composition.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionBlendEffect.Helpers
{
    public static class CompositionExtensions
    {
        public static ContainerVisual GetContainerVisual(this UIElement element)
        {
            return ElementCompositionPreview.GetElementVisual(element) as ContainerVisual;
        }

        public static Task<CompositionSurfaceBrushWithSize> CreateBrushFromAssetWithSize(this Compositor compositor, Uri uri)
        {
            var surfaceFactory = CompositionImageFactory.CreateCompositionImageFactory(compositor);
            var image = surfaceFactory.CreateImageFromUri(uri);

            var tcs = new TaskCompletionSource<CompositionSurfaceBrushWithSize>();
            CompositionImageLoadedEventHandler handler = null;
            handler = (sender, status) =>
            {
                image.ImageLoaded -= handler;
                if (status == CompositionImageLoadStatus.Success)
                {
                    var brush = compositor.CreateSurfaceBrush(image.Surface);
                    tcs.TrySetResult(new CompositionSurfaceBrushWithSize(brush, image.Size));
                }
                else
                    tcs.TrySetException(new Exception(status.ToString()));
            };
            image.ImageLoaded += handler;

            return tcs.Task;
        }
        

        public static void ResizeSprite(this SpriteVisual sprite, Size size)
        {
            sprite.Size = new Vector2((float)size.Width, (float)size.Height);
        }
        
        public static Task StartAsync(this CompositionObject visual, string property, CompositionAnimation animation)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (animation == null)
                tcs.SetException(new ArgumentNullException(nameof(animation)));
            else if (property == null)
                tcs.SetException(new ArgumentNullException(nameof(property)));
            else
            {
                var batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                TypedEventHandler<object, CompositionBatchCompletedEventArgs> onComplete = null;
                onComplete = (s, e) =>
                {
                    batch.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                batch.Completed += onComplete;

                visual.StartAnimation(property, animation);

                batch.End();
            }
            return tcs.Task;
        }
    }

    public class CompositionSurfaceBrushWithSize
    {
        public CompositionSurfaceBrush Brush { get; set; }
        public Size Size { get; set; }

        public CompositionSurfaceBrushWithSize(CompositionSurfaceBrush brush, Size size)
        {
            Brush = brush;
            Size = size;
        }
    }
}
