using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configure AWS S3 Client
var awsS3Client = new AmazonS3Client();
builder.Services.AddSingleton<IAmazonS3>(awsS3Client);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/upload", async (IFormFile file, IAmazonS3 s3Client) =>
{
    if (file != null && file.Length > 0)
    {
        try
        {
            // Generate a unique key for the S3 object (you can customize this)
            var key = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Specify the S3 bucket name
            var bucketName = "my-youtube-sample-bucket";

            // Upload the file to S3
            using var fileStream = file.OpenReadStream();
            var fileTransferUtility = new TransferUtility(s3Client);
            await fileTransferUtility.UploadAsync(fileStream, bucketName, key);

            // Construct the S3 URL for the uploaded file
            var s3Url = $"https://{bucketName}.s3.amazonaws.com/{key}";

            return Results.Ok(new { Message = "Image uploaded successfully", S3Url = s3Url });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Message = "Error uploading image", Error = ex.Message });
        }
    }

    return Results.BadRequest(new { Message = "No file uploaded" });
});

app.UseHttpsRedirection();
app.Run();
