using CommunityToolkit.Mvvm.ComponentModel;
using Google.Cloud.Firestore;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public partial class Dish : ObservableObject
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public string SubCategoryId { get; set; }

        // ObservableProperty — UI обновляется без замены объекта в коллекции
        [ObservableProperty]
        private bool _isInPlate;

        private RequestStatus _status;
        public RequestStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string dishOnPlateId { get; set; }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private Color _statusColor;
        public Color StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        private Color _buttonBackgroundColor;
        public Color ButtonBackgroundColor
        {
            get => _buttonBackgroundColor;
            set => SetProperty(ref _buttonBackgroundColor, value);
        }

        private Color _buttonTextColor;
        public Color ButtonTextColor
        {
            get => _buttonTextColor;
            set => SetProperty(ref _buttonTextColor, value);
        }

        private bool _isStatusVisible;
        public bool IsStatusVisible
        {
            get => _isStatusVisible;
            set => SetProperty(ref _isStatusVisible, value);
        }

        private bool _canShowActions = true;
        public bool CanShowActions
        {
            get => _canShowActions;
            set => SetProperty(ref _canShowActions, value);
        }

        public Dish() { }
        public Dish(string name, string familyId, string subcategoryId)
        {
            Name = name;
            FamilyId = familyId;
            SubCategoryId = subcategoryId;
        }
    }
}
