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
    
    public partial class ComprobanteDeDiario
    {
        public ComprobanteDeDiario()
        {
            this.DetalleComprobanteDeDiarios = new HashSet<DetalleComprobanteDeDiario>();
        }
    
        public int IdComprobanteDeDiario { get; set; }
        public int NumeroComprobante { get; set; }
        public string Concepto { get; set; }
        public System.DateTime FechaInicial { get; set; }
        public Nullable<System.DateTime> FechaCierre { get; set; }
    
        public virtual ICollection<DetalleComprobanteDeDiario> DetalleComprobanteDeDiarios { get; set; }
    }
}
