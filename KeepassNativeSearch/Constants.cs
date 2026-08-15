namespace KeepassNativeSearch;

public static class Constants
{
    /**
     * Keywords typed by the user for specific application behavior.
     */
    public static class UserEntryConstants
    {
        public const string DatabaseControlKeyword = "db";
    }

    /**
     * Image keys to show for different kinds of results.
     */
    public static class ImageKeys
    {
        private const string ImagePrefix = "Images\\";
        public const string Main = ImagePrefix + "keepassdb.png";
        public const string Title = ImagePrefix + "title.png";
        public const string Username = ImagePrefix + "username.png";
        public const string Password = ImagePrefix + "password.png";
        public const string Url = ImagePrefix + "url.png";
        public const string Tag = ImagePrefix + "tag.png";
        public const string Note = ImagePrefix + "note.png";
        public const string Entry = ImagePrefix + "key.png";
        public const string Control = ImagePrefix + "db.png";
    }
}