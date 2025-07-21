using System;
using System.Collections.Generic;
using System.IO;
using financesApi.models;
using OFXParser;

namespace financesApi.services
{
    public static class OfxParserService
    {
        public static List<OfxTransactionDto> Parse(Stream ofxStream)
        {
            // Read the stream into a string
            using var reader = new StreamReader(ofxStream);
            string ofxContent = reader.ReadToEnd();

            // Parse the OFX content
            var extract = Parser.GenerateExtract(ofxContent);
            
            var results = new List<OfxTransactionDto>();

            foreach (var tx in extract.Transactions)
            {
                results.Add(new OfxTransactionDto
                {
                    Date = tx.Date,
                    Amount = tx.TransactionValue,
                    Payee = tx.Description,
                    Memo = null // OFXParser.NET doesn't expose memo separately
                });
            }

            return results;
        }
    }
}
