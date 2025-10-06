using Google.Cloud.Firestore;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services;

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

        public async Task UpdateUserModel(User user)
        {

        }

        public async Task InsertUserModel(User user)
        {
            await SetupFirestore();
            await _db.Collection("Users").Document(user.Uid).SetAsync(user);
            _currentUserService.SetCurrentUser(user);
            Console.WriteLine($"User {user.Uid} saved to Firestore");
        }

        public async Task InsertFamilyModel(Family family)
        {
            await SetupFirestore();
            await _db.Collection("Families").Document(family.Id).SetAsync(family);
            Console.WriteLine($"User {family.Id} saved to Firestore");
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
