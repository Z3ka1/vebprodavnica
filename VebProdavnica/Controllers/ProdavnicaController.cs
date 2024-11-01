﻿using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
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
            string korisnickoIme, string lozinka)
        {
            //HttpContext.Application["korisnici"] = Data.ReadKorisnici();
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
            if (polStr == "Z")
                pol = Pol.Z;

            if(datumRodjenja > DateTime.Now)
            {
                ViewBag.Message = $"Ne validan datum rodjenja.";
                return View("Registracija");
            }

            Korisnik novi = new Korisnik(ime, prezime, pol, email, datumRodjenja, Uloga.Kupac, korisnickoIme, lozinka);
            korisnici.Add(novi.korisnickoIme, novi);
            Data.UpdateKorisnikXml(novi);
            Session["korisnik"] = novi;

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult UlogujSe(string username, string password)
        {
            //HttpContext.Application["korisnici"] = Data.ReadKorisnici();
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            if (korisnici.ContainsKey(username))
            {
                Korisnik korisnik = korisnici[username];
                if (korisnik == null || korisnici[username].lozinka != password || korisnik.obrisan)
                {
                    ViewBag.Greska = "Pogresno korisnicko ime ili lozinka!";
                    return View("Prijava");
                }
                Session["korisnik"] = korisnik;
            }
            else
            {
                ViewBag.Greska = "Pogresno korisnicko ime ili lozinka!";
                return View("Prijava");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Odjava()
        {
            Session["korisnik"] = null;
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            ViewData["Proizvodi"] = proizvodi;
            ViewBag.Odjava = "Odjavljeni ste.";
            return View("Index");
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

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
            {
                return RedirectToAction("Odjava");
            }

            if (!((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Any(p => p.id == id))
            {
                //((Korisnik)Session["korisnik"]).listaOmiljenihProizvoda.Add(proizvodi[id]);
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

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
            {
                return RedirectToAction("Odjava");
            }

            if (proizvodi[id].kolicina >= kolicina)
            {
                Porudzbina nova = new Porudzbina(Data.GenerateID(), id, kolicina,
                    ((Korisnik)Session["korisnik"]).korisnickoIme, DateTime.Now, Status.AKTIVNA, false, 0);

                //korisnici.porudzbine
                korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].listaPorudzbina.Add(nova);

                porudzbine.Add(nova.id, nova);
                Data.UpdatePorudzbinaXml(nova);

                proizvodi[id].kolicina -= kolicina;

                //Idx proizvoda u listi korisnika koji prodaje
                int idxMenjanja = korisnici[proizvodi[id].userProdavca].listaObjavljenihProizvoda.FindIndex(p => p.id == id);
                korisnici[proizvodi[id].userProdavca].listaObjavljenihProizvoda[idxMenjanja].kolicina = proizvodi[id].kolicina;
                
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
        } //Stara metoda

        public ActionResult Pretraga(string naziv, string cenaOd, string cenaDo, string grad, string kriterijum)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Proizvod> pretraga = new Dictionary<int, Proizvod>();

            cenaOd = cenaOd.Replace(" ", "");
            cenaDo = cenaDo.Replace(" ", "");

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

                if (naziv == "" && cenaOd == "" && cenaDo == "" && grad == "")
                    pretraga.Add(p.id, p);
            }

            var listaProizvoda = pretraga.ToList();

            if (kriterijum == "Naziv(rastuce)")
                listaProizvoda.Sort((x, y) => x.Value.naziv.CompareTo(y.Value.naziv));
            else if (kriterijum == "Naziv(opadajuce)")
                listaProizvoda.Sort((x, y) => y.Value.naziv.CompareTo(x.Value.naziv));
            else if (kriterijum == "Cena(rastuce)")
                listaProizvoda.Sort((x, y) => x.Value.cena.CompareTo(y.Value.cena));
            else if (kriterijum == "Cena(opadajuce)")
                listaProizvoda.Sort((x, y) => y.Value.cena.CompareTo(x.Value.cena));
            else if (kriterijum == "Datum oglasavanja(rastuce)")
                listaProizvoda.Sort((x, y) => x.Value.datumPostavljanja.CompareTo(y.Value.datumPostavljanja));
            else if (kriterijum == "Datum oglasavanja(opadajuce)")
                listaProizvoda.Sort((x, y) => y.Value.datumPostavljanja.CompareTo(x.Value.datumPostavljanja));

            pretraga = listaProizvoda.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            ViewBag.Pretraga = "Rezultati pretrage";
            ViewData["Proizvodi"] = pretraga;
            return View("Index");
        }

        public ActionResult Profil()
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");
            

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

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

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
                ViewBag.Message = "Profil uspesno azuriran.";
            }
            else
            {
                ViewBag.Greska = "Stara lozinka je ne ispravna!";
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

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

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
        public ActionResult PublishRecenziju(int idPorudzbine, string naslov,string sadrzaj,HttpPostedFileBase slika)
        {
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Porudzbina> porudzbine = (Dictionary<int, Porudzbina>)HttpContext.Application["porudzbine"];
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

            Korisnik recezent = (Korisnik)HttpContext.Session["korisnik"];

            string slikaName = "";
            if (slika != null && slika.ContentLength > 0)
            {
                string fileName = Path.GetFileName(slika.FileName);
                string fileExtension = Path.GetExtension(slika.FileName);

                string uniqueFileName = Guid.NewGuid().ToString("N") + fileExtension;

                string targetFolderPath = Server.MapPath("~/Slike");
                Directory.CreateDirectory(targetFolderPath);

                string fullFilePath = Path.Combine(targetFolderPath, uniqueFileName);
                slika.SaveAs(fullFilePath);
                slikaName = uniqueFileName;
            }

            Recenzija nova = new Recenzija(Data.GenerateID(), porudzbine[idPorudzbine].idProizvod,
                recezent.korisnickoIme, naslov, sadrzaj, slikaName, false, StatusRecenzije.CEKA);

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

            ViewBag.Message = "Recenzija poslata na uvid. Ukoliko bude odobrena bice objavljena na stranici proizvoda.";
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

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

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
        public ActionResult PublishIzmenjenuRecenziju(int idRecenzije, string naslov, string sadrzaj, HttpPostedFileBase slika)
        {
            Dictionary<int, Recenzija> recenzije = (Dictionary<int, Recenzija>)HttpContext.Application["recenzije"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

            string slikaName = "";
            if (slika != null && slika.ContentLength > 0)
            {
                string fileName = Path.GetFileName(slika.FileName);
                string fileExtension = Path.GetExtension(slika.FileName);

                string uniqueFileName = Guid.NewGuid().ToString("N") + fileExtension;

                string targetFolderPath = Server.MapPath("~/Slike");
                Directory.CreateDirectory(targetFolderPath);

                string fullFilePath = Path.Combine(targetFolderPath, uniqueFileName);
                slika.SaveAs(fullFilePath);
                slikaName = uniqueFileName;
            }

            //recenzije + XMLRecenzije
            recenzije[idRecenzije].naslov = naslov;
            recenzije[idRecenzije].sadrzajRecenzije = sadrzaj;
            recenzije[idRecenzije].status = StatusRecenzije.CEKA;
            if(slikaName != "")
                recenzije[idRecenzije].slika = slikaName;
            Data.UpdateRecenzijaXml(recenzije[idRecenzije]);

            //proizvodi.recenzije
            int idxMenjanja = proizvodi[recenzije[idRecenzije].idProizvod].listaRecenzija.FindIndex(r => r.id == idRecenzije);
            proizvodi[recenzije[idRecenzije].idProizvod].listaRecenzija[idxMenjanja] = recenzije[idRecenzije];

            ViewBag.Message = "Recenzija uspesno izmenjena i poslata na odobravanje.";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil", (Korisnik)Session["korisnik"]);
        }
        public ActionResult DodajProizvod()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PublishProizvod(string naziv, string cena, int kolicina, string opis, HttpPostedFileBase slika, 
            string grad)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik trenutni = (Korisnik)Session["korisnik"];

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

            cena = cena.Replace(" ", "");

            bool uspesno;
            double cenaDouble;
            uspesno = double.TryParse(cena, out cenaDouble);
            if(!uspesno)
            {
                ViewBag.Greska = "Ne pravilno unesena cena.";
                return View("DodajProizvod");
            }
            if(cenaDouble < 0)
            {
                ViewBag.Greska = "Cena mora biti pozitivan broj.";
                return View("DodajProizvod");
            }

            bool dostupan = true;
            if (kolicina == 0)
                dostupan = false;
            string userProdavca = trenutni.korisnickoIme;

            string slikaName = "";
            if(slika != null && slika.ContentLength > 0)
            {
                string fileName = Path.GetFileName(slika.FileName);
                string fileExtension = Path.GetExtension(slika.FileName);

                string uniqueFileName = Guid.NewGuid().ToString("N") + fileExtension;

                string targetFolderPath = Server.MapPath("~/Slike");
                Directory.CreateDirectory(targetFolderPath);

                string fullFilePath = Path.Combine(targetFolderPath, uniqueFileName);
                slika.SaveAs(fullFilePath);
                slikaName = uniqueFileName;
            }


            //System.Diagnostics.Debug.WriteLine("Ovo je opis = " + opis);
            Proizvod novi = new Proizvod(naziv, cenaDouble, kolicina, opis, slikaName, grad, dostupan, userProdavca);


            proizvodi.Add(novi.id, novi);
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

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

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

        [HttpPost]
        public ActionResult PublishIzmenjenProizvod(int id, string naziv, string cena,int kolicina, string opis, 
            HttpPostedFileBase slika, string grad)
        {
            Dictionary<string, Korisnik> korisnici = (Dictionary<string, Korisnik>)HttpContext.Application["korisnici"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Korisnik trenutni = (Korisnik)Session["korisnik"];

            if (korisnici[((Korisnik)Session["korisnik"]).korisnickoIme].obrisan == true)
                return RedirectToAction("Odjava");

            cena = cena.Replace(" ", "");
            double cenaParse;
            bool valid = double.TryParse(cena, out cenaParse);

            if(!valid)
            {
                ViewBag.Greska = "Proizvod nije izmenjen, pogresna vrednost za cenu!";
                return View("IzmeniProizvod",proizvodi[id]);
            }
            if(cenaParse < 0)
            {
                ViewBag.Greska = "Proizvod nije izmenjen, cena mora biti pozitivan broj!";
                return View("IzmeniProizvod", proizvodi[id]);
            }

            string slikaName = "";
            if (slika != null && slika.ContentLength > 0)
            {
                string fileName = Path.GetFileName(slika.FileName);
                string fileExtension = Path.GetExtension(slika.FileName);

                string uniqueFileName = Guid.NewGuid().ToString("N") + fileExtension;

                string targetFolderPath = Server.MapPath("~/Slike");
                Directory.CreateDirectory(targetFolderPath);

                string fullFilePath = Path.Combine(targetFolderPath, uniqueFileName);
                slika.SaveAs(fullFilePath);
                slikaName = uniqueFileName;
            }

            //pozicija u listi objavljenih proizvoda za Session
            int idxMenjanja = trenutni.listaObjavljenihProizvoda.FindIndex(p => p.id == id);
            //pozicija u listi objavljenih proizvoda za Application
            int idxMenjanjaApp = korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda.FindIndex(p => p.id == id);

            proizvodi[id].naziv = naziv;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].naziv = naziv;

            proizvodi[id].cena = cenaParse;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].cena = cenaParse;

            proizvodi[id].kolicina = kolicina;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].kolicina = kolicina;

            proizvodi[id].opis = opis;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].opis = opis;

            if(slikaName != "")
            {
                proizvodi[id].slika = slikaName;
                korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].slika = slikaName;
            }

            proizvodi[id].grad = grad;
            korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].grad = grad;

            if(kolicina > 0)
            {
                proizvodi[id].dostupan = true;
                korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].dostupan = true;
            }
            else
            {
                proizvodi[id].dostupan = false;
                korisnici[trenutni.korisnickoIme].listaObjavljenihProizvoda[idxMenjanjaApp].dostupan = false;
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
                    if(slikaName != "")
                        k.listaOmiljenihProizvoda[idxMenjanja].slika = slikaName;
                    if(kolicina>0)
                        k.listaOmiljenihProizvoda[idxMenjanja].dostupan = true;
                    else
                        k.listaOmiljenihProizvoda[idxMenjanja].dostupan = false;

                }
            }

            ViewBag.Message = "Proizvod uspesno izmenjen";
            ViewData["Proizvodi"] = proizvodi;
            return View("Profil",trenutni);
        }

        public ActionResult ProfilSort(string status, string kriterijum)
        {
            Korisnik trenutni = (Korisnik)Session["korisnik"];
            Dictionary<int, Proizvod> proizvodi = (Dictionary<int, Proizvod>)HttpContext.Application["proizvodi"];
            Dictionary<int, Proizvod> pretraga = new Dictionary<int, Proizvod>();

            foreach (Proizvod p in trenutni.listaObjavljenihProizvoda)
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
            ViewBag.pretraga = pretraga;
            return View("Profil", trenutni);
        }

    }
}