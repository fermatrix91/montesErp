using MontesERP.Models.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace MontesERP.Models.Contabilidad
{
    public class ComprobantesDiario
    {
        public readonly MontesERPEntities modeloErp = new MontesERPEntities();

        public List<ComprobanteObj> ListadoDeComprobantes()
        {
            List<ComprobanteObj> listaDeComprobantes = new List<ComprobanteObj>();

            var consultaComprobantes = (from comprobante in modeloErp.ComprobanteDeDiarios
                                        select comprobante).ToList();

            foreach (ComprobanteDeDiario comprobante in consultaComprobantes)
            {
                listaDeComprobantes.Add(new ComprobanteObj
                {
                    IdComprobante = comprobante.IdComprobanteDeDiario,
                    NumeroComprobante = comprobante.NumeroComprobante,
                    Concepto = comprobante.Concepto,
                    FechaInicial = Convert.ToDateTime(comprobante.FechaInicial).ToShortDateString(),
                    FechaCierre = comprobante.FechaCierre != null ? (Convert.ToDateTime(comprobante.FechaCierre).ToShortDateString()) : null
                });
            }

            return listaDeComprobantes;
        }

        public DetalleDeComprobante DetalleDeComprobante(int idComprobante)
        {
            DetalleDeComprobante listaElementoComprobante = new DetalleDeComprobante();

            var consultaComprobante = (from comprobant in modeloErp.ComprobanteDeDiarios
                                       where comprobant.IdComprobanteDeDiario == idComprobante
                                       select comprobant).FirstOrDefault();

            if (consultaComprobante != null)
            {
                listaElementoComprobante.Comprobante = new ComprobanteObj
                {
                    IdComprobante = consultaComprobante.IdComprobanteDeDiario,
                    NumeroComprobante = consultaComprobante.NumeroComprobante,
                    Concepto = consultaComprobante.Concepto,
                    FechaInicial = Convert.ToDateTime(consultaComprobante.FechaInicial).ToShortDateString()
                };

                var consultaDetalle = (from comprobant in modeloErp.DetalleComprobanteDeDiarios
                                       where comprobant.IdComprobanteDeDiario == idComprobante
                                       select comprobant).ToList();

                if (consultaDetalle.Count != 0)
                {
                    listaElementoComprobante.DetalleComprobante = new List<ElementoComprobante>();
                    foreach (DetalleComprobanteDeDiario detalle in consultaDetalle)
                    {
                        listaElementoComprobante.DetalleComprobante.Add(new ElementoComprobante
                        {
                            IdSubCuenta = detalle.IdSubCuenta.ToString(),
                            NombreSubCuenta = detalle.SubCuenta.NombreSubCuenta,
                            Debe = String.Format("{0:0.00}", detalle.Debe).ToString(),
                            Haber = String.Format("{0:0.00}", detalle.Haber).ToString()
                        });
                    }
                }
            }
            else
            {
                listaElementoComprobante.Comprobante = new ComprobanteObj
                {
                    IdComprobante = 0,
                    NumeroComprobante = 0,
                    Concepto = "",
                    FechaInicial = DateTime.Now.ToShortDateString()
                };
            }

            return listaElementoComprobante;
        }

        public bool GuardadoDeComprobante(int idComprobante, string movimientos, string conceptoActual, string fechaActual, int numeroDeComprobante)
        {
            decimal sumaDebe = 0;
            decimal sumaHaber = 0;
            var listaJSON = new JavaScriptSerializer();
            List<ElementoComprobante> listaElementoComprobante = listaJSON.Deserialize<List<ElementoComprobante>>(movimientos);
            List<DetalleComprobanteDeDiario> listaElementoComprobanteInicial = (from comprobanteDetalle in modeloErp.DetalleComprobanteDeDiarios
                                                                                where comprobanteDetalle.IdComprobanteDeDiario == idComprobante
                                                                                select comprobanteDetalle).ToList();

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
                        comprobanteDiario.NumeroComprobante = numeroDeComprobante;
                        modeloErp.ComprobanteDeDiarios.Attach(comprobanteDiario);
                        modeloErp.Entry(comprobanteDiario).State = EntityState.Modified;
                        modeloErp.SaveChanges();
                    }
                    else
                    {
                        comprobanteDiario.FechaInicial = fechaActualTemporal.Date;
                        comprobanteDiario.Concepto = conceptoActual;
                        comprobanteDiario.NumeroComprobante = numeroDeComprobante;
                        modeloErp.ComprobanteDeDiarios.Add(comprobanteDiario);
                        modeloErp.Entry(comprobanteDiario).State = EntityState.Added;
                        modeloErp.SaveChanges();

                        idComprobante = comprobanteDiario.IdComprobanteDeDiario;
                    }


                    //////Revertir saldo de subcuentas
                    foreach (var detalle in listaElementoComprobanteInicial)//this.ListaInicialDetalleComprobante)
                    {
                        int idSubCuenta = Convert.ToInt32(detalle.IdSubCuenta);

                        SubCuenta subCuenta = modeloErp.SubCuentas.Find(idSubCuenta);
                        subCuenta.debeAnterior = subCuenta.debeActual;
                        subCuenta.haberAnterior = subCuenta.haberActual;

                        subCuenta.debeActual = (subCuenta.debeActual != null ? subCuenta.debeActual : 0) - Convert.ToDecimal(detalle.Debe);
                        subCuenta.haberActual = (subCuenta.haberActual != null ? subCuenta.debeActual : 0) - Convert.ToDecimal(detalle.Haber);

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
                    foreach (var detalle in listaElementoComprobanteInicial)
                    {
                        modeloErp.DetalleComprobanteDeDiarios.Attach(detalle);
                        modeloErp.Entry(detalle).State = EntityState.Deleted;
                        modeloErp.SaveChanges();
                    }


                    ////Guardar detalle Comprobante de Diario
                    foreach (var detalle in listaElementoComprobante)
                    {
                        DetalleComprobanteDeDiario detalleComprobante = new DetalleComprobanteDeDiario();
                        detalleComprobante.Debe = (!string.IsNullOrEmpty(detalle.Debe)) ? Convert.ToDecimal(detalle.Debe) : 0;
                        detalleComprobante.Haber = (!string.IsNullOrEmpty(detalle.Haber)) ? Convert.ToDecimal(detalle.Haber) : 0;
                        detalleComprobante.IdComprobanteDeDiario = idComprobante;
                        detalleComprobante.IdSubCuenta = Convert.ToInt32(detalle.IdSubCuenta);

                        SubCuenta subCuenta = modeloErp.SubCuentas.Find(detalleComprobante.IdSubCuenta);
                        subCuenta.debeAnterior = subCuenta.debeActual;
                        subCuenta.haberAnterior = subCuenta.haberActual;        //Asignar debe y haber anterior


                        ///Acumular debe y haber
                        subCuenta.debeActual = (subCuenta.debeActual != null ? subCuenta.debeActual : 0) + detalleComprobante.Debe;
                        subCuenta.haberActual = (subCuenta.haberActual != null ? subCuenta.haberActual : 0) + detalleComprobante.Haber;

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
                    return true;

                }
                catch (Exception mensaje)
                {
                    return false;
                    //MessageBox.Show("Error Inesperado en el Server");
                }
            }

            return false;
        }
    }

    public class DetalleDeComprobante
    {
        public ComprobanteObj Comprobante { get; set; }
        public List<ElementoComprobante> DetalleComprobante { get; set; }
    }

    public class ElementoComprobante
    {
        public string IdSubCuenta { get; set; }

        public string NombreSubCuenta { get; set; }

        public string Debe { get; set; }

        public string Haber { get; set; }
    }

    public class ComprobanteObj
    {
        public int IdComprobante { get; set; }
        public int NumeroComprobante { get; set; }

        public string Concepto { get; set; }

        public string FechaInicial { get; set; }

        public string FechaCierre { get; set; }
    }
}