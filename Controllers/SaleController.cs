using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Invoice_system.Controllers
{
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales";
            // Filter out sales where qty is 0 (fully returned)
            var sales = _context.sales
                .AsEnumerable() // Pull to memory to handle string parsing safely in C#
                .Where(s => {
                    var match = System.Text.RegularExpressions.Regex.Match(s.qty_unit_type ?? "", @"^([0-9.-]+)");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal q))
                    {
                        return q > 0;
                    }
                    return true; // Keep it if we can't parse it (safety)
                })
                .OrderByDescending(s => s.date)
                .ToList();
            
            ViewBag.Returns = _context.returns.OrderByDescending(r => r.Date).ToList();

            return View(sales);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "New Sale";
            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
            ViewBag.Products = _context.products_services
                .Where(p => p.status == "Available")
                .ToList();
            
            // Fetch Seller Info for Receipt
            var seller = _context.sellers.FirstOrDefault(s => s.status == "Active") 
                         ?? _context.sellers.FirstOrDefault();
            ViewBag.SellerAddress = seller?.address ?? "Address";
            ViewBag.SellerContact = seller?.contact ?? "0313-3879645";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string customer_name, string status, string payment_method, string? description, string? billNo, List<Sale> items)
        {
            if (string.IsNullOrWhiteSpace(customer_name))
            {
                customer_name = "Walk-in";
            }
            ModelState.Remove("customer_name");

            // Remove description from validation as it is optional
            ModelState.Remove("description");

            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", "At least one product/service must be added.");
            }

            if (ModelState.IsValid)
            {
                // Ensure description is null if empty
                if (string.IsNullOrWhiteSpace(description)) description = null;

                DateTime now = DateTime.Now;
                var salesToInsert = new List<Sale>();
                var returnsToInsert = new List<ReturnDetail>();

                foreach (var item in items)
                {
                    item.status = status;
                    item.payment_method = payment_method;
                    item.description = description;
                    item.date = now;

                    // Parse numeric quantity
                    decimal qty = 0;
                    string unit = "";
                    if (!string.IsNullOrEmpty(item.qty_unit_type))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(item.qty_unit_type.Trim(), @"^([0-9.-]+)\s*(.*)$");
                        if (match.Success)
                        {
                            decimal.TryParse(match.Groups[1].Value, out qty);
                            unit = match.Groups[2].Value;
                        }
                    }

                    // Calculate total
                    item.total_price = (item.price * qty) - item.discount;

                    // PROCESS AS RETURN
                    if (qty < 0)
                    {
                        Sale? originalSale = null;
                        
                        // If we have an ID (from a loaded invoice), use it for precise matching
                        if (item.id > 0)
                        {
                            originalSale = _context.sales.FirstOrDefault(s => s.id == item.id);
                        }
                        
                        // Fallback to searching by name (original logic)
                        if (originalSale == null)
                        {
                            originalSale = _context.sales
                                .Where(s => s.prod_name_service == item.prod_name_service)
                                .OrderByDescending(s => s.date)
                                .FirstOrDefault();
                        }

                        if (originalSale != null)
                        {
                            // Parse original quantity and UNIT
                            decimal originalQty = 0;
                            string originalUnit = "";
                            var originalMatch = System.Text.RegularExpressions.Regex.Match(originalSale.qty_unit_type ?? "", @"^([0-9.-]+)\s*(.*)$");
                            if (originalMatch.Success)
                            {
                                decimal.TryParse(originalMatch.Groups[1].Value, out originalQty);
                                originalUnit = originalMatch.Groups[2].Value;
                            }

                            // Calculate new quantity
                            decimal newQty = originalQty + qty; // qty is negative
                            if (newQty < 0) newQty = 0;

                            // PRORATE DISCOUNT based on remaining items
                            decimal unitDiscount = originalQty > 0 ? originalSale.discount / originalQty : 0;
                            decimal newDiscount = unitDiscount * newQty;
                            decimal returnDiscount = unitDiscount * Math.Abs(qty);

                            // Update original sale: quantity, discount and price
                            originalSale.qty_unit_type = $"{newQty} {originalUnit}".Trim();
                            originalSale.discount = newDiscount;
                            originalSale.total_price = (originalSale.price * newQty) - newDiscount;

                            // ---- POS Logic: Mark original sale as returned if fully returned ----
                            // Mirrors: "update pos_sales_accounts set is_returned = 'true' where billNo = ..."
                            if (newQty == 0)
                            {
                                originalSale.is_returned = true;
                                originalSale.status = "Returned";
                            }
                            _context.sales.Update(originalSale);

                            // Create a dedicated return record (mirrors pos_return_accounts + pos_returns_details)
                            returnsToInsert.Add(new ReturnDetail
                            {
                                billNo = billNo,
                                SaleId = originalSale.id,
                                Date = now,
                                ProdNameService = item.prod_name_service,
                                Barcode = item.barcode,
                                QtyUnitType = $"{Math.Abs(qty)} {unit}".Trim(),
                                Amount = (originalSale.price * Math.Abs(qty)) - returnDiscount,
                                Method = payment_method,
                                Status = "Return"
                            });
                        }
                        else
                        {
                            // No original sale found - validation error
                            ModelState.AddModelError("", $"No previous sale found for '{item.prod_name_service}' under '{customer_name}'. Return denied.");
                            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
                            ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
                            return View();
                        }
                    }
                    else
                    {
                        item.billNo = billNo;
                        salesToInsert.Add(item);
                    }

                    // Update Inventory
                    var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == item.prod_name_service);
                    if (product != null && !string.IsNullOrEmpty(product.qty_unit_type))
                    {
                        var prodMatch = System.Text.RegularExpressions.Regex.Match(product.qty_unit_type.Trim(), @"^([0-9.-]+)\s*(.*)$");
                        if (prodMatch.Success && decimal.TryParse(prodMatch.Groups[1].Value, out decimal currentQty))
                        {
                            string prodUnit = prodMatch.Groups[2].Value;
                            if (!string.IsNullOrEmpty(prodUnit)) prodUnit = " " + prodUnit;
                            
                            // Check for Stock Limit if selling (qty > 0)
                            if (qty > 0 && currentQty < qty)
                            {
                                ModelState.AddModelError("", $"Insufficient stock for {item.prod_name_service}. Available: {currentQty}, Requested: {qty}");
                                
                                // Reload lists
                                ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
                                ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
                                return View();
                            }

                            currentQty -= qty; // If qty is negative (return), this adds to stock.
                            
                            product.qty_unit_type = $"{currentQty}{prodUnit}";
                            _context.products_services.Update(product);
                        }
                    }
                }
                
                if (salesToInsert.Any()) _context.sales.AddRange(salesToInsert);
                if (returnsToInsert.Any()) _context.returns.AddRange(returnsToInsert);

                // ---- CREDIT LOGIC: Update Customer Balance ----
                if (payment_method == "Credit" && !string.IsNullOrEmpty(customer_name))
                {
                    var customer = _context.customers.FirstOrDefault(c => c.name == customer_name);
                    if (customer != null)
                    {
                        decimal orderTotal = salesToInsert.Sum(s => s.total_price);
                        
                        // Check Credit Limit (as in Spices_pos)
                        if (customer.CreditLimit > 0 && (customer.Balance + orderTotal) > customer.CreditLimit)
                        {
                            ModelState.AddModelError("", $"Credit limit exceeded for {customer_name}. Limit: {customer.CreditLimit}, Current Balance: {customer.Balance}, Order: {orderTotal}");
                            
                            // Reload lists and return to view (abort save)
                            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
                            ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
                            return View();
                        }

                        // Update Balance
                        customer.Balance += orderTotal;
                        _context.customers.Update(customer);
                    }
                }
                
                _context.SaveChanges();
                TempData["Success"] = "Transaction processed successfully!";
                return RedirectToAction(nameof(Create));
            }

            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
            ViewBag.Products = _context.products_services
                .Where(p => p.status == "Available")
                .ToList();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var sale = _context.sales.FirstOrDefault(s => s.id == id);
                if (sale != null)
                {
                    _context.sales.Remove(sale);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // If it's already gone, we don't need to do anything
                if (!_context.sales.Any(s => s.id == id))
                {
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public JsonResult GetProductDetails(int productId)
        {
            var product = _context.products_services.Find(productId);
            if (product == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                price = product.price,
                discount = product.discount,
                tax = product.tax,
                qty_unit_type = product.qty_unit_type,
                barcode = product.barcode,
                name = product.prod_name_service
            });
        }

        [HttpGet]
        public JsonResult CheckPurchaseHistory(string productName)
        {
            if (string.IsNullOrEmpty(productName))
            {
                return Json(new { hasPurchased = false, purchasedQty = 0 });
            }

            var sales = _context.sales
                .Where(s => s.prod_name_service == productName)
                .ToList();

            decimal totalPurchased = 0;
            foreach (var s in sales)
            {
                var match = System.Text.RegularExpressions.Regex.Match(s.qty_unit_type ?? "", @"^([0-9.-]+)");
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal q))
                {
                    if (q > 0) totalPurchased += q;
                    // Note: We don't subtract returns here because the system updates the original sale record's quantity when a return is processed. 
                    // So originalSale.qty_unit_type always reflects the REMAINING quantity available to return.
                }
            }

            return Json(new { 
                hasPurchased = totalPurchased > 0, 
                purchasedQty = totalPurchased 
            });
        }

        [HttpGet]
        public JsonResult CheckAnyPurchaseHistory()
        {
            // Check for any sales where qty > 0 (using the same parsing logic as Index)
            var sales = _context.sales.ToList();
            var hasAnyHistory = sales.Any(s => {
                var match = System.Text.RegularExpressions.Regex.Match(s.qty_unit_type ?? "", @"^([0-9.-]+)");
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal q))
                {
                    return q > 0;
                }
                return false;
            });

            return Json(new { hasAnyHistory = hasAnyHistory });
        }

        [HttpGet]
        public JsonResult GetNextBillNo(string mode)
        {
            if (mode == "return")
            {
                var latestReturn = _context.returns
                    .Where(r => r.billNo != null && r.billNo.StartsWith("RETURN_"))
                    .OrderByDescending(r => r.Date)
                    .ThenByDescending(r => r.Id)
                    .Select(r => r.billNo)
                    .FirstOrDefault();

                int num = 0;
                if (latestReturn != null && int.TryParse(latestReturn.Substring(7), out int n)) num = n;

                return Json(new { billNo = $"RETURN_{num + 1}" });
            }
            else
            {
                var latestSale = _context.sales
                    .Where(s => s.billNo != null && s.billNo.StartsWith("SALE_"))
                    .OrderByDescending(s => s.date)
                    .ThenByDescending(s => s.id)
                    .Select(s => s.billNo)
                    .FirstOrDefault();

                int num = 0;
                if (latestSale != null && int.TryParse(latestSale.Substring(5), out int n)) num = n;

                return Json(new { billNo = $"SALE_{num + 1}" });
            }
        }

        [HttpGet]
        public JsonResult GetItemsByBillNo(string billNo)
        {
            if (string.IsNullOrEmpty(billNo)) return Json(new { success = false });

            // Fetch all items with this billNo
            var itemsRaw = _context.sales
                .Where(s => s.billNo == billNo)
                .ToList();

            var items = itemsRaw.Select(s => {
                decimal qty = 0;
                var match = System.Text.RegularExpressions.Regex.Match(s.qty_unit_type ?? "", @"^([0-9.-]+)");
                if (match.Success) decimal.TryParse(match.Groups[1].Value, out qty);

                return new
                {
                    id = s.id,
                    prod_name_service = s.prod_name_service,
                    barcode = s.barcode,
                    qty = qty, 
                    price = s.price,
                    discount = s.discount,
                    expiry_date = s.expiry_date,
                    total_price = s.total_price
                };
            })
            .Where(x => x.qty > 0) // Only load items that haven't been fully returned
            .ToList();

            if (!items.Any()) return Json(new { success = false, message = "No valid items found for this Bill No." });

            return Json(new { success = true, items = items });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReturn(int id)
        {
            var returnRecord = _context.returns.Find(id);
            if (returnRecord != null)
            {
                _context.returns.Remove(returnRecord);
                _context.SaveChanges();
                TempData["Success"] = "Return record deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
