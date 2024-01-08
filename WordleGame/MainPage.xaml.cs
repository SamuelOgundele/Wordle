using System.Text.Json;


namespace WordleGame
{
    public partial class MainPage : ContentPage
    {
        private static String path;
        private static String fullPath;
        public static String playerFilePath;
        private static String word;
        private int tries = 0;
        public static string playerName = "test"; // = "test" To avoid exception on if (!string.IsNullOrWhiteSpace(playerName)) check in Stats Page
        private static DateTime gameStartTime;
        private String gameResult;
        List<string> colorEmojis = new List<string>();
        // Ask for player name
        private async void AskForPlayerName()
        {
            // Check if the player file already exists
            if (!File.Exists(playerFilePath))
            {
                // Prompt the user to enter a player name
                playerName = await DisplayPromptAsync("Username", "Enter your Username", maxLength: 50);
                // Check user input
                if (!string.IsNullOrWhiteSpace(playerName))
                {
                    // Create a player file if it doesn't exist
                    await CreatePlayerFile();
                }
                else
                {
                    await CreatePlayerFile();
                    // If no player name is entered, show an alert and close the app
                    await DisplayAlert("Error", "Username is required!", "OK");
                    CloseApp();
                }
            }
        }
        // Create player file
        private async Task CreatePlayerFile()
        {
            try
            {
                // Combine the directory path and player name to get the full file path
                playerFilePath = Path.Combine(path, $"{playerName}.txt");
                // Check if the player file already exists
                if (!File.Exists(playerFilePath))
                {
                    // Create a new player file
                    using (StreamWriter writer = File.CreateText(playerFilePath)) { }
                }
            }
            catch (Exception ex)
            {
                // Exception
                Console.WriteLine($"Error creating player file: {ex.Message}");
            }
        }
        //Write data to the player file
        private async Task WriteStats()
        {
            while (colorEmojis.Count < 30)
            {
                colorEmojis.Add("\ud83c\udf2b\ufe0f"); // Append gray square emoji
            }
            try
            {
                // Object for game stats
                var gameStats = new Stats
                {
                    Timestamp = gameStartTime,
                    CorrectWord = word.ToUpper(),
                    NumberOfTries = tries,
                    GameResult = gameResult,
                    Colors = colorEmojis
                };
                // Serialize to JSON
                string jsonStats = JsonSerializer.Serialize(gameStats);
                // Write the JSON data to the file
                string playerFilePath = Path.Combine(path, $"{playerName}.txt");
                await File.AppendAllTextAsync(playerFilePath, jsonStats + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Exception
                Console.WriteLine($"Error writing game stats: {ex.Message}");
            }
        }
        // Close the app
        public void CloseApp()
        {
            // Close the app
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        // Frame animation
        private async Task RotateFrame(Frame frame)
        {
            await frame.RotateXTo(360, 750); // Rotate the frame
            frame.RotationX = 0; // Reset the rotation after the animation
        }
        // Method to count letters. Return dict(letter - key, number of letters - value)
        static Dictionary<char, int> CountIdenticalLetters(string input)
        {
            Dictionary<char, int> letterCounts = new Dictionary<char, int>();
            foreach (char c in input)
            {
                if (char.IsLetter(c))
                {
                    if (letterCounts.ContainsKey(c))
                    {
                        letterCounts[c]++;
                    }
                    else
                    {
                        letterCounts[c] = 1;
                    }
                }
            }
            return letterCounts;
        }
        // Method to check user input
        string CheckUserInput(string str)
        {
            str = str.ToLower();
            if (IsWordInFile(fullPath, str))
            {
                return str;
            }
            else
            {
                DisplayAlert("Check your input", "The word is not on the list", "OK");
                return "";
            }
        }
        // Method to check if word is in the file
        bool IsWordInFile(string filePath, string userInput)
        {
            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);
            // Check if any line is equal to the specified word
            return lines.Any(line => line.Equals(userInput));
        }
        // Method to get random word from the file
        static string GetRandomWord(string filePath)
        {
            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);
            // Random number generator to select a word
            Random random = new Random();
            int randomIndex = random.Next(0, lines.Length);
            string randomLine = lines[randomIndex];
            // Split the "line" into words
            string[] word = randomLine.Split(' ');
            return word[0];
        }

