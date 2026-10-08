using Microsoft.EntityFrameworkCore;
using Pharmacy.Application;
using Pharmacy.Domain;

namespace Pharmacy.Infrastructure;

public sealed class MedicineRepository : IMedicineRepository
{
    private readonly PharmacyDbContext _db;
    public MedicineRepository(PharmacyDbContext db) => _db = db;

    public Medicine? GetById(int id) => _db.Medicines.SingleOrDefault(x => x.Id == id);
    public Medicine? GetByName(string name) => _db.Medicines.SingleOrDefault(x => x.Name == name);
    public IReadOnlyList<Medicine> GetAll() => _db.Medicines.AsNoTracking().OrderBy(x => x.Name).ToList();
    public IReadOnlyList<Medicine> Search(string query) => string.IsNullOrWhiteSpace(query)
        ? GetAll()
        : _db.Medicines.AsNoTracking().Where(x => EF.Functions.Like(x.Name, $"%{query}%") || EF.Functions.Like(x.Category, $"%{query}%"))
            .OrderBy(x => x.Name).ToList();
    public void Add(Medicine medicine) { _db.Medicines.Add(medicine); _db.SaveChanges(); }
    public void Update(Medicine medicine) { _db.Medicines.Update(medicine); _db.SaveChanges(); }
}

public sealed class SaleRepository : ISaleRepository
{
    private readonly PharmacyDbContext _db;
    public SaleRepository(PharmacyDbContext db) => _db = db;
    public void Add(Sale sale) { _db.Sales.Add(sale); _db.SaveChanges(); }
}
