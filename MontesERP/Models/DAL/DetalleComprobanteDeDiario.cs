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
    
    public partial class DetalleComprobanteDeDiario
    {
        public int IdDetalleComprobanteDeDiario { get; set; }
        public int IdComprobanteDeDiario { get; set; }
        public int IdSubCuenta { get; set; }
        public Nullable<decimal> Debe { get; set; }
        public Nullable<decimal> Haber { get; set; }
    
        public virtual SubCuenta SubCuenta { get; set; }
        public virtual ComprobanteDeDiario ComprobanteDeDiario { get; set; }
    }
}
