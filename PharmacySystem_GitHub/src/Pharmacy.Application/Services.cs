using Pharmacy.Domain;

namespace Pharmacy.Application;

public interface IMedicineRepository
{
    Medicine? GetById(int id);
    Medicine? GetByName(string name);
    IReadOnlyList<Medicine> GetAll();
    IReadOnlyList<Medicine> Search(string query);
    void Add(Medicine medicine);
    void Update(Medicine medicine);
}

public interface ISaleRepository
{
    void Add(Sale sale);
}

public sealed class MedicineService
{
    private readonly IMedicineRepository _medicines;
    private readonly ISaleRepository _sales;

    public MedicineService(IMedicineRepository medicines, ISaleRepository sales)
    {
        _medicines = medicines;
        _sales = sales;
    }

    public IReadOnlyList<Medicine> GetAll() => _medicines.GetAll();

    public Medicine Add(string name, string category, decimal price, int quantity, DateTime expiryDate)
    {
        Validate(name, category, price, quantity, expiryDate);
        if (_medicines.GetByName(name.Trim()) is not null)
        {
            throw new ArgumentException("Лекарство с таким названием уже существует.");
        }

        var medicine = new Medicine
        {
            Name = name.Trim(),
            Category = category.Trim(),
            Price = price,
            Quantity = quantity,
            ExpiryDate = expiryDate.Date,
            IsActive = true
        };
        _medicines.Add(medicine);
        return medicine;
    }

    public IReadOnlyList<Medicine> Search(string query) => _medicines.Search(query.Trim());

    public void Update(Medicine medicine)
    {
        Validate(medicine.Name, medicine.Category, medicine.Price, medicine.Quantity, medicine.ExpiryDate);
        var duplicate = _medicines.GetByName(medicine.Name.Trim());
        if (duplicate is not null && duplicate.Id != medicine.Id)
        {
            throw new ArgumentException("Лекарство с таким названием уже существует.");
        }
        _medicines.Update(medicine);
    }

    public decimal Sell(int medicineId, int quantity, DateTime saleDate)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Количество продажи должно быть больше нуля.");

        var medicine = _medicines.GetById(medicineId)
            ?? throw new KeyNotFoundException("Лекарство не найдено.");

        if (!medicine.IsActive)
            throw new InvalidOperationException("Лекарство недоступно для продажи.");

        if (saleDate.Date > medicine.ExpiryDate.Date)
            throw new InvalidOperationException("Срок годности лекарства истёк.");

        if (medicine.Quantity < quantity)
            throw new InvalidOperationException("Недостаточно товара на складе.");

        medicine.Quantity -= quantity;
        _medicines.Update(medicine);
        var total = medicine.Price * quantity;
        _sales.Add(new Sale { MedicineId = medicineId, Quantity = quantity, Total = total, CreatedAt = saleDate });
        return total;
    }

    public bool IsLowStock(Medicine medicine, int threshold = 5) => medicine.Quantity <= threshold;

    private static void Validate(string name, string category, decimal price, int quantity, DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название обязательно.");
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Категория обязательна.");
        if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price), "Цена должна быть больше нуля.");
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Остаток не может быть отрицательным.");
        if (expiryDate.Date < DateTime.Today) throw new ArgumentException("Срок годности не может быть в прошлом.");
    }
}
