using System.Windows.Input;
using Pidamg.KeePass;

namespace KeepassNativeSearch;

public partial class PreviewPanel
{
    private readonly Entry _entry;

    private bool _hidePassword = true;

    public PreviewPanel(Entry entry)
    {
        _entry = entry;
        InitializeComponent();

        TitleLabel.Content = _entry.Title;
        GroupLabel.Content = _entry.ParentGroup == null
            ? KeepassNativeSearch.Resources.NoGroupLabel
            : _entry.ParentGroup.Name;
        UsernameLabel.Content = _entry.UserName;
        TransformPassword(false);
        UrlLabel.Content = _entry.Url;
        TagsLabel.Content = _entry.Tags;
        NotesTextBlock.Text = _entry.Notes;
    }

    private void TransformPassword(bool isVisible)
    {
        PasswordLabel.Content = isVisible ? _entry.Password : new string('*', _entry.Password.Length);
    }

    private void PasswordLabelOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hidePassword = !_hidePassword;
        TransformPassword(_hidePassword);
    }
}