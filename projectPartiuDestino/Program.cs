var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Sessão
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".Biblioteca.Session";
    o.IdleTimeout = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // The default HSTS value is 30 days.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// A SESSÃO DEVE VIR ANTES DO MVC
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",

    // alterado de Home para Login, começar na tela de login
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();