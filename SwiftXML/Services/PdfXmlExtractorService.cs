using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

using System.Text;
using System.Text.RegularExpressions;

namespace SwiftXML.Services
{
    public class PdfXmlExtractorService
    {
        public async Task<(List<string> XmlContents, string ErrorMessage)> ExtractXmlAsync(IFormFile? pdfFile)
        {
            List<string> xmlContents = [];
            string errorMessage = string.Empty;

            try
            {
                if (pdfFile == null || !pdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return (xmlContents, "Please upload a valid PDF file.");
                }

                using var stream = new MemoryStream();
                await pdfFile.CopyToAsync(stream);
                stream.Position = 0;

                using var pdfDoc = new PdfDocument(new PdfReader(stream));
                StringBuilder textContent = new();

                // Extract text from all pages
                for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                {
                    var page = pdfDoc.GetPage(pageNum);
                    var text = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
                    textContent.Append(text);
                }

                // Use regex to find XML-like content
                string pattern = @"<\?xml.*?</Invoice>";
                var matches = Regex.Matches(textContent.ToString(), pattern, RegexOptions.Singleline);

                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        xmlContents.Add(match.Value);
                    }
                }
                else
                {
                    errorMessage = "No XML data found in the PDF text content.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error extracting XML: {ex.Message}";
            }

            return (xmlContents, errorMessage);
        }
    }
}