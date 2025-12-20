using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Text;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class KeyboardTesterToolViewModel : ToolViewModel
    {
        public override int Id => 34;
        public override string Name => ToolkitLocalization.GetString("Tool_KeyboardTester_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_KeyboardTester_Description");
        public override string IconKey => "KeyboardIcon";

        private bool _isListening;
        public bool IsListening
        {
            get => _isListening;
            set
            {
                if (SetProperty(ref _isListening, value))
                {
                    (StopListeningCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (StartListeningCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        private KeyEventModel _currentKey;
        public KeyEventModel CurrentKey
        {
            get => _currentKey;
            set => SetProperty(ref _currentKey, value);
        }

        private ObservableCollection<KeyEventModel> _keyEvents;
        public ObservableCollection<KeyEventModel> KeyEvents
        {
            get => _keyEvents;
            set => SetProperty(ref _keyEvents, value);
        }

        // Stats
        private int _totalKeys;
        public int TotalKeys { get => _totalKeys; set => SetProperty(ref _totalKeys, value); }

        private int _uniqueKeys;
        public int UniqueKeys { get => _uniqueKeys; set => SetProperty(ref _uniqueKeys, value); }

        private double _averageSpeed;
        public double AverageSpeed { get => _averageSpeed; set => SetProperty(ref _averageSpeed, value); }

        private string _mostPressed;
        public string MostPressed { get => _mostPressed; set => SetProperty(ref _mostPressed, value); }

        // Typing Test
        private string _typingTestText;
        public string TypingTestText
        {
            get => _typingTestText;
            set => SetProperty(ref _typingTestText, value);
        }

        private string _typingInput;
        public string TypingInput
        {
            get => _typingInput;
            set
            {
                if (SetProperty(ref _typingInput, value))
                {
                    ProcessTypingInput();
                }
            }
        }

        private bool _isTypingTestActive;
        public bool IsTypingTestActive
        {
            get => _isTypingTestActive;
            set
            {
                if (SetProperty(ref _isTypingTestActive, value))
                {
                    (StartTypingTestCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        private double _wpm;
        public double Wpm { get => _wpm; set => SetProperty(ref _wpm, value); }

        private double _accuracy;
        public double Accuracy { get => _accuracy; set => SetProperty(ref _accuracy, value); }

        private long _testStartTime;
        private long _sessionStartTime;

        // Continuous mode
        private bool _isContinuousMode;
        public bool IsContinuousMode
        {
            get => _isContinuousMode;
            set => SetProperty(ref _isContinuousMode, value);
        }

        private int _sentencesCompleted;
        public int SentencesCompleted
        {
            get => _sentencesCompleted;
            set => SetProperty(ref _sentencesCompleted, value);
        }

        private double _sessionWpm;
        public double SessionWpm { get => _sessionWpm; set => SetProperty(ref _sessionWpm, value); }

        private double _sessionAccuracy;
        public double SessionAccuracy { get => _sessionAccuracy; set => SetProperty(ref _sessionAccuracy, value); }

        private int _totalCharactersTyped;
        private int _totalCorrectCharacters;
        private int _currentSentenceIndex;
        private readonly Random _random = new Random();

        private int _wrongWords;
        public int WrongWords { get => _wrongWords; set => SetProperty(ref _wrongWords, value); }

        private int _sessionWrongWords;
        public int SessionWrongWords { get => _sessionWrongWords; set => SetProperty(ref _sessionWrongWords, value); }

        private readonly string[] _defaultTexts = new[]
        {
            "The quick brown fox jumps over the lazy dog.",
            "Pack my box with five dozen liquor jugs.",
            "Waltz, bad nymph, for quick jigs vex.",
            "How vexingly quick daft zebras jump!",
            "The five boxing wizards jump quickly."
        };

        private readonly string[] _continuousSentences = new[]
        {
            "The sun set behind the mountains, casting long shadows across the valley.",
            "She opened the old wooden box and found a collection of faded photographs from her grandmother's youth.",
            "The coffee shop on the corner serves the best espresso in the entire neighborhood.",
            "After months of hard work, the team finally completed the project ahead of schedule.",
            "The ancient oak tree in the park has stood there for over three hundred years.",
            "Learning a new language requires patience, dedication, and consistent daily practice to achieve fluency.",
            "The musician played a haunting melody that echoed through the empty concert hall, touching the hearts of everyone present.",
            "Storm clouds gathered on the horizon as the fishermen hurried to bring their boats back to the safety of the harbor before nightfall.",
            "The children laughed and played in the garden while their parents prepared a delicious barbecue dinner.",
            "Technology has transformed the way we communicate, work, and interact with the world around us in profound and unexpected ways.",
            "The old lighthouse keeper climbed the spiral staircase every evening to light the beacon that guided ships safely through the treacherous rocky coastline.",
            "Fresh bread from the bakery filled the kitchen with a warm, inviting aroma that reminded her of childhood mornings.",
            "The detective examined the crime scene carefully, looking for any clues that might help solve the mysterious disappearance.",
            "Mountains rise majestically in the distance, their snow-capped peaks glistening in the morning sunlight.",
            "The library was quiet except for the soft rustling of pages being turned and the occasional whisper between students.",
            "Scientists have discovered a new species of deep-sea fish that produces its own light in the dark ocean depths.",
            "The marathon runner crossed the finish line exhausted but triumphant, having achieved her personal best time despite the challenging weather conditions and steep hills.",
            "Traveling to different countries opens your mind to new cultures, cuisines, and perspectives that you would never experience by staying at home.",
            "The garden was full of colorful flowers, buzzing bees, and delicate butterflies dancing from bloom to bloom in the warm afternoon breeze.",
            "Writing code requires logical thinking, attention to detail, and the ability to break complex problems into smaller, manageable steps that can be solved one at a time."
        };

        public ObservableCollection<SentenceItem> AvailableTexts { get; }

        private SentenceItem _selectedText;
        public SentenceItem SelectedText
        {
            get => _selectedText;
            set
            {
                if (SetProperty(ref _selectedText, value))
                {
                    if (value != null)
                    {
                        TypingTestText = value.FullText;
                    }
                    ResetTypingTest();
                }
            }
        }

        public ICommand StartListeningCommand { get; }
        public ICommand StopListeningCommand { get; }
        public ICommand ClearEventsCommand { get; }
        public ICommand StartTypingTestCommand { get; }
        public ICommand ResetTypingTestCommand { get; }

        // Track pending KeyDown to enrich with TextInput character
        private KeyEventModel _pendingKeyDown;
        private string _lastTextInput;

        public KeyboardTesterToolViewModel()
        {
            KeyEvents = new ObservableCollection<KeyEventModel>();
            AvailableTexts = new ObservableCollection<SentenceItem>(
                _continuousSentences.Select(s => new SentenceItem(s)));
            SelectedText = AvailableTexts[0];

            StartListeningCommand = new RelayCommand(() => IsListening = true, () => !IsListening);
            StopListeningCommand = new RelayCommand(() => IsListening = false, () => IsListening);
            ClearEventsCommand = new RelayCommand(ClearEvents);
            StartTypingTestCommand = new RelayCommand(StartTypingTest, () => !IsTypingTestActive);
            ResetTypingTestCommand = new RelayCommand(ResetTypingTest);
        }

        public void HandleKeyEvent(global::Avalonia.Input.KeyEventArgs e, string type)
        {
            if (!IsListening) return;

            // Only log KeyDown events to avoid duplicate/confusing entries
            if (type == "KeyUp")
            {
                CurrentKey = null;
                _lastTextInput = null;
                return;
            }

            // Use KeySymbol if available (gives actual character), otherwise fall back to friendly name
            // Note: KeySymbol may be null/empty on Windows, TextInput will update it later
            string keyName;
            if (!string.IsNullOrEmpty(e.KeySymbol))
            {
                keyName = e.KeySymbol;
            }
            else
            {
                keyName = GetFriendlyKeyName(e.Key);
            }

            var keyEvent = new KeyEventModel
            {
                Key = keyName,
                Code = (int)e.Key,
                Modifiers = e.KeyModifiers.ToString(),
                Timestamp = DateTime.Now,
                Type = "Press"
            };

            // Always set pending so TextInput can update with actual character if needed
            _pendingKeyDown = keyEvent;
            CurrentKey = keyEvent;
            KeyEvents.Insert(0, keyEvent);
            if (KeyEvents.Count > 100) KeyEvents.RemoveAt(KeyEvents.Count - 1);
            CalculateStats();
        }

        public void HandleTextInput(string text)
        {
            if (!IsListening || string.IsNullOrEmpty(text)) return;

            _lastTextInput = text;

            // Update the pending KeyDown event with the actual character
            if (_pendingKeyDown != null && KeyEvents.Count > 0)
            {
                // Find the pending KeyDown in the list and update it
                int idx = KeyEvents.IndexOf(_pendingKeyDown);
                if (idx >= 0)
                {
                    // Create updated event with the actual character
                    var updated = new KeyEventModel
                    {
                        Key = text,
                        Code = _pendingKeyDown.Code,
                        Modifiers = _pendingKeyDown.Modifiers,
                        Timestamp = _pendingKeyDown.Timestamp,
                        Type = _pendingKeyDown.Type
                    };
                    // Replace to trigger UI update
                    KeyEvents[idx] = updated;
                }
                _pendingKeyDown = null;
            }

            // Update current key display with the actual typed character
            var keyEvent = new KeyEventModel
            {
                Key = text,
                Code = text.Length > 0 ? (int)text[0] : 0,
                Modifiers = "None",
                Timestamp = DateTime.Now,
                Type = "Char"
            };

            CurrentKey = keyEvent;
        }

        private string GetFriendlyKeyName(global::Avalonia.Input.Key key)
        {
            // This is only used as fallback when KeySymbol is not available (special keys)
            switch (key)
            {
                case global::Avalonia.Input.Key.Space: return "Space";
                case global::Avalonia.Input.Key.Return: return "Enter";
                case global::Avalonia.Input.Key.Tab: return "Tab";
                case global::Avalonia.Input.Key.Escape: return "Escape";
                case global::Avalonia.Input.Key.Back: return "Backspace";
                case global::Avalonia.Input.Key.Delete: return "Delete";
                case global::Avalonia.Input.Key.Left: return "←";
                case global::Avalonia.Input.Key.Right: return "→";
                case global::Avalonia.Input.Key.Up: return "↑";
                case global::Avalonia.Input.Key.Down: return "↓";
                case global::Avalonia.Input.Key.Home: return "Home";
                case global::Avalonia.Input.Key.End: return "End";
                case global::Avalonia.Input.Key.PageUp: return "Page Up";
                case global::Avalonia.Input.Key.PageDown: return "Page Down";
                case global::Avalonia.Input.Key.Insert: return "Insert";
                case global::Avalonia.Input.Key.CapsLock: return "Caps Lock";
                case global::Avalonia.Input.Key.NumLock: return "Num Lock";
                case global::Avalonia.Input.Key.Scroll: return "Scroll Lock";
                case global::Avalonia.Input.Key.PrintScreen: return "Print Screen";
                case global::Avalonia.Input.Key.Pause: return "Pause";
                case global::Avalonia.Input.Key.LeftShift: return "Left Shift";
                case global::Avalonia.Input.Key.RightShift: return "Right Shift";
                case global::Avalonia.Input.Key.LeftCtrl: return "Left Ctrl";
                case global::Avalonia.Input.Key.RightCtrl: return "Right Ctrl";
                case global::Avalonia.Input.Key.LeftAlt: return "Left Alt";
                case global::Avalonia.Input.Key.RightAlt: return "Right Alt";
                case global::Avalonia.Input.Key.LWin: return "Left Win";
                case global::Avalonia.Input.Key.RWin: return "Right Win";
                default:
                    var keyStr = key.ToString();
                    // For Oem keys, show placeholder - TextInput will update with actual char
                    if (keyStr.StartsWith("Oem"))
                    {
                        return "…"; // Placeholder, will be replaced by TextInput
                    }
                    return keyStr;
            }
        }

        private void CalculateStats()
        {
            var downEvents = KeyEvents.Where(k => k.Type == "Press").ToList();
            TotalKeys = downEvents.Count;
            UniqueKeys = downEvents.Select(k => k.Key).Distinct().Count();

            if (TotalKeys > 0)
            {
                var grouped = downEvents.GroupBy(k => k.Key).OrderByDescending(g => g.Count()).FirstOrDefault();
                MostPressed = grouped?.Key ?? "";

                // Simple WPM approximation from recent keys
                // (This is distinct from the Typing Test WPM)
            }
        }

        private void ClearEvents()
        {
            KeyEvents.Clear();
            CurrentKey = null;
            TotalKeys = 0;
            UniqueKeys = 0;
            MostPressed = "";
        }

        private void StartTypingTest()
        {
            IsTypingTestActive = true;
            TypingInput = "";
            Wpm = 0;
            Accuracy = 0;
            WrongWords = 0;
            _testStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (IsContinuousMode)
            {
                _sessionStartTime = _testStartTime;
                SentencesCompleted = 0;
                SessionWpm = 0;
                SessionAccuracy = 0;
                SessionWrongWords = 0;
                _totalCharactersTyped = 0;
                _totalCorrectCharacters = 0;
                // Keep the user's selected sentence, use the combobox index
                _currentSentenceIndex = SelectedText != null ? AvailableTexts.IndexOf(SelectedText) : 0;
                if (_currentSentenceIndex < 0)
                {
                    _currentSentenceIndex = 0;
                }
            }
        }

        private void ResetTypingTest()
        {
            IsTypingTestActive = false;
            TypingInput = "";
            Wpm = 0;
            Accuracy = 0;
            WrongWords = 0;
            SentencesCompleted = 0;
            SessionWpm = 0;
            SessionAccuracy = 0;
            SessionWrongWords = 0;
            _totalCharactersTyped = 0;
            _totalCorrectCharacters = 0;
        }

        private void ProcessTypingInput()
        {
            if (!IsTypingTestActive) return;

            string target = TypingTestText;
            string current = TypingInput ?? "";

            int charErrors = 0;
            for (int i = 0; i < current.Length; i++)
            {
                if (i >= target.Length || current[i] != target[i])
                {
                    charErrors++;
                }
            }

            // Calculate wrong words by comparing word by word
            var targetWords = target.Split(' ');
            var currentWords = current.Split(' ');
            int wrongWordCount = 0;
            for (int i = 0; i < currentWords.Length; i++)
            {
                if (i >= targetWords.Length || currentWords[i] != targetWords[i])
                {
                    wrongWordCount++;
                }
            }
            WrongWords = wrongWordCount;

            // Calculate stats
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            double minutes = (now - _testStartTime) / 1000.0 / 60.0;

            if (minutes > 0)
            {
                Wpm = Math.Round((current.Length / 5.0) / minutes);
            }

            if (current.Length > 0)
            {
                Accuracy = Math.Round(((double)(current.Length - charErrors) / current.Length) * 100);
            }

            // Sentence is complete when user has typed at least as many characters as target
            bool sentenceComplete = current.Length >= target.Length;

            if (sentenceComplete)
            {
                if (IsContinuousMode)
                {
                    // Update session totals
                    _totalCharactersTyped += target.Length;
                    _totalCorrectCharacters += target.Length - charErrors;
                    SessionWrongWords += wrongWordCount;
                    SentencesCompleted++;

                    // Calculate session stats
                    double sessionMinutes = (now - _sessionStartTime) / 1000.0 / 60.0;
                    if (sessionMinutes > 0)
                    {
                        SessionWpm = Math.Round((_totalCharactersTyped / 5.0) / sessionMinutes);
                    }
                    if (_totalCharactersTyped > 0)
                    {
                        SessionAccuracy = Math.Round(((double)_totalCorrectCharacters / _totalCharactersTyped) * 100);
                    }

                    // Check if all sentences completed
                    if (SentencesCompleted >= _continuousSentences.Length)
                    {
                        IsTypingTestActive = false;
                        ClearTypingInput();
                    }
                    else
                    {
                        // Move to next sentence (random, avoiding repeat)
                        int nextIndex;
                        do
                        {
                            nextIndex = _random.Next(_continuousSentences.Length);
                        } while (nextIndex == _currentSentenceIndex && _continuousSentences.Length > 1);

                        _currentSentenceIndex = nextIndex;
                        TypingTestText = _continuousSentences[_currentSentenceIndex];
                        ClearTypingInput();
                        Wpm = 0;
                        Accuracy = 0;
                        WrongWords = 0;
                        _testStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                }
                else
                {
                    IsTypingTestActive = false;
                }
            }
        }

        private void ClearTypingInput()
        {
            // Post to UI thread to avoid reentrancy issues with TextBox binding
            Dispatcher.UIThread.Post(() =>
            {
                _typingInput = "";
                OnPropertyChanged(nameof(TypingInput));
            }, DispatcherPriority.Send);
        }
    }

    public class KeyEventModel
    {
        public string Key { get; set; }
        public int Code { get; set; }
        public string Modifiers { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } // KeyDown, KeyUp
    }

    public class SentenceItem
    {
        public string FullText { get; }
        public string DisplayText { get; }

        public SentenceItem(string text)
        {
            FullText = text;
            DisplayText = text.Length > 60 ? text.Substring(0, 57) + "..." : text;
        }

        public override string ToString() => DisplayText;
    }
}
