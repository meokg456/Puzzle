using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        
        public MainWindow()
		{
			InitializeComponent();
        }

        const int LeftPadding = 10;
		const int TopPadding = 10;
		const int Rows = 3;
		const int Columns = 3;
		const int SideHeight = 100;
        int[,] _a = new int[Rows, Columns];
        Image[,] _pieces = new Image[Rows, Columns];
		private void NewImageMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var screen = new OpenFileDialog();
			if (screen.ShowDialog() == true)
			{
				var image = new Image();
				var source = new BitmapImage(new Uri(screen.FileName, UriKind.Absolute));
				image.Stretch = Stretch.Fill;
				double ratio = source.Height / source.Width; //ti le cao / dai
				if (ratio >= 1)
				{
					image.Height = 350;
					image.Width = 350 * 1 / ratio;
				}
				else
				{
					image.Width = 350;
					image.Height = 350 * ratio;
				}
				image.Source = source;
				uiCanvas.Children.Add(image);
				Canvas.SetLeft(image, LeftPadding + Columns * SideHeight + 80);
				Canvas.SetTop(image, TopPadding);

				cropImage(source);
				
                Shuffle();
			}
		}

		private void cropImage(BitmapImage source)
		{
			int side = 0; //cạnh hình vuông nhỏ
			if (source.Width < source.Height)
			{
				side = (int)source.Width * 1 / Columns;
			}
			else
			{
				side = (int)source.Height * 1 / Rows;
			}
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					if (!((i == Rows - 1) && (j == Columns - 1)))
					{
						//Debug.WriteLine($"Len = {len}");
						var rect = new Int32Rect(j * side, i * side, side, side);
						var cropBitmap = new CroppedBitmap(source, rect);

						var cropImage = new Image();
						cropImage.Stretch = Stretch.Fill;
						cropImage.Width = SideHeight;
						cropImage.Height = SideHeight;
						cropImage.Source = cropBitmap;
						uiCanvas.Children.Add(cropImage);
						Canvas.SetLeft(cropImage, LeftPadding + j * (SideHeight + 2));
						Canvas.SetTop(cropImage, TopPadding + i * (SideHeight + 2));


						cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
						cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
						cropImage.Tag = new Tuple<int, int>(i, j);
						_a[i, j] = i * Rows + j + 1;
						_pieces[i, j] = cropImage;
						//cropImage.MouseLeftButtonUp
					}
					else
					{

						var cropImage = new Image();
						cropImage.Stretch = Stretch.Fill;
						cropImage.Width = SideHeight;
						cropImage.Height = SideHeight;
						cropImage.Source = new BitmapImage(new Uri("Blank.png", UriKind.Relative));
						uiCanvas.Children.Add(cropImage);
						Canvas.SetLeft(cropImage, LeftPadding + j * (SideHeight + 2));
						Canvas.SetTop(cropImage, TopPadding + i * (SideHeight + 2));

						cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
						cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
						cropImage.Tag = new Tuple<int, int>(i, j);
						_a[i, j] = 0;
						_pieces[i, j] = cropImage;

					}
				}
			}
		}

		bool _isDragging = false;
		Image _selectedImage = null;
        private void Shuffle()
        {
            int[] di = new int[] { 0, 0, -1, 1 };
            int[] dj = new int[] { -1, 1, 0, 0 };
            int emptyI = Rows - 1;
            int emptyJ = Columns -1;
            Random rng = new Random();
            for(int i = 0; i < 1000; i++)
            {
                var move = rng.Next(4);
                if(emptyI + di[move] >= 0 && emptyI + di[move] < Rows && emptyJ + dj[move] >= 0 && emptyJ + dj[move] < Columns)
                {
                    _selectedImage = _pieces[emptyI + di[move], emptyJ + dj[move]];
                    swapToSource(_pieces[emptyI, emptyJ]);
                    emptyI = emptyI + di[move];
                    emptyJ = emptyJ + dj[move];
                }
            }

        }
		private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_isDragging)
			{
				var dropDownImage = sender as Image;
                swapToSource(dropDownImage);
				if (checkWin() == true)
				{
                    timer.Stop();
					MessageBox.Show("You won!!!", "Congratulation");
				}

			}
            _isDragging = !_isDragging;
		}

        private void swapToSource(Image dropDownImage)
        {
            var desTag = dropDownImage.Tag as Tuple<int, int>;
            var i = desTag.Item1;
            var j = desTag.Item2;
            if (_a[i, j] == 0)
            {
                var sourceTag = _selectedImage.Tag as Tuple<int, int>;
                if (nextTo(sourceTag, desTag))
                {
                    _a[i, j] = _a[sourceTag.Item1, sourceTag.Item2];
                    _a[sourceTag.Item1, sourceTag.Item2] = 0;
                    var dropDownImageSource = dropDownImage.Source;
                    dropDownImage.Source = _selectedImage.Source;
                    _selectedImage.Source = dropDownImageSource;
                    
                }
            }
        }

        private bool checkWin()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if(i != Rows - 1 || j != Columns - 1)
                        if (_a[i, j] != i * Rows + j + 1)
                        {
                            return false;
                        }
                }
            }
            return true;
        }


        private bool nextTo(Tuple<int, int> sourceTag, Tuple<int, int> desTag)
        {
            int[] di = new int[] { 0, 0, -1, 1 };
            int[] dj = new int[] { -1, 1, 0, 0 };
            for(int i = 0; i < 4; i++)
            {
                if (desTag.Item1 == sourceTag.Item1 + di[i] && desTag.Item2 == sourceTag.Item2 + dj[i])
                {
                    return true;
                }
            }
            return false;
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isDragging = true;
			_selectedImage = sender as Image;
            
		}
        
        DispatcherTimer timer = new DispatcherTimer();
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        int minutes = 2;
        int seconds = 59;
        void timer_Tick(object sender, EventArgs e)
        {
            if (seconds == -1)
            {
                if (minutes == 0)
                {
                    timer.Stop();
                    MessageBox.Show("You losted! Try again^^");
                    return;
                }
                else
                {
                    minutes--;
                    seconds = 59;
                }
            }
            var time = $"{minutes}:{seconds}";
            lblTime.Content = time;
            seconds--;
        }
    }
}
