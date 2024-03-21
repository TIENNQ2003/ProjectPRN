using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using TratieShop.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace TratieShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly TraTieShopContext context = new TraTieShopContext();
        public IActionResult Index()
        {
            return View("Index");
        }
        public IActionResult listProduct(int cateID = 0, int page = 1, int pageSize = 12)
        {
            List<Category> categories = context.Categories.ToList();
            List<Product> products = new();
            int totalItems;

            if (cateID != 0)
            {
                products = context.Products.Include(x => x.Category)
                                           .Where(x => x.CategoryId == cateID)
                                           .Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();

                totalItems = context.Products.Where(x => x.CategoryId == cateID).Count();
            }
            else
            {
                products = context.Products.Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();

                totalItems = context.Products.Count();
            }

            ViewData["Categories"] = categories;
            ViewData["products"] = products;
            ViewData["cateID"] = cateID;

            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;

            return View("Index");
        }
        public IActionResult login()
        {
            return View("Login");
        }
        public IActionResult Register()
        {
            return View("Register");
        }
        public IActionResult loginURL()
        {
            string userName = Request.Form["Username"];
            string password = Request.Form["Password"];

            // Kiểm tra người dùng tồn tại trong cơ sở dữ liệu
            User account = context.Users.FirstOrDefault(x => x.UserName == userName && x.PassWord == password);

            if (account == null)
            {
                ViewBag.message = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View("Login");
            }
            else
            {
                HttpContext.Session.SetString("account", JsonSerializer.Serialize(account));
                if (account.RoleId == 2) {
                    return RedirectToAction("listProduct", new { cateID = 0 });
                }
                else
                {
                    return RedirectToAction("AdminPage");
                }
            }
        }
        public IActionResult ViewProfile()
        {
            return View("ViewProfile");
        }
        public IActionResult ProfileURL(int uID)
        {
            List<Category> categories = context.Categories.ToList();
            string userName = Request.Form["FullName"];
            string Address = Request.Form["Address"];
            string Phone = Request.Form["Phone"];
            string Gender = Request.Form["Gender"];
            string Email = Request.Form["Email"];
            User user = context.Users.FirstOrDefault(x => x.UserId == uID);
            user.FullName = userName;
            user.Address = Address;
            user.Phone = Phone;
            user.Gender = bool.Parse(Gender);
            user.Email = Email;
            context.Users.Update(user);
            context.SaveChanges();
            User account = context.Users.FirstOrDefault(x => x.UserId == uID);
            HttpContext.Session.SetString("account", JsonSerializer.Serialize(account));
            ViewBag.updateProfile = "UpdateSucess";
            ViewData["Categories"] = categories;
            return View("ViewProfile");
        }
        public IActionResult longOut()
        {
            HttpContext.Session.Remove("account");
            return RedirectToAction("listProduct", new { cateID = 0 });
        }
        public IActionResult MyPurchaseURL(int page = 1, int pageSize = 8)
        {
            String data = HttpContext.Session.GetString("account");
            User user = JsonSerializer.Deserialize<User>(data);
            List<Order> orders = context.Orders.Include(x=>x.Status).Where(x=>x.UserId==user.UserId).Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();
            ViewBag.Orders= orders;
            int totalItems;
            totalItems = context.Orders.Include(x => x.Status).Where(x => x.UserId == user.UserId).ToList().Count();
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            return View("ViewOrder");
        }
        public IActionResult ViewOrderDetail( int oId)
        {
           List < OrderDetail > orderDetailss = context.OrderDetails.Include(x => x.Product).Where(x => x.OrderId == oId).ToList();
            ViewBag.OrderDetails = orderDetailss;
            return View("ViewOrderDetail");
        }
        public IActionResult addToCart(int proID, int cateID)
        {
            List<OrderDetail> orders;
            if (HttpContext.Session.GetString("cart") != null)
            {
                String data = HttpContext.Session.GetString("cart");
                orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);

            }
            else
            {
                orders = new List<OrderDetail>();
            }
            OrderDetail order = orders.FirstOrDefault(x => x.ProductId == proID);
            if (order != null)
            {
                order.Quantity++;
            }
            else
            {
                order = new OrderDetail();
                order.Quantity = 1;
                order.ProductId = proID;
                orders.Add(order);
            }
            ViewBag.Message = "sucess";
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(orders));
            List<Category> categories = context.Categories.ToList();
            List<Product> products = new();
            if (cateID != 0)
            {
                products = context.Products.Include(x => x.Category).Where(x => x.CategoryId == cateID).ToList();
            }
            else
            {
                products = context.Products.ToList();
            }
            ViewData["Categories"] = categories;
            ViewData["products"] = products;
            ViewData["cateID"] = cateID;
            return View("Index");
        }
        public IActionResult Cart()
        {
            List<OrderDetail> orders;
            if (HttpContext.Session.GetString("cart") != null)
            {
                String data = HttpContext.Session.GetString("cart");
                orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);

            }
            else
            {
                orders = new List<OrderDetail>();
            }
            orders.ForEach(
               x =>
               {
                   x.Product = context.Products.FirstOrDefault(z => z.ProductId == x.ProductId);
               });
            ViewBag.Cart = orders;
            return View("ViewCart");
        }
        public IActionResult DeleteCart(int Id)
        {
            List<OrderDetail> orders;
            if (HttpContext.Session.GetString("cart") != null)
            {
                String data = HttpContext.Session.GetString("cart");
                orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);

            }
            else
            {
                orders = new List<OrderDetail>();
            }
            OrderDetail order = orders.FirstOrDefault(x => x.ProductId == Id);
            orders.Remove(order);
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(orders));
            orders.ForEach(
               x =>
               {
                   x.Product = context.Products.FirstOrDefault(z => z.ProductId == x.ProductId);
               });
            ViewBag.Cart = orders;
            ViewBag.delete = "sucess";
            return View("ViewCart");
        }
        
        public IActionResult UpdateCart(List<int> proID, List<int> quantity)
        {
            List<OrderDetail> orders;
            if (HttpContext.Session.GetString("cart") != null)
            {
                String data = HttpContext.Session.GetString("cart");
                orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);

            }
            else
            {
                orders = new List<OrderDetail>();
            }
            for (int i = 0; i < proID.Count; i++)
            {
                OrderDetail order = orders.FirstOrDefault(x => x.ProductId == proID[i]);
                order.Quantity = quantity[i];
            }
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(orders));
            orders.ForEach(
               x =>
               {
                   x.Product = context.Products.FirstOrDefault(z => z.ProductId == x.ProductId);
               });
            ViewBag.Cart = orders;
            ViewBag.UpdateCart = "sucess";
            return View("ViewCart");
        }
        public IActionResult CheckOut()
        {
            List<OrderDetail> orders;
            if (HttpContext.Session.GetString("cart") != null)
            {
                String data = HttpContext.Session.GetString("cart");
                orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);

            }
            else
            {
                orders = new List<OrderDetail>();
            }
            orders.ForEach(
               x =>
               {
                   x.Product = context.Products.FirstOrDefault(z => z.ProductId == x.ProductId);
               });
            ViewBag.Cart = orders;
            return View("CheckOut");
        }
        public IActionResult PlaceOrder()
        {
            int maxOrderID = context.Orders.Max(o => o.OrderId) + 1;
            string name = Request.Form["name"];
            string phone = Request.Form["phone"];
            string address = Request.Form["address"];
            string totalAmount = Request.Form["totalAmount"];
            String account = HttpContext.Session.GetString("account");
             User user = JsonSerializer.Deserialize<User>(account);
            Order order= new Order();
            order.OrderId = maxOrderID;
            order.UserId = user.UserId;
            order.OrderDate=DateTime.Now;
            order.StatusId = 0;
            order.ReceiverName = name;
            order.Phone= phone;
            order.DeliverAddress= address;
            order.TotalPrice=decimal.Parse(totalAmount);
            context.Orders.Add(order);
            List<OrderDetail> orderDetails;
                String data = HttpContext.Session.GetString("cart");
                orderDetails = JsonSerializer.Deserialize<List<OrderDetail>>(data);
            for (int i = 0; i < orderDetails.Count; i++)
            {
                OrderDetail orDetail = orderDetails[i];
                Product product = context.Products.FirstOrDefault(x => x.ProductId == orDetail.ProductId);
                orDetail.UnitPrice = product.UnitPrice * orDetail.Quantity;
                orDetail.OrderId = maxOrderID;
                context.OrderDetails.Add(orDetail);
            }

            // Sau khi đã thêm tất cả các OrderDetail, bạn cần lưu thay đổi vào cơ sở dữ liệu
            context.SaveChanges();
            HttpContext.Session.Remove("cart");
            List<Category> categories = context.Categories.ToList();
            List<Product> products = context.Products.ToList();
            ViewData["Categories"] = categories;
            ViewData["products"] = products;
            ViewBag.OrderSucess = "sucess";
            return View("Index");
        }
        public IActionResult UpdateStatusUser(int orderID)
        {
            Order order = context.Orders.FirstOrDefault(x=>x.OrderId== orderID);
            if (order != null)
            {
                order.StatusId = 3;
            }
            context.Orders.Update(order);
            context.SaveChanges();
            String data = HttpContext.Session.GetString("account");
            User user = JsonSerializer.Deserialize<User>(data);
            List<Order> orders = context.Orders.Include(x => x.Status).Where(x => x.UserId == user.UserId).ToList();
            ViewBag.Orders = orders;
            ViewBag.updateSucess = "Sucess";
            return View("ViewOrder");
        }
        public IActionResult AdminPage(int id=0, int page = 1, int pageSize = 8)
        {
            if (id == 0 || id == 1)
            {
                List<Order> orders = context.Orders.Include(x => x.Status).Include(y=>y.User).Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();
                ViewBag.Orders = orders;
                int totalItems;
                totalItems = context.Orders.Include(x => x.Status).Include(y => y.User).ToList().Count();
                ViewBag.TotalItems = totalItems;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = page;
                return View("OrderManagement");
            }
            else if (id == 2)
            {
                List<Product> products = context.Products.Include(x => x.Category).Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();
                int totalItems;
                totalItems = context.Products.Include(x => x.Category).ToList().Count();
                ViewBag.TotalItems = totalItems;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = page;
                ViewBag.Products = products;
                return View("ProductManagement");
            }
            else if (id == 3)
            {
                List<Category> categories = context.Categories.ToList();
                ViewBag.Category = categories;
                return View("CategoryManagement");
            }
            else
            {
                List<User> users = context.Users.ToList();
                ViewBag.User = users;
                return View("UserManagement");
            }
        }
        public IActionResult UpdateStatusAdmin(int orderID)
        {
            Order order = context.Orders.FirstOrDefault(x => x.OrderId == orderID);
            if (order != null)
            {
                if(order.StatusId==0)
                {
                    order.StatusId = 1;
                    order.RequireDate = DateTime.Now;
                }
                else
               {
                    order.StatusId = 2;
                    order.ShippedDate = DateTime.Now;
                }
                
            }
            context.Orders.Update(order);
            context.SaveChanges();
            return RedirectToAction("AdminPage");
        }
        public IActionResult ViewUserDetail(int uID)
        {
            User user = context.Users.FirstOrDefault(x => x.UserId == uID);
            ViewBag.User = user;
            return View("ViewUserDetail");
        }
        public  IActionResult ViewCategoryDetail(int cateId) {
            Category category = context.Categories.FirstOrDefault(x => x.CategoryId == cateId);
            if (category == null)
            {
                return NotFound(); // Hoặc xử lý khi không tìm thấy dữ liệu
            }
            else
            {
                ViewBag.Category = category;

            }
            return View("ViewCategoryDetail");
        }
        public IActionResult ChangeCategory()
        {
            string cateID = Request.Form["cateID"];
            string CateName = Request.Form["CateName"];
            Category category = new Category();
            category.CategoryId = int.Parse(cateID);
            category.CategoryName = CateName;
            context.Categories.Update(category);
            context.SaveChanges();
            ViewBag.Category = category;
            ViewBag.Message = "Update Sucess";
            return View("ViewCategoryDetail");
        }
        public IActionResult InsertCategory()
        {
            return View("AddNewCategory");
        }
        public IActionResult InsertNewCategory()
        {
            string cateID = Request.Form["cateID"];
            string CateName = Request.Form["CateName"];
            Category category = context.Categories.FirstOrDefault(x => x.CategoryId == int.Parse(cateID));
            Category newCategory = new Category();
            newCategory.CategoryId = int.Parse(cateID);
            newCategory.CategoryName = CateName;
            if (category == null)
            {
                context.Categories.Add(newCategory);
                context.SaveChanges();
                ViewBag.Category = newCategory;
                ViewBag.Message = "Add Sucess";
            }
            else
            {
                ViewBag.Category = newCategory;
                ViewBag.Message = "Add fail";
            }
            
            return View("AddNewCategory");
        }
        public IActionResult registerURL()
        {
            string Fullname = Request.Form["Fullname"];
            string Address = Request.Form["Address"];
            string Phone = Request.Form["Phone"];
            string Username = Request.Form["Username"];
            string Password = Request.Form["Password"];
            User account = context.Users.FirstOrDefault(x => x.Phone == Phone && x.UserName == Username);
            int maxOrderID = context.Users.Max(o => o.UserId) + 1;
            User newUser = new User();
            newUser.FullName = Fullname;
            newUser.Address = Address;
            newUser.Phone = Phone;
            newUser.UserName = Username;
            newUser.PassWord = Password;
            newUser.RoleId = 1;
            newUser.UserId = maxOrderID;
            if (account == null)
            {
                context.Users.Add(newUser);
                context.SaveChanges();
                ViewBag.User = newUser;
                ViewBag.Message = "Add Sucess";
            }
            else
            {
                ViewBag.User = newUser;
                ViewBag.Message = "Add fail";
            }
            return View("Register");
        }
        public IActionResult InsertProduct()
        {
            List<Category> categories = context.Categories.ToList();
            ViewBag.Category = categories;
            return View("AddNewProduct");
        }
        public IActionResult addProductURL()
        {
            string ProductName = Request.Form["ProductName"];
            string CategoryID = Request.Form["CategoryID"];
            string Descrsiption = Request.Form["Descrsiption"];
            string UnitPrice = Request.Form["UnitPrice"];
            string UnitInStock = Request.Form["UnitInStock"];
            string Discount = Request.Form["Discount"];
            string Status = Request.Form["Status"];
            Product pro = context.Products.FirstOrDefault(x => x.ProductName == ProductName);
            int maxProductID = context.Products.Max(o => o.ProductId) + 1;
            Product newPro = new Product();
            newPro.ProductId = maxProductID;
            newPro.ProductName = ProductName;
            newPro.CategoryId = int.Parse(CategoryID);
            newPro.Description = Descrsiption;
            newPro.UnitPrice=decimal.Parse(UnitPrice);
            newPro.UnitinStock = int.Parse(UnitInStock);
            newPro.Discount=int.Parse(Discount);
            newPro.Status=int.Parse(Status);
            if (pro == null)
            {
                context.Products.Add(newPro);
                context.SaveChanges();
                ViewBag.newProduct = newPro;
                ViewBag.Message = "Add Sucess";
            }
            else
            {
                ViewBag.newProduct = newPro;
                ViewBag.Message = "Add Fail";
            }
            List<Category> categories = context.Categories.ToList();
            ViewBag.Category = categories;
            return View("AddNewProduct");
        }
        public IActionResult searchProductURL()
        {
            string key = Request.Form["key"];
            List<Product> products = context.Products.Where(p => p.ProductName.Contains(key)).ToList();
            ViewData["products"] = products;
            ViewBag.key = key;
            return View("Index");
        }
        public IActionResult OrderFilterURL(int page = 1, int pageSize = 8)
        {
            string DateFrom = Request.Form["DateFrom"];
            string DateTo = Request.Form["DateTo"];
            string status = Request.Form["status"];
            List<Order> OrderList = context.Orders.Include(x => x.Status).Include(x => x.User).ToList();

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(DateFrom))
            {
                DateTime fromDate = DateTime.Parse(DateFrom);
                OrderList = OrderList.Where(x => x.OrderDate >= fromDate).ToList();
                ViewBag.DateFrom = DateFrom;
            }

            if (!string.IsNullOrEmpty(DateTo))
            {
                DateTime toDate = DateTime.Parse(DateTo);
                OrderList = OrderList.Where(x => x.OrderDate <= toDate).ToList();
                ViewBag.DateTo = DateTo;
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                int statusId = int.Parse(status);
                OrderList = OrderList.Where(x => x.StatusId == statusId).ToList();
                ViewBag.Status = statusId;
            }
            int totalItems;
            totalItems = OrderList.Count();
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            OrderList = OrderList.Skip((page - 1) * pageSize)
                                           .Take(pageSize).ToList();
            ViewBag.Orders = OrderList;
            return View("OrderManagement");
        }
        public IActionResult ViewProductDetail(int pId)
        {
            Product pro = context.Products.FirstOrDefault(x => x.ProductId == pId);
            List<Category> categories = context.Categories.ToList();
            ViewBag.Category = categories;
            ViewBag.Product = pro;
            return View("ViewProductDetail");
        }
       public IActionResult UpdateProductURL()
        {
            string pid = Request.Form["pid"];
            string ProductName = Request.Form["ProductName"];
            string CategoryID = Request.Form["CategoryID"];
            string Descrsiption = Request.Form["Descrsiption"];
            string UnitPrice = Request.Form["UnitPrice"];
            string UnitInStock = Request.Form["UnitInStock"];
            string Discount = Request.Form["Discount"];
            string Status = Request.Form["Status"];
            Product pro = context.Products.FirstOrDefault(x => x.ProductId == int.Parse(pid));
            pro.ProductName = ProductName;
            pro.CategoryId = int.Parse(CategoryID);
            pro.Description = Descrsiption;
            pro.UnitPrice = decimal.Parse(UnitPrice);
            pro.UnitinStock = int.Parse(UnitInStock);
            pro.Discount = int.Parse(Discount);
            pro.Status = int.Parse(Status);
            context.Products.Update(pro);
            context.SaveChanges();
            List<Category> categories = context.Categories.ToList();
            ViewBag.Product = pro;
            ViewBag.Category = categories;
            @ViewBag.Message = "Change Sucess";
            return View("ViewProductDetail");
        }
        public IActionResult ChangePassword()
        {
            return View("ChangePassword");
        }
        public IActionResult ChangePasswordURL()
        {
            string OldPassword = Request.Form["OldPassword"];
            string NewPassword = Request.Form["NewPassword"];
            string ConfirmNewPassword = Request.Form["ConfirmNewPassword"];
            String data = HttpContext.Session.GetString("account");
            User user = JsonSerializer.Deserialize<User>(data);
            if (!user.PassWord.Equals(OldPassword))
            {
                ViewBag.message = "Current Password Incorrect";
            }
            else
            {
                if (!NewPassword.Equals(ConfirmNewPassword))
                {
                    ViewBag.message = "Confirm new password not match new Password Incorrect";
                }
                else
                {
                    user.PassWord = NewPassword;
                    context.Users.Update(user);
                    context.SaveChanges();
                    User account = context.Users.FirstOrDefault(x => x.UserId == user.UserId);
                    HttpContext.Session.SetString("account", JsonSerializer.Serialize(account));
                    ViewBag.message = "ChangeSucess";
                }
            }
            ViewBag.OldPassword = OldPassword;
            ViewBag.NewPassword = NewPassword;
            ViewBag.ConfirmNewPassword = ConfirmNewPassword;
            return View("ChangePassword");
        }
    }
}