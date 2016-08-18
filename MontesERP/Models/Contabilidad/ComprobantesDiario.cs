using MontesERP.Models.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                    Concepto = comprobante.Concepto,
                    FechaInicial = Convert.ToDateTime(comprobante.FechaInicial).ToShortDateString(),
                    FechaCierre = Convert.ToDateTime(comprobante.FechaCierre).ToShortDateString()
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
                    NumeroFolio = consultaComprobante.NumeroFolio,
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
                            Debe = detalle.Debe.ToString(),
                            Haber = detalle.Haber.ToString()
                        });
                    }
                }
            }
            else
            {
                listaElementoComprobante.Comprobante = new ComprobanteObj
                {
                    IdComprobante = 0,
                    NumeroFolio = 1,
                    Concepto = "",
                    FechaInicial = DateTime.Now.ToShortDateString()
                };
            }

            return listaElementoComprobante;
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
        public int NumeroFolio { get; set; }

        public string Concepto { get; set; }

        public string FechaInicial { get; set; }

        public string FechaCierre { get; set; }
    }
}