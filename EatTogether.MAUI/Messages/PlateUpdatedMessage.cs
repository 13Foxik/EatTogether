namespace EatTogether.MAUI.Messages
{
    public enum PlateUpdateAction
    {
        Added,   // тарелка отправлена (+1)
        Removed  // тарелка удалена (-1)
    }

    public class PlateUpdatedMessage
    {
        public PlateUpdateAction Action { get; }

        public PlateUpdatedMessage(PlateUpdateAction action = PlateUpdateAction.Added)
        {
            Action = action;
        }
    }
}
