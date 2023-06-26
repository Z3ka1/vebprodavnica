using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VebProdavnica.Models;

namespace VebProdavnica.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdminPanel()
        {
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];
            return View(admin);
        }

        public ActionResult AdminPanelKorisnici()
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            ViewData["Korisnici"] = korisnici;
            return View(admin);
        }

        public ActionResult AdminPanelProizvodi()
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            ViewData["Proizvodi"] = proizvodi;
            return View(admin);
        }

        public ActionResult AdminPanelPorudzbine()
        {
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            ViewData["Porudzbine"] = porudzbine;
            ViewData["Proizvodi"] = proizvodi;
            ViewData["Korisnici"] = korisnici;
            return View(admin);
        }

        public ActionResult AdminPanelRecenzije()
        {
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            ViewData["Recenzije"] = recenzije;
            return View(admin);
        }

        public ActionResult IzmeniPodatkeAdmina(string ime, string prezime, string email, DateTime datumRodjenja,
            string staraLozinka, string novaLozinka)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            if (staraLozinka == admin.lozinka)
            {
                korisnici[admin.korisnickoIme].ime = ime;
                korisnici[admin.korisnickoIme].prezime = prezime;
                korisnici[admin.korisnickoIme].email = email;
                korisnici[admin.korisnickoIme].datumRodjenja = datumRodjenja;
                if (novaLozinka != "")
                {
                    korisnici[admin.korisnickoIme].lozinka = novaLozinka;
                }
                Data.UpdateKorisnikXml(admin);
            }
            else
            {
                ViewBag.Greska = "Stara lozinka je ne ispravna";
            }

            return View("AdminPanel",admin);
        }

        public ActionResult PretragaKorisnika(string ime, string prezime, DateTime datumOd, DateTime datumDo,
            string uloga, string kriterijum)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            Dictionary<string, Korisnik> pretraga = new Dictionary<string, Korisnik>();

            foreach(Korisnik k in korisnici.Values)
            {
                if (uloga == "" || uloga == "SVI")
                {
                    if(ime != "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) &&
                            k.prezime.ToLower().Contains(prezime.ToLower()) &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.prezime.ToLower().Contains(prezime.ToLower()) &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime == "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime != "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) &&
                            k.prezime.ToLower().Contains(prezime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime == "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime != "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.prezime.ToLower().Contains(prezime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime == "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime == "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        pretraga.Add(k.korisnickoIme, k);
                    }

                }

                if(uloga == "KUPCI")
                {
                    if (ime != "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) && k.uloga == Uloga.Kupac &&
                            k.prezime.ToLower().Contains(prezime.ToLower()) &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.prezime.ToLower().Contains(prezime.ToLower()) && k.uloga == Uloga.Kupac &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime == "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) && k.uloga == Uloga.Kupac &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime != "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) && k.uloga == Uloga.Kupac &&
                            k.prezime.ToLower().Contains(prezime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime == "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo && k.uloga == Uloga.Kupac)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime != "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.uloga == Uloga.Kupac && k.prezime.ToLower().Contains(prezime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime == "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.uloga == Uloga.Kupac && k.ime.ToLower().Contains(ime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime == "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.uloga == Uloga.Kupac)
                            pretraga.Add(k.korisnickoIme, k);
                    }
                }

                if(uloga == "PRODAVCI")
                {
                    if (ime != "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) && k.uloga == Uloga.Prodavac &&
                            k.prezime.ToLower().Contains(prezime.ToLower()) &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.prezime.ToLower().Contains(prezime.ToLower()) && k.uloga == Uloga.Prodavac &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime == "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) && k.uloga == Uloga.Prodavac &&
                            k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime != "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.ime.ToLower().Contains(ime.ToLower()) && k.uloga == Uloga.Prodavac &&
                            k.prezime.ToLower().Contains(prezime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime == "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.datumRodjenja >= datumOd && k.datumRodjenja <= datumDo && k.uloga == Uloga.Prodavac)
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime != "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.uloga == Uloga.Prodavac && k.prezime.ToLower().Contains(prezime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime != "" && prezime == "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.uloga == Uloga.Prodavac && k.ime.ToLower().Contains(ime.ToLower()))
                            pretraga.Add(k.korisnickoIme, k);
                    }

                    if (ime == "" && prezime == "" && datumOd == DateTime.MinValue && datumDo == DateTime.MinValue)
                    {
                        if (datumDo == DateTime.MinValue)
                            datumDo = DateTime.MaxValue;

                        if (k.uloga == Uloga.Prodavac)
                            pretraga.Add(k.korisnickoIme, k);
                    }
                }


            }

            //pretraga je namestena sada sort
            var listaKorisnika = pretraga.ToList();

            if (kriterijum == "Ime(rastuce)")
                listaKorisnika.Sort((x, y) => x.Value.ime.CompareTo(y.Value.ime));
            else if (kriterijum == "Ime(opadajuce)")
                listaKorisnika.Sort((x, y) => y.Value.ime.CompareTo(x.Value.ime));
            else if (kriterijum == "Uloga(rastuce)")
                listaKorisnika.Sort((x, y) => x.Value.uloga.CompareTo(y.Value.uloga));
            else if (kriterijum == "Uloga(opadajuce)")
                listaKorisnika.Sort((x, y) => y.Value.uloga.CompareTo(x.Value.uloga));
            else if (kriterijum == "Datum rodjenja(rastuce)")
                listaKorisnika.Sort((x, y) => x.Value.datumRodjenja.CompareTo(y.Value.datumRodjenja));
            else if (kriterijum == "Datum rodjenja(opadajuce)")
                listaKorisnika.Sort((x, y) => y.Value.datumRodjenja.CompareTo(x.Value.datumRodjenja));

            pretraga = listaKorisnika.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            ViewData["Korisnici"] = pretraga;
            return View("AdminPanelKorisnici",admin);
        }

        public ActionResult PretragaProizvoda(string status, string kriterijum)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            Dictionary<int, Proizvod> pretraga = new Dictionary<int, Proizvod>();

            foreach(Proizvod p in proizvodi.Values)
            {
                if (status == "dostupan" && p.dostupan)
                    pretraga.Add(p.id, p);
                else if (status == "nedostupan" && !p.dostupan)
                    pretraga.Add(p.id, p);
                else if (status == "svi")
                    pretraga.Add(p.id, p);
            }

            var listaKorisnika = pretraga.ToList();

            if (kriterijum == "nazRastuce")
                listaKorisnika.Sort((x, y) => x.Value.naziv.CompareTo(y.Value.naziv));
            else if (kriterijum == "nazOpadajuce")
                listaKorisnika.Sort((x, y) => y.Value.naziv.CompareTo(x.Value.naziv));
            else if (kriterijum == "cenaRastuce")
                listaKorisnika.Sort((x, y) => x.Value.cena.CompareTo(y.Value.cena));
            else if (kriterijum == "cenaOpadajuce")
                listaKorisnika.Sort((x, y) => y.Value.cena.CompareTo(x.Value.cena));
            else if (kriterijum == "datumRastuce")
                listaKorisnika.Sort((x, y) => x.Value.datumPostavljanja.CompareTo(y.Value.datumPostavljanja));
            else if (kriterijum == "datumOpadajuce")
                listaKorisnika.Sort((x, y) => y.Value.datumPostavljanja.CompareTo(x.Value.datumPostavljanja));

            pretraga = listaKorisnika.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            ViewData["Proizvodi"] = pretraga;
            return View("AdminPanelProizvodi", admin);
        }
    }
}