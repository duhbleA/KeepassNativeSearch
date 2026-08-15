namespace KeepassNativeSearch;

public static class Extensions
{
    /**
     * <summary>Coalesces a string after a cutoff length.</summary>
     *
     * <param name="text">This string</param>
     * <param name="charLengthCutOff">Length of the string before an ellipsis is attached to the end.</param>
     *
     * <returns>A coalesced string</returns>
     */
    public static string Coalesce(this string text, in int charLengthCutOff = 100)
    {
        return text.Length <= charLengthCutOff ? text : text[..charLengthCutOff] + "...";
    }

    /**
     * <summary>The label to show based on filter fields</summary>
     *
     * <param name="field">A KeePass database field searched by user query</param>
     *
     * <returns>A label representing the field queried from a KeePass database.</returns>
     */
    private static string Label(this ScoringEntryFilter.ContainingField field)
    {
        return field switch
        {
            ScoringEntryFilter.ContainingField.Title => Resources.TitleLabel,
            ScoringEntryFilter.ContainingField.Notes => Resources.NoteLabel,
            ScoringEntryFilter.ContainingField.Url => Resources.UrlLabel,
            ScoringEntryFilter.ContainingField.Group => Resources.GroupLabel,
            ScoringEntryFilter.ContainingField.Tags => Resources.TagsLabel,
            ScoringEntryFilter.ContainingField.UserName => Resources.UsernameLabel,
            ScoringEntryFilter.ContainingField.None => Resources.NonApplicableLabel,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
    }
    
    /**
     * <summary>Joins all containing fields into a neat comma-separated list.</summary>
     *
     * <param name="field">List of fields a keyword was found in.</param>
     *
     * <returns>A comma-delimited list of containing fields where keyword entries were found.</returns>
     */
    public static string Label(this List<ScoringEntryFilter.ContainingField> field)
    {
        return string.Join(", ", field.Select(value => value.Label()));
    }
}