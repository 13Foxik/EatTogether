namespace EatTogether.MAUI.Helpers
{
    public static class AvatarColors
    {
        public sealed record AvatarColorPreset(string Hex, string Name);

        public static IReadOnlyList<AvatarColorPreset> All { get; } = new List<AvatarColorPreset>
        {
            new("#1F744D", "Зелёный"),
            new("#2196F3", "Синий"),
            new("#9C27B0", "Фиолетовый"),
            new("#F44336", "Красный"),
            new("#FF9800", "Оранжевый"),
            new("#607D8B", "Серый"),
        };
    }
}
