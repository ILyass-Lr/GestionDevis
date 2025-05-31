using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
namespace GestionClientFactures
{
    public class DevisRepository
    {
        private SqlConnection Conn;
        public DevisRepository(SqlConnection conn)
        {
            this.Conn = conn;
        }

        public string GenerateDevisN()
        {
            Conn.Open();
            SqlCommand cmd = Conn.CreateCommand();
            object devisNumber;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT TOP 1 DevisNumber FROM [Devis] ORDER BY DevisN DESC";
            devisNumber = cmd.ExecuteScalar();
            if (devisNumber == null)
            {
                this.Conn.Close();
                return "D001/" + DateTime.Now.Year;
            }
            int num = int.Parse(devisNumber.ToString().Split('/')[0].Substring(1));
            num++;
            Conn.Close();
            return "D" + num.ToString("D3") + "/" + DateTime.Now.Year;
        }
        public List<Devis> GetAllDevis()
        {
            List<Devis> devisList = new List<Devis>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM [Devis]", Conn))
                {
                    Conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Devis devis = new Devis(
                                reader.GetInt32(reader.GetOrdinal("DevisN")),
                                reader.GetInt32(reader.GetOrdinal("Quantity")),
                                reader.GetFloat(reader.GetOrdinal("MontantHT")),
                                reader.GetFloat(reader.GetOrdinal("MontantTVA")),
                                reader.GetDateTime(reader.GetOrdinal("Date")),
                                reader.GetInt32(reader.GetOrdinal("ClientId")),
                                (DevisStatus)Enum.Parse(typeof(DevisStatus), reader.GetString(reader.GetOrdinal("Status"))),
                                reader.GetString(reader.GetOrdinal("RS")),
                                reader.GetString(reader.GetOrdinal("DevisNumber")) // Assuming DevisNumber is a column in the Devis table
                            );
                            devisList.Add(devis);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving devis: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
            return devisList;
        }

        public Devis GetLastDevis()
        {
            Devis lastDevis = null;
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM Devis ORDER BY DevisN DESC", Conn))
                {
                    Conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lastDevis = new Devis(
                                reader.GetInt32(reader.GetOrdinal("DevisN")),
                                reader.GetInt32(reader.GetOrdinal("Quantity")),
                                reader.GetFloat(reader.GetOrdinal("MontantHT")),
                                reader.GetFloat(reader.GetOrdinal("MontantTVA")),
                                reader.GetDateTime(reader.GetOrdinal("Date")),
                                reader.GetInt32(reader.GetOrdinal("ClientId")),
                                (DevisStatus)Enum.Parse(typeof(DevisStatus), reader.GetString(reader.GetOrdinal("Status"))),
                                reader.GetString(reader.GetOrdinal("RS")),
                                reader.GetString(reader.GetOrdinal("DevisNumber")) // Assuming DevisNumber is a column in the Devis table
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving last devis: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
            return lastDevis;
        }

        public void InsertDevis(Devis devis)
        {
            if (devis == null)
            {
                throw new ArgumentNullException(nameof(devis), "Devis cannot be null");

            }
            try
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Devis (DevisN, Quantity, MontantHT, Date, ClientId, Status, RS, MontantTVA, DevisNumber) VALUES (@DevisN, @Quantity, @Total, @Date, @ClientId, @Status, @RS, @TVA, @number)", Conn))
                {
                    cmd.Parameters.AddWithValue("@DevisN", devis.DevisId);
                    cmd.Parameters.AddWithValue("@Quantity", devis.Quantity);
                    cmd.Parameters.AddWithValue("@Total", devis.MontantHT);
                    cmd.Parameters.AddWithValue("@Date", devis.Date);
                    cmd.Parameters.AddWithValue("@ClientId", devis.ClientId);
                    cmd.Parameters.AddWithValue("@Status", devis.Status.ToString());
                    cmd.Parameters.AddWithValue("@RS", devis.RS);
                    cmd.Parameters.AddWithValue("@TVA", devis.MontantTVA);
                    cmd.Parameters.AddWithValue("@number", devis.Number);

                    Conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting devis: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
        }
    }
}