        // Method to reset the game
        private void ResetGame()
        {
            // Clear emoji list
            colorEmojis.Clear();
            // Reset word and tries
            word = null;
            tries = 0;
            // Clear the entered text
            UserGuess.Text = "";
            // Reset labels and backgrounds in the grid
            foreach (Frame frame in WordleGrid.Children.OfType<Frame>())
            {
                frame.BackgroundColor = Colors.Black;
                frame.BorderColor = Color.FromArgb("#3A3A3C");
                frame.RotationX = 0;
                Label label = frame.Content as Label;
                if (label != null)
                {
                    label.Text = "";
                }
            }
            // Disable entry field
            UserGuess.IsVisible = false;
            UserGuess.IsEnabled = false;
            // Disable enter button
            GuessButton.IsVisible = false;
            GuessButton.IsEnabled = false;
            // Enable new wordle button
            StartButton.IsVisible = true;
            StartButton.IsEnabled = true;
            // Disable reset button
            ResetButton.IsVisible = false;
            ResetButton.IsEnabled = false;
            // Test code
            //test.Text = "";
        }
        // Main
        public MainPage()
        {
            Application.Current.UserAppTheme = AppTheme.Dark;

            // Word file path
            path = FileSystem.Current.AppDataDirectory;
            fullPath = Path.Combine(path, "words.txt");
            InitializeComponent();
            // Download word list
            WordList download = new WordList();
        }
        // New Wordle btn
        private void Start_OnClicked(object sender, EventArgs e)
        {
            AskForPlayerName();
            // Gets TimeStamp
            gameStartTime = DateTime.Now;
            // Enable entry field 
            UserGuess.IsVisible = true;
            UserGuess.IsEnabled = true;
            // Enable Guess button
            GuessButton.IsVisible = true;
            GuessButton.IsEnabled = true;
            // Disable Start button
            StartButton.IsVisible = false;
            StartButton.IsEnabled = false;
            // Enable reset button
            ResetButton.IsVisible = true;
            ResetButton.IsEnabled = true;
            // Gets Random Word
            word = GetRandomWord(fullPath);

        }
        // Enter btn
        private async void GuessButton_OnClicked(object sender, EventArgs e)
        {
            // Disable btns
            GuessButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            // Array to keep track of correct letters
            bool[] correctLetters = new bool[word.Length];
            // Dictionaries to keep track of letters
            Dictionary<char, int> letterCounts = CountIdenticalLetters(word);
            Dictionary<char, int> userInputLetterCounts = new Dictionary<char, int>();
            // User input + validation
            if (!string.IsNullOrEmpty(UserGuess.Text))
            {
                string userInput = UserGuess.Text.ToLower();
                if (CheckUserInput(userInput) != "")
                {
                    // Keeps track of remaining attempts
                    tries++;
                    // Identify correct letters
                    for (int i = 0; i < word.Length; i++)
                    {
                        char lui = userInput[i]; // Get a letter from user input
                        char lw = word[i]; // Get a letter from the word
                        if (lui == lw)
                        {
                            correctLetters[i] = true;
                            letterCounts[lw]--; // Decrement for the used letter
                        }
                    }
                    // Loop through each row
                    for (int row = tries - 1; row < tries; row++)
                    {
                        // Loop through each column
                        for (int column = 0; column < WordleGrid.ColumnDefinitions.Count; column++)
                        {
                            // Get the Frame at the current row and column
                            Frame frame = WordleGrid.Children.OfType<Frame>().FirstOrDefault(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == column);
                            if (frame != null)
                            {
                                // Access the Label inside the Frame
                                Label label = frame.Content as Label;
                                if (label != null)
                                {
                                    char lui = userInput[column]; // Get a letter from user input
                                    char lw = word[column]; // Get a letter from the word
                                    label.Text = lui.ToString().ToUpper();
                                    // Change background color based on the letter
                                    if (correctLetters[column])
                                    {
                                        frame.BackgroundColor = Color.FromRgb(83, 141, 78); // Green
                                        frame.BorderColor = Color.FromRgb(83, 141, 78);
                                        colorEmojis.Add("\ud83d\udfe9");
                                        await RotateFrame(frame);
                                    }
                                    else if (word.Contains(lui.ToString()) && letterCounts[lui] > 0)
                                    {
                                        // Check if the letter is used in the user input
                                        if (userInputLetterCounts.ContainsKey(lui) && userInputLetterCounts[lui] > 0)
                                        {
                                            // If the letter is already used, color it gray
                                            frame.BackgroundColor = Color.FromRgb(58, 58, 60); // Gray
                                            colorEmojis.Add("\ud83c\udf2b\ufe0f");
                                            await RotateFrame(frame);
                                        }
                                        else
                                        {
                                            frame.BackgroundColor = Color.FromRgb(181, 159, 59); // Yellow
                                            frame.BorderColor = Color.FromRgb(181, 159, 59);
                                            colorEmojis.Add("\ud83d\udfe8");
                                            // Increment the count for the used letter in user input
                                            userInputLetterCounts[lui] = 1;
                                            await RotateFrame(frame);
                                        }
                                    }
                                    else
                                    {
                                        // Else color the background gray
                                        frame.BackgroundColor = Color.FromRgb(58, 58, 60); // Gray
                                        colorEmojis.Add("\ud83c\udf2b\ufe0f");
                                        await RotateFrame(frame);
                                    }
                                }
                            }
                        }
                    }
                    // Break the loop if the word is correct. Reset the game
                    if (correctLetters.All(x => x))
                    {
                        await DisplayAlert("Correct!", "The word is: " + word.ToUpper() + "\nNumber of tries: " + tries, "OK");
                        gameResult = "Winner";
                        await WriteStats();
                        // Disable entry field
                        UserGuess.IsVisible = false;
                        UserGuess.IsEnabled = false;
                        // Disable enter button
                        GuessButton.IsVisible = false;
                        GuessButton.IsEnabled = false;
                    }
                    // Break the loop if the maximum number of tries is reached
                    else if (tries == 6)
                    {
                        gameResult = "Loser";
                        await WriteStats();
                        // Disable enter button
                        GuessButton.IsVisible = false;
                        GuessButton.IsEnabled = false;
                        // Disable entry field
                        UserGuess.IsVisible = false;
                        UserGuess.IsEnabled = false;
                        await DisplayAlert("Wrong!", "The word is: " + word.ToUpper() + "\n", "OK");
                    }
                }
            }
            // Enable buttons
            GuessButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
        }
        // Reset btn
        private void ResetButton_OnClicked(object sender, EventArgs e)
        {
            // Resets the game
            ResetGame();
        }
    }
}