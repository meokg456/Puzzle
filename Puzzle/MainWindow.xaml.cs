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
        int[,] _a = new int[Rows, Columns];
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
				int side = 0; //cạnh hình vuông nhỏ
				if(source.Width < source.Height)
				{
					side = (int) source.Width * 1 / Columns;
				}
				else
				{
					side = (int) source.Height * 1 / Rows;
				}
				image.Stretch = Stretch.Fill;
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
                        }
					}
				}
			}
		}
		bool _isDragging = false;
		Image _selectedImage = null;
		private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_isDragging)
			{
				var dropDownImage = sender as Image;
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
                        if(checkWin() == true)
                        {
                            MessageBox.Show("You won!!!", "Congratulation");
                        }
                    }
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
	}
}
