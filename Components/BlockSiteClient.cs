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
        
        public void ReadBlockedDomain()
        {
            try
            {
                string line = "";
                StreamReader sr = new StreamReader(hostsPath);
                Console.WriteLine("Start Read File");
                while (line != null)
                {
                    line = sr.ReadLine();
                    if (line == null) break;
                    if (line[0] == '#') continue;
                    Console.WriteLine(line);
                }
                Console.WriteLine("Stop Read File");
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public void BlockDomain(string domain)
        {
            try
            {
                StreamWriter sw = new StreamWriter(hostsPath);
                sw.WriteLine($"127.0.0.1 {domain}");
                sw.WriteLine($"127.0.0.1 www.{domain}");
                sw.Close();
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
            FileStream fs = new FileStream(hostsPath, FileMode.Open);
            fs.SetLength(0);
        }
    }
}
