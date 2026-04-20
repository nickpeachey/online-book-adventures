using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Storage;

/// <summary>
/// Implements <see cref="IStorageService"/> using Amazon S3 (or MinIO-compatible S3 API).
/// </summary>
internal sealed class S3StorageService(IAmazonS3 s3Client, ILogger<S3StorageService> logger) : IStorageService
{
    /// <inheritdoc/>
    public async Task<string> UploadAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucketName, cancellationToken).ConfigureAwait(false);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await s3Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Uploaded object {Key} to bucket {Bucket}", objectKey, bucketName);

        // Return a direct URL (works for MinIO with public-read ACL)
        var config = (AmazonS3Config)s3Client.Config;
        var serviceUrl = config.ServiceURL?.TrimEnd('/') ?? "http://localhost:9000";
        return $"{serviceUrl}/{bucketName}/{objectKey}";
    }

    /// <inheritdoc/>
    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectKey, int expiryMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Verb = HttpVerb.PUT
        };

        return await Task.FromResult(s3Client.GetPreSignedURL(request)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };

        await s3Client.DeleteObjectAsync(request, cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Deleted object {Key} from bucket {Bucket}", objectKey, bucketName);
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is AmazonS3Exception s3Ex &&
            (s3Ex.ErrorCode == "BucketAlreadyOwnedByYou" || s3Ex.ErrorCode == "BucketAlreadyExists"))
        {
            // Bucket already exists — safe to ignore
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not ensure bucket {Bucket} exists", bucketName);
        }
    }
}
