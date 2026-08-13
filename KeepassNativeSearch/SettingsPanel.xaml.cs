using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace KeepassNativeSearch;

public partial class SettingsPanel
{
    private readonly Action<Settings> _onSettingsChanged;
    private readonly TaskExecutor _taskExecutor = new();

    private const int MinClipboardClearDurationSec = 5;
    private const int MaxClipboardClearDurationSec = 1800;

    private const int MinCloseDbAfterDurationMin = 1;
    private const int MaxCloseDbAfterDurationMin = 1440;

    public SettingsPanel(Action<Settings> onSettingsChanged, Settings initialSettings)
    {
        _onSettingsChanged = onSettingsChanged;
        InitializeComponent();


        // Initialize field values with what is contained from the settings
        DatabaseAbsolutePathTextBox.Text = initialSettings.DatabaseAbsolutePath;
        DatabasePasswordBox.Password = initialSettings.DatabasePassword;
        KeyFileAbsolutePathTextBox.Text = initialSettings.KeyFileAbsolutePath;
        SearchTitleCheckBox.IsChecked = initialSettings.SearchTitle;
        SearchNotesCheckBox.IsChecked = initialSettings.SearchNotes;
        SearchUrlCheckBox.IsChecked = initialSettings.SearchUrl;
        SearchGroupCheckBox.IsChecked = initialSettings.SearchGroups;
        SearchTagsCheckBox.IsChecked = initialSettings.SearchTags;
        SearchUserNameCheckBox.IsChecked = initialSettings.SearchUserName;
        ClearClipboardCheckbox.IsChecked = initialSettings.ClearClipboard;
        var clipboardDurationSeconds = Normalize(initialSettings.ClearClipboardDurationSeconds,
            MinClipboardClearDurationSec, MaxClipboardClearDurationSec);
        ClearClipboardDurationTextBox.Text = clipboardDurationSeconds.ToString();
        ClearClipboardDurationTextBox.IsEnabled = initialSettings.ClearClipboard;
        RequireWindowsHelloCheckBox.IsChecked = initialSettings.RequireWindowsHello;
        if (!initialSettings.ClearClipboard)
        {
            DurationRangeWarningLabel.Visibility = Visibility.Hidden;
        }
        else
        {
            DurationRangeWarningLabel.Visibility =
                initialSettings.ClearClipboardDurationSeconds is < MinClipboardClearDurationSec
                    or > MaxClipboardClearDurationSec
                    ? Visibility.Visible
                    : Visibility.Hidden;
        }

        CloseDbAfterDurationCheckBox.IsChecked = initialSettings.CloseDbAfterDuration;
        var closeDbMinutes = Normalize(initialSettings.CloseDbDurationMinutes, MinCloseDbAfterDurationMin,
            MaxCloseDbAfterDurationMin);
        CloseDbAfterDurationTextBox.Text = closeDbMinutes.ToString();
        CloseDbAfterDurationTextBox.IsEnabled = initialSettings.CloseDbAfterDuration;
        if (!initialSettings.CloseDbAfterDuration)
        {
            CloseDbDurationRangeWarningLabel.Visibility = Visibility.Hidden;
        }
        else
        {
            CloseDbDurationRangeWarningLabel.Visibility =
                initialSettings.CloseDbDurationMinutes is < MinCloseDbAfterDurationMin
                    or > MaxCloseDbAfterDurationMin
                    ? Visibility.Visible
                    : Visibility.Hidden;
        }

        CloseDbOnSessionChangeCheckBox.IsChecked = initialSettings.CloseDbUserSession;
        CloseDbOnLockCheckBox.IsChecked = initialSettings.CloseDbLockScreen;
        CloseDbOnSleepCheckBox.IsChecked = initialSettings.CloseDbComputerSleep;
    }

    private void BrowserDatabaseOpenOnButtonClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "KeePass Databases (*.kdbx;*.kdb)|*.kdbx;*.kdb|All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true) return;
        DatabaseAbsolutePathTextBox.Text = openFileDialog.FileName;
        UpdateContent();
    }

    private void DatabasePasswordBoxOnPasswordChanged(object sender, RoutedEventArgs e)
    {
        UpdateContent();
    }

    private void BrowserKeyFileOpenOnButtonClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "KeePass Keyfile (*.key)|*.key|All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true) return;
        KeyFileAbsolutePathTextBox.Text = openFileDialog.FileName;
        UpdateContent();
    }

    private void CheckBoxOnClicked(object sender, RoutedEventArgs e)
    {
        UpdateContent();
    }

    private void KeyFileAbsolutePathTextBoxOnKeyUp(object sender, KeyEventArgs e)
    {
        UpdateContent();
    }

    private void DatabaseAbsolutePathTextBoxOnTextChanged(object sender, KeyEventArgs e)
    {
        UpdateContent();
    }

    private void ClearClipboardCheckboxOnClick(object sender, RoutedEventArgs e)
    {
        ClearClipboardDurationTextBox.IsEnabled =
            ClearClipboardCheckbox.IsChecked ?? SettingsDefaults.ClearClipboard;
        UpdateContent();
    }

    private void ClearClipboardDurationTextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        DoOnPreviewText(sender, e);
    }

    private void ClearClipboardDurationTextBoxOnPasting(object sender, DataObjectPastingEventArgs
        e)
    {
        DoOnPasting(sender, e);
    }

    private void CloseDbAfterDurationCheckBoxOnClick(object sender, RoutedEventArgs e)
    {
        CloseDbAfterDurationTextBox.IsEnabled =
            CloseDbAfterDurationCheckBox.IsChecked ?? SettingsDefaults.CloseDbAfterDuration;
        UpdateContent();
    }

    private void CloseDbAfterDurationTextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        DoOnPreviewText(sender, e);
    }

    private void CloseDbAfterDurationTextBoxOnPasting(object sender, DataObjectPastingEventArgs e)
    {
        DoOnPasting(sender, e);
    }

    private void RequireWindowsHelloCheckBoxOnClick(object sender, RoutedEventArgs e)
    {
        UpdateContent();
    }

    private void CloseDbOnLockCheckBoxOnClick(object sender, RoutedEventArgs e)
    {
        UpdateContent();
    }

    private void CloseDbOnSleepCheckBoxOnClick(object sender, RoutedEventArgs e)
    {
        UpdateContent();
    }

    private void CloseDbOnSessionChangeCheckBoxOnClick(object sender, RoutedEventArgs e)
    {
        UpdateContent();
    }

    private void UpdateContent()
    {
        _taskExecutor.Execute(() =>
        {
            // Ensures that accessing UI field values is done with the right thread affinity.
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!int.TryParse(ClearClipboardDurationTextBox.Text, out var clipboardDuration))
                {
                    clipboardDuration = SettingsDefaults.ClearClipboardDurationSeconds;
                }

                // Update visibility for warning labels based on whether clear clipboard is enabled.
                if (!(ClearClipboardCheckbox.IsChecked ?? SettingsDefaults.ClearClipboard))
                {
                    DurationRangeWarningLabel.Visibility = Visibility.Hidden;
                }
                else
                {
                    DurationRangeWarningLabel.Visibility =
                        clipboardDuration is < MinClipboardClearDurationSec
                            or > MaxClipboardClearDurationSec
                            ? Visibility.Visible
                            : Visibility.Hidden;
                }

                if (!int.TryParse(CloseDbAfterDurationTextBox.Text, out var closeDbDuration))
                {
                    closeDbDuration = SettingsDefaults.CloseDbDurationMinutes;
                }
                else
                {
                    CloseDbDurationRangeWarningLabel.Visibility =
                        closeDbDuration is < MinCloseDbAfterDurationMin or > MaxCloseDbAfterDurationMin
                            ? Visibility.Visible
                            : Visibility.Hidden;
                }

                var updatedSettings = new Settings
                {
                    DatabaseAbsolutePath = DatabaseAbsolutePathTextBox.Text,
                    DatabasePassword = DatabasePasswordBox.Password,
                    KeyFileAbsolutePath = KeyFileAbsolutePathTextBox.Text,
                    SearchTitle = SearchTitleCheckBox.IsChecked ?? SettingsDefaults.SearchTitle,
                    SearchNotes = SearchNotesCheckBox.IsChecked ?? SettingsDefaults.SearchNotes,
                    SearchUrl = SearchUrlCheckBox.IsChecked ?? SettingsDefaults.SearchUrl,
                    SearchTags = SearchTagsCheckBox.IsChecked ?? SettingsDefaults.SearchTags,
                    SearchGroups = SearchGroupCheckBox.IsChecked ?? SettingsDefaults.SearchGroups,
                    SearchUserName = SearchUserNameCheckBox.IsChecked ?? SettingsDefaults.SearchUsername,
                    ClearClipboard = ClearClipboardCheckbox.IsChecked ?? SettingsDefaults.ClearClipboard,
                    RequireWindowsHello = RequireWindowsHelloCheckBox.IsChecked ?? SettingsDefaults.RequireWindowsHello,
                    ClearClipboardDurationSeconds = Normalize(clipboardDuration, MinClipboardClearDurationSec,
                        MaxClipboardClearDurationSec),
                    CloseDbAfterDuration =
                        CloseDbAfterDurationCheckBox.IsChecked ?? SettingsDefaults.CloseDbAfterDuration,
                    CloseDbDurationMinutes = Normalize(closeDbDuration, MinCloseDbAfterDurationMin,
                        MaxCloseDbAfterDurationMin),
                    CloseDbComputerSleep = CloseDbOnSleepCheckBox.IsChecked ?? SettingsDefaults.CloseDbSleep,
                    CloseDbUserSession = CloseDbOnSessionChangeCheckBox.IsChecked ??
                                         SettingsDefaults.CloseDbUserSessionChange,
                    CloseDbLockScreen = CloseDbOnLockCheckBox.IsChecked ?? SettingsDefaults.CloseDbLockChange
                };
                _onSettingsChanged(updatedSettings);
            });
        });
    }

    private static int Normalize(int duration, int min, int max)
    {
        return (duration < min || duration > max)
            ? SettingsDefaults.CloseDbDurationMinutes
            : duration;
    }

    private bool IsTextNonNegativeInteger(string text)
    {
        return string.IsNullOrEmpty(text) ||
               Regex.IsMatch(text, @"^\d*$");
    }

    private static string GetProposedText(TextBox textBox, string newText)
    {
        var currentText = textBox.Text;
        var selectionStart = textBox.SelectionStart;
        var selectionLength = textBox.SelectionLength;

        return currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, newText);
    }

    private void DoOnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            TextBox? textBox = sender as TextBox;
            string? pastedText = (string)e.DataObject.GetData(DataFormats.Text);
            string fullText = GetProposedText(textBox, pastedText);

            if (!IsTextNonNegativeInteger(fullText))
            {
                e.CancelCommand();
            }
            else
            {
                UpdateContent();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void DoOnPreviewText(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        var fullText = GetProposedText(textBox, e.Text);

        var isTextNonNegativeInteger = IsTextNonNegativeInteger(fullText);
        if (isTextNonNegativeInteger)
        {
            UpdateContent();
        }

        e.Handled = !isTextNonNegativeInteger;
    }
}