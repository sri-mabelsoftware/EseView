﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EseDump
{
    class Program
    {
        static void Usage()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly().GetName();
            Console.WriteLine("{0} version {1}", assembly.Name, assembly.Version);
            Console.WriteLine("usage: {0}.exe [/recover] <database path> [<table name>[/<index name>] [...]]", assembly.Name);
        }

        static async Task<int> Run(IEnumerable<string> args)
        {
            var vm = new EseView.MainViewModel();
            bool recover = false;

            string dbPath = args.First();
            args = args.Skip(1);

            if (args.FirstOrDefault() == "/recover")
            {
                recover = true;
                args = args.Skip(1);
            }

            try
            {
                await vm.OpenDatabaseAsync(dbPath, recover);
            }
            catch (Microsoft.Isam.Esent.Interop.EsentDatabaseDirtyShutdownException)
            {
                Console.WriteLine("The database was not shut down cleanly.");
                Console.WriteLine("Use the /recover flag to enable recovery.");
                Usage();
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading database: " + ex.Message);
                return -2;
            }

            List<string> tables = args.ToList();
            if (tables.Count == 0)
            {
                tables = vm.Tables;
            }

            using (var output = Console.OpenStandardOutput())
            {
                vm.DumpTable(tables, output);
            }

            return 0;
        }

        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                Environment.Exit(Run(args).Result);
            }
            else
            {
                Usage();
                Environment.Exit(1);
            }
        }
    }
}
