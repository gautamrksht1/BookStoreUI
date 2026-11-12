using System.Net.Http.Json;
using BookStoreUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreUI.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _client;

    private static string GetServiceUrl(string environmentVariableName, string fallback)
    {
        var configured = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(configured) ? fallback : configured.TrimEnd('/');
    }

    public HomeController(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        var catalogUrl = GetServiceUrl("CATALOG_SERVICE_URL", "http://localhost:8001");

        try
        {
            var books = await _client.GetFromJsonAsync<List<Book>>($"{catalogUrl}/books");
            return View(books ?? new List<Book>());
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = $"Unable to reach the catalog service at {catalogUrl}. Make sure the Catalog API is running.";
            return View(new List<Book>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Buy(int bookId, int quantity)
    {
        var orderUrl = GetServiceUrl("ORDER_SERVICE_URL", "http://localhost:8002");

        try
        {
            await _client.PostAsJsonAsync(
                $"{orderUrl}/orders",
                new
                {
                    book_id = bookId,
                    quantity = quantity
                });
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = $"Unable to place the order because the order service at {orderUrl} is unavailable.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }
}
