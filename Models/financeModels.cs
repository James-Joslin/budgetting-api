using System.Text.Json.Serialization;
using financesApi.utilities;

namespace financesApi.models
{
    public class NewAccountRequest
    {
        public required string _firstName;
        public required string _lastName;
        public required string _accountName;
        public string FirstName 
        { 
            get => _firstName;
            set => _firstName = value.ToTitleCase();
        }
        
        public string LastName 
        { 
            get => _lastName;
            set => _lastName = value.ToTitleCase();
        }
        
        public string AccountName 
        { 
            get => _accountName;
            set => _accountName = value.ToTitleCase();
        }
        public decimal StartingBalance { get; set; }
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
        public required IFormFile OfxContent { get; set; }
        public int AccountId { get; set; }
        public bool SkipDuplicates { get; set; } = true;
        public string? SourceFileName { get; set; }
    }

    public class TransactionQueryRequest
    {
        public string? AccountName { get; set; }
    }
}

