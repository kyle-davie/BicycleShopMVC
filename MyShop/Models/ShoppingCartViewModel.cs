namespace MyShop.Models
{
	public class ShoppingCartViewModel
	{
		public List<ShoppingCartItem> CartItems { get; set; }
		public decimal? TotalPrice { get; set; }
		public int? TotalQuanity { get; set; }
	}
}
