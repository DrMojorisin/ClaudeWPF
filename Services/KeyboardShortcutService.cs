using System.Windows.Input;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Service for managing keyboard shortcuts with persistence
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly IUserSettingsService _userSettingsService;
    private readonly Dictionary<string, KeyboardShortcut> _shortcuts = new();
    private readonly Dictionary<(Key, ModifierKeys), string> _keyToCommandMap = new();
    private const string SHORTCUTS_SETTINGS_KEY = "KeyboardShortcuts";

    public event EventHandler<ShortcutChangedEventArgs>? ShortcutChanged;

    public KeyboardShortcutService(IUserSettingsService userSettingsService)
    {
        _userSettingsService = userSettingsService;
        RegisterDefaultShortcuts();
        _ = LoadShortcutsAsync();
    }

    public void RegisterShortcut(string commandName, Key key, ModifierKeys modifiers, ICommand command, string description = "")
    {
        var oldShortcut = _shortcuts.TryGetValue(commandName, out var existing) ? existing : null;
        
        var shortcut = new KeyboardShortcut
        {
            CommandName = commandName,
            Key = key,
            Modifiers = modifiers,
            Command = command,
            Description = description,
            IsEnabled = true,
            IsCustom = false
        };

        // Remove old mapping if exists
        if (oldShortcut != null)
        {
            _keyToCommandMap.Remove((oldShortcut.Key, oldShortcut.Modifiers));
        }

        // Check for conflicts
        if (_keyToCommandMap.ContainsKey((key, modifiers)))
        {
            var conflictingCommand = _keyToCommandMap[(key, modifiers)];
            throw new InvalidOperationException($"Shortcut {shortcut.DisplayText} is already assigned to {conflictingCommand}");
        }

        _shortcuts[commandName] = shortcut;
        _keyToCommandMap[(key, modifiers)] = commandName;

        ShortcutChanged?.Invoke(this, new ShortcutChangedEventArgs(commandName, oldShortcut, shortcut));
    }

    public void UnregisterShortcut(string commandName)
    {
        if (_shortcuts.TryGetValue(commandName, out var shortcut))
        {
            _shortcuts.Remove(commandName);
            _keyToCommandMap.Remove((shortcut.Key, shortcut.Modifiers));
            ShortcutChanged?.Invoke(this, new ShortcutChangedEventArgs(commandName, shortcut, null));
        }
    }

    public IEnumerable<KeyboardShortcut> GetShortcuts()
    {
        return _shortcuts.Values.OrderBy(s => s.CommandName);
    }

    public KeyboardShortcut? GetShortcut(string commandName)
    {
        return _shortcuts.TryGetValue(commandName, out var shortcut) ? shortcut : null;
    }

    public bool UpdateShortcut(string commandName, Key newKey, ModifierKeys newModifiers)
    {
        if (!_shortcuts.TryGetValue(commandName, out var shortcut))
            return false;

        // Check if new combination is already in use
        if (_keyToCommandMap.ContainsKey((newKey, newModifiers)) && 
            _keyToCommandMap[(newKey, newModifiers)] != commandName)
        {
            return false; // Conflict
        }

        var oldShortcut = new KeyboardShortcut
        {
            CommandName = shortcut.CommandName,
            Key = shortcut.Key,
            Modifiers = shortcut.Modifiers,
            Command = shortcut.Command,
            Description = shortcut.Description,
            IsEnabled = shortcut.IsEnabled,
            IsCustom = shortcut.IsCustom
        };

        // Remove old mapping
        _keyToCommandMap.Remove((shortcut.Key, shortcut.Modifiers));

        // Update shortcut
        shortcut.Key = newKey;
        shortcut.Modifiers = newModifiers;
        shortcut.IsCustom = true;

        // Add new mapping
        _keyToCommandMap[(newKey, newModifiers)] = commandName;

        ShortcutChanged?.Invoke(this, new ShortcutChangedEventArgs(commandName, oldShortcut, shortcut));
        return true;
    }

    public bool IsShortcutInUse(Key key, ModifierKeys modifiers)
    {
        return _keyToCommandMap.ContainsKey((key, modifiers));
    }

    public ICommand? GetCommand(Key key, ModifierKeys modifiers)
    {
        if (_keyToCommandMap.TryGetValue((key, modifiers), out var commandName) &&
            _shortcuts.TryGetValue(commandName, out var shortcut))
        {
            return shortcut.Command;
        }
        return null;
    }

    public bool ExecuteShortcut(Key key, ModifierKeys modifiers)
    {
        var command = GetCommand(key, modifiers);
        if (command != null && command.CanExecute(null))
        {
            command.Execute(null);
            return true;
        }
        return false;
    }

    public async Task LoadShortcutsAsync()
    {
        try
        {
            var savedShortcuts = _userSettingsService.GetSetting<Dictionary<string, ShortcutSettings>>(SHORTCUTS_SETTINGS_KEY);
            if (savedShortcuts == null) return;

            foreach (var (commandName, settings) in savedShortcuts)
            {
                if (_shortcuts.TryGetValue(commandName, out var shortcut))
                {
                    // Remove old mapping
                    _keyToCommandMap.Remove((shortcut.Key, shortcut.Modifiers));

                    // Update with saved settings
                    shortcut.Key = settings.Key;
                    shortcut.Modifiers = settings.Modifiers;
                    shortcut.IsEnabled = settings.IsEnabled;
                    shortcut.IsCustom = settings.IsCustom;

                    // Add new mapping if enabled
                    if (shortcut.IsEnabled)
                    {
                        _keyToCommandMap[(shortcut.Key, shortcut.Modifiers)] = commandName;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading shortcuts: {ex}");
        }
    }

    public async Task SaveShortcutsAsync()
    {
        try
        {
            var shortcutsToSave = new Dictionary<string, ShortcutSettings>();
            
            foreach (var (commandName, shortcut) in _shortcuts)
            {
                shortcutsToSave[commandName] = new ShortcutSettings
                {
                    Key = shortcut.Key,
                    Modifiers = shortcut.Modifiers,
                    IsEnabled = shortcut.IsEnabled,
                    IsCustom = shortcut.IsCustom
                };
            }

            _userSettingsService.SetSetting(SHORTCUTS_SETTINGS_KEY, shortcutsToSave);
            await _userSettingsService.SaveAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving shortcuts: {ex}");
        }
    }

    public void ResetToDefaults()
    {
        _shortcuts.Clear();
        _keyToCommandMap.Clear();
        RegisterDefaultShortcuts();
        
        // Remove saved shortcuts from settings
        _userSettingsService.RemoveSetting(SHORTCUTS_SETTINGS_KEY);
    }

    public string GetShortcutText(Key key, ModifierKeys modifiers)
    {
        var modifierText = string.Empty;
        
        if (modifiers.HasFlag(ModifierKeys.Control))
            modifierText += "Ctrl+";
        if (modifiers.HasFlag(ModifierKeys.Alt))
            modifierText += "Alt+";
        if (modifiers.HasFlag(ModifierKeys.Shift))
            modifierText += "Shift+";
        if (modifiers.HasFlag(ModifierKeys.Windows))
            modifierText += "Win+";

        return modifierText + key.ToString();
    }

    private void RegisterDefaultShortcuts()
    {
        // File shortcuts
        RegisterDefaultShortcut("New", Key.N, ModifierKeys.Control, "Create new document");
        RegisterDefaultShortcut("Open", Key.O, ModifierKeys.Control, "Open file");
        RegisterDefaultShortcut("Save", Key.S, ModifierKeys.Control, "Save current document");
        RegisterDefaultShortcut("SaveAll", Key.S, ModifierKeys.Control | ModifierKeys.Shift, "Save all documents");
        RegisterDefaultShortcut("Print", Key.P, ModifierKeys.Control, "Print current document");

        // Edit shortcuts
        RegisterDefaultShortcut("Undo", Key.Z, ModifierKeys.Control, "Undo last action");
        RegisterDefaultShortcut("Redo", Key.Y, ModifierKeys.Control, "Redo last action");
        RegisterDefaultShortcut("Cut", Key.X, ModifierKeys.Control, "Cut selection");
        RegisterDefaultShortcut("Copy", Key.C, ModifierKeys.Control, "Copy selection");
        RegisterDefaultShortcut("Paste", Key.V, ModifierKeys.Control, "Paste from clipboard");
        RegisterDefaultShortcut("SelectAll", Key.A, ModifierKeys.Control, "Select all");
        RegisterDefaultShortcut("Find", Key.F, ModifierKeys.Control, "Find text");
        RegisterDefaultShortcut("Replace", Key.H, ModifierKeys.Control, "Find and replace");

        // View shortcuts
        RegisterDefaultShortcut("ToggleTheme", Key.T, ModifierKeys.Control | ModifierKeys.Shift, "Toggle application theme");
        RegisterDefaultShortcut("FullScreen", Key.F11, ModifierKeys.None, "Toggle full screen");
        RegisterDefaultShortcut("ZoomIn", Key.OemPlus, ModifierKeys.Control, "Zoom in");
        RegisterDefaultShortcut("ZoomOut", Key.OemMinus, ModifierKeys.Control, "Zoom out");
        RegisterDefaultShortcut("ResetZoom", Key.D0, ModifierKeys.Control, "Reset zoom");

        // Window shortcuts
        RegisterDefaultShortcut("CloseTab", Key.W, ModifierKeys.Control, "Close current tab");
        RegisterDefaultShortcut("NewTab", Key.T, ModifierKeys.Control, "Create new tab");
        RegisterDefaultShortcut("NextTab", Key.Tab, ModifierKeys.Control, "Switch to next tab");
        RegisterDefaultShortcut("PreviousTab", Key.Tab, ModifierKeys.Control | ModifierKeys.Shift, "Switch to previous tab");

        // Application shortcuts
        RegisterDefaultShortcut("Exit", Key.F4, ModifierKeys.Alt, "Exit application");
        RegisterDefaultShortcut("Settings", Key.OemComma, ModifierKeys.Control, "Open settings");
        RegisterDefaultShortcut("Help", Key.F1, ModifierKeys.None, "Show help");
    }

    private void RegisterDefaultShortcut(string commandName, Key key, ModifierKeys modifiers, string description)
    {
        // Don't assign command here - it will be assigned when the actual command is registered
        var shortcut = new KeyboardShortcut
        {
            CommandName = commandName,
            Key = key,
            Modifiers = modifiers,
            Command = null,
            Description = description,
            IsEnabled = false, // Will be enabled when command is assigned
            IsCustom = false
        };

        _shortcuts[commandName] = shortcut;
    }
}

/// <summary>
/// Serializable settings for keyboard shortcuts
/// </summary>
internal class ShortcutSettings
{
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsCustom { get; set; } = false;
}