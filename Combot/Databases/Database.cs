using System;
using System.Collections.Generic;
using Combot.Configurations;
using MySql.Data.MySqlClient;

namespace Combot.Databases
{
    public class Database
    {
        private bool Connected { get; set; }
        private MySqlConnection Connection { get; set; }

        public Database(DatabaseConfig config)
        {
            Connected = false;
            Connection = null;
            Connect(config);
        }

        public List<Dictionary<string, object>> Query(string query, params object[] args)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            if (Connected)
            {
                MySqlCommand cmd = PrepareQuery(query, args);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    rows.Add(row);
                }
                reader.Close();
            }
            return rows;
        }

        public object ScalarQuery(string query, params object[] args)
        {
            if (Connected)
            {
                MySqlCommand cmd = PrepareQuery(query, args);
                return cmd.ExecuteScalar();
            }
            return null;
        }

        public void Execute(string query, params object[] args)
        {
            if (Connected)
            {
                MySqlCommand cmd = PrepareQuery(query, args);
                cmd.ExecuteNonQuery();
            }
        }

        private void Connect(DatabaseConfig config)
        {
            if (Connection == null)
            {
                if (config.Server != string.Empty && config.Database != string.Empty && config.Username != string.Empty && config.Password != string.Empty)
                {
                    string strCon = string.Format("Server={0}; database={1}; user={2}; password={3}; port={4}; charset=utf8", config.Server, config.Database, config.Username, config.Password, config.Port);
                    Connection = new MySqlConnection(strCon);
                    try
                    {
                        Connection.Open();
                        Connected = true;
                    }
                    catch (MySqlException ex)
                    {
                        Connected = false;
                    }
                }
            }
        }

        private void Disconnect()
        {
            if (Connection != null && Connected)
            {
                Connected = false;
                Connection.Close();
            }
        }

        private MySqlCommand PrepareQuery(string query, object[] args)
        {
            if (Connected)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Connection;
                for (int i = 0; i < args.Length; i++)
                {
                    string param = "{" + i + "}";
                    string paramName = "@DBVar_" + i;
                    query = query.Replace(param, paramName);
                    cmd.Parameters.AddWithValue(paramName, args[i]);
                }
                cmd.CommandText = query;
                return cmd;
            }
            return null;
        }
    }
}