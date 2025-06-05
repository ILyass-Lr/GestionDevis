using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Text.RegularExpressions;
using GestionClientFactures.Repositories;
using System.Data.SqlClient;
using BaseClient;
using GestionClient;
using System.IO;
using ClosedXML.Excel;
namespace GestionClientFactures

{
    public partial class GestionDevis : MetroForm
    {
        readonly private FactureRepository FactureRepo;
        readonly private ClientRepository ClientRepo;
        readonly private DevisRepository DevisRepo;
        private bool isUpdatingControls = false;
        readonly SqlConnection Conn;
        readonly CultureInfo culture;
        List<Client> Clients;
        Dictionary<string, List<Facture>> ListeFactures;
        List<Devis> DevisList;
        private string oldRef;

        // CONSTRUTEUR ET INITIATION
        public GestionDevis()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\lenovo\Desktop\GI\S8\Dot Net Technologies\TP5-6\GestionClientFactures\Database.mdf"";Integrated Security=True";
            Conn = new SqlConnection(connectionString);
            FactureRepo = new FactureRepository(Conn);
            ClientRepo = new ClientRepository(Conn);
            DevisRepo = new DevisRepository(Conn);
            Clients = new List<Client>();
            DevisList = DevisRepo.GetAllDevis();
            ListeFactures = FactureRepo.GetAllFactures();

            culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberGroupSeparator = " ";
            culture.NumberFormat.NumberDecimalSeparator = ",";
            InitializeComponent();
        }
        private void Facture_Load(object sender, EventArgs e)
        {
            this.ActiveControl = Filtrer_TextBox;
            Devis_TextBox.Text = DevisRepo.GenerateDevisN();
            Status_ComboBox.SelectedIndex = 1;


            // Activer OwnerDraw pour les deux ListView
            Produit_ListView.OwnerDraw = true;
            Devis_ListView.OwnerDraw = true;

            // Ensuite assigner les événements
            Produit_ListView.DrawColumnHeader += MyListView_DrawColumnHeader;
            Devis_ListView.DrawColumnHeader += MyListView_DrawColumnHeaderDevis;
            Devis_ListView.DrawItem += MyListView_DrawItem;
            //Devis_ListView.DrawSubItem += MyListView_DrawSubItem;
            Produit_ListView.DrawItem += MyListView_DrawItem;
            Produit_ListView.DrawSubItem += MyListView_DrawSubItem;

            LoadDevis();
            LoadRS();
            LoadProduits();
        }


