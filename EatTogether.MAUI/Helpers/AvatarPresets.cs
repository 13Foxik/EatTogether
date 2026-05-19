namespace EatTogether.MAUI.Helpers
{
    public static class AvatarPresets
    {
        public sealed record AvatarPreset(string Key, string FileName, string DisplayName);

        public static IReadOnlyList<AvatarPreset> All { get; } = new List<AvatarPreset>
        {
            new("fox",    "avatar_fox.png",    "Лис"),
            new("koala",  "avatar_koala.png",  "Коала"),
            new("monkey", "avatar_monkey.png", "Обезьянка"),
            new("pig",    "avatar_pig.png",    "Поросёнок"),
            new("alpaka", "avatar_alpaka.png", "Альпака"),
            new("slon",   "avatar_slon.png",   "Слон"),
        };

        public static string? FileNameForKey(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            foreach (var p in All)
            {
                if (string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase))
                    return p.FileName;
            }

            return null;
        }
    }
}
