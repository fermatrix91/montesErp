using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MontesERP.Models.DAL;

namespace MontesERP.Models.Contabilidad
{
    public class Cuentas
    {
        public readonly MontesERPEntities modeloErp = new MontesERPEntities();

        public List<CuentaObj> ListadoDeCuentas()
        {
            List<CuentaObj> listaDeCuentas = new List<CuentaObj>();

            var consultaCuentas = (from cuenta in modeloErp.Cuentas
                                   select cuenta).ToList();

            foreach (Cuenta cuenta in consultaCuentas)
            {
                listaDeCuentas.Add(new CuentaObj
                {
                    Grupo = cuenta.SubGrupoCuenta.GrupoCuenta.NombreGrupoCuenta,
                    SubGrupo = cuenta.SubGrupoCuenta.NombreSubGrupoCuenta,
                    IdCuenta = cuenta.IdCuenta,
                    Cuenta = cuenta.NombreCuenta
                });
            }

            return listaDeCuentas;
        }

        public List<SubCuentaObj> ListadoDeSubCuentas(int idCuenta)
        {
            List<SubCuentaObj> listaDeSubCuentas = new List<SubCuentaObj>();

            var consultaCuentas = (from subCuenta in modeloErp.SubCuentas
                                   where subCuenta.IdCuenta == idCuenta
                                   select subCuenta).ToList();

            foreach (SubCuenta cuenta in consultaCuentas)
            {
                listaDeSubCuentas.Add(new SubCuentaObj
                {
                    IdSubCuenta = cuenta.IdSubCuenta,
                    SubCuenta = cuenta.NombreSubCuenta
                });
            }

            return listaDeSubCuentas;
        }

        public List<SubCuentaObj> ListadoTotalDeSubCuentas()
        {
            List<SubCuentaObj> listaDeSubCuentas = new List<SubCuentaObj>();

            var consultaCuentas = (from subCuenta in modeloErp.SubCuentas
                                   select subCuenta).ToList();

            foreach (SubCuenta cuenta in consultaCuentas)
            {
                listaDeSubCuentas.Add(new SubCuentaObj
                {
                    IdSubCuenta = cuenta.IdSubCuenta,
                    SubCuenta = cuenta.NombreSubCuenta
                });
            }

            return listaDeSubCuentas;
        }

        public ArrayList DetalleDeSubCuenta(int idSubCuenta)
        {
            ArrayList subCuenta = new ArrayList();

            var consultaSubCuenta = (from subCuent in modeloErp.SubCuentas
                                     where subCuent.IdSubCuenta == idSubCuenta
                                     select subCuent).FirstOrDefault();

            if (consultaSubCuenta != null)
                subCuenta.Add(new
                {
                    consultaSubCuenta.IdSubCuenta,
                    consultaSubCuenta.IdCuenta,
                    consultaSubCuenta.NombreSubCuenta
                });

            return subCuenta;
        }
    }

    public class CuentaObj
    {
        public string Grupo { get; set; }

        public string SubGrupo { get; set; }

        public int IdCuenta { get; set; }

        public string Cuenta { get; set; }
    }

    public class SubCuentaObj
    {
        public int IdSubCuenta { get; set; }

        public string SubCuenta { get; set; }
    }
}