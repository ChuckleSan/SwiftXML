﻿using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace SwiftXML.Services
{
    public class PdfXmlExtractorService : IPdfXmlExtractorService
    {
        public async Task<(List<string> XmlContents, string ErrorMessage)> ExtractXmlAsync(IFormFile? pdfFile)
        {
            List<string> xmlContents = new();
            string errorMessage;
            try
            {
                // Validate input
                if (!PdfXmlExtractorUtils.IsValidPdfFile(pdfFile, out errorMessage))
                {
                    return (xmlContents, errorMessage);
                }

                // Extract text from PDF
                string textContent = await ExtractPdfTextAsync(pdfFile);
                if (string.IsNullOrEmpty(textContent))
                {
                    return (xmlContents, "No text content found in the PDF.");
                }

                // Find XML matches
                var xmlMatches = PdfXmlExtractorUtils.FindXmlMatches(textContent);
                if (xmlMatches.Count == 0)
                {
                    return (xmlContents, "No XML data found in the PDF text content.");
                }

                // Process each XML match
                foreach (Match match in xmlMatches)
                {
                    string formattedXml = FormatXmlContent(match.Value);
                    xmlContents.Add(formattedXml);
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error extracting XML: {ex.Message}";
            }

            return (xmlContents, errorMessage);
        }

        private static async Task<string> ExtractPdfTextAsync(IFormFile pdfFile)
        {
            using var stream = new MemoryStream();
            await pdfFile.CopyToAsync(stream);
            stream.Position = 0;

            using var pdfDoc = new PdfDocument(new PdfReader(stream));
            StringBuilder textContent = new();

            for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
            {
                var page = pdfDoc.GetPage(pageNum);
                var text = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
                textContent.Append(text);
            }

            return textContent.ToString();
        }

        private static string FormatXmlContent(string xmlContent)
        {
            try
            {
                // Clean non-XML text
                string cleanedXml = PdfXmlExtractorUtils.CleanNonXmlText(xmlContent);

                // Parse and format XML (existing pretty-printing logic)
                var xmlDoc = XDocument.Parse(cleanedXml);
                return xmlDoc.ToString();
            }
            catch (System.Xml.XmlException)
            {
                // Return original content with a warning if malformed
                return $"<!-- Malformed XML -->\n{xmlContent}";
            }
        }
    }
}