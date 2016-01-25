using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
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

namespace FedoraControls.Controls
{
    [TemplatePart(Name = _rightContent, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = _leftContent, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = _leftTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = _rightTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = _leftContentContainer, Type = typeof(Grid))]
    [TemplatePart(Name = _rightContentContainer, Type = typeof(Grid))]
    [TemplatePart(Name = _leftContainer, Type = typeof(Grid))]
    [TemplatePart(Name = _rightContainer, Type = typeof(Grid))]
    [TemplatePart(Name = _contentContainer, Type = typeof(Grid))]
    public sealed class SwipeActionItem : ContentControl
    {
        private const string _leftContent = "LeftContent";
        private const string _leftTextBlock = "LeftText";
        private const string _leftContentContainer = "LeftContentContainer";
        private const string _leftContainer = "LeftContainer";
        private const string _rightContent = "RightContent";
        private const string _rightTextBlock = "RightText";
        private const string _rightContentContainer = "RightContentContainer";
        private const string _rightContainer = "RightContainer";
        private const string _contentContainer = "ContentContainer";

        private TextBlock _LeftTextBlock;
        private Grid _LeftContentContainer;
        private Grid _LeftContainer;
        private TextBlock _RightTextBlock;
        private Grid _RightContentContainer;
        private Grid _RightContainer;
        private Grid _ContentContainer;
        private ContentPresenter _RightContent;
        private ContentPresenter _LeftContent;

        private Compositor _Compositor;
        private Visual _ContentVisual, _LeftContentVisual, _LeftTextVisual, _RightTextVisual, _RightContentVisual;
        private ExpressionAnimation _ContentAnimation, _LeftTextAnimation, _LeftContentAnimation, _RightTextAnimation, _RightContentAnimation;
        private ScalarKeyFrameAnimation _ResetContentAnimation, _ResetLeftContentAnimation, _ResetLeftTextAnimation, _ResetRightContentAnimation, _ResetRightTextAnimation;

        private double _MaxOffsetX;
        private double _ContentWidth;
        private double _OffsetX = 0;

        private double _BeginRotationAngle = -1;
        private double _BeginRotationAngle2 = 1;

        private double _RightTriggerDistance = 0;
        private double _LeftTriggerDistance = 0;

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

        protected override Size ArrangeOverride(Size finalSize)
        {
            var r = new RectangleGeometry();
            r.Rect = new Rect(new Point(0, 0), finalSize);
            Clip = r;
            return base.ArrangeOverride(finalSize);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _LeftContainer = GetTemplateChild(_leftContainer) as Grid;
            _LeftContentContainer = GetTemplateChild(_leftContentContainer) as Grid;
            _LeftTextBlock = GetTemplateChild(_leftTextBlock) as TextBlock;
            _RightContainer = GetTemplateChild(_rightContainer) as Grid;
            _RightContentContainer = GetTemplateChild(_rightContentContainer) as Grid;
            _RightTextBlock = GetTemplateChild(_rightTextBlock) as TextBlock;
            _ContentContainer = GetTemplateChild(_contentContainer) as Grid;
            _RightContent = GetTemplateChild(_rightContent) as ContentPresenter;
            _LeftContent = GetTemplateChild(_leftContent) as ContentPresenter;

            WireAnimations();
            WireManipulation();
        }

        private void WireAnimations()
        {
            _ContentVisual = ElementCompositionPreview.GetElementVisual(_ContentContainer);
            _LeftTextVisual = ElementCompositionPreview.GetElementVisual(_LeftTextBlock);
            _LeftContentVisual = ElementCompositionPreview.GetElementVisual(_LeftContent);
            _RightTextVisual = ElementCompositionPreview.GetElementVisual(_RightTextBlock);
            _RightContentVisual = ElementCompositionPreview.GetElementVisual(_RightContent);

            _Compositor = _ContentVisual.Compositor;

           

            _ContentAnimation = _Compositor.CreateExpressionAnimation("offset");

            _ContentAnimation.SetScalarParameter("offset", (float)_OffsetX);
            //_ContentAnimation.SetScalarParameter("maxOffset", (float)_MaxOffsetX);

            

            // Reset Animations
            

            
        }

        private void PrepareExpressionAnimations()
        {
            _ContentVisual.StartAnimation("Offset.X", _ContentAnimation);
            //_LeftContentVisual.StartAnimation("RotationAngle", _LeftTextAnimation);
            StartRightAnimations();
            StartLeftAnimations();
        }

