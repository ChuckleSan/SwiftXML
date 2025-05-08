namespace SwiftXML.Services
{
    public interface IPdfXmlExtractorService
    {
        Task<(List<string> XmlContents, string ErrorMessage)> ExtractXmlAsync(IFormFile? pdfFile);
    }
}