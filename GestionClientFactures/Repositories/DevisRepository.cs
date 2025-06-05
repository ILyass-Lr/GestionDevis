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
        readonly private SqlConnection Conn;
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
            string[] numYear = devisNumber.ToString().Split('/');
            int num = (numYear[1] != DateTime.Now.Year.ToString()) ? 1 : int.Parse(numYear[0].Substring(1)) + 1;
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
                                Convert.ToInt32(reader["DevisN"]),
                                Convert.ToInt32(reader["Quantite"]),
                                Convert.ToSingle(reader["MontantHT"]),
                                Convert.ToSingle(reader["MontantTVA"]),
                                Convert.ToDateTime(reader["Date"]),
                                Convert.ToInt32(reader["ClientId"]),
                                (DevisStatus)Enum.Parse(typeof(DevisStatus), reader.GetString(reader.GetOrdinal("Status"))),
                                reader["RS"].ToString(),
                                reader["DevisNumber"].ToString()
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
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM [Devis] ORDER BY DevisN DESC", Conn))
                {
                    Conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lastDevis = new Devis(
                                reader.GetInt32(reader.GetOrdinal("DevisN")),
                                reader.GetInt32(reader.GetOrdinal("Quantite")),
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
        public int GetDevisIdByNumber(string devisNbr)
        {
            int devisId = -1;
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT DevisN FROM [Devis] WHERE DevisNumber = @DevisNbr", Conn))
                {
                    cmd.Parameters.AddWithValue("@DevisNbr", devisNbr);
                    Conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        devisId = Convert.ToInt32(result);
                        Console.WriteLine($"Devis ID for {devisNbr} is {devisId}");

                    }
                    else
                    {
                        Console.WriteLine($"No Devis found with number {devisNbr}");
                    }
                    return devisId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving Devis ID: {ex.Message}");
                return -1;
            }
            finally
            {
                Conn.Close();

            }
        }
        public int CreateDevis(Devis devis)
        {
            if (devis == null)
            {
                throw new ArgumentNullException(nameof(devis), "Devis cannot be null");
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO [Devis] 
                (Quantite, MontantHT, Date, ClientId, Status, RS, MontantTVA, DevisNumber) 
            VALUES 
                (@Quantity, @Total, @Date, @ClientId, @Status, @RS, @TVA, @number); 
            SELECT CAST(SCOPE_IDENTITY() AS INT);", Conn))
                {
                    cmd.Parameters.AddWithValue("@Quantity", 0);
                    cmd.Parameters.AddWithValue("@Total", 0);
                    cmd.Parameters.AddWithValue("@Date", devis.Date);
                    cmd.Parameters.AddWithValue("@ClientId", devis.ClientId);
                    cmd.Parameters.AddWithValue("@Status", devis.Status.ToString());
                    cmd.Parameters.AddWithValue("@RS", devis.RS);
                    cmd.Parameters.AddWithValue("@TVA", 0);
                    cmd.Parameters.AddWithValue("@number", devis.Number);

                    Conn.Open();
                    int newId = (int)cmd.ExecuteScalar(); // récupère l'ID inséré
                    return newId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting devis: {ex.Message}");
                return -1; // ou lève une exception selon ton besoin
            }
            finally
            {
                Conn.Close();
            }
        }
        public bool UpdateDevis(Devis devis)
        {
            if (devis == null)
            {
                throw new ArgumentNullException(nameof(devis), "Devis cannot be null");
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE [Devis] SET Quantite = @Quantity, MontantHT = @MontantHT, MontantTVA = @MontantTVA, Date = @Date, ClientId = @ClientId, Status = @Status, RS = @RS WHERE DevisN = @DevisN", Conn))
                {
                    cmd.Parameters.AddWithValue("@Quantity", devis.Quantity);
                    cmd.Parameters.AddWithValue("@MontantHT", devis.MontantHT);
                    cmd.Parameters.AddWithValue("@MontantTVA", devis.MontantTVA);
                    cmd.Parameters.AddWithValue("@Date", devis.Date);
                    cmd.Parameters.AddWithValue("@ClientId", devis.ClientId);
                    cmd.Parameters.AddWithValue("@Status", devis.Status.ToString());
                    cmd.Parameters.AddWithValue("@RS", devis.RS);
                    cmd.Parameters.AddWithValue("@DevisN", devis.DevisId);
                    Conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating devis: {ex.Message}");
                return false;
            }
            finally
            {
                Conn.Close();
            }
        }
    }
}