using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.ViewModels;
using Firebase.Auth;
using Google.Cloud.Firestore;
using System.Reflection;
using System.Xml.Linq;
using User = EatTogether.MAUI.Models.User;

namespace EatTogether.MAUI.Services
{
    public class FirestoreService : ICloudStoreService
    {
        private FirestoreDb _db;
        private readonly CurrentUserService _currentUserService;

        private const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int ID_LENGTH = 6;
        private const int MAX_ATTEMPTS = 10; // на случай коллизий

        public FirestoreService(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        private async Task SetupFirestore()
        {
            if (_db == null)
            {
                try
                {
                    var stream = await FileSystem.OpenAppPackageFileAsync("admin-sdk.json");
                    using var reader = new StreamReader(stream);
                    var contents = await reader.ReadToEndAsync();

                    _db = new FirestoreDbBuilder() 
                    {
                        ProjectId = "eattogether-79984",
                        ConverterRegistry = new ConverterRegistry
                    {
                        new DateTimeToTimestampConverter()
                    },
                        JsonCredentials = contents
                    }.Build();

                    Console.WriteLine("Firestore initialized successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Firestore initialization failed: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task InsertUserModel(User user)
        {
            await SetupFirestore();
            await _db.Collection("Users").Document(user.Uid).SetAsync(user);
            _currentUserService.SetCurrentUser(user);
            Console.WriteLine($"User {user.Uid} saved to Firestore");
        }

        public async Task UpdateUserFamilies(User user)
        {
            await SetupFirestore();
            var document = _db.Collection("Users").Document(user.Uid);
            var snapshot = await document.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Обновляем только поле UserFamilies
                await document.UpdateAsync("UserFamilies", user.UserFamilies);
                Console.WriteLine($"User {user.Uid} updated - UserFamilies changed");
            }
        }
        public async Task UpdateRequestStatus(MembershipRequest request, RequestStatus status)
        {
            await SetupFirestore();

            // 1. Сначала получим текущий массив
            var document = _db.Collection("Families").Document(request.FamilyId);
            var snapshot = await document.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var memberships = snapshot.GetValue<List<MembershipRequest>>("Memberships")
                                 ?? new List<MembershipRequest>();

                // 2. Найдем и обновим нужный запрос
                var existingRequest = memberships.FirstOrDefault(m => m.Id == request.Id);
                if (existingRequest != null)
                {
                    existingRequest.Status = status;

                    // 3. Полностью заменяем массив
                    await document.UpdateAsync("Memberships", memberships);
                    Console.WriteLine($"Request {request.Id} status updated to {status}");
                }
            }
        }

        public async Task InsertFamilyModel(Family family)
        {
            await SetupFirestore();
            await _db.Collection("Families").Document(family.Id).SetAsync(family);
            Console.WriteLine($"Family {family.Id} saved to Firestore");
        }

        public async Task AddMemberToFamily(string familyId, FamilyMember member)
        {
            await SetupFirestore();
            var documentFamily = _db.Collection("Families").Document(familyId);
            var documentUser = _db.Collection("Users").Document(member.UserId);
            try
            {
                await documentFamily.UpdateAsync("Members", FieldValue.ArrayUnion(member));
                await documentUser.UpdateAsync("UserFamilies", FieldValue.ArrayUnion(familyId));
                Console.WriteLine($"Member {member.UserId} added to family");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка добавления:{ex}");
            }
        }

        public async Task InsertMembership(MembershipRequest request)
        {
            await SetupFirestore();

            var familyDoc = await _db.Collection("Families").Document(request.FamilyId).GetSnapshotAsync();

            if (familyDoc.Exists)
            {
                var family = familyDoc.ConvertTo<Family>();
                bool hasPendingRequest = family.Memberships
                                        .Any(m => m.UserId == request.UserId && m.Status == RequestStatus.Pending);
                if (!hasPendingRequest)
                {
                    await _db.Collection("Families")
                        .Document(request.FamilyId)
                        .UpdateAsync("Memberships", FieldValue.ArrayUnion(request));
                }
                else
                {
                    throw new Exception("Заявка уже подана");
                }
            }
            else
            {
                throw new Exception("Такой семьи не существует");
            }
        }

        public async Task<User?> GetUserModel(string documentId)
        {
            await SetupFirestore();

            DocumentReference docRef = _db.Collection("Users").Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<User>();
            }
            else
            {
                Console.WriteLine($"User document with ID {documentId} not found in Firestore.");
                return null;
            }
        }

        public async Task<Family?> GetFamilyModel(string documentId)
        {
            await SetupFirestore();

            DocumentReference docRef = _db.Collection("Families").Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Family>();
            }
            else
            {
                Console.WriteLine($"Family document with ID {documentId} not found in Firestore.");
                return null;
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await SetupFirestore();

            CollectionReference categoriesRef = _db.Collection("Categories");
            QuerySnapshot snapshot = await categoriesRef.GetSnapshotAsync();

            List<Category> categories = new List<Category>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Category category = document.ConvertTo<Category>();
                    categories.Add(category);
                }
            }

            Console.WriteLine($"Retrieved {categories.Count} categories from Firestore.");
            return categories;
        }

