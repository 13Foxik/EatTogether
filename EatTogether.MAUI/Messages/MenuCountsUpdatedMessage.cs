namespace EatTogether.MAUI.Messages
{
    public class MenuCountsUpdatedMessage
    {
        public string CategoryId { get; }
        public string SubcategoryId { get; }
        public int DishCountDelta { get; }
        public int SubcategoryCountDelta { get; }

        public MenuCountsUpdatedMessage(
            string categoryId = null,
            string subcategoryId = null,
            int dishCountDelta = 0,
            int subcategoryCountDelta = 0)
        {
            CategoryId = categoryId ?? string.Empty;
            SubcategoryId = subcategoryId ?? string.Empty;
            DishCountDelta = dishCountDelta;
            SubcategoryCountDelta = subcategoryCountDelta;
        }
    }
}
