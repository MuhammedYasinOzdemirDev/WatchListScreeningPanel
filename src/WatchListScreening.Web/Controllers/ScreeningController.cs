using Microsoft.AspNetCore.Mvc;

namespace WatchListScreening.Web.Controllers;

public class ScreeningController : Controller
{
    // Tekil tarama başlatılan ekran (Arama Motoru arayüzü)
    public IActionResult Index()
    {
        return View();
    }

    // Pending bekleyen sonuçların listelendiği, compliance onaylama ekranı
    public IActionResult Results()
    {
        return View();
    }

    // Detaylı İnceleme ve Kıyaslama (Review Dashboard) Ekranı
    public IActionResult Details(Guid id)
    {
        // Gerçek uygulamada API'den bu id'ye ait veriler getirilip View(model) olarak verilir.
        // Ancak biz şu an Frontend UI hazırladığımız için AJAX ile içeriden çekeceğiz (veya mocklayacağız).
        ViewBag.ReviewId = id;
        return View();
    }
}