        private void PrepareRightAnimation()
        {
            switch (this.RightAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _RightTextVisual.Offset = new Vector3(-100, 0, 0);
                    _RightTextAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * -100");
                    _RightTextAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _RightTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);

                    _RightContentVisual.Offset = new Vector3(-75, 0, 0);
                    _RightContentAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * -75");
                    _RightContentAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _RightContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    // RightContentVisual Expression Animation
                    _RightTextVisual.CenterPoint = new Vector3(-150, 0, 0);
                    _RightTextVisual.RotationAxis = new Vector3(0, 1, 0);
                    _RightTextVisual.RotationAngle = (float)_BeginRotationAngle;
                    _RightTextAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * y");
                    _RightTextAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _RightTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    _RightTextAnimation.SetScalarParameter("y", (float)_BeginRotationAngle);

                    _RightContentVisual.CenterPoint = new Vector3(-100, 0, 0);
                    _RightContentVisual.RotationAxis = new Vector3(0, 1, 0);
                    _RightContentVisual.RotationAngle = (float)_BeginRotationAngle;
                    _RightContentAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * y");
                    _RightContentAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _RightContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    _RightContentAnimation.SetScalarParameter("y", (float)_BeginRotationAngle);
                    break;
                default:
                    break;
            }
        }

        private void StopRightAnimations()
        {
            switch (this.RightAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _RightContentVisual.StopAnimation("Offset.X");
                    _RightTextVisual.StopAnimation("Offset.X");
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _RightTextVisual.StopAnimation("RotationAngle");
                    _RightContentVisual.StopAnimation("RotationAngle");
                    break;
                default:
                    break;
            }
        }

        private void StartRightAnimations()
        {
            switch (this.RightAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _RightTextVisual.StartAnimation("Offset.X", _RightTextAnimation);
                    _RightContentVisual.StartAnimation("Offset.X", _RightContentAnimation);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _RightTextVisual.StartAnimation("RotationAngle", _RightTextAnimation);
                    _RightContentVisual.StartAnimation("RotationAngle", _RightContentAnimation);
                    break;
                default:
                    break;
            }
        }

        private void PrepareLeftAnimation()
        {
            switch (this.LeftAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _LeftTextVisual.Offset = new Vector3(100, 0, 0);
                    _LeftTextAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * 100");
                    _LeftTextAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _LeftTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    _LeftContentVisual.Offset = new Vector3(75, 0, 0);
                    _LeftContentAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * 75");
                    _LeftContentAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _LeftContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _LeftTextVisual.CenterPoint = new Vector3(150, 0, 0);
                    _LeftTextVisual.RotationAxis = new Vector3(0, 1, 0);
                    _LeftTextVisual.RotationAngle = (float)_BeginRotationAngle;
                    _LeftTextAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * y");
                    _LeftTextAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _LeftTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    _LeftTextAnimation.SetScalarParameter("y", (float)_BeginRotationAngle);
                    
                    _LeftContentVisual.CenterPoint = new Vector3(100, (float)_LeftContent.ActualHeight / 2, 0);
                    _LeftContentVisual.RotationAxis = new Vector3(0, 1, 0);
                    //_LeftContentVisual.CenterPoint = new Vector3(0f, 1f, 0.5f);
                    //_LeftContentVisual.AnchorPoint = new Vector2(0f, 1f);
                    _LeftContentVisual.RotationAngle = (float)_BeginRotationAngle;
                    _LeftContentAnimation = _Compositor.CreateExpressionAnimation("(1 - (o / x)) * y");
                    _LeftContentAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _LeftContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    _LeftContentAnimation.SetScalarParameter("y", (float)_BeginRotationAngle);
                    break;
                default:
                    break;
            }
        }

        private void StopLeftAnimations()
        {
            switch (this.LeftAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _LeftContentVisual.StopAnimation("Offset.X");
                    _LeftTextVisual.StopAnimation("Offset.X");
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _LeftTextVisual.StopAnimation("RotationAngle");
                    _LeftContentVisual.StopAnimation("RotationAngle");
                    break;
                default:
                    break;
            }
        }

        private void StartLeftAnimations()
        {
            switch (this.LeftAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _LeftTextVisual.StartAnimation("Offset.X", _LeftTextAnimation);
                    _LeftContentVisual.StartAnimation("Offset.X", _LeftContentAnimation);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _LeftTextVisual.StartAnimation("RotationAngle", _LeftTextAnimation);
                    _LeftContentVisual.StartAnimation("RotationAngle", _LeftContentAnimation);
                    break;
                default:
                    break;
            }
        }

        private void SetAnimationParameters()
        {
            switch (this.LeftAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _LeftTextAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _LeftTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    _LeftContentAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _LeftContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _LeftTextAnimation.SetScalarParameter("o", (float)_OffsetX);
                    _LeftTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);

                    _LeftContentAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _LeftContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    break;
                default:
                    break;
            }
            switch (this.RightAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _RightTextAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _RightTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);

                    _RightContentAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _RightContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _RightTextAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _RightTextAnimation.SetScalarParameter("x", (float)_MaxOffsetX);

