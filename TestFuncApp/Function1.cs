//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Logging;

//namespace TestFuncApp
//{
//    public class Function1
//    {
//        private readonly ILogger<Function1> _logger;

//        public Function1(ILogger<Function1> logger)
//        {
//            _logger = logger;
//        }

//        [Function(nameof(Function1))]
//        public async Task Run([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name,
//            FunctionContext context)

//        {
//            using var blobStreamReader = new StreamReader(stream);
//            var content = await blobStreamReader.ReadToEndAsync();
//            _logger.LogInformation($"C# Blob Trigger (using Event Grid) processed blob\n Name: {name} \n Data: {content}");


//            var metadata = context.BindingContext.BindingData;
//            if (metadata.TryGetValue("Url", out var blobUri))
//            {
//                _logger.LogInformation($"Blob Uri: {blobUri}");
//            }

//            if (metadata.TryGetValue("Properties", out var properties))
//            {
//                var infoObj = 0;
//            }
//        }
//        [Function("OutputFunction")]
//        [BlobOutput("imagesorginal/{name}", Connection = "AzureWebJobsStorage")]
//        public async Task<byte[]> RunOut([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] byte[] inputBytes, string name)
//        {
//            if (name.EndsWith(".png"))
//            {
//                return inputBytes;
//            }
//            return null;
//        }

//    }
//}

using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace TestFuncApp
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        public async Task Run([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name,
            FunctionContext context)
        {
           
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray(); 

            _logger.LogInformation($"C# Blob Trigger (Event Grid istifadə edərək) blob-u işləyib\n Adı: {name} \n Məzmunun ölçüsü: {content.Length} bayt");

            var metadata = context.BindingContext.BindingData;
            if (metadata.TryGetValue("Url", out var blobUri))
            {
                _logger.LogInformation($"Blob URL: {blobUri}");
            }

            if (metadata.TryGetValue("Properties", out var properties))
            {
                var infoObj = 0;
            }
        }

        [Function("OutputFunction")]
        [BlobOutput("imagesoriginal/{name}", Connection = "AzureWebJobsStorage")]
        public async Task<byte[]> RunOut([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] byte[] inputBytes, string name)
        {
         
            if (name.EndsWith(".png"))
            {
                using (var ms = new MemoryStream(inputBytes))
                {
                    using (var image = Image.FromStream(ms))
                    {
                        using (var jpgStream = new MemoryStream())
                        {
                            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                            var encoderParams = new EncoderParameters(1);

                           
                            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);

                            
                            image.Save(jpgStream, jpegEncoder, encoderParams);
                            return jpgStream.ToArray(); 
                        }
                    }
                }
            }

       
            if (name.EndsWith(".jpg") || name.EndsWith(".jpeg"))
            {
                return inputBytes; 
            }

           
            return null;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(encoder => encoder.FormatID == format.Guid);
        }
    }
}
