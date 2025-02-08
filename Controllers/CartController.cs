using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ApiOnLamda.Model;
using ApiOnLamda.Data;
using ApiOnLamda.Model;
using ApiOnLamda;
using System.Drawing;
using OfficeOpenXml;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly DatabaseHelper _dbHelper;

    public CartController(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }


    [HttpPost("add")]

    public async Task<IActionResult> AddToCart([FromBody] Cart cartItem)
    {
        if (cartItem == null)
        {
            return BadRequest("Invalid cart item.");
        }
        cartItem.Date = DateTime.Now;
        int result = await _dbHelper.AddCartItem(cartItem);
        if (result > 0)
        {
            return Ok(new
            {
                Message = "Item added to cart successfully.",

                Date = cartItem.Date.ToString("MM/dd/yyyy")
            });
        }

        return StatusCode(500, "An error occurred while adding the item to the cart.");
    }

    // Get cart items for a user
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCartItems(int userId)
    {
        var cartItems = await _dbHelper.GetCartItemsForUser(userId);
        return Ok(cartItems);
    }

    [HttpGet("cart-items-by-date")]
    public async Task<IActionResult> GetCartItemsByDateRange(DateTime startDate, DateTime endDate)
    {
        //var startDateOny = DateTimeHelper.ConvertToMonthDayYearFormat(startDate.Date);
        //ar endDateOny = DateTimeHelper.ConvertToMonthDayYearFormat(endDate.Date);
        var cartItems = await _dbHelper.GetCartItemsByDateRange(startDate, endDate);
        if (cartItems == null || cartItems.Count == 0)
        {
            return NotFound("No cart items found in the given date range.");
        }
        return Ok(cartItems);
    }

    //[HttpGet("total sales")]
    //public async Task<IActionResult> GetTotalSalesByCategory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    //{
    //    if (startDate > endDate)
    //    {
    //        return BadRequest("Start date should be earlier than end date.");
    //    }

    //    // Fetch cart items within the specified date range
    //    var cartItems = await _dbHelper.GetCartItemsByDateRange(startDate, endDate);
    //    if (cartItems == null || !cartItems.Any())
    //    {
    //        return NotFound("No sales found in the specified date range.");
    //    }

    //    // Fetch all products
    //    var products = await _dbHelper.GetAllProducts();

    //    // Join cart items with products to get category information
    //    var categoryWiseSales = cartItems
    //        .Join(products,
    //              cart => cart.ProductId,
    //              product => product.Id,
    //              (cart, product) => new
    //              {
    //                  product.Category,
    //                  TotalPrice = product.NewPrice * cart.Quantity
    //              })
    //        .GroupBy(x => x.Category)
    //        .Select(g => new
    //        {
    //            Category = g.Key,
    //            TotalSales = g.Sum(x => x.TotalPrice)
    //        })
    //        .ToList();

    //    return Ok(categoryWiseSales);
    //}


    [HttpPost("totalsalesReport")]
    public async Task<IActionResult> GetTotalSalesCount([FromBody] DateRangeRequest request)
    {
        try
        {
            // Validate the incoming request
            if (request == null || request.StartDate == default || request.EndDate == default)
            {
                return BadRequest(new { success = false, message = "Invalid date range provided." });
            }
            // Use the DatabaseHelper to fetch the total sales count
            var totalSales = await _dbHelper.GetTotalSalesReport(request.StartDate, request.EndDate);
            return Ok(new { success = true, totalSales });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("popularItems")]
    public async Task<IActionResult> GetPopularItems()
    {
        try
        {
            var popularItems = await _dbHelper.GetTopSoldItemsPerCategory();
            return Ok(new { success = true, data = popularItems });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }



    //[HttpPost("totalsalesReport/exportExcel")]
    //public async Task<IActionResult> ExportSalesReportToExcel([FromBody] DateRangeRequest request)
    //{
    //    var salesReport = await _dbHelper.GetTotalSalesReport(request.StartDate, request.EndDate);

    //    using (var package = new ExcelPackage())
    //    {
    //        var worksheet = package.Workbook.Worksheets.Add("Sales Report");
    //        worksheet.Cells[1, 1].Value = "Date";
    //        worksheet.Cells[1, 2].Value = "Category";
    //        worksheet.Cells[1, 3].Value = "Total Sales";

    //        int row = 2;
    //        foreach (var record in salesReport)
    //        {
    //            worksheet.Cells[row, 1].Value = record.SaleDate.ToString("yyyy-MM-dd");
    //            worksheet.Cells[row, 2].Value = record.Category;
    //            worksheet.Cells[row, 3].Value = record.TotalSales;
    //            row++;
    //        }

    //        var stream = new MemoryStream(package.GetAsByteArray());
    //        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
    //    }
    //}

    [HttpPost("totalsalesReport/exportExcel")]
    public async Task<IActionResult> ExportSalesReportToExcel([FromBody] DateRangeRequest request)
    {
        try
        {
            // Validate request
            if (request == null || request.StartDate == DateTime.MinValue || request.EndDate == DateTime.MinValue)
            {
                return BadRequest("Invalid date range provided.");
            }

            // Fetch sales report data
            var salesReport = await _dbHelper.GetTotalSalesReport(request.StartDate, request.EndDate);
            if (salesReport == null || !salesReport.Any())
            {
                return NotFound("No sales data found for the given date range.");
            }

            // Generate dynamic filename based on the date range
            var formattedStartDate = request.StartDate.ToString("yyyyMMdd");
            var formattedEndDate = request.EndDate.ToString("yyyyMMdd");
            var fileName = $"SalesReport_{formattedStartDate}-{formattedEndDate}.xlsx";

            using (var package = new ExcelPackage())
            {
                // Create worksheet
                var worksheet = package.Workbook.Worksheets.Add("Sales Report");

                // Add headers
                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "Category";
                worksheet.Cells[1, 3].Value = "Total Sales";

                // Add data
                int row = 2;
                foreach (var record in salesReport)
                {
                    worksheet.Cells[row, 1].Value = record.SaleDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = record.Category;
                    worksheet.Cells[row, 3].Value = record.TotalSales;
                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Write the Excel file to memory stream
                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    memoryStream.Position = 0;

                    // Return the file
                    return File(memoryStream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                fileName);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error exporting sales report to Excel: {ex.Message}");
            return StatusCode(500, "An error occurred while generating the Excel file.");
        }
    }


    //[HttpPost("totalsalesReport/exportPdf")]
    //public async Task<IActionResult> ExportSalesReportToPdf([FromBody] DateRangeRequest request)
    //{
    //    var salesReport = await _dbHelper.GetTotalSalesReport(request.StartDate, request.EndDate);

    //    using (var ms = new MemoryStream())
    //    {
    //        var document = new PdfDocument();
    //        var page = document.AddPage();
    //        var gfx = XGraphics.FromPdfPage(page);
    //        var font = new XFont("Arial", 12, XFontStyle.Regular);

    //        gfx.DrawString("Sales Report", font, XBrushes.Black, new XPoint(20, 20));

    //        int y = 40;
    //        gfx.DrawString("Date", font, XBrushes.Black, new XPoint(20, y));
    //        gfx.DrawString("Category", font, XBrushes.Black, new XPoint(120, y));
    //        gfx.DrawString("Total Sales", font, XBrushes.Black, new XPoint(220, y));
    //        y += 20;

    //        foreach (var record in salesReport)
    //        {
    //            gfx.DrawString(record.SaleDate.ToString("yyyy-MM-dd"), font, XBrushes.Black, new XPoint(20, y));
    //            gfx.DrawString(record.Category, font, XBrushes.Black, new XPoint(120, y));
    //            gfx.DrawString(record.TotalSales.ToString(), font, XBrushes.Black, new XPoint(220, y));
    //            y += 20;
    //        }

    //        document.Save(ms);
    //        ms.Position = 0;

    //        return File(ms.ToArray(), "application/pdf", "SalesReport.pdf");
    //    }
    //}

    [HttpPost("totalsalesReport/exportPdf")]
    public async Task<IActionResult> ExportSalesReportToPdf([FromBody] DateRangeRequest request)
    {
        try
        {
            // Validate request
            if (request == null || request.StartDate == DateTime.MinValue || request.EndDate == DateTime.MinValue)
            {
                return BadRequest("Invalid date range provided.");
            }

            if (request.StartDate > request.EndDate)
            {
                return BadRequest("StartDate cannot be later than EndDate.");
            }

            // Fetch sales report data
            var salesReport = await _dbHelper.GetTotalSalesReport(request.StartDate, request.EndDate);
            if (salesReport == null || !salesReport.Any())
            {
                return NotFound("No sales data found for the given date range.");
            }

            // Generate filename
            var formattedStartDate = request.StartDate.ToString("yyyyMMdd");
            var formattedEndDate = request.EndDate.ToString("yyyyMMdd");
            var fileName = $"SalesReport_{formattedStartDate}-{formattedEndDate}.pdf";

            // Create the PDF document
            using (var memoryStream = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Helvetica", 12, XFontStyle.Regular);

                // Add content to the PDF
                gfx.DrawString("Sales Report", font, XBrushes.Black, new XPoint(20, 20));
                int y = 40;
                gfx.DrawString("Date", font, XBrushes.Black, new XPoint(20, y));
                gfx.DrawString("Category", font, XBrushes.Black, new XPoint(120, y));
                gfx.DrawString("Total Sales", font, XBrushes.Black, new XPoint(220, y));
                y += 20;

                foreach (var record in salesReport)
                {
                    gfx.DrawString(record.SaleDate.ToString("yyyy-MM-dd"), font, XBrushes.Black, new XPoint(20, y));
                    gfx.DrawString(record.Category, font, XBrushes.Black, new XPoint(120, y));
                    gfx.DrawString(record.TotalSales.ToString(), font, XBrushes.Black, new XPoint(220, y));
                    y += 20;
                }

                // Save the PDF to the stream
                document.Save(memoryStream);
                        var a = 1;
                //memoryStream.Position = 0; // Reset the stream position before returning it

                return File(memoryStream.ToArray(), "application/pdf", fileName);
            }
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.Error.WriteLine($"Error exporting sales report to PDF: {ex}");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }











    // Add methods to manage cart items, e.g., add to cart, get cart items for a user, etc.
}

