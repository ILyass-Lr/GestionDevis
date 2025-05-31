using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Text.RegularExpressions;
using GestionClientFactures.Repositories;
using System.Data.SqlClient;
using BaseClient;
using GestionClient;
namespace GestionClientFactures

{
    public partial class GestionDevis : MetroForm
    {
        private FactureRepository FactureRepo;
        private ClientRepository ClientRepo;
        private DevisRepository DevisRepo;
        SqlConnection Conn;
        GestionClient GestionClient;
        CultureInfo culture;
        List<Client> Clients;
        List<Devis> DevisList;
        public GestionDevis()
        {
            Conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\lenovo\\Desktop\\GI\\S8\\Dot Net Technologies\\TP5-6\\GestionClientFactures\\Database.mdf\";Integrated Security=True");
            FactureRepo = new FactureRepository(Conn);
            ClientRepo = new ClientRepository(Conn);
            DevisRepo = new DevisRepository(Conn);
            Clients = new List<Client>();
            DevisList = DevisRepo.GetAllDevis();
            culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberGroupSeparator = " ";
            culture.NumberFormat.NumberDecimalSeparator = ",";
            InitializeComponent();

        }

        private void Facture_Load(object sender, EventArgs e)
        {
            Filtrer_TextBox.Focus();
            Devis_TextBox.Text = DevisRepo.GenerateDevisN();
            LoadRS();
            LoadDevis();
        }

