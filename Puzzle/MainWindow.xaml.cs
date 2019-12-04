using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
		const string FileName = "Save.txt";
		int[,] _a = new int[Rows, Columns];
        Image[,] _pieces = new Image[Rows, Columns];
        BitmapImage _source = null;
        string dirImgRoot;
        private void NewImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var screen = new OpenFileDialog();
                if (screen.ShowDialog() == true)
                {
                    loadImage(screen.FileName);

                    Button playButton = new Button();
                    playButton.Height = 90;
                    playButton.Width = 240;
                    playButton.FontSize = 48;
                    playButton.Content = "Play";
                    uiCanvas.Children.Add(playButton);
                    Canvas.SetLeft(playButton, 430);
                    Canvas.SetTop(playButton, 250);
                    playButton.Click += PlayButton_Click;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            
        }

        private void loadImage(string fileName)
        {
            try
            {
                var image = new Image();
                _source = new BitmapImage(new Uri(fileName, UriKind.Absolute));
                image.Stretch = Stretch.Fill;
                double ratio = _source.Height / _source.Width; //ti le cao / dai
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
                image.Source = _source;
                uiCanvas.Children.Add(image);
                Canvas.SetLeft(image, LeftPadding + Columns * SideHeight + 80);
                Canvas.SetTop(image, TopPadding);
                dirImgRoot = fileName;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }
        private void cropImage()
        {
            try
            {
                int side = 0; //cạnh hình vuông nhỏ
                if (_source.Width < _source.Height)
                {
                    side = (int)_source.Width * 1 / Columns;
                }
                else
                {
                    side = (int)_source.Height * 1 / Rows;
                }
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        if (!((i == Rows - 1) && (j == Columns - 1)))
                        {
                            var rect = new Int32Rect(j * side, i * side, side, side);
                            var cropBitmap = new CroppedBitmap(_source, rect);

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
                            cropImage.PreviewMouseMove += CropImage_PreviewMouseMove;
                            cropImage.Tag = new Tuple<int, int>(i, j);
                            _a[i, j] = i * Rows + j + 1;
                            _pieces[i, j] = cropImage;
                        }
                        else
                        {
                            _pieces[i, j] = null;
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void CropImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (_isDragging)
                {
                    var position = e.GetPosition(this);

                    var dx = position.X - _lastPosition.X;
                    var dy = position.Y - _lastPosition.Y;

                    var lastLeft = Canvas.GetLeft(_selectedImage);
                    var lastTop = Canvas.GetTop(_selectedImage);
                    var image = sender as Image;
                    Canvas.SetLeft(_selectedImage, lastLeft + dx);
                    Canvas.SetTop(_selectedImage, lastTop + dy);


                    _lastPosition = position;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        bool _isDragging = false;
        Image _selectedImage = null;
        Point _lastPosition;
        private void Shuffle()
        {
            try
            {
                int[] di = new int[] { 0, 0, -1, 1 };
                int[] dj = new int[] { -1, 1, 0, 0 };
                int emptyI = Rows - 1;
                int emptyJ = Columns - 1;
                Random rng = new Random();
                for (int i = 0; i < 1000; i++)
                {
                    var move = rng.Next(4);
                    if (emptyI + di[move] >= 0 && emptyI + di[move] < Rows && emptyJ + dj[move] >= 0 && emptyJ + dj[move] < Columns)
                    {
                        _selectedImage = _pieces[emptyI + di[move], emptyJ + dj[move]];
                        swapToSelectedItem(new Tuple<int, int>(emptyI, emptyJ));
                        _pieces[emptyI + di[move], emptyJ + dj[move]] = null;
                        _pieces[emptyI, emptyJ] = _selectedImage;
                        emptyI = emptyI + di[move];
                        emptyJ = emptyJ + dj[move];

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_isDragging)
                {
                    var dropDownImage = sender as Image;
                    var position = e.GetPosition(this);
                    var j = (int)(position.X - LeftPadding) / (SideHeight + 2);
                    var i = (int)(position.Y - TopPadding) / (SideHeight + 2);
                    if (swapToSelectedItem(new Tuple<int, int>(i, j)))
                    {
                        if (checkWin() == true)
                        {
                            timer.Stop();
                            MessageBox.Show("You won!!!", "Congratulation");
                        }
                        Canvas.SetZIndex(_selectedImage, 1);
                    }
                    _isDragging = false;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }
        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _isDragging = true;
                _selectedImage = sender as Image;
                _lastPosition = e.GetPosition(this);
                Canvas.SetZIndex(_selectedImage, 2);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private bool swapToSelectedItem(Tuple<int, int> desTag)
        {

            var i = desTag.Item1;
            var j = desTag.Item2;
            var sourceTag = _selectedImage.Tag as Tuple<int, int>;
            if (i >= 0 && i < Rows && j >= 0 && j < Columns)
                if (_a[i, j] == 0)
                {
                    if (nextTo(sourceTag, desTag))
                    {
                        _a[i, j] = _a[sourceTag.Item1, sourceTag.Item2];
                        _a[sourceTag.Item1, sourceTag.Item2] = 0;
                        _pieces[sourceTag.Item1, sourceTag.Item2] = null;
                        _pieces[i, j] = _selectedImage;
                        Canvas.SetLeft(_selectedImage, LeftPadding + j * (SideHeight + 2));
                        Canvas.SetTop(_selectedImage, TopPadding + i * (SideHeight + 2));
                        _selectedImage.Tag = desTag;
                        return true;
                    }

                }
            Canvas.SetLeft(_selectedImage, LeftPadding + sourceTag.Item2 * (SideHeight + 2));
            Canvas.SetTop(_selectedImage, TopPadding + sourceTag.Item1 * (SideHeight + 2));
            return false;
        }

        private bool checkWin()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (i != Rows - 1 || j != Columns - 1)
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
            for (int i = 0; i < 4; i++)
            {
                if (desTag.Item1 == sourceTag.Item1 + di[i] && desTag.Item2 == sourceTag.Item2 + dj[i])
                {
                    return true;
                }
            }
            return false;
        }
        bool _isPlaying = false;
        DispatcherTimer timer = new DispatcherTimer();
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isPlaying = true;
                var playButton = sender as Button;
                uiCanvas.Children.Remove(playButton);

                cropImage();

                Shuffle();

                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        int minutes = 0;
        int seconds = 10;
        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (seconds == -1)
                {
                    if (minutes == 0)
                    {
                        timer.Stop();
                        MessageBox.Show("You lose! Try again^^");
                        _isPlaying = false;
                        Restart();
                        return;
                    }
                    else
                    {
                        minutes--;
                        seconds = 59;
                    }
                }
                var time = $"{minutes.ToString("00")}:{seconds.ToString("00")}";
                lblTime.Content = time;
                seconds--;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Restart()
        {
            Button restartButton = new Button();
            restartButton.Height = 90;
            restartButton.Width = 240;
            restartButton.FontSize = 48;
            restartButton.Content = "Restart";
            uiCanvas.Children.Add(restartButton);
            Canvas.SetLeft(restartButton, 430);
            Canvas.SetTop(restartButton, 250);
            restartButton.Click += RestartButton_Click;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isPlaying = true;
                var restartButton = sender as Button;
                uiCanvas.Children.Remove(restartButton);

                for (int i = 0; i < Rows; i++)
                {
                    for(int j = 0; j < Columns; j++)
                    {
                        if (_pieces[i, j] != null)
                        {
                            uiCanvas.Children.Remove(_pieces[i, j]);
                        }
                    }
                }

                cropImage();
                Debug.WriteLine(_source);
                Shuffle();
                
                minutes = 2;
                seconds = 59;
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isPlaying)
                {
                    var write = new StreamWriter(FileName);
                    //Dòng đầu tiên là source của hình gốc
                    write.WriteLine(dirImgRoot);
                    //Dòng thứ hai là thời gian còn lại
                    write.WriteLine($"{minutes}:{seconds}");

                    //Theo sau la ma tran bieu dien game
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            write.Write($"{_a[i, j]}");
                            if (j != (Columns - 1))
                            {
                                write.Write(" ");
                            }
                        }
                        write.WriteLine("");
                    }

                    write.Close();
                    MessageBox.Show("Game is saved!");
                }
                else
                {
                    MessageBox.Show("Game can't save! Please Play^^");
                }   
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void LoadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reader = new StreamReader(FileName);
                var firstLine = reader.ReadLine();
                var pieces = new Image[Rows, Columns];
                loadImage(firstLine);
                cropImage();

                var secondLine = reader.ReadLine().Split(
                        new string[] { ":" }, StringSplitOptions.None);
                minutes = int.Parse(secondLine[0]);
                seconds = int.Parse(secondLine[1]);
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();

                for (int i = 0; i < Rows; i++)
                {
                    var tokens = reader.ReadLine().Split(
                        new string[] { " " }, StringSplitOptions.None);
                    for (int j = 0; j < Columns; j++)
                    {
                        _a[i, j] = int.Parse(tokens[j]);
                        if (_a[i, j] != 0)
                        {
                            var x = (_a[i, j] - 1) / Columns;
                            var y = (_a[i, j] - 1) % Columns;

                            Canvas.SetLeft(_pieces[x, y], LeftPadding + j * (SideHeight + 2));
                            Canvas.SetTop(_pieces[x, y], TopPadding + i * (SideHeight + 2));

                            pieces[i, j] = _pieces[x, y];
                            _pieces[x, y].Tag = new Tuple<int, int>(i, j);
                        }
                    }
                }
                _pieces = pieces;
                _isPlaying = true;
                reader.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (_isPlaying == true)
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (_pieces[i, j] == null)
                            {
                                if (e.Key == Key.Up)
                                {
                                    if (i + 1 < Rows)
                                    {
                                        _selectedImage = _pieces[i + 1, j];
                                        swapToSelectedItem(new Tuple<int, int>(i, j));
                                        return;
                                    }
                                }
                                if (e.Key == Key.Down)
                                {
                                    if (i - 1 >= 0)
                                    {
                                        _selectedImage = _pieces[i - 1, j];
                                        swapToSelectedItem(new Tuple<int, int>(i, j));
                                        return;
                                    }
                                }
                                if (e.Key == Key.Left)
                                {
                                    if (j + 1 < Columns)
                                    {
                                        _selectedImage = _pieces[i, j + 1];
                                        swapToSelectedItem(new Tuple<int, int>(i, j));
                                        return;
                                    }
                                }
                                if (e.Key == Key.Right)
                                {
                                    if (j - 1 >= 0)
                                    {
                                        _selectedImage = _pieces[i, j - 1];
                                        swapToSelectedItem(new Tuple<int, int>(i, j));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isPlaying == true)
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (_pieces[i, j] == null)
                            {
                                if (j + 1 < Columns)
                                {
                                    _selectedImage = _pieces[i, j + 1];
                                    swapToSelectedItem(new Tuple<int, int>(i, j));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isPlaying == true)
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (_pieces[i, j] == null)
                            {
                                if (j - 1 >= 0)
                                {
                                    _selectedImage = _pieces[i, j - 1];
                                    swapToSelectedItem(new Tuple<int, int>(i, j));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isPlaying == true)
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (_pieces[i, j] == null)
                            {
                                if (i + 1 < Rows)
                                {
                                    _selectedImage = _pieces[i + 1, j];
                                    swapToSelectedItem(new Tuple<int, int>(i, j));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isPlaying == true)
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (_pieces[i, j] == null)
                            {
                                if (i - 1 >= 0)
                                {
                                    _selectedImage = _pieces[i - 1, j];
                                    swapToSelectedItem(new Tuple<int, int>(i, j));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
