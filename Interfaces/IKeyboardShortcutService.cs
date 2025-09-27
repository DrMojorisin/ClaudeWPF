using System.Windows.Input;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for managing keyboard shortcuts
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Event raised when shortcuts are changed
    /// </summary>
    event EventHandler<ShortcutChangedEventArgs>? ShortcutChanged;

    /// <summary>
    /// Register a keyboard shortcut
    /// </summary>
    void RegisterShortcut(string commandName, Key key, ModifierKeys modifiers, ICommand command, string description = "");

    /// <summary>
    /// Unregister a keyboard shortcut
    /// </summary>
    void UnregisterShortcut(string commandName);

    /// <summary>
    /// Get all registered shortcuts
    /// </summary>
    IEnumerable<KeyboardShortcut> GetShortcuts();

    /// <summary>
    /// Get shortcut by command name
    /// </summary>
    KeyboardShortcut? GetShortcut(string commandName);

    /// <summary>
    /// Update an existing shortcut
    /// </summary>
    bool UpdateShortcut(string commandName, Key newKey, ModifierKeys newModifiers);

    /// <summary>
    /// Check if a key combination is already in use
    /// </summary>
    bool IsShortcutInUse(Key key, ModifierKeys modifiers);

    /// <summary>
    /// Get the command for a key combination
    /// </summary>
    ICommand? GetCommand(Key key, ModifierKeys modifiers);

    /// <summary>
    /// Execute command for key combination
    /// </summary>
    bool ExecuteShortcut(Key key, ModifierKeys modifiers);

    /// <summary>
    /// Load shortcuts from settings
    /// </summary>
    Task LoadShortcutsAsync();

    /// <summary>
    /// Save shortcuts to settings
    /// </summary>
    Task SaveShortcutsAsync();

    /// <summary>
    /// Reset shortcuts to defaults
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Get shortcut display text
    /// </summary>
    string GetShortcutText(Key key, ModifierKeys modifiers);
}

/// <summary>
/// Represents a keyboard shortcut
/// </summary>
public class KeyboardShortcut
{
    public string CommandName { get; set; } = string.Empty;
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }
    public ICommand? Command { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsCustom { get; set; } = false;

    /// <summary>
    /// Gets the display text for this shortcut
    /// </summary>
    public string DisplayText
    {
        get
        {
            var modifierText = string.Empty;
            
            if (Modifiers.HasFlag(ModifierKeys.Control))
                modifierText += "Ctrl+";
            if (Modifiers.HasFlag(ModifierKeys.Alt))
                modifierText += "Alt+";
            if (Modifiers.HasFlag(ModifierKeys.Shift))
                modifierText += "Shift+";
            if (Modifiers.HasFlag(ModifierKeys.Windows))
                modifierText += "Win+";

            return modifierText + Key.ToString();
        }
    }

    /// <summary>
    /// Checks if this shortcut matches the given key combination
    /// </summary>
    public bool Matches(Key key, ModifierKeys modifiers)
    {
        return Key == key && Modifiers == modifiers;
    }
}

/// <summary>
/// Event arguments for shortcut changes
/// </summary>
public class ShortcutChangedEventArgs : EventArgs
{
    public string CommandName { get; }
    public KeyboardShortcut? OldShortcut { get; }
    public KeyboardShortcut? NewShortcut { get; }

    public ShortcutChangedEventArgs(string commandName, KeyboardShortcut? oldShortcut, KeyboardShortcut? newShortcut)
    {
        CommandName = commandName;
        OldShortcut = oldShortcut;
        NewShortcut = newShortcut;
    }
}