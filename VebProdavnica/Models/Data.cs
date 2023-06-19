using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml;

namespace VebProdavnica.Models
{
    public class Data
    {
        static string korisniciPath = HostingEnvironment.MapPath("~/App_Data/korisnici.xml");
        static string proizvodiPath = HostingEnvironment.MapPath("~/App_Data/proizvodi.xml");
        static string porudzbinePath = HostingEnvironment.MapPath("~/App_Data/porudzbine.xml");
        static string recenzijePath = HostingEnvironment.MapPath("~/App_Data/recenzije.xml");

        public static Dictionary<string, Korisnik> ReadKorisnici()
        {
            Dictionary<string, Korisnik> ret = new Dictionary<string, Korisnik>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(korisniciPath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList korisnikNodes = root.SelectNodes("korisnik");

            foreach(XmlNode korisnikNode in korisnikNodes)
            {
                List<Porudzbina> listaPorudzbina = new List<Porudzbina>();
                List<Proizvod> listaOmiljenihProizvoda = new List<Proizvod>();
                List<Proizvod> listaObjavljenihProizvoda = new List<Proizvod>();

                string korisnickoIme = korisnikNode.SelectSingleNode("KORISNICKO_IME")?.InnerText;
                string lozinka = korisnikNode.SelectSingleNode("LOZINKA")?.InnerText;
                string ime = korisnikNode.SelectSingleNode("IME")?.InnerText;
                string prezime = korisnikNode.SelectSingleNode("PREZIME")?.InnerText;
                string polStr = korisnikNode.SelectSingleNode("POL")?.InnerText;
                string email = korisnikNode.SelectSingleNode("EMAIL")?.InnerText;
                string datumStr = korisnikNode.SelectSingleNode("DATUM")?.InnerText;
                string ulogaStr = korisnikNode.SelectSingleNode("ULOGA")?.InnerText;

                Pol pol = Pol.M;
                if (polStr == "Z")
                    pol = Pol.Z;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);

                Uloga uloga = Uloga.Kupac;
                if (ulogaStr == "Administrator")
                    uloga = Uloga.Administrator;
                else if (ulogaStr == "Prodavac")
                    uloga = Uloga.Prodavac;

                if (uloga == Uloga.Kupac)
                {
                    XmlNode omiljeniNode = korisnikNode.SelectSingleNode("omiljeni");
                    XmlNodeList idNodes = omiljeniNode.SelectNodes("ID_PROIZVOD");
                    foreach (XmlNode idNode in idNodes)
                    {
                        string id = idNode.InnerText;

                        Proizvod omiljeni = VratiOmiljeni(int.Parse(id));
                        if(omiljeni != null)
                            listaOmiljenihProizvoda.Add(omiljeni);
                    }

                    listaPorudzbina = VratiPorudzbineKorisnika(korisnickoIme);

                }
                if (uloga == Uloga.Prodavac)
                {
                    listaObjavljenihProizvoda = VratiObjavljene(korisnickoIme);
                }

                Korisnik novi = new Korisnik(korisnickoIme,lozinka,ime,prezime,pol,email,datum,uloga,
                    listaPorudzbina,listaOmiljenihProizvoda,listaObjavljenihProizvoda);
                ret.Add(korisnickoIme, novi);
            }


            return ret;
        }

        public static Dictionary<int, Proizvod> ReadProizvodi()
        {
            Dictionary<int, Proizvod> ret = new Dictionary<int, Proizvod>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(proizvodiPath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList proizvodNodes = root.SelectNodes("proizvod");

            foreach (XmlNode proizvodNode in proizvodNodes)
            {
                List<Recenzija> recenzije = new List<Recenzija>();
                string id = proizvodNode.SelectSingleNode("ID")?.InnerText;
                string naziv = proizvodNode.SelectSingleNode("NAZIV")?.InnerText;
                string cena = proizvodNode.SelectSingleNode("CENA")?.InnerText;
                string kolicina = proizvodNode.SelectSingleNode("KOLICINA")?.InnerText;
                string opis = proizvodNode.SelectSingleNode("OPIS")?.InnerText;
                string slika = proizvodNode.SelectSingleNode("SLIKA")?.InnerText;
                string datumStr = proizvodNode.SelectSingleNode("DATUM")?.InnerText;
                string grad = proizvodNode.SelectSingleNode("GRAD")?.InnerText;
                string dostupanStr = proizvodNode.SelectSingleNode("DOSTUPAN")?.InnerText;
                string userProdavca = proizvodNode.SelectSingleNode("ID_PRODAVCA")?.InnerText;

                bool dostupan = false;
                if (dostupanStr == "true")
                    dostupan = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);


                recenzije = vratiRecenzije(int.Parse(id));

                Proizvod novi = new Proizvod(int.Parse(id), naziv, double.Parse(cena), int.Parse(kolicina),
                    opis, slika, datum, grad, dostupan, userProdavca);
                ret.Add(int.Parse(id), novi);
            }


            return ret;
        }

