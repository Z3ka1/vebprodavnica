using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VebProdavnica.Models
{
    public enum Pol {M, Z};
    public enum Uloga { Kupac, Prodavac, Administrator };

    public class Korisnik
    {

        public string korisnickoIme { get; set; }
        public string lozinka { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public Pol pol { get; set; }
        public string email { get; set; }
        public DateTime datumRodjenja { get; set; }
        public Uloga uloga { get; set; }

        public List<Porudzbina> listaPorudzbina { get; set; } //Uloga kupac
        public List<Proizvod> listaOmiljenihProizvoda { get; set; } //Uloga kupac
        public List<Proizvod> listaObjavljenihProizvoda { get; set; }   //Uloga prodavac

        public Korisnik(string korisnickoIme, string lozinka, string ime, string prezime, Pol pol, 
            string email, DateTime datumRodjenja, Uloga uloga, List<Porudzbina> porudzbine, 
            List<Proizvod> omiljeni, List<Proizvod> objavljeni)
        {
            this.korisnickoIme = korisnickoIme;
            this.lozinka = lozinka;
            this.ime = ime;
            this.prezime = prezime;
            this.pol = pol;
            this.email = email;
            this.datumRodjenja = datumRodjenja;
            this.uloga = uloga;

            listaPorudzbina = porudzbine;
            listaOmiljenihProizvoda = omiljeni;
            listaObjavljenihProizvoda = objavljeni;
        }

        
    }
}