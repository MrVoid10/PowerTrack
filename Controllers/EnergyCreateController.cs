//using Microsoft.AspNetCore.Mvc;
//using PowerTrack.Data;
//using PowerTrack.Models;

//namespace PowerTrack.Controllers
//{
//  public class EnergyCreateController : Controller
//  {
//    private readonly ApplicationDbContext _context;

//    public EnergyCreateController(ApplicationDbContext context)
//    {
//      _context = context;
//    }

//    // GET: /EnergyCreate/Create
//    [HttpGet]
//    public IActionResult Create()
//    {
//      return View(); // shows the form
//    }

//    // POST: /EnergyCreate/Create
//    [HttpPost]
//    public IActionResult Create(EnergyConsumption model)
//    {
//      var userId = HttpContext.Session.GetInt32("UserId");
//      if (userId == null)
//        return RedirectToAction("Login", "Account");

//      model.UserId = userId.Value; // assign BEFORE validation

//      if (!ModelState.IsValid)
//      {
//        // Temporary: print all validation errors
//        Console.WriteLine("ModelState is invalid. Fields with errors:");
//        foreach (var entry in ModelState)
//        {
//          if (entry.Value.Errors.Count > 0)
//          {
//            Console.WriteLine($"- {entry.Key}:");
//            foreach (var error in entry.Value.Errors)
//            {
//              Console.WriteLine($"    {error.ErrorMessage}");
//            }
//          }
//        }

//        // Pass errors to the view
//        ViewBag.Error = "Please fill in all required fields.";
//        ViewBag.ValidationErrors = ModelState
//            .Where(ms => ms.Value.Errors.Count > 0)
//            .ToDictionary(ms => ms.Key, ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToArray());

//        return View(model);
//      }

//      try
//      {
//        _context.EnergyConsumptions.Add(model);
//        _context.SaveChanges();

//        TempData["Success"] = "Energy record saved successfully!";
//        return RedirectToAction("Create");
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine($"Error saving energy record: {ex.Message}");
//        ViewBag.Error = "An error occurred while saving the record.";
//        return View(model);
//      }
//    }

//  }
//}

using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using PowerTrack.Services;
using Microsoft.EntityFrameworkCore;

namespace PowerTrack.Controllers
{
  public class EnergyCreateController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public EnergyCreateController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    // GET: /EnergyCreate/Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /EnergyCreate/Create
    [HttpPost]
    public IActionResult Create(EnergyConsumption model)
    {
      var userId = HttpContext.Session.GetInt32("UserId");

      if (userId == null)
      {
        _audit.Log(
            "Energy Create Access",
            "FAILED",
            "WARNING",
            "Unauthorized access attempt to EnergyCreate"
        );

        return RedirectToAction("Login", "Account");
      }

      model.UserId = userId.Value;

      if (!ModelState.IsValid)
      {
        _audit.Log(
            "Energy Create Validation",
            "FAILED",
            "WARNING",
            $"Validation failed for user {userId}"
        );

        Console.WriteLine("ModelState is invalid:");
        foreach (var entry in ModelState)
        {
          if (entry.Value.Errors.Count > 0)
          {
            Console.WriteLine($"- {entry.Key}");
            foreach (var error in entry.Value.Errors)
              Console.WriteLine($"  -> {error.ErrorMessage}");
          }
        }

        ViewBag.Error = "Please fill all required fields.";
        return View(model);
      }

      try
      {
        _context.EnergyConsumptions.Add(model);
        _context.SaveChanges();

        _audit.Log(
            "Energy Consumption Create",
            "SUCCESS",
            "INFO",
            $"Energy record created for user {userId}"
        );

        TempData["Success"] = "Energy record saved successfully!";
        return RedirectToAction("Create");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);

        _audit.Log(
            "Energy Consumption Create",
            "FAILED",
            "ERROR",
            ex.Message
        );

        ViewBag.Error = "Error saving energy record";
        return View(model);
      }
    }
  }
}