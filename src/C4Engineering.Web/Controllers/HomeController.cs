using Microsoft.AspNetCore.Mvc;

namespace C4Engineering.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Services()
    {
        return RedirectToAction("Index", "ServiceCatalog");
    }
    
    public IActionResult Architecture()
    {
        return RedirectToAction("Index", "Architecture");
    }
    
    public IActionResult Pipelines()
    {
        return RedirectToAction("Index", "Pipeline");
    }
    
    public IActionResult Deployments()
    {
        return RedirectToAction("Index", "Deployment");
    }
}