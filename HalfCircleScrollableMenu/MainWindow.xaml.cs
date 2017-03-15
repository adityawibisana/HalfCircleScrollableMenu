using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HalfCircleScrollableMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int itemsAmount = 9;        

        private int currentTransformIndex = 0;
        private int prevCurrentTransformIndex = 0;

        private DateTime lastMouseWheelEvent = DateTime.MinValue;

        DoubleAnimation rotateAnimation = new DoubleAnimation();
        private bool animationRunning = false;
        double r = 300;
        double imageWidth = 150;
        double imageHeight = 150; 

        Grid rotationContainer;
        List<Point> positions = new List<Point>();

        public MainWindow()
        {
            InitializeComponent();

            RotateTransform rt = new RotateTransform() { CenterX = r, CenterY = r };
            rotationContainer = new Grid()
            {
                Width = 2*r,
                Height = 2*r,
                RenderTransform = rt
            };

            LayoutRoot.Children.Add(rotationContainer);

            InitWithImages();
            LayoutRoot.Children.Add(new Grid()
            {
                Width = r,
                Height = 2*r,
                Margin = new Thickness(r * -1, 0, 0, 0)
            }); 

            this.MouseWheel += ((sender, e) =>
            {
                if (animationRunning)
                    return;

                prevCurrentTransformIndex = currentTransformIndex;


                if (e.Delta>0)
                {
                    if (currentTransformIndex > 0)
                        currentTransformIndex--;
                    else
                        currentTransformIndex = itemsAmount - 1;

                    Animate(false);
                }
                else
                {
                    if (currentTransformIndex < itemsAmount - 1)
                        currentTransformIndex++;
                    else
                        currentTransformIndex = 0;

                    Animate(true);
                }
            });
            
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void InitWithImages()
        {
            Storyboard storyboard = new Storyboard();  
            for (int i = 0; i < itemsAmount; i++)
            {
                BitmapImage bi = new BitmapImage(new Uri(String.Format(@"Images\{0}.png", i), UriKind.Relative));  

                Image im = new Image() { Source = bi, Width = imageWidth, Height = imageHeight};
                im.RenderTransform = new TranslateTransform();
                rotationContainer.Children.Add(im);

                double xPos = r * Math.Sin((360 / itemsAmount * i) * (Math.PI / 180));
                DoubleAnimation translateAnimationX = new DoubleAnimation()
                {
                    From = 0,
                    To = xPos
                };
                Storyboard.SetTarget(translateAnimationX, im);
                Storyboard.SetTargetProperty(translateAnimationX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                storyboard.Children.Add(translateAnimationX);

                DoubleAnimation translateAnimationY = new DoubleAnimation()
                {
                    From = 0,
                    To = -1 * r * Math.Cos((360 / itemsAmount * i) * (Math.PI / 180))
                };
                Storyboard.SetTarget(translateAnimationY, im);
                Storyboard.SetTargetProperty(translateAnimationY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(translateAnimationY);

                positions.Add(new Point(translateAnimationX.To.Value, translateAnimationY.To.Value));

                if (xPos<0)
                {
                    im.Visibility = Visibility.Hidden;
                }
            } 

            storyboard.Begin();            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (animationRunning)
                return; 

            prevCurrentTransformIndex = currentTransformIndex;            
            if (e.Key == Key.Down)
            {
                if (currentTransformIndex < itemsAmount - 1)
                    currentTransformIndex++;
                else
                    currentTransformIndex = 0;

                Animate(true);
            }
            else if (e.Key == Key.Up)
            {
                if (currentTransformIndex > 0)
                    currentTransformIndex--;
                else 
                    currentTransformIndex = itemsAmount - 1;

                Animate(false);
            }
            else
            {
                return;
            } 
        } 

        private void Animate(bool isDown)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.SpeedRatio = 3;

            // translate the children
            int i = 0;
            foreach (Image image in rotationContainer.Children)
            {
                //x translation     
                int fromIndex = i + prevCurrentTransformIndex >= itemsAmount ? i + prevCurrentTransformIndex - itemsAmount : i + prevCurrentTransformIndex;

                int addition = isDown ? 1 : -1;
                int toIndex = fromIndex + addition >= itemsAmount ? fromIndex + addition - itemsAmount : fromIndex + addition;
                toIndex = toIndex == -1 ? itemsAmount - 1 : toIndex;

                DoubleAnimation translateAnimationX = new DoubleAnimation()
                {
                    From = positions[fromIndex].X,
                    To = positions[toIndex].X
                };
                Storyboard.SetTarget(translateAnimationX, image);
                Storyboard.SetTargetProperty(translateAnimationX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                storyboard.Children.Add(translateAnimationX);

                //y translation
                DoubleAnimation translateAnimationY = new DoubleAnimation()
                {
                    From = positions[fromIndex].Y,
                    To = positions[toIndex].Y
                };
                Storyboard.SetTarget(translateAnimationY, image);
                Storyboard.SetTargetProperty(translateAnimationY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(translateAnimationY);

                i++;

                image.Visibility = translateAnimationX.To >= 0 ? Visibility.Visible : Visibility.Hidden; 
            }

            storyboard.Completed += ((sen, args) =>
            {
                animationRunning = false; 
            });
            storyboard.Begin();
            animationRunning = true;
        }

        private void SetImageVisibility(Image image)
        {
            if (image.RenderTransform.Value.OffsetX < 0)
            {
                image.Visibility = Visibility.Collapsed;
            }
            else
            {
                image.Visibility = Visibility.Visible;
            }
        }
    }
}
