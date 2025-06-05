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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ClosedXML.Excel;
namespace GestionClient
{
    public partial class InformationClient : MetroForm
    {
        public event Action Synchronisation;
        readonly private SqlConnection Conn;
        readonly private ClientRepository ClientRepo;
        private List<Client> Clients = new List<Client>();
        private List<Client> FilteredClients = new List<Client>();
        private Client SelectedClient = null;

        // CONSTRUCTEUR ET INITIALISATION
        public InformationClient()
        {
            InitializeComponent();
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\lenovo\Desktop\GI\S8\Dot Net Technologies\TP5-6\GestionClientFactures\Database.mdf"";Integrated Security=True";
            Conn = new SqlConnection(connectionString);
            ClientRepo = new ClientRepository(Conn);
        }

        private void InformationClient_Load(object sender, EventArgs e)
        {
            // Événements pour le custom drawing
            ListView.DrawColumnHeader += MyListView_DrawColumnHeader;
            ListView.DrawItem += MyListView_DrawItem;
            ListView.DrawSubItem += MyListView_DrawSubItem;
            // Initialiser la ListView
            LoadClients();
        }


        /*
        * Ces méthodes sont utilisées pour personnaliser l'apparence de la ListView.
        * Elles permettent de dessiner les en-têtes de colonnes, les éléments et les sous-éléments.
        */
        private void MyListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void MyListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void MyListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(255, 0, 174, 219))) // Set your color here
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                e.Graphics.DrawString(e.Header.Text, ListView.Font, textBrush, e.Bounds, sf);
            }
        }


        /*        
         * Ces méthodes sont utilisées pour gérer les événements de saisie des TextBox.
         * Elles permettent de valider les entrées de l'utilisateur en temps réel.
         */
        private void ICE_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = "";
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back && !char.IsControl(e.KeyChar))
            {
                error_Label.Text = "L'ICE ne peut contenir que des chiffres.";
                e.Handled = true;
                return;
            }

            // Limit to 15 digits maximum
            if (sender is Control control && char.IsDigit(e.KeyChar) && control.Text.Length >= 15)
            {
                error_Label.Text = "L'ICE ne peut pas dépasser 15 chiffres.";
                e.Handled = true;
            }
        }

        private void RegistreC_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = "";
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back && !char.IsControl(e.KeyChar))
            {
                error_Label.Text = "Le Registre de Commerce ne peut contenir que des chiffres.";
                e.Handled = true;
                return;
            }

            // Limit to 5 digits maximum
            if (sender is Control control && char.IsDigit(e.KeyChar) && control.Text.Length >= 5)
            {
                error_Label.Text = "Le Registre de Commerce ne peut pas dépasser 5 chiffres.";
                e.Handled = true;
            }
        }

        private void IF_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = "";
            // Only allow digits and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back && !char.IsControl(e.KeyChar))
            {
                error_Label.Text = "L'Identifiant Fiscal ne peut contenir que des chiffres.";
                e.Handled = true;
                return;
            }

            // Safely cast to get the text length
            if (sender is Control control && char.IsDigit(e.KeyChar) && control.Text.Length >= 8)
            {
                error_Label.Text = "L'Identifiant Fiscal ne peut pas dépasser 8 chiffres.";
                e.Handled = true;
            }
        }

        private void Patente_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = "";
            if (!char.IsLetterOrDigit(e.KeyChar) &&
                e.KeyChar != (char)Keys.Back &&
                e.KeyChar != ' ' &&
                e.KeyChar != '-' &&
                e.KeyChar != '_'
                 && !char.IsControl(e.KeyChar)
                )
            {
                error_Label.Text = "La Patente ne peut contenir que des lettres, des chiffres, des espaces, des tirets et des underscores.";
                e.Handled = true;
            }
        }

        private void Tel_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = "";
            // Allow digits, spaces, +, -, (, ), and backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back &&
                e.KeyChar != ' ' && e.KeyChar != '+' && e.KeyChar != '-' &&
                e.KeyChar != '(' && e.KeyChar != ')'
                 && !char.IsControl(e.KeyChar)
                )
            {
                error_Label.Text = "Le numéro de téléphone ne peut contenir que des chiffres, des espaces, des tirets, des parenthèses et le signe +.";
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
            error_Label.Text = "";
            // Allow most characters for email, restrict some special characters
            if (e.KeyChar == ' ' || e.KeyChar == '\t' && !char.IsControl(e.KeyChar))
            {
                error_Label.Text = "L'email ne peut pas contenir d'espaces.";
                e.Handled = true;
            }
        }

        private void Nom_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = "";
            // Allow letters, spaces, hyphens, apostrophes
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != (char)Keys.Back &&
                e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '\'' && !char.IsControl(e.KeyChar))
            {
                error_Label.Text = "Le nom ne peut contenir que des lettres, des espaces, des tirets et des apostrophes.";
                e.Handled = true;
            }
        }

        private void Prenom_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Same as Nom_TextBox
            Nom_TextBox_KeyPress(sender, e);
        }

        private void RS_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) &&
                e.KeyChar != '-' && e.KeyChar != '_' && e.KeyChar != '\'' && e.KeyChar != '.')
            {
                error_Label.Text = "La Raison Sociale ne peut contenir que des lettres, des espaces, des tirets, des underscores, des apostrophes et des points.";
                e.Handled = true;
            }
        }


        /*         
         * Ces méthodes sont utilisées pour valider les champs spécifiques.
         * Elles vérifient si les entrées respectent les formats requis.
         */
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


        // VAALIDATION DES CHAMPS OBLIGATOIRES
        private bool ValidateRequiredField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show($"Le champ {fieldName} est obligatoire.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }


        // VALIDATION DE TOUS LES CHAMPS
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

            if (!ValidatePatente(Patente_TextBox.Text))
            {
                MessageBox.Show("Le champ Patente est obligatoire.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Patente_TextBox.Focus();
                return false;
            }


            return true;
        }


        /*        
         * Ces méthodes sont utilisées pour ajouter et modifier des clients.
         * Elles vérifient d'abord si tous les champs sont valides avant de procéder à l'ajout ou à la modification.
         * Après l'ajout ou la modification, elles mettent à jour la ListView (filtré ou non selon si le filtre est active ou non)
         * et effacent les champs.
         */
        private void AjouterC_Tile_Click(object sender, EventArgs e)
        {
            if (!ValidateAllFields()) return;

            Client client = new Client(
                (string.IsNullOrEmpty(ClientID_TextBox.Text)) ? -1 : int.Parse(ClientID_TextBox.Text),
                NomR_TextBox.Text,
                TypeS_TextBox.Text,
                Nom_TextBox.Text,
                Prenom_TextBox.Text,
                Tel_TextBox.Text,
                Portable_TextBox.Text,
                Fax_TextBox.Text,
                RS_TextBox.Text,
                Email_TextBox.Text,
                Adress_TextBox.Text,
                Ville_ComboBox.Text,
                Patente_TextBox.Text,
                ICE_TextBox.Text,
                RegistreC_TextBox.Text,
                IF_TextBox.Text,
                Pays_TextBox.Text
             );
            ClientRepo.InsertClient(client);

            if (string.IsNullOrEmpty(Query_TextBox.Text))
            {
                LoadClients();
            }
            else
            {
                LoadFilteredClients();
            }
            ClearAllFields();
            ClientID_TextBox.Focus();
            Synchronisation.Invoke();
        }


        /*         
         * Cette méthode est utilisée pour modifier un client sélectionné.
         * Elle vérifie d'abord si un client est sélectionné, puis valide les champs avant de procéder à la modification.
         * Après la modification, elle met à jour la ListView (filtré ou non selon si le filtre est active ou non)
         * et efface les champs.
         */
        private void ModifierC_Tile_Click(object sender, EventArgs e)
        {
            // Check if a client is selected
            if (SelectedClient == null)
            {
                MessageBox.Show("Veuillez sélectionner un client à modifier.", "Aucun client sélectionné",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
         
            // Vérifier si le client existe en base
            var existingClient = ClientRepo.GetClientById(SelectedClient.ClientId);
            if (existingClient == null)
            {
                MessageBox.Show("Le client sélectionné n'existe pas en base de données!", "Erreur");
                return;
            }


            if (!ValidateAllFields()) return;

            // Créer le client avec les nouvelles données
            Client updatedClient = new Client(
                SelectedClient.ClientId,
                NomR_TextBox.Text,
                TypeS_TextBox.Text,
                Nom_TextBox.Text,
                Prenom_TextBox.Text,
                Tel_TextBox.Text,
                Portable_TextBox.Text,
                Fax_TextBox.Text,
                RS_TextBox.Text,
                Email_TextBox.Text,
                Adress_TextBox.Text,
                Ville_ComboBox.Text,
                Patente_TextBox.Text,
                ICE_TextBox.Text,
                RegistreC_TextBox.Text,
                IF_TextBox.Text,
                Pays_TextBox.Text
            );

            // Tenter la mise à jour
            bool success = ClientRepo.UpdateClient(updatedClient);

            if (success)
            {
                MessageBox.Show("Client mis à jour avec succès!", "Succès",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearAllFields();

                if (string.IsNullOrEmpty(Query_TextBox.Text))
                {
                    LoadClients();
                }
                else
                {
                    LoadFilteredClients();
                }
            }
            else
            {
                MessageBox.Show("Échec de la mise à jour du client.", "Erreur",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*  
         *  Mise à jour de la ListView
         *  LoadClients() : appelle la table [InfoClients] de la BD pour afficher tous les clients   
         *  LoadFilteredClients() : appelle la table [InfoClients] de la BD pour afficher les clients filtrés en fonction du champ sélectionné et de la requête saisie.
         */
        private void LoadClients()
        {
            ListView.Items.Clear();
            Clients.Clear();
            Clients = ClientRepo.GetAllClients();
            foreach (Client client in Clients)
            {
                ListViewItem item = new ListViewItem(client.ClientId.ToString());
                item.SubItems.Add(client.RS);
                item.SubItems.Add(client.NomResp);
                item.SubItems.Add(client.Tel);
                item.SubItems.Add(client.Portable);
                item.SubItems.Add(client.Fax);
                item.SubItems.Add(client.Email);
                item.SubItems.Add(client.Adresse);

                // Store the client index in the Tag property for easy retrieval
                item.Tag = client;

                ListView.Items.Add(item);
            }
            if (ListView.Items.Count > 0)
            {
                ListView.Items[0].Selected = true; // Select the first item by default
            }
            else
            {
                ClearSelection(); // Clear fields if no clients are found
            }
        }

        private void LoadFilteredClients()
        {
            string selectedField = Query_ComboBox.SelectedItem.ToString();
            string query = Query_TextBox.Text.Trim().ToLower();

            switch (selectedField)
            {
                case "Client Id":
                    FilteredClients = Clients.Where(c => c.ClientId.ToString().Contains(query)).ToList();
                    break;
                case "Raison Sociale":
                    FilteredClients = Clients.Where(c => c.RS.ToLower().Contains(query)).ToList();
                    break;
                case "Nom Responsable":
                    FilteredClients = Clients.Where(c => c.NomResp.ToLower().Contains(query)).ToList();
                    break;
                case "Tel":
                    FilteredClients = Clients.Where(c => c.Tel.Contains(query)).ToList();
                    break;
                case "Portable":
                    FilteredClients = Clients.Where(c => c.Portable.Contains(query)).ToList();
                    break;
                case "Fax":
                    FilteredClients = Clients.Where(c => c.Fax.Contains(query)).ToList();
                    break;
                case "Email":
                    FilteredClients = Clients.Where(c => c.Email.ToLower().Contains(query)).ToList();
                    break;
                case "Adress":
                    FilteredClients = Clients.Where(c => c.Adresse.ToLower().Contains(query)).ToList();
                    break;
                case "ICE":
                    FilteredClients = Clients.Where(c => c.ICE.Contains(query)).ToList();
                    break;
                case "IF":
                    FilteredClients = Clients.Where(c => c.IF.Contains(query)).ToList();
                    break;
                case "RC":
                    FilteredClients = Clients.Where(c => c.RC.Contains(query)).ToList();
                    break;
                case "Patente":
                    FilteredClients = Clients.Where(c => c.Patente.ToLower().Contains(query)).ToList();
                    break;
                case "Ville":
                    FilteredClients = Clients.Where(c => c.Ville.ToLower().Contains(query)).ToList();
                    break;
                case "Pays":
                    FilteredClients = Clients.Where(c => c.Pays.ToLower().Contains(query)).ToList();
                    break;
                default:
                    MessageBox.Show("Champ de recherche non valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            // Clear the ListView and repopulate it with filtered clients
            ListView.Items.Clear();
            foreach (Client client in FilteredClients)
            {
                ListViewItem item = new ListViewItem(client.ClientId.ToString());
                item.SubItems.Add(client.RS);
                item.SubItems.Add(client.NomResp);
                item.SubItems.Add(client.Tel);
                item.SubItems.Add(client.Portable);
                item.SubItems.Add(client.Fax);
                item.SubItems.Add(client.Email);
                item.SubItems.Add(client.Adresse);
                // Store the client index in the Tag property for easy retrieval
                item.Tag = client;
                ListView.Items.Add(item);
            }

        }


        /* 
         * Méthodes pour gérer le filtrage des clients
         * Query_TextBox_TextChanged : déclenchée lorsque le texte de la zone de recherche change.
         * Query_ComboBox_SelectedIndexChanged : déclenchée lorsque l'utilisateur change le champ de recherche sélectionné.
         */
        private void Query_TextBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Query_TextBox.Text))
            {
                LoadClients();
                return;
            }

            if (Query_ComboBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un champ de recherche.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadFilteredClients();

        }

        private void Query_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear the search textbox when field selection changes
            Query_TextBox.Clear();
            LoadClients(); // Show all data
        }


        /*
         * Méthodes pour gérer la sélection d'un client dans la ListView
         * et son affichage dans les champs de saisie.
         */
        private void ListView_Click(object sender, EventArgs e)
        {
            ClientID_TextBox.ReadOnly = true;
            ClientID_TextBox.Enabled = false;
            if (ListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = ListView.SelectedItems[0];
                SelectedClient = (Client)selectedItem.Tag; // Retrieve the client from the selected item


                // Populate all fields with client data
                PopulateFieldsWithClientData(SelectedClient);
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


        /*
         * Méthodes pour le nettoyage des champs et la désélection des clients.
         */
        private void Vider_Tile_Click(object sender, EventArgs e)
        {
            ClearAllFields();
            ClientID_TextBox.ReadOnly = false;
        }

        private void ClearSelection()
        {

            ClearAllFields();
            SelectedClient = null;
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


        /*
         * Méthodes pour exporter les données des clients vers un fichier Excel.
         * Le fichier Excel contient tous les données des clients et non seulement des données filtrées.
         * Le fichier Excel inclut un sheet ayant des statistiques sur les clients
         */
        private void Exporter_Tile_Click(object sender, EventArgs e)
        {
            try
            {
                // Récupérer la liste des clients
                Clients = ClientRepo.GetAllClients(); // Remplacez par votre méthode

                if (Clients == null || Clients.Count == 0)
                {
                    MessageBox.Show("Aucun client à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Exporter avec ClosedXML
                ExportClientsDetailedToExcel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exportation : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportClientsDetailedToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                // Feuille principale avec les clients
                var clientsSheet = workbook.Worksheets.Add("Liste des Clients");
                CreateClientsSheet(clientsSheet);

                // Feuille avec les statistiques
                var statsSheet = workbook.Worksheets.Add("Statistiques");
                CreateStatsSheet(statsSheet);

                // Sauvegarder
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Sauvegarder le rapport complet des clients",
                    FileName = $"Rapport_Clients_Complet_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show($"Export détaillé réussi ! {Clients.Count} clients exportés avec statistiques.",
                                  "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
        }

        private void CreateClientsSheet(IXLWorksheet worksheet)
        {
            // Titre
            worksheet.Cell(1, 1).Value = "LISTE DES CLIENTS";
            worksheet.Range("A1:Q1").Merge().Style.Font.Bold = true;
            worksheet.Range("A1:Q1").Style.Font.SetFontSize(16);
            worksheet.Range("A1:Q1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("A1:Q1").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            worksheet.Range("A1:Q1").Style.Font.FontColor = XLColor.White;

            // Date d'export
            worksheet.Cell(2, 1).Value = $"Exporté le : {DateTime.Now:dd/MM/yyyy à HH:mm}";
            worksheet.Cell(3, 1).Value = $"Nombre total de clients : {Clients.Count}";

            // En-têtes (ligne 5)
            int headerRow = 5;
            worksheet.Cell(headerRow, 1).Value = "ID";
            worksheet.Cell(headerRow, 2).Value = "Nom Responsable";
            worksheet.Cell(headerRow, 3).Value = "Type Société";
            worksheet.Cell(headerRow, 4).Value = "Nom";
            worksheet.Cell(headerRow, 5).Value = "Prénom";
            worksheet.Cell(headerRow, 6).Value = "Téléphone";
            worksheet.Cell(headerRow, 7).Value = "Portable";
            worksheet.Cell(headerRow, 8).Value = "Fax";
            worksheet.Cell(headerRow, 9).Value = "Raison Sociale";
            worksheet.Cell(headerRow, 10).Value = "Email";
            worksheet.Cell(headerRow, 11).Value = "Adresse";
            worksheet.Cell(headerRow, 12).Value = "Ville";
            worksheet.Cell(headerRow, 13).Value = "Patente";
            worksheet.Cell(headerRow, 14).Value = "ICE";
            worksheet.Cell(headerRow, 15).Value = "RC";
            worksheet.Cell(headerRow, 16).Value = "IF";
            worksheet.Cell(headerRow, 17).Value = "Pays";

            // Style des en-têtes
            var headerRange = worksheet.Range($"A{headerRow}:Q{headerRow}");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Données des clients
            for (int i = 0; i < Clients.Count; i++)
            {
                var client = Clients[i];
                int row = headerRow + 1 + i;

                worksheet.Cell(row, 1).Value = client.ClientId;
                worksheet.Cell(row, 2).Value = client.NomResp ?? "";
                worksheet.Cell(row, 3).Value = client.TypeSociete ?? "";
                worksheet.Cell(row, 4).Value = client.Nom ?? "";
                worksheet.Cell(row, 5).Value = client.Prenom ?? "";
                worksheet.Cell(row, 6).Value = client.Tel ?? "";
                worksheet.Cell(row, 7).Value = client.Portable ?? "";
                worksheet.Cell(row, 8).Value = client.Fax ?? "";
                worksheet.Cell(row, 9).Value = client.RS ?? "";
                worksheet.Cell(row, 10).Value = client.Email ?? "";
                worksheet.Cell(row, 11).Value = client.Adresse ?? "";
                worksheet.Cell(row, 12).Value = client.Ville ?? "";
                worksheet.Cell(row, 13).Value = client.Patente ?? "";
                worksheet.Cell(row, 14).Value = client.ICE ?? "";
                worksheet.Cell(row, 15).Value = client.RC ?? "";
                worksheet.Cell(row, 16).Value = client.IF ?? "";
                worksheet.Cell(row, 17).Value = client.Pays ?? "";
            }

            // Ajuster les colonnes et ajouter des bordures
            worksheet.ColumnsUsed().AdjustToContents();
            var dataRange = worksheet.Range($"A{headerRow}:Q{headerRow + Clients.Count}");
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        }

        private void CreateStatsSheet(IXLWorksheet worksheet)
        {
            // Titre
            worksheet.Cell(1, 1).Value = "STATISTIQUES DES CLIENTS";
            worksheet.Range("A1:D1").Merge().Style.Font.Bold = true;
            worksheet.Range("A1:D1").Style.Font.SetFontSize(16);
            worksheet.Range("A1:D1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.Green;
            worksheet.Range("A1:D1").Style.Font.FontColor = XLColor.White;

            int currentRow = 3;

            // Statistiques générales
            worksheet.Cell(currentRow, 1).Value = "STATISTIQUES GÉNÉRALES";
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Font.Bold = true;
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Fill.BackgroundColor = XLColor.LightBlue;
            currentRow += 2;

            worksheet.Cell(currentRow, 1).Value = "Nombre total de clients :";
            worksheet.Cell(currentRow, 2).Value = Clients.Count;
            currentRow++;

            // Répartition par ville
            var villesStats = Clients.GroupBy(c => c.Ville ?? "Non spécifiée")
                                    .OrderByDescending(g => g.Count())
                                    .Take(10);

            currentRow += 2;
            worksheet.Cell(currentRow, 1).Value = "RÉPARTITION PAR VILLE (Top 10)";
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Font.Bold = true;
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Fill.BackgroundColor = XLColor.LightBlue;
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Ville";
            worksheet.Cell(currentRow, 2).Value = "Nombre de clients";
            worksheet.Range($"A{currentRow}:B{currentRow}").Style.Font.Bold = true;
            currentRow++;

            foreach (var ville in villesStats)
            {
                worksheet.Cell(currentRow, 1).Value = ville.Key;
                worksheet.Cell(currentRow, 2).Value = ville.Count();
                currentRow++;
            }

            // Répartition par pays
            var paysStats = Clients.GroupBy(c => c.Pays ?? "Non spécifié")
                                  .OrderByDescending(g => g.Count());

            currentRow += 2;
            worksheet.Cell(currentRow, 1).Value = "RÉPARTITION PAR PAYS";
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Font.Bold = true;
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Fill.BackgroundColor = XLColor.LightBlue;
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Pays";
            worksheet.Cell(currentRow, 2).Value = "Nombre de clients";
            worksheet.Range($"A{currentRow}:B{currentRow}").Style.Font.Bold = true;
            currentRow++;

            foreach (var pays in paysStats)
            {
                worksheet.Cell(currentRow, 1).Value = pays.Key;
                worksheet.Cell(currentRow, 2).Value = pays.Count();
                currentRow++;
            }

            // Répartition par type de société
            var typesStats = Clients.GroupBy(c => c.TypeSociete ?? "Non spécifié")
                                   .OrderByDescending(g => g.Count());

            currentRow += 2;
            worksheet.Cell(currentRow, 1).Value = "RÉPARTITION PAR TYPE DE SOCIÉTÉ";
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Font.Bold = true;
            worksheet.Range($"A{currentRow}:D{currentRow}").Style.Fill.BackgroundColor = XLColor.LightBlue;
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Type de Société";
            worksheet.Cell(currentRow, 2).Value = "Nombre de clients";
            worksheet.Range($"A{currentRow}:B{currentRow}").Style.Font.Bold = true;
            currentRow++;

            foreach (var type in typesStats)
            {
                worksheet.Cell(currentRow, 1).Value = type.Key;
                worksheet.Cell(currentRow, 2).Value = type.Count();
                currentRow++;
            }

            // Ajuster les colonnes
            worksheet.ColumnsUsed().AdjustToContents();
        }


    }
}