        public static Dictionary<int, Porudzbina> ReadPorudzbine()
        {
            Dictionary<int, Porudzbina> ret = new Dictionary<int, Porudzbina>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(porudzbinePath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList porudzbinaNodes = root.SelectNodes("porudzbina");

            foreach (XmlNode porudzbinaNode in porudzbinaNodes)
            {
                string id = porudzbinaNode.SelectSingleNode("ID")?.InnerText;
                string kolicina = porudzbinaNode.SelectSingleNode("KOLICINA")?.InnerText;
                string datumStr = porudzbinaNode.SelectSingleNode("DATUM")?.InnerText;
                string statusStr = porudzbinaNode.SelectSingleNode("STATUS")?.InnerText;
                string idProizvoda = porudzbinaNode.SelectSingleNode("ID_PROIZVOD")?.InnerText;
                string userKorisnik = porudzbinaNode.SelectSingleNode("USER_KORISNIK")?.InnerText;


                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);

                Status status = Status.IZVRSENA;
                if (statusStr == "AKTIVNA")
                    status = Status.AKTIVNA;
                else if (statusStr == "OTKAZANA")
                    status = Status.OTKAZANA;

                Porudzbina nova = new Porudzbina(int.Parse(id), int.Parse(idProizvoda), int.Parse(kolicina),
                    userKorisnik, datum, status);
                ret.Add(int.Parse(id), nova);

            }

            return ret;
        }

