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
		private void NewImageMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var screen = new OpenFileDialog();
			if (screen.ShowDialog() == true)
			{
				var image = new Image();
				var source = new BitmapImage(new Uri(screen.FileName, UriKind.Absolute));
				image.Source = source;
				uiCanvas.Children.Add(image);
				Canvas.SetLeft(image, LeftPadding * 2 + Columns * SideHeight);
				Canvas.SetTop(image, TopPadding);
				int side = 0; //cạnh hình vuông nhỏ
				if(source.Width < source.Height)
				{
					side = (int) source.Width * 1 / 3;
				}
				else
				{
					side = (int) source.Height * 1 / 3;
				}
				image.Stretch = Stretch.Fill;
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						if (!((i == 2) && (j == 2)))
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
							//cropImage.MouseLeftButtonUp
						}
					}
				}
			}
		}

        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            _isDragging = false;
            var position = e.GetPosition(this);

            int x = (int)(position.X - LeftPadding) / (SideHeight + 2) * (SideHeight + 2) + LeftPadding;
            int y = (int)(position.Y - TopPadding) / (SideHeight + 2) * (SideHeight + 2) + TopPadding;

            Canvas.SetLeft(_selectedBitmap, x);
            Canvas.SetTop(_selectedBitmap, y);

            var image = sender as Image;
            var (i, j) = image.Tag as Tuple<int, int>;

            MessageBox.Show($"{i} - {j}");
        }

		private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
            _isDragging = true;
            _selectedBitmap = sender as Image;
            _lastPosition = e.GetPosition(this);
        }


	}
}
