using Pharmacy.Application;
using Pharmacy.Domain;
using Xunit;

namespace Pharmacy.Application.Tests;

public class MedicineServiceTests
{
    private static MedicineService Create(out FakeMedicineRepository medicines, out FakeSaleRepository sales)
    {
        medicines = new FakeMedicineRepository();
        sales = new FakeSaleRepository();
        return new MedicineService(medicines, sales);
    }

    [Fact]
    public void Add_ValidData_CreatesMedicine()
    {
        var service = Create(out _, out _);
        var result = service.Add("Парацетамол", "Обезболивающие", 120m, 10, DateTime.Today.AddYears(1));
        Assert.Equal("Парацетамол", result.Name);
        Assert.Equal(10, result.Quantity);
    }

    [Fact]
    public void Add_EmptyName_Throws()
    {
        var service = Create(out _, out _);
        Assert.Throws<ArgumentException>(() => service.Add(" ", "Обезболивающие", 120m, 10, DateTime.Today.AddYears(1)));
    }

    [Fact]
    public void Add_ZeroPrice_Throws()
    {
        var service = Create(out _, out _);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Add("Парацетамол", "Обезболивающие", 0m, 10, DateTime.Today.AddYears(1)));
    }

    [Fact]
    public void Add_NegativeQuantity_Throws()
    {
        var service = Create(out _, out _);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Add("Парацетамол", "Обезболивающие", 120m, -1, DateTime.Today.AddYears(1)));
    }

    [Fact]
    public void Add_PastExpiryDate_Throws()
    {
        var service = Create(out _, out _);
        Assert.Throws<ArgumentException>(() => service.Add("Парацетамол", "Обезболивающие", 120m, 10, DateTime.Today.AddDays(-1)));
    }

    [Fact]
    public void Add_DuplicateName_Throws()
    {
        var service = Create(out var medicines, out _);
        medicines.Seed(new Medicine { Id = 1, Name = "Парацетамол", Category = "Обезболивающие", Price = 120m, Quantity = 10, ExpiryDate = DateTime.Today.AddYears(1) });
        Assert.Throws<ArgumentException>(() => service.Add("Парацетамол", "Обезболивающие", 130m, 5, DateTime.Today.AddYears(1)));
    }

    [Fact]
    public void Search_TrimsQuery()
    {
        var service = Create(out var medicines, out _);
        medicines.Seed(new Medicine { Id = 1, Name = "Ибупрофен", Category = "Обезболивающие", Price = 99m, Quantity = 8, ExpiryDate = DateTime.Today.AddYears(1) });
        var result = service.Search("  Ибупрофен  ");
        Assert.Single(result);
        Assert.Equal("Ибупрофен", result[0].Name);
    }

    [Fact]
    public void Update_ValidMedicine_UpdatesRepository()
    {
        var service = Create(out var medicines, out _);
        var medicine = new Medicine { Id = 1, Name = "Но-шпа", Category = "Спазмолитики", Price = 200m, Quantity = 4, ExpiryDate = DateTime.Today.AddYears(1) };
        service.Update(medicine);
        Assert.Equal(medicine, medicines.LastUpdated);
    }

    [Fact]
    public void Sell_ValidQuantity_DecreasesStockAndSavesSale()
    {
        var service = Create(out var medicines, out var sales);
        medicines.Seed(new Medicine { Id = 5, Name = "Амоксициллин", Category = "Антибиотики", Price = 300m, Quantity = 10, ExpiryDate = DateTime.Today.AddYears(1) });
        var total = service.Sell(5, 3, DateTime.Today);
        Assert.Equal(7, medicines.GetById(5)!.Quantity);
        Assert.Equal(900m, total);
        Assert.Single(sales.Items);
    }

    [Fact]
    public void Sell_ExactStock_IsAllowed()
    {
        var service = Create(out var medicines, out _);
        medicines.Seed(new Medicine { Id = 6, Name = "Анальгин", Category = "Обезболивающие", Price = 50m, Quantity = 4, ExpiryDate = DateTime.Today.AddYears(1) });
        service.Sell(6, 4, DateTime.Today);
        Assert.Equal(0, medicines.GetById(6)!.Quantity);
    }

    [Fact]
    public void Sell_OverStock_ThrowsAndDoesNotChangeStock()
    {
        var service = Create(out var medicines, out var sales);
        medicines.Seed(new Medicine { Id = 7, Name = "Аскорутин", Category = "Витамины", Price = 80m, Quantity = 2, ExpiryDate = DateTime.Today.AddYears(1) });
        Assert.Throws<InvalidOperationException>(() => service.Sell(7, 3, DateTime.Today));
        Assert.Equal(2, medicines.GetById(7)!.Quantity);
        Assert.Empty(sales.Items);
    }

    [Fact]
    public void Sell_ZeroQuantity_Throws()
    {
        var service = Create(out _, out _);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Sell(1, 0, DateTime.Today));
    }

    [Fact]
    public void Sell_ExpiredMedicine_Throws()
    {
        var service = Create(out var medicines, out _);
        medicines.Seed(new Medicine { Id = 8, Name = "Препарат", Category = "Прочее", Price = 10m, Quantity = 5, ExpiryDate = DateTime.Today.AddDays(-1) });
        Assert.Throws<InvalidOperationException>(() => service.Sell(8, 1, DateTime.Today));
    }

    [Fact]
    public void Sell_InactiveMedicine_Throws()
    {
        var service = Create(out var medicines, out _);
        medicines.Seed(new Medicine { Id = 9, Name = "Медикамент", Category = "Прочее", Price = 10m, Quantity = 5, ExpiryDate = DateTime.Today.AddYears(1), IsActive = false });
        Assert.Throws<InvalidOperationException>(() => service.Sell(9, 1, DateTime.Today));
    }

    [Fact]
    public void Sell_UnknownMedicine_Throws()
    {
        var service = Create(out _, out _);
        Assert.Throws<KeyNotFoundException>(() => service.Sell(999, 1, DateTime.Today));
    }

    [Fact]
    public void IsLowStock_AtThreshold_ReturnsTrue()
    {
        var service = Create(out _, out _);
        var medicine = new Medicine { Quantity = 5 };
        Assert.True(service.IsLowStock(medicine));
    }

    [Fact]
    public void IsLowStock_AboveThreshold_ReturnsFalse()
    {
        var service = Create(out _, out _);
        var medicine = new Medicine { Quantity = 6 };
        Assert.False(service.IsLowStock(medicine));
    }

    private sealed class FakeMedicineRepository : IMedicineRepository
    {
        private readonly List<Medicine> _items = new();
        public Medicine? LastUpdated { get; private set; }
        public Medicine? GetById(int id) => _items.SingleOrDefault(x => x.Id == id);
        public Medicine? GetByName(string name) => _items.SingleOrDefault(x => x.Name == name);
        public IReadOnlyList<Medicine> GetAll() => _items;
        public IReadOnlyList<Medicine> Search(string query) => _items.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || x.Category.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        public void Add(Medicine medicine) { medicine.Id = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1; _items.Add(medicine); }
        public void Update(Medicine medicine) { LastUpdated = medicine; var current = GetById(medicine.Id); if (current is not null) { current.Name = medicine.Name; current.Quantity = medicine.Quantity; current.Price = medicine.Price; current.Category = medicine.Category; current.ExpiryDate = medicine.ExpiryDate; current.IsActive = medicine.IsActive; } }
        public void Seed(Medicine medicine) => _items.Add(medicine);
    }

    private sealed class FakeSaleRepository : ISaleRepository
    {
        public List<Sale> Items { get; } = new();
        public void Add(Sale sale) => Items.Add(sale);
    }
}
