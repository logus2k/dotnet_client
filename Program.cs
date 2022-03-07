using System.Net.Http.Headers;


namespace TECH5.IDencode.Client
{
    class Program
    {        
        static async Task Main()
        {
            bool success = await Enroll();
        
            Console.WriteLine("Success: " + success.ToString());
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

        static async Task<bool> Enroll()
        {
            Dictionary<string,string> configurationProperties = await LoadConfigurationProperties();
            
            try 
            {
                using var client = new HttpClient();
                using var stream = File.OpenRead(configurationProperties["faceImagePath"]);

                var content = new MultipartFormDataContent();
                var file_content = new ByteArrayContent(new StreamContent(stream).ReadAsByteArrayAsync().Result);

                file_content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                file_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = Path.GetFileName(configurationProperties["faceImagePath"]),
                    Name = "face_image",
                };

                content.Add(file_content);
                client.BaseAddress = new Uri(configurationProperties["idencodeBaseUrl"]);


                // string requestContentFilePath = @"C:\TECH5\Products\IDencode\REST_API\dotnet_client\raw_request.txt";
                // await DumpRequest(content, requestContentFilePath);

                var response = await client.PostAsync("enroll", content);

                HttpResponseMessage responseMessage = response.EnsureSuccessStatusCode();

                
                // var responseReader = new StreamReader(await responseMessage.Content.ReadAsStreamAsync());
                // Console.WriteLine(await responseReader.ReadToEndAsync());

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!" + ex.Message);

                return false;
            }
        }

        static async Task<bool> DumpRequest(MultipartFormDataContent content, string requestContentFilePath)
        {
            bool success = false;
            
            try
            {
                using var binaryWriter = new BinaryWriter(File.Open(requestContentFilePath, FileMode.Create));
                binaryWriter.Write(await content.ReadAsByteArrayAsync());
                binaryWriter.Close();

                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return success;
        }

        static async Task<bool> DumpResponse(MultipartFormDataContent content, string requestContentFilePath)
        {
            bool success = false;
            
            try
            {
                using var binaryWriter = new BinaryWriter(File.Open(requestContentFilePath, FileMode.Create));
                binaryWriter.Write(await content.ReadAsByteArrayAsync());
                binaryWriter.Close();

                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return success;
        }        
    }
}
