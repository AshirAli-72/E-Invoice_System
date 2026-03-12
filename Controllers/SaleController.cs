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
                        var originalSaleQuery = _context.sales.Where(s => s.prod_name_service == item.prod_name_service);
                        
                        var originalSale = originalSaleQuery
                            .OrderByDescending(s => s.date)
                            .FirstOrDefault();

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
                
                _context.SaveChanges();
                TempData["Success"] = "Transaction processed successfully!";
                return RedirectToAction(nameof(Index));
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
                var maxBill = _context.returns
                    .Where(r => r.billNo != null && r.billNo.StartsWith("RETURN_"))
                    .ToList()
                    .Select(r => 
                    {
                        if (int.TryParse(r.billNo.Substring(7), out int n)) return n;
                        return 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();
                return Json(new { billNo = $"RETURN_{maxBill + 1}" });
            }
            else
            {
                var maxBill = _context.sales
                    .Where(s => s.billNo != null && s.billNo.StartsWith("SALE_"))
                    .ToList()
                    .Select(s => 
                    {
                        if (int.TryParse(s.billNo.Substring(5), out int n)) return n;
                        return 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();
                return Json(new { billNo = $"SALE_{maxBill + 1}" });
            }
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
