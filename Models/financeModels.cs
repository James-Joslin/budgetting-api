using System.Text.Json.Serialization;

namespace financesApi.models
{
    public class NewAccountRequest
    {
        // [JsonPropertyName("account_name")]
        public required string AccountName { get; set; }
        
        // [JsonPropertyName("first_name")]
        public required string FirstName { get; set; }
        
        // [JsonPropertyName("last_name")]
        public required string LastName { get; set; }
        
        // [JsonPropertyName("starting_balance")]
        public decimal StartingBalance { get; set; }
        
        // [JsonPropertyName("starting_date")]
        public required string StartingDate { get; set; }
    }


    public class OfxTransactionDto
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public required string Payee { get; set; }
        public string? Memo { get; set; }
    }

    public class OfxUploadRequest
    {
        public string OfxContent { get; set; } = "";
        public int AccountId { get; set; }
        public bool SkipDuplicates { get; set; } = true;
        public string? SourceFileName { get; set; }
    }

    public class TransactionQueryRequest
    {
        public string? AccountName { get; set; }
    }
}

