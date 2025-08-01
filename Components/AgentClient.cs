using DotNetEnv;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using user_client.Model;

namespace user_client.Components
{
    public class AgentClient
    {
        private Process _agentProc;

        public void StartAgent(string empId)
        {
            bool isAllowActiveAgent = IsAllowAgentRunning(empId);
            if (!isAllowActiveAgent) return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(baseDir, "Agent", "PacketFlowMonitor.exe"),
                Arguments = empId,
                UseShellExecute = false,
            };
            _agentProc = Process.Start(startInfo);
            Console.WriteLine("Started Agent");
        }
        public void KillAgent()
        {
            try
            {
                if (_agentProc == null) return;
                _agentProc.Kill();
                _agentProc.Close();
                Console.WriteLine("Killed Agent");
            } catch(Exception ex)
            {
                Console.WriteLine($"Agent {ex.Message}");
            }
        }

        public bool IsAllowAgentRunning(string empId)
        {
            string query = $@"
                select p.is_active_agent
                from policys p
                where p.emp_id='{empId}';";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string dbConnection;
            try
            {
                Env.Load();
                dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                if (dbHost == null) throw new Exception(".env DB_HOST is null");
                dbPort = Environment.GetEnvironmentVariable("DB_PORT");
                if (dbPort == null) throw new Exception(".env DB_PORT is null");
                dbUid = Environment.GetEnvironmentVariable("DB_UID");
                if (dbUid == null) throw new Exception(".env DB_UID is null"); ;
                dbPwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (dbPwd == null) throw new Exception(".env DB_PWD is null");
                dbName = Environment.GetEnvironmentVariable("DB_NAME");
                if (dbName == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUid};Pwd={dbPwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand selectCmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = selectCmd.ExecuteReader();
                    if (rdr == null || !rdr.Read()) return false;

                    bool isAllow = rdr[0].ToString() == "0" ? false : true;

                    connection.Close();
                    return isAllow;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading posts: " + ex.Message);
                return false;
            }
        }
    }
}
