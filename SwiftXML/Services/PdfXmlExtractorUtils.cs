using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SwiftXML.Services
{
    public static class PdfXmlExtractorUtils
    {
        public static bool IsValidPdfFile(IFormFile? pdfFile, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (pdfFile == null || !pdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Please upload a valid PDF file.";
                return false;
            }
            return true;
        }

        public static MatchCollection FindXmlMatches(string textContent)
        {
            const string pattern = @"<pacs:Document xmlns.*?</pacs:Document>";
            return Regex.Matches(textContent, pattern, RegexOptions.Singleline);
        }

        public static string CleanNonXmlText(string input)
        {
            try
            {
                // Try parsing the input as XML
                var xmlDoc = XDocument.Parse(input);

                // Remove free text nodes in non-leaf elements
                RemoveFreeTextInNonLeafElements(xmlDoc.Root);

                // Return the cleaned XML as a string
                return xmlDoc.ToString();
            }
            catch (System.Xml.XmlException)
            {
                // Fallback: Remove lines that don't appear to be XML tags
                var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var cleanedLines = new List<string>();
                const string xmlTagPattern = @"^\s*<.+>.$|.</.+>\s*$";

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (Regex.IsMatch(trimmedLine, xmlTagPattern))
                    {
                        cleanedLines.Add(line);
                    }
                }

                return string.Join("", cleanedLines).Trim();
            }
        }

        private static void RemoveFreeTextInNonLeafElements(XElement element)
        {
            if (element == null) return;

            // Process nodes
            var nodes = element.Nodes().ToList(); // Copy to avoid modification issues
            foreach (var node in nodes)
            {
                if (node is XText textNode && !string.IsNullOrWhiteSpace(textNode.Value))
                {
                    // Check if the parent is a non-leaf element (has child elements)
                    bool isNonLeaf = element.Elements().Any();
                    if (isNonLeaf)
                    {
                        // Remove free text in non-leaf elements
                        textNode.Remove();
                    }
                    else
                    {
                        // Trim whitespace from valid text in leaf elements
                        textNode.Value = textNode.Value.Trim();
                    }
                }
                else if (node is XElement childElement)
                {
                    // Recursively process child elements
                    RemoveFreeTextInNonLeafElements(childElement);
                }
            }
        }
    }
}