using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static System.Formats.Asn1.AsnWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };
        private static string foodValue = Environment.GetEnvironmentVariable("CustomVar1");
        private string name;
        private string moveType = "WASD";
        private static string userId;
        private double sizeSliderValue;
        private double speedValueHolder;
        private int speedValue = 100;
        private int rows = 15, cols = 15;
        private Image[,] gridImages;
        private GameState gameState;
        private AudioPlayer _audioPlayer;
        private bool gameRunning;
        private bool isPaused = false;
        private bool nameEntered = false;
        
        public MainWindow()
        {
            InitializeComponent();
            UniqueID();
            LoadLeaderboard();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols, foodValue);
            _audioPlayer = new AudioPlayer();
        }

        static void UniqueID()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "SnakeGame");
            string filePath = Path.Combine(folderPath, "user_id.txt");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(filePath))
            {
                userId = File.ReadAllText(filePath);
            }
            else
            {
                userId = GenerateUniqueId(9);
                File.WriteAllText(filePath, userId);
            }

            Console.WriteLine($"User ID: {userId}");
        }

        static string GenerateUniqueId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async void LoadLeaderboard()
        {
            List<LeaderboardEntry> scores = await FirebaseLeaderboard.GetLeaderboardAsync();
            scores.Sort((a, b) => b.Score.CompareTo(a.Score));
            LeaderboardList.ItemsSource = scores;
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            ToggleUI(false);
            _audioPlayer.PlayAudio("Mine.mp3");
            await GameLoop();
            await ShowGameOver();
            ToggleUI(true);
            gameState = new GameState(rows, cols, foodValue);
            _audioPlayer.StopAudio();
        }

        private void GetName()
        {
            name = User.Text;
        }

        private void ToggleUI(bool toggle)
        {
            if (toggle)
            {
                sizeSlider.Visibility = Visibility.Visible;
                Confirm.Visibility = Visibility.Visible;
                valueDisplay.Visibility = Visibility.Visible;
                speedSlider.Visibility = Visibility.Visible;
                speedConfirm.Visibility = Visibility.Visible;
                speedDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                sizeSlider.Visibility = Visibility.Hidden;
                Confirm.Visibility = Visibility.Hidden;
                valueDisplay.Visibility = Visibility.Hidden;
                speedSlider.Visibility = Visibility.Hidden;
                speedConfirm.Visibility = Visibility.Hidden;
                speedDisplay.Visibility = Visibility.Hidden;
            }
        }

        private void TogglePause()
        {
            if (gameState.GameOver) return;

            isPaused = !isPaused;

            if (isPaused)
            {
                _audioPlayer.Pause();
                ToggleUI(true);
                Overlay.Visibility = Visibility.Visible;
                OverlayText.Text = "PAUSED";
                ResumeButton.Visibility = Visibility.Visible;
                OptionsButton.Visibility = Visibility.Visible;
                RestartButton.Visibility = Visibility.Visible;
            }
            else
            {
                _audioPlayer.Resume();
                ToggleUI(false);
                Overlay.Visibility = Visibility.Hidden;
                ResumeButton.Visibility = Visibility.Collapsed;
                OptionsButton.Visibility = Visibility.Collapsed;
                RestartButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible && nameEntered)
            {
                e.Handled = true;
            }

            if (!gameRunning && nameEntered)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver) return;

            if (e.Key == Key.P)
            {
                TogglePause();
                return;
            }

            if (isPaused) return; // Ignore other inputs when paused.

            if (moveType == "WASD")
            {
                switch (e.Key)
                {
                    case Key.A:
                        gameState.ChangeDirection(Direction.Left);
                        break;
                    case Key.D:
                        gameState.ChangeDirection(Direction.Right);
                        break;
                    case Key.W:
                        gameState.ChangeDirection(Direction.Up);
                        break;
                    case Key.S:
                        gameState.ChangeDirection(Direction.Down);
                        break;
                }
            }
            else if (moveType == "Arrows")
            {
                switch (e.Key)
                {
                    case Key.Left:
                        gameState.ChangeDirection(Direction.Left);
                        break;
                    case Key.Right:
                        gameState.ChangeDirection(Direction.Right);
                        break;
                    case Key.Up:
                        gameState.ChangeDirection(Direction.Up);
                        break;
                    case Key.Down:
                        gameState.ChangeDirection(Direction.Down);
                        break;
                }
            }
            
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                if (isPaused)
                {
                    await Task.Delay(100); // Prevent tight loop while paused.
                    continue;
                }

                await Task.Delay((int)speedValue);
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                if (positions.Count < 20)
                    await Task.Delay(50);
                else if (positions.Count > 20 && positions.Count < 40)
                    await Task.Delay(25);
                else if (positions.Count > 40)
                    await Task.Delay(5);
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            await Task.Delay(100);
            sizeSliderValue = (int)e.NewValue;
            valueDisplay.Text = $"Size: {sizeSliderValue.ToString()}";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            rows = (int)sizeSliderValue; cols = (int)sizeSliderValue;
            await Task.Delay(1000);
            GameGrid.Children.Clear();
            gameState = new GameState(rows, cols, foodValue);
            gridImages = SetupGrid();
            Draw();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void ToggleOptions(bool toggle)
        {
            if (toggle)
            {
                ToggleUI(false);
                OverlayText.Text = "Options";
                ResumeButton.Visibility = Visibility.Collapsed;
                OptionsButton.Visibility = Visibility.Collapsed;
                RestartButton.Visibility = Visibility.Collapsed;
                MovementType.Visibility = Visibility.Visible;
                MovementText.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Visible;
            }
            else
            {
                ToggleUI(true);
                OverlayText.Text = "Options";
                ResumeButton.Visibility = Visibility.Visible;
                OptionsButton.Visibility = Visibility.Visible;
                RestartButton.Visibility = Visibility.Visible;
                MovementType.Visibility = Visibility.Collapsed;
                MovementText.Visibility = Visibility.Collapsed;
                BackButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            ToggleOptions(true);
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            GameGrid.Children.Clear();
            gameState = new GameState(rows, cols, foodValue);
            gridImages = SetupGrid();
            Draw();
        }

        private async void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            await Task.Delay(100);
            if (e.NewValue == 0.5)
            {
                speedValueHolder = 200;
            }
            else if (e.NewValue == 1)
            {
                speedValueHolder = 100;
            }
            else if (e.NewValue == 2)
            {
                speedValueHolder = 50;
            }
            else if (e.NewValue == 4)
            {
                speedValueHolder = 25;
            }
            else if (e.NewValue == 8)
            {
                speedValueHolder = 12;
            }
            speedDisplay.Text = $"Speed: {e.NewValue.ToString()}x";
        }

        private void speedConfirm_Click(object sender, RoutedEventArgs e)
        {
            speedValue = (int)speedValueHolder;
        }

        private void MovementType_Click(object sender, RoutedEventArgs e)
        {
            if (moveType == "WASD")
            {
                moveType = "Arrows";
                MovementType.Content = "Arrows";
            }
            else if (moveType == "Arrows")
            {
                moveType = "WASD";
                MovementType.Content = "WASD";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleOptions(false);
        }

        private void User_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetName();
                e.Handled = true;
                nameEntered = true;
                User.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ShowGameOver()
        {
            LeaderboardEntry newEntry = new LeaderboardEntry(userId, name, gameState.Score);
            await FirebaseLeaderboard.AddOrUpdateScoreAsync(newEntry);
            LoadLeaderboard();
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO START";
        }
    }
}