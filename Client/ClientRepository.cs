using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace BaseClient
{
    public class ClientRepository
    {
        private SqlConnection Conn;
        public ClientRepository(SqlConnection conn)
        {
            this.Conn = conn;
        }
        public Client getClientById(int clientId)
        {
            Client client = null;
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Clients WHERE ClientId = @ClientId", Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                Conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        client = new Client
                        {
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            NomResp = reader.GetString(reader.GetOrdinal("NomResp")),
                            TypeSociete = reader.GetString(reader.GetOrdinal("TypeSociete")),
                            Nom = reader.GetString(reader.GetOrdinal("Nom")),
                            Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                            Tel = reader.GetString(reader.GetOrdinal("Tel")),
                            Portable = reader.GetString(reader.GetOrdinal("Portable")),
                            Fax = reader.IsDBNull(reader.GetOrdinal("Fax")) ? null : reader.GetString(reader.GetOrdinal("Fax")),
                            RS = reader.IsDBNull(reader.GetOrdinal("RS")) ? null : reader.GetString(reader.GetOrdinal("RS")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                            Adresse = reader.IsDBNull(reader.GetOrdinal("Adresse")) ? null : reader.GetString(reader.GetOrdinal("Adresse")),
                            Ville = reader.IsDBNull(reader.GetOrdinal("Ville")) ? null : reader.GetString(reader.GetOrdinal("Ville")),
                            Patente = reader.IsDBNull(reader.GetOrdinal("Patente")) ? null : reader.GetString(reader.GetOrdinal("Patente")),
                            ICE = reader.IsDBNull(reader.GetOrdinal("ICE")) ? null : reader.GetString(reader.GetOrdinal("ICE")),
                            RC = reader.IsDBNull(reader.GetOrdinal("RC")) ? null : reader.GetString(reader.GetOrdinal("RC")),
                            IF = reader.IsDBNull(reader.GetOrdinal("IF")) ? null : reader.GetString(reader.GetOrdinal("IF")),
                            Pays = reader.IsDBNull(reader.GetOrdinal("Pays")) ? null : reader.GetString(reader.GetOrdinal("Pays"))
                        };
                    }
                }
                Conn.Close();
            }
            return client;
        }
        public List<Client> GetClientsByRS(string rs)
        {
            List<Client> clients = new List<Client>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Clients WHERE RS = @RS", Conn))
            {
                cmd.Parameters.AddWithValue("@RS", rs);
                Conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Client client = new Client
                        {
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            Nom = reader.GetString(reader.GetOrdinal("Nom")),
                            Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                            ICE = reader.IsDBNull(reader.GetOrdinal("ICE")) ? null : reader.GetString(reader.GetOrdinal("ICE")),
                            IF = reader.IsDBNull(reader.GetOrdinal("IF")) ? null : reader.GetString(reader.GetOrdinal("IF")),
                        };
                        clients.Add(client);
                    }
                }
                Conn.Close();
            }
            return clients;
        }
        public List<Client> GetAllClients()
        {
            List<Client> clients = new List<Client>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Clients", Conn))
            {
                Conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Client client = new Client
                        {
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            NomResp = reader.GetString(reader.GetOrdinal("NomResp")),
                            TypeSociete = reader.GetString(reader.GetOrdinal("TypeSociete")),
                            Nom = reader.GetString(reader.GetOrdinal("Nom")),
                            Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                            Tel = reader.GetString(reader.GetOrdinal("Tel")),
                            Portable = reader.GetString(reader.GetOrdinal("Portable")),
                            Fax = reader.IsDBNull(reader.GetOrdinal("Fax")) ? null : reader.GetString(reader.GetOrdinal("Fax")),
                            RS = reader.IsDBNull(reader.GetOrdinal("RS")) ? null : reader.GetString(reader.GetOrdinal("RS")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                            Adresse = reader.IsDBNull(reader.GetOrdinal("Adresse")) ? null : reader.GetString(reader.GetOrdinal("Adresse")),
                            Ville = reader.IsDBNull(reader.GetOrdinal("Ville")) ? null : reader.GetString(reader.GetOrdinal("Ville")),
                            Patente = reader.IsDBNull(reader.GetOrdinal("Patente")) ? null : reader.GetString(reader.GetOrdinal("Patente")),
                            ICE = reader.IsDBNull(reader.GetOrdinal("ICE")) ? null : reader.GetString(reader.GetOrdinal("ICE")),
                            RC = reader.IsDBNull(reader.GetOrdinal("RC")) ? null : reader.GetString(reader.GetOrdinal("RC")),
                            IF = reader.IsDBNull(reader.GetOrdinal("IF")) ? null : reader.GetString(reader.GetOrdinal("IF")),
                            Pays = reader.IsDBNull(reader.GetOrdinal("Pays")) ? null : reader.GetString(reader.GetOrdinal("Pays"))
                        };
                        clients.Add(client);
                    }
                }
                Conn.Close();
            }
            return clients;
        }
        public void AddClient(Client client)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO Clients (NomResp, TypeSociete, Nom, Prenom, Tel, Portable, Fax, RS, Email, Adresse, Ville, Patente, ICE, RC, IF, Pays) VALUES (@NomResp, @TypeSociete, @Nom, @Prenom, @Tel, @Portable, @Fax, @RS, @Email, @Adresse, @Ville, @Patente, @ICE, @RC, @IFValue, @Pays)", Conn))
            {
                cmd.Parameters.AddWithValue("@NomResp", client.NomResp);
                cmd.Parameters.AddWithValue("@TypeSociete", client.TypeSociete);
                cmd.Parameters.AddWithValue("@Nom", client.Nom);
                cmd.Parameters.AddWithValue("@Prenom", client.Prenom);
                cmd.Parameters.AddWithValue("@Tel", client.Tel);
                cmd.Parameters.AddWithValue("@Portable", client.Portable);
                cmd.Parameters.AddWithValue("@Fax", client.Fax);
                cmd.Parameters.AddWithValue("@RS", client.RS);
                cmd.Parameters.AddWithValue("@Email", client.Email);
                cmd.Parameters.AddWithValue("@Adresse", client.Adresse);
                cmd.Parameters.AddWithValue("@Ville", client.Ville);
                cmd.Parameters.AddWithValue("@Patente", client.Patente);
                cmd.Parameters.AddWithValue("@ICE", client.ICE);
                cmd.Parameters.AddWithValue("@RC", client.RC);
                cmd.Parameters.AddWithValue("@IFValue", client.IF);
                cmd.Parameters.AddWithValue("@Pays", client.Pays);
                Conn.Open();
                cmd.ExecuteNonQuery();
                Conn.Close();
            }
        }
    }
}
