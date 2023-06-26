using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using VebProdavnica.Models;
using WebGrease;

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

        public ActionResult Prijava()
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
                    return View("Prijava");
                }
                Session["korisnik"] = korisnik;
            }
            return RedirectToAction("Index");
        }

        public ActionResult Odjava()
        {
            Session["korisnik"] = null;

            return RedirectToAction("Index");
        }

        public ActionResult DetaljiProizvoda(int id)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            return View(proizvodi[id]);
        }

        [HttpPost]
        public ActionResult DodajUOmiljene(int id)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            if (!((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Any(p => p.id == id))
            {
                ((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Add(proizvodi[id]);
                korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].listaOmiljenihProizvoda.Add(proizvodi[id]);
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
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            if (proizvodi[id].kolicina >= kolicina)
            {
                Porudzbina nova = new Porudzbina(Data.GenerateID(), id, kolicina,
                    ((Korisnik)Session["korisnik"]).korisnickoIme, DateTime.Now, Status.AKTIVNA, false, 0);
                //Session.porudzbine
                //((Korisnik)Session["korisnik"]).listaPorudzbina.Add(nova);
                //korisnici.porudzbine
                korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].listaPorudzbina.Add(nova);
                //porudzbine + XMLPorudzbine
                porudzbine.Add(nova.id, nova);
                Data.UpdatePorudzbinaXml(nova);
                //proizvodi.kolicina + XMLProizvodi
                proizvodi[id].kolicina -= kolicina;
                //Idx proizvoda u listi korisnika koji prodaje
                int idxMenjanja = korisnici[proizvodi[id].userProdavca].listaObjavljenihProizvoda.FindIndex(p => p.id == id);
                korisnici[proizvodi[id].userProdavca].listaObjavljenihProizvoda[idxMenjanja].kolicina -= kolicina;
                if (proizvodi[id].kolicina <= 0)
                {
                    proizvodi[id].dostupan = false;
                    korisnici[proizvodi[id].userProdavca].listaObjavljenihProizvoda[idxMenjanja].dostupan = false;
                }
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

        public ActionResult Profil()
        {
            Korisnik trenutni = (Korisnik)HttpContext.Session["korisnik"];

            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            return View(trenutni);
        }

        [HttpPost]
        public ActionResult IzmeniPodatkeKorisnika(string ime, string prezime, string email, DateTime datumRodjenja, 
            string staraLozinka, string novaLozinka)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            Korisnik update = (Korisnik)HttpContext.Session["korisnik"];
            if(staraLozinka == update.lozinka)
            {
                update.ime = ime;
                korisnici[update.korisnickoIme].ime = ime;
                update.prezime = prezime;
                korisnici[update.korisnickoIme].prezime = prezime;
                update.email = email;
                korisnici[update.korisnickoIme].email = email;
                update.datumRodjenja = datumRodjenja;
                korisnici[update.korisnickoIme].datumRodjenja = datumRodjenja;
                if (novaLozinka != "")
                {
                    update.lozinka = novaLozinka;
                    korisnici[update.korisnickoIme].lozinka = novaLozinka;
                }
                Data.UpdateKorisnikXml(update);
            }
            else
            {
                ViewBag.Greska = "Stara lozinka je ne ispravna";
            }

            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil",update);
        }

        [HttpPost]
        public ActionResult OznaciPorudzbinuIzvrsenom(int id)
        {
            Korisnik trenutni = (Korisnik)HttpContext.Session["korisnik"];
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            
            porudzbine[id].status = Status.IZVRSENA;
            trenutni.listaPorudzbina.Find(x => x.id == id).status = Status.IZVRSENA;
            korisnici[trenutni.korisnickoIme].listaPorudzbina.Find(x => x.id == id).status = Status.IZVRSENA;
            Data.UpdatePorudzbinaXml(porudzbine[id]);

            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil",trenutni);
        }

        [HttpPost]
        public ActionResult OstaviRecenziju(int id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public ActionResult PublishRecenziju(int idPorudzbine, string naslov,string sadrzaj,string slika)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            Korisnik recezent = (Korisnik)HttpContext.Session["korisnik"];

            Recenzija nova = new Recenzija(Data.GenerateID(), porudzbine[idPorudzbine].idProizvod,
                recezent.korisnickoIme, naslov, sadrzaj, slika, false);

            //proizvodi.recenzije
            proizvodi[nova.idProizvod].listaRecenzija.Add(nova);
            //porudzbine + XMLporudzbine
            porudzbine[idPorudzbine].recenzijaOstavljena = true;
            porudzbine[idPorudzbine].idRecenzije = nova.id;
            Data.UpdatePorudzbinaXml(porudzbine[idPorudzbine]);
            //recenzije + XMLrecenzije
            recenzije.Add(nova.id, nova);
            Data.UpdateRecenzijaXml(nova);
            //Session.porudzbine
            int idxMenjanja = recezent.listaPorudzbina.FindIndex(p => p.id == idPorudzbine);
            recezent.listaPorudzbina[idxMenjanja] = porudzbine[idPorudzbine];
            //korisnici.porudzbine
            korisnici[recezent.korisnickoIme].listaPorudzbina[idxMenjanja] = porudzbine[idPorudzbine];

            ViewBag.Message = "Recenzija postavljena";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil", recezent);
        }

        [HttpPost]
        public ActionResult ObrisiRecenziju(int id)
        {
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Korisnik recezent = (Korisnik)HttpContext.Session["korisnik"];

            //Ovo ne moze da ne bude tacno al za svaki slucaj
            if (porudzbine[id].recenzijaOstavljena == true)
            {
                //Recenzije + XMLRecenzije
                recenzije[porudzbine[id].idRecenzije].obrisana = true;
                Data.UpdateRecenzijaXml(recenzije[porudzbine[id].idRecenzije]);
                
                //Proizvodi.recenzije
                int idxMenjanja = proizvodi[porudzbine[id].idProizvod].listaRecenzija.FindIndex(r => r.id == porudzbine[id].idRecenzije);
                proizvodi[porudzbine[id].idProizvod].listaRecenzija[idxMenjanja] = recenzije[porudzbine[id].idRecenzije];
                
                //Porudzbine + XMLPorudzbine
                porudzbine[id].recenzijaOstavljena = false;
                porudzbine[id].idRecenzije = 0;
                Data.UpdatePorudzbinaXml(porudzbine[id]);

                //korisnici.porudzbine
                idxMenjanja = recezent.listaPorudzbina.FindIndex(p => p.id == id);
                korisnici[recezent.korisnickoIme].listaPorudzbina[idxMenjanja] = porudzbine[id];
                //Session.porudzbine
                recezent.listaPorudzbina[idxMenjanja] = porudzbine[id];
            }

            ViewBag.Message = "Recenzija obrisana";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil", (Korisnik)HttpContext.Session["korisnik"]);
        }

        [HttpPost]
        public ActionResult IzmeniRecenziju(int id)
        {
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];

            ViewData["Recenzija"] = recenzije[porudzbine[id].idRecenzije];
            return View();
        }

        [HttpPost]
        public ActionResult PublishIzmenjenuRecenziju(int idRecenzije, string naslov, string sadrzaj, string slika)
        {
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];

            //recenzije + XMLRecenzije
            recenzije[idRecenzije].naslov = naslov;
            recenzije[idRecenzije].sadrzajRecenzije = sadrzaj;
            if(slika != "")
                recenzije[idRecenzije].slika = slika;
            Data.UpdateRecenzijaXml(recenzije[idRecenzije]);

            //proizvodi.recenzije
            int idxMenjanja = proizvodi[recenzije[idRecenzije].idProizvod].listaRecenzija.FindIndex(r => r.id == idRecenzije);
            proizvodi[recenzije[idRecenzije].idProizvod].listaRecenzija[idxMenjanja] = recenzije[idRecenzije];

            ViewBag.Message = "Recenzija uspesno izmenjena";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil", (Korisnik)Session["korisnik"]);
        }
        public ActionResult DodajProizvod()
        {
            return View();
        }

        public ActionResult PublishProizvod(string naziv, string cena, int kolicina, string opis, string slika, 
            string grad)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik trenutni = (Korisnik)Session["korisnik"];

            bool uspesno;
            double cenaDouble;
            uspesno = double.TryParse(cena, out cenaDouble);
            if(!uspesno)
            {
                ViewBag.Greska = "Ne pravilno unesena cena.";
                return View();
            }
            bool dostupan = true;
            if (kolicina == 0)
                dostupan = false;
            string userProdavca = trenutni.korisnickoIme;

            //System.Diagnostics.Debug.WriteLine("Ovo je opis = " + opis);
            Proizvod novi = new Proizvod(naziv, cenaDouble, kolicina, opis, slika, grad, dostupan, userProdavca);


            proizvodi.Add(novi.id, novi);
            //TODO Ovde se nekako 2 puta doda, proveriti to
            //trenutni.listaObjavljenihProizvoda.Add(novi);
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda.Add(novi);
            Data.UpdateProizvodXml(novi);

            ViewBag.Message = "Proizvod uspesno dodat!";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil",trenutni);
        }

        public ActionResult ObrisiProizvod(int id)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik trenutni = (Korisnik)Session["korisnik"];

            proizvodi[id].obrisan = true;

            //int idxMenjanja = trenutni.listaObjavljenihProizvoda.FindIndex(p => p.id == id);
            //trenutni.listaObjavljenihProizvoda[idxMenjanja].obrisan = true;

            int idxMenjanja = korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda.FindIndex(p => p.id == id);
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanja].obrisan = true;
            
            //Svakom korisniku brisemo ovaj proizvod iz liste omlijenih
            foreach(Korisnik k in korisnici.Values)
            {
                idxMenjanja = k.listaOmiljenihProizvoda.FindIndex(p => p.id == id);
                if(idxMenjanja != -1)
                {
                    k.listaOmiljenihProizvoda[idxMenjanja].obrisan = true;
                }
            }
            Data.UpdateProizvodXml(proizvodi[id]);

            ViewBag.Message = "Proizvod uspesno obrisan";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil", trenutni);
        }

        public ActionResult IzmeniProizvod(int id)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];

            return View(proizvodi[id]);
        }

        public ActionResult PublishIzmenjenProizvod(int id, string naziv, string cena,int kolicina, string opis, 
            string slika, string grad)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik trenutni = (Korisnik)Session["korisnik"];

            double cenaParse;
            bool valid = double.TryParse(cena, out cenaParse);

            if(!valid)
            {
                ViewBag.Greska = "Proizvod nije izmenjen, pogresna vrednost za cenu!";
                return View(proizvodi[id]);
            }

            //pozicija u listi objavljenih proizvoda za Session
            int idxMenjanja = trenutni.listaObjavljenihProizvoda.FindIndex(p => p.id == id);
            //pozicija u listi objavljenih proizvoda za Application
            int idxMenjanjaApp = korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda.FindIndex(p => p.id == id);

            proizvodi[id].naziv = naziv;
            //trenutni.listaObjavljenihProizvoda[idxMenjanja].naziv = naziv;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].naziv = naziv;

            proizvodi[id].cena = cenaParse;
            //trenutni.listaObjavljenihProizvoda[idxMenjanja].cena = cenaParse;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].cena = cenaParse;

            proizvodi[id].kolicina = kolicina;
            //trenutni.listaObjavljenihProizvoda[idxMenjanja].kolicina = kolicina;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].kolicina = kolicina;

            proizvodi[id].opis = opis;
            //trenutni.listaObjavljenihProizvoda[idxMenjanja].opis = opis;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].opis = opis;

            if(slika != "")
            {
                proizvodi[id].slika = slika;
                //trenutni.listaObjavljenihProizvoda[idxMenjanja].slika = slika;
                korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].slika = slika;
            }

            proizvodi[id].grad = grad;
            //trenutni.listaObjavljenihProizvoda[idxMenjanja].grad = grad;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].grad = grad;

            if(kolicina > 0)
            {
                proizvodi[id].dostupan = true;
                //trenutni.listaObjavljenihProizvoda[idxMenjanja].dostupan = true;
                korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].dostupan = true;
            }   

            Data.UpdateProizvodXml(proizvodi[id]);

            //Menjanje svim korisnicima koji imaju taj proizvod u listi omiljenih
            foreach(Korisnik k in korisnici.Values)
            {
                //Trazimo proizvod iz liste omiljenih takav da mu je id jednak sa promenjenim proizvodom
                idxMenjanja = k.listaOmiljenihProizvoda.FindIndex(p => p.id == id);
                if(idxMenjanja != -1)
                {
                    k.listaOmiljenihProizvoda[idxMenjanja].naziv = naziv;
                    k.listaOmiljenihProizvoda[idxMenjanja].cena = cenaParse;
                    k.listaOmiljenihProizvoda[idxMenjanja].kolicina = kolicina;
                    k.listaOmiljenihProizvoda[idxMenjanja].grad = grad;
                    k.listaOmiljenihProizvoda[idxMenjanja].opis = opis;
                    if(slika != "")
                        k.listaOmiljenihProizvoda[idxMenjanja].slika = slika;
                    if(kolicina>0)
                        k.listaOmiljenihProizvoda[idxMenjanja].dostupan = true;
                }
            }

            ViewBag.Message = "Proizvod uspesno izmenjen";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil",trenutni);
        }

        public ActionResult FilterDostupan()
        {
            ViewBag.Dostupan = true;

            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil",(Korisnik)Session["korisnik"]);
        }

        public ActionResult ProfilSort(string kriterijum)
        {
            Korisnik trenutni = (Korisnik)Session["korisnik"];

            if (kriterijum == "Naziv(rastuce)")
                trenutni.listaObjavljenihProizvoda.Sort((x, y) => x.naziv.CompareTo(y.naziv));
            else if (kriterijum == "Naziv(opadajuce)")
                trenutni.listaObjavljenihProizvoda.Sort((x, y) => y.naziv.CompareTo(x.naziv));
            else if (kriterijum == "Cena(rastuce)")
                trenutni.listaObjavljenihProizvoda.Sort((x, y) => x.cena.CompareTo(y.cena));
            else if (kriterijum == "Cena(opadajuce)")
                trenutni.listaObjavljenihProizvoda.Sort((x, y) => y.cena.CompareTo(x.cena));
            else if (kriterijum == "Datum oglasavanja(rastuce)")
                trenutni.listaObjavljenihProizvoda.Sort((x, y) => x.datumPostavljanja.CompareTo(y.datumPostavljanja));
            else if (kriterijum == "Datum oglasavanja(opadajuce)")
                trenutni.listaObjavljenihProizvoda.Sort((x, y) => y.datumPostavljanja.CompareTo(x.datumPostavljanja));
 
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil", trenutni);
        }

    }
}