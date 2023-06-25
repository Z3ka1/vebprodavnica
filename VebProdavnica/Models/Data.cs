using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;

namespace VebProdavnica.Models
{
    public class Data
    {
        static string korisniciPath = HostingEnvironment.MapPath("~/App_Data/korisnici.xml");
        static string proizvodiPath = HostingEnvironment.MapPath("~/App_Data/proizvodi.xml");
        static string porudzbinePath = HostingEnvironment.MapPath("~/App_Data/porudzbine.xml");
        static string recenzijePath = HostingEnvironment.MapPath("~/App_Data/recenzije.xml");

        #region Read/Update Korisnici
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
                string obrisanStr = korisnikNode.SelectSingleNode("OBRISAN")?.InnerText;

                bool obrisan = false;
                if (obrisanStr == "True")
                    obrisan = true;

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
                    listaPorudzbina,listaOmiljenihProizvoda,listaObjavljenihProizvoda, obrisan);
                ret.Add(korisnickoIme, novi);
            }


            return ret;
        }

        //Ukoliko prosledjeni korisnik ne postoji dodaje se, u suprotnom se vrsi update
        public static void UpdateKorisnikXml(Korisnik update)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(korisniciPath);

            XmlNode korisnikNode = xmlDoc.SelectSingleNode($"//korisnik[KORISNICKO_IME='{update.korisnickoIme}']");
            if(korisnikNode != null)
                korisnikNode.ParentNode.RemoveChild(korisnikNode);

            //Lista koja sadrzi sve IDeve omiljenih proizvoda
            List<int> idProizvoda = update.listaOmiljenihProizvoda.Select(proizvod => proizvod.id).ToList();

            
            XmlElement korisnikXmlElement = xmlDoc.CreateElement("korisnik");

            XmlElement korisnickoImeElement = xmlDoc.CreateElement("KORISNICKO_IME");
            korisnickoImeElement.InnerText = update.korisnickoIme;
            korisnikXmlElement.AppendChild(korisnickoImeElement);

            XmlElement lozinkaElement = xmlDoc.CreateElement("LOZINKA");
            lozinkaElement.InnerText = update.lozinka;
            korisnikXmlElement.AppendChild(lozinkaElement);

            XmlElement imeElement = xmlDoc.CreateElement("IME");
            imeElement.InnerText = update.ime;
            korisnikXmlElement.AppendChild(imeElement);

            XmlElement prezimeElement = xmlDoc.CreateElement("PREZIME");
            prezimeElement.InnerText = update.prezime;
            korisnikXmlElement.AppendChild(prezimeElement);

            XmlElement polElement = xmlDoc.CreateElement("POL");
            polElement.InnerText = update.pol.ToString();
            korisnikXmlElement.AppendChild(polElement);

            XmlElement emailElement = xmlDoc.CreateElement("EMAIL");
            emailElement.InnerText = update.email;
            korisnikXmlElement.AppendChild(emailElement);

            XmlElement datumElement = xmlDoc.CreateElement("DATUM");
            datumElement.InnerText = update.datumRodjenja.ToString("dd.MM.yyyy");
            korisnikXmlElement.AppendChild(datumElement);

            XmlElement ulogaElement = xmlDoc.CreateElement("ULOGA");
            ulogaElement.InnerText = update.uloga.ToString();
            korisnikXmlElement.AppendChild(ulogaElement);

            XmlElement obirsanElement = xmlDoc.CreateElement("OBRISAN");
            obirsanElement.InnerText = update.obrisan.ToString();
            korisnikXmlElement.AppendChild(obirsanElement);

            XmlElement omiljeniElement = xmlDoc.CreateElement("omiljeni");
            foreach(int id in idProizvoda)
            {
                XmlElement idProizvodElement = xmlDoc.CreateElement("ID_PROIZVOD");
                idProizvodElement.InnerText = id.ToString();
                omiljeniElement.AppendChild(idProizvodElement);
            }
            korisnikXmlElement.AppendChild(omiljeniElement);

            xmlDoc.DocumentElement.AppendChild(korisnikXmlElement);
            xmlDoc.Save(korisniciPath);
        }
        #endregion

        #region Read/Update Proizvodi
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
                string userProdavca = proizvodNode.SelectSingleNode("USER_PRODAVCA")?.InnerText;
                string obrisanStr = proizvodNode.SelectSingleNode("OBRISAN")?.InnerText;

                bool obrisan = false;
                if (obrisanStr == "True")
                    obrisan = true;

                bool dostupan = false;
                if (dostupanStr == "True")
                    dostupan = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);


                recenzije = vratiRecenzije(int.Parse(id));

                Proizvod novi = new Proizvod(int.Parse(id), naziv, double.Parse(cena), int.Parse(kolicina),
                    opis, slika, datum, grad, dostupan, userProdavca, recenzije,obrisan);
                ret.Add(int.Parse(id), novi);
            }


            return ret;
        }

        public static void UpdateProizvodXml(Proizvod update)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(proizvodiPath);

            XmlNode proizvodNode = xmlDoc.SelectSingleNode($"//proizvod[ID='{update.id}']");
            if (proizvodNode != null)
                proizvodNode.ParentNode.RemoveChild(proizvodNode);

            XmlElement proizvodXmlElement = xmlDoc.CreateElement("proizvod");

            XmlElement idElement = xmlDoc.CreateElement("ID");
            idElement.InnerText = update.id.ToString();
            proizvodXmlElement.AppendChild(idElement);

            XmlElement nazivElement = xmlDoc.CreateElement("NAZIV");
            nazivElement.InnerText = update.naziv;
            proizvodXmlElement.AppendChild(nazivElement);

            XmlElement cenaElement = xmlDoc.CreateElement("CENA");
            cenaElement.InnerText = update.cena.ToString();
            proizvodXmlElement.AppendChild(cenaElement);

            XmlElement kolicinaElement = xmlDoc.CreateElement("KOLICINA");
            kolicinaElement.InnerText = update.kolicina.ToString();
            proizvodXmlElement.AppendChild(kolicinaElement);

            XmlElement opisElement = xmlDoc.CreateElement("OPIS");
            opisElement.InnerText = update.opis;
            proizvodXmlElement.AppendChild(opisElement);

            XmlElement slikaElement = xmlDoc.CreateElement("SLIKA");
            slikaElement.InnerText = update.slika;
            proizvodXmlElement.AppendChild(slikaElement);

            XmlElement datumElement = xmlDoc.CreateElement("DATUM");
            datumElement.InnerText = update.datumPostavljanja.ToString("dd.MM.yyyy");
            proizvodXmlElement.AppendChild(datumElement);

            XmlElement gradElement = xmlDoc.CreateElement("GRAD");
            gradElement.InnerText = update.grad;
            proizvodXmlElement.AppendChild(gradElement);

            XmlElement dostupanElement = xmlDoc.CreateElement("DOSTUPAN");
            dostupanElement.InnerText = update.dostupan.ToString();
            proizvodXmlElement.AppendChild(dostupanElement);

            XmlElement userProdavacElement = xmlDoc.CreateElement("USER_PRODAVCA");
            userProdavacElement.InnerText = update.userProdavca;
            proizvodXmlElement.AppendChild(userProdavacElement);

            XmlElement obrisanElement = xmlDoc.CreateElement("OBRISAN");
            obrisanElement.InnerText = update.obrisan.ToString();
            proizvodXmlElement.AppendChild(obrisanElement);

            xmlDoc.DocumentElement.AppendChild(proizvodXmlElement);
            xmlDoc.Save(proizvodiPath);
        }
        #endregion

        #region Read/Update Porudzbine
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
                string recenzijaOstavljenaStr = porudzbinaNode.SelectSingleNode("RECENZIJA_OSTAVLJENA")?.InnerText;
                string idRecenzije = porudzbinaNode.SelectSingleNode("ID_RECENZIJA")?.InnerText;

                bool recenzijaOstavljena = false;
                if (recenzijaOstavljenaStr == "True")
                    recenzijaOstavljena = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);

                Status status = Status.IZVRSENA;
                if (statusStr == "AKTIVNA")
                    status = Status.AKTIVNA;
                else if (statusStr == "OTKAZANA")
                    status = Status.OTKAZANA;

                Porudzbina nova = new Porudzbina(int.Parse(id), int.Parse(idProizvoda), int.Parse(kolicina),
                    userKorisnik, datum, status, recenzijaOstavljena, int.Parse(idRecenzije));
                ret.Add(int.Parse(id), nova);

            }

            return ret;
        }

        public static void UpdatePorudzbinaXml(Porudzbina update)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(porudzbinePath);

            XmlNode porudzbinaNode = xmlDoc.SelectSingleNode($"//porudzbina[ID='{update.id}']");
            if (porudzbinaNode != null)
                porudzbinaNode.ParentNode.RemoveChild(porudzbinaNode);

            XmlElement porudzbinaXmlElement = xmlDoc.CreateElement("porudzbina");

            XmlElement idElement = xmlDoc.CreateElement("ID");
            idElement.InnerText = update.id.ToString();
            porudzbinaXmlElement.AppendChild(idElement);

            XmlElement kolicinaElement = xmlDoc.CreateElement("KOLICINA");
            kolicinaElement.InnerText = update.kolicina.ToString();
            porudzbinaXmlElement.AppendChild(kolicinaElement);

            XmlElement datumElement = xmlDoc.CreateElement("DATUM");
            datumElement.InnerText = update.datumPorudzbine.ToString("dd.MM.yyyy");
            porudzbinaXmlElement.AppendChild(datumElement);

            XmlElement statusElement = xmlDoc.CreateElement("STATUS");
            statusElement.InnerText = update.status.ToString();
            porudzbinaXmlElement.AppendChild(statusElement);

            XmlElement idProizvodaElement = xmlDoc.CreateElement("ID_PROIZVOD");
            idProizvodaElement.InnerText = update.idProizvod.ToString();
            porudzbinaXmlElement.AppendChild(idProizvodaElement);

            XmlElement userKorisnikElement = xmlDoc.CreateElement("USER_KORISNIK");
            userKorisnikElement.InnerText = update.userKupac.ToString();
            porudzbinaXmlElement.AppendChild(userKorisnikElement);

            XmlElement recenzijaOstavljenaElement = xmlDoc.CreateElement("RECENZIJA_OSTAVLJENA");
            recenzijaOstavljenaElement.InnerText = update.recenzijaOstavljena.ToString();
            porudzbinaXmlElement.AppendChild(recenzijaOstavljenaElement);

            XmlElement idRecenzijaElement = xmlDoc.CreateElement("ID_RECENZIJA");
            idRecenzijaElement.InnerText = update.idRecenzije.ToString();
            porudzbinaXmlElement.AppendChild(idRecenzijaElement);

            xmlDoc.DocumentElement.AppendChild(porudzbinaXmlElement);
            xmlDoc.Save(porudzbinePath);
        }
        #endregion

        #region Read/Update Recenzije
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
                string obrisanaStr = recenzijaNode.SelectSingleNode("OBRISANA")?.InnerText;

                bool obrisana = false;
                if (obrisanaStr == "True")
                    obrisana = true;

                Recenzija nova = new Recenzija(int.Parse(id), int.Parse(idProizvoda), userKorisnik, naslov,
                        sadrzaj, slika, obrisana);
                ret.Add(int.Parse(id), nova);

            }

            return ret;
        }

        public static void UpdateRecenzijaXml(Recenzija update)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(recenzijePath);

            XmlNode recenzijaNode = xmlDoc.SelectSingleNode($"//recenzija[ID='{update.id}']");
            if (recenzijaNode != null)
                recenzijaNode.ParentNode.RemoveChild(recenzijaNode);

            XmlElement recenzijaXmlElement = xmlDoc.CreateElement("recenzija");

            XmlElement idElement = xmlDoc.CreateElement("ID");
            idElement.InnerText = update.id.ToString();
            recenzijaXmlElement.AppendChild(idElement);

            XmlElement naslovElement = xmlDoc.CreateElement("NASLOV");
            naslovElement.InnerText = update.naslov;
            recenzijaXmlElement.AppendChild(naslovElement);

            XmlElement sadrzajElement = xmlDoc.CreateElement("SADRZAJ");
            sadrzajElement.InnerText = update.sadrzajRecenzije;
            recenzijaXmlElement.AppendChild(sadrzajElement);

            XmlElement slikaElement = xmlDoc.CreateElement("SLIKA");
            slikaElement.InnerText = update.slika;
            recenzijaXmlElement.AppendChild(slikaElement);

            XmlElement idProizvodaElement = xmlDoc.CreateElement("ID_PROIZVOD");
            idProizvodaElement.InnerText = update.idProizvod.ToString();
            recenzijaXmlElement.AppendChild(idProizvodaElement);

            XmlElement userKorisnikElement = xmlDoc.CreateElement("USER_KORISNIK");
            userKorisnikElement.InnerText = update.userRecezent;
            recenzijaXmlElement.AppendChild(userKorisnikElement);

            XmlElement obrisanaElement = xmlDoc.CreateElement("OBRISANA");
            obrisanaElement.InnerText = update.obrisana.ToString();
            recenzijaXmlElement.AppendChild(obrisanaElement);

            xmlDoc.DocumentElement.AppendChild(recenzijaXmlElement);
            xmlDoc.Save(recenzijePath);
        }
        #endregion

        #region Pomocne metode
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
                string obrisanaStr = recenzijaNode.SelectSingleNode("OBRISANA")?.InnerText;

                bool obrisana = false;
                if (obrisanaStr == "True")
                    obrisana = true;

                if (int.Parse(idProizvoda) == idTrazenogProizvoda)
                {
                    Recenzija trazena = new Recenzija(int.Parse(id), int.Parse(idProizvoda), userKorisnik, naslov,
                        sadrzaj, slika, obrisana);
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
                string userProdavca = proizvodNode.SelectSingleNode("USER_PRODAVCA")?.InnerText;
                //RAZMISLITI DA LI OVO TREBA
                string obrisanStr = proizvodNode.SelectSingleNode("OBRISAN")?.InnerText;

                bool obrisan = false;
                if (obrisanStr == "True")
                    obrisan = true;

                bool dostupan = false;
                if (dostupanStr == "True")
                    dostupan = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);



                if (userProdavca == korisnickoImeTrazenog)
                {
                    List<Recenzija> recenzije = new List<Recenzija>();
                    recenzije = vratiRecenzije(int.Parse(id));
                    Proizvod objavljen = new Proizvod(int.Parse(id), naziv, double.Parse(cena), int.Parse(kolicina),
                        opis, slika, datum, grad, dostupan, userProdavca,recenzije,obrisan);
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
                string recenzijaOstavljenaStr = porudzbinaNode.SelectSingleNode("RECENZIJA_OSTAVLJENA")?.InnerText;
                string idRecenzije = porudzbinaNode.SelectSingleNode("ID_RECENZIJA")?.InnerText;

                bool recenzijaOstavljena = false;
                if (recenzijaOstavljenaStr == "True")
                    recenzijaOstavljena = true;

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
                        datum, status, recenzijaOstavljena, int.Parse(idRecenzije));
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
                //RAZMISLITI DA LI OVO TREBA
                string obrisanStr = proizvodNode.SelectSingleNode("OBRISAN")?.InnerText;

                bool obrisan = false;
                if (obrisanStr == "True")
                    obrisan = true;

                bool dostupan = false;
                if (dostupanStr == "True")
                    dostupan = true;

                DateTime datum;
                string format = "dd.MM.yyyy";
                DateTime.TryParseExact(datumStr, format, null, System.Globalization.DateTimeStyles.None, out datum);

                if (int.Parse(id) == trazeniId)
                {
                    List<Recenzija> recenzije = new List<Recenzija>();
                    recenzije = vratiRecenzije(int.Parse(id));
                    Proizvod omiljeni = new Proizvod(int.Parse(id), naziv, double.Parse(cena), int.Parse(kolicina),
                        opis, slika, datum, grad, dostupan, userProdavca, recenzije, obrisan);
                    return omiljeni;
                }

            }

            return null;
        }

        #endregion

        public static int GenerateID()
        {
            long timestamp = DateTime.UtcNow.Ticks;
            int random = new Random().Next();
            int uniqueId = (int)((timestamp & 0XFFFFFFFF) ^ random);
            return uniqueId;
        }
    }
}