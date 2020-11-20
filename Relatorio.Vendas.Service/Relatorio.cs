using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Relatorio.Vendas.Service
{
    class Relatorio
    {
        public static void LerArquivo(string arquivoCaminho)
        {
            try
            {
                string homePath = Directory.GetCurrentDirectory();

                string diretorioSaida = $"{homePath}/data/out/";

                string conteudoArquivoOrigem = File.ReadAllText(arquivoCaminho);

                string caminhoArquivoSaida = $"{diretorioSaida}/{arquivoCaminho.Split(new string[] { "/data/in\\" }, StringSplitOptions.None)[1]}";

                string nomeArquivoSaida = Path.GetFileNameWithoutExtension(caminhoArquivoSaida);

                string relatorio = GerarRelatorio(conteudoArquivoOrigem);

                if (File.Exists(caminhoArquivoSaida))
                {
                    string conteudoArquivoSaida = File.ReadAllText(caminhoArquivoSaida);

                    if (conteudoArquivoSaida != relatorio)
                    {
                        nomeArquivoSaida = $"{diretorioSaida}/{nomeArquivoSaida}-{DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss")}.txt";

                        SalvarRelatorio(nomeArquivoSaida, relatorio);
                    }
                }
                else
                {
                    nomeArquivoSaida = $"{diretorioSaida}/{nomeArquivoSaida}.txt";

                    SalvarRelatorio(nomeArquivoSaida, relatorio);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"Algo deu errado: {e.Message}");
            }
        }

        private static string GerarRelatorio(string conteudo)
        {
            string[] linhas = conteudo.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            List<Vendedor> vendedores = new List<Vendedor>();

            List<Cliente> clientes = new List<Cliente>();

            List<Venda> vendas = new List<Venda>();

            int tipoId;

            foreach (var linha in linhas)
            {
                bool success = Int32.TryParse(linha.Substring(0, 3), out tipoId);

                if (success)
                {
                    switch (tipoId)
                    {
                        case 001:

                            decimal salary;

                            bool salarySuccess = decimal.TryParse(linha.Split(new[] { "ç" }, StringSplitOptions.None)[3], out salary);

                            if (salarySuccess)
                            {
                                Vendedor vendedor = new Vendedor
                                {
                                    CPF = linha.Split(new[] { "ç" }, StringSplitOptions.None)[1],
                                    Name = linha.Split(new[] { "ç" }, StringSplitOptions.None)[2],
                                    Salary = salary
                                };

                                bool existeCPF = vendedores.Exists(x => x.CPF == vendedor.CPF);

                                if (!existeCPF)
                                    vendedores.Add(vendedor);
                            }

                            break;

                        case 002:
                            Cliente cliente = new Cliente
                            {
                                CNPJ = linha.Split(new[] { "ç" }, StringSplitOptions.None)[1],
                                Name = linha.Split(new[] { "ç" }, StringSplitOptions.None)[2],
                                BusinessArea = linha.Split(new[] { "ç" }, StringSplitOptions.None)[3]
                            };

                            bool existeCNPJ = clientes.Exists(x => x.CNPJ == cliente.CNPJ);

                            if (!existeCNPJ)
                                clientes.Add(cliente);

                            break;

                        case 003:

                            char[] remover = { '[', ']' };

                            string itemsVendaString = linha.Split(new[] { "ç" }, StringSplitOptions.None)[2].Trim(remover);

                            string[] itemsVenda = itemsVendaString.Split(new[] { "," }, StringSplitOptions.None);

                            List<Item> items = new List<Item>();

                            decimal valorTotal = 0;

                            foreach (var itemVenda in itemsVenda)
                            {
                                int itemId;

                                bool itemIdSuccess = Int32.TryParse(linha.Split(new[] { "-" }, StringSplitOptions.None)[0], out itemId);

                                int itemQuantity;

                                bool itemQuantitySuccess = Int32.TryParse(linha.Split(new[] { "-" }, StringSplitOptions.None)[1], out itemQuantity);

                                int itemPrice;

                                bool itemPriceSuccess = Int32.TryParse(linha.Split(new[] { "-" }, StringSplitOptions.None)[2], out itemPrice);

                                if (itemIdSuccess && itemQuantitySuccess && itemPriceSuccess)
                                {
                                    Item item = new Item
                                    {
                                        ItemId = itemId,
                                        ItemQuantity = itemQuantity,
                                        Price = itemPrice
                                    };

                                    valorTotal += item.Price;
                                }

                            }

                            int id;

                            bool idSuccess = Int32.TryParse(linha.Split(new[] { "ç" }, StringSplitOptions.None)[0], out id);

                            if (idSuccess)
                            {
                                Venda venda = new Venda
                                {
                                    Id = Int32.Parse(linha.Split(new[] { "ç" }, StringSplitOptions.None)[1]),
                                    Items = items,
                                    SalesmanName = linha.Split(new[] { "ç" }, StringSplitOptions.None)[3],
                                    ValorTotal = valorTotal
                                };

                                bool existeVendaId = vendas.Exists(x => x.Id == venda.Id);

                                if (!existeVendaId)
                                    vendas.Add(venda);
                            }

                            break;

                        default:
                            Console.WriteLine("Linha inválida");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Linha inválida");
                }

            }

            foreach (var vendedor in vendedores)
            {
                vendedor.Vendas = vendas.Where(x => x.SalesmanName == vendedor.Name).ToList();

                vendedor.TotalVendas = vendedor.Vendas.Sum(x => x.ValorTotal);
            }

            int quantidadeClientes = clientes.Count();

            int quantidadeVendedores = vendedores.Count();

            decimal vendaMaisCaraValor = vendas.Max(x => x.ValorTotal);

            int vendaMaisCaraId = vendas.First(x => x.ValorTotal == vendaMaisCaraValor).Id;

            decimal piorSomaVendas = vendedores.Min(x => x.TotalVendas);

            Vendedor piorVendedor = vendedores.First(x => x.TotalVendas == piorSomaVendas);

            string relatorio = $"Quantidade de clientes no arquivo de entrada: {quantidadeClientes}{Environment.NewLine}";

            relatorio += $"Quantidade de vendedores no arquivo de entrada: {quantidadeVendedores}{Environment.NewLine}";

            relatorio += $"Id da venda mais cara: {vendaMaisCaraId}{Environment.NewLine}";

            relatorio += $"Pior vendedor: {piorVendedor.Name}, CPF: {piorVendedor.CPF}{Environment.NewLine}";

            return relatorio;

        }

        private static void SalvarRelatorio(string nomeArquivo, string conteudo)
        {
            using (FileStream fs = File.Create(nomeArquivo))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(conteudo);

                fs.Write(info, 0, info.Length);
            }
            Console.WriteLine(conteudo);
        }

    }
}
