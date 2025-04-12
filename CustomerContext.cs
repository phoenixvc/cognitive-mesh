public class CustomerContext
{
    public string CustomerId { get; set; }
    public string Name { get; set; }
    public string AccountType { get; set; }
    public List<string> PurchasedProducts { get; set; } = new List<string>();
    public List<string> RecentInteractions { get; set; } = new List<string>();
}
