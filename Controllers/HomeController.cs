using System.Diagnostics;
using HomeCare.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeCare.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Can either show landing page with "Go to Dashboard" button or directly redirect
                ViewBag.IsUserLoggedIn = true;
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ActionName("StatusCode")]
        public IActionResult ErrorStatusCode(int code)
        {
            ViewBag.StatusCode = code;
            switch (code)
            {
                case 404:
                    ViewBag.Message = "Sorry, the page or resource you requested could not be found.";
                    ViewBag.Title = "Page Not Found";
                    break;
                case 403:
                    ViewBag.Message = "Access Denied: You do not have permission to view or modify this resource.";
                    ViewBag.Title = "Access Forbidden";
                    break;
                case 500:
                    ViewBag.Message = "An internal server error occurred while processing your request. Please try again.";
                    ViewBag.Title = "Server Error";
                    break;
                default:
                    ViewBag.Message = $"An unexpected HTTP {code} error occurred.";
                    ViewBag.Title = $"Error {code}";
                    break;
            }
            return View("ErrorStatus");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
