﻿using Microsoft.Win32;
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
        int[,] _a;
        Image[,] _images;
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
                int check = 1;
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

                            _images[i, j] = cropImage;

							cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
							cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                            cropImage.Tag = new Tuple<int, int>(i, j);
                            _a[i, j] = check;
                            check++;
							//cropImage.MouseLeftButtonUp
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
				Tuple<int, int> tag = dropDownImage.Tag as Tuple<int, int>;
				var dropDownImageSource = dropDownImage.Source;
                var oldDropDownImage = dropDownImage;

                dropDownImage.Source = _selectedImage.Source;
				dropDownImage.Tag = _selectedImage.Tag;
                var (i, j) = _selectedImage.Tag as Tuple<int, int>;
                _images[i, j] = _selectedImage;


                _selectedImage.Source = dropDownImageSource;
                _selectedImage.Tag = tag;
                _images[tag.Item1, tag.Item2] = oldDropDownImage;

                var win = checkWin();
			}
		}

        private bool checkWin()
        {
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    
                }
            }
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isDragging = true;
			_selectedImage = sender as Image;
		}
	}
}
