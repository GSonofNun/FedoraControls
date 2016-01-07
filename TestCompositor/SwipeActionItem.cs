using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace TestCompositor
{
    public sealed class SwipeActionItem : ContentControl
    {
        private TextBlock _LeftTextBlock;
        private Grid _LeftContentContainer;
        private Grid _LeftContainer;
        private TextBlock _RightTextBlock;
        private Grid _RightContentContainer;
        private Grid _RightContainer;
        private Grid _ContentContainer;

        private Compositor _Compositor;
        private Visual _ContentVisual, _LeftContentVisual, _RightContentVisual;
        private ExpressionAnimation _ContentAnimation, _LeftContentAnimation, _RightContentAnimation;
        private ScalarKeyFrameAnimation _ResetAnimation;

        private double _MaxOffsetX;
        private double _ContentWidth;
        private double _StretchFactor = 1;
        private double _OffsetX = 0;
        private void AddToOffsetX(double nOffset)
        {
            _OffsetX += nOffset;
            if (_OffsetX > _ContentContainer.ActualWidth / 4)
            {
                _OffsetX = _ContentContainer.ActualWidth / 4;
            }
        }

        public SwipeActionItem()
        {
            this.DefaultStyleKey = typeof(SwipeActionItem);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _LeftContainer = GetTemplateChild("LeftContainer") as Grid;
            _LeftContentContainer = GetTemplateChild("LeftContentContainer") as Grid;
            _LeftTextBlock = GetTemplateChild("LeftDescription") as TextBlock;
            _RightContainer = GetTemplateChild("RightContainer") as Grid;
            _RightContentContainer = GetTemplateChild("RightContentContainer") as Grid;
            _RightTextBlock = GetTemplateChild("RightDescription") as TextBlock;
            _ContentContainer = GetTemplateChild("ContentContainer") as Grid;

            

            WireAnimations();
            WireManipulation();
        }

        private void WireAnimations()
        {
            _ContentVisual = ElementCompositionPreview.GetElementVisual(_ContentContainer);
            _LeftContentVisual = ElementCompositionPreview.GetElementVisual(_LeftContentContainer);
            _RightContentVisual = ElementCompositionPreview.GetElementVisual(_RightContentContainer);

            _Compositor = _ContentVisual.Compositor;

            // ContentVisual Expression Animation
            //_ContentAnimation = _Compositor.CreateExpressionAnimation("objectOffset.X + (manipOffset.X * Min(stretchFactor, max)");
            //_ContentAnimation = _Compositor.CreateExpressionAnimation("Max(Min(((c.X * -1) * t.X * (t.X - 2) + 0) * objectWidth, objectWidth), objectWidth * -1)");
            //_ContentAnimation = _Compositor.CreateExpressionAnimation("((c.X * -1) * t.X * (t.X - 2) + 0) * objectWidth");
            _ContentAnimation = _Compositor.CreateExpressionAnimation("offset.X * 1");
            _ContentAnimation.SetVector3Parameter("t", new Vector3(0, 0, 0));
            _ContentAnimation.SetVector2Parameter("c", new Vector2(0, 0));
            _ContentAnimation.SetVector3Parameter("offset", new Vector3(0, 0, 0));
            // RightContentVisual Expression Animation

            // LeftContentVisual Expression Animation

            // Reset Animation
            _ResetAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            _ResetAnimation.InsertKeyFrame(1.0f, 0.0f);
        }

        private void PrepareExpressionAnimations()
        {
            _ContentVisual.StartAnimation("Offset.X", _ContentAnimation);
        }

        private void StartResetAnimations()
        {
            var batch = _Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) => _ContentAnimation.SetVector2Parameter("manipOffset", new Vector2(0, 0));
            _ContentVisual.StartAnimation("Offset.X", _ResetAnimation);
            batch.End();
        }

        private void WireManipulation()
        {
            _ContentContainer.ManipulationMode = ManipulationModes.TranslateX;
            _ContentContainer.ManipulationStarted += _ContentContainer_ManipulationStarted;
            _ContentContainer.ManipulationDelta += _ContentContainer_ManipulationDelta;
            _ContentContainer.ManipulationCompleted += _ContentContainer_ManipulationCompleted;

        }

        private void _ContentContainer_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _ContentWidth = _ContentContainer.ActualWidth;
            _MaxOffsetX = (_ContentWidth / 4) * 3;
            _StretchFactor = 1;
            _ContentAnimation.SetScalarParameter("objectWidth", (float)_ContentWidth);
            Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
        }

        private void _ContentContainer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double cOffsetX = e.Cumulative.Translation.X - _ContentVisual.Offset.X;
            System.Diagnostics.Debug.WriteLine(e.Cumulative.Translation.X + " - " + _ContentVisual.Offset.X);
            //double dOffsetX = e.Delta.Translation.X;

            Vector3 t = new Vector3((float)Math.Min((e.Cumulative.Translation.X / _ContentWidth), 1), 0, 0);
            //System.Diagnostics.Debug.WriteLine(t.X);
            double offset = Math.Min(((cOffsetX * -1) * t.X * (t.X - 2) + 0), 1) * _ContentWidth;
            //System.Diagnostics.Debug.WriteLine(offset);
            //LogService.WriteLine(t.X.ToString(), "0", cOffsetX.ToString(), _ContentWidth.ToString());
            //System.Diagnostics.Debug.WriteLine(Math.Max(Math.Min((((cOffsetX * -1) * t.X * (t.X - 2) + 0) * _ContentWidth), _ContentWidth), _ContentWidth * -1));
            _ContentAnimation.SetVector3Parameter("t", t);
            _ContentAnimation.SetVector2Parameter("c", new Vector2((float)cOffsetX, (float)e.Cumulative.Translation.Y));
            _ContentAnimation.SetVector3Parameter("offset", new Vector3((float)offset, 0, 0));
        }

        private void _ContentContainer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;

            StartResetAnimations();
        }

        private void OnCompositionTargetRendering(object sender, object e)
        {
            _ContentVisual.StopAnimation("Offset.X");
            
            _ContentVisual.StartAnimation("Offset.X", _ContentAnimation);
            
        }


        #region PropDPs

        public object LeftContent
        {
            get { return (object)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register("LeftContent", typeof(object), typeof(SwipeActionItem), new PropertyMetadata(0));



        public object RightContent
        {
            get { return (object)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register("RightContent", typeof(object), typeof(SwipeActionItem), new PropertyMetadata(0));



        public string LeftDescription
        {
            get { return (string)GetValue(LeftDescriptionProperty); }
            set { SetValue(LeftDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftDescription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftDescriptionProperty =
            DependencyProperty.Register("LeftDescription", typeof(string), typeof(SwipeActionItem), new PropertyMetadata(""));



        public string RightDescription
        {
            get { return (string)GetValue(RightDescriptionProperty); }
            set { SetValue(RightDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightDescription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightDescriptionProperty =
            DependencyProperty.Register("RightDescription", typeof(string), typeof(SwipeActionItem), new PropertyMetadata(""));



        public Brush LeftBackground
        {
            get { return (Brush)GetValue(LeftBackgroundProperty); }
            set { SetValue(LeftBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftBackgroundProperty =
            DependencyProperty.Register("LeftBackground", typeof(Brush), typeof(SwipeActionItem), new PropertyMetadata(new SolidColorBrush(Colors.Red)));



        public Brush RightBackground
        {
            get { return (Brush)GetValue(RightBackgroundProperty); }
            set { SetValue(RightBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightBackgroundProperty =
            DependencyProperty.Register("RightBackground", typeof(Brush), typeof(SwipeActionItem), new PropertyMetadata(new SolidColorBrush(Colors.Yellow)));



        public Brush LeftContentForeground
        {
            get { return (Brush)GetValue(LeftContentForegroundProperty); }
            set { SetValue(LeftContentForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftContentForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftContentForegroundProperty =
            DependencyProperty.Register("LeftContentForeground", typeof(Brush), typeof(SwipeActionItem), new PropertyMetadata(new SolidColorBrush(Colors.White)));



        public Brush RightContentForeground
        {
            get { return (Brush)GetValue(RightContentForegroundProperty); }
            set { SetValue(RightContentForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightContentForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightContentForegroundProperty =
            DependencyProperty.Register("RightContentForeground", typeof(Brush), typeof(SwipeActionItem), new PropertyMetadata(new SolidColorBrush(Colors.White)));



        public Brush LeftDescriptionForeground
        {
            get { return (Brush)GetValue(LeftDescriptionForegroundProperty); }
            set { SetValue(LeftDescriptionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftDescriptionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftDescriptionForegroundProperty =
            DependencyProperty.Register("LeftDescriptionForeground", typeof(Brush), typeof(SwipeActionItem), new PropertyMetadata(new SolidColorBrush(Colors.White)));



        public Brush RightDescriptionForeground
        {
            get { return (Brush)GetValue(RightDescriptionForegroundProperty); }
            set { SetValue(RightDescriptionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightDescriptionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightDescriptionForegroundProperty =
            DependencyProperty.Register("RightDescriptionForeground", typeof(Brush), typeof(SwipeActionItem), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        
        #endregion

    }
}
