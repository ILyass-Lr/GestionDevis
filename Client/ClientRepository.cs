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
        readonly private SqlConnection Conn;
        public ClientRepository(SqlConnection conn)
        {
            this.Conn = conn;
        }

        public Client GetClientById(int clientId)
        {
            Client client = null;
            using (SqlCommand cmd = new SqlCommand("SELECT [IF], ICE FROM [InfoClients] WHERE ClientId = @ClientId", Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                Conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        client = new Client
                        {
                            ClientId = clientId,
                            ICE = reader.IsDBNull(reader.GetOrdinal("ICE")) ? null : reader.GetString(reader.GetOrdinal("ICE")),
                            IF = reader.IsDBNull(reader.GetOrdinal("IF")) ? null : reader.GetString(reader.GetOrdinal("IF")),
                        };
                    }
                }
                Conn.Close();
            }
            return client;
        }
        public List<string> GetRS()
        {
            List<string> rsList = new List<string>();
            using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT RS FROM [InfoClients] WHERE RS IS NOT NULL", Conn))
            {
                Conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("RS")))
                        {
                            rsList.Add(reader.GetString(reader.GetOrdinal("RS")));
                        }
                    }
                }
                Conn.Close();
            }
            return rsList;
        }
        public List<Client> GetClientsByRS(string rs)
        {
            List<Client> clients = new List<Client>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM [InfoClients] WHERE RS = @RS", Conn))
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
            Conn.Open();

            List<Client> clients = new List<Client>();
            using (SqlCommand cmd = new SqlCommand("SELECT TypeSociete, Nom, Prenom, Ville, Patente, ICE, RC, [IF], Pays, ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM [InfoClients]", Conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Client client = new Client
                        {
                            TypeSociete = reader.IsDBNull(reader.GetOrdinal("TypeSociete")) ? null : reader.GetString(reader.GetOrdinal("TypeSociete")),
                            Nom = reader.IsDBNull(reader.GetOrdinal("Nom")) ? null : reader.GetString(reader.GetOrdinal("Nom")),
                            Prenom = reader.IsDBNull(reader.GetOrdinal("Prenom")) ? null : reader.GetString(reader.GetOrdinal("Prenom")),
                            Ville = reader.IsDBNull(reader.GetOrdinal("Ville")) ? null : reader.GetString(reader.GetOrdinal("Ville")),
                            Patente = reader.IsDBNull(reader.GetOrdinal("Patente")) ? null : reader.GetString(reader.GetOrdinal("Patente")),
                            ICE = reader.IsDBNull(reader.GetOrdinal("ICE")) ? null : reader.GetString(reader.GetOrdinal("ICE")),
                            RC = reader.IsDBNull(reader.GetOrdinal("RC")) ? null : reader.GetString(reader.GetOrdinal("RC")),
                            IF = reader.IsDBNull(reader.GetOrdinal("IF")) ? null : reader.GetString(reader.GetOrdinal("IF")),
                            Pays = reader.IsDBNull(reader.GetOrdinal("Pays")) ? null : reader.GetString(reader.GetOrdinal("Pays")),
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            NomResp = reader.GetString(reader.GetOrdinal("NomResp")),
                            Tel = reader.GetString(reader.GetOrdinal("Tel")),
                            Portable = reader.GetString(reader.GetOrdinal("Portable")),
                            Fax = reader.IsDBNull(reader.GetOrdinal("Fax")) ? null : reader.GetString(reader.GetOrdinal("Fax")),
                            RS = reader.IsDBNull(reader.GetOrdinal("RS")) ? null : reader.GetString(reader.GetOrdinal("RS")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                            Adresse = reader.IsDBNull(reader.GetOrdinal("Adresse")) ? null : reader.GetString(reader.GetOrdinal("Adresse")),
                        };
                        clients.Add(client);
                    }
                }
                Conn.Close();
            }
            return clients;
        }
        public bool UpdateClient(Client client)
        {
            string query = @"UPDATE [InfoClients] SET 
                     NomResp = @NomResp, 
                     TypeSociete = @TypeSociete, 
                     Nom = @Nom, 
                     Prenom = @Prenom,
                     Tel = @Tel, 
                     Portable = @Portable, 
                     Fax = @Fax, 
                     RS = @RS, 
                     Email = @Email, 
                     Adresse = @Adresse,
                     Ville = @Ville, 
                     Patente = @Patente, 
                     ICE = @ICE, 
                     RC = @RC, 
                     [IF] = @IF, 
                     Pays = @Pays 
                     WHERE ClientId = @ClientId";
            try
            {
         
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    // Ajouter les paramètres
                    cmd.Parameters.AddWithValue("@ClientId", client.ClientId);
                    cmd.Parameters.AddWithValue("@NomResp", client.NomResp ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TypeSociete", client.TypeSociete ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nom", client.Nom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Prenom", client.Prenom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tel", client.Tel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Portable", client.Portable ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fax", client.Fax ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RS", client.RS ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", client.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Adresse", client.Adresse ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ville", client.Ville ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Patente", client.Patente ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ICE", client.ICE ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RC", client.RC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IF", client.IF ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pays", client.Pays ?? (object)DBNull.Value);

                    Conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Conn.Close();

                    return rowsAffected > 0; // Retourne true si la mise à jour a réussi
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur ET afficher à l'utilisateur
                Debug.WriteLine($"Error updating client: {ex.Message}");
                            return false;
            }
        }
        public Boolean ClientExists(int clientId)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [InfoClients] WHERE ClientId = @ClientId", Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                Conn.Open();
                int count = (int)cmd.ExecuteScalar();
                Conn.Close();
                return count > 0;
            }
        }
        public void InsertClient(Client client)
        {
            Console.WriteLine("Inserting client: " + client.NomResp);

            // Une seule requête sans ClientId car c'est un IDENTITY
            string query = "INSERT INTO [InfoClients] (NomResp, TypeSociete, Nom, Prenom, Tel, Portable, Fax, RS, Email, Adresse, Ville, Patente, ICE, RC, [IF], Pays) " +
                           "VALUES (@NomResp, @TypeSociete, @Nom, @Prenom, @Tel, @Portable, @Fax, @RS, @Email, @Adresse, @Ville, @Patente, @ICE, @RC, @IF, @Pays)";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    // Ne pas inclure ClientId - il sera généré automatiquement
                    cmd.Parameters.AddWithValue("@NomResp", (object)client.NomResp ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TypeSociete", (object)client.TypeSociete ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nom", (object)client.Nom ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Prenom", (object)client.Prenom ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tel", (object)client.Tel ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Portable", (object)client.Portable ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fax", (object)client.Fax ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RS", (object)client.RS ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)client.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Adresse", (object)client.Adresse ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ville", (object)client.Ville ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Patente", (object)client.Patente ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ICE", (object)client.ICE ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RC", (object)client.RC ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IF", (object)client.IF ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pays", (object)client.Pays ?? DBNull.Value);

                    if (Conn.State != ConnectionState.Open)
                        Conn.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Rows affected: {rowsAffected}");

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Client inserted successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting client: {ex.Message}");
                throw; // Re-lancer l'exception pour debugging
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }
    }
}
