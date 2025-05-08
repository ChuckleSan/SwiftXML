using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SwiftXML.Services;

namespace SwiftXML.Pages
{
    public class IndexModel(IPdfXmlExtractorService pdfXmlExtractorService) : PageModel
    {
        private readonly IPdfXmlExtractorService _pdfXmlExtractorService = pdfXmlExtractorService;

        [BindProperty]
        public IFormFile? PdfFile { get; set; }

        public List<string> XmlContents { get; set; } = [];
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // Initial page load, no action needed
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            var (xmlContents, errorMessage) = await _pdfXmlExtractorService.ExtractXmlAsync(PdfFile);
            XmlContents = xmlContents;
            ErrorMessage = errorMessage;

            return Page();
        }

        public IActionResult OnPostDownloadSamplePdfAsync()
        {
            try
            {
                // Define XML payload
                string xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                    <Invoice xmlns=""http://example.com/invoice"" InvoiceNumber=""INV-2025-001"">
                                      <IssueDate>2025-04-22</IssueDate>
                                      <DueDate>2025-05-22</DueDate>
                                      <Customer>
                                        <Name>John Doe</Name>
                                        <Company>Acme Corp</Company>
                                        <Address>
                                          <Street>123 Main St</Street>
                                          <City>Springfield</City>
                                          <State>IL</State>
                                          <PostalCode>62701</PostalCode>
                                          <Country>USA</Country>
                                        </Address>
                                        <Email>john.doe@acmecorp.com</Email>
                                        <Phone>+1-555-123-4567</Phone>
                                      </Customer>
                                      <Items>
                                        <Item>
                                          <ID>1</ID>
                                          <Description>Laptop Model X</Description>
                                          <Quantity>2</Quantity>
                                          <UnitPrice>1200.00</UnitPrice>
                                          <Total>2400.00</Total>
                                          <TaxRate>0.08</TaxRate>
                                          <TaxAmount>192.00</TaxAmount>
                                        </Item>
                                        <Item>
                                          <ID>2</ID>
                                          <Description>Wireless Mouse</Description>
                                          <Quantity>5</Quantity>
                                          <UnitPrice>25.00</UnitPrice>
                                          <Total>125.00</Total>
                                          <TaxRate>0.08</TaxRate>
                                          <TaxAmount>10.00</TaxAmount>
                                        </Item>
                                        <Item>
                                          <ID>3</ID>
                                          <Description>Extended Warranty</Description>
                                          <Quantity>1</Quantity>
                                          <UnitPrice>150.00</UnitPrice>
                                          <Total>150.00</Total>
                                          <TaxRate>0.08</TaxRate>
                                          <TaxAmount>12.00</TaxAmount>
                                        </Item>
                                      </Items>
                                      <Summary>
                                        <Subtotal>2675.00</Subtotal>
                                        <TotalTax>214.00</TotalTax>
                                        <TotalAmount>2889.00</TotalAmount>
                                        <Currency>USD</Currency>
                                      </Summary>
                                      <PaymentTerms>
                                        <Method>Bank Transfer</Method>
                                        <AccountDetails>
                                          <BankName>First National Bank</BankName>
                                          <AccountNumber>1234567890</AccountNumber>
                                          <RoutingNumber>111000614</RoutingNumber>
                                        </AccountDetails>
                                        <DueDays>30</DueDays>
                                      </PaymentTerms>
                                      <Notes>
                                        <Note>Thank you for your business!</Note>
                                        <Note>Please contact us for any questions regarding this invoice.</Note>
                                      </Notes>
                                    </Invoice>";

                // Create PDF in memory
                using var memoryStream = new MemoryStream();
                using (var writer = new PdfWriter(memoryStream))
                {
                    using var pdf = new PdfDocument(writer);
                    // Add content to the PDF
                    var document = new Document(pdf);
                    document.Add(new Paragraph("Sample PDF with XML Content")
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontSize(16));
                    document.Add(new Paragraph("The following XML is included as text in this PDF:"));
                    document.Add(new Paragraph(xmlContent)
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.COURIER))
                        .SetFontSize(10)
                        .SetFixedLeading(12));

                    document.Close();
                }

                // Return the PDF as a downloadable file
                var pdfBytes = memoryStream.ToArray();
                return File(pdfBytes, "application/pdf", "sample_invoice.pdf");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating sample PDF: {ex.Message}";
                return Page();
            }
        }

        public IActionResult OnPostDownloadXmlAsync([FromForm] string xmlContent)
        {
            try
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(xmlContent);
                return File(bytes, "text/plain", "extracted_xml.txt");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating text file: {ex.Message}";
                return Page();
            }
        }
    }
}