using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TryTwo.Controllers
{
    public class BattleshipsController : Controller
    {        
        public ActionResult Index()
        {
            return View();
        }

        // GET: BattleshipsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BattleshipsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BattleshipsController/Create
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BattleshipsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BattleshipsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
