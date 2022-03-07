using System.Net.Http.Headers;

namespace TECH5.IDencode.Client
{
    internal class Dumper 
    {
        internal static async Task<int> DumpRequest(MultipartFormDataContent content, string requestContentFilePath)
        {
            int errorCode = -1;
            
            try
            {
                using var binaryWriter = new BinaryWriter(File.Open(requestContentFilePath, FileMode.Create));

                /*
                Dictionary<string, IEnumerable<string>> headers = Enumerable.ToDictionary(content.Headers, h => h.Key, h => h.Value);

                foreach (var header in headers)
                {
                    string key = header.Key + ": ";
                    
                    foreach (var value in header.Value)
                    {
                        binaryWriter.Write(key + value + Environment.NewLine);
                    }
                }
                */

                binaryWriter.Write(await content.ReadAsByteArrayAsync());
                binaryWriter.Close();

                errorCode = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return errorCode;
        }

        internal static async Task<int> DumpResponse(MultipartFormDataContent content, string requestContentFilePath)
        {
            int errorCode = -1;
            
            try
            {
                using var binaryWriter = new BinaryWriter(File.Open(requestContentFilePath, FileMode.Create));
                binaryWriter.Write(await content.ReadAsByteArrayAsync());
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