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

        [HttpPost]
        public ActionResult RegistrujSe(string ime, string prezime, string polStr, string email, DateTime datumRodjenja,
            string ulogaStr, string korisnickoIme, string lozinka)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            
            foreach(string user in korisnici.Keys)
            {
                if(korisnickoIme == user)
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

            return View("Index");
        }
    }
}