        public async Task<List<FamilyCategory>> GetFamilyCategoriesAsync(string familyId)
        {
            await SetupFirestore();

            CollectionReference famCategoriesRef = _db.Collection("FamilyCategories");
            QuerySnapshot snapshot = await famCategoriesRef.GetSnapshotAsync();

            List<FamilyCategory> famCategories = new List<FamilyCategory>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    FamilyCategory famCategory = document.ConvertTo<FamilyCategory>();
                    if(famCategory.FamilyId == familyId)
                    {
                        famCategories.Add(famCategory);
                    }
                }
            }

            Console.WriteLine($"Retrieved {famCategories.Count} categories from Firestore.");
            return famCategories;
        }

        public async Task CreateSubcategoriesAsync(string categoryId, string familyId, string name)
        {
            await SetupFirestore();
            string id = await GenerateUniqueIdAsync("Subcategories");

            Subcategory subcategory = new Subcategory(id, name, familyId, categoryId);

            await _db.Collection("Subcategories").Document(subcategory.Id).SetAsync(subcategory);
            Console.WriteLine($"Subcategory: {subcategory.Id}- saved to Firestore");
        }

        public async Task<List<Subcategory>> GetSubcategoriesAsync(string categoryId, string familyId)
        {
            await SetupFirestore();

            CollectionReference subcategoriesRef = _db.Collection("Subcategories");
            QuerySnapshot snapshot = await subcategoriesRef.GetSnapshotAsync();

            List<Subcategory> subcategories = new List<Subcategory>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Subcategory subcategory = document.ConvertTo<Subcategory>();
                    if(subcategory.FamilyId == familyId && subcategory.CategoryId == categoryId)
                    {
                        subcategories.Add(subcategory);
                    }
                }
            }

            Console.WriteLine($"Retrieved {subcategories.Count} Subcategories from Firestore.");
            return subcategories;
        }

        public async Task<bool> DeleteSubcategoryFromDBAsync(string subcategoryId)
        {
            List<Dish> dishes = new List<Dish>();
            try
            {
                await SetupFirestore();

                if (string.IsNullOrEmpty(subcategoryId))
                {
                    Console.WriteLine("subcategory ID cannot be null or empty.");
                    return false;
                }

                DocumentReference subcategoryRef = _db.Collection("Subcategories").Document(subcategoryId);
                DocumentSnapshot snapshot = await subcategoryRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    Console.WriteLine($"subcategory with ID {subcategoryId} does not exist.");
                    return false;
                }

                dishes = await GetDishListFromDbAsync(subcategoryId);

                foreach(Dish dish in dishes)
                {
                    await DeleteDishFromDBAsync(dish.Id);
                }

                await subcategoryRef.DeleteAsync();
                Console.WriteLine($"subcategory with ID {subcategoryId} successfully deleted.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting dish with ID {subcategoryId}: {ex.Message}");
                return false;
            }
        }

        public async Task EditSubcategoryFromDBAsync(Subcategory subcategory)
        {
            await SetupFirestore();
            var document = _db.Collection("Subcategories").Document(subcategory.Id);
            var snapshot = await document.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Обновляем только поле UserFamilies
                await document.UpdateAsync("Name", subcategory.Name);
                Console.WriteLine($"Subcategory {subcategory.Id} updated");
            }
        }

        public async Task AddDishToDbAsync(Dish dish)
        {
            await SetupFirestore();
            string id = await GenerateUniqueIdAsync("Dishes");

            dish.Id = id;

            await _db.Collection("Dishes").Document(dish.Id).SetAsync(dish);
            Console.WriteLine($"Dish: {dish.Name}- saved to Firestore");
        }

        public async Task<List<Dish>> GetDishListFromDbAsync(string subcategoryId)
        {
            await SetupFirestore();

            CollectionReference DishesRef = _db.Collection("Dishes");
            QuerySnapshot snapshot = await DishesRef.GetSnapshotAsync();

            List<Dish> Dishes = new List<Dish>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dish Dish = document.ConvertTo<Dish>();
                    if(Dish.SubCategoryId == subcategoryId)
                    {

                        Dishes.Add(Dish);
                    }
                }
            }

            Console.WriteLine($"Retrieved {Dishes.Count} Dishes from Firestore.");
            return Dishes;
        }

        public async Task<bool> DeleteDishFromDBAsync(string dishId)
        {
            try
            {
                await SetupFirestore();

                if (string.IsNullOrEmpty(dishId))
                {
                    Console.WriteLine("Dish ID cannot be null or empty.");
                    return false;
                }

                DocumentReference dishRef = _db.Collection("Dishes").Document(dishId);
                DocumentSnapshot snapshot = await dishRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    Console.WriteLine($"Dish with ID {dishId} does not exist.");
                    return false;
                }

                await dishRef.DeleteAsync();
                Console.WriteLine($"Dish with ID {dishId} successfully deleted.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting dish with ID {dishId}: {ex.Message}");
                return false;
            }
        }

        public async Task<Dish> GetDishAsync(string dishId)
        {
            await SetupFirestore();

            DocumentReference dishRef = _db.Collection("Dishes").Document(dishId);
            DocumentSnapshot document = await dishRef.GetSnapshotAsync();

            if (document.Exists)
            {
                return document.ConvertTo<Dish>();
            }

            return null;
        }

        public async Task EditDishFromDBAsync(Dish dish)
        {
            await SetupFirestore();
            var document = _db.Collection("Dishes").Document(dish.Id);
            var snapshot = await document.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Обновляем только поле UserFamilies
                await document.UpdateAsync("Name", dish.Name);
                await document.UpdateAsync("SubCategoryId", dish.SubCategoryId);
                Console.WriteLine($"Dish {dish.Id} updated");
            }
        }

        public async Task AddPlateToDB(Plate plate)
        {
            await SetupFirestore();
            string id = await GenerateUniqueIdAsync("Plates");

            plate.Id = id;

            await _db.Collection("Plates").Document(plate.Id).SetAsync(plate);
            Console.WriteLine($"Тарелка: {plate.UserId}- saved to Firestore");
        }

        public async Task AddDishOnPlateToDB(DishOnPlate dish)
        {
            await SetupFirestore();
            string id = await GenerateUniqueIdAsync("DishOnPlate");

            dish.Id = id;

            await _db.Collection("DishOnPlate").Document(dish.Id).SetAsync(dish);
            Console.WriteLine($"Тарелка: {dish.DishId}- saved to Firestore");
        }

        public async Task<List<Plate>> GetFamilyPlatesFromDB(string familyId)
        {
            await SetupFirestore();

            CollectionReference PlateRef = _db.Collection("Plates");
            QuerySnapshot snapshot = await PlateRef.GetSnapshotAsync();

            List<Plate> Plates = new List<Plate>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Plate plate = document.ConvertTo<Plate>();
                    if (plate.FamilyId == familyId && plate.Status == RequestStatus.Pending)
                    {
                        Plates.Add(plate);
                    }
                }
            }

            Console.WriteLine($"Retrieved {Plates.Count} Dishes from Firestore.");
            return Plates;
        }
        public async Task EditPlateStatus(Plate plate)
        {
            await SetupFirestore();
            var document = _db.Collection("Plates").Document(plate.Id);
            var snapshot = await document.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Обновляем только поле UserFamilies
                await document.UpdateAsync("Status", plate.Status);
                Console.WriteLine($"plate {plate.Id} updated");
            }
        }

        public async Task<List<Dish>> GetDishesOnPlateFromDb(string PlateId)
        {
            await SetupFirestore();

            CollectionReference DishesRef = _db.Collection("DishOnPlate");
            QuerySnapshot snapshot = await DishesRef.GetSnapshotAsync();

            List<Dish> Dishes = new List<Dish>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    DishOnPlate dishOnPlate = document.ConvertTo<DishOnPlate>();
                    if (dishOnPlate.PlateId == PlateId)
                    {
                        Dishes.Add(await GetDishAsync(dishOnPlate.DishId));
                    }
                }
            }

            Console.WriteLine($"Retrieved {Dishes.Count} Dishes from Firestore.");
            return Dishes;
        }

        public async Task SetFamilyCategoriesModels(List<FamilyCategory> familyCategories)
        {
            await SetupFirestore();

            CollectionReference familyCategoriesRef = _db.Collection("FamilyCategories");

            foreach (var familyCategory in familyCategories)
            {
                if (string.IsNullOrEmpty(familyCategory.FamilyId) || string.IsNullOrEmpty(familyCategory.CategoryId))
                {
                    Console.WriteLine("SKIP: FamilyId or CategoryId is empty");
                    continue;
                }

                try
                {
                    DocumentReference newDocRef = familyCategoriesRef.Document();

                    familyCategory.Id = newDocRef.Id;

                    await newDocRef.SetAsync(familyCategory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ FAILED: {ex.Message}");
                }
            }
        }

        public async Task<string> GenerateUniqueIdAsync(string collection)
        {
            await SetupFirestore();

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                string candidateId = GenerateRandomId(20);

                // Проверяем существует ли такой ID в коллекции
                DocumentReference docRef = _db.Collection(collection).Document(candidateId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return candidateId; // Нашли уникальный ID
                }
            }

            throw new InvalidOperationException($"Could not generate unique ID after {MAX_ATTEMPTS} attempts");
        }

        public async Task<string> GenerateUniqueFamilyIdAsync(string collection)
        {
            await SetupFirestore();
        
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                string candidateId = GenerateRandomId(6);
            
                // Проверяем существует ли такой ID в коллекции
                DocumentReference docRef = _db.Collection(collection).Document(candidateId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            
                if (!snapshot.Exists)
                {
                    return candidateId; // Нашли уникальный ID
                }
            }
        
            throw new InvalidOperationException($"Could not generate unique ID after {MAX_ATTEMPTS} attempts");
        }

    private string GenerateRandomId(int lenght)
    {
        var random = new Random();
        var result = new char[lenght];
        
        for (int i = 0; i < lenght; i++)
        {
            result[i] = CHARACTERS[random.Next(CHARACTERS.Length)];
        }
        
        return new string(result);
    }

    }
}
