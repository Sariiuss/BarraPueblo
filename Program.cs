using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var accessCode = Environment.GetEnvironmentVariable("BAR_ACCESS_CODE");

app.Use(async (context, next) => {
    if (string.IsNullOrEmpty(accessCode) || context.Request.Path == "/healthz") {
        await next();
        return;
    }

    var authorization = context.Request.Headers.Authorization.ToString();
    var valid = false;
    if (authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) {
        try {
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authorization[6..]));
            var separator = credentials.IndexOf(':');
            if (separator >= 0 && credentials[..separator] == "barra") {
                var supplied = Encoding.UTF8.GetBytes(credentials[(separator + 1)..]);
                var expected = Encoding.UTF8.GetBytes(accessCode);
                valid = supplied.Length == expected.Length && CryptographicOperations.FixedTimeEquals(supplied, expected);
            }
        } catch (FormatException) { }
    }

    if (!valid) {
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Barra de Valdorros\", charset=\"UTF-8\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Acceso restringido.");
        return;
    }

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
var products = new Product[] {
    new("agua", "Agua", 100), new("refresco", "Refresco", 200),
    new("cachi", "Cachi Kalimotxo / Cerveza", 500), new("cachi-cubata", "Cachi Cubata", 1100),
    new("pinta", "Pinta cerveza / Kali", 300), new("cubata", "Cubata", 600),
    new("cana", "Caña / botellín", 150), new("radler", "Radler / 0,0", 150),
    new("chupito", "Chupito", 150), new("jager", "Jäger", 200),
    new("vino", "Vino", 150), new("vermut", "Vermut", 200),
    new("mosto", "Mosto", 150), new("zumo", "Zumo / batido", 200)
};
var gate = new object();
var dataDirectory = Environment.GetEnvironmentVariable("BAR_DATA_DIR") ?? Path.Combine(app.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDirectory);
var file = Path.Combine(dataDirectory, "sales.json");
var sales = File.Exists(file) ? JsonSerializer.Deserialize<List<Sale>>(File.ReadAllText(file)) ?? [] : [];
Quote Calculate(Order order) {
    if (order.Items is null || order.Items.Length == 0 || order.Items.Length > products.Length || order.Items.Any(i => i is null))
        throw new ArgumentException("Añade al menos una bebida.");
    if (order.Items.Select(i => i.ProductId).Distinct().Count() != order.Items.Length)
        throw new ArgumentException("Productos duplicados.");
    var lines = order.Items.Select(i => {
        var product = products.FirstOrDefault(p => p.Id == i.ProductId) ?? throw new ArgumentException("Producto desconocido.");
        if (i.Quantity is < 1 or > 100) throw new ArgumentException("Cantidad no válida (1–100).");
        return new Line(product.Id, product.Name, i.Quantity, product.PriceCents, product.PriceCents * i.Quantity);
    }).ToArray();
    var total = lines.Sum(l => l.TotalCents);
    if (order.ReceivedCents is < 0 or > 1000000) throw new ArgumentException("Importe recibido no válido.");
    return new Quote(lines, total, order.ReceivedCents, order.ReceivedCents - total);
}
app.MapGet("/api/products", () => products);
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapPost("/api/quote", (Order order) => {
    try { return Results.Ok(Calculate(order)); }
    catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
});
app.MapPost("/api/sales", (SaleRequest request) => {
    lock (gate) {
        var existing = sales.FirstOrDefault(s => s.Id == request.Id);
        if (existing != null) return Results.Ok(existing);
        if (request.Id == Guid.Empty) return Results.BadRequest(new { error = "Identificador no válido." });
        try {
            var quote = Calculate(new Order(request.Items, request.ReceivedCents));
            if (quote.ChangeCents < 0) return Results.BadRequest(new { error = "El dinero recibido no alcanza el total." });
            var sale = new Sale(request.Id, DateTimeOffset.UtcNow, quote);
            var next = sales.Append(sale).ToList();
            var temporary = file + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(next));
            File.Move(temporary, file, true);
            sales = next;
            return Results.Ok(sale);
        } catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    }
});
app.MapGet("/api/summary", () => {
    lock (gate) return Results.Ok(new {
        count = sales.Count, totalCents = sales.Sum(s => (long)s.Quote.TotalCents),
        products = products.Select(p => new { p.Name, quantity = sales.SelectMany(s => s.Quote.Lines).Where(l => l.ProductId == p.Id).Sum(l => (long)l.Quantity) })
    });
});
app.Run();
record Product(string Id, string Name, int PriceCents);
record Item(string ProductId, int Quantity);
record Order(Item[] Items, int ReceivedCents);
record SaleRequest(Guid Id, Item[] Items, int ReceivedCents);
record Line(string ProductId, string Name, int Quantity, int PriceCents, int TotalCents);
record Quote(Line[] Lines, int TotalCents, int ReceivedCents, int ChangeCents);
record Sale(Guid Id, DateTimeOffset CreatedAt, Quote Quote);
