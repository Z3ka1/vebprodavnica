using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VebProdavnica.Models
{
    public enum StatusRecenzije { CEKA, ODOBRENA, ODBIJENA};
    
    public class Recenzija
    {
        public int id { get; set; }
        public Proizvod proizvod { get; set; }
        public Korisnik recezent { get; set; }

        public int idProizvod { get; set; }
        public string userRecezent { get; set; }
        public string naslov { get; set; }
        public string sadrzajRecenzije { get; set; }
        public string slika { get; set; }
        public bool obrisana { get; set; }
        public StatusRecenzije status { get; set; }

        public Recenzija(int id, int idProizvod, string userRecezent, string naslov, string sadrzajRecenzije, string slika, bool obrisana, StatusRecenzije status)
        {
            this.id = id;
            this.idProizvod = idProizvod;
            this.userRecezent = userRecezent;
            this.naslov = naslov;
            this.sadrzajRecenzije = sadrzajRecenzije;
            this.slika = slika;
            this.obrisana = obrisana;
            this.status = status;
        }

    }
}