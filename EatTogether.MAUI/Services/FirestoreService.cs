using Google.Cloud.Firestore;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services
{
    public class FirestoreService : ICloudStoreService
    {
        private FirestoreDb _db;

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
            Console.WriteLine($"User {user.Uid} saved to Firestore");
        }
        public async Task<User?> GetUserModels(string documentId)
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

    }
}
