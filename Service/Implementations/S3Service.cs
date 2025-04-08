using Amazon; 
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Service.Interfaces;

namespace Service.Implementations
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration configuration)
        {
            var awsConfig = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
            };

            _s3Client = new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                awsConfig
            );

            _bucketName = configuration["AWS:BucketName"];
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            try
            {
                var fileKey = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";

                using (var stream = file.OpenReadStream())
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = fileKey,
                        InputStream = stream,
                        ContentType = file.ContentType
                    };

                    await _s3Client.PutObjectAsync(request);

                    // Trả về URL của file
                    return $"https://{_bucketName}.s3.amazonaws.com/{fileKey}";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file to S3: {ex.Message}");
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                var fileKey = fileUrl.Split($"{_bucketName}.s3.amazonaws.com/")[1];

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file from S3: {ex.Message}");
            }
        }
    }
}
