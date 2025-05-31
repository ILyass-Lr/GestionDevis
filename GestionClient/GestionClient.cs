using BaseClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
namespace GestionClient
{
    public partial class InformationClient : MetroForm
    {
        private SqlConnection connection;
        private List<Client> clients = new List<Client>();
        private Client selectedClient = null;
        public InformationClient()
        {
            InitializeComponent();
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\lenovo\Desktop\GI\S8\Dot Net Technologies\TP5-6\GestionClientFactures\Database.mdf"";Integrated Security=True";
            connection = new SqlConnection(connectionString);
        }

        private void InformationClient_Load(object sender, EventArgs e)
        {
            LoadClientsToListView();
        }

        private void ICE_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
                return;
            }

            // Limit to 15 digits maximum
            Control control = sender as Control;
            if (control != null && char.IsDigit(e.KeyChar) && control.Text.Length >= 15)
            {
                e.Handled = true;
            }
        }

        private void RegistreC_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
                return;
            }

            // Limit to 5 digits maximum
            Control control = sender as Control;
            if (control != null && char.IsDigit(e.KeyChar) && control.Text.Length >= 5)
            {
                e.Handled = true;
            }
        }

        private void ClientId_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
        private void IF_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
                return;
            }

            // Safely cast to get the text length
            Control control = sender as Control;
            if (control != null && char.IsDigit(e.KeyChar) && control.Text.Length >= 8)
            {
                e.Handled = true;
            }
        }
        private void Patente_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) &&
                e.KeyChar != (char)Keys.Back &&
                e.KeyChar != ' ' &&
                e.KeyChar != '-' &&
                e.KeyChar != '_')
            {
                e.Handled = true;
            }
        }


        private void Tel_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits, spaces, +, -, (, ), and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back &&
                e.KeyChar != ' ' && e.KeyChar != '+' && e.KeyChar != '-' &&
                e.KeyChar != '(' && e.KeyChar != ')')
            {
                e.Handled = true;
            }
        }

        private void Portable_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Same as Tel_TextBox
            Tel_TextBox_KeyPress(sender, e);
        }

        private void Fax_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Same as Tel_TextBox
            Tel_TextBox_KeyPress(sender, e);
        }

        private void Email_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow most characters for email, restrict some special characters
            if (e.KeyChar == ' ' || e.KeyChar == '\t')
            {
                e.Handled = true;
            }
        }

        private void Nom_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, spaces, hyphens, apostrophes
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != (char)Keys.Back &&
                e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '\'')
            {
                e.Handled = true;
            }
        }

        private void Prenom_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Same as Nom_TextBox
            Nom_TextBox_KeyPress(sender, e);
        }

        private bool ValidateICE(string ice)
        {
            // Check if exactly 15 digits
            if (string.IsNullOrWhiteSpace(ice) || ice.Length != 15)
            {
                return false;
            }

            // Check if all characters are digits
            return ice.All(char.IsDigit);
        }

        private bool ValidateIF(string identifiantFiscal)
        {
            // Check if not empty and maximum 8 digits
            if (string.IsNullOrWhiteSpace(identifiantFiscal) || identifiantFiscal.Length > 8)
            {
                return false;
            }

            // Check if all characters are digits
            return identifiantFiscal.All(char.IsDigit);
        }

        private bool ValidateRC(string registreCommerce)
        {
            // Check if not empty and maximum 5 digits
            if (string.IsNullOrWhiteSpace(registreCommerce) || registreCommerce.Length > 5)
            {
                return false;
            }

            // Check if all characters are digits
            return registreCommerce.All(char.IsDigit);
        }

        private bool ValidatePatente(string patente)
        {
            // Check if not empty (assuming patente is required)
            return !string.IsNullOrWhiteSpace(patente);
        }

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true; // Email is optional

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidatePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true; // Phone numbers are optional

            // Remove spaces, hyphens, parentheses for validation
            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Check if it starts with + (international) or is all digits
            if (cleanPhone.StartsWith("+"))
            {
                return cleanPhone.Substring(1).All(char.IsDigit) && cleanPhone.Length >= 10;
            }

            return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 8;
        }

        private bool ValidateRequiredField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show($"Le champ {fieldName} est obligatoire.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateAllFields()
        {
            // Validate required fields (adjust based on your business rules)
            if (!ValidateRequiredField(RS_TextBox.Text, "Raison Sociale")) return false;
            if (!ValidateRequiredField(ICE_TextBox.Text, "ICE")) return false;
            if (!ValidateRequiredField(IF_TextBox.Text, "IF")) return false;
            if (!ValidateRequiredField(RegistreC_TextBox.Text, "Registre de Commerce")) return false;

            // Validate ICE format
            if (!ValidateICE(ICE_TextBox.Text))
            {
                MessageBox.Show("L'ICE doit contenir exactement 15 chiffres.", "Erreur ICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ICE_TextBox.Focus();
                return false;
            }

            // Validate IF format
            if (!ValidateIF(IF_TextBox.Text))
            {
                MessageBox.Show("L'Identifiant Fiscal doit contenir au maximum 8 chiffres.", "Erreur IF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                IF_TextBox.Focus();
                return false;
            }

            // Validate RC format

            if (!ValidateRC(RegistreC_TextBox.Text))
            {
                MessageBox.Show("Le Registre de Commerce doit contenir au maximum 5 chiffres.", "Erreur RC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RegistreC_TextBox.Focus();
                return false;
            }
            // Validate email if provided
            if (!ValidateEmail(Email_TextBox.Text))
            {
                MessageBox.Show("Veuillez saisir une adresse email valide.", "Email invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Email_TextBox.Focus();
                return false;
            }

            // Validate phone numbers if provided
            if (!ValidatePhoneNumber(Tel_TextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un numéro de téléphone valide.", "Téléphone invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Tel_TextBox.Focus();
                return false;
            }

            if (!ValidatePhoneNumber(Portable_TextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un numéro de portable valide.", "Portable invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Portable_TextBox.Focus();
                return false;
            }

            if (!ValidatePhoneNumber(Fax_TextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un numéro de fax valide.", "Fax invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Fax_TextBox.Focus();
                return false;
            }

            return true;
        }

        private void AjouterC_Tile_Click(object sender, EventArgs e)
        {
            if (!ValidateAllFields()) return;

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;

                // Note: ClientId is IDENTITY, so we don't insert it. It will be auto-generated.
                cmd.CommandText = @"INSERT INTO [InfoClients] 
                           (NomResp, TypeSociete, Nom, Prenom, Tel, Portable, Fax, RS, Email, Adresse, Ville, Patente, ICE, RC, [IF], Pays) 
                           VALUES 
                           (@nomResp, @typeSociete, @nom, @prenom, @tel, @portable, @fax, @rs, @email, @adresse, @ville, @patente, @ice, @rc, @if, @pays)";

                // Add parameters
                cmd.Parameters.AddWithValue("@nomResp", string.IsNullOrWhiteSpace(NomR_TextBox.Text) ? (object)DBNull.Value : NomR_TextBox.Text);
                cmd.Parameters.AddWithValue("@typeSociete", string.IsNullOrWhiteSpace(TypeS_TextBox.Text) ? (object)DBNull.Value : TypeS_TextBox.Text);
                cmd.Parameters.AddWithValue("@nom", string.IsNullOrWhiteSpace(Nom_TextBox.Text) ? (object)DBNull.Value : Nom_TextBox.Text);
                cmd.Parameters.AddWithValue("@prenom", string.IsNullOrWhiteSpace(Prenom_TextBox.Text) ? (object)DBNull.Value : Prenom_TextBox.Text);
                cmd.Parameters.AddWithValue("@tel", string.IsNullOrWhiteSpace(Tel_TextBox.Text) ? (object)DBNull.Value : Tel_TextBox.Text);
                cmd.Parameters.AddWithValue("@portable", string.IsNullOrWhiteSpace(Portable_TextBox.Text) ? (object)DBNull.Value : Portable_TextBox.Text);
                cmd.Parameters.AddWithValue("@fax", string.IsNullOrWhiteSpace(Fax_TextBox.Text) ? (object)DBNull.Value : Fax_TextBox.Text);
                cmd.Parameters.AddWithValue("@rs", string.IsNullOrWhiteSpace(RS_TextBox.Text) ? (object)DBNull.Value : RS_TextBox.Text);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(Email_TextBox.Text) ? (object)DBNull.Value : Email_TextBox.Text);
                cmd.Parameters.AddWithValue("@adresse", string.IsNullOrWhiteSpace(Adress_TextBox.Text) ? (object)DBNull.Value : Adress_TextBox.Text);
                cmd.Parameters.AddWithValue("@ville", string.IsNullOrWhiteSpace(Ville_ComboBox.Text) ? (object)DBNull.Value : Ville_ComboBox.Text);
                cmd.Parameters.AddWithValue("@patente", string.IsNullOrWhiteSpace(Patente_TextBox.Text) ? (object)DBNull.Value : Patente_TextBox.Text);
                cmd.Parameters.AddWithValue("@pays", string.IsNullOrWhiteSpace(Pays_TextBox.Text) ? (object)DBNull.Value : Pays_TextBox.Text);

                // Convert numeric fields
                cmd.Parameters.AddWithValue("@ice", string.IsNullOrWhiteSpace(ICE_TextBox.Text) ? (object)DBNull.Value : long.Parse(ICE_TextBox.Text));
                cmd.Parameters.AddWithValue("@rc", string.IsNullOrWhiteSpace(RegistreC_TextBox.Text) ? (object)DBNull.Value : int.Parse(RegistreC_TextBox.Text));
                cmd.Parameters.AddWithValue("@if", string.IsNullOrWhiteSpace(IF_TextBox.Text) ? (object)DBNull.Value : int.Parse(IF_TextBox.Text));

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Client ajouté avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearAllFields();
                    LoadClientsToListView(); // Refresh the ListView
                }
                else
                {
                    MessageBox.Show("Erreur lors de l'ajout du client.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Erreur de base de données: {sqlEx.Message}", "Erreur SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur inattendue: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void ClearAllFields()
        {
            // Clear all textboxes (ClientId will be auto-generated, so user input is ignored)
            ClientID_TextBox.Clear();
            NomR_TextBox.Clear();
            TypeS_TextBox.Clear();
            Nom_TextBox.Clear();
            Prenom_TextBox.Clear();
            Tel_TextBox.Clear();
            Portable_TextBox.Clear();
            Fax_TextBox.Clear();
            RS_TextBox.Clear();
            Email_TextBox.Clear();
            Adress_TextBox.Clear();
            Ville_ComboBox.SelectedIndex = -1;
            Patente_TextBox.Clear();
            ICE_TextBox.Clear();
            RegistreC_TextBox.Clear();
            IF_TextBox.Clear();
            Pays_TextBox.Clear();
        }

        private void LoadClientsToListView()
        {
            try
            {
                listView.Items.Clear();
                clients.Clear(); // Clear the existing list

                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                // Load all client data to populate the Client objects
                SqlCommand cmd = new SqlCommand(@"SELECT ClientId, NomResp, TypeSociete, Nom, Prenom, 
                                                Tel, Portable, Fax, RS, Email, Adresse, Ville, 
                                                Patente, ICE, RC, [IF], Pays 
                                         FROM InfoClients", connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // Create Client object with all data
                    Client client = new Client(
                        Convert.ToInt32(reader["ClientId"]),
                        reader["NomResp"]?.ToString(),
                        reader["TypeSociete"]?.ToString(),
                        reader["Nom"]?.ToString(),
                        reader["Prenom"]?.ToString(),
                        reader["Tel"]?.ToString(),
                        reader["Portable"]?.ToString(),
                        reader["Fax"]?.ToString(),
                        reader["RS"]?.ToString(),
                        reader["Email"]?.ToString(),
                        reader["Adresse"]?.ToString(),
                        reader["Ville"]?.ToString(),
                        reader["Patente"]?.ToString(),
                        reader["ICE"]?.ToString(),
                        reader["RC"]?.ToString(),
                        reader["IF"]?.ToString(),
                        reader["Pays"]?.ToString()
                    );

                    // Add client to the list
                    clients.Add(client);

                    // Create ListView item (display only selected fields)
                    ListViewItem item = new ListViewItem(client.ClientId.ToString());
                    item.SubItems.Add(client.RS);
                    item.SubItems.Add(client.NomResp);
                    item.SubItems.Add(client.Tel);
                    item.SubItems.Add(client.Portable);
                    item.SubItems.Add(client.Fax);
                    item.SubItems.Add(client.Email);
                    item.SubItems.Add(client.Adresse);

                    // Store the client index in the Tag property for easy retrieval
                    item.Tag = clients.Count - 1;

                    listView.Items.Add(item);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des clients: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void Query_TextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // If search box is empty, display all data
                if (string.IsNullOrWhiteSpace(Query_TextBox.Text))
                {
                    LoadClientsToListView();
                    return;
                }

                // Check if a search field is selected
                if (Query_ComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un champ de recherche.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected field
                string selectedField = Query_ComboBox.SelectedItem.ToString();
                string query = "%" + Query_TextBox.Text.Trim() + "%";
                string sql = "";

                // Determine the SQL query based on selected field
                switch (selectedField)
                {
                    case "Client Id":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE ClientId LIKE @query";
                        break;
                    case "Raison Sociale":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE RS LIKE @query";
                        break;
                    case "Num Responsable":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE NomResp LIKE @query";
                        break;
                    case "Tel":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Tel LIKE @query";
                        break;
                    case "Portable":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Portable LIKE @query";
                        break;
                    case "Fax":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Fax LIKE @query";
                        break;
                    case "Email":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Email LIKE @query";
                        break;
                    case "Adress":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Adresse LIKE @query";
                        break;
                    case "ICE":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE ICE LIKE @query";
                        break;
                    case "IF":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE [IF] LIKE @query";
                        break;
                    case "RC":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE RC LIKE @query";
                        break;
                    case "Patente":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Patente LIKE @query";
                        break;
                    case "Ville":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Ville LIKE @query";
                        break;
                    case "Pays":
                        sql = "SELECT ClientId, RS, NomResp, Tel, Portable, Fax, Email, Adresse FROM InfoClients WHERE Pays LIKE @query";
                        break;
                    default:
                        MessageBox.Show("Champ de recherche non valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }
                // Execute the search query and populate ListView
                SearchAndPopulateListView(sql, query);
            }
            catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();

                MessageBox.Show($"Erreur lors de la recherche: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchAndPopulateListView(string sql, string query)
        {
            try
            {
                listView.Items.Clear();
                connection.Open();

                SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@query", query);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["ClientId"].ToString());
                    item.SubItems.Add(reader["RS"].ToString());
                    item.SubItems.Add(reader["NomResp"].ToString());
                    item.SubItems.Add(reader["Tel"].ToString());
                    item.SubItems.Add(reader["Portable"].ToString());
                    item.SubItems.Add(reader["Fax"].ToString());
                    item.SubItems.Add(reader["Email"].ToString());
                    item.SubItems.Add(reader["Adresse"].ToString());

                    listView.Items.Add(item);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void Query_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear the search textbox when field selection changes
            Query_TextBox.Clear();
            LoadClientsToListView(); // Show all data
        }

        private void ModifierC_Tile_Click(object sender, EventArgs e)
        {
            // Check if a client is selected
            if (selectedClient == null)
            {
                MessageBox.Show("Veuillez sélectionner un client à modifier.", "Aucun client sélectionné",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateAllFields()) return;

            try
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = @"UPDATE [InfoClients] SET 
                           NomResp = @nomResp, 
                           TypeSociete = @typeSociete, 
                           Nom = @nom, 
                           Prenom = @prenom, 
                           Tel = @tel, 
                           Portable = @portable, 
                           Fax = @fax, 
                           RS = @rs, 
                           Email = @email, 
                           Adresse = @adresse, 
                           Ville = @ville, 
                           Patente = @patente, 
                           ICE = @ice, 
                           RC = @rc, 
                           [IF] = @if, 
                           Pays = @pays 
                           WHERE ClientId = @clientId";

                // Add parameters for all fields
                cmd.Parameters.AddWithValue("@nomResp", string.IsNullOrWhiteSpace(NomR_TextBox.Text) ? (object)DBNull.Value : NomR_TextBox.Text);
                cmd.Parameters.AddWithValue("@typeSociete", string.IsNullOrWhiteSpace(TypeS_TextBox.Text) ? (object)DBNull.Value : TypeS_TextBox.Text);
                cmd.Parameters.AddWithValue("@nom", string.IsNullOrWhiteSpace(Nom_TextBox.Text) ? (object)DBNull.Value : Nom_TextBox.Text);
                cmd.Parameters.AddWithValue("@prenom", string.IsNullOrWhiteSpace(Prenom_TextBox.Text) ? (object)DBNull.Value : Prenom_TextBox.Text);
                cmd.Parameters.AddWithValue("@tel", string.IsNullOrWhiteSpace(Tel_TextBox.Text) ? (object)DBNull.Value : Tel_TextBox.Text);
                cmd.Parameters.AddWithValue("@portable", string.IsNullOrWhiteSpace(Portable_TextBox.Text) ? (object)DBNull.Value : Portable_TextBox.Text);
                cmd.Parameters.AddWithValue("@fax", string.IsNullOrWhiteSpace(Fax_TextBox.Text) ? (object)DBNull.Value : Fax_TextBox.Text);
                cmd.Parameters.AddWithValue("@rs", string.IsNullOrWhiteSpace(RS_TextBox.Text) ? (object)DBNull.Value : RS_TextBox.Text);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(Email_TextBox.Text) ? (object)DBNull.Value : Email_TextBox.Text);
                cmd.Parameters.AddWithValue("@adresse", string.IsNullOrWhiteSpace(Adress_TextBox.Text) ? (object)DBNull.Value : Adress_TextBox.Text);
                cmd.Parameters.AddWithValue("@ville", string.IsNullOrWhiteSpace(Ville_ComboBox.Text) ? (object)DBNull.Value : Ville_ComboBox.Text);
                cmd.Parameters.AddWithValue("@patente", string.IsNullOrWhiteSpace(Patente_TextBox.Text) ? (object)DBNull.Value : Patente_TextBox.Text);
                cmd.Parameters.AddWithValue("@pays", string.IsNullOrWhiteSpace(Pays_TextBox.Text) ? (object)DBNull.Value : Pays_TextBox.Text);
                cmd.Parameters.AddWithValue("@ice", string.IsNullOrWhiteSpace(ICE_TextBox.Text) ? (object)DBNull.Value : ICE_TextBox.Text);
                cmd.Parameters.AddWithValue("@rc", string.IsNullOrWhiteSpace(RegistreC_TextBox.Text) ? (object)DBNull.Value : RegistreC_TextBox.Text);
                cmd.Parameters.AddWithValue("@if", string.IsNullOrWhiteSpace(IF_TextBox.Text) ? (object)DBNull.Value : IF_TextBox.Text);

                // The WHERE clause parameter
                cmd.Parameters.AddWithValue("@clientId", selectedClient.ClientId);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Client modifié avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearAllFields();
                    LoadClientsToListView(); // Refresh the ListView and client list
                    selectedClient = null; // Reset selection
                }
                else
                {
                    MessageBox.Show("Aucun client n'a été modifié. Vérifiez que le client existe encore.", "Attention",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Erreur de base de données: {sqlEx.Message}", "Erreur SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur inattendue: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void ClearSelection()
        {

            ClearAllFields();
            selectedClient = null;
        }
        private Client GetClientById(int clientId)
        {
            return clients.FirstOrDefault(c => c.ClientId == clientId);
        }

        private void listView_Click(object sender, EventArgs e)
        {
            ClientID_TextBox.ReadOnly = true;
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView.SelectedItems[0];
                int clientIndex = (int)selectedItem.Tag;

                selectedClient = clients[clientIndex];

                // Populate all fields with client data
                PopulateFieldsWithClientData(selectedClient);
            }
        }
        private void PopulateFieldsWithClientData(Client client)
        {
            ClientID_TextBox.Text = client.ClientId.ToString();
            NomR_TextBox.Text = client.NomResp;
            TypeS_TextBox.Text = client.TypeSociete;
            Nom_TextBox.Text = client.Nom;
            Prenom_TextBox.Text = client.Prenom;
            Tel_TextBox.Text = client.Tel;
            Portable_TextBox.Text = client.Portable;
            Fax_TextBox.Text = client.Fax;
            RS_TextBox.Text = client.RS;
            Email_TextBox.Text = client.Email;
            Adress_TextBox.Text = client.Adresse;
            Ville_ComboBox.Text = client.Ville;
            Patente_TextBox.Text = client.Patente;
            ICE_TextBox.Text = client.ICE;
            RegistreC_TextBox.Text = client.RC;
            IF_TextBox.Text = client.IF;
            Pays_TextBox.Text = client.Pays;
        }

        private void Vider_Tile_Click(object sender, EventArgs e)
        {
            ClearAllFields();
            ClientID_TextBox.ReadOnly = false;
        }
    }
}
