// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UploadProgressChangedEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The <see cref="UploadProgressChangedEventArgs" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GoogleDriveExample.Events;

/// <summary>
///     The <see cref="UploadProgressChangedEventArgs" /> class.
/// </summary>
public class UploadProgressChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UploadProgressChangedEventArgs"/> class.
    /// </summary>
    /// <param name="status">The upload status.</param>
    /// <param name="bytesSent">The bytes already sent.</param>
    public UploadProgressChangedEventArgs(UploadStatus status, long bytesSent)
    {
        this.Status = status;
        this.BytesSent = bytesSent;
    }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the status.
    /// </summary>
    private UploadStatus Status { get; }

    /// <summary>
    /// Gets the bytes sent.
    /// </summary>
    private long BytesSent { get; }

    /// <summary>
    ///     Gets the <see cref="UploadProgressChangedEventArgs" /> status.
    /// </summary>
    /// <returns>The status of the <see cref="UploadProgressChangedEventArgs" />.</returns>
    public UploadStatus GetStatus()
    {
        return this.Status;
    }

    /// <summary>
    ///     Gets the <see cref="UploadProgressChangedEventArgs" /> sent bytes.
    /// </summary>
    /// <returns>The sent bytes of the <see cref="UploadProgressChangedEventArgs" />.</returns>
    public long GetSentBytes()
    {
        return this.BytesSent;
    }
}
