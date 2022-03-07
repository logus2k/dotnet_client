using System.Text.Json;


namespace TECH5.IDencode.Client
{
    public class Pipeline
    {
        public FacePipeline facePipeline { get; set; }
        public BarcodeGenerationParameters barcodeGenerationParameters { get; set; }
        public EmailSender emailSender{ get; set; }

        public Pipeline(Dictionary<string,string> configurationProperties)
        {
            facePipeline = new();
            barcodeGenerationParameters = new();
            emailSender = new();

            facePipeline.faceDetectorConfidence = 0.6;
            facePipeline.faceSelectorAlg = 1;
            
            if (configurationProperties["includeFaceTemplate"] != null && configurationProperties["includeFaceTemplate"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                facePipeline.performTemplateExtraction = true;
            }
            else
            {
                facePipeline.performTemplateExtraction = false;
            }

            if (configurationProperties["includeCompressedImage"] != null && configurationProperties["includeCompressedImage"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                facePipeline.performCompression = true;
            }
            else
            {
                facePipeline.performCompression = false;
            }

            if (configurationProperties["compressionLevel"] != null)
            {
                if (Int32.TryParse(configurationProperties["compressionLevel"], out int compressionLevel))
                {                  
                    facePipeline.compressionLevel = compressionLevel;
                }
            }                  

            if (configurationProperties["cols"] != null)
            {
                if (Int32.TryParse(configurationProperties["cols"], out int blockCols))
                {                  
                    barcodeGenerationParameters.blockCols = blockCols;
                }
            }
            
            if (configurationProperties["rows"] != null)
            {
                if (Int32.TryParse(configurationProperties["rows"], out int blockRows))
                {                  
                    barcodeGenerationParameters.blockRows = blockRows;
                }
            }

            if (configurationProperties["errorCorrection"] != null)
            {
                if (Int32.TryParse(configurationProperties["errorCorrection"], out int errorCorrection))
                {                  
                    barcodeGenerationParameters.errorCorrection = errorCorrection;
                }
            }

            if (configurationProperties["gridSize"] != null)
            {
                if (Int32.TryParse(configurationProperties["gridSize"], out int gridSize))
                {                  
                    barcodeGenerationParameters.gridSize = gridSize;
                }
            }

            if (configurationProperties["thickness"] != null)
            {
                if (Int32.TryParse(configurationProperties["thickness"], out int thickness))
                {                  
                    barcodeGenerationParameters.thickness = thickness;
                }
            }            

            if (configurationProperties["expiryDate"] != null)
            {

                if (DateTime.TryParse(configurationProperties["expiryDate"], out DateTime expirationDate))
                {
                    barcodeGenerationParameters.expirationdate = expirationDate.ToString("yyyy-MM-ddT23:59:59Z");
                }
                else
                {
                    barcodeGenerationParameters.expirationdate = DateTime.Now.AddYears(1).ToString("yyyy-MM-ddT23:59:59Z");
                }
            }            

            if (configurationProperties["email"] != null)
            {
                emailSender.emailto = configurationProperties["email"];
            }
            else
            {
                throw new ApplicationException("Configuration property: \"email\" is missing a value");
            }

            emailSender.subject = "Your Tech5 IDencode";
        }
    }

    public class FacePipeline
    {
        public bool performTemplateExtraction { get; set; } // true
        public double faceDetectorConfidence { get; set; } // 0.6;
        public int faceSelectorAlg { get; set; } // 1;
        public bool performCompression { get; set; } // true;
        public int compressionLevel { get; set; } // 1;
    }

    public class BarcodeGenerationParameters
    {
        public int blockCols { get; set; } // 30;
        public int blockRows { get; set; } // 8;
        public int errorCorrection { get; set; } // 12;
        public int gridSize { get; set; } // 7;
        public int thickness { get; set; } // 2;
        public string? expirationdate { get; set; } // DateTime.Parse("2023-03-12T00:00:00Z");        
    }

    public class EmailSender
    {
        public string? emailto { get; set; } // "someone_427387428784@gmail.com";
        public string? subject { get; set; } // "Your Tech5 IDencode";        
    }    
}