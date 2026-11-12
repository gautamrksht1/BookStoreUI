using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookStoreUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreUI.Controllers;

[Route("Admin")]
public class AdminController : Controller
{
    private readonly HttpClient _client;

    public AdminController(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
    }

    [AllowAnonymous]
    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new AdminLoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
    {
        var configuredPassword = Environment.GetEnvironmentVariable("BOOKSTORE_ADMIN_PASSWORD");
        if (string.IsNullOrWhiteSpace(configuredPassword))
        {
            ModelState.AddModelError(string.Empty, "Admin access is not configured. Set BOOKSTORE_ADMIN_PASSWORD.");
        }
        else if (ModelState.IsValid && !PasswordsMatch(model.Password, configuredPassword))
        {
            ModelState.AddModelError(string.Empty, "The password is incorrect.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "BookStore Admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl!)
            : RedirectToAction(nameof(Books));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("Books")]
    public async Task<IActionResult> Books()
    {
        var model = new AdminBooksViewModel();
        await LoadBooks(model);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("Books")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Books(AdminBooksViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(
                    $"{GetCatalogUrl()}/books",
                    new
                    {
                        title = model.BookEntry.Title.Trim(),
                        author = model.BookEntry.Author.Trim(),
                        price = model.BookEntry.Price!.Value,
                        available_quantity = model.BookEntry.AvailableQuantity!.Value
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Book added to the catalog.";
                    return RedirectToAction(nameof(Books));
                }

                model.ErrorMessage = $"Catalog Service rejected the book (HTTP {(int)response.StatusCode}).";
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to reach the Catalog Service.";
            }
        }

        await LoadBooks(model);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task LoadBooks(AdminBooksViewModel model)
    {
        try
        {
            model.Books = await _client.GetFromJsonAsync<List<Book>>($"{GetCatalogUrl()}/books") ?? [];
        }
        catch (HttpRequestException)
        {
            model.ErrorMessage ??= "Unable to load books from the Catalog Service.";
        }
    }

    private static string GetCatalogUrl()
    {
        var configured = Environment.GetEnvironmentVariable("CATALOG_SERVICE_URL");
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("CATALOG_SERVICE_URL environment variable must be set");
        }

        return configured.TrimEnd('/');
    }

    private static bool PasswordsMatch(string submitted, string configured)
    {
        var submittedBytes = Encoding.UTF8.GetBytes(submitted);
        var configuredBytes = Encoding.UTF8.GetBytes(configured);
        return submittedBytes.Length == configuredBytes.Length
            && CryptographicOperations.FixedTimeEquals(submittedBytes, configuredBytes);
    }
}