using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VebProdavnica.Models
{
    public enum Status { AKTIVNA, IZVRSENA, OTKAZANA}
    public class Porudzbina
    {
        public int id { get; set; }
        public Proizvod proizvod { get; set; }
        public int idProizvod { get; set; }
        public int kolicina { get; set; }
        public Korisnik kupac { get; set; }
        public string userKupac { get; set; }
        public DateTime datumPorudzbine { get; set; }
        public Status status { get; set; }
        public bool recenzijaOstavljena { get; set; }
        public int idRecenzije { get; set; }

        public Porudzbina(int id, int idProizvod, int kolicina, string userKupac, DateTime datumPorudzbine, Status status, bool recenzijaOstavljena, int idRecenzije)
        {
            this.id = id;
            this.idProizvod = idProizvod;
            this.kolicina = kolicina;
            this.userKupac = userKupac;
            this.datumPorudzbine = datumPorudzbine;
            this.status = status;
            this.recenzijaOstavljena = recenzijaOstavljena;
            this.idRecenzije = idRecenzije;
        }


    }
}