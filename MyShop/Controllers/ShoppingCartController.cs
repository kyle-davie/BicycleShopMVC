using Microsoft.AspNetCore.Mvc;
using MyShop.Models;

namespace MyShop.Controllers
{
	public class ShoppingCartController : Controller
	{

		private readonly ShopContext _context;
		private List<ShoppingCartItem> _cartItems;

		public ShoppingCartController(ShopContext context)
		{
			_context = context;
			_cartItems = new List<ShoppingCartItem>();
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult AddToCart(int id)
		{
			var bicycleToAdd = _context.Bicycles.Find(id);
			var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
			var existingCartItems = cartItems.FirstOrDefault(item => item.Bicycle.Id == id);
			if (existingCartItems != null)
			{
				existingCartItems.Quantity++;
			}
			else
			{
				cartItems.Add(new ShoppingCartItem { Bicycle = bicycleToAdd, Quantity = 1 });
			}

			HttpContext.Session.Set("Cart", cartItems);
			TempData["CartMessage"] = $"{bicycleToAdd.Brand} {bicycleToAdd.Model} added to cart";

			return RedirectToAction("ViewCart");
		}

		public IActionResult ViewCart()
		{
			var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
			var cartViewModel = new ShoppingCartViewModel
			{
				CartItems = cartItems,
				TotalPrice = (decimal)cartItems.Sum(item => item.Bicycle.Price * item.Quantity),
			};

			ViewBag.CartMessage = TempData["CartMessage"];

			return View(cartViewModel);
		}

		public IActionResult RemoveItem(int id)
		{
			var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
			var itemToRemove = cartItems.FirstOrDefault(item => item.Bicycle.Id == id);
			TempData["CartMessage"] = $"{itemToRemove.Bicycle.Brand} {itemToRemove.Bicycle.Model} removed from cart";
			if (itemToRemove != null)
			{
				if (itemToRemove.Quantity > 1)
				{
					itemToRemove.Quantity--;
				}
				else
				{
					cartItems.Remove(itemToRemove);
				}
			}
			HttpContext.Session.Set("Cart", cartItems);

			return RedirectToAction("ViewCart");
		}

		public IActionResult PurchaseItems()
		{
			var cartItems = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart") ?? new List<ShoppingCartItem>();
			foreach (var item in cartItems) {
				//save each item as a pruhcase
				_context.Purchases.Add(new Purchase
				{
					BicycleId = item.Bicycle.Id,
					Quantity = item.Quantity,
					PurchaseDate = DateTime.Now,
					Total = item.Bicycle.Price * item.Quantity
				});
			}
			// save changes to the database
			_context.SaveChanges();

			// clear the cart
			HttpContext.Session.Set("Cart", new List<ShoppingCartItem>());
			return RedirectToAction("Index", "Home");

		}
	}
}
