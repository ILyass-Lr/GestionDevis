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
        readonly private SqlConnection Conn;
        public FactureRepository(SqlConnection conn)
        {
            this.Conn = conn;
        }
        public void InsertFacture(Facture facture)
        {
            Console.WriteLine("Insertion encours");
            try
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO [factureClt] (Designation, Quantite, Prix, Tva, Reference, Date, DevisN) VALUES (@Designation, @Quantity, @Prix, @Tva, @Reference, @Date, @DevisN)", Conn))
                {
                    cmd.Parameters.AddWithValue("@Designation", facture.Designation);
                    cmd.Parameters.AddWithValue("@Quantity", facture.Quantity);
                    cmd.Parameters.AddWithValue("@Prix", facture.Prix);
                    cmd.Parameters.AddWithValue("@Tva", facture.Tva);  // Maintenant correspond à @Tva dans la requête
                    cmd.Parameters.AddWithValue("@Reference", facture.Reference);
                    cmd.Parameters.AddWithValue("@Date", facture.Date);
                    cmd.Parameters.AddWithValue("@DevisN", facture.DevisN);

                    Conn.Open();
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Facture insérée avec succès");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting facture: {ex.Message}");
                // Ajoutez cette ligne pour voir l'erreur complète
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                Conn.Close();
            }
        }
        public void DeleteById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM [factureClt] WHERE FactureN = @Id", Conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
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
        public Dictionary<string, List<Facture>> GetAllFactures()
        {
            Dictionary<string, List<Facture>> factures = new Dictionary<string, List<Facture>>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT d.DevisNumber, f.* FROM [factureClt] f INNER JOIN [Devis] d ON f.DevisN = d.DevisN", Conn))
                {
                    Conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                // Utilisation de GetValue() avec conversion sécurisée
                                int devisN = Convert.ToInt32(reader["DevisN"]);
                                string designation = reader["Designation"]?.ToString();
                                int quantite = Convert.ToInt32(reader["Quantite"]);
                                float prix = Convert.ToSingle(reader["Prix"]);
                                float tva = Convert.ToSingle(reader["Tva"]);
                                string reference = reader["Reference"]?.ToString();
                                DateTime date = Convert.ToDateTime(reader["Date"]);
                                string devisNumber = reader["DevisNumber"]?.ToString();

                                Facture facture = new Facture(devisN, designation, quantite, prix, tva, reference, date)
                                {
                                    FactureN = reader.GetInt32(reader.GetOrdinal("FactureN"))
                                };
                                if (!string.IsNullOrEmpty(devisNumber) && !factures.ContainsKey(devisNumber))
                                {
                                    factures.Add(devisNumber, new List<Facture> { facture });
                                }
                                else if (!string.IsNullOrEmpty(devisNumber))
                                {
                                    factures[devisNumber].Add(facture);
                                }
                            }
                            catch (Exception innerEx)
                            {
                                MessageBox.Show($"Erreur conversion ligne: {innerEx.Message}\nValeurs: DevisN={reader["DevisN"]}, Prix={reader["Prix"]}, TVA={reader["Tva"]}", "Erreur");
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching factures: {ex.Message}", "Erreur DB");
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }

            return factures;
        }
        public void UpdateFacture(Facture facture, string oldRef)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE [factureClt] SET Designation = @Designation, Quantite = @Quantity, Prix = @Prix, Tva = @Tva, Reference = @NewReference, Date = @Date WHERE Reference = @OldReference", Conn))
                {
                    cmd.Parameters.AddWithValue("@Designation", facture.Designation);
                    cmd.Parameters.AddWithValue("@Quantity", facture.Quantity);
                    cmd.Parameters.AddWithValue("@Prix", facture.Prix);
                    cmd.Parameters.AddWithValue("@Tva", facture.Tva);
                    cmd.Parameters.AddWithValue("@OldReference", oldRef);
                    cmd.Parameters.AddWithValue("@Date", facture.Date);
                    cmd.Parameters.AddWithValue("@NewReference", facture.Reference);
                    Conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating facture: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
        }
    }
}
