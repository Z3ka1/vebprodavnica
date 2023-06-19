using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using VebProdavnica.Models;

namespace VebProdavnica
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Ucitavanje iz datoteka
            Dictionary<string, Korisnik> korisnici = Data.ReadKorisnici();
            HttpContext.Current.Application["korisnici"] = korisnici;

            Dictionary<int, Proizvod> proizvodi = Data.ReadProizvodi();
            HttpContext.Current.Application["proizvodi"] = proizvodi;

            Dictionary<int, Porudzbina> porudzbine = Data.ReadPorudzbine();
            HttpContext.Current.Application["porudzbine"] = porudzbine;

            Dictionary<int, Recenzija> recenzije = Data.ReadRecenzije();
            HttpContext.Current.Application["recenzije"] = recenzije;
        }
    }
}
