using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace FedoraControls.Controls
{
    [TemplatePart(Name = _ContentContainerName, Type = typeof(Border))]
    public sealed class PopInControl : ContentControl
    {
        private const string _ContentContainerName = "ContentContainer";

        private Border ContentContainer;
        private Visual ContentVisual;
        private ScalarKeyFrameAnimation OpenAnimation, CloseAnimation;

        private Compositor _Compositor;

        public PopInControl()
        {
            this.DefaultStyleKey = typeof(PopInControl);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ContentContainer = GetTemplateChild(_ContentContainerName) as Border;
            ContentVisual = ElementCompositionPreview.GetElementVisual(ContentContainer);
            ContentVisual.CenterPoint = new System.Numerics.Vector3(0.5f, 0.9f, 0);
            ContentVisual.RotationAxis = new System.Numerics.Vector3(1, 0, 0);
            ContentVisual.RotationAngle = 20f;
        }

        private void RunOpenAnimation()
        {
            if (ContentVisual == null)
            {
                if (ContentContainer == null)
                {
                    return;
                }
                ContentVisual = ElementCompositionPreview.GetElementVisual(ContentContainer);
            }
            if (_Compositor == null)
            {
                _Compositor = ContentVisual.Compositor;
            }
            OpenAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            OpenAnimation.Duration = TimeSpan.FromMilliseconds(500);
            OpenAnimation.InsertKeyFrame(1.0f, 0);
            ContentVisual.StartAnimation("RotationAngle", OpenAnimation);
        }

        private void RunCloseAnimation()
        {
            if (ContentVisual == null)
            {
                if (ContentContainer == null)
                {
                    return;
                }
                ContentVisual = ElementCompositionPreview.GetElementVisual(ContentContainer);
            }
        }


        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(PopInControl), new PropertyMetadata(null, IsOpen_Changed));

        private static void IsOpen_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopInControl pic = d as PopInControl;
            if (pic.IsOpen)
            {
                // Run open animation
                pic.RunOpenAnimation();
            }
            else
            {
                // Run close animation
                pic.RunCloseAnimation();
            }
        }
    }
}
