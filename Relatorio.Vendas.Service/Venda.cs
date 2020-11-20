using System;
using System.Collections.Generic;
using System.Text;

namespace Relatorio.Vendas.Service
{
    public class Venda
    {
        public int Id { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public string SalesmanName { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class Item
    {
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
        public decimal Price { get; set; }

    }
}
