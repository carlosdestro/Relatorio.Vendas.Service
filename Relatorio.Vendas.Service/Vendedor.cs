using System;
using System.Collections.Generic;
using System.Text;

namespace Relatorio.Vendas.Service
{
    public class Vendedor
    {
        public string CPF { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public decimal TotalVendas { get; set; }
        public virtual ICollection<Venda> Vendas { get; set; }
    }
}
