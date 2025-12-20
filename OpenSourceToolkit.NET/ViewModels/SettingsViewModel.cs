using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LlmTornado;
using LlmTornado.Code;
using OpenSourceToolkit.NET.Helpers;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.NET.Services;
using OpenSourceToolkit.NET.Services.Ai;

namespace OpenSourceToolkit.NET.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private const int MaxConnections = 50;

        /// <summary>
        /// Action to show exception details in a popup (DEBUG builds only).
        /// </summary>
        public Action<Exception> ShowDebugExceptionAction { get; set; }

        /// <summary>
        /// Action to prompt user to save/discard unsaved changes. Returns true if user wants to proceed (save or discard), false to cancel.
        /// Parameters: (message, callback with result: true=Save, false=Discard, null=Cancel)
        /// </summary>
        public Func<string, Task<bool?>> PromptSaveChangesAction { get; set; }

        #region General Settings

        private string _audioInputDeviceName;
        public string AudioInputDeviceName
        {
            get => _audioInputDeviceName;
            set
            {
                if (SetProperty(ref _audioInputDeviceName, value))
                    AppSettings.Current.AudioInputDeviceName = value;
            }
        }

        private string _audioExportFormat;
        public string AudioExportFormat
        {
            get => _audioExportFormat;
            set
            {
                if (SetProperty(ref _audioExportFormat, value))
                    AppSettings.Current.AudioExportFormat = value;
            }
        }

        private int _audioMp3Bitrate;
        public int AudioMp3Bitrate
        {
            get => _audioMp3Bitrate;
            set
            {
                if (SetProperty(ref _audioMp3Bitrate, value))
                    AppSettings.Current.AudioMp3Bitrate = value;
            }
        }

        private ThemeVariant _activeTheme = ThemeVariant.Dark;
        public ThemeVariant ActiveTheme
        {
            get => _activeTheme;
            private set => SetProperty(ref _activeTheme, value);
        }

        public string[] AudioFormats { get; } = new[] { "WAV", "MP3" };
        public int[] Mp3Bitrates { get; } = new[] { 128, 192, 256, 320 };

        private string _gitHubToken;
        public string GitHubToken
        {
            get => _gitHubToken;
            set
            {
                if (SetProperty(ref _gitHubToken, value))
                {
                    AppSettings.SetGitHubToken(value);
                    OnPropertyChanged(nameof(HasGitHubToken));
                }
            }
        }

        public bool HasGitHubToken => !string.IsNullOrEmpty(GitHubToken);

        // Locale selector for Semi theme
        public LocaleItem[] AvailableLocales { get; } = new[]
        {
            new LocaleItem("English (US)", "en-US"),
            new LocaleItem("English (UK)", "en-GB"),
            new LocaleItem("Deutsch", "de-DE"),
            new LocaleItem("Español", "es-ES"),
            new LocaleItem("Français", "fr-FR"),
            new LocaleItem("Italiano", "it-IT"),
            new LocaleItem("Nederlands", "nl-NL"),
            new LocaleItem("Polski", "pl-PL"),
            new LocaleItem("Русский", "ru-RU"),
            new LocaleItem("Українська", "uk-UA"),
            new LocaleItem("日本語", "ja-JP"),
            new LocaleItem("한국어", "ko-KR"),
            new LocaleItem("简体中文", "zh-CN"),
            new LocaleItem("繁體中文", "zh-TW"),
        };

        private LocaleItem _selectedLocale;
        public LocaleItem SelectedLocale
        {
            get => _selectedLocale;
            set
            {
                if (SetProperty(ref _selectedLocale, value) && value != null)
                {
                    AppSettings.Current.Locale = value.Code;
                    ApplyLocale(value.Code);
                    AppSettings.Save();
                }
            }
        }

        private static void ApplyLocale(string localeCode)
        {
            // Locale setting is stored for future use
        }

        // UI Language selector for ToolkitLocalization
        public LocaleItem[] AvailableLanguages { get; } = new[]
        {
            new LocaleItem("English", "en-US"),
            new LocaleItem("Deutsch", "de-DE"),
            new LocaleItem("Français", "fr-FR"),
            new LocaleItem("Español", "es-ES"),
            new LocaleItem("Italiano", "it-IT"),
            new LocaleItem("中文 (简体)", "zh-Hans"),
            new LocaleItem("한국어", "ko-KR"),
            new LocaleItem("日本語", "ja-JP"),
            new LocaleItem("العربية", "ar-SA"),
            new LocaleItem("Türkçe", "tr-TR"),
            new LocaleItem("Українська", "uk-UA"),
        };

        private LocaleItem _selectedLanguage;
        public LocaleItem SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value) && value != null)
                {
                    // Apply UI language change
                    ToolkitLocalization.SetCulture(value.Code);
                    
                    // Save to settings
                    AppSettings.Current.Language = value.Code;
                    AppSettings.Save();
                }
            }
        }

        /// <summary>
        /// Applies the saved locale setting. Call this on app startup.
        /// </summary>
        public static void ApplySavedLocale()
        {
            var locale = AppSettings.Current.Locale;
            if (string.IsNullOrEmpty(locale))
            {
                // First start - detect and save system locale
                locale = DetectSystemLocale();
                AppSettings.Current.Locale = locale;
                AppSettings.Save();
            }
            ApplyLocale(locale);
        }

        /// <summary>
        /// Detects the system locale and returns a matching supported locale code,
        /// or "en-US" if no match is found.
        /// </summary>
        private static string DetectSystemLocale()
        {
            var supportedLocales = new[]
            {
                "en-US", "en-GB", "de-DE", "es-ES", "fr-FR", "it-IT", "zh-Hans",
                "ko-KR", "ja-JP", "ar-SA", "tr-TR", "uk-UA"
            };

            try
            {
                var systemCulture = CultureInfo.CurrentUICulture;

                // Try exact match first (e.g., "de-DE")
                var exactMatch = supportedLocales.FirstOrDefault(l =>
                    l.Equals(systemCulture.Name, StringComparison.OrdinalIgnoreCase));
                if (exactMatch != null)
                    return exactMatch;

                // Try language-only match (e.g., "de" matches "de-DE")
                var langCode = systemCulture.TwoLetterISOLanguageName;
                var langMatch = supportedLocales.FirstOrDefault(l =>
                    l.StartsWith(langCode + "-", StringComparison.OrdinalIgnoreCase));
                if (langMatch != null)
                    return langMatch;
            }
            catch
            {
                // Ignore culture detection errors
            }

            return "en-US";
        }

        #endregion

        #region AI Connections

        public ObservableCollection<AiConnectionViewModel> Connections { get; } = new ObservableCollection<AiConnectionViewModel>();

        private AiConnectionViewModel _selectedConnection;
        private bool _isProcessingSelection;
        private bool _suppressSelectionChange;

        public AiConnectionViewModel SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                if (_suppressSelectionChange || _isProcessingSelection || value == _selectedConnection) return;
                _ = HandleConnectionSelectionAsync(value);
            }
        }

        private async Task HandleConnectionSelectionAsync(AiConnectionViewModel newSelection)
        {
            if (_isProcessingSelection) return;
            _isProcessingSelection = true;

            var previousSelection = _selectedConnection;

            try
            {
                if (HasUnsavedConnectionChanges && PromptSaveChangesAction != null)
                {
                    var result = await PromptSaveChangesAction("You have unsaved changes. Do you want to save them?");
                    if (result == null)
                    {
                        // Cancel - restore previous selection in UI
                        _suppressSelectionChange = true;
                        try
                        {
                            // Force ListBox to re-select by toggling through null
                            _selectedConnection = null;
                            OnPropertyChanged(nameof(SelectedConnection));
                            _selectedConnection = previousSelection;
                            OnPropertyChanged(nameof(SelectedConnection));
                        }
                        finally
                        {
                            _suppressSelectionChange = false;
                        }
                        return;
                    }
                    if (result == true)
                        SaveConnection();
                    else
                        ResetDirtyTracking(); // Discard - clear dirty state
                }

                _selectedConnection = newSelection;
                OnPropertyChanged(nameof(SelectedConnection));
                ((RelayCommand)EditConnectionCommand)?.NotifyCanExecuteChanged();
                ((RelayCommand)DeleteConnectionCommand)?.NotifyCanExecuteChanged();
                if (newSelection != null)
                    StartEditConnection();
            }
            finally
            {
                _isProcessingSelection = false;
            }
        }

        private bool _isEditingConnection;
        public bool IsEditingConnection
        {
            get => _isEditingConnection;
            set => SetProperty(ref _isEditingConnection, value);
        }

        private bool _isAddingConnection;
        public bool IsAddingConnection
        {
            get => _isAddingConnection;
            set => SetProperty(ref _isAddingConnection, value);
        }

        // Edit form fields
        private string _editConnectionName;
        public string EditConnectionName
        {
            get => _editConnectionName;
            set => SetProperty(ref _editConnectionName, value);
        }

        private string _editSelectedProvider;
        public string EditSelectedProvider
        {
            get => _editSelectedProvider;
            set
            {
                if (SetProperty(ref _editSelectedProvider, value))
                {
                    UpdateEditAvailableModels();
                    // Select first model by default
                    EditSelectedModel = EditAvailableModels?.FirstOrDefault();
                }
            }
        }

        private string _editSelectedModel;
        public string EditSelectedModel
        {
            get => _editSelectedModel;
            set => SetProperty(ref _editSelectedModel, value);
        }

        private bool _editShowCustomApiKey;
        public bool EditShowCustomApiKey
        {
            get => _editShowCustomApiKey;
            set => SetProperty(ref _editShowCustomApiKey, value);
        }

        private string _editCustomApiKey;
        public string EditCustomApiKey
        {
            get => _editCustomApiKey;
            set => SetProperty(ref _editCustomApiKey, value);
        }

        private int _editMaxTokens = 4096;
        public int EditMaxTokens
        {
            get => _editMaxTokens;
            set => SetProperty(ref _editMaxTokens, value);
        }

        private double _editTemperature = 0.7;
        public double EditTemperature
        {
            get => _editTemperature;
            set => SetProperty(ref _editTemperature, value);
        }

        private bool _editSupportsMultiModal;
        public bool EditSupportsMultiModal
        {
            get => _editSupportsMultiModal;
            set => SetProperty(ref _editSupportsMultiModal, value);
        }

        private bool _editSupportsImageGeneration;
        public bool EditSupportsImageGeneration
        {
            get => _editSupportsImageGeneration;
            set => SetProperty(ref _editSupportsImageGeneration, value);
        }

        private List<string> _editAvailableModels = new List<string>();
        public List<string> EditAvailableModels
        {
            get => _editAvailableModels;
            set => SetProperty(ref _editAvailableModels, value);
        }

        private string _connectionTestStatus;
        public string ConnectionTestStatus
        {
            get => _connectionTestStatus;
            set => SetProperty(ref _connectionTestStatus, value);
        }

        private bool _isTestingConnection;
        public bool IsTestingConnection
        {
            get => _isTestingConnection;
            set => SetProperty(ref _isTestingConnection, value);
        }

        public bool CanAddConnection => Connections.Count < MaxConnections;

        // Original values for dirty tracking
        private string _originalConnectionName;
        private string _originalProvider;
        private string _originalModel;
        private int _originalMaxTokens;
        private double _originalTemperature;
        private bool _originalSupportsMultiModal;
        private bool _originalSupportsImageGeneration;
        private bool _originalHasCustomApiKey;

        public bool HasUnsavedConnectionChanges
        {
            get
            {
                if (!IsEditingConnection) return false;
                if (IsAddingConnection)
                    return !string.IsNullOrWhiteSpace(EditConnectionName) || !string.IsNullOrEmpty(EditSelectedProvider);

                return EditConnectionName != _originalConnectionName ||
                       EditSelectedProvider != _originalProvider ||
                       EditSelectedModel != _originalModel ||
                       EditMaxTokens != _originalMaxTokens ||
                       Math.Abs(EditTemperature - _originalTemperature) > 0.001 ||
                       EditSupportsMultiModal != _originalSupportsMultiModal ||
                       EditSupportsImageGeneration != _originalSupportsImageGeneration ||
                       (EditShowCustomApiKey && !string.IsNullOrEmpty(EditCustomApiKey)) != _originalHasCustomApiKey;
            }
        }

        private void ResetDirtyTracking()
        {
            IsEditingConnection = false;
            IsAddingConnection = false;
            _originalConnectionName = null;
        }

        #endregion

        #region AI Provider API Keys

        public ObservableCollection<ProviderApiKeyViewModel> ProviderApiKeys { get; } = new ObservableCollection<ProviderApiKeyViewModel>();

        private ProviderApiKeyViewModel _selectedProviderApiKey;
        public ProviderApiKeyViewModel SelectedProviderApiKey
        {
            get => _selectedProviderApiKey;
            set
            {
                if (SetProperty(ref _selectedProviderApiKey, value))
                    UpdateProviderModels();
            }
        }

        private ObservableCollection<string> _selectedProviderModels = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedProviderModels
        {
            get => _selectedProviderModels;
            set => SetProperty(ref _selectedProviderModels, value);
        }

        private string _newModelName;
        public string NewModelName
        {
            get => _newModelName;
            set => SetProperty(ref _newModelName, value);
        }

        private string _selectedModelToRemove;
        public string SelectedModelToRemove
        {
            get => _selectedModelToRemove;
            set => SetProperty(ref _selectedModelToRemove, value);
        }

        private string _providerTestStatus;
        public string ProviderTestStatus
        {
            get => _providerTestStatus;
            set => SetProperty(ref _providerTestStatus, value);
        }

        private bool _isTestingProviderConnection;
        public bool IsTestingProviderConnection
        {
            get => _isTestingProviderConnection;
            set => SetProperty(ref _isTestingProviderConnection, value);
        }

        #endregion

        public string[] AiProviders { get; } = AiSettingsManager.SupportedProviders;

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ChangeThemeCommand { get; }

        // Connection commands
        public ICommand AddConnectionCommand { get; }
        public ICommand EditConnectionCommand { get; }
        public ICommand DeleteConnectionCommand { get; }
        public ICommand SaveConnectionCommand { get; }
        public ICommand CancelConnectionCommand { get; }
        public ICommand ShowCustomApiKeyCommand { get; }
        public ICommand TestConnectionCommand { get; }

        // Provider model commands
        public ICommand AddModelCommand { get; }
        public ICommand RemoveModelCommand { get; }
        public ICommand ResetModelsCommand { get; }
        public ICommand TestProviderConnectionCommand { get; }

        #endregion

        public SettingsViewModel()
        {
            LoadSettings();

            SaveCommand = new RelayCommand(Save);
            ResetCommand = new RelayCommand(Reset);
            ChangeThemeCommand = new RelayCommand<ThemeVariant>(ChangeTheme);

            AddConnectionCommand = new RelayCommand(StartAddConnection, () => CanAddConnection);
            EditConnectionCommand = new RelayCommand(StartEditConnection, () => SelectedConnection != null);
            DeleteConnectionCommand = new RelayCommand(DeleteConnection, () => SelectedConnection != null);
            SaveConnectionCommand = new RelayCommand(SaveConnection);
            CancelConnectionCommand = new RelayCommand(CancelConnectionEdit);
            ShowCustomApiKeyCommand = new RelayCommand(() => EditShowCustomApiKey = true);
            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync);

            AddModelCommand = new RelayCommand(AddModel);
            RemoveModelCommand = new RelayCommand(RemoveModel);
            ResetModelsCommand = new RelayCommand(ResetModelsToDefault);
            TestProviderConnectionCommand = new AsyncRelayCommand(TestProviderConnectionAsync);
        }

        private void LoadSettings()
        {
            var settings = AppSettings.Current;

            // General settings
            _audioInputDeviceName = settings.AudioInputDeviceName;
            _audioExportFormat = settings.AudioExportFormat ?? "WAV";
            _audioMp3Bitrate = settings.AudioMp3Bitrate;
            _gitHubToken = AppSettings.GetGitHubToken();

            // Load locale setting (detect from system if not set)
            var savedLocale = settings.Locale;
            if (string.IsNullOrEmpty(savedLocale))
            {
                // First start - detect from Windows system locale
                savedLocale = DetectSystemLocale();
                settings.Locale = savedLocale;
            }
            _selectedLocale = AvailableLocales.FirstOrDefault(l => l.Code == savedLocale)
                              ?? AvailableLocales.First();

            // Load UI language setting (use current culture or default to English)
            var savedLanguage = settings.Language;
            if (string.IsNullOrEmpty(savedLanguage))
            {
                // Use current ToolkitLocalization culture
                savedLanguage = ToolkitLocalization.CurrentCulture.Name;
            }
            _selectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == savedLanguage)
                                ?? AvailableLanguages.First();

            // Sync ActiveTheme with current app theme (already applied at startup in App.axaml.cs)
            var savedTheme = settings.Theme;
            _activeTheme = !string.IsNullOrEmpty(savedTheme) ? savedTheme.ParseThemeVariant() : ThemeVariant.Dark;

            // Load AI settings
            LoadAiSettings();
        }

        private void LoadAiSettings()
        {
            var aiManager = AppSettings.AiManager;

            // Load provider API keys (retrieving actual keys from secure storage)
            ProviderApiKeys.Clear();
            foreach (var provider in AiProviders)
            {
                var vm = new ProviderApiKeyViewModel
                {
                    ProviderType = provider,
                    ApiKey = aiManager.GetProviderApiKey(provider) ?? "",
                    CustomEndpoint = aiManager.GetProviderEndpoint(provider)
                };
                ProviderApiKeys.Add(vm);
            }

            // Wire up save callbacks after loading to avoid saving during load
            foreach (var vm in ProviderApiKeys)
            {
                vm.OnChanged = Save;
            }

            // Load connections
            Connections.Clear();
            foreach (var conn in aiManager.Connections)
            {
                Connections.Add(new AiConnectionViewModel
                {
                    Id = conn.Id,
                    Name = conn.Name,
                    ProviderType = conn.ProviderType,
                    ModelId = conn.ModelId,
                    HasCustomApiKey = !string.IsNullOrEmpty(conn.CustomApiKey),
                    MaxTokens = conn.MaxTokens,
                    Temperature = conn.Temperature,
                    SupportsMultiModalInput = conn.SupportsMultiModalInput,
                    SupportsImageGeneration = conn.SupportsImageGeneration
                });
            }

            if (ProviderApiKeys.Count > 0)
                SelectedProviderApiKey = ProviderApiKeys[0];
        }

        private void UpdateProviderModels()
        {
            SelectedProviderModels.Clear();
            if (SelectedProviderApiKey == null) return;

            var models = AppSettings.AiManager.GetProviderModels(SelectedProviderApiKey.ProviderType);
            foreach (var model in models)
                SelectedProviderModels.Add(model);
        }

        private void UpdateEditAvailableModels()
        {
            if (string.IsNullOrEmpty(EditSelectedProvider))
            {
                EditAvailableModels = new List<string>();
                return;
            }

            EditAvailableModels = AppSettings.AiManager.GetProviderModels(EditSelectedProvider);
        }

        #region Connection Management

        private void StartAddConnection()
        {
            if (!CanAddConnection) return;

            IsAddingConnection = true;
            IsEditingConnection = true;

            EditConnectionName = "";
            EditSelectedProvider = "OpenAI";
            EditShowCustomApiKey = false;
            EditCustomApiKey = "";
            EditMaxTokens = 4096;
            EditTemperature = 0.7;
            EditSupportsMultiModal = true;
            EditSupportsImageGeneration = false;
            ConnectionTestStatus = "";

            UpdateEditAvailableModels();
            EditSelectedModel = EditAvailableModels?.FirstOrDefault();

            // Reset original values for dirty tracking
            _originalConnectionName = "";
            _originalProvider = "";
            _originalModel = "";
            _originalMaxTokens = 4096;
            _originalTemperature = 0.7;
            _originalSupportsMultiModal = true;
            _originalSupportsImageGeneration = false;
            _originalHasCustomApiKey = false;
        }

        private void StartEditConnection()
        {
            if (SelectedConnection == null) return;

            IsAddingConnection = false;
            IsEditingConnection = true;

            EditConnectionName = SelectedConnection.Name;
            EditSelectedProvider = SelectedConnection.ProviderType;
            EditShowCustomApiKey = false;
            EditCustomApiKey = "";
            EditMaxTokens = SelectedConnection.MaxTokens;
            EditTemperature = SelectedConnection.Temperature;
            EditSupportsMultiModal = SelectedConnection.SupportsMultiModalInput;
            EditSupportsImageGeneration = SelectedConnection.SupportsImageGeneration;
            ConnectionTestStatus = "";

            UpdateEditAvailableModels();
            EditSelectedModel = SelectedConnection.ModelId;

            // Store original values for dirty tracking
            _originalConnectionName = EditConnectionName;
            _originalProvider = EditSelectedProvider;
            _originalModel = EditSelectedModel;
            _originalMaxTokens = EditMaxTokens;
            _originalTemperature = EditTemperature;
            _originalSupportsMultiModal = EditSupportsMultiModal;
            _originalSupportsImageGeneration = EditSupportsImageGeneration;
            _originalHasCustomApiKey = SelectedConnection.HasCustomApiKey;
        }

        private void SaveConnection()
        {
            if (string.IsNullOrWhiteSpace(EditConnectionName))
            {
                ConnectionTestStatus = "Please enter a connection name.";
                return;
            }

            if (string.IsNullOrEmpty(EditSelectedProvider))
            {
                ConnectionTestStatus = "Please select a provider.";
                return;
            }

            if (string.IsNullOrEmpty(EditSelectedModel))
            {
                ConnectionTestStatus = "Please select a model.";
                return;
            }

            var aiManager = AppSettings.AiManager;

            if (IsAddingConnection)
            {
                // Add via AiSettingsManager (handles secure storage)
                var customApiKey = EditShowCustomApiKey ? EditCustomApiKey : null;
                var newAiConn = aiManager.AddConnection(
                    EditConnectionName.Trim(),
                    EditSelectedProvider,
                    EditSelectedModel,
                    customApiKey);

                newAiConn.MaxTokens = EditMaxTokens;
                newAiConn.Temperature = EditTemperature;
                newAiConn.SupportsMultiModalInput = EditSupportsMultiModal;
                newAiConn.SupportsImageGeneration = EditSupportsImageGeneration;

                // Add to ViewModel collection
                Connections.Add(new AiConnectionViewModel
                {
                    Id = newAiConn.Id,
                    Name = newAiConn.Name,
                    ProviderType = newAiConn.ProviderType,
                    ModelId = newAiConn.ModelId,
                    HasCustomApiKey = !string.IsNullOrEmpty(customApiKey),
                    MaxTokens = newAiConn.MaxTokens,
                    Temperature = newAiConn.Temperature,
                    SupportsMultiModalInput = newAiConn.SupportsMultiModalInput,
                    SupportsImageGeneration = newAiConn.SupportsImageGeneration
                });
            }
            else if (SelectedConnection != null)
            {
                // Update ViewModel
                SelectedConnection.Name = EditConnectionName.Trim();
                SelectedConnection.ProviderType = EditSelectedProvider;
                SelectedConnection.ModelId = EditSelectedModel;
                SelectedConnection.HasCustomApiKey = EditShowCustomApiKey && !string.IsNullOrEmpty(EditCustomApiKey);
                SelectedConnection.MaxTokens = EditMaxTokens;
                SelectedConnection.Temperature = EditTemperature;
                SelectedConnection.SupportsMultiModalInput = EditSupportsMultiModal;
                SelectedConnection.SupportsImageGeneration = EditSupportsImageGeneration;

                // Update in AiSettingsManager
                var existing = aiManager.Connections.FirstOrDefault(c => c.Id == SelectedConnection.Id);
                if (existing != null)
                {
                    existing.Name = SelectedConnection.Name;
                    existing.ProviderType = SelectedConnection.ProviderType;
                    existing.ModelId = SelectedConnection.ModelId;
                    existing.MaxTokens = SelectedConnection.MaxTokens;
                    existing.Temperature = SelectedConnection.Temperature;
                    existing.SupportsMultiModalInput = SelectedConnection.SupportsMultiModalInput;
                    existing.SupportsImageGeneration = SelectedConnection.SupportsImageGeneration;

                    // Store custom API key securely via manager
                    if (EditShowCustomApiKey)
                        aiManager.SetConnectionApiKey(existing.Id, EditCustomApiKey);
                }
            }

            Save();
            IsEditingConnection = false;
            IsAddingConnection = false;
            OnPropertyChanged(nameof(CanAddConnection));
        }

        private void CancelConnectionEdit()
        {
            IsEditingConnection = false;
            IsAddingConnection = false;
            ConnectionTestStatus = "";
            // Reset dirty tracking
            _originalConnectionName = null;
        }

        /// <summary>
        /// Checks for unsaved changes before closing the dialog. Returns true if safe to close.
        /// </summary>
        public async Task<bool> CanCloseAsync()
        {
            if (!HasUnsavedConnectionChanges || PromptSaveChangesAction == null)
                return true;

            var result = await PromptSaveChangesAction("You have unsaved changes. Do you want to save them?");
            if (result == null)
                return false; // Cancel close
            if (result == true)
                SaveConnection();
            return true;
        }

        private void DeleteConnection()
        {
            if (SelectedConnection == null) return;

            // Remove via AiSettingsManager (handles secure storage cleanup)
            AppSettings.AiManager.RemoveConnection(SelectedConnection.Id);

            Connections.Remove(SelectedConnection);
            SelectedConnection = null;
            Save();
            OnPropertyChanged(nameof(CanAddConnection));
        }

        private async Task TestConnectionAsync()
        {
            if (IsTestingConnection) return;
            if (string.IsNullOrEmpty(EditSelectedProvider) || string.IsNullOrEmpty(EditSelectedModel))
            {
                ConnectionTestStatus = "Please select a provider and model.";
                return;
            }

            IsTestingConnection = true;
            ConnectionTestStatus = "Testing...";

            try
            {
                var aiManager = AppSettings.AiManager;
                var providerType = ParseProviderType(EditSelectedProvider);

                string apiKey = EditShowCustomApiKey && !string.IsNullOrEmpty(EditCustomApiKey)
                    ? EditCustomApiKey
                    : aiManager.GetProviderApiKey(EditSelectedProvider);

                string endpoint = aiManager.GetProviderEndpoint(EditSelectedProvider);

                var api = CreateTornadoApi(providerType, apiKey, endpoint);
                if (api == null)
                {
                    ConnectionTestStatus = "Failed to create API client.";
                    return;
                }

                var models = await api.Models.GetModels(MapToLlmProvider(providerType));
                if (models != null && models.Count > 0)
                {
                    var modelIds = models.Select(m => m.Id).Where(id => !string.IsNullOrEmpty(id)).OrderBy(id => id).ToList();
                    if (modelIds.Count > 0)
                    {
                        var currentModel = EditSelectedModel;
                        AppSettings.AiManager.SetProviderModels(EditSelectedProvider, modelIds);
                        UpdateEditAvailableModels();
                        var matchingModel = modelIds.FirstOrDefault(m => string.Equals(m, currentModel, StringComparison.OrdinalIgnoreCase));
                        EditSelectedModel = matchingModel ?? modelIds.FirstOrDefault();
                    }
                    ConnectionTestStatus = $"Connection successful! ({models.Count} models loaded)";
                }
                else
                {
                    ConnectionTestStatus = "Connection failed: No models returned.";
                }
            }
            catch (Exception ex)
            {
                ConnectionTestStatus = GetUserFriendlyMessage(ex);
#if DEBUG
                ShowDebugExceptionAction?.Invoke(ex);
#endif
            }
            finally
            {
                IsTestingConnection = false;
            }
        }

        private static TornadoApi CreateTornadoApi(AiProviderType providerType, string apiKey, string endpoint)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                if (providerType == AiProviderType.Ollama || providerType == AiProviderType.LMStudio)
                {
                    return new TornadoApi(new Uri(endpoint));
                }
                return null;
            }

            switch (providerType)
            {
                case AiProviderType.OpenAI:
                    return new TornadoApi(LLmProviders.OpenAi, apiKey);
                case AiProviderType.OpenRouter:
                    return new TornadoApi(LLmProviders.OpenRouter, apiKey);
                case AiProviderType.Anthropic:
                    return new TornadoApi(LLmProviders.Anthropic, apiKey);
                case AiProviderType.Google:
                    return new TornadoApi(LLmProviders.Google, apiKey);
                case AiProviderType.Ollama:
                    return new TornadoApi(new Uri(endpoint), apiKey);
                case AiProviderType.LMStudio:
                    return new TornadoApi(new Uri(endpoint), apiKey);
                default:
                    return new TornadoApi(LLmProviders.OpenAi, apiKey);
            }
        }

        private static LLmProviders MapToLlmProvider(AiProviderType providerType)
        {
            switch (providerType)
            {
                case AiProviderType.OpenAI: return LLmProviders.OpenAi;
                case AiProviderType.OpenRouter: return LLmProviders.OpenRouter;
                case AiProviderType.Anthropic: return LLmProviders.Anthropic;
                case AiProviderType.Google: return LLmProviders.Google;
                case AiProviderType.Ollama: return LLmProviders.OpenAi;
                case AiProviderType.LMStudio: return LLmProviders.OpenAi;
                default: return LLmProviders.OpenAi;
            }
        }

        private static string GetUserFriendlyMessage(Exception ex)
        {
            if (ex is System.Net.Http.HttpRequestException)
                return "Unable to connect to the AI provider. Please check your network connection and endpoint URL.";

            if (ex is TaskCanceledException || ex is OperationCanceledException)
                return "The request was cancelled or timed out.";

            var message = ex.Message ?? "An unexpected error occurred.";
            message = System.Text.RegularExpressions.Regex.Replace(
                message,
                @"(sk-[a-zA-Z0-9]{20,}|key[=:]\s*[""']?[a-zA-Z0-9\-_]{20,}[""']?|Bearer\s+[a-zA-Z0-9\-_\.]+|api[_-]?key[=:]\s*[^\s,}]+)",
                "[REDACTED]",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (message.Length > 500)
                return "An error occurred while communicating with the AI provider.";

            return message;
        }

        #endregion

        #region Provider Model Management

        private void AddModel()
        {
            if (SelectedProviderApiKey == null || string.IsNullOrWhiteSpace(NewModelName))
                return;

            var modelName = NewModelName.Trim();
            if (SelectedProviderModels.Contains(modelName))
                return;

            SelectedProviderModels.Add(modelName);
            AppSettings.AiManager.AddProviderModel(SelectedProviderApiKey.ProviderType, modelName);
            NewModelName = "";
            Save();
        }

        private void RemoveModel()
        {
            if (SelectedProviderApiKey == null || string.IsNullOrEmpty(SelectedModelToRemove))
                return;

            SelectedProviderModels.Remove(SelectedModelToRemove);
            AppSettings.AiManager.RemoveProviderModel(SelectedProviderApiKey.ProviderType, SelectedModelToRemove);
            SelectedModelToRemove = null;
            Save();
        }

        private void ResetModelsToDefault()
        {
            if (SelectedProviderApiKey == null) return;

            AppSettings.AiManager.ResetProviderModels(SelectedProviderApiKey.ProviderType);
            var defaultModels = AppSettings.AiManager.GetProviderModels(SelectedProviderApiKey.ProviderType);

            SelectedProviderModels.Clear();
            foreach (var model in defaultModels)
                SelectedProviderModels.Add(model);

            Save();
        }

        private async Task TestProviderConnectionAsync()
        {
            if (SelectedProviderApiKey == null) return;

            IsTestingProviderConnection = true;
            ProviderTestStatus = "Testing...";

            try
            {
                var providerType = ParseProviderType(SelectedProviderApiKey.ProviderType);
                var apiKey = SelectedProviderApiKey.ApiKey;
                var endpoint = SelectedProviderApiKey.CustomEndpoint;

                var api = CreateTornadoApi(providerType, apiKey, endpoint);
                if (api == null)
                {
                    ProviderTestStatus = "Failed: No API key configured.";
                    return;
                }

                var models = await api.Models.GetModels(MapToLlmProvider(providerType));
                if (models != null && models.Count > 0)
                {
                    var modelIds = models.Select(m => m.Id).Where(id => !string.IsNullOrEmpty(id)).OrderBy(id => id).ToList();
                    if (modelIds.Count > 0)
                    {
                        AppSettings.AiManager.SetProviderModels(SelectedProviderApiKey.ProviderType, modelIds);
                        UpdateProviderModels();
                        Save();
                    }
                    ProviderTestStatus = $"Success! {models.Count} models loaded.";
                }
                else
                {
                    ProviderTestStatus = "Failed: No models returned.";
                }
            }
            catch (Exception ex)
            {
                ProviderTestStatus = GetUserFriendlyMessage(ex);
#if DEBUG
                ShowDebugExceptionAction?.Invoke(ex);
#endif
            }
            finally
            {
                IsTestingProviderConnection = false;
            }
        }

        #endregion

        private void ChangeTheme(ThemeVariant theme)
        {
            var app = Application.Current;
            if (app?.Styles == null) return;

            app.RequestedThemeVariant = theme ?? ThemeVariant.Dark;
            ActiveTheme = theme ?? ThemeVariant.Dark;

            AppSettings.Current.Theme = theme.ToSettingsString();
            AppSettings.Save();
        }

        private void Save()
        {
            // Save provider API keys via AiSettingsManager (handles secure storage)
            var aiManager = AppSettings.AiManager;

            foreach (var vm in ProviderApiKeys)
            {
                aiManager.SetProviderApiKey(vm.ProviderType, vm.ApiKey);
                aiManager.SetProviderEndpoint(vm.ProviderType, vm.CustomEndpoint);
            }

            AppSettings.Save();
        }

        private void Reset()
        {
            AudioInputDeviceName = null;
            AudioExportFormat = "WAV";
            AudioMp3Bitrate = 192;
            SelectedLocale = AvailableLocales.First(); // Reset to en-US
            ChangeTheme(ThemeVariant.Dark);

            // Reset AI settings via manager (clears secure storage)
            AppSettings.AiManager.Reset();

            LoadAiSettings();
            AppSettings.Save();
        }

        private static AiProviderType ParseProviderType(string provider)
        {
            if (Enum.TryParse<AiProviderType>(provider, out var result))
                return result;
            return AiProviderType.OpenAI;
        }
    }

    public class AiConnectionViewModel : ObservableObject
    {
        public string Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _providerType;
        public string ProviderType
        {
            get => _providerType;
            set => SetProperty(ref _providerType, value);
        }

        private string _modelId;
        public string ModelId
        {
            get => _modelId;
            set => SetProperty(ref _modelId, value);
        }

        private bool _hasCustomApiKey;
        public bool HasCustomApiKey
        {
            get => _hasCustomApiKey;
            set => SetProperty(ref _hasCustomApiKey, value);
        }

        private int _maxTokens = 4096;
        public int MaxTokens
        {
            get => _maxTokens;
            set => SetProperty(ref _maxTokens, value);
        }

        private double _temperature = 0.7;
        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        private bool _supportsMultiModalInput;
        public bool SupportsMultiModalInput
        {
            get => _supportsMultiModalInput;
            set => SetProperty(ref _supportsMultiModalInput, value);
        }

        private bool _supportsImageGeneration;
        public bool SupportsImageGeneration
        {
            get => _supportsImageGeneration;
            set => SetProperty(ref _supportsImageGeneration, value);
        }

        public string DisplayText => $"{Name} ({ProviderType}: {ModelId})";
    }

    public class ProviderApiKeyViewModel : ObservableObject
    {
        public string ProviderType { get; set; }

        public Action OnChanged { get; set; }

        private string _apiKey;
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (SetProperty(ref _apiKey, value))
                {
                    OnPropertyChanged(nameof(HasApiKey));
                    OnChanged?.Invoke();
                }
            }
        }

        private string _customEndpoint;
        public string CustomEndpoint
        {
            get => _customEndpoint;
            set
            {
                if (SetProperty(ref _customEndpoint, value))
                    OnChanged?.Invoke();
            }
        }

        public bool HasApiKey => !string.IsNullOrEmpty(ApiKey);
    }

    /// <summary>
    /// Represents a locale option for the Semi theme.
    /// </summary>
    public class LocaleItem
    {
        public string DisplayName { get; }
        public string Code { get; }

        public LocaleItem(string displayName, string code)
        {
            DisplayName = displayName;
            Code = code;
        }

        public override string ToString() => DisplayName;
    }
}
