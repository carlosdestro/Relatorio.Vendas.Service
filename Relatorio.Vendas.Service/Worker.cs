using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Relatorio.Vendas.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            string homePath = Directory.GetCurrentDirectory();

            string entrada = $"{homePath}/data/in";

            string saida = $"{homePath}/data/out";

            if (!Directory.Exists(entrada))
                Directory.CreateDirectory(entrada);

            if (!Directory.Exists(saida))
                Directory.CreateDirectory(saida);

            _logger.LogInformation($"O diretório de entrada é: {entrada}");

            _logger.LogInformation($"O diretório de saída é: {saida}");

            var arquivosExistentes = Directory.GetFiles(entrada);

            foreach (var arquivo in arquivosExistentes)
            {
                Relatorio.LerArquivo(arquivo);
            }

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                await Task.Delay(1000, stoppingToken);

                watcher.Path = entrada;

                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

                watcher.Filter = "";

                watcher.Changed += OnChanged;

                watcher.Created += OnChanged;

                watcher.EnableRaisingEvents = true;

                while (!stoppingToken.IsCancellationRequested) ;
            }
        }
        private static void OnChanged(object source, FileSystemEventArgs e) =>
            Relatorio.LerArquivo(e.FullPath);
    }
}
