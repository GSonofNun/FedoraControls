using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using FedoraControls.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FedoraControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<ListItem> Items { get; set; }
        public ObservableCollection<OmdbSharp.Movie> MovieItems { get; set; }

        private const string _ItemsHost = "ItemsHost";
        private const string _ContentContainer = "ContentContainer";

        private List<ScalarKeyFrameAnimation> ItemOffsetAnimations, ItemOpacityAnimations;
        private ScalarKeyFrameAnimation HeaderOutAnimation, HeaderInAnimation, 
            HeaderFadeOutAnimation, HeaderFadeInAnimation, 
            TitleOutAnimation, TitleInAnimation, 
            TitleFadeOutAnimation, TitleFadeInAnimation, 
            BackgroundInAnimation, BackgroundOutAnimation, 
            SelectedBackgroundInAnimation, SelectedBackgroundOutAnimation,
            SidebarInAnimation, SidebarOutAnimation,
            SidebarFadeInAnimation, SidebarFadeOutAnimation;
        private List<Visual> ItemVisuals;
        private ListView ItemsHost;
        //private Border ContentContainer;
        private Visual HeaderVisual, TitleVisual, BackgroundImage, SelectedBackgroundImage, SidebarVisual, DescriptionVisual;
        private Compositor _Compositor;

        private float ItemOffsetY = 300;
        private double AnimationDuration = 0.5;
        private float SidebarWidth = 0;

        private OmdbSharp.Movie SelectedItem = null;

        private CubicBezierEasingFunction EaseIn, EaseOut;

        public MainPage()
        {
            this.InitializeComponent();
            Items = new ObservableCollection<ListItem>();
            MovieItems = new ObservableCollection<OmdbSharp.Movie>();
            ItemVisuals = new List<Visual>();
            ItemOffsetAnimations = new List<ScalarKeyFrameAnimation>();
            ItemOpacityAnimations = new List<ScalarKeyFrameAnimation>();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            ItemsHost = lstSuggestedMovies;
            

            lstSuggestedMovies.ItemsSource = MovieItems;

            HeaderVisual = ElementCompositionPreview.GetElementVisual(txtHeader);
            TitleVisual = ElementCompositionPreview.GetElementVisual(txtTitle);
            SidebarVisual = ElementCompositionPreview.GetElementVisual(AboutPanel);
            SidebarVisual.Offset = new Vector3(1000, 0, 0);
            DescriptionVisual = ElementCompositionPreview.GetElementVisual(txtDescription);
            BackgroundImage = ElementCompositionPreview.GetElementVisual(imgBackground);
            SelectedBackgroundImage = ElementCompositionPreview.GetElementVisual(imgSelectedImageBackground);
            SelectedBackgroundImage.Opacity = 0f;
            TitleVisual.Opacity = 0f;
            TitleVisual.Offset = new Vector3(0, -25f, 0);

            _Compositor = HeaderVisual.Compositor;

            SetupAnimations();

            this.DataContext = this;

            LoadMovies();

            for (int i = 0; i < 1; i++)
            {
                Items.Add(new ListItem("Captain America: Civil War", "Assets/MovieArt/Captain-America-Civil-War-poster.jpg", "After another incident involving the Avengers results in collateral damage, political pressure mounts to install a system of accountability, headed by a governing body to oversee and direct the team. The new status quo fractures the Avengers, resulting in two camps, one led by Steve Rogers and his desire for the Avengers to remain free to defend humanity without government interference, and the other following Tony Stark's surprising decision to support government oversight and accountability."));
                Items.Add(new ListItem("Batman V Superman: Dawn of Justice", "Assets/MovieArt/batman_v_superman_poster.jpg", "Following his titanic struggle against General Zod, Metropolis has been razed to the ground and Superman is the most controversial figure in the world. While for many he is still an emblem of hope, a growing number of people consider him a threat to humanity, seeking justice for the chaos he has brought to Earth. As far as Bruce Wayne is concerned, Superman is clearly a danger to society. He fears for the future of the world with such a reckless power left ungoverned, and so he dons his mask and cape to right Superman's wrongs. The rivalry between them is furious, fueled by bitterness and vengeance, and nothing can dissuade them from waging this war. However, a dark new threat arises in the form of a third man: one who has a power greater than either of them to endanger the world and cause total destruction."));
                Items.Add(new ListItem("Deadpool", "Assets/MovieArt/deadpool-ver8-acc2e.jpg", "Based upon Marvel Comics most unconventional anti-hero, DEADPOOL tells the origin story of former Special Forces operative turned mercenary Wade Wilson, who after being subjected to a rogue experiment that leaves him with accelerated healing powers, adopts the alter ego Deadpool. Armed with his new abilities and a dark, twisted sense of humor, Deadpool hunts down the man who nearly destroyed his life."));
                Items.Add(new ListItem("X-Men Apocalypse", "Assets/MovieArt/x-men-apocalypse-poster-full.jpg", "Since the dawn of civilization, he was worshiped as a god. Apocalypse, the first and most powerful mutant from Marvel's X-Men universe, amassed the powers of many other mutants, becoming immortal and invincible. Upon awakening after thousands of years, he is disillusioned with the world as he finds it and recruits a team of powerful mutants, including a disheartened Magneto, to cleanse mankind and create a new world order, over which he will reign. As the fate of the Earth hangs in the balance, Raven with the help of Professor X must lead a team of young X-Men to stop their greatest nemesis and save mankind from complete destruction."));
                Items.Add(new ListItem("The Jungle Book", "Assets/MovieArt/the_jungle_book_2016_poster.jpg", "Mowgli is a boy who was raised by the Indian wolves Raksha and Akela. When the fearsome Bengal tiger Shere Khan threatens his life, Mowgli leaves his jungle home. Guided by Bagheera the black panther and Baloo the bear , he sets out on a journey of self-discovery. Along the way, Mowgli encounters jungle creatures who do not exactly have his best interests at heart-including Kaa, a python whose seductive voice and gaze hypnotizes the man-cub, and the smooth-talking King Louie, an orangutan-resembling Gigantopithecus who tries to coerce Mowgli into giving up the secret to the elusive and deadly \"red flower\" (fire)."));
                Items.Add(new ListItem("Finding Dory", "Assets/MovieArt/finding_dory_poster.jpg", "“Finding Dory” reunites the friendly-but-forgetful blue tang fish with her loved ones, and everyone learns a few things about the true meaning of family along the way. The all-new big-screen adventure dives into theaters in 2016, taking moviegoers back to the extraordinary underwater world from the original film. "));
                Items.Add(new ListItem("The Finest Hours", "Assets/MovieArt/The-Finest-Hours-Poster.jpg", "“In February of 1952, one of the worst storms to ever hit the East Coast struck New England, damaging an oil tanker off the coast of Cape Cod and literally ripping it in half.On a small lifeboat faced with frigid temperatures and 70 - foot high waves, four members of the Coast Guard set out to rescue the more than 30 stranded sailors trapped aboard the rapidly-sinking vessel."));
                Items.Add(new ListItem("The Fifth Wave", "Assets/MovieArt/the-fifth-wave-poster.jpg", "Four waves of increasingly deadly attacks have left most of Earth in ruin. Against a backdrop of fear and distrust, Cassie is on the run, desperately trying to save her younger brother. As she prepares for the inevitable and lethal fifth wave, Cassie teams up with a young man who may become her final hope - if she can only trust him."));
                Items.Add(new ListItem("Concussion", "Assets/MovieArt/Concussion-Poster.jpg", "Will Smith stars in Concussion, a dramatic thriller based on the incredible true David vs. Goliath story of American immigrant Dr. Bennet Omalu, the brilliant forensic neuropathologist who made the first discovery of CTE, a football-related brain trauma, in a pro player and fought for the truth to be known. Omalu's emotional quest puts him at dangerous odds with one of the most powerful institutions in the world."));

            }
            //Items.Add(new ListItem("", "Assets/MovieArt/.jpg", ""));

            
        }


        private async void lstSuggestedMovies_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedItem = e.ClickedItem as OmdbSharp.Movie;
            SidebarVisual.Offset = new Vector3((float)AboutPanel.ActualWidth, 0, 0);
            txtTitle.Text = SelectedItem.Title;
            txtDescription.Text = SelectedItem.Plot;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new Uri(imgSelectedImageBackground.BaseUri, SelectedItem.Poster);
            imgSelectedImageBackground.Source = bitmapImage;
            StartClosedAnimation();
            //imgSelectedImageBackground.Source = await HiddenImage.BlurElementAsync();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            StartOpenAnimation();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            e.Handled = true;
        }


        private void SetupAnimations()
        {
            // Used for incoming elements
            //EaseOut = _Compositor.CreateCubicBezierEasingFunction(new Vector2(0f, 0f), new Vector2(0.58f, 1f));
            EaseOut = _Compositor.CreateCubicBezierEasingFunction(new Vector2(0.8f, 0f), new Vector2(0.2f, 1f));
            // Used for outgoing elements
            //EaseIn = _Compositor.CreateCubicBezierEasingFunction(new Vector2(0.42f, 0f), new Vector2(1f, 1f));
            EaseIn = _Compositor.CreateCubicBezierEasingFunction(new Vector2(0.8f, 0f), new Vector2(0.2f, 1f));

            // HeaderOutAnimation
            HeaderOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            HeaderOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            HeaderOutAnimation.InsertKeyFrame(1.0f, 25f, EaseIn);
            // HeaderInAnimation
            HeaderInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            HeaderInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            HeaderInAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            HeaderInAnimation.InsertKeyFrame(1.0f, 0f, EaseOut);
            // HeaderFadeInAnimation
            HeaderFadeInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            HeaderFadeInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            HeaderFadeInAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            HeaderFadeInAnimation.InsertKeyFrame(1.0f, 1f, EaseOut);
            // HeaderFadeOutAnimation
            HeaderFadeOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            HeaderFadeOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            HeaderFadeOutAnimation.InsertKeyFrame(1.0f, 0f, EaseIn);
            // TitleOutAnimation
            TitleOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            TitleOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            TitleOutAnimation.InsertKeyFrame(1.0f, -25f, EaseIn);
            // TitleInAnimation
            TitleInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            TitleInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            TitleInAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            TitleInAnimation.InsertKeyFrame(1.0f, 0f, EaseOut);
            // TitleFadeInAnimation
            TitleFadeInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            TitleFadeInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            TitleFadeInAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            TitleFadeInAnimation.InsertKeyFrame(1.0f, 1f, EaseOut);
            // TitleFadeOutAnimation
            TitleFadeOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            TitleFadeOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration / 2);
            TitleFadeOutAnimation.InsertKeyFrame(1.0f, 0f, EaseIn);
            // BackgroundInAnimation
            BackgroundInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            BackgroundInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            BackgroundInAnimation.InsertKeyFrame(1.0f, 1.0f, EaseOut);
            // BackgroundOutAnimation
            BackgroundOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            BackgroundOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            BackgroundOutAnimation.InsertKeyFrame(1.0f, 0f, EaseIn);
            // SelectedBackgroundInAnimation
            SelectedBackgroundInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            SelectedBackgroundInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            SelectedBackgroundInAnimation.InsertKeyFrame(1.0f, 1.0f, EaseOut);
            // SelectedBackgroundOutAnimation
            SelectedBackgroundOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            SelectedBackgroundOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            SelectedBackgroundOutAnimation.InsertKeyFrame(1.0f, 0f, EaseIn);
            
        }

        private void SetUpSidebarAnimations()
        {
            SidebarWidth = (float)AboutPanel.ActualWidth;
            // SidebarInAnimation
            SidebarInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            SidebarInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            SidebarInAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            SidebarInAnimation.InsertKeyFrame(1.0f, 0f, EaseOut);
            // SidebarOutAnimation
            SidebarOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            SidebarOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            //SidebarOutAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            SidebarOutAnimation.InsertKeyFrame(1.0f, SidebarWidth, EaseIn);
            // SidebarFadeInAnimation
            SidebarFadeInAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            SidebarFadeInAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            SidebarFadeInAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            SidebarFadeInAnimation.InsertKeyFrame(1.0f, 1.0f, EaseOut);
            // SidebarFadeOutAnimation
            SidebarFadeOutAnimation = _Compositor.CreateScalarKeyFrameAnimation();
            SidebarFadeOutAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
            //SidebarFadeOutAnimation.DelayTime = TimeSpan.FromSeconds(AnimationDuration / 2);
            SidebarFadeOutAnimation.InsertKeyFrame(1.0f, 0f, EaseIn);
        }

        private void StartOpenAnimation()
        {
            SetupItemVisuals();
            SetUpSidebarAnimations();
            ItemOffsetAnimations.Clear();
            ItemOpacityAnimations.Clear();
            double currentDelay = 0.05;
            double itemIndex = MovieItems.IndexOf(SelectedItem);
            //currentDelay = currentDelay * itemIndex;
            for (int i = 0; i < ItemsHost.Items.Count; i++)
            {
                ScalarKeyFrameAnimation itemAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                itemAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
                itemAnimation.DelayTime = TimeSpan.FromSeconds((AnimationDuration / 2) + currentDelay);
                itemAnimation.InsertKeyFrame(1.0f, 0f, EaseOut);
                ItemOffsetAnimations.Add(itemAnimation);

                ScalarKeyFrameAnimation itemOpacityAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                itemOpacityAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
                itemOpacityAnimation.DelayTime = TimeSpan.FromSeconds((AnimationDuration / 2) + currentDelay);
                itemOpacityAnimation.InsertKeyFrame(1.0f, 1.0f, EaseOut);
                ItemOpacityAnimations.Add(itemOpacityAnimation);

                if (i < itemIndex)
                {
                    currentDelay += 0.05;
                }
                else
                {
                    currentDelay -= 0.05;

                    if (currentDelay <= 0)
                    {
                        currentDelay = 0;
                    }
                }
            }

            var batch = _Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            HeaderVisual.StartAnimation("Offset.Y", HeaderInAnimation);
            HeaderVisual.StartAnimation("Opacity", HeaderFadeInAnimation);
            TitleVisual.StartAnimation("Offset.Y", TitleOutAnimation);
            TitleVisual.StartAnimation("Opacity", TitleFadeOutAnimation);
            BackgroundImage.StartAnimation("Opacity", BackgroundInAnimation);
            SelectedBackgroundImage.StartAnimation("Opacity", SelectedBackgroundOutAnimation);
            SidebarVisual.StartAnimation("Offset.X", SidebarOutAnimation);
            SidebarVisual.StartAnimation("Opacity", SidebarFadeOutAnimation);
            for (int i = 0; i < ItemVisuals.Count; i++)
            {
                ItemVisuals[i].StartAnimation("Offset.Y", ItemOffsetAnimations[i]);
                ItemVisuals[i].StartAnimation("Opacity", ItemOpacityAnimations[i]);
            }
            batch.End();
        }

        private void StartClosedAnimation()
        {
            SetupItemVisuals();
            SetUpSidebarAnimations();
            ItemOffsetAnimations.Clear();
            ItemOpacityAnimations.Clear();
            double currentDelay = 0.05;
            double itemIndex = MovieItems.IndexOf(SelectedItem);
            for (int i = 0; i < ItemsHost.Items.Count; i++)
            {
                ScalarKeyFrameAnimation itemAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                itemAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
                itemAnimation.DelayTime = TimeSpan.FromSeconds(currentDelay);
                //CubicBezierEasingFunction ease = _Compositor.CreateCubicBezierEasingFunction(new Vector2(0.42f, 0f), new Vector2(1f, 1f));
                itemAnimation.InsertKeyFrame(1.0f, ItemOffsetY, EaseIn);
                ItemOffsetAnimations.Add(itemAnimation);

                ScalarKeyFrameAnimation itemOpacityAnimation = _Compositor.CreateScalarKeyFrameAnimation();
                itemOpacityAnimation.Duration = TimeSpan.FromSeconds(AnimationDuration);
                itemOpacityAnimation.DelayTime = TimeSpan.FromSeconds(currentDelay);
                itemOpacityAnimation.InsertKeyFrame(1.0f, 0f, EaseIn);
                ItemOpacityAnimations.Add(itemOpacityAnimation);

                if (i < itemIndex)
                {
                    currentDelay += 0.05;
                }
                else
                {
                    currentDelay -= 0.05;
                    if (currentDelay <= 0)
                    {
                        currentDelay = 0;
                    }
                }
            }

            var batch = _Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            HeaderVisual.StartAnimation("Offset.Y", HeaderOutAnimation);
            HeaderVisual.StartAnimation("Opacity", HeaderFadeOutAnimation);
            TitleVisual.StartAnimation("Offset.Y", TitleInAnimation);
            TitleVisual.StartAnimation("Opacity", TitleFadeInAnimation);
            BackgroundImage.StartAnimation("Opacity", BackgroundOutAnimation);
            SelectedBackgroundImage.StartAnimation("Opacity", SelectedBackgroundInAnimation);
            SidebarVisual.StartAnimation("Offset.X", SidebarInAnimation);
            SidebarVisual.StartAnimation("Opacity", SidebarFadeInAnimation);
            for (int i = 0; i < ItemVisuals.Count; i++)
            {
                ItemVisuals[i].StartAnimation("Offset.Y", ItemOffsetAnimations[i]);
                ItemVisuals[i].StartAnimation("Opacity", ItemOpacityAnimations[i]);
            }
            batch.End();
        }

        

        public void SetupItemVisuals()
        {
            if (ItemsHost == null)
            {
                return;
            }
            if (ItemsHost.Items.Count > 0)
            {
                ItemVisuals.Clear();
                for (int i = 0; i < ItemsHost.Items.Count; i++)
                {
                    ListViewItem itemContainer = ItemsHost.ContainerFromIndex(i) as ListViewItem;
                    if (itemContainer != null)
                    {
                        Visual itemVisual = ElementCompositionPreview.GetElementVisual(itemContainer);
                        //itemVisual.Offset = new Vector3(0, 0, 0);
                        ItemVisuals.Add(itemVisual);
                    }

                }
            }
        }



        private async void LoadMovies()
        {
            OmdbSharp.Omdb omdb = new OmdbSharp.Omdb();
            var movies = await omdb.GetMovieList("Ryan Reynolds");
            foreach (var movie in movies.Search)
            {
                MovieItems.Add(await omdb.GetMovie(movie.Title));
            }
        }

    }


    public class ListItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string ImageSource { get; set; }
        public string Description { get; set; }

        public ListItem(string title, string imageSource, string description)
        {
            ID = IDManager.GetNextID();
            Title = title;
            ImageSource = imageSource;
            Description = description;
        }
    }

    public static class IDManager
    {
        private static int currentID = 0;

        public static int GetNextID()
        {
            return currentID++;
        }
    }
}
