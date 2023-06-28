using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            ViewData["Recenzije"] = recenzije;
            ViewData["Proizvodi"] = proizvodi;
            ViewData["Korisnici"] = korisnici;
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

            return View("AdminPanel", admin);
        }

        public ActionResult PretragaKorisnika(string ime, string prezime, DateTime datumOd, DateTime datumDo,
            string uloga, string kriterijum)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            Dictionary<string, Korisnik> pretraga = new Dictionary<string, Korisnik>();

            foreach (Korisnik k in korisnici.Values)
            {
                if (uloga == "" || uloga == "SVI")
                {
                    if (ime != "" && prezime != "" && (datumOd != DateTime.MinValue || datumDo != DateTime.MinValue))
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

                if (uloga == "KUPCI")
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

                if (uloga == "PRODAVCI")
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
            return View("AdminPanelKorisnici", admin);
        }

        public ActionResult PretragaProizvoda(string status, string kriterijum)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            Dictionary<int, Proizvod> pretraga = new Dictionary<int, Proizvod>();

            foreach (Proizvod p in proizvodi.Values)
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

        public ActionResult DodajProdavca()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegistrujProdavca(string ime, string prezime, string pol, string email, DateTime datumRodjenja,
            string korisnickoIme, string lozinka)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            foreach (string user in korisnici.Keys)
            {
                if (korisnickoIme == user)
                {
                    ViewBag.Greska = $"Korisnicko ime '{user}' je vec u upotrebi!";
                    ViewData["Korisnici"] = korisnici;
                    return View("AdminPanelKorisnici", admin);
                }
            }

            Pol polp = Pol.M;
            if (pol == "Z")
                polp = Pol.Z;

            Korisnik novi = new Korisnik(ime, prezime, polp, email, datumRodjenja, Uloga.Prodavac, korisnickoIme, lozinka);
            korisnici.Add(novi.korisnickoIme, novi);
            Data.UpdateKorisnikXml(novi);

            ViewBag.Message = "Novi prodavac registrovan!";
            ViewData["Korisnici"] = korisnici;
            return View("AdminPanelKorisnici", admin);
        }

        [HttpPost]
        public ActionResult IzmeniKorisnika(string user)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            return View(korisnici[user]);
        }


        [HttpPost]
        public ActionResult PublishIzmenjenogKorisnika(string user, string ime, string prezime, string pol, string email,
            DateTime datumRodjenja, string lozinka)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            Pol p = Pol.M;
            if (pol == "Z")
                p = Pol.Z;

            korisnici[user].ime = ime;
            korisnici[user].prezime = prezime;
            korisnici[user].pol = p;
            korisnici[user].email = email;
            korisnici[user].datumRodjenja = datumRodjenja;
            if (lozinka != "")
                korisnici[user].lozinka = lozinka;
            Data.UpdateKorisnikXml(korisnici[user]);


            ViewBag.Message = "Korisnik izmenjen!";
            ViewData["Korisnici"] = korisnici;
            return View("AdminPanelKorisnici", admin);
        }

        [HttpPost]
        public ActionResult ObrisiProdavca(string user)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            korisnici[user].obrisan = true;
            Data.UpdateKorisnikXml(korisnici[user]);

            foreach (Proizvod p in korisnici[user].listaObjavljenihProizvoda)
            {
                p.obrisan = true;
                proizvodi[p.id].obrisan = true;
                Data.UpdateProizvodXml(p);
                //RAZMISLITI DA LI TREBA RECENZIJE BRISATI
            }
            //Ponovo iscitavamo korisnike zbog proizvoda koji su se promenili a nalazili su se u liti omiljenih
            korisnici.Clear();
            korisnici = Data.ReadKorisnici();

            ViewBag.Message = "Prodavac obrisan!";
            ViewData["Korisnici"] = korisnici;
            return View("AdminPanelKorisnici", admin);
        }

        [HttpPost]
        public ActionResult ObrisiKupca(string user)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            korisnici[user].obrisan = true;
            Data.UpdateKorisnikXml(korisnici[user]);
            foreach (Porudzbina p in korisnici[user].listaPorudzbina)
            {
                if (p.status == Status.AKTIVNA)
                {
                    p.status = Status.OTKAZANA;
                    porudzbine[p.id].status = Status.OTKAZANA;

                    proizvodi[p.idProizvod].kolicina += p.kolicina;
                    proizvodi[p.idProizvod].dostupan = true;

                    Data.UpdateProizvodXml(proizvodi[p.idProizvod]);
                    Data.UpdatePorudzbinaXml(p);
                }
            }

            ViewBag.Message = "Kupac obrisan!";
            ViewData["Korisnici"] = korisnici;
            return View("AdminPanelKorisnici", admin);
        }

        [HttpPost]
        public ActionResult IzmeniProizvod(int id)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            return View(proizvodi[id]);
        }

        [HttpPost]
        public ActionResult PublishIzmenjenProizvod(int id, string naziv, string cena, int kolicina, string opis, string slika,
            string grad)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            double cenaParse;
            bool valid = double.TryParse(cena, out cenaParse);

            if (!valid)
            {
                ViewBag.Greska = "Proizvod nije izmenjen, pogresna vrednost za cenu!";
                return View("IzmeniProizvod", proizvodi[id]);
            }

            proizvodi[id].naziv = naziv;
            proizvodi[id].cena = cenaParse;
            proizvodi[id].kolicina = kolicina;
            proizvodi[id].opis = opis;
            proizvodi[id].grad = grad;
            if(slika != "")
                proizvodi[id].slika = slika;
            Data.UpdateProizvodXml(proizvodi[id]);

            foreach(Korisnik k in korisnici.Values)
            {
                if(k.uloga == Uloga.Kupac)
                    foreach(Proizvod p in k.listaOmiljenihProizvoda)
                    {
                        if(p.id == id)
                        {
                            p.naziv = naziv;
                            p.cena = cenaParse;
                            p.kolicina = kolicina;
                            p.opis = opis;
                            p.grad = grad;
                            if (slika != "")
                                p.slika = slika;
                        }
                    }
                if(k.uloga == Uloga.Prodavac)
                    foreach(Proizvod p in k.listaObjavljenihProizvoda)
                    {
                        if (p.id == id)
                        {
                            p.naziv = naziv;
                            p.cena = cenaParse;
                            p.kolicina = kolicina;
                            p.opis = opis;
                            p.grad = grad;
                            if (slika != "")
                                p.slika = slika;
                        }
                    }
            }

            ViewBag.Message = "Proizvod izmenjen!";
            ViewData["Proizvodi"] = proizvodi;
            return View("AdminPanelProizvodi", admin);
        }

        [HttpPost]
        public ActionResult ObrisiProizvod(int id)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            proizvodi[id].obrisan = true;
            Data.UpdateProizvodXml(proizvodi[id]);

            foreach(Korisnik k in korisnici.Values)
            {
                if (k.uloga == Uloga.Kupac)
                    foreach (Proizvod p in k.listaOmiljenihProizvoda)
                        if (p.id == id)
                            p.obrisan = true;
                if(k.uloga == Uloga.Prodavac)
                    foreach(Proizvod p in k.listaObjavljenihProizvoda)
                        if (p.id == id)
                            p.obrisan = true;
            }

            ViewBag.Message = "Proizvod obrisan!";
            ViewData["Proizvodi"] = proizvodi;
            return View("AdminPanelProizvodi", admin);
        }

        [HttpPost]
        public ActionResult PorudzbinaIzvrsena(int id)
        {
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            porudzbine[id].status = Status.IZVRSENA;
            Data.UpdatePorudzbinaXml(porudzbine[id]);
            int idxMenjanja = korisnici[porudzbine[id].userKupac].listaPorudzbina.FindIndex(p => p.id == id);
            korisnici[porudzbine[id].userKupac].listaPorudzbina[idxMenjanja].status = Status.IZVRSENA;

            ViewBag.Message = "Porudzbina oznacena kao izvrsena";
            ViewData["Porudzbine"] = porudzbine;
            return View("AdminPanelPorudzbine", admin);
        }

        [HttpPost]
        public ActionResult PorudzbinaOtkazana(int id)  //AKCIJA SAMO ZA AKTIVNE PORUDZBINE
        {
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];

            porudzbine[id].status = Status.OTKAZANA;
            Data.UpdatePorudzbinaXml(porudzbine[id]);
            int idxMenjanja = korisnici[porudzbine[id].userKupac].listaPorudzbina.FindIndex(p => p.id == id);
            korisnici[porudzbine[id].userKupac].listaPorudzbina[idxMenjanja].status = Status.OTKAZANA;

            //proizvod ponovo postaje aktivan
            proizvodi[porudzbine[id].idProizvod].kolicina += porudzbine[id].kolicina;
            proizvodi[porudzbine[id].idProizvod].dostupan = true;
            Data.UpdateProizvodXml(proizvodi[porudzbine[id].idProizvod]);

            foreach (Korisnik k in korisnici.Values)
            {
                if (k.uloga == Uloga.Kupac)
                {
                    idxMenjanja = k.listaOmiljenihProizvoda.FindIndex(p => p.id == porudzbine[id].idProizvod);
                    if (idxMenjanja != -1)
                    {
                        k.listaOmiljenihProizvoda[idxMenjanja].kolicina += porudzbine[id].kolicina;
                        k.listaOmiljenihProizvoda[idxMenjanja].dostupan = true;
                    }
                }
                if(k.uloga == Uloga.Prodavac)
                {
                    idxMenjanja = k.listaObjavljenihProizvoda.FindIndex(p => p.id == porudzbine[id].idProizvod);
                    if(idxMenjanja != -1)
                    {
                        k.listaObjavljenihProizvoda[idxMenjanja].kolicina += porudzbine[id].kolicina;
                        k.listaObjavljenihProizvoda[idxMenjanja].dostupan = true;
                    }
                }
            }

            ViewBag.Message = "Porudzbina otkazana";
            ViewData["Porudzbine"] = porudzbine;
            return View("AdminPanelPorudzbine", admin);
        }

        [HttpPost]
        public ActionResult OdobriRecenziju(int id)
        {
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            recenzije[id].status = StatusRecenzije.ODOBRENA;
            Data.UpdateRecenzijaXml(recenzije[id]);

            int idxMenjanja = proizvodi[recenzije[id].idProizvod].listaRecenzija.FindIndex(r => r.id == id);
            proizvodi[recenzije[id].idProizvod].listaRecenzija[idxMenjanja].status = StatusRecenzije.ODOBRENA;

            ViewBag.Message = "Recenzija odobrena!";
            ViewData["Recenzije"] = recenzije;
            ViewData["Proizvodi"] = proizvodi;
            ViewData["Korisnici"] = korisnici;
            return View("AdminPanelRecenzije", admin);
        }

        [HttpPost]
        public ActionResult OdbiRecenziju(int id)
        {
            Korisnik admin = (Korisnik)HttpContext.Session["korisnik"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            recenzije[id].status = StatusRecenzije.ODBIJENA;
            Data.UpdateRecenzijaXml(recenzije[id]);

            int idxMenjanja = proizvodi[recenzije[id].idProizvod].listaRecenzija.FindIndex(r => r.id == id);
            proizvodi[recenzije[id].idProizvod].listaRecenzija[idxMenjanja].status = StatusRecenzije.ODBIJENA;

            ViewBag.Message = "Recenzija odbijena!";
            ViewData["Recenzije"] = recenzije;
            ViewData["Proizvodi"] = proizvodi;
            ViewData["Korisnici"] = korisnici;
            return View("AdminPanelRecenzije", admin);
        }
    }
}