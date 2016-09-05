using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MontesERP.Models.DAL;

namespace MontesERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly MontesERPEntities modeloMontesErp = new MontesERPEntities();

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.PrimerProducto = modeloMontesErp.Productoes.FirstOrDefault().NombreProducto;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}