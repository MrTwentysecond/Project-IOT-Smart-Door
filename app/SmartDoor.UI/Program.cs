/*
    Program.cs
    ─────────────────────────────────────────────────────────────────
    Titik masuk utama (entry point) aplikasi ASP.NET Core Blazor.
    File ini mengatur:
      1. Service (autentikasi Google, Razor Components, dll.)
      2. Middleware pipeline (urutan pemrosesan request HTTP)
      3. Endpoint routing (URL apa → handler apa)
    ─────────────────────────────────────────────────────────────────
*/

using SmartDoorAccess.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────
// 1. KONFIGURASI AUTENTIKASI GOOGLE
// ─────────────────────────────────────────────────────────────────
// Aplikasi menggunakan dua skema:
//   - Cookie     → menyimpan sesi login pengguna di browser
//   - Google     → menangani proses login via Google OAuth 2.0
builder.Services.AddAuthentication(options =>
{
    // Skema default untuk membaca sesi login yang sudah ada
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // Skema default untuk memulai proses login baru
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()  // Daftarkan handler Cookie
.AddGoogle(options =>
{
    // ⚠️  GANTI DENGAN CLIENT ID & SECRET DARI GOOGLE CLOUD CONSOLE
    //     Cara mendapatkannya:
    //     1. Buka https://console.cloud.google.com/
    //     2. Buat project → APIs & Services → Credentials
    //     3. Create OAuth 2.0 Client ID (Web Application)
    //     4. Tambahkan redirect URI: https://localhost:PORT/signin-google/callback
    options.ClientId     = builder.Configuration["Authentication:Google:ClientId"]
                           ?? "YOUR_GOOGLE_CLIENT_ID";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                           ?? "YOUR_GOOGLE_CLIENT_SECRET";
    
    // Callback path → harus cocok dengan yang didaftarkan di Google Console
    // Default Blazor: /signin-google/callback (bukan /signin-google)
    options.CallbackPath = "/signin-google/callback";
});

// ─────────────────────────────────────────────────────────────────
// 2. DAFTARKAN RAZOR COMPONENTS (BLAZOR SERVER)
// ─────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Aktifkan mode interaktif Blazor Server

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────
// 3. MIDDLEWARE PIPELINE
// ─────────────────────────────────────────────────────────────────
// ⚠️  URUTAN MIDDLEWARE SANGAT PENTING — jangan diubah sembarangan!

// Error handling (hanya aktif di Production, bukan Development)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts(); // Paksa HTTPS di production
}

app.UseHttpsRedirection(); // Redirect HTTP → HTTPS
app.UseStaticFiles();      // Sajikan file statis (wwwroot/)

// Autentikasi HARUS sebelum Otorisasi
app.UseAuthentication();   // Baca & validasi cookie/token login
app.UseAuthorization();    // Cek apakah user punya akses ke resource
app.UseAntiforgery();      // Proteksi CSRF untuk form POST

// ─────────────────────────────────────────────────────────────────
// 4. ROUTING BLAZOR
// ─────────────────────────────────────────────────────────────────
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // Komponen dengan @rendermode InteractiveServer

// ─────────────────────────────────────────────────────────────────
// 5. ENDPOINT KUSTOM: /signin-google
// ─────────────────────────────────────────────────────────────────
// Saat tombol "Login dengan Google" diklik di Home.razor,
// form POST ke /signin-google → endpoint ini yang menanganinya.
// ChallengeAsync → mengarahkan browser ke halaman login Google.
// Setelah login berhasil → Google callback ke /signin-google/callback
// → lalu diarahkan ke /dashboard.
app.MapGet("/signin-google", async context =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/dashboard" // URL tujuan setelah login berhasil
    });
});

// ─────────────────────────────────────────────────────────────────
// 6. JALANKAN APLIKASI
// ─────────────────────────────────────────────────────────────────
app.Run();