        /*
         * Méthodes pour personnaliser le dessin des ListView
         * des éléments, sous-éléments et en-têtes de colonnes
         * pour la liste des produits et des devis.
         */
        private void MyListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // Toujours utiliser le dessin par défaut pour DrawItem
            // et laisser DrawSubItem gérer la personnalisation
            e.DrawDefault = true;
        }
        private void MyListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item.Selected)
            {
                // Couleur de fond pour l'élément sélectionné
                using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(255, 0, 198, 247)))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }

                // Utiliser TextRenderer pour un meilleur rendu
                TextFormatFlags flags = TextFormatFlags.Left |
                                       TextFormatFlags.VerticalCenter |
                                       TextFormatFlags.NoPrefix |
                                       TextFormatFlags.EndEllipsis;

                // Ajouter un padding
                System.Drawing.Rectangle textRect = new System.Drawing.Rectangle(
                    e.Bounds.X + 3,
                    e.Bounds.Y,
                    e.Bounds.Width - 6,
                    e.Bounds.Height
                );

                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, textRect, Color.Black, flags);
            }
            else
            {
                e.DrawDefault = true;
            }
        }
        private void MyListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(255, 0, 174, 219))) // Set your color here
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                e.Graphics.DrawString(e.Header.Text, Produit_ListView.Font, textBrush, e.Bounds, sf);
            }
        }
        private void MyListView_DrawColumnHeaderDevis(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(255, 0, 174, 219))) // Set your color here
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                e.Graphics.DrawString(e.Header.Text, Devis_ListView.Font, textBrush, e.Bounds, sf);
            }
        }


        /*
         * Méthodes pour charger les données dans les ComboBox
         * 1. LoadRS() charge les Raison Sociale (RS) dans leutre ComboBox et sélectionner le premier élément
         * 2. RS_ComboBox_SelectedIndexChanged() cest déclenché et faire appel à ClientRepo.GetClientsByRS() 
         *    pour charger les clients associés à la RS sélectionnée en sélectionnant le premier client.
         * 3. Client_ComboBox_SelectedIndexChanged() est déclenché lorsque l'utilisateur sélectionne un client
         *    et les TextBox liée au client sont mis à jour.
         */
        private void LoadRS()
        {
            RS_ComboBox.Items.Clear();
            foreach (string rs in ClientRepo.GetRS())
            {
                RS_ComboBox.Items.Add(rs);
            }

            if (RS_ComboBox.Items.Count > 0)
            {
                RS_ComboBox.SelectedIndex = 0;
                Client_ComboBox.Items.Clear();
                // Load clients based on the selected RS
                Clients = ClientRepo.GetClientsByRS(RS_ComboBox.SelectedItem.ToString());
                foreach (Client client in Clients)
                {
                    Client_ComboBox.Items.Add(client.Nom + " " + client.Prenom + " (" + client.ClientId + ")");
                }
                if (Client_ComboBox.Items.Count > 0)
                {
                    Client_ComboBox.SelectedIndex = 0;
                    Id_TextBox.Text = Clients[0].ClientId.ToString();
                    IF_TextBox.Text = Clients[0].IF ?? string.Empty;
                    ICE_TextBox.Text = Clients[0].ICE ?? string.Empty;
                }
                EnableFields(true);
            }
            else
            {
                RS_ComboBox.SelectedIndex = -1;
                Id_TextBox.Text = string.Empty;
                IF_TextBox.Text = string.Empty;
                ICE_TextBox.Text = string.Empty;
                Client_ComboBox.Items.Clear();
                RS_ComboBox.Enabled = false;
                Client_ComboBox.Enabled = false;
                EnableFields(false);

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
                Client_ComboBox.Items.Add(client.Nom + " " + client.Prenom + " (" + client.ClientId + ")");
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
            Enregistrer_Button.Enabled = CommandeEnregistre();
        }


        /*
         * Méthodes pour charger les devis dans le ListView et sélectionner le premier élément.
         * La sélection d'un élément dans le ListView déclenche la mise à jour des TextBox nécessaires
         * 
         */
      private void LoadDevis()
{
    isUpdatingControls = true;
    try
    {
        Devis_ListView.Items.Clear();
        DevisList = DevisRepo.GetAllDevis();

        foreach (Devis devis in DevisList)
        {
            ListViewItem item = new ListViewItem(devis.Status.ToString());
            item.SubItems.Add(devis.Number);
            item.SubItems.Add(devis.RS);
            item.SubItems.Add(devis.Quantity.ToString());
            item.SubItems.Add(devis.MontantHT.ToString("N2", culture));
            item.SubItems.Add(devis.MontantTVA.ToString("N2", culture));
            item.SubItems.Add((devis.MontantHT + devis.MontantTVA).ToString("N2", culture));
            item.SubItems.Add(devis.Date.ToString("dd/MM/yyyy"));

            item.Tag = devis;
            Devis_ListView.Items.Add(item);
        }
    }
    finally
    {
        isUpdatingControls = false;
    }

    // Sélection du devis correspondant au numéro dans Devis_TextBox
    if (Devis_ListView.Items.Count > 0)
    {
        string targetNumber = Devis_TextBox.Text?.Trim();
        bool found = false;

        if (!string.IsNullOrEmpty(targetNumber))
        {
            foreach (ListViewItem item in Devis_ListView.Items)
            {
                Devis devis = item.Tag as Devis;
                if (devis != null && devis.Number == targetNumber)
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible(); // Pour s'assurer que l'élément soit visible
                    found = true;
                    break;
                }
            }
        }

        // Si aucun devis correspondant n'est trouvé, sélectionner le premier élément
        if (!found)
        {
            Devis_ListView.Items[0].Selected = true;
            Devis_ListView.Items[0].Focused = true;
        }
    }
}        private void Devis_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Éviter les appels récursifs
            if (isUpdatingControls) return;

            try
            {
                isUpdatingControls = true;

                if (Devis_ListView.SelectedItems.Count > 0)
                {
                    if (!(Devis_ListView.SelectedItems[0].Tag is Devis devis))
                        return;

                    // Gestion du statut avec vérification
                    string status = GetStatusText(devis.Status);

                    // Mise à jour des contrôles avec vérifications
                    Devis_TextBox.Text = devis.Number ?? string.Empty;

                    // Sélection sécurisée du statut
                    SetComboBoxSelectionSafe(Status_ComboBox, status);

                    // Récupération du client avec gestion d'erreur
                    Client client = ClientRepo.GetClientById(devis.ClientId);
                    if (client != null)
                    {
                        // Mise à jour des informations client
                        SetComboBoxSelectionSafe(RS_ComboBox, devis.RS);
                        Id_TextBox.Text = devis.ClientId.ToString();
                        IF_TextBox.Text = client.IF ?? string.Empty;
                        ICE_TextBox.Text = client.ICE ?? string.Empty;

                        // Sélection sécurisée du client
                        string clientDisplay = $"{client.Nom} {client.Prenom} ({client.ClientId})";
                        Client_ComboBox.SelectedItem = clientDisplay; // Sélectionner le client
                        //SetComboBoxSelectionSafe(Client_ComboBox, clientDisplay);
                    }
                    else
                    {
                        // Réinitialisation si client non trouvé
                        ClearClientControls();
                    }

                    // Activation du bouton avec gestion d'erreur
                    try
                    {
                        Enregistrer_Button.Enabled = CommandeEnregistre();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur dans CommandeEnregistre: {ex.Message}");
                        Enregistrer_Button.Enabled = false;
                    }
                }
                else
                {
                    // Aucune sélection - réinitialiser l'interface
                    ClearAllControls();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sélection du devis: {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isUpdatingControls = false;
            }
        }


        /*
         *  Méthodes pour vérifer les entrées de l'utilisateur dans les TextBox
         */
        private void Design_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '_' && e.KeyChar != '-' && e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }
        private void Qtt_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                error_Label.Text = "Veuillez entrer un nombre entier pour la quantité.";
            }
            else
            {
                error_Label.Text = string.Empty;
            }
        }
        private void Prix_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits, backspace, and decimal separator
            char decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

            if (!char.IsDigit(e.KeyChar) &&
                e.KeyChar != (char)Keys.Back &&
                e.KeyChar != decimalSeparator && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                error_Label.Text = "Veuillez entrer un nombre valide pour le prix.";
            }
            else
            {
                error_Label.Text = string.Empty;
            }

            // Prevent multiple decimal separators
            if (e.KeyChar == decimalSeparator && Prix_TextBox.Text.Contains(decimalSeparator))
            {
                e.Handled = true;
            }
        }
        private void Ref_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back && !char.IsLetter(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
                error_Label.Text = "Veuillez entrer un nombre entier pour la référence.";
            }
            else
            {
                error_Label.Text = string.Empty;
            }
        }
        private void Filtrer_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            error_Label.Text = string.Empty;
            // Allow backspace and other control keys
            if (char.IsControl(e.KeyChar))
                return;

            int length = Filtrer_TextBox.Text.Length;

            if (length == 0 && char.IsLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
            else if (e.KeyChar != '/' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                error_Label.Text = "Le format doit être DXXX/YYYY, où D est une lettre, \nX sont des chiffres et YYYY est l'année.";

            }
            else if (length >= 5 && length <= 8)
            {
                // Last 4 characters must be digits (year)
                if (!char.IsDigit(e.KeyChar))
                    e.Handled = true;


            }
        }


        /*
         * Méthode pour filtrer la liste des produits en fonction de numéro de Devis saisie 
         */
        private void Filtrer_TextBox_TextChanged(object sender, EventArgs e)
        {
            LoadProduits();
        }


        /*
         *  Méthodes pour valider les entrées de l'utilisateur dans le TextBox de désignation
         */
        public bool ValidateDesignation(string input)
        {
            string pattern = @"^[A-Za-z][A-Za-z0-9_\s-]*$";
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


        /*
         *  Cette méthode est appelée avant l'insertion d'un produit:
         *  1. Si le numéro de devis spécifié dans le Devis_TextBox existe dans la BD, retourne 
         *     l'id de ce deis pour l'utiliser dans l'insertion de ce produit
         *  2. Si le numéro de devis n'existe pas, crée un nouveau devis et retourn son id
         *     initiant ainsi la création d'un nouveau produit
         */
        private int DevisNFacture()
        {
            int nDevisFacture = DevisRepo.GetDevisIdByNumber(Devis_TextBox.Text);
            if (nDevisFacture == -1)
            {
                return DevisRepo.CreateDevis(new Devis()
                {
                    Quantity = 0,
                    MontantHT = 0,
                    MontantTVA = 0,
                    Date = DateTime.Now,
                    ClientId = int.Parse(Id_TextBox.Text),
                    Status = DevisStatus.EnAttente,
                    RS = RS_ComboBox.SelectedItem.ToString(),
                    Number = Devis_TextBox.Text
                });
            }
            else
            {
                return nDevisFacture;
            }
        }


        /*
         * Cette méthode utilisée pour activer ou désactiver la button d'enregistrement d'un devis
         * Il est appelée à chaque fois qu'un produit est ajouté ou modifié, ou à chaque fois
         * la RS ou le client est modifié.
         */
        private bool CommandeEnregistre()
        {
            if (Devis_ListView.SelectedItems.Count == 0)
            {
                return false; // Aucun devis sélectionné
            }
            Devis devis = (Devis)Devis_ListView.SelectedItems[0].Tag;
            float totalDevis = (devis.MontantHT + devis.MontantTVA);
            if (ListeFactures == null || !ListeFactures.ContainsKey(Devis_TextBox.Text) || ListeFactures.Count == 0)
            {
                return totalDevis != 0 || (string)RS_ComboBox.SelectedItem != devis.RS || Id_TextBox.Text != devis.ClientId.ToString();
            }
            List<Facture> totalFactures = ListeFactures[Devis_TextBox.Text];
            return totalDevis != (totalFactures.Sum(f => f.Quantity * f.Prix) + totalFactures.Sum(f => f.Quantity * f.Prix * (f.Tva / 100f))) || (string)RS_ComboBox.SelectedItem != devis.RS || Id_TextBox.Text != devis.ClientId.ToString();
        }


        /*
         *  Cette méthode permet d'ajouter un nouveau produit ou de modifier un produit existant
         *  Il active aussi le button d'entregistrement d'un devis si le produit est ajouté ou modifié
         *  pour pouvoir le mis à jour dans la BD.
         */
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
            int devisId = DevisNFacture();
            Facture facture = new Facture
            {
                Designation = Design_TextBox.Text,
                Quantity = int.Parse(Qtt_TextBox.Text),
                Prix = float.Parse(Prix_TextBox.Text, CultureInfo.InvariantCulture),
                Tva = (tva20_RadioButton.Checked) ? 20 : 7,
                Reference = Ref_TextBox.Text,
                Date = DateTime.Now,
                DevisN = devisId
            };
            if (AjouterP_Tile.Text == "Modifier" && oldRef != null)
                FactureRepo.UpdateFacture(facture, oldRef);
            else
                FactureRepo.InsertFacture(facture);
            LoadProduits();
            LoadDevis();
            Enregistrer_Button.Enabled = CommandeEnregistre();
            AjouterP_Tile.Text = "Ajouter";
            Enregistrer_Button.Enabled = true;
            Clear();
            Filtrer_TextBox.Focus();

        }


        /*
         * Ces méthdes permet de charger la liste de tous les produits dans leur ListView
         * et gérér la sélection d'un produit.
         * La sélection d'un produit peuple les TextBox et changer le texte du bouton d'ajout en "Modifier"
         * Ce qui change la fonction de la fonction Ajouter_Tile_Click d'une fonction d'insertion d'un produit 
         * à une fonction de mise à jour d'un produit.
         */
        public void LoadProduits()
        {

            Console.WriteLine("Fetching all factures...");
            Produit_ListView.Items.Clear();

            // Récupérer toutes les factures
            ListeFactures = new Dictionary<string, List<Facture>>();
            ListeFactures = FactureRepo.GetAllFactures();
            Dictionary<string, List<Facture>> mainList = ListeFactures;


            // Appliquer le filtre de recherche si nécessaire
            if (!string.IsNullOrWhiteSpace(Filtrer_TextBox.Text))
            {
                mainList = ListeFactures.Where(f => f.Key.Contains(Filtrer_TextBox.Text)).ToDictionary(f => f.Key, f => f.Value);
            }


            float sumHT = 0;
            float sumTVA = 0;
            float sumTTC = 0;

            foreach (var pair in mainList)
            {
                string devisNbr = pair.Key;
                List<Facture> factures = pair.Value;
                foreach (Facture facture in factures)
                {

                    float montantHT = facture.Quantity * facture.Prix;
                    float montantTVA = montantHT * (facture.Tva / 100f);
                    float totalTTC = montantHT + montantTVA;
                    sumHT += montantHT;
                    sumTVA += montantTVA;
                    sumTTC += totalTTC;

                    ListViewItem item = new ListViewItem(devisNbr);
                    item.SubItems.Add(facture.Designation);
                    item.SubItems.Add(facture.Prix.ToString("N2", culture));
                    item.SubItems.Add(facture.Quantity.ToString());
                    item.SubItems.Add(montantHT.ToString("N2", culture));
                    item.SubItems.Add(montantTVA.ToString("N2", culture));
                    item.SubItems.Add(totalTTC.ToString("N2", culture));
                    item.SubItems.Add(facture.Reference);

                    item.Tag = facture; // Stocker l'objet Facture dans l'élément de la liste
                    Produit_ListView.Items.Add(item);

                }
            }

            Montant_TextBox.Text = sumHT.ToString("N2", culture);
            TVA_TextBox.Text = sumTVA.ToString("N2", culture);
            Total_TextBox.Text = sumTTC.ToString("N2", culture);
        }
        private void Produit_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Produit_ListView.SelectedItems.Count > 0)
            {
                Facture facture = Produit_ListView.SelectedItems[0].Tag as Facture;
                Design_TextBox.Text = facture.Designation;
                Qtt_TextBox.Text = facture.Quantity.ToString();
                Prix_TextBox.Text = facture.Prix.ToString();
                Ref_TextBox.Text = facture.Reference;
                AjouterP_Tile.Text = "Modifier";
                oldRef = facture.Reference;
            }
        }


        /*
         * Cette méthode permet de supprimer un produit à partir de son Id après 
         * sélection et confirmation de l'utilisateur.
         */
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
                int id = ((Facture)Produit_ListView.SelectedItems[0].Tag).FactureN;
                FactureRepo.DeleteById(id);
                LoadProduits();
                Clear();
                AjouterP_Tile.Text = "Ajouter";
                Enregistrer_Button.Enabled = CommandeEnregistre();
            }
        }


        /*
         *  Cette méthode lance le projet WinForms de la création d'un client et passe une fonction
         *  qui fait mise à jour de RS_ComboBox et Client_ComboBox après l'ajout d'un nouveau client.
         */
        private void NouveauC_Tile_Click(object sender, EventArgs e)
        {
            var form = new InformationClient(); // Corrected: Removed 'GestionClient.' prefix  
            form.Synchronisation += LoadRS;
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                // Reload the RS and Client ComboBoxes after adding a new client
                LoadRS();
                RS_ComboBox.SelectedIndex = 0; // Select the first RS by default
                Client_ComboBox.Items.Clear(); // Clear previous clients
                EnableFields(true); // Enable fields after adding a new client

            }
        }


        /*
         * Méthodes utilitaires pour gérer la sélection sécurisée d'un élément dans une ComboBox
         * et pour obtenir le texte du statut d'un devis.
         */
        private void SetComboBoxSelectionSafe(ComboBox comboBox, string value)
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(new System.Action(() => SetComboBoxSelectionSafe(comboBox, value)));
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    comboBox.SelectedIndex = -1;
                    return;
                }

                // Recherche par texte exact
                int index = comboBox.FindStringExact(value);
                if (index >= 0)
                {
                    comboBox.SelectedIndex = index;
                    return;
                }

                // Recherche partielle si exact échoue
                index = comboBox.FindString(value);
                if (index >= 0)
                {
                    comboBox.SelectedIndex = index;
                }
                else
                {
                    comboBox.SelectedIndex = -1;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur SetComboBoxSelectionSafe: {ex.Message}");
                comboBox.SelectedIndex = -1;
            }
        }
        private string GetStatusText(DevisStatus status)
        {
            switch (status)
            {
                case DevisStatus.EnAttente:
                    return "En Attente";
                case DevisStatus.Approuve:
                    return "Accepté";
                case DevisStatus.Refuse:
                    return "Refusée";
                case DevisStatus.Facture:
                    return "Facturé";
                default:
                    return "Inconnu";
            }
        }


        /*
         * ClearClientControls() : Méthode pour gérer les contrôles client dans l'interface utilisateur.
         * ClearAllControls() : Méthode utilitaire pour vider tous les contrôles de l'interface utilisateur.
         * incluant celle du client.
         */
        private void ClearClientControls()
        {
            if (isUpdatingControls) return;

            bool wasUpdating = isUpdatingControls;
            isUpdatingControls = true;

            try
            {
                // Désabonner temporairement l'événement si pas déjà en cours de mise à jour
                if (!wasUpdating)
                {
                    Client_ComboBox.SelectedIndexChanged -= Client_ComboBox_SelectedIndexChanged;
                }

                Client_ComboBox.SelectedIndex = -1;
                RS_ComboBox.SelectedIndex = -1;
                Id_TextBox.Clear();
                IF_TextBox.Clear();
                ICE_TextBox.Clear();

                // Réabonner l'événement si pas déjà en cours de mise à jour
                if (!wasUpdating)
                {
                    Client_ComboBox.SelectedIndexChanged += Client_ComboBox_SelectedIndexChanged;

                }
            }
            finally
            {
                if (!wasUpdating)
                {
                    isUpdatingControls = false;
                }
            }
        }
        private void ClearAllControls()
        {
            if (isUpdatingControls) return;

            isUpdatingControls = true;
            try
            {
                // Désabonner temporairement les événements
                Client_ComboBox.SelectedIndexChanged -= Client_ComboBox_SelectedIndexChanged;

                // Nettoyer les contrôles
                Devis_TextBox.Clear();
                Status_ComboBox.SelectedIndex = -1;
                ClearClientControls();

                // Réabonner les événements
                Client_ComboBox.SelectedIndexChanged += Client_ComboBox_SelectedIndexChanged;
            }
            finally
            {
                isUpdatingControls = false;
                Enregistrer_Button.Enabled = false; // Désactiver le bouton Enregistrer
            }
        }


        /*
         * Méthodes pour gérer la création et la mise à jour d'u devis
         * NouveauD_Button_Click() : Crée un nouveau devis avec un numéro généré automatiquement
         * Enregistrer_Button_Click() : après la génération d'un devis et l'ajout des produits, cette methode
         * est activé grâce au méthode CommandeEnregistre() pour mettre à jour le devis dans la BD.
         */
        private void NouveauD_Button_Click(object sender, EventArgs e)
        {
            Devis_TextBox.Text = DevisRepo.GenerateDevisN();
            Status_ComboBox.SelectedIndex = 1; // Set to "En Attente" by default
        }
        private void Enregistrer_Button_Click(object sender, EventArgs e)
        {
            List<Facture> factures = ListeFactures[Devis_TextBox.Text];
            Filtrer_TextBox.Text = Devis_TextBox.Text;
            if (factures.Count == 0)
            {
                MessageBox.Show("Aucun modification pour ce devis !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Devis devis = new Devis
            {
                DevisId = ((Devis)Devis_ListView.SelectedItems[0].Tag).DevisId,
                Number = Devis_TextBox.Text,
                ClientId = int.Parse(Id_TextBox.Text),
                RS = RS_ComboBox.SelectedItem.ToString(),
                Quantity = factures.Sum(f => f.Quantity),
                MontantHT = factures.Sum(f => f.Prix * f.Quantity),
                MontantTVA = factures.Sum(f => f.Prix * f.Quantity * (f.Tva / 100f)),
                Date = datePicker.Value,
                Status = Status_ComboBox.SelectedItem.ToString() == "En Attente" ? DevisStatus.EnAttente :
                        Status_ComboBox.SelectedItem.ToString() == "Accepté" ? DevisStatus.Approuve :
                        Status_ComboBox.SelectedItem.ToString() == "Refuseé" ? DevisStatus.Refuse : DevisStatus.Facture
            };
            Enregistrer_Button.Enabled = !DevisRepo.UpdateDevis(devis);
            LoadDevis();
            LoadProduits();

        }

        /*
         * Méthode pour lancer le projet WinForms d'impression du devis
         * Il permet d'imprimer le rapport lié au devis sélectionnée
         */
        private void Imprimer_Button_Click(object sender, EventArgs e)
        {
            Devis selectedDevis = (Devis)Devis_ListView.SelectedItems[0].Tag;

            string clientInfo = Client_ComboBox.SelectedItem?.ToString() ?? "";
            string raisonSociale = RS_ComboBox.SelectedItem?.ToString() ?? "";

            string nomPrenom = clientInfo;
            int indexParenthese = clientInfo.LastIndexOf('(');
            if (indexParenthese > 0)
            {
                nomPrenom = clientInfo.Substring(0, indexParenthese).Trim();
            }

            List<Facture> factures = new List<Facture>();
            if (ListeFactures.ContainsKey(Devis_TextBox.Text))
            {
                factures = ListeFactures[Devis_TextBox.Text];
            }

            // Créer une nouvelle instance du rapport
            CrystalReport report = new CrystalReport();

            // IMPORTANT: Définir d'abord la source de données avant les paramètres
            System.Data.DataTable productTable = CreateProductDataTable(factures);
            DataSet ds = new DataSet();
            ds.Tables.Add(productTable);
            report.SetDataSource(ds);

            SetReportParameters(report, selectedDevis, nomPrenom, raisonSociale);



            Imprimante.Form imprimerRapport = new Imprimante.Form();
            imprimerRapport.crystalReportViewer.ReportSource = report;
            imprimerRapport.ShowDialog();

        }


        /*
         * Méthodes pour générer un rapport PDF contenant les informations du devis sélectionné 
         * avec le client associé, une liste des produits et les totaux HT, TVA et TTC.
         * Le PDF générée est enregistrée dans Documents\Factures et ouverte automatiquement.
         * Le rapport est généré à partir de CrystalReport; Il contient des paramètrest définis
         * et pour la liste des produits, Il utilise un fichier .xsd pour définir la structure des données.
         * Le document généré à le nom suivant: Devis_XXX_YYYYMMDD_HHMMSS.pdf avec XXX étant le numéro du devis
         * - SetReportParameters(ReportDocument report, Devis devis, string nomPrenom, string raisonSociale) : Méthode 
         *   pour définir les paramètres du rapport en utilisant le contenu des TextBox et ComboBox actuelle
         * - CreateProducteDataTable(List<Facture> factures) : Méthode pour créer un DataTable à partir de la liste des factures
         */
        private void Facture_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (Devis_ListView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Veuillez sélectionner un devis pour générer le rapport.",
                        "Aucun devis sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Devis selectedDevis = (Devis)Devis_ListView.SelectedItems[0].Tag;

                string clientInfo = Client_ComboBox.SelectedItem?.ToString() ?? "";
                string raisonSociale = RS_ComboBox.SelectedItem?.ToString() ?? "";

                string nomPrenom = clientInfo;
                int indexParenthese = clientInfo.LastIndexOf('(');
                if (indexParenthese > 0)
                {
                    nomPrenom = clientInfo.Substring(0, indexParenthese).Trim();
                }

                List<Facture> factures = new List<Facture>();
                if (ListeFactures.ContainsKey(Devis_TextBox.Text))
                {
                    factures = ListeFactures[Devis_TextBox.Text];
                }

                // Créer une nouvelle instance du rapport
                CrystalReport report = new CrystalReport();

                // IMPORTANT: Définir d'abord la source de données avant les paramètres
                System.Data.DataTable productTable = CreateProductDataTable(factures);
                DataSet ds = new DataSet();
                ds.Tables.Add(productTable);
                report.SetDataSource(ds);

                // Ensuite définir les paramètres
                SetReportParameters(report, selectedDevis, nomPrenom, raisonSociale);

                //DebugReportParameters(report); // Debugging des paramètres

                // Créer le dossier s'il n'existe pas
                string dossierFactures = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Factures");
                Directory.CreateDirectory(dossierFactures);

                string nomFichier = $"Devis_{selectedDevis.Number.Replace("/", "-")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string cheminFichier = Path.Combine(dossierFactures, nomFichier);

                // Exporter vers PDF
                report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, cheminFichier);

                // Ouvrir automatiquement le fichier PDF généré
                MessageBox.Show($"Le fichier PDF a été généré avec succès :\n{cheminFichier}",
                    "PDF généré", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(cheminFichier);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la génération du rapport : {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SetReportParameters(CrystalDecisions.CrystalReports.Engine.ReportDocument report,
            Devis devis, string nomPrenom, string raisonSociale)
        {
            try
            {
                // Méthode correcte pour définir les paramètres sans vérification de cast
                report.SetParameterValue("ClientNomPrenom", nomPrenom ?? "");
                report.SetParameterValue("RaisonSociale", raisonSociale ?? "");
                report.SetParameterValue("DateDevis", devis.Date.ToString("dd/MM/yyyy"));
                report.SetParameterValue("NumeroDevis", devis.Number ?? "");
                report.SetParameterValue("MontantHT", devis.MontantHT.ToString("N2", culture));
                report.SetParameterValue("MontantTVA", devis.MontantTVA.ToString("N2", culture));
                report.SetParameterValue("MontantTTC", (devis.MontantHT + devis.MontantTVA).ToString("N2", culture));
                report.SetParameterValue("DateImpression", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                // Debug: Afficher les paramètres disponibles (corrigé)
                Console.WriteLine("Paramètres disponibles dans le rapport:");
                foreach (CrystalDecisions.Shared.ParameterField param in report.ParameterFields)
                {
                    Console.WriteLine($"- {param.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur dans SetReportParameters : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }
        private System.Data.DataTable CreateProductDataTable(List<Facture> factures)
        {
            System.Data.DataTable dt = new System.Data.DataTable("ProductData");

            // Définir les colonnes comme STRING sauf Quantite (int) pour correspondre à votre DataSet
            dt.Columns.Add("Reference", typeof(string));
            dt.Columns.Add("Designation", typeof(string));
            dt.Columns.Add("Quantite", typeof(int));
            dt.Columns.Add("Prix", typeof(string));
            dt.Columns.Add("Total", typeof(string));

            foreach (Facture facture in factures)
            {
                DataRow row = dt.NewRow();
                row["Reference"] = facture.Reference ?? "";
                row["Designation"] = facture.Designation ?? "";
                row["Quantite"] = facture.Quantity;
                row["Prix"] = facture.Prix.ToString("N2", culture);
                row["Total"] = (facture.Prix * facture.Quantity).ToString("N2", culture) + " Dh";
                dt.Rows.Add(row);
            }

            return dt;
        }


        /*
         * Méthodes pour exporter la liste des devis en Excel utilisant ClosedXML    
         * - Exporter_Tile_Click(): Méthode pour gérer l'événement de clic sur le bouton d'exportation
         *   et appeller la liste des devis actualisée
         * - ExportWithClosedXML() : Méthode pour exporter les devis en utilisant en fichier Excel en utilisant ClosedXML
         */
        private void Exporter_Tile_Click(object sender, EventArgs e)
        {
            try
            {
                // Récupérer la liste des devis
                DevisList = DevisRepo.GetAllDevis();

                if (DevisList == null || DevisList.Count == 0)
                {
                    MessageBox.Show("Aucun devis à exporter.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ExportWithClosedXML();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exportation : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportWithClosedXML()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Devis");

                // En-têtes
                worksheet.Cell(1, 1).Value = "N° Devis";
                worksheet.Cell(1, 2).Value = "Date";
                worksheet.Cell(1, 3).Value = "Client";
                worksheet.Cell(1, 4).Value = "Raison Sociale";
                worksheet.Cell(1, 5).Value = "Montant HT";
                worksheet.Cell(1, 6).Value = "Montant TVA";
                worksheet.Cell(1, 7).Value = "Montant TTC";

                // Style des en-têtes
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Remplir les données
                for (int i = 0; i < DevisList.Count; i++)
                {
                    var devis = DevisList[i];
                    int row = i + 2;
                    Client client = ClientRepo.GetClientById(devis.ClientId);

                    worksheet.Cell(row, 1).Value = devis.Number ?? "";
                    worksheet.Cell(row, 2).Value = devis.Date.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 3).Value = $"{client.Nom} {client.Prenom}" ?? "";
                    worksheet.Cell(row, 4).Value = devis.RS ?? "";
                    worksheet.Cell(row, 5).Value = devis.MontantHT.ToString("F2") ?? "0,00";
                    worksheet.Cell(row, 6).Value = devis.MontantTVA.ToString("F2") ?? "0,00";
                    worksheet.Cell(row, 7).Value = (devis.MontantHT + devis.MontantTVA).ToString("F2") ?? "0,00";

                }

                // Ajuster les colonnes
                worksheet.ColumnsUsed().AdjustToContents();

                // Sauvegarder
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Sauvegarder le fichier Excel",
                    FileName = $"Export_Devis_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show("Export réussi !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
        }


        /*
         * Méthodes pour gérer l'activation des champs de saisie dans l'interface utilisateur
         * et de vider les champs de saisie.
         */
        public void EnableFields(bool enable)
        {
            Design_TextBox.Enabled = enable;
            Qtt_TextBox.Enabled = enable;
            Prix_TextBox.Enabled = enable;
            Ref_TextBox.Enabled = enable;
            tva20_RadioButton.Enabled = enable;
            tva7_RadioButton.Enabled = enable;
            AjouterP_Tile.Enabled = enable;
            datePicker.Enabled = enable;
            RS_ComboBox.Enabled = enable;
            Client_ComboBox.Enabled = enable;

        }
        private void Clear()
        {
            Design_TextBox.Clear();
            Qtt_TextBox.Clear();
            Prix_TextBox.Clear();
            Ref_TextBox.Clear();
        }
    }
}
