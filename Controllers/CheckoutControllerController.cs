using ApiOnLamda.Data;
using ApiOnLamda.Model;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly DatabaseHelper _dbHelper;

    public CheckoutController(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    // [HttpPost("proceed")]
    //    public async Task<IActionResult> ProceedToCheckout([FromBody] CheckoutRequest request)
    //    {
    //        if (request.UserId <= 0 || request.CartItems == null || !request.CartItems.Any())
    //        {
    //            return BadRequest("Invalid checkout data.");
    //        }

    //        try
    //        {
    //            // Calculate total amount
    //            decimal totalAmount = 0;
    //            foreach (var item in request.CartItems)
    //            {
    //                var product = await _dbHelper.GetProductById(item.ProductId);
    //                if (product == null)
    //                {
    //                    return NotFound($"Product with ID {item.ProductId} not found.");
    //                }
    //                totalAmount += item.Quantity * product.NewPrice;
    //            }

    //            // Insert order into database
    //            var orderId = await _dbHelper.AddOrder(request.UserId, totalAmount);

    //            if (orderId > 0)
    //            {
    //                // Insert order items
    //                foreach (var item in request.CartItems)
    //                {
    //                    var product = await _dbHelper.GetProductById(item.ProductId);
    //                    //await _dbHelper.AddOrderItem(orderId, item.ProductId, item.Quantity, product.NewPrice);
    //                    await _dbHelper.AddOrderItem(item);
    //                }

    //                return Ok(new
    //                {
    //                    Message = "Checkout successful",
    //                    OrderId = orderId,
    //                    TotalAmount = totalAmount
    //                });
    //            }

    //            return StatusCode(500, "An error occurred while processing the checkout.");
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, $"An error occurred: {ex.Message}");
    //        }
    //    }
    //}

    private bool IsValidCheckoutRequest(CheckoutRequest request)
    {
        if (request == null)
            return false;

        if (request.UserId <= 0 || request.CartItems == null || !request.CartItems.Any())
            return false;

        // Additional validations (e.g., valid product IDs or quantities)
        foreach (var item in request.CartItems)
        {
            if (item.ProductId <= 0 || item.Quantity <= 0)
                return false;
        }

        return true;
    }


    [HttpPost("proceed")]
    public async Task<IActionResult> ProceedToCheckoutWithTransaction([FromBody] CheckoutRequest request)
    {
        if (request.UserId <= 0 || request.CartItems == null || !request.CartItems.Any())
        {
            return BadRequest("Invalid checkout data.");
        }

        try
        {
            // Calculate total amount
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.CartItems)
            {
                var product = await _dbHelper.GetProductById(item.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {item.ProductId} not found.");
                }

                //if (product.StockQuantity < item.Quantity)
                //{
                //    return BadRequest($"Insufficient stock for product {product.Name}.");
                //}

                totalAmount += item.Quantity * product.NewPrice;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.NewPrice
                });
            }

            // Proceed with the transaction
            var success = await _dbHelper.ProceedWithTransactionAsync(request.UserId, totalAmount, orderItems);

            if (success)
            {
                return Ok(new { Message = "Checkout successful", TotalAmount = totalAmount });
            }
            else
            {
                return StatusCode(500, "Transaction failed.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    [HttpPost("saveCheckout")]
    public async Task<IActionResult> SaveCheckoutDataAsync ([FromBody] checkoutrequestdata request)
    {
        if (request.UserId <= 0 || request == null )
        {
            return BadRequest("Invalid checkout data.");
        }

        try
        {
            // Call DatabaseHelper method to save checkout data using the stored procedure
            var success = await _dbHelper.SaveCheckoutDataAsync(request);

            if (success)
            {
                return Ok(new { Message = "Checkout data saved successfully." });
            }
            else
            {
                return StatusCode(500, "Failed to save checkout data.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }





    //[HttpPost("proceed")]
    //public async Task<IActionResult> ProceedToCheckout([FromBody] CheckoutRequest request)
    //{
    //    if (request.UserId <= 0 || request.CartItems == null || !request.CartItems.Any())
    //    {
    //        return BadRequest("Invalid checkout data.");
    //    }

    //    try
    //    {
    //        // Calculate total amount
    //        decimal totalAmount = 0;
    //        foreach (var item in request.CartItems)
    //        {
    //            var product = await _dbHelper.GetProductById(item.ProductId);
    //            if (product == null)
    //            {
    //                return NotFound($"Product with ID {item.ProductId} not found.");
    //            }
    //            totalAmount += item.Quantity * product.NewPrice;
    //        }

    //        // Insert order into database
    //        var orderId = await _dbHelper.AddOrder(request.UserId, totalAmount);

    //        if (orderId > 0)
    //        {
    //            // Insert order items
    //            foreach (var cartItem in request.CartItems)
    //            {
    //                var product = await _dbHelper.GetProductById(cartItem.ProductId);
    //                if (product == null)
    //                {
    //                    return NotFound($"Product with ID {cartItem.ProductId} not found.");
    //                }

    //                // Create OrderItem object
    //                var orderItem = new OrderItem
    //                {
    //                    OrderId = orderId,
    //                    ProductId = cartItem.ProductId,
    //                    Quantity = cartItem.Quantity,
    //                    Price = product.NewPrice
    //                };

    //                // Add OrderItem to database
    //                await _dbHelper.AddOrderItem(orderItem);
    //                await _dbHelper.ClearCart(request.UserId);

    //            }

    //            return Ok(new
    //            {
    //                Message = "Checkout successful",
    //                OrderId = orderId,
    //                TotalAmount = totalAmount
    //            });
    //        }

    //        return StatusCode(500, "An error occurred while processing the checkout.");
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, $"An error occurred: {ex.Message}");
    //    }
    //}
}
