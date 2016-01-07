using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestCompositor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        //private ContainerVisual _root;
        private Visual _square;
        private ExpressionAnimation _expression;
        private ScalarKeyFrameAnimation _resetAnimation, _loadingAnimation;
        public MainPage()
        {
            this.InitializeComponent();
            _square = ElementCompositionPreview.GetElementVisual(MySquare);
            
            _compositor = _square.Compositor;

            _expression = _compositor.CreateExpressionAnimation("objectOffset.X + manipOffset.X");
            _expression.SetVector3Parameter("objectOffset", new Vector3(0, 0, 0));
            _expression.SetVector2Parameter("manipOffset", new Vector2(0, 0));
            

            _resetAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _resetAnimation.InsertKeyFrame(1.0f, 0.0f);

            MySquare.ManipulationDelta += MySquare_ManipulationDelta;
            MySquare.ManipulationStarted += MySquare_ManipulationStarted;
            MySquare.ManipulationCompleted += MySquare_ManipulationCompleted;

            PrepareExpressionAnimations();
        }

        private void MySquare_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;

            StartResetAnimations();
        }

        private void MySquare_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
        }

        private void OnCompositionTargetRendering(object sender, object e)
        {
            _square.StopAnimation("Offset.X");
            
            _square.StartAnimation("Offset.X", _expression);
        }

        private void MySquare_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _expression.SetVector2Parameter("manipOffset", new Vector2((float)e.Cumulative.Translation.X, (float)e.Cumulative.Translation.Y));
        }

        private void PrepareExpressionAnimations()
        {
            _square.StartAnimation("Offset.X", _expression);
        }

        private void StartResetAnimations()
        {
            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) => _expression.SetVector2Parameter("manipOffset", new Vector2(0,0));
            _square.StartAnimation("Offset.X", _resetAnimation);
            batch.End();
        }
}
}
