using MontesERP.Models.Contabilidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MontesERP.Models.DAL;
using System.Web.Script.Serialization;

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
                List<int> haySubCuentas = (from subcuenta in modeloErp.SubCuentas
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

            if (modeloErp.SubCuentas.Find(idSubCuenta) != null)
            {
                subCuenta = modeloErp.SubCuentas.Find(idSubCuenta);
                subCuenta.NombreSubCuenta = nombreCuenta;

                modeloErp.SubCuentas.Attach(subCuenta);
                modeloErp.Entry(subCuenta).State = EntityState.Modified;
            }
            else
            {
                subCuenta = new SubCuenta();
                subCuenta.IdCuenta = idCuenta;
                subCuenta.IdSubCuenta = idSubCuenta;
                subCuenta.NombreSubCuenta = nombreCuenta;
                subCuenta.saldo = 0;

                modeloErp.SubCuentas.Add(subCuenta);
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
        public ActionResult GuardarComprobante(int idComprobante, string movimientos, string conceptoActual, string fechaActual)
        {
            decimal sumaDebe = 0;
            decimal sumaHaber = 0;

            var listaJSON = new JavaScriptSerializer();
            List<ElementoComprobante> listaElementoComprobante = listaJSON.Deserialize<List<ElementoComprobante>>(movimientos);

            foreach (var detalle in listaElementoComprobante)
            {
                sumaDebe += string.IsNullOrWhiteSpace(detalle.Debe) ? 0 : Convert.ToDecimal(detalle.Debe);
                sumaHaber += string.IsNullOrWhiteSpace(detalle.Haber) ? 0 : Convert.ToDecimal(detalle.Haber);
            }


            DateTime fechaActualTemporal;

            if (sumaDebe == sumaHaber && (sumaDebe != 0 || sumaDebe != 0)
                && !string.IsNullOrWhiteSpace(conceptoActual) && DateTime.TryParse(fechaActual, out fechaActualTemporal))
            {
                try
                {
                    ////Guarda datos De Comprobante de Diario
                    ComprobanteDeDiario comprobanteDiario = new ComprobanteDeDiario();

                    if (idComprobante != 0)
                    {
                        comprobanteDiario = modeloErp.ComprobanteDeDiarios.Find(idComprobante);
                        comprobanteDiario.FechaInicial = fechaActualTemporal.Date;
                        comprobanteDiario.Concepto = conceptoActual;
                        modeloErp.ComprobanteDeDiarios.Attach(comprobanteDiario);
                        modeloErp.Entry(comprobanteDiario).State = EntityState.Modified;
                        modeloErp.SaveChanges();
                    }
                    else
                    {
                        comprobanteDiario.FechaInicial = fechaActualTemporal.Date;
                        comprobanteDiario.Concepto = conceptoActual;
                        modeloErp.ComprobanteDeDiarios.Add(comprobanteDiario);
                        modeloErp.Entry(comprobanteDiario).State = EntityState.Added;
                        modeloErp.SaveChanges();

                        idComprobante = (from comprobante in modeloErp.ComprobanteDeDiarios
                                         select comprobanteDiario.IdComprobanteDeDiario).Max();
                    }


                    //////Revertir saldo de subcuentas
                    foreach (var detalle in listaElementoComprobante)//this.ListaInicialDetalleComprobante)
                    {
                        int idSubCuenta = Convert.ToInt32(detalle.IdSubCuenta);

                        SubCuenta subCuenta = modeloErp.SubCuentas.Find(idSubCuenta);
                        subCuenta.debeAnterior = subCuenta.debeActual;
                        subCuenta.haberAnterior = subCuenta.haberActual;

                        subCuenta.debeActual = subCuenta.debeActual - Convert.ToDecimal(detalle.Debe);
                        subCuenta.haberActual = subCuenta.haberActual - Convert.ToDecimal(detalle.Haber);

                        //Saldo
                        switch (subCuenta.Cuenta.SubGrupoCuenta.GrupoCuenta.IdGrupoCuenta)
                        {
                            case 1:
                                subCuenta.saldo = subCuenta.debeActual - subCuenta.haberActual;
                                break;

                            case 2:
                                subCuenta.saldo = subCuenta.haberActual - subCuenta.debeActual;
                                break;

                            case 3:
                                subCuenta.saldo = subCuenta.haberActual - subCuenta.debeActual;
                                break;

                            case 4:
                                subCuenta.saldo = subCuenta.haberActual - subCuenta.debeActual;
                                break;

                            case 5:
                                subCuenta.saldo = subCuenta.debeActual - subCuenta.haberActual;
                                break;

                            default:
                                break;
                        }

                        modeloErp.SubCuentas.Attach(subCuenta);
                        modeloErp.Entry(subCuenta).State = EntityState.Modified;
                        modeloErp.SaveChanges();
                    }

                    ///Eliminar detalle Anterior
                    ///
                    foreach (var detalle in this.ListaInicialDetalleComprobante)
                    {
                        modeloErp.DetalleComprobanteDeDiarios.Attach(detalle);
                        modeloErp.Entry(detalle).State = EntityState.Deleted;
                        modeloErp.SaveChanges();
                    }


                    ////Guardar detalle Comprobante de Diario
                    foreach (var detalle in listaElementoComprobante)
                    {
                        DetalleComprobanteDeDiario detalleComprobante = new DetalleComprobanteDeDiario();
                        detalleComprobante.Debe = Convert.ToDecimal(detalle.Debe);
                        detalleComprobante.Haber = Convert.ToDecimal(detalle.Haber);
                        detalleComprobante.IdComprobanteDeDiario = idComprobante;
                        detalleComprobante.IdSubCuenta = Convert.ToInt32(detalle.IdSubCuenta);

                        SubCuenta subCuenta = modeloErp.SubCuentas.Find(detalleComprobante.IdSubCuenta);
                        subCuenta.debeAnterior = subCuenta.debeActual;
                        subCuenta.haberAnterior = subCuenta.haberActual;        //Asignar debe y haber anterior


                        ///Acumular debe y haber
                        subCuenta.debeActual = subCuenta.debeActual + detalleComprobante.Debe;
                        subCuenta.haberActual = subCuenta.haberActual + detalleComprobante.Haber;

                        //Saldo
                        switch (subCuenta.Cuenta.SubGrupoCuenta.GrupoCuenta.IdGrupoCuenta)
                        {
                            case 1:
                                subCuenta.saldo = subCuenta.debeActual - subCuenta.haberActual;
                                break;

                            case 2:
                                subCuenta.saldo = subCuenta.haberActual - subCuenta.debeActual;
                                break;

                            case 3:
                                subCuenta.saldo = subCuenta.haberActual - subCuenta.debeActual;
                                break;

                            case 4:
                                subCuenta.saldo = subCuenta.haberActual - subCuenta.debeActual;
                                break;

                            case 5:
                                subCuenta.saldo = subCuenta.debeActual - subCuenta.haberActual;
                                break;

                            default:
                                break;
                        }

                        modeloErp.DetalleComprobanteDeDiarios.Add(detalleComprobante); // Agrega movimientos
                        modeloErp.Entry(subCuenta).State = EntityState.Added;

                        modeloErp.SubCuentas.Attach(subCuenta);        //Actualiza cuenta
                        modeloErp.Entry(subCuenta).State = EntityState.Modified;

                        modeloErp.SaveChanges();
                    }
                    //MessageBox.Show("Operación realizada con éxito");

                }
                catch (Exception mensaje)
                {
                    //MessageBox.Show("Error Inesperado en el Server");
                }
            }
            //else
            //{
            //    MessageBox.Show("No Cuadra");
            //}

            return RedirectToAction("ComprobantesDeDiario", "Contabilidad");
        }

    }
}