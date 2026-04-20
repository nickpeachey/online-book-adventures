namespace OnlineBookAdventures.Application.Common.Interfaces;

/// <summary>
/// Provides file storage operations for story media.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file to storage and returns its public URL.
    /// </summary>
    /// <param name="bucketName">The target bucket name.</param>
    /// <param name="objectKey">The object key (path) within the bucket.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The public URL of the uploaded object.</returns>
    Task<string> UploadAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a pre-signed URL for direct client access to an object.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectKey">The object key within the bucket.</param>
    /// <param name="expiryMinutes">The number of minutes until the URL expires.</param>
    /// <returns>A pre-signed URL string valid for the specified duration.</returns>
    Task<string> GetPresignedUrlAsync(string bucketName, string objectKey, int expiryMinutes = 60);

    /// <summary>
    /// Deletes an object from storage.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectKey">The object key to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DeleteAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);
}
