using EatTogether.MAUI.Models;
using User = EatTogether.MAUI.Models.User;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using Firebase.Auth;
using Google.Cloud.Firestore;
using System.Reflection;

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
            var document = _db.Collection("Families").Document(familyId);
            try
            {
                await document.UpdateAsync("Members", FieldValue.ArrayUnion(member));
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


        public async Task<string> GenerateUniqueIdAsync(string collection)
    {
        await SetupFirestore();
        
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            string candidateId = GenerateRandomId();
            
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

    private string GenerateRandomId()
    {
        var random = new Random();
        var result = new char[ID_LENGTH];
        
        for (int i = 0; i < ID_LENGTH; i++)
        {
            result[i] = CHARACTERS[random.Next(CHARACTERS.Length)];
        }
        
        return new string(result);
    }

    }
}
