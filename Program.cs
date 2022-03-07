using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace TECH5.IDencode.Client
{
    class Program
    {        
        static async Task Main()
        {
            int errorCode = await Enroll();
        
            Console.WriteLine("errorCode: " + errorCode.ToString());
        }

        static async Task<int> Enroll()
        {
            int errorCode = -1;
            
            Dictionary<string,string> configurationProperties = await LoadConfigurationProperties();
            
            try 
            {
                using var client = new HttpClient();
                using var faceImageStream = File.OpenRead(configurationProperties["faceImagePath"]);

                var content = new MultipartFormDataContent();

                var file_content = new ByteArrayContent(new StreamContent(faceImageStream).ReadAsByteArrayAsync().Result);

                file_content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                file_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = Path.GetFileName(configurationProperties["faceImagePath"]),
                    Name = "face_image",
                };

                content.Add(file_content);
                faceImageStream.Close();



                bool includeDemographics = false;

                if (configurationProperties["includeDemographics"] != null && configurationProperties["includeDemographics"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    includeDemographics = true;
                }

                if (includeDemographics)
                {
                    using var demographicDataStream = File.OpenRead(configurationProperties["demographicsFilePath"]);
                    var demographicData_content = new ByteArrayContent(new StreamContent(demographicDataStream).ReadAsByteArrayAsync().Result);

                    demographicData_content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    demographicData_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        FileName = Path.GetFileName(configurationProperties["demographicsFilePath"]),
                        Name = "demog",
                    };

                    content.Add(demographicData_content);
                    demographicDataStream.Close();
                }



                Pipeline pipeline = new(configurationProperties);
                using var pipelineDataStream = new MemoryStream();
                await JsonSerializer.SerializeAsync(pipelineDataStream, pipeline, pipeline.GetType());

                pipelineDataStream.Position = 0;
                var pipelineData_content = new ByteArrayContent(new StreamContent(pipelineDataStream).ReadAsByteArrayAsync().Result);
                
                pipelineData_content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                pipelineData_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = "pipeline.json",
                    Name = "pipeline",
                };

                content.Add(pipelineData_content);
                pipelineDataStream.Close();



                client.BaseAddress = new Uri(configurationProperties["idencodeBaseUrl"]);


                string requestContentFilePath = @"C:\TECH5\Products\IDencode\REST_API\dotnet_client\raw_request.txt";
                await Dumper.DumpRequest(content, requestContentFilePath);

                
                var response = await client.PostAsync("enroll", content);

                HttpResponseMessage responseMessage = response.EnsureSuccessStatusCode();

                
                JsonDocument jsonDocument = await JsonDocument.ParseAsync(await responseMessage.Content.ReadAsStreamAsync());
                string? base64EncodedCryptograph = jsonDocument.RootElement.GetProperty("image").GetString();

                if (string.IsNullOrEmpty(base64EncodedCryptograph))
                {
                    throw new ApplicationException("Cryptograph image is null or empty");
                }

                string? uuid = jsonDocument.RootElement.GetProperty("uuid").GetString();

                if (string.IsNullOrEmpty(uuid))
                {
                    throw new ApplicationException("uuid is null or empty");
                }              

                string outputFilesPath = Path.Combine(configurationProperties["outputFilesPath"], uuid);

                errorCode = await Base64ToImage(base64EncodedCryptograph, outputFilesPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return errorCode;
        }

        static async Task<Dictionary<string,string>> LoadConfigurationProperties()
        {
            string propertiesFileName = string.Concat(AppDomain.CurrentDomain.FriendlyName, ".properties");
            string propertiesFilePath = Path.Combine(Environment.CurrentDirectory, propertiesFileName);
            string propertiesFileContentString = await File.ReadAllTextAsync(propertiesFilePath);

            Dictionary<string,string> configurationProperties = propertiesFileContentString.Split(Environment.NewLine)
                .Where(value => value.Length > 0)
                .Select(value => value.Split('='))
                .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());

            return configurationProperties;
        }        

        private static async Task<int> Base64ToImage(string base64String, string outputFilesPath)
        {
            int errorCode = -1;
            
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                IImage image;

                using var stream = new MemoryStream(imageBytes);
                image = PlatformImage.FromStream(stream, ImageFormat.Png);
                IImage newImage = image.ToPlatformImage();
                stream.Close();

                Directory.CreateDirectory(outputFilesPath);

                using var binaryWriter = new BinaryWriter(File.Open(Path.Combine(outputFilesPath, "cryptograph.png"), FileMode.Create));
                binaryWriter.Write(await newImage.AsBytesAsync());
                binaryWriter.Close();

                errorCode = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return errorCode;
        }
    }
}
