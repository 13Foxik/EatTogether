using Google.Cloud.Firestore;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class FamilyCategory
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public string CategoryId { get; set; }

        [FirestoreProperty]
        public bool IsEnabled { get; set; }

        [FirestoreProperty]
        public int SortOrder { get; set; }

        public FamilyCategory() { }

        public FamilyCategory(string familyId, string categoryId, bool isEnabled, int sortOrder)
        {
            FamilyId = familyId;
            CategoryId = categoryId;
            IsEnabled = isEnabled;
            SortOrder = sortOrder;
        }
    }

    // Конвертор для списка FamilyCategory
    public class FamilyCategoryListConverter : IFirestoreConverter<List<FamilyCategory>>
    {
        public object ToFirestore(List<FamilyCategory> value)
        {
            if (value == null || !value.Any())
                return new List<object>();

            return value.Select(category => new Dictionary<string, object>
            {
                ["Id"] = category.FamilyId ?? "",
                ["FamilyId"] = category.FamilyId ?? "",
                ["CategoryId"] = category.CategoryId ?? "",
                ["IsEnabled"] = category.IsEnabled,
                ["SortOrder"] = category.SortOrder
            }).ToList();
        }

        public List<FamilyCategory> FromFirestore(object value)
        {
            var categories = new List<FamilyCategory>();

            if (value is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        var category = new FamilyCategory
                        {
                            Id = dict.ContainsKey("Id") ? dict["Id"]?.ToString() : "",
                            FamilyId = dict.ContainsKey("FamilyId") ? dict["FamilyId"]?.ToString() : "",
                            CategoryId = dict.ContainsKey("CategoryId") ? dict["CategoryId"]?.ToString() : "",
                            IsEnabled = dict.ContainsKey("IsEnabled") && (bool)dict["IsEnabled"],
                            SortOrder = dict.ContainsKey("SortOrder") ? Convert.ToInt32(dict["SortOrder"]) : 0
                        };
                        categories.Add(category);
                    }
                }
            }

            return categories;
        }
    }
}