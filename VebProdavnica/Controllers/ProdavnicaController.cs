using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (!((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Any(p => p.id == id))
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

        [HttpPost]
        public ActionResult PoruciProizvod(int id, int kolicina)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];

            if (proizvodi[id].kolicina >= kolicina)
            {
                Porudzbina nova = new Porudzbina(Data.GenerateID(), id, kolicina,
                    ((Korisnik)Session["korisnik"]).korisnickoIme, DateTime.Now, Status.AKTIVNA);
                ((Korisnik)Session["korisnik"]).listaPorudzbina.Add(nova);
                Data.UpdatePorudzbinaXml(nova);
                proizvodi[id].kolicina -= kolicina;
                Data.UpdateProizvodXml(proizvodi[id]);

                ViewBag.Message = $"Porudzbina {nova.id} uspesno kreirana, detalje o porudzbini mozete videti na svom profilu.";
            }
            else
            {
                ViewBag.Greska = $"Nema dovoljno proizvoda na stanju! Preostala kolicina je {proizvodi[id].kolicina}.";
            }

            return View("DetaljiProizvoda", proizvodi[id]);
        }

        public ActionResult Sort(string kriterijum)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            var listaProizvoda = proizvodi.ToList();

            if (kriterijum == "Naziv(rastuce)")
                listaProizvoda.Sort((x, y) => x.Value.naziv.CompareTo(y.Value.naziv));
            else if (kriterijum == "Naziv(opadajuce)")
                listaProizvoda.Sort((x, y) => y.Value.naziv.CompareTo(x.Value.naziv));
            else if(kriterijum == "Cena(rastuce)")
                listaProizvoda.Sort((x, y) => x.Value.cena.CompareTo(y.Value.cena));
            else if (kriterijum == "Cena(opadajuce)")
                listaProizvoda.Sort((x, y) => y.Value.cena.CompareTo(x.Value.cena));
            else if (kriterijum == "Datum oglasavanja(rastuce)")
                listaProizvoda.Sort((x, y) => x.Value.datumPostavljanja.CompareTo(y.Value.datumPostavljanja));
            else if (kriterijum == "Datum oglasavanja(opadajuce)")
                listaProizvoda.Sort((x, y) => y.Value.datumPostavljanja.CompareTo(x.Value.datumPostavljanja));

            proizvodi = listaProizvoda.ToDictionary(kvp => kvp.Key, kvp=> kvp.Value);
            ViewData["Proizvodi"] = proizvodi;
            return View("Index");
        }

        public ActionResult Pretraga(string naziv, string cenaOd, string cenaDo, string grad)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Proizvod> pretraga = new Dictionary<int, Proizvod>();

            bool uspesno = false;
            double cenaOddouble;
            double cenaDodouble;
            uspesno = double.TryParse(cenaOd, out cenaOddouble);
            uspesno = double.TryParse(cenaDo, out cenaDodouble);

            foreach(Proizvod p in proizvodi.Values)
            {
                if (naziv != "" && (cenaOd != "" || cenaDo != "") && grad != "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.naziv.ToLower().Contains(naziv.ToLower()) &&
                        p.cena <= cenaDodouble && p.cena >= cenaOddouble &&
                        p.grad.ToLower().Equals(grad.ToLower()))
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }

                if (naziv != "" && (cenaOd == "" && cenaDo == "") && grad == "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.naziv.ToLower().Contains(naziv.ToLower()))
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }

                if (naziv == "" && (cenaOd != "" || cenaDo != "") && grad == "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.cena <= cenaDodouble && p.cena >= cenaOddouble)
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }

                if (naziv == "" && (cenaOd == "" && cenaDo == "") && grad != "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.grad.ToLower().Equals(grad.ToLower()))
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }

                if (naziv != "" && (cenaOd != "" || cenaDo != "") && grad == "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.naziv.ToLower().Contains(naziv.ToLower()) &&
                        p.cena <= cenaDodouble && p.cena >= cenaOddouble)
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }

                if (naziv != "" && (cenaOd == "" && cenaDo == "") && grad != "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.naziv.ToLower().Contains(naziv.ToLower()) && 
                        p.grad.ToLower().Equals(grad.ToLower()))
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }

                if (naziv == "" && (cenaOd != "" || cenaDo != "") && grad != "")
                {
                    if (cenaOd == "")
                        cenaOddouble = 0;
                    if (cenaDo == "")
                        cenaDodouble = double.MaxValue;

                    if (p.cena <= cenaDodouble && p.cena >= cenaOddouble &&
                        p.grad.ToLower().Equals(grad.ToLower()))
                        if (!pretraga.ContainsKey(p.id))
                            pretraga.Add(p.id, p);
                }
            }

            if(pretraga.Count == 0)
            {
                ViewBag.Pretraga = "Nije pronadjen ni jedan proizvod za datu pretragu. Prikazani su svi proizvodi.";
                ViewData["Proizvodi"] = proizvodi;
                return View("Index");
            }

            ViewBag.Pretraga = "Rezultati pretrage";
            ViewData["Proizvodi"] = pretraga;
            return View("Index");
        }
    }
}