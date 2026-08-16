using Microsoft.AspNetCore.Mvc;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Web.Controllers;

public class ListSourcesController(IHttpClientFactory httpClientFactory) : Controller
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

    public async Task<IActionResult> Index()
    {
        var response = await _client.GetAsync("api/Sources");
        if (response.IsSuccessStatusCode)
        {
            var sources = await response.Content.ReadFromJsonAsync<List<ListSource>>();
            return View(sources);
        }
        return View(new List<ListSource>());
    }

    public IActionResult Create()
    {
        return View(new ListSource());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ListSource source)
    {
        var response = await _client.PostAsJsonAsync("api/Sources", source);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }
        
        ModelState.AddModelError("", "Kaynak eklenirken bir hata oluştu.");
        return View(source);
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _client.DeleteAsync($"api/Sources/{id}");
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> TriggerHarvest(int id)
    {
        var response = await _client.PostAsync($"api/Harvest/trigger/{id}", null);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Tetikleme başarısız veya aktif bir Scraper implementasyonu yok.";
        }
        else
        {
            TempData["Success"] = "Kaynak manuel olarak tetiklendi!";
        }
        return RedirectToAction(nameof(Index));
    }
}
