using Flow.Launcher.Plugin;
using Pidamg.KeePass;

namespace KeepassNativeSearch;

/**
 * <summary>
 * A filter system that gives findings a rank based on the number of complete occurrences and individual search term
 * occurrences.
 * </summary>
 *
 * A complete occurrence is defined as the total query search matching a value found in the set of entry parameters.
 * This is given a significantly higher weight than query term matches.
 *
 * A search term occurrence is defined as an occurrence of a search term in the set of entry parameters. This is
 * given a smaller weight than complete occurrence matches.
 *
 * An entry's rank is sum of each complete occurrence across the enabled entry parameters multiplied by 13, added to the sum of
 * each search term multiplied by 3 and added together for each enabled entry parameter.
 *
 *
 */
public class ScoringEntryFilter(Settings settings, bool caseSensitive = false)
{
    private readonly StringComparison _caseSensitivityComparison =
        caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    private const int CompleteOccurrenceWeight = 13;
    private const int SearchTermOccurrenceWeight = 3;


    /**
     * <summary>
     * Returns true if the search text or search terms occur in the entry.
     * </summary>
     *
     * <param name="searchText">The total query input from the user</param>
     * <param name="searchTerms">The search terms split by spaces from the user input determined by Flow.</param>
     * <param name="entry">The database entry, whose characteristics are being searched through for matches to searchText and searchTerms</param>
     */
    public bool Filter(string searchText, string[] searchTerms, Entry entry)
    {
        return ScoreAndFilter(searchText, searchTerms, entry, true).score > 0;
    }

    /**
     * <summary>
     * Calculates the score of each entry, as well as the entry characteristics that contained any of the provided
     * query.
     * </summary>
     *
     * <param name="searchText">The total query input from the user.</param>
     * <param name="searchTerms">The search terms split by spaces from the user input determined by Flow.</param>
     * <param name="entry">The database entry, whose characteristics are being searched through for matches to searchText and searchTerms.</param>
     * <param name="earlyReturn">Returns as soon as a score is greater than 0, meaning a query match was found.</param>
     */
    public (int score, List<ContainingField> field) ScoreAndFilter(string searchText, string[] searchTerms, Entry entry,
        bool earlyReturn = false)
    {
        var score = 0;
        var list = new List<ContainingField>();

        var titleScore = DoFilterAndScore(settings.SearchTitle, searchText, searchTerms, entry.Title);
        score += titleScore;

        if (titleScore > 0)
        {
            list.Add(ContainingField.Title);
            if (earlyReturn)
            {
                return (score, list);
            }
        }

        var notesScore = DoFilterAndScore(settings.SearchNotes, searchText, searchTerms, entry.Notes);
        score += notesScore;
        if (notesScore > 0)
        {
            list.Add(ContainingField.Notes);
            if (earlyReturn)
            {
                return (score, list);
            }
        }

        var urlScore = DoFilterAndScore(settings.SearchUrl, searchText, searchTerms, entry.Url);
        score += urlScore;
        if (urlScore > 0)
        {
            list.Add(ContainingField.Url);
            if (earlyReturn)
            {
                return (score, list);
            }
        }

        var tagScore = DoFilterAndScore(settings.SearchTags, searchText, searchTerms, entry.Tags);
        score += tagScore;
        if (tagScore > 0)
        {
            list.Add(ContainingField.Tags);
            if (earlyReturn)
            {
                return (score, list);
            }
        }

        var groupScore = DoFilterAndScore(settings.SearchGroups, searchText, searchTerms,
            entry.ParentGroup?.Name ?? string.Empty);
        score += groupScore;
        if (groupScore > 0)
        {
            list.Add(ContainingField.Group);
            if (earlyReturn)
            {
                return (score, list);
            }
        }

        var userNameScore = DoFilterAndScore(settings.SearchUserName, searchText, searchTerms, entry.UserName);
        score += userNameScore;

        if (userNameScore <= 0) return (score >= Result.MaxScore ? Result.MaxScore : score, list);

        list.Add(ContainingField.UserName);

        return earlyReturn ? (score, list) : (score >= Result.MaxScore ? Result.MaxScore : score, list);
    }

    private int DoFilterAndScore(bool fieldEnabled, string searchText, string[] searchTerms, string content)
    {
        if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(content) || !fieldEnabled)
        {
            return 0;
        }

        var completeOccurrencesScore = CountOccurrences(searchText, content) * CompleteOccurrenceWeight;
        var termScores = searchTerms.Aggregate(0,
            (current, term) => current + CountOccurrences(term, content) * SearchTermOccurrenceWeight);

        return completeOccurrencesScore + termScores;
    }

    private int CountOccurrences(string lookup, string source)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(lookup))
        {
            return 0;
        }

        var count = 0;
        var index = 0;

        while ((index = source.IndexOf(lookup, index, _caseSensitivityComparison)) != -1)
        {
            count++;
            index += lookup.Length;
        }

        return count;
    }

    /**
     * <summary>
     * Searchable fields in an entry that are investigated for matching queries provided by the user.
     * </summary>
     */
    public enum ContainingField
    {
        Title,
        Notes,
        Url,
        Tags,
        UserName,
        Group,
        None
    }
}