using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
namespace GestionClientFactures.Repositories
{
    public class FactureRepository
    {
        private SqlConnection Conn;
        public FactureRepository(SqlConnection conn)
        {
            this.Conn = conn;
        }
        public void InertFacture(Facture facture)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO [factureClt] (FactureN, Designation, Quantity, Prix, Tva, Reference, Date, ClientId) VALUES (@FactureN, @Designation, @Quantity, @Prix, @Total, @Reference, @Date, @ClientId)", Conn))
                {
                    cmd.Parameters.AddWithValue("@Designation", facture.Designation);
                    cmd.Parameters.AddWithValue("@Quantity", facture.Quantity);
                    cmd.Parameters.AddWithValue("@Prix", facture.Prix);
                    cmd.Parameters.AddWithValue("@Tva", facture.Tva);
                    cmd.Parameters.AddWithValue("@Reference", facture.Reference);
                    cmd.Parameters.AddWithValue("@Date", facture.Date);
                    cmd.Parameters.AddWithValue("@ClientId", facture.ClientId);
                    Conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting facture: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
        }
        public void DeleteByReference(string reference)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM [factureClt] WHERE Reference = @Reference", Conn))
                {
                    cmd.Parameters.AddWithValue("@Reference", reference);
                    Conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting facture by reference: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
        }
        public List<Facture> GetAllFactures()
        {
            List<Facture> factures = new List<Facture>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT d.DevisNumber, f.* FROM [factureClt] f INNER JOIN [Devis] d ON f.DevisN = d.DevisN", Conn))
                {
                    Conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facture facture = new Facture(
                                reader.GetString(reader.GetOrdinal("DevisNumber")),
                                reader.GetString(reader.GetOrdinal("Designation")),
                                reader.GetInt32(reader.GetOrdinal("Quantity")),
                                reader.GetFloat(reader.GetOrdinal("Prix")),
                                reader.GetFloat(reader.GetOrdinal("Tva")),
                                reader.GetString(reader.GetOrdinal("Reference")),
                                reader.GetDateTime(reader.GetOrdinal("Date")),
                                reader.GetInt32(reader.GetOrdinal("ClientId"))
                            );
                            factures.Add(facture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching factures: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
            return factures;
        }
    }
}
