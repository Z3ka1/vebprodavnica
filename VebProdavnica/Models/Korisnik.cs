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

        public bool obrisan { get; set; }

        public List<Porudzbina> listaPorudzbina { get; set; } //Uloga kupac
        public List<Proizvod> listaOmiljenihProizvoda { get; set; } //Uloga kupac
        public List<Proizvod> listaObjavljenihProizvoda { get; set; }   //Uloga prodavac

        //za read iz xml
        public Korisnik(string korisnickoIme, string lozinka, string ime, string prezime, Pol pol, 
            string email, DateTime datumRodjenja, Uloga uloga, List<Porudzbina> porudzbine, 
            List<Proizvod> omiljeni, List<Proizvod> objavljeni, bool obrisan)
        {
            this.korisnickoIme = korisnickoIme;
            this.lozinka = lozinka;
            this.ime = ime;
            this.prezime = prezime;
            this.pol = pol;
            this.email = email;
            this.datumRodjenja = datumRodjenja;
            this.uloga = uloga;
            this.obrisan = obrisan;

            listaPorudzbina = porudzbine;
            listaOmiljenihProizvoda = omiljeni;
            listaObjavljenihProizvoda = objavljeni;
        }

        //Za registraciju
        public Korisnik(string ime, string prezime, Pol pol, string email, DateTime datumRodjenja, Uloga uloga,
            string korisnickoIme, string lozinka)
        {
            this.ime = ime;
            this.prezime = prezime;
            this.pol = pol;
            this.email = email;
            this.datumRodjenja = datumRodjenja;
            this.uloga = uloga;
            this.korisnickoIme = korisnickoIme;
            this.lozinka = lozinka;
            obrisan = false;

            listaPorudzbina = new List<Porudzbina>();
            listaOmiljenihProizvoda = new List<Proizvod>();
            listaObjavljenihProizvoda = new List<Proizvod>();
        }
        
    }
}