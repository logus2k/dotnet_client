using System.Text.Json;


namespace TECH5.IDencode.Client
{
    public class Pipeline
    {
        public FacePipeline FacePipeline { get; set; }
        public BarcodeGenerationParameters BarcodeGenerationParameters { get; set; }
        public EmailSender EmailSender{ get; set; }

        public Pipeline(Dictionary<string, string> configurationProperties)
        {
            FacePipeline = new();
            BarcodeGenerationParameters = new();
            EmailSender = new();

            FacePipeline.FaceDetectorConfidence = 0.6;
            FacePipeline.FaceSelectorAlg = 1;
            
            if (configurationProperties["includeFaceTemplate"] != null && configurationProperties["includeFaceTemplate"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                FacePipeline.PerformTemplateExtraction = true;
            }
            else
            {
                FacePipeline.PerformTemplateExtraction = false;
            }

            if (configurationProperties["includeCompressedImage"] != null && configurationProperties["includeCompressedImage"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                FacePipeline.PerformCompression = true;
            }
            else
            {
                FacePipeline.PerformCompression = false;
            }

            if (configurationProperties["compressionLevel"] != null)
            {
                if (Int32.TryParse(configurationProperties["compressionLevel"], out int compressionLevel))
                {                  
                    FacePipeline.CompressionLevel = compressionLevel;
                }
                else
                {
                    FacePipeline.CompressionLevel = 1;
                }
            }                  

            if (configurationProperties["cols"] != null)
            {
                if (Int32.TryParse(configurationProperties["cols"], out int blockCols))
                {                  
                    BarcodeGenerationParameters.BlockCols = blockCols;
                }
                else
                {
                    BarcodeGenerationParameters.BlockCols = 30;
                }
            }
            
            if (configurationProperties["rows"] != null)
            {
                if (Int32.TryParse(configurationProperties["rows"], out int blockRows))
                {                  
                    BarcodeGenerationParameters.BlockRows = blockRows;
                }
                else
                {
                    BarcodeGenerationParameters.BlockRows = 8;
                }
            }

            if (configurationProperties["errorCorrection"] != null)
            {
                if (Int32.TryParse(configurationProperties["errorCorrection"], out int errorCorrection))
                {                  
                    BarcodeGenerationParameters.ErrorCorrection = errorCorrection;
                }
                else
                {
                    BarcodeGenerationParameters.ErrorCorrection = 12;
                }
            }

            if (configurationProperties["gridSize"] != null)
            {
                if (Int32.TryParse(configurationProperties["gridSize"], out int gridSize))
                {                  
                    BarcodeGenerationParameters.GridSize = gridSize;
                }
                else
                {
                    BarcodeGenerationParameters.GridSize = 7;
                }
            }

            if (configurationProperties["thickness"] != null)
            {
                if (Int32.TryParse(configurationProperties["thickness"], out int thickness))
                {                  
                    BarcodeGenerationParameters.Thickness = thickness;
                }
                else
                {
                    BarcodeGenerationParameters.Thickness = 2;
                }
            }            

            if (configurationProperties["expiryDate"] != null)
            {

                if (DateTime.TryParse(configurationProperties["expiryDate"], out DateTime expirationDate))
                {
                    BarcodeGenerationParameters.Expirationdate = expirationDate.ToString("yyyy-MM-ddT23:59:59Z");
                }
                else
                {
                    BarcodeGenerationParameters.Expirationdate = DateTime.Now.AddYears(1).ToString("yyyy-MM-ddT23:59:59Z");
                }
            }            

            if (configurationProperties["email"] != null)
            {
                EmailSender.Emailto = configurationProperties["email"];
            }
            else
            {
                throw new ApplicationException("Configuration property \"email\" is missing a required value");
            }

            EmailSender.Subject = "Your Tech5 IDencode";
        }
    }

    public class FacePipeline
    {
        public bool PerformTemplateExtraction { get; set; } 
        public double FaceDetectorConfidence { get; set; }
        public int FaceSelectorAlg { get; set; } 
        public bool PerformCompression { get; set; }
        public int CompressionLevel { get; set; }
    }

    public class BarcodeGenerationParameters
    {
        public int BlockCols { get; set; }
        public int BlockRows { get; set; }
        public int ErrorCorrection { get; set; }
        public int GridSize { get; set; } 
        public int Thickness { get; set; } 
        public string? Expirationdate { get; set; }        
    }

    public class EmailSender
    {
        public string? Emailto { get; set; }
        public string? Subject { get; set; }     
    }    
}