        private void LoadRS()
        {
            Conn.Open();
            SqlCommand cmd = Conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT DISTINCT RS FROM [InfoClients]";
            RS_ComboBox.Items.Clear();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string rs = reader.GetString(0);
                    RS_ComboBox.Items.Add(rs);
                }
            }
            Conn.Close();

            if (RS_ComboBox.Items.Count > 0)
            {
                RS_ComboBox.SelectedIndex = 0;
            }
        }

        private void LoadDevis()
        {
            Devis_ListView.Clear();
            DevisList = DevisRepo.GetAllDevis();
            foreach (Devis devis in DevisList)
            {
                string[] row = new string[]
                {
                    devis.Status.ToString(),
                    devis.Number,
                    devis.RS,
                    devis.Quantity.ToString(),
                    devis.MontantHT.ToString("N2", culture),
                    devis.MontantTVA.ToString("N2", culture),
                    (devis.MontantHT + devis.MontantTVA).ToString("N2", culture),
                    devis.Date.ToString("dd/MM/yyyy"),
                };
                Devis_ListView.Items.Add(new ListViewItem(row));
            }
        }

        private void Design_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '_' && e.KeyChar != '-')
            {
                e.Handled = true;
            }
        }


        private void Qtt_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }

        }

        private void Prix_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits, backspace, and decimal separator
            char decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

            if (!char.IsDigit(e.KeyChar) &&
                e.KeyChar != (char)Keys.Back &&
                e.KeyChar != decimalSeparator)
            {
                e.Handled = true;
            }

            // Prevent multiple decimal separators
            if (e.KeyChar == decimalSeparator && Prix_TextBox.Text.Contains(decimalSeparator))
            {
                e.Handled = true;
            }
        }

        private void Ref_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        public bool ValidateDesignation(string input)
        {
            string pattern = @"^[A-Za-z][A-Za-z0-9_-]*$";
            return Regex.IsMatch(input, pattern);
        }

        private void Design_TextBox_Leave(object sender, EventArgs e)
        {
            if (!ValidateDesignation(Design_TextBox.Text))
            {
                error_Label.Text = "La désignation doit commencer par une lettre et peut contenir des lettres, \ndes chiffres, des tirets ou des underscores.";
            }
            else
            {
                error_Label.Text = string.Empty;
            }
        }


        private void Ajouter_Tile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Design_TextBox.Text) ||
                string.IsNullOrWhiteSpace(Qtt_TextBox.Text) ||
                string.IsNullOrWhiteSpace(Prix_TextBox.Text) ||
                string.IsNullOrWhiteSpace(Ref_TextBox.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ValidateDesignation(Design_TextBox.Text))
            {
                error_Label.Text = "La désignation doit commencer par une lettre et peut contenir des lettres, des chiffres, des tirets ou des underscores.";
                return;
            }

            Facture facture = new Facture
            {
                Designation = Design_TextBox.Text,
                Quantity = int.Parse(Qtt_TextBox.Text),
                Prix = float.Parse(Prix_TextBox.Text, CultureInfo.InvariantCulture),
                Tva = (tva20_RadioButton.Checked) ? 20 : 7,
                Reference = Ref_TextBox.Text,
                Date = DateTime.Now,
                ClientId = int.Parse(Id_TextBox.Text)
            };

            FactureRepo.InertFacture(facture);
            LoadProduits();
            Clear();
            Filtrer_TextBox.Focus();

        }



        public void LoadProduits()
        {
            Produit_ListView.Items.Clear();

            List<Facture> listeFactures = FactureRepo.GetAllFactures();

            foreach (Facture facture in listeFactures)
            {
                float montantHT = facture.Quantity * facture.Prix;
                float montantTVA = montantHT * (facture.Tva / 100f);
                float totalTTC = montantHT + montantTVA;

                string[] row = new string[]
                {
            facture.DevisNbr,
            facture.Designation,
            facture.Prix.ToString("N2", culture),
            facture.Quantity.ToString(),
            montantHT.ToString("N2", culture),
            montantTVA.ToString("N2", culture),
            totalTTC.ToString("N2", culture),
            facture.Reference
                };

                Produit_ListView.Items.Add(new ListViewItem(row));
            }
        }
        private void Clear()
        {
            Design_TextBox.Clear();
            Qtt_TextBox.Clear();
            Prix_TextBox.Clear();
            Ref_TextBox.Clear();
        }



        private void Supprimer_Tile_Click(object sender, EventArgs e)
        {
            // Check if any item is selected
            if (Produit_ListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un produit à supprimer.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm deletion
            DialogResult result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer {Produit_ListView.SelectedItems.Count} produit(s) ?",
                "Confirmer la suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {

                FactureRepo.DeleteByReference(Produit_ListView.SelectedItems[0].SubItems[7].Text);
                LoadProduits();
            }
        }

        private void RS_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RS_ComboBox.SelectedItem == null)
                return;
            Client_ComboBox.Items.Clear();
            string selectedRS = RS_ComboBox.SelectedItem.ToString();
            Clients = ClientRepo.GetClientsByRS(selectedRS);
            foreach (Client client in Clients)
            {
                Client_ComboBox.Items.Add(client.Nom + " " + client.Prenom);
            }
            if (Client_ComboBox.Items.Count > 0)
            {
                Client_ComboBox.SelectedIndex = 0;
                Id_TextBox.Text = Clients[0].ClientId.ToString();
                IF_TextBox.Text = Clients[0].IF;
                ICE_TextBox.Text = Clients[0].ICE;
            }
        }

        private void Client_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Client client = Clients[Client_ComboBox.SelectedIndex];
            Id_TextBox.Text = client.ClientId.ToString();
            IF_TextBox.Text = client.IF ?? string.Empty;
            ICE_TextBox.Text = client.ICE ?? string.Empty;
        }

        private void NouveauC_Tile_Click(object sender, EventArgs e)
        {
            var form = new InformationClient(); // Corrected: Removed 'GestionClient.' prefix  
            form.ShowDialog();
            if(form.DialogResult == DialogResult.OK)
            {
                // Reload the RS and Client ComboBoxes after adding a new client
                LoadRS();
                RS_ComboBox.SelectedIndex = 0; // Select the first RS by default
                Client_ComboBox.Items.Clear(); // Clear previous clients
               // Reload RS to populate clients
               
            }
        }
    }
}
