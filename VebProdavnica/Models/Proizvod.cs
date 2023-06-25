using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Web;

namespace VebProdavnica.Models
{
    public class Proizvod
    {
        public int id { get; set; }
        public string naziv { get; set; }
        public double cena { get; set; }
        public int kolicina { get; set; }
        public string opis { get; set; }
        public string slika { get; set; }
        public DateTime datumPostavljanja { get; set; }
        public string grad { get; set; }
        public List<Recenzija> listaRecenzija { get; set; }
        public bool dostupan { get; set; }
        public string userProdavca { get; set; }
        public bool obrisan { get; set; }

        public Proizvod(int id, string naziv, double cena, int kolicina, string opis, string slika, 
            DateTime datumPostavljanja, string grad, bool dostupan, string userProdavca, List<Recenzija> recenzije,
            bool obrisan)
        {
            this.id = id;
            this.naziv = naziv;
            this.cena = cena;
            this.kolicina = kolicina;
            this.opis = opis;
            this.slika = slika;
            this.datumPostavljanja = datumPostavljanja;
            this.grad = grad;
            this.dostupan = dostupan;
            this.listaRecenzija = recenzije;
            this.userProdavca = userProdavca;
            this.obrisan = obrisan;
        }

        //Za dodavanje novog
        public Proizvod(string naziv, double cena, int kolicina, string opis, string slika,
            string grad, bool dostupan, string userProdavca)
        {
            this.id = Data.GenerateID();
            this.naziv = naziv;
            this.cena = cena;
            this.kolicina = kolicina;
            this.opis = opis;
            this.slika = slika;
            this.datumPostavljanja = DateTime.Now;
            this.grad = grad;
            this.dostupan = dostupan;
            this.listaRecenzija = new List<Recenzija>();
            this.userProdavca = userProdavca;
            this.obrisan = false;
        }

        public Proizvod()
        {

        }

    }
}