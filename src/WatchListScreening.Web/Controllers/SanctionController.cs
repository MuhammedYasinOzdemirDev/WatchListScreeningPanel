using Microsoft.AspNetCore.Mvc;

namespace WatchListScreening.Web.Controllers;

public class SanctionController : Controller
{
    // Yaptırım Listesi Yönetimi - DataTables ve Modal CRUD operasyonlarını barındıran View
    public IActionResult Index()
    {
        return View();
    }

    // Yeni Yaptırım Kaydı Oluşturma (Tam Sayfa Enterprise Form)
    public IActionResult Create()
    {
        return View();
    }
}
