using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VebProdavnica.Models
{
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

        public Recenzija(int id, int idProizvod, string userRecezent, string naslov, string sadrzajRecenzije, string slika)
        {
            this.id = id;
            this.idProizvod = idProizvod;
            this.userRecezent = userRecezent;
            this.naslov = naslov;
            this.sadrzajRecenzije = sadrzajRecenzije;
            this.slika = slika;
        }

    }
}