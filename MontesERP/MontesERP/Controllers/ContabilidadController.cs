using MontesERP.Models.Contabilidad;
using MontesERP.Models.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MontesERP.Controllers
{
    public class ContabilidadController : Controller
    {
        // GET: Contabilidad
        public readonly MontesERPEntities modeloErp = new MontesERPEntities();
        public readonly Cuentas cuentas = new Cuentas();
        public readonly ComprobantesDiario comprobantes = new ComprobantesDiario();

        public List<DetalleComprobanteDeDiario> ListaInicialDetalleComprobante { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        /******************************************************************************/
        /************************************Cuentas***********************************/
        /******************************************************************************/
        [Authorize]
        public ActionResult CatalogoDeCuentas()
        {
            return View("CatalogoDeCuentas", cuentas.ListadoDeCuentas());
        }

        [Authorize]
        public ActionResult SubCuentas(int idCuenta)
        {
            return View("SubCuentas", cuentas.ListadoDeSubCuentas(idCuenta));
        }

        [Authorize]
        public ActionResult DetalleSubCuenta(int idSubCuenta)
        {
            return Json(new
            {
                DetalleDeSubCuenta = cuentas.DetalleDeSubCuenta(idSubCuenta),
                ListaCuentas = cuentas.ListadoDeCuentas()
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult IdSubCuentaActual(int idCuenta)
        {
            if (idCuenta != 0)
            {
                List<int> haySubCuentas = (from subcuenta in modeloErp.SubCuenta
                                           where subcuenta.IdCuenta == idCuenta
                                           select subcuenta.IdSubCuenta).ToList();

                return haySubCuentas.Count == 0 ? Json(idCuenta + "001") : Json(haySubCuentas.LastOrDefault() + 1);
            }
            return Json(0);
        }

        [Authorize]
        public ActionResult GuardarSubCuenta(int idCuenta, int idSubCuenta, string nombreCuenta)
        {
            SubCuenta subCuenta = new SubCuenta();

            if (modeloErp.SubCuenta.Find(idSubCuenta) != null)
            {
                subCuenta = modeloErp.SubCuenta.Find(idSubCuenta);
                subCuenta.NombreSubCuenta = nombreCuenta;

                modeloErp.SubCuenta.Attach(subCuenta);
                modeloErp.Entry(subCuenta).State = EntityState.Modified;
            }
            else
            {
                subCuenta = new SubCuenta();
                subCuenta.IdCuenta = idCuenta;
                subCuenta.IdSubCuenta = idSubCuenta;
                subCuenta.NombreSubCuenta = nombreCuenta;
                subCuenta.saldo = 0;

                modeloErp.SubCuenta.Add(subCuenta);
                modeloErp.Entry(subCuenta).State = EntityState.Added;
            }

            modeloErp.SaveChanges();

            return RedirectToAction("SubCuentas", new { idCuenta = idCuenta });
        }

        /******************************************************************************/
        /*****************************Comprobantes de Diario***************************/
        /******************************************************************************/

        [Authorize]
        public ActionResult ComprobantesDeDiario()
        {
            return View("ComprobantesDeDiario", comprobantes.ListadoDeComprobantes());
        }

        [Authorize]
        public ActionResult DetalleComprobante(int idComprobante)
        {
            ComprobantesDiario comprobante = new ComprobantesDiario();

            return Json(new
            {
                ListaSubCuentas = cuentas.ListadoTotalDeSubCuentas(),
                DetalleComprobante = comprobante.DetalleDeComprobante(idComprobante)
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult GuardarComprobante(int idComprobante, string movimientos, string conceptoActual, string fechaActual, int numeroDeComprobante)
        {
            bool guardadoDeComprobante = comprobantes.GuardadoDeComprobante(idComprobante, movimientos, conceptoActual, fechaActual, numeroDeComprobante);

            return RedirectToAction("ComprobantesDeDiario", "Contabilidad");
        }

        [Authorize]
        public JsonResult ActualizaNumeroComprobante(string fechaActual)
        {
            string numeroComprobante;
            DateTime fechaResultante;

            if (DateTime.TryParse(fechaActual, out fechaResultante))
            {
                try
                {
                    var consultaComprobantes = (from comprobante in modeloErp.ComprobanteDeDiario
                                                where comprobante.FechaInicial.Month == fechaResultante.Month
                                                select comprobante.NumeroComprobante).Max();

                    return Json(new { NumeroComprobante = consultaComprobantes + 1, JsonRequestBehavior.AllowGet });
                }
                catch (Exception mensaje)
                {
                    return Json(new { NumeroComprobante = "1", JsonRequestBehavior.AllowGet });
                }
            }
            return Json(new { NumeroComprobante = "1", JsonRequestBehavior.AllowGet });
        }
    }

    public enum TipoComprobante
    {
        ComprobanteDiario,
        Factura
    }

    public enum TipoSaldo
    {
        Deudor,
        Acreedor,
        Saldado
    }
}