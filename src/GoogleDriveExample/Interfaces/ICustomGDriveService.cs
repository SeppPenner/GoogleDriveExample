// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICustomGDriveService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The Google <see cref="CustomGDriveService" /> interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GoogleDriveExample.Interfaces
{
    using System;

    using Google.Apis.Drive.v3;

    using GoogleDriveExample.Implementation;

    /// <summary>
    ///     The Google <see cref="CustomGDriveService" /> interface.
    /// </summary>
    public interface ICustomGDriveService
    {
        /// <summary>
        ///     Called when the upload process is changed.
        /// </summary>
        event EventHandler? OnUploadProgressChanged;

        /// <summary>
        ///     Called when the upload process is finished.
        /// </summary>
        event EventHandler? OnUploadSuccessful;

        /// <summary>
        ///     Gets the used quota of the account.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <returns>The used quota of the account.</returns>
        long GetQuotaUsed(DriveService service);

        /// <summary>
        ///     Gets the total quota (is 15 GB) of the account.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <returns>The total quota of the account.</returns>
        long GetQuotaTotal(DriveService service);

        /// <summary>
        ///     Uploads a file to GDrive with "everyone that has the link can read the file" rights.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <param name="uploadFile">The File that should be uploaded.</param>
        /// <param name="parent">The parent folder.</param>
        /// <returns>The download link to the file.</returns>
        string UploadToGDrive(DriveService service, string uploadFile, string parent);

        /// <summary>
        ///     Gets the root folder id of the account.
        /// </summary>
        /// <param name="service">The <see cref="DriveService" /> that is needed.</param>
        /// <returns>The root folder id for the account.</returns>
        string GetRootFolderId(DriveService service);

        /// <summary>
        ///     Gets the <see cref="DriveService" /> needed in the methods above.
        /// </summary>
        /// <param name="clientId">The client id that needs to be set inside the Google account (API-Key).</param>
        /// <param name="clientSecret">The client secret/ password.</param>
        /// <param name="userName">The client username.</param>
        /// <returns>The <see cref="DriveService" /> needed to authenticate the above methods.</returns>
        DriveService GetDriveService(string clientId, string clientSecret, string userName);
    }
}