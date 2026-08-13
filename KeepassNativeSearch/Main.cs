using System.Windows;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Pidamg.KeePass;

namespace KeepassNativeSearch;

public class Main : IAsyncPlugin, ISettingProvider, IContextMenu
{
    private volatile PluginInitContext? _context;
    private volatile KdbxDatabase? _db;
    private volatile Settings _settings = null!;

    private readonly TaskExecutor _loadDatabaseTaskExecutor = new();
    private readonly TaskExecutor _clearClipboardTaskExecutor = new();
    private readonly TaskExecutor _closeDbTaskExecutor = new();

    private static readonly string? LogTag = typeof(Main).Namespace;

    public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        return Task.Run(() =>
        {
            var results = new List<Result>();
            /*
             *  If the first search term is the control constant, don't show any options but the control options
             */
            if (query.FirstSearch == Constants.UserEntryConstants.DatabaseControlKeyword)
            {
                return ResultsFactory.CreateControlResults(!string.IsNullOrWhiteSpace(_settings.KeyFileAbsolutePath),
                    () => { _loadDatabaseTaskExecutor.Execute(LoadDatabase, 0); }, () =>
                    {
                        _closeDbTaskExecutor.Cancel();
                        CloseDatabase();
                    });
            }

            /*
             * If the first search term was not the control constant, show the database status result and database
             * entry results
             */
            results.Add(ResultsFactory.CreateKeepassFileStatusResult(_db != null));

            var filter = new ScoringEntryFilter(_settings);
            var foundEntries = _db?.FindAllEntries(entry => filter.Filter(query.Search, query.SearchTerms, entry)) ??
                               new List<Entry>();

            results.AddRange(foundEntries.Select(entry =>
            {
                var filterCondition = filter.ScoreAndFilter(query.Search, query.SearchTerms, entry);
                var subTitle = $"({filterCondition.field.Label()}) " +
                               (string.IsNullOrEmpty(entry.UserName)
                                   ? Resources.EmptyUsernameFieldLabel
                                   : entry.UserName);
                return ResultsFactory.CreateEntryResult(entry, subTitle, filterCondition.score, ResultAction);
            }));
            return results;
        }, token);
    }

    public Task InitAsync(PluginInitContext context)
    {
        _context = context;
        _settings = _context.API.LoadSettingJsonStorage<Settings>();

        return Task.Run(() => { });
    }

    public Control CreateSettingPanel()
    {
        return new SettingsPanel(OnSettingsChanged, _settings.DecryptValues());
    }

    public List<Result> LoadContextMenus(Result selectedResult)
    {
        return ResultsFactory.CreateContextMenuResults(selectedResult, ResultAction);
    }

    /**
     * <summary>
     * Attempts to load and initialize the database based on the settings parameters provided by the user.
     * </summary>
     */
    private void LoadDatabase()
    {
        try
        {
            if (_db != null) return;

            _context?.API.LogInfo(LogTag, "Attempting to load database");
            // Need to decrypt the encrypted fields before passing them to the KeePass library to load the database
            var decryptedFields = _settings.DecryptValues();

            var normalizedKeyFilePath = string.IsNullOrWhiteSpace(decryptedFields.KeyFileAbsolutePath)
                ? null
                : decryptedFields.KeyFileAbsolutePath;
            _db = KdbxDatabase.Open(decryptedFields.DatabaseAbsolutePath, decryptedFields.DatabasePassword,
                normalizedKeyFilePath);
            _context?.API.LogInfo(LogTag, "Successfully loaded database");
            _context?.API.ShowMsg(Resources.LoadedSuccessfullyLabel, "",
                Constants.ImageKeys.Main);

            if (decryptedFields.CloseDbAfterDuration)
            {
                _closeDbTaskExecutor.Execute(CloseDatabase, decryptedFields.CloseDbDurationMinutes * 60000);
            }
        }
        catch
        {
            _db = null;
            _context?.API.LogError(LogTag, "Opening the KeePass database failed");
            _context?.API.ShowMsg(Resources.FailedLoadingLabel, "", Constants.ImageKeys.Main);
        }
    }

    /**
     * <summary>Callback when the user changes settings that updates the captured settings reference to store the
     * values in Flow Launcher.</summary>
     *
     * <param name="updatedSettings">Settings updated by the user after a change is detected</param>
     */
    private void OnSettingsChanged(Settings updatedSettings)
    {
        var hasDatabaseChanges = !_settings.DatabaseConfigEquals(updatedSettings);
        if (_settings.Equals(updatedSettings)) return;

        _context?.API.LogInfo(LogTag, "Settings changed");

        _settings.EncryptValues(updatedSettings);

        _context?.API.SaveSettingJsonStorage<Settings>();

        if (!hasDatabaseChanges) return;
        _closeDbTaskExecutor.Cancel();
        CloseDatabase();
    }

    /**
     * <summary>
     * Copies an entry's password to the system clipboard, and optionally clears them after a delay.
     * </summary>
     *
     * <param name="text">Value from database to be copied.</param>
     */
    private void ResultAction(string text)
    {
        _context?.API.CopyToClipboard(text,
            false,
            false);

        if (!_settings.ClearClipboard) return;

        _context?.API.LogInfo(LogTag,
            $"Clearing clipboard in {_settings.ClearClipboardDurationSeconds}");
        _clearClipboardTaskExecutor.Execute(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Clipboard.Clear();
                _context?.API.LogInfo(LogTag,
                    "Cleared clipboard");
            });
        }, _settings.ClearClipboardDurationSeconds * 1000);
    }

    private void CloseDatabase()
    {
        if (_db != null)
        {
            _context?.API.ShowMsg(Resources.ClosedFileLabel,
                "", Constants.ImageKeys.Main);
        }

        _db?.Dispose();
        _db = null;
    }
}