using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace user_client.Components
{
    public class BlockSiteClient
    {
        const string hostsPath = @"C:\Windows\System32\drivers\etc\hosts";
        
        public bool IsExistDomain(string domain)
        {
            try
            {
                using (var sr = new StreamReader(
                    new FileStream(hostsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    string line;
                    Console.WriteLine("Start Read File");
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line[0] == '#') continue;
                        Console.WriteLine(line);

                        if (line.Contains(domain)) return true;
                    }
                }
                Console.WriteLine("Stop Read File");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            return false;
        }

        public void BlockDomain(string domain)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(hostsPath, true, Encoding.UTF8, 4096))
                {
                    sw.WriteLine($"127.0.0.1 {domain}");
                    sw.WriteLine($"127.0.0.1 www.{domain}");
                }
                Console.WriteLine($"BlockDomain: Success ({domain})");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
        public void RemoveDomain(string domain)
        {
            try
            {
                var lines = File.ReadAllLines(hostsPath);
                var filteredLines = lines
                    .Where(line => !line.Contains(domain))
                    .ToList();

                File.WriteAllLines(hostsPath, filteredLines);
                Console.WriteLine("Successful Remove Domain");
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        public void ClearDomain()
        {
            using (FileStream fs = new FileStream(hostsPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.SetLength(0);
            }
            Console.WriteLine("ClearDomain: Success");
        }
    }
}
