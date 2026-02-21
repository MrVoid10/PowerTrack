using Microsoft.AspNetCore.Mvc;

namespace PowerTrack.Controllers
{
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      if (!HttpContext.Session.Keys.Contains("UserId"))
        return RedirectToAction("Login", "Account");

      return View();
    }
  }

}
