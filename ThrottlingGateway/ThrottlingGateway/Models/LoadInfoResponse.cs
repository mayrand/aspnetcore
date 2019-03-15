using System;

namespace ThrottlingGateway.Models
{
    public class LoadInfoResponse
    {
        public bool SmartNodesAreProcessing { get; set; }
        public bool IngestIsProcessing { get; set; }
        public bool TvaIsProcessing { get; set; }
        public DateTime Occured { get; set; }
    }
}