        public static Dictionary<int, Recenzija> ReadRecenzije()
        {
            Dictionary<int, Recenzija> ret = new Dictionary<int, Recenzija>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(recenzijePath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList recenzijaNodes = root.SelectNodes("recenzija");

            foreach (XmlNode recenzijaNode in recenzijaNodes)
            {
                string id = recenzijaNode.SelectSingleNode("ID")?.InnerText;
                string naslov = recenzijaNode.SelectSingleNode("NASLOV")?.InnerText;
                string sadrzaj = recenzijaNode.SelectSingleNode("SADRZAJ")?.InnerText;
                string slika = recenzijaNode.SelectSingleNode("SLIKA")?.InnerText;
                string idProizvoda = recenzijaNode.SelectSingleNode("ID_PROIZVOD")?.InnerText;
                string userKorisnik = recenzijaNode.SelectSingleNode("USER_KORISNIK")?.InnerText;


                Recenzija nova = new Recenzija(int.Parse(id), int.Parse(idProizvoda), userKorisnik, naslov,
                        sadrzaj, slika);
                ret.Add(int.Parse(id), nova);

            }

            return ret;
        }


        private static List<Recenzija> vratiRecenzije(int idTrazenogProizvoda)
        {
            List<Recenzija> ret = new List<Recenzija>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(recenzijePath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList recenzijaNodes = root.SelectNodes("recenzija");

            foreach (XmlNode recenzijaNode in recenzijaNodes)
            {
                string id = recenzijaNode.SelectSingleNode("ID")?.InnerText;
                string naslov = recenzijaNode.SelectSingleNode("NASLOV")?.InnerText;
                string sadrzaj = recenzijaNode.SelectSingleNode("SADRZAJ")?.InnerText;
                string slika = recenzijaNode.SelectSingleNode("SLIKA")?.InnerText;
                string idProizvoda = recenzijaNode.SelectSingleNode("ID_PROIZVOD")?.InnerText;
                string userKorisnik = recenzijaNode.SelectSingleNode("USER_KORISNIK")?.InnerText;

                if(int.Parse(idProizvoda) == idTrazenogProizvoda)
                {
                    Recenzija trazena = new Recenzija(int.Parse(id), int.Parse(idProizvoda), userKorisnik, naslov,
                        sadrzaj, slika);
                    ret.Add(trazena);
                }

            }

                return ret;
        }

        private static List<Proizvod> VratiObjavljene(string korisnickoImeTrazenog)
        {
            List<Proizvod> ret = new List<Proizvod>();


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(proizvodiPath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList proizvodNodes = root.SelectNodes("proizvod");

            foreach (XmlNode proizvodNode in proizvodNodes)
            {
                string id = proizvodNode.SelectSingleNode("ID")?.InnerText;
                string naziv = proizvodNode.SelectSingleNode("NAZIV")?.InnerText;
                string cena = proizvodNode.SelectSingleNode("CENA")?.InnerText;
                string kolicina = proizvodNode.SelectSingleNode("KOLICINA")?.InnerText;
                string opis = proizvodNode.SelectSingleNode("OPIS")?.InnerText;
                string slika = proizvodNode.SelectSingleNode("SLIKA")?.InnerText;
                string datumStr = proizvodNode.SelectSingleNode("DATUM")?.InnerText;
                string grad = proizvodNode.SelectSingleNode("GRAD")?.InnerText;
                string dostupanStr = proizvodNode.SelectSingleNode("DOSTUPAN")?.InnerText;
                string userProdavca = proizvodNode.SelectSingleNode("ID_PRODAVCA")?.InnerText;

                bool dostupan = false;
                if (dostupanStr == "true")
                    dostupan = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);


                if (userProdavca == korisnickoImeTrazenog)
                {
                    Proizvod objavljen = new Proizvod(int.Parse(id), naziv, double.Parse(cena), int.Parse(kolicina),
                        opis, slika, datum, grad, dostupan, userProdavca);
                    ret.Add(objavljen);
                }

            }

            return ret;
        }

        private static List<Porudzbina> VratiPorudzbineKorisnika(string korisnickoImeTrazenog)
        {
            List<Porudzbina> porudzbine = new List<Porudzbina>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(porudzbinePath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList porudzbinaNodes = root.SelectNodes("porudzbina");

            foreach (XmlNode porudzbinaNode in porudzbinaNodes)
            {
                string id = porudzbinaNode.SelectSingleNode("ID")?.InnerText;
                string kolicina = porudzbinaNode.SelectSingleNode("KOLICINA")?.InnerText;
                string datumStr = porudzbinaNode.SelectSingleNode("DATUM")?.InnerText;
                string statusStr = porudzbinaNode.SelectSingleNode("STATUS")?.InnerText;
                string idProizvoda = porudzbinaNode.SelectSingleNode("ID_PROIZVOD")?.InnerText;
                string userKorisnik = porudzbinaNode.SelectSingleNode("USER_KORISNIK")?.InnerText;


                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);

                Status status = Status.IZVRSENA;
                if (statusStr == "AKTIVNA")
                    status = Status.AKTIVNA;
                else if (statusStr == "OTKAZANA")
                    status = Status.OTKAZANA;

                if(userKorisnik == korisnickoImeTrazenog)
                {
                    Porudzbina trazena = new Porudzbina(int.Parse(id), int.Parse(idProizvoda), int.Parse(kolicina), userKorisnik,
                        datum, status);
                    porudzbine.Add(trazena);
                }

            }

            return porudzbine;
        }

        private static Proizvod VratiOmiljeni(int trazeniId)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(proizvodiPath);

            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList proizvodNodes = root.SelectNodes("proizvod");

            foreach(XmlNode proizvodNode in proizvodNodes)
            {
                string id = proizvodNode.SelectSingleNode("ID")?.InnerText;
                string naziv = proizvodNode.SelectSingleNode("NAZIV")?.InnerText;
                string cena = proizvodNode.SelectSingleNode("CENA")?.InnerText;
                string kolicina = proizvodNode.SelectSingleNode("KOLICINA")?.InnerText;
                string opis = proizvodNode.SelectSingleNode("OPIS")?.InnerText;
                string slika = proizvodNode.SelectSingleNode("SLIKA")?.InnerText;
                string datumStr = proizvodNode.SelectSingleNode("DATUM")?.InnerText;
                string grad = proizvodNode.SelectSingleNode("GRAD")?.InnerText;
                string dostupanStr = proizvodNode.SelectSingleNode("DOSTUPAN")?.InnerText;
                string userProdavca = proizvodNode.SelectSingleNode("ID_PRODAVCA")?.InnerText;

                bool dostupan = false;
                if (dostupanStr == "true")
                    dostupan = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);

                if (int.Parse(id) == trazeniId)
                {
                    Proizvod omiljeni = new Proizvod(int.Parse(id), naziv, double.Parse(cena), int.Parse(kolicina),
                        opis, slika, datum, grad, dostupan, userProdavca);
                    return omiljeni;
                }

            }

            return null;
        }
    }
}