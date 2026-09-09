using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;

namespace Application.Seafood
{
    public class SeafoodDataSeeder
    {
        private readonly DreamContext _db;

        public SeafoodDataSeeder(DreamContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            if (await _db.CatalogLookups.AnyAsync()) return;

            var lookups = new List<CatalogLookup>();
            void Add(LookupCategory cat, string code, string name, string? desc = null, int order = 0)
                => lookups.Add(new CatalogLookup { Category = cat, Code = code, Name = name, Description = desc, SortOrder = order, IsActive = true });

            Add(LookupCategory.FishSpecies, "TUNA", "Cá Ngừ");
            Add(LookupCategory.FishForm, "WR", "Cá nguyên con", "Whole Round");
            Add(LookupCategory.FishForm, "GG", "Cá đã bỏ nội tạng và vây", "Gilled & Gutted");
            Add(LookupCategory.FishForm, "DWT", "Cá đã bỏ đầu", "Dressed without tail");
            Add(LookupCategory.SizeGrade, "10-20", "10-20kg", "Size từ 10 đến 20 kg");
            Add(LookupCategory.SizeGrade, "20UP", "20kg up", "Size từ 20 kg trở lên");
            Add(LookupCategory.SizeGrade, "20-30", "20-30kg", "Size từ 20 đến 30 kg");
            Add(LookupCategory.SizeGrade, "30UP", "30kg Up", "Size từ 30kg trở lên");
            Add(LookupCategory.Sensory, "DENT", "Cấn/ Móp");
            Add(LookupCategory.Sensory, "BEAUTIFUL", "Đẹp");
            Add(LookupCategory.Sensory, "DRY", "Khô");
            Add(LookupCategory.CatchMethod, "HL", "Handline");
            Add(LookupCategory.CatchMethod, "LL", "Long line");
            Add(LookupCategory.CatchMethod, "PS", "PS");
            Add(LookupCategory.CatchMethod, "PS_SALT", "PS Muối");
            Add(LookupCategory.FreezeMethod, "LAND", "Landfrozen");
            Add(LookupCategory.FreezeMethod, "SEA", "Seafrozen");
            Add(LookupCategory.Certificate, "EUCC", "EUCC", "Đi được cho thị trường Châu Âu");
            Add(LookupCategory.Certificate, "COA", "COA", "Đi được cho thị trường Mỹ");
            Add(LookupCategory.Certificate, "MSC", "MSC", "Giấy chứng nhận kèm theo, có thể kèm EUCC hoặc COA");
            Add(LookupCategory.Origin, "VN", "Việt Nam");
            Add(LookupCategory.Origin, "IMPORT", "Nhập khẩu");
            Add(LookupCategory.Market, "USA", "USA");
            Add(LookupCategory.Market, "EU", "Châu Âu");
            Add(LookupCategory.Market, "OTHER", "Khác");
            Add(LookupCategory.Warehouse, "MAIN", "Kho nhà máy");
            Add(LookupCategory.Carrier, "MSC", "MSC");
            Add(LookupCategory.Carrier, "MSK", "MSK");
            Add(LookupCategory.ContainerType, "20RF", "20RF'");
            Add(LookupCategory.ContainerType, "40RF", "40RF'");
            _db.CatalogLookups.AddRange(lookups);

            _db.MarketCertificateRules.AddRange(
                new MarketCertificateRule { MarketCode = "USA", MustHaveCertificateCode = "COA", AddonCertificateCode = "MSC" },
                new MarketCertificateRule { MarketCode = "EU", MustHaveCertificateCode = "EUCC", AddonCertificateCode = "MSC" },
                new MarketCertificateRule { MarketCode = "OTHER", MustHaveCertificateCode = null, Note = "Không đòi hỏi" }
            );

            var groups = new[]
            {
                new ProductGroup { Code = "LOIN_CO_USA", Name = "Tuna loin Co", DefaultMarket = "USA", SortOrder = 1 },
                new ProductGroup { Code = "TAIL_CO", Name = "Tuna tail Co", DefaultMarket = "USA", SortOrder = 2 },
                new ProductGroup { Code = "LOIN_CO_A", Name = "Tuna loin Co A", DefaultMarket = "USA", SortOrder = 3 },
                new ProductGroup { Code = "LOIN_VITAMIN", Name = "Tuna loin Vitamin", DefaultMarket = "EU", SortOrder = 4 },
                new ProductGroup { Code = "LOIN_DOMESTIC", Name = "Tuna loin Co nội địa", DefaultMarket = "OTHER", SortOrder = 5 },
                new ProductGroup { Code = "LOIN_NOCO", Name = "Tuna loin NOCO", DefaultMarket = null, SortOrder = 6 },
                new ProductGroup { Code = "TAIL_NOCO", Name = "Tuna tail NOCO", DefaultMarket = null, SortOrder = 7 },
                new ProductGroup { Code = "REO", Name = "Tuna rẻo", DefaultMarket = "OTHER", SortOrder = 8 },
            };
            _db.ProductGroups.AddRange(groups);
            await _db.SaveChangesAsync();

            var byCode = groups.ToDictionary(g => g.Code);
            void Sku(string group, string name, string? market, decimal price = 0, bool byproduct = false)
            {
                var g = byCode[group];
                _db.ProductSkus.Add(new ProductSku
                {
                    GroupId = g.Id,
                    Code = name.Trim().ToUpperInvariant().Replace(' ', '_'),
                    Name = name.Trim(),
                    ExportMarket = market ?? g.DefaultMarket,
                    DefaultUnitPriceUsd = price,
                    IsByproduct = byproduct
                });
            }

            foreach (var n in new[] { "2-4 AAA USA SAKU", "1-2 AAA USA SAKU", "LOIN AAA CUT SAKU", "2-4 AA USA", "2-4 AA USA (2)", "4UP AA USA", "4UP AA USA (2)", "4UP AA USA CUT STEAK", "2-4 AA USA CUT STEAK", "5UPA SAKU (2)", "LOIN AA CUT CUBE" })
                Sku("LOIN_CO_USA", n, "USA");
            foreach (var n in new[] { "TAIL AA CUT CUBE", "TAIL AA CUT STEAK", "TAIL AA XAY GROUND MEAT" })
                Sku("TAIL_CO", n, "USA", byproduct: n.Contains("XAY"));
            foreach (var n in new[] { "5UPA", "3-5A", "2-3A" })
                Sku("LOIN_CO_A", n, "USA");
            foreach (var n in new[] { "5up", "5-8kg", "2-5kg", "1-2KG", "cut saku", "cut steak", "Tuna tail vitamin cut steak" })
                Sku("LOIN_VITAMIN", n, "EU");
            foreach (var n in new[] { "5UPA NĐ", "3-5A NĐ", "2-3A NĐ", "1-2A NĐ", "LOIN CUT STEAK NĐ" })
                Sku("LOIN_DOMESTIC", n, "OTHER");
            foreach (var n in new[] { "TUNA LOIN CUT CUBE", "TUNA LOIN 2-5KG", "TUNA LOIN 5-9LBS", "TUNA LOIN 3-5LBS TAIL WITHKIN" })
                Sku("LOIN_NOCO", n, null);
            foreach (var n in new[] { "TUNA TAIL CUT CUBE", "TUNA TAIL CUT VỤN 4G" })
                Sku("TAIL_NOCO", n, null, byproduct: n.Contains("VỤN"));
            foreach (var n in new[] { "TUNA RẺO NOCO LOCK 5KG", "TUNA RẺO CO LOCK 5KG", "TUNA RẺO VITAMIN LOCK 5KG" })
                Sku("REO", n, n.Contains("VITAMIN") ? "EU" : "OTHER");

            _db.PaymentTerms.AddRange(
                new PaymentTerm { Code = "TT_20_80", Name = "TT (20+80)", RatiosJson = "[0.2,0.8]", IsLc = false },
                new PaymentTerm { Code = "TT_30_70", Name = "TT (30+70)", RatiosJson = "[0.3,0.7]", IsLc = false },
                new PaymentTerm { Code = "TT_20_60_20", Name = "TT (20+60+20)", RatiosJson = "[0.2,0.6,0.2]", IsLc = false },
                new PaymentTerm { Code = "LC_45", Name = "LC 45 DAYS", RatiosJson = "[1]", DueDays = 45, IsLc = true },
                new PaymentTerm { Code = "LC_45_FDA", Name = "LC 45 DAYS PASS FDA", RatiosJson = "[1]", DueDays = 45, IsLc = true },
                new PaymentTerm { Code = "TT", Name = "TT", RatiosJson = "[1]", IsLc = false },
                new PaymentTerm { Code = "LC", Name = "LC", RatiosJson = "[1]", IsLc = true }
            );

            foreach (var (code, name) in new[] { ("E12", "Khách 12"), ("E45", "Khách 45"), ("E92", "Khách 92"), ("E101", "Khách 101"), ("E110", "Khách 110"), ("E118", "Khách 118"), ("E119", "Khách 119"), ("E137", "Khách 137"), ("E155", "Khách 155"), ("E162", "Khách 162"), ("E171", "Khách 171") })
            {
                _db.Customers.Add(new Customer { Code = code, Name = name, Kind = CustomerKind.Both, IsActive = true });
            }

            await _db.SaveChangesAsync();
        }
    }
}
