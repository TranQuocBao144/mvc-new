using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DXWebApplication4.Services
{
    public class FirebaseService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseService(string bucketName, string serviceAccountPath)
        {
            _bucketName = bucketName;
            var credential = GoogleCredential.FromFile(serviceAccountPath);

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential
                });
            }
            _storageClient = StorageClient.Create(credential);
        }

        private string CreateFirebaseUrl(string objectName, string token)
        {
            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media&token={token}";
        }
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath = "images")
        {
            return await InternalUploadFileAsync(fileStream, fileName, folderPath);
        }

        public async Task<string> UploadFileNoteAsync(Stream fileStream, string fileName, string folderPath = "notes")
        {
            return await InternalUploadFileAsync(fileStream, fileName, folderPath);
        }

        private async Task<String>InternalUploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            var objectName = $"{folderPath}/{Guid.NewGuid()}_{fileName}";
            var firebaseToken = Guid.NewGuid().ToString();
            var obj = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _bucketName,
                Name = objectName,
                Metadata = new Dictionary<string, string>
                {
                    { 
                        "firebaseStorageDownloadTokens", firebaseToken 
                    }
                }
            };
            await _storageClient.UploadObjectAsync(obj, fileStream);
            return CreateFirebaseUrl(objectName, firebaseToken);
        }
    }
}
