using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Pidamg.KeePass;

namespace KeepassNativeSearch;

public static class ResultsFactory
{
    /**
     * <summary>Creates the context menu results based on a user selected result.</summary>
     *
     * <param name="selectedResult">Result selected by the user.</param>
     * <param name="resultAction">The action to be performed if the result is selected.</param>
     *
     * <returns>The context menu for the selected result by the user.</returns>
     */
    public static List<Result> CreateContextMenuResults(Result selectedResult, Action<string> resultAction)
    {
        var list = new List<Result>();
        if (selectedResult.ContextData is not Entry entry)
        {
            return list;
        }

        var titleResult = new Result
        {
            Title = Resources.TitleLabel,
            SubTitle = entry.Title,
            CopyText = entry.Title,
            IcoPath = Constants.ImageKeys.Title,
            Action = _ =>
            {
                resultAction(entry.Title);
                return true;
            }
        };
        list.Add(titleResult);
        var usernameResult = new Result
        {
            Title = Resources.UsernameLabel,
            SubTitle = entry.UserName,
            CopyText = entry.UserName,
            IcoPath = Constants.ImageKeys.Username,
            Action = _ =>
            {
                resultAction(entry.UserName);
                return true;
            }
        };
        list.Add(usernameResult);

        var passwordResult = new Result
        {
            Title = Resources.PasswordLabel,
            SubTitle = new string('*', entry.Password.Length),
            CopyText = entry.Password,
            IcoPath = Constants.ImageKeys.Password,
            Action = _ =>
            {
                resultAction(entry.Password);
                return true;
            }
        };
        list.Add(passwordResult);

        var urlResult = new Result
        {
            Title = Resources.UrlLabel,
            SubTitle = entry.Url,
            CopyText = entry.Url,
            IcoPath = Constants.ImageKeys.Url,
            Action = _ =>
            {
                resultAction(entry.Url);
                return true;
            }
        };
        list.Add(urlResult);

        var tagsResult = new Result
        {
            Title = Resources.TagsLabel,
            SubTitle = entry.Tags,
            CopyText = entry.Tags,
            IcoPath = Constants.ImageKeys.Tag,
            Action = _ =>
            {
                resultAction(entry.Tags);
                return true;
            }
        };
        list.Add(tagsResult);

        var notesResult = new Result
        {
            Title = Resources.NoteLabel,
            SubTitle = entry.Notes.Coalesce().Replace("\n", " "),
            CopyText = entry.Notes,
            IcoPath = Constants.ImageKeys.Note,
            Action = _ =>
            {
                resultAction(entry.Notes);
                return true;
            }
        };
        list.Add(notesResult);

        return list;
    }

    /**
     * <summary>Creates a result representing an entry in a KeePass database.</summary>
     *
     * <param name="entry">Entry in the KeePass database.</param>
     * <param name="score">The result score, which determines result ordering based on result accuracy to user query.</param>
     * <param name="subTitle">Subtitle of the result to be displayed</param>
     * <param name="resultAction">An action performed when the user selects the entry result.</param>
     */
    public static Result CreateEntryResult(Entry entry, string subTitle, int score, Action<string> resultAction)
    {
        return new Result
        {
            Title = entry.Title.Coalesce(),
            SubTitle = subTitle,
            IcoPath = Constants.ImageKeys.Entry,
            CopyText = entry.Password,
            PreviewPanel = new Lazy<UserControl>(() => new PreviewPanel(entry)),
            Action = _ =>
            {
                resultAction(entry.Password);
                return true;
            },
            ContextData = entry,
            Score = score
        };
    }

    /**
     * <summary>A list of controls presented to the user when the control keyword is typed in</summary>
     *
     * <param name="canTryFileOpen">True if a load database attempt can be made based on user settings</param>
     * <param name="openAction">The action performed if an open result is selected by the user</param>
     * <param name="closeAction">The action performed if a close result is selected by the user</param>
     *
     * <returns>A list of results that control the plugin</returns>
     */
    public static List<Result> CreateControlResults(bool canTryFileOpen, Action openAction, Action closeAction)
    {
        var results = new List<Result>();
        if (canTryFileOpen)
        {
            results.Add(new Result
            {
                Title = Resources.OpenFileLabel,
                SubTitle =
                    Resources.OpenFileSubtitleLabel,
                IcoPath = Constants.ImageKeys.Control,
                Action = _ =>
                {
                    openAction();
                    return true;
                },
                Score = 2
            });
            results.Add(new Result
            {
                Title = Resources.CloseFileLabel,
                SubTitle = Resources.CloseFileSubtitleLabel,
                IcoPath = Constants.ImageKeys.Control,
                Action = _ =>
                {
                    closeAction();
                    return true;
                },
                Score = 1
            });
        }
        else
        {
            results.Add(new Result
            {
                Title = Resources.NoFileConfigLabel,
                SubTitle = Resources.NoFileConfigSubtitleLabel,
                IcoPath = Constants.ImageKeys.Control
            });
        }

        return results;
    }

    /**
     * A result shown to the user to denote the status of the KeePass database.
     *
     * <param name="isKeepassFileOpen">True if the user has enough settings configured to try and load the database</param>
     *
     * <returns>A result showing whether the database is opened or closed</returns>
    **/
    public static Result CreateKeepassFileStatusResult(bool isKeepassFileOpen)
    {
        return new Result
        {
            Title = Resources.FileStatusLabel,
            SubTitle = $"{(isKeepassFileOpen ? Resources.OpenStatusLabel : Resources.ClosedStatusLabel)}",
            IcoPath = Constants.ImageKeys.Control,
            Score = 0
        };
    }
}