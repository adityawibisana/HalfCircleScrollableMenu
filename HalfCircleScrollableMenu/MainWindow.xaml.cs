﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace HalfCircleScrollableMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        private int itemsAmount = 7;
        private int visibleItems = 5;
        private int currentIndex = 0; 

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
            DataContext = this;
            Init();
        } 

        public int GetSelectedIndex()
        {
            int helperIndex;
            if (visibleItems % 2 == 0)
            {
                helperIndex = visibleItems / 2 - 1;
            }
            else
            {
                helperIndex = visibleItems / 2;
            }

            int retVal = currentIndex + helperIndex;
            if (retVal >= itemsAmount)
                retVal = retVal - itemsAmount;
            return retVal;
        }

        private void Init()
        {
            if (visibleItems > itemsAmount)
                return;

            currentIndex = 0;

            RotateTransform rt = new RotateTransform() { CenterX = r, CenterY = r };
            rotationContainer = new Grid()
            {
                Width = 2 * r,
                Height = 2 * r,
                RenderTransform = rt
            };

            LayoutRoot.Children.Add(rotationContainer);

            //TODO: CHANGE this into your desired images
            String[] dummyImages = new String[itemsAmount];
            for (int i=0;i<itemsAmount;i++)
            {
                dummyImages[i] = (String.Format(@"Images\{0}.png", i));
            }
            InitWithImages(dummyImages);


            LayoutRoot.Children.Add(new Grid()
            {
                Width = r,
                Height = 2 * r,
                Margin = new Thickness(r * -1, 0, 0, 0)
            });

            this.MouseWheel += ((sender, e) =>
            {
                if (animationRunning)
                    return;

                if (e.Delta > 0)
                {
                    Animate(false);
                }
                else
                {
                    Animate(true);
                }
            });
        }

        public void InitWithImages(String[] Images)
        {
            Storyboard storyboard = new Storyboard();  
            for (int i = 0; i < visibleItems; i++)
            {
                BitmapImage bi = new BitmapImage(new Uri(Images[i], UriKind.Relative));  

                Image im = new Image() { Source = bi, Width = imageWidth, Height = imageHeight};
                im.RenderTransform = new TranslateTransform();
                rotationContainer.Children.Add(im);

                double xPos = 0, yPos = 0;
                if (visibleItems%2==1) //if odd
                {
                    xPos = r * Math.Sin((180 / (visibleItems + 1) * (i + 1)) * (Math.PI / 180));
                    yPos = -1 * r * Math.Cos((180 / (visibleItems + 1) * (i + 1)) * (Math.PI / 180));
                }
                else //if the items is even
                {
                    xPos = r * Math.Sin((180 / (visibleItems) * (i + 1)) * (Math.PI / 180));
                    yPos = -1 * r * Math.Cos((180 / (visibleItems) * (i + 1)) * (Math.PI / 180));
                }
                
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
                    To = yPos
                };
                Storyboard.SetTarget(translateAnimationY, im);
                Storyboard.SetTargetProperty(translateAnimationY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(translateAnimationY);

                positions.Add(new Point(translateAnimationX.To.Value, translateAnimationY.To.Value));

                im.Tag = i; 
            } 

            // create the rest
            for (int i= visibleItems;i<itemsAmount;i++)
            {
                BitmapImage bi = new BitmapImage(new Uri(Images[i], UriKind.Relative));

                Image im = new Image() { Source = bi, Width = imageWidth, Height = imageHeight };
                im.RenderTransform = new TranslateTransform();
                rotationContainer.Children.Add(im);

                double xPos = (-1) * r * Math.Sin((180 / (itemsAmount - visibleItems + 1) * (i-visibleItems + 1)) * (Math.PI / 180));
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
                    To = r * Math.Cos((180 / (itemsAmount - visibleItems + 1) * (i - visibleItems + 1)) * (Math.PI / 180))
                };
                Storyboard.SetTarget(translateAnimationY, im);
                Storyboard.SetTargetProperty(translateAnimationY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(translateAnimationY);

                positions.Add(new Point(translateAnimationX.To.Value, translateAnimationY.To.Value));

                im.Tag = i;

                im.Visibility = Visibility.Hidden; 
            }

            storyboard.Begin();            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (animationRunning)
                return; 

            if (e.Key == Key.Down)
            {  
                Animate(true);
            }
            else if (e.Key == Key.Up)
            {  
                Animate(false);
            }
            else
            {
                return;
            }  
        } 

        public void ScrollToIndex(int destIndex)
        {
            int helperIndex;
            if (visibleItems % 2 == 0)
            {
                helperIndex = visibleItems / 2 - 1;
            }
            else
            {
                helperIndex = visibleItems / 2;
            } 

            destIndex = destIndex - helperIndex;
            if (destIndex < 0)
                destIndex = itemsAmount + destIndex;

            Task.Run(() =>
            {
                while (currentIndex != destIndex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Animate(false);
                    }));
                    Task.Delay(500).Wait();
                }
            });
        }

        private void Animate(bool isDown)
        { 
            Storyboard storyboard = new Storyboard();
            storyboard.SpeedRatio = 4;

            // translate the children
            int i = 0;
            foreach (Image image in rotationContainer.Children)
            {
                //x translation     
                int fromIndex = (int) image.Tag;
                int toIndex = fromIndex;
                if (isDown)
                {
                    toIndex++;
                    if (toIndex>=itemsAmount)
                    {
                        toIndex = 0;
                    }
                }
                else
                {
                    toIndex--;
                    if (toIndex<0)
                    {
                        toIndex = itemsAmount - 1;
                    }
                }

                image.Tag = toIndex;

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

                if (translateAnimationX.To.Value<0)
                {
                    image.Visibility = Visibility.Hidden;
                }
                else
                {
                    image.Visibility = Visibility.Visible;
                }

                i++; 
                
            }

            storyboard.Completed += ((sen, args) =>
            {
                animationRunning = false;                 
            });
            storyboard.Begin();
            animationRunning = true;

            if (!isDown) currentIndex++;
            else currentIndex--;

            if (currentIndex >= itemsAmount)
                currentIndex = 0;
            else if (currentIndex==-1)
            {
                currentIndex = itemsAmount - 1;
            }
        } 

        private void ItemsAmountChanged(object sender, TextChangedEventArgs e)
        {
            if (rotationContainer == null)
                return;

            TextBox tb = sender as TextBox;

            if (Int32.TryParse(tb.Text, out int result))
            {
                rotationContainer.Children.Clear();
                LayoutRoot.Children.Clear();
                positions.Clear();

                itemsAmount = result;
                Init();
            }
        }

        private void VisibleItemsChanged(object sender, TextChangedEventArgs e)
        {
            if (rotationContainer == null)
                return;

            TextBox tb = sender as TextBox;
            if (Int32.TryParse(tb.Text, out int result))
            {
                rotationContainer.Children.Clear();
                LayoutRoot.Children.Clear();
                positions.Clear();

                visibleItems = result;
                Init();
            }
        }

        private void JumpToChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse((sender as TextBox).Text, out int dest))
            {
                if (dest < itemsAmount)                    
                    ScrollToIndex(dest);
            } 
        }

        private void OnIndexClicked(object sender, RoutedEventArgs e)
        {    
            MessageBox.Show("SelectedIndex = "+GetSelectedIndex());
        }
    }
}
