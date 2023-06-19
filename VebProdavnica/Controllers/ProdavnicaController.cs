using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VebProdavnica.Models;

namespace VebProdavnica.Controllers
{
    public class ProdavnicaController : Controller
    {
        // GET: Prodavnica
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Registracija()
        {
            return View();
        }
    }
}