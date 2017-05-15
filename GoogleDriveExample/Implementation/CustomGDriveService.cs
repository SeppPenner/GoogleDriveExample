using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using GoogleDriveExample.Events;
using GoogleDriveExample.Interfaces;
using Microsoft.Win32;
using GoogleFile = Google.Apis.Drive.v3.Data.File;
using IOFile = System.IO.File;

namespace GoogleDriveExample.Implementation
{
    /// <summary>
    ///     <inheritdoc cref="ICustomGDriveService" />
    /// </summary>
    public sealed class CustomGDriveService : ICustomGDriveService
    {
        private readonly string[] _scopes =
        {
            DriveService.Scope.Drive,
            DriveService.Scope.DriveAppdata,
            DriveService.Scope.DriveFile,
            DriveService.Scope.DriveMetadata,
            DriveService.Scope.DriveMetadataReadonly,
            DriveService.Scope.DrivePhotosReadonly,
            DriveService.Scope.DriveReadonly,
            DriveService.Scope.DriveScripts
        };

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public long GetQuotaUsed(DriveService service)
        {
            var ag = new AboutResource.GetRequest(service) {Fields = "user,storageQuota"};
            var response = ag.Execute();
            if (response.StorageQuota.Usage.HasValue)
                return response.StorageQuota.Usage.Value;
            return -1;
        }

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public long GetQuotaTotal(DriveService service)
        {
            var ag = new AboutResource.GetRequest(service) {Fields = "user,storageQuota"};
            var response = ag.Execute();
            if (response.StorageQuota.Limit.HasValue)
                return response.StorageQuota.Limit.Value;
            return -1;
        }

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public string UploadToGDrive(DriveService service, string uploadFile, string parent)
        {
            var byteArray = IOFile.ReadAllBytes(uploadFile);
            var stream = new MemoryStream(byteArray);
            var request = service.Files.Create(GetBody(uploadFile, parent), stream, GetMimeType(uploadFile));
            request.ProgressChanged += UploadProgessChanged;
            request.ResponseReceived += UploadSuccessfull;
            request.Upload();
            var response = request.ResponseBody;
            CreatePermissionForFile(service, response.Id);
            return "https://drive.google.com/open?id=" + response.Id;
        }

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public string GetRootFolderId(DriveService service)
        {
            return service.Files.Get("root").FileId;
        }

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public DriveService GetDriveService(string clientId, string clientSecret, string userName)
        {
            var credential =
                GoogleWebAuthorizationBroker.AuthorizeAsync(GetClientSecrets(clientId, clientSecret), _scopes, userName,
                    CancellationToken.None, new FileDataStore(Assembly.GetExecutingAssembly().FullName)).Result;
            return GetService(credential);
        }

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public event EventHandler OnUploadProgessChanged;

        /// <summary>
        ///     <inheritdoc cref="ICustomGDriveService" />
        /// </summary>
        public event EventHandler OnUploadSuccessfull;

        private void UploadSuccessfull(GoogleFile file)
        {
            UploadSuccessfull(new UploadFinishedEventArgs(file.Name));
        }

        private void UploadProgessChanged(IUploadProgress progress)
        {
            UploadProgessChanged(new UploadProgessChangedEventArgs(progress.Status, progress.BytesSent));
        }

        private void UploadProgessChanged(EventArgs e)
        {
            var handler = OnUploadProgessChanged;
            handler?.Invoke(this, e);
        }

        private void UploadSuccessfull(EventArgs e)
        {
            var handler = OnUploadSuccessfull;
            handler?.Invoke(this, e);
        }

        private GoogleFile GetBody(string uploadFile, string parent)
        {
            var file = new GoogleFile
            {
                Name = Path.GetFileName(uploadFile),
                Description = uploadFile,
                MimeType = GetMimeType(uploadFile),
                Parents = new List<string> {parent}
            };
            return file;
        }

        private DriveService GetService(UserCredential credential)
        {
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().FullName
            });
        }

        private ClientSecrets GetClientSecrets(string clientId, string clientSecret)
        {
            return new ClientSecrets {ClientId = clientId, ClientSecret = clientSecret};
        }

        private string GetMimeType(string fileName)
        {
            var mimeType = "application/unknown";
            var extension = Path.GetExtension(fileName);
            if (extension == null) return mimeType;
            var ext = extension.ToLower();
            var regKey = Registry.ClassesRoot.OpenSubKey(ext);
            return regKey?.GetValue("Content Type") == null ? mimeType : regKey.GetValue("Content Type").ToString();
        }

        private void CreatePermissionForFile(DriveService driveService, string fileId)
        {
            var everonePermission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            var request = driveService.Permissions.Create(everonePermission, fileId);
            request.Execute();
        }
    }
}