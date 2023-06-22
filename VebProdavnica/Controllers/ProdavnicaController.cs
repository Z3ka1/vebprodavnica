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
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;

            return View();
        }

        public ActionResult Registracija()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegistrujSe(string ime, string prezime, string polStr, string email, DateTime datumRodjenja,
            string ulogaStr, string korisnickoIme, string lozinka)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            foreach (string user in korisnici.Keys)
            {
                if (korisnickoIme == user)
                {
                    ViewBag.Message = $"Korisnicko ime '{user}' je vec u upotrebi!";
                    return View("Registracija");
                }
            }

            Pol pol = Pol.M;
            if (polStr == "Zensko")
                pol = Pol.Z;

            Uloga uloga = Uloga.Kupac;
            if (ulogaStr == "Prodavac")
                uloga = Uloga.Prodavac;

            Korisnik novi = new Korisnik(ime, prezime, pol, email, datumRodjenja, uloga, korisnickoIme, lozinka);
            korisnici.Add(novi.korisnickoIme, novi);
            Data.UpdateKorisnikXml(novi);
            Session["korisnik"] = novi;

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult UlogujSe(string username, string password)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            if (korisnici.ContainsKey(username))
            {
                Korisnik korisnik = korisnici[username];
                if (korisnik == null || korisnici[username].lozinka != password)
                {
                    ViewBag.Greska = "Pogresno korisnicko ime ili lozinka!";
                    Dictionary<int, Proizvod> proizvodii = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
                    ViewData["Proizvodi"] = proizvodii;
                    return View("Index");
                }
                Session["korisnik"] = korisnik;
            }
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            return View("Index");
        }

        [HttpPost]
        public ActionResult DetaljiProizvoda(int id)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            return View(proizvodi[id]);
        }

        [HttpPost]
        public ActionResult DodajUOmiljene(int id)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            if (!((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Contains(proizvodi[id]))
            {
                ((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Add(proizvodi[id]);
                Data.UpdateKorisnikXml((Korisnik)Session["korisnik"]);
                ViewBag.Message = "Proizvod dodat u omiljene!";
            }
            else
            {
                ViewBag.Message = "Proizvod je vec medju omiljenim proizvodima.";
            }
            return View("DetaljiProizvoda", proizvodi[id]);
        }
    }
}