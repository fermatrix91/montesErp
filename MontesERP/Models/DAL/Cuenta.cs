//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MontesERP.Models.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Cuenta
    {
        public Cuenta()
        {
            this.SubCuentas = new HashSet<SubCuenta>();
        }
    
        public int IdCuenta { get; set; }
        public int IdSubGrupoCuenta { get; set; }
        public string NombreCuenta { get; set; }
        public Nullable<decimal> debeAnterior { get; set; }
        public Nullable<decimal> haberAnterior { get; set; }
        public Nullable<decimal> debeActual { get; set; }
        public Nullable<decimal> haberActual { get; set; }
    
        public virtual SubGrupoCuenta SubGrupoCuenta { get; set; }
        public virtual ICollection<SubCuenta> SubCuentas { get; set; }
    }
}
