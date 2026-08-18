using Application.Api.Features.Auth;
using Application.Api.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Common.Data;

/// <summary>
/// Runs once at startup (after migrations are applied) to seed the Admin
/// user. Not done via EF's HasData because PasswordHasher.Hash uses a
/// random salt per call, so there's no static hash value HasData could use.
/// Matches the ShopEase prototype's demo admin account so both stay usable
/// together: admin@shopease.com / Admin@123.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAdminUserAsync(AppDbContext db)
    {
        var adminExists = await db.Users.AnyAsync(u => u.Email == "admin@shopease.com");
        if (adminExists) return;

        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null) return; // roles migration hasn't run yet - nothing to attach to

        var admin = new User
        {
            Email = "admin@shopease.com",
            FullName = "ShopEase Admin",
            PasswordHash = PasswordHasher.Hash("Admin@123"),
        };
        admin.UserRoles.Add(new UserRole { Role = adminRole });

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static readonly (string Name, int CategoryId, decimal Price, int Stock, string Description)[] CatalogSeed =
    [
        // Electronics (CategoryId 1)
        ("Wireless Mouse", 1, 599m, 25, "A reliable 2.4GHz wireless mouse with a 12-month battery life."),
        ("Mechanical Keyboard", 1, 3499m, 18, "Hot-swappable mechanical keyboard with tactile brown switches."),
        ("4K Monitor", 1, 21999m, 6, "27-inch 4K IPS monitor with USB-C and 95% DCI-P3 coverage."),
        ("USB-C Hub", 1, 1299m, 40, "7-in-1 USB-C hub with HDMI, SD card reader, and 100W power delivery."),
        ("HD Webcam", 1, 2199m, 22, "1080p webcam with autofocus and a built-in noise-cancelling mic."),
        ("Bluetooth Speaker", 1, 2999m, 15, "Portable speaker with 12-hour battery life and IPX7 water resistance."),
        ("Noise Cancelling Headphones", 1, 7999m, 10, "Over-ear ANC headphones with 30-hour battery life."),
        ("20000mAh Power Bank", 1, 1899m, 30, "Fast-charging power bank with dual USB-C PD ports."),
        ("Smart Watch", 1, 5499m, 14, "Fitness tracking smart watch with heart-rate and SpO2 monitoring."),
        ("Wireless Charger", 1, 999m, 35, "15W Qi wireless charging pad, compatible with most phone cases."),
        ("External SSD 1TB", 1, 6499m, 12, "Portable NVMe SSD with read speeds up to 1050MB/s."),
        ("Laptop Stand", 1, 1499m, 20, "Adjustable aluminium laptop stand for better posture and cooling."),

        // Furniture (CategoryId 2)
        ("Office Chair", 2, 8999m, 8, "Ergonomic mesh-back office chair with adjustable lumbar support."),
        ("Standing Desk", 2, 15999m, 5, "Electric height-adjustable standing desk, 120x60cm top."),
        ("Desk Lamp", 2, 799m, 45, "Dimmable LED desk lamp with USB charging port."),
        ("Bookshelf", 2, 4999m, 9, "5-tier wooden bookshelf, freestanding, easy assembly."),
        ("Filing Cabinet", 2, 6999m, 7, "3-drawer lockable steel filing cabinet for home offices."),
        ("Monitor Arm", 2, 2499m, 16, "Single-arm monitor mount with full tilt, swivel, and height adjustment."),
        ("Ergonomic Footrest", 2, 1299m, 24, "Adjustable-angle footrest with a massaging textured surface."),

        // Stationery (CategoryId 3)
        ("Notebook Set", 3, 249m, 100, "Pack of 3 dotted-grid notebooks, 160 pages each."),
        ("Fountain Pen", 3, 1299m, 20, "Stainless steel nib fountain pen with a converter included."),
        ("Sticky Notes Pack", 3, 149m, 120, "12-pad set of sticky notes in assorted colors and sizes."),
        ("Desk Organizer", 3, 599m, 30, "Multi-compartment mesh desk organizer for pens, cards, and supplies."),
        ("Whiteboard Markers", 3, 299m, 60, "Pack of 8 low-odor whiteboard markers with an eraser."),
        ("Highlighter Set", 3, 199m, 80, "6-color highlighter set with chisel tips."),
    ];

    /// <summary>
    /// Idempotent per-item (checks by name, not a row count) so it never
    /// duplicates a product that already exists - e.g. "Wireless Mouse",
    /// created manually while verifying Phase 1 - and it's safe to extend
    /// this list later without re-seeding everything.
    /// </summary>
    public static async Task SeedProductsAsync(AppDbContext db)
    {
        var existingNames = await db.Products.Select(p => p.Name).ToListAsync();
        var toAdd = CatalogSeed
            .Where(p => !existingNames.Contains(p.Name))
            .Select(p => new Product
            {
                Name = p.Name,
                CategoryId = p.CategoryId,
                Price = p.Price,
                Stock = p.Stock,
                Description = p.Description,
            });

        db.Products.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}
