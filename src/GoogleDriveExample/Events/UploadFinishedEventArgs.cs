// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UploadFinishedEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The <see cref="UploadFinishedEventArgs" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GoogleDriveExample.Events;

/// <summary>
///     The <see cref="UploadFinishedEventArgs" /> class.
/// </summary>
public class UploadFinishedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UploadFinishedEventArgs"/> class.
    /// </summary>
    /// <param name="fileName">The uploaded file name.</param>
    public UploadFinishedEventArgs(string fileName)
    {
        this.FileName = fileName;
    }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    private string FileName { get; }

    /// <summary>
    ///     Gets the <see cref="UploadFinishedEventArgs" /> status.
    /// </summary>
    /// <returns>The uploaded file name of the <see cref="UploadFinishedEventArgs" />.</returns>
    public string GetStatus()
    {
        return this.FileName;
    }
}