                    _RightContentAnimation.SetScalarParameter("o", (float)Math.Abs(_OffsetX));
                    _RightContentAnimation.SetScalarParameter("x", (float)_MaxOffsetX);
                    break;
                default:
                    break;
            }
            _ContentAnimation.SetScalarParameter("offset", (float)_OffsetX);
            _ContentAnimation.SetScalarParameter("maxOffset", (float)_MaxOffsetX);
            
        }

        private void StartResetAnimations()
        {
            var batch = _Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) =>
            {
                _ContentAnimation.SetScalarParameter("offset", 0);
                _LeftContentAnimation.SetScalarParameter("o", 0);
                _RightContentAnimation.SetScalarParameter("o", 0);
                _OffsetX = 0;
            };
            _ResetContentAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            _ResetContentAnimation.Duration = TimeSpan.FromSeconds(0.25);
            _ResetContentAnimation.InsertKeyFrame(1.0f, 0.0f);
            _ContentVisual.StartAnimation("Offset.X", _ResetContentAnimation);
            switch (this.RightAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _ResetRightContentAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetRightContentAnimation.InsertKeyFrame(1.0f, -75f);
                    _ResetRightTextAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetRightTextAnimation.InsertKeyFrame(1.0f, -100f);
                    _RightTextVisual.StartAnimation("Offset.X", _ResetRightTextAnimation);
                    _RightContentVisual.StartAnimation("Offset.X", _ResetRightContentAnimation);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _ResetRightContentAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetRightContentAnimation.InsertKeyFrame(1.0f, (float)_BeginRotationAngle2);
                    _ResetRightTextAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetRightTextAnimation.InsertKeyFrame(1.0f, (float)_BeginRotationAngle);
                    _RightTextVisual.StartAnimation("RotationAngle", _ResetRightTextAnimation);
                    _RightContentVisual.StartAnimation("RotationAngle", _ResetRightContentAnimation);
                    break;
                default:
                    break;
            }
            switch (this.LeftAnimationType)
            {
                case SwipeAnimationType.SlideFromInside:
                    _ResetLeftContentAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetLeftContentAnimation.InsertKeyFrame(1.0f, 75f);
                    _ResetLeftTextAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetLeftTextAnimation.InsertKeyFrame(1.0f, 100f);
                    _LeftTextVisual.StartAnimation("Offset.X", _ResetLeftTextAnimation);
                    _LeftContentVisual.StartAnimation("Offset.X", _ResetLeftContentAnimation);
                    break;
                case SwipeAnimationType.RotateFromInside:
                    _ResetLeftContentAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetLeftContentAnimation.InsertKeyFrame(1.0f, (float)_BeginRotationAngle);
                    _ResetLeftTextAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                    _ResetLeftTextAnimation.InsertKeyFrame(1.0f, (float)_BeginRotationAngle);
                    _LeftTextVisual.StartAnimation("RotationAngle", _ResetLeftTextAnimation);
                    _LeftContentVisual.StartAnimation("RotationAngle", _ResetLeftContentAnimation);
                    break;
                default:
                    break;
            }
            
            
            batch.End();
        }

        private void WireManipulation()
        {
            //_ContentContainer.ManipulationMode = ManipulationModes.TranslateX;
            _ContentContainer.ManipulationStarted += _ContentContainer_ManipulationStarted;
            _ContentContainer.ManipulationDelta += _ContentContainer_ManipulationDelta;
            _ContentContainer.ManipulationCompleted += _ContentContainer_ManipulationCompleted;

        }

        private void _ContentContainer_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _ContentWidth = _ContentContainer.ActualWidth;
            _MaxOffsetX = (_ContentWidth / 4) * 3;
            _LeftTriggerDistance = _ContentWidth / 2;
            _RightTriggerDistance = _ContentWidth / 2 * -1;
            PrepareRightAnimation();
            PrepareLeftAnimation();
            Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
            
        }

        private void _ContentContainer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < 0)
            {
                _LeftContainer.Opacity = 0;
                _RightContainer.Opacity = 1;
            }
            else
            {
                _LeftContainer.Opacity = 1;
                _RightContainer.Opacity = 0;
                
            }
            
            if (e.Cumulative.Translation.X < 0)
            {
                if (e.Cumulative.Translation.X <= _MaxOffsetX * -1)
                {
                    _OffsetX = _MaxOffsetX * -1;
                }
                else
                {
                    _OffsetX = e.Cumulative.Translation.X;
                }
            }
            else
            {
                if (e.Cumulative.Translation.X <= _MaxOffsetX)
                {
                    _OffsetX = e.Cumulative.Translation.X;
                }
                else
                {
                    _OffsetX = _MaxOffsetX;
                }
            }
            
        }

        private void _ContentContainer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;

            if (_OffsetX >= _LeftTriggerDistance)
            {

                // Execute Left Command
                if (LeftCommand != null)
                {
                    LeftCommand.Execute(LeftCommandParameter);
                }
                // Start animation
                StartLeftCommandActivatedAnimation();
            }
            else if (_OffsetX <= _RightTriggerDistance)
            {
                // Execute Right Command
                if (RightCommand != null)
                {
                    RightCommand.Execute(RightCommandParameter);
                }
                // Start Animation
                StartRightCommandActivatedAnimation();
            }
            else
            {
                StartResetAnimations();
            }
            
        }

        private void StartLeftCommandActivatedAnimation()
        {
            var batch = _Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) =>
            {
                _LeftContentVisual.CenterPoint = new Vector3(100, (float)_LeftContent.ActualHeight / 2, 0);
                StartResetAnimations();
            };

            var activatedAnimation = _Compositor.CreateVector3KeyFrameAnimation();
            
            activatedAnimation.InsertKeyFrame(0.5f, new Vector3(1.25f, 1.25f, 1));
            activatedAnimation.InsertKeyFrame(1.0f, new Vector3(1, 1, 1));
            activatedAnimation.Duration = TimeSpan.FromSeconds(0.5);
            _LeftContentVisual.CenterPoint = new Vector3((float)_LeftContent.ActualWidth / 2, (float)_LeftContent.ActualHeight / 2, 0f);
            _LeftContentVisual.StartAnimation("Scale", activatedAnimation);

            batch.End();
        }

        private void StartRightCommandActivatedAnimation()
        {
            var batch = _Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) =>
            {
                _RightContentVisual.CenterPoint = new Vector3(-100, 0, 0);
                StartResetAnimations();
            };

            var activatedAnimation = _Compositor.CreateVector3KeyFrameAnimation();

            activatedAnimation.InsertKeyFrame(0.5f, new Vector3(1.25f, 1.25f, 1));
            activatedAnimation.InsertKeyFrame(1.0f, new Vector3(1, 1, 1));
            activatedAnimation.Duration = TimeSpan.FromSeconds(0.5);
            _RightContentVisual.CenterPoint = new Vector3((float)_RightContent.ActualWidth / 2, (float)_RightContent.ActualHeight / 2, 0f);
            _RightContentVisual.StartAnimation("Scale", activatedAnimation);

            batch.End();
        }

        private void OnCompositionTargetRendering(object sender, object e)
        {
            _ContentVisual.StopAnimation("Offset.X");
            StopLeftAnimations();
            StopRightAnimations();
            SetAnimationParameters();
            _ContentVisual.StartAnimation("Offset.X", _ContentAnimation);
            StartRightAnimations();
            StartLeftAnimations();
        }


        #region PropDPs



        public ICommand LeftCommand
        {
            get { return (ICommand)GetValue(LeftCommandProperty); }
            set { SetValue(LeftCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.Register("LeftCommand", typeof(ICommand), typeof(SwipeActionItem), new PropertyMetadata(null));



        public ICommand RightCommand
        {
            get { return (ICommand)GetValue(RightCommandProperty); }
            set { SetValue(RightCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.Register("RightCommand", typeof(ICommand), typeof(SwipeActionItem), new PropertyMetadata(null));



        public object LeftCommandParameter
        {
            get { return (object)GetValue(LeftCommandParameterProperty); }
            set { SetValue(LeftCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftCommandParameterProperty =
            DependencyProperty.Register("LeftCommandParameter", typeof(object), typeof(SwipeActionItem), new PropertyMetadata(null));



        public object RightCommandParameter
        {
            get { return (object)GetValue(RightCommandParameterProperty); }
            set { SetValue(RightCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightCommandParameterProperty =
            DependencyProperty.Register("RightCommandParameter", typeof(object), typeof(SwipeActionItem), new PropertyMetadata(null));



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



        public SwipeAnimationType LeftAnimationType
        {
            get { return (SwipeAnimationType)GetValue(LeftAnimationTypeProperty); }
            set { SetValue(LeftAnimationTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftAnimationType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftAnimationTypeProperty =
            DependencyProperty.Register("LeftAnimationType", typeof(SwipeAnimationType), typeof(SwipeActionItem), new PropertyMetadata(SwipeAnimationType.RotateFromInside));



        public SwipeAnimationType RightAnimationType
        {
            get { return (SwipeAnimationType)GetValue(RightAnimationTypeProperty); }
            set { SetValue(RightAnimationTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightAnimationType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightAnimationTypeProperty =
            DependencyProperty.Register("RightAnimationType", typeof(SwipeAnimationType), typeof(SwipeActionItem), new PropertyMetadata(SwipeAnimationType.RotateFromInside));



        #endregion

    }

    public enum SwipeAnimationType
    {
        SlideFromInside,
        RotateFromInside
    }
}
