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