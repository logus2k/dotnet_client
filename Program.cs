using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;


namespace TECH5.IDencode.Client
{
    public class Program
    {        
        static async Task Main()
        {
            EnrollResult enrollResult = await Enroll();
        
            if (enrollResult.ErrorCode != 0)
            {
                Console.WriteLine("Error code: " + enrollResult.ErrorCode.ToString());
            }
            else
            {
                Console.WriteLine(enrollResult.Guid);
            } 
        }

        static async Task<EnrollResult> Enroll()
        {
            EnrollResult enrollResult = new(-1, string.Empty);

            Dictionary<string, string> configurationProperties = await LoadConfigurationProperties();
            
            try 
            {
                using var client = new HttpClient();
                var requestContent = new MultipartFormDataContent();


                // Add face image data part
                using var faceImageStream = File.OpenRead(configurationProperties["faceImagePath"]);
                var file_content = new ByteArrayContent(new StreamContent(faceImageStream).ReadAsByteArrayAsync().Result);
                faceImageStream.Close();

                file_content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                file_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = Path.GetFileName(configurationProperties["faceImagePath"]),
                    Name = "face_image",
                };

                requestContent.Add(file_content);


                // Add demographics data part
                bool includeDemographics = false;

                if (configurationProperties["includeDemographics"] != null && configurationProperties["includeDemographics"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    includeDemographics = true;
                }

                if (includeDemographics)
                {
                    using var demographicDataStream = File.OpenRead(configurationProperties["demographicsFilePath"]);
                    var demographicData_content = new ByteArrayContent(new StreamContent(demographicDataStream).ReadAsByteArrayAsync().Result);
                    demographicDataStream.Close();

                    demographicData_content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    demographicData_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        FileName = Path.GetFileName(configurationProperties["demographicsFilePath"]),
                        Name = "demog",
                    };

                    requestContent.Add(demographicData_content);
                }


                // Add pipeline data part
                using var pipelineDataStream = new MemoryStream();
                await JsonSerializer.SerializeAsync(
                    pipelineDataStream, 
                    new Pipeline(configurationProperties), 
                    typeof(Pipeline), 
                    new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }, 
                    new CancellationToken());

                pipelineDataStream.Position = 0;
                var reader = new StreamReader(pipelineDataStream);
                string pipelineJsonText = reader.ReadToEnd();

                pipelineDataStream.Position = 0;
                var pipelineData_content = new ByteArrayContent(new StreamContent(pipelineDataStream).ReadAsByteArrayAsync().Result);
                pipelineDataStream.Close();
                
                pipelineData_content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                pipelineData_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = "pipeline.json",
                    Name = "pipeline",
                };

                requestContent.Add(pipelineData_content);

                
                // Write the raw request to a file
                /*
                string requestContentFilePath = Path.Combine(Environment.CurrentDirectory, "raw_request.txt");
                await Dumper.DumpRequest(requestContent, requestContentFilePath);
                */

                
                client.BaseAddress = new Uri(configurationProperties["idencodeBaseUrl"]);                
                var response = await client.PostAsync("enroll", requestContent);

                HttpResponseMessage responseMessage = response.EnsureSuccessStatusCode();

                
                JsonDocument jsonDocument = await JsonDocument.ParseAsync(await responseMessage.Content.ReadAsStreamAsync());
                string? base64EncodedCryptograph = jsonDocument.RootElement.GetProperty("image").GetString();

                if (string.IsNullOrEmpty(base64EncodedCryptograph))
                {
                    throw new ApplicationException("Cryptograph image is null or empty");
                }

                enrollResult.Guid = jsonDocument.RootElement.GetProperty("uuid").GetString();

                if (string.IsNullOrEmpty(enrollResult.Guid))
                {
                    throw new ApplicationException("uuid is null or empty");
                }            

                string outputFilesPath = Path.Combine(configurationProperties["outputFilesPath"], enrollResult.Guid);

                enrollResult.ErrorCode = await Base64ToImage(base64EncodedCryptograph, outputFilesPath);

                if (enrollResult.ErrorCode == 0)
                {
                    await File.WriteAllTextAsync(Path.Combine(outputFilesPath, "pipeline.json"), pipelineJsonText);
                    await File.WriteAllTextAsync(Path.Combine(outputFilesPath, "response.json"), jsonDocument.RootElement.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return enrollResult;
        }

        static async Task<Dictionary<string, string>> LoadConfigurationProperties()
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

    internal class EnrollResult
    {
        internal int ErrorCode {get; set;}
        internal string? Guid {get; set;}

        internal EnrollResult(int errorCode, string guid)
        {
            ErrorCode = errorCode;
            Guid = guid;
        }
    }
}
