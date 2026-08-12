namespace KeepassNativeSearch;

/**
 * Plugin settings for KeepassNativeSearch.
 */
public class Settings
{
    public string DatabaseAbsolutePath { get; set; } = SettingsDefaults.DatabaseAbsolutePath;
    public string DatabasePassword { get; set; } = SettingsDefaults.DatabasePassword;
    public string KeyFileAbsolutePath { get; set; } = SettingsDefaults.KeyfileAbsolutePath;

    /**
     * Flags to determine which entry fields in the database should be searched for keywords.
     */
    public bool SearchTitle { get; set; } = SettingsDefaults.SearchTitle;

    public bool SearchNotes { get; set; } = SettingsDefaults.SearchNotes;
    public bool SearchUrl { get; set; } = SettingsDefaults.SearchUrl;
    public bool SearchTags { get; set; } = SettingsDefaults.SearchTags;
    public bool SearchUserName { get; set; } = SettingsDefaults.SearchUsername;
    public bool SearchGroups { get; set; } = SettingsDefaults.SearchGroups;

    /**
     * Whether automatic clipboard clearing should be enabled.
     */
    public bool ClearClipboard { get; set; } = SettingsDefaults.ClearClipboard;

    /**
     * Duration before the clipboard should be cleared in seconds.
     */
    public int ClearClipboardDurationSeconds { get; set; } = SettingsDefaults.ClearClipboardDurationSeconds;

    private bool Equals(Settings other)
    {
        return DatabaseAbsolutePath == other.DatabaseAbsolutePath && DatabasePassword == other.DatabasePassword &&
               KeyFileAbsolutePath == other.KeyFileAbsolutePath && SearchTitle == other.SearchTitle &&
               SearchNotes == other.SearchNotes && SearchUrl == other.SearchUrl && SearchTags == other.SearchTags &&
               SearchUserName == other.SearchUserName && ClearClipboard == other.ClearClipboard &&
               ClearClipboardDurationSeconds == other.ClearClipboardDurationSeconds &&
               SearchGroups == other.SearchGroups;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Settings)obj);
    }

    public bool DatabaseConfigEquals(Settings other)
    {
        return DatabaseAbsolutePath == other.DatabaseAbsolutePath && DatabasePassword == other.DatabasePassword &&
               KeyFileAbsolutePath == other.KeyFileAbsolutePath;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(DatabaseAbsolutePath);
        hashCode.Add(DatabasePassword);
        hashCode.Add(KeyFileAbsolutePath);
        hashCode.Add(SearchTitle);
        hashCode.Add(SearchNotes);
        hashCode.Add(SearchUrl);
        hashCode.Add(SearchTags);
        hashCode.Add(SearchUserName);
        hashCode.Add(ClearClipboard);
        hashCode.Add(SearchGroups);
        hashCode.Add(ClearClipboardDurationSeconds);
        return hashCode.ToHashCode();
    }

    /**
     * <summary>Updates this object with encrypted values.</summary>
     *
     * This object is modified instead of returning a new one because of how Flow's settings saving works.
     *
     * <param name="updatedSettings">The settings updated by the user that will update this reference.</param>
     *
     */
    public void EncryptValues(Settings updatedSettings)
    {
        DatabaseAbsolutePath = EncryptionUtilities.Encrypt(updatedSettings.DatabaseAbsolutePath);
        KeyFileAbsolutePath = EncryptionUtilities.Encrypt(updatedSettings.KeyFileAbsolutePath);
        DatabasePassword = EncryptionUtilities.Encrypt(updatedSettings.DatabasePassword);
        SearchTitle = updatedSettings.SearchTitle;
        SearchNotes = updatedSettings.SearchNotes;
        SearchUrl = updatedSettings.SearchUrl;
        SearchTags = updatedSettings.SearchTags;
        SearchGroups = updatedSettings.SearchGroups;
        SearchUserName = updatedSettings.SearchUserName;
        ClearClipboard = updatedSettings.ClearClipboard;
        ClearClipboardDurationSeconds = updatedSettings.ClearClipboardDurationSeconds;
    }

    /**
     * <summary>Decrypts values and returns a new Settings object.</summary>
     *
     * <returns>A new settings object with decrypted class members.</returns>
     */
    public Settings DecryptValues()
    {
        return new Settings
        {
            DatabaseAbsolutePath = EncryptionUtilities.Decrypt(DatabaseAbsolutePath),
            KeyFileAbsolutePath = EncryptionUtilities.Decrypt(KeyFileAbsolutePath),
            DatabasePassword = EncryptionUtilities.Decrypt(DatabasePassword),
            SearchTitle = SearchTitle,
            SearchNotes = SearchNotes,
            SearchUrl = SearchUrl,
            SearchTags = SearchTags,
            SearchGroups = SearchGroups,
            SearchUserName = SearchUserName,
            ClearClipboard = ClearClipboard,
            ClearClipboardDurationSeconds = ClearClipboardDurationSeconds
        };
    }

    public static bool operator ==(Settings? left, Settings? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Settings? left, Settings? right)
    {
        return !Equals(left, right);
    }
}

/**
 * Default configuration values for the plugin settings.
 */
public static class SettingsDefaults
{
    public const string DatabaseAbsolutePath = "";
    public const string DatabasePassword = "";
    public const string KeyfileAbsolutePath = "";
    public const bool SearchTitle = true;
    public const bool SearchNotes = true;
    public const bool SearchUrl = false;
    public const bool SearchTags = false;
    public const bool SearchUsername = false;
    public const bool SearchGroups = false;
    public const bool ClearClipboard = true;
    public const int ClearClipboardDurationSeconds = 10;
}