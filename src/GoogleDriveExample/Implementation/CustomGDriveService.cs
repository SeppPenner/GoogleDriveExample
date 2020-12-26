// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomGDriveService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The Google <see cref="CustomGDriveService" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GoogleDriveExample.Implementation
{
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

    /// <inheritdoc cref="ICustomGDriveService"/>
    /// <summary>
    ///     The Google <see cref="CustomGDriveService" /> class.
    /// </summary>
    /// <seealso cref="ICustomGDriveService"/>
    public sealed class CustomGDriveService : ICustomGDriveService
    {
        /// <summary>
        /// The default MIME type constant.
        /// </summary>
        private const string DefaultMimeType = "application/unknown";

        /// <summary>
        /// The content type constant.
        /// </summary>
        private const string ContentType = "Content Type";

        /// <summary>
        /// The storage quota constant.
        /// </summary>
        private const string StorageQuota = "user,storageQuota";

        /// <summary>
        /// The root identifier constant.
        /// </summary>
        private const string RootIdentifier = "root";

        /// <summary>
        /// The permission type constant.
        /// </summary>
        private const string PermissionType = "anyone";

        /// <summary>
        /// The permission role constant.
        /// </summary>
        private const string PermissionRole = "reader";

        /// <summary>
        /// The Google Drive Url.
        /// </summary>
        private const string GoogleDriveUrl = "https://drive.google.com/open?id=";

        /// <summary>
        /// The scopes.
        /// </summary>
        private readonly string[] scopes =
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

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Called when the upload process is changed.
        /// </summary>
        /// <seealso cref="ICustomGDriveService"/>
        public event EventHandler OnUploadProgressChanged;

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Called when the upload process is finished.
        /// </summary>
        /// <seealso cref="ICustomGDriveService"/>
        public event EventHandler OnUploadSuccessful;

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Gets the used quota of the account.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <returns>The used quota of the account.</returns>
        /// <seealso cref="ICustomGDriveService"/>
        public long GetQuotaUsed(DriveService service)
        {
            var ag = new AboutResource.GetRequest(service) { Fields = StorageQuota };
            var response = ag.Execute();

            if (response.StorageQuota.Usage.HasValue)
            {
                return response.StorageQuota.Usage.Value;
            }

            return -1;
        }

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Gets the total quota (is 15 GB) of the account.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <returns>The total quota of the account.</returns>
        /// <seealso cref="ICustomGDriveService"/>
        public long GetQuotaTotal(DriveService service)
        {
            var ag = new AboutResource.GetRequest(service) { Fields = StorageQuota };
            var response = ag.Execute();

            if (response.StorageQuota.Limit.HasValue)
            {
                return response.StorageQuota.Limit.Value;
            }

            return -1;
        }

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Uploads a file to GDrive with "everyone that has the link can read the file" rights.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <param name="uploadFile">The File that should be uploaded.</param>
        /// <param name="parent">The parent folder.</param>
        /// <returns>The download link to the file.</returns>
        /// <seealso cref="ICustomGDriveService"/>
        public string UploadToGDrive(DriveService service, string uploadFile, string parent)
        {
            var byteArray = IOFile.ReadAllBytes(uploadFile);
            var stream = new MemoryStream(byteArray);
            var request = service.Files.Create(this.GetBody(uploadFile, parent), stream, this.GetMimeType(uploadFile));
            request.ProgressChanged += this.UploadProgressChanged;
            request.ResponseReceived += this.UploadSuccessful;
            request.Upload();
            var response = request.ResponseBody;
            CreatePermissionForFile(service, response.Id);
            return $"{GoogleDriveUrl}{response.Id}";
        }

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Gets the root folder id of the account.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <returns>The root folder id for the account.</returns>
        /// <seealso cref="ICustomGDriveService"/>
        public string GetRootFolderId(DriveService service)
        {
            return service.Files.Get(RootIdentifier).FileId;
        }

        /// <inheritdoc cref="ICustomGDriveService"/>
        /// <summary>
        ///     Gets the <see cref="DriveService" /> needed in the methods above.
        /// </summary>
        /// <param name="clientId">The client id that needs to be set inside the Google account (API-Key).</param>
        /// <param name="clientSecret">The client secret/ password.</param>
        /// <param name="userName">The client username.</param>
        /// <returns>The <see cref="DriveService" /> needed to authenticate the above methods.</returns>
        /// <seealso cref="ICustomGDriveService"/>
        public DriveService GetDriveService(string clientId, string clientSecret, string userName)
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GetClientSecrets(clientId, clientSecret),
                this.scopes,
                userName,
                CancellationToken.None,
                new FileDataStore(Assembly.GetExecutingAssembly().FullName)).Result;
            return GetService(credential);
        }

        /// <summary>
        /// Creates the permissions for the file.
        /// </summary>
        /// <param name="driveService">The drive service.</param>
        /// <param name="fileId">The file identifier.</param>
        private static void CreatePermissionForFile(DriveService driveService, string fileId)
        {
            var everyonePermission = new Permission
            {
                Type = PermissionType,
                Role = PermissionRole
            };

            var request = driveService.Permissions.Create(everyonePermission, fileId);
            request.Execute();
        }

        /// <summary>
        /// Gets the drive service.
        /// </summary>
        /// <param name="credential">The credentials.</param>
        /// <returns>A new <see cref="DriveService"/>.</returns>
        private static DriveService GetService(UserCredential credential)
        {
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().FullName
            });
        }

        /// <summary>
        /// Gets the client secrets.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns>The <see cref="ClientSecrets"/>.</returns>
        private static ClientSecrets GetClientSecrets(string clientId, string clientSecret)
        {
            return new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret };
        }

        /// <summary>
        /// Handles the upload successful event.
        /// </summary>
        /// <param name="file">The file.</param>
        private void UploadSuccessful(GoogleFile file)
        {
            this.UploadSuccessful(new UploadFinishedEventArgs(file.Name));
        }

        /// <summary>
        /// Handles the upload successful event.
        /// </summary>
        /// <param name="e">The event args.</param>
        private void UploadSuccessful(EventArgs e)
        {
            var handler = this.OnUploadSuccessful;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Handles the upload progress changed event.
        /// </summary>
        /// <param name="progress">The progress.</param>
        private void UploadProgressChanged(IUploadProgress progress)
        {
            this.UploadProgressChanged(new UploadProgressChangedEventArgs(progress.Status, progress.BytesSent));
        }

        /// <summary>
        /// Handles the upload progress changed event.
        /// </summary>
        /// <param name="e">The event args.</param>
        private void UploadProgressChanged(EventArgs e)
        {
            var handler = this.OnUploadProgressChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Gets the file body.
        /// </summary>
        /// <param name="uploadFile">The upload file.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>The body of the <see cref="GoogleFile"/>.</returns>
        private GoogleFile GetBody(string uploadFile, string parent)
        {
            var file = new GoogleFile
            {
                Name = Path.GetFileName(uploadFile),
                Description = uploadFile,
                MimeType = this.GetMimeType(uploadFile),
                Parents = new List<string> { parent }
            };

            return file;
        }

        /// <summary>
        /// Gets the MIME type.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The MIME type.</returns>
        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (extension == null)
            {
                return DefaultMimeType;
            }

            var ext = extension.ToLower();
            var regKey = Registry.ClassesRoot.OpenSubKey(ext);
            // ReSharper disable once PossibleNullReferenceException
            return regKey?.GetValue(ContentType) == null ? DefaultMimeType : regKey.GetValue(ContentType).ToString();
        }
    }
}