using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClient
{
    public class Client
    {
        public int ClientId { get; set; }
        public string NomResp { get; set; }
        public string TypeSociete { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Tel { get; set; }
        public string Portable { get; set; }
        public string Fax { get; set; }
        public string RS { get; set; }
        public string Email { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public string Patente { get; set; }
        public string ICE { get; set; }
        public string RC { get; set; }
        public string IF { get; set; }
        public string Pays { get; set; }

        // Constructor
        public Client()
        {
            // Initialize with empty strings to avoid null reference issues
            NomResp = string.Empty;
            TypeSociete = string.Empty;
            Nom = string.Empty;
            Prenom = string.Empty;
            Tel = string.Empty;
            Portable = string.Empty;
            Fax = string.Empty;
            RS = string.Empty;
            Email = string.Empty;
            Adresse = string.Empty;
            Ville = string.Empty;
            Patente = string.Empty;
            ICE = string.Empty;
            RC = string.Empty;
            IF = string.Empty;
            Pays = string.Empty;
        }

        // Constructor with parameters
        public Client(int clientId, string nomResp, string typeSociete, string nom, string prenom,
                      string tel, string portable, string fax, string rs, string email,
                      string adresse, string ville, string patente, string ice, string rc,
                      string ifValue, string pays)
        {
            ClientId = clientId;
            NomResp = nomResp ?? string.Empty;
            TypeSociete = typeSociete ?? string.Empty;
            Nom = nom ?? string.Empty;
            Prenom = prenom ?? string.Empty;
            Tel = tel ?? string.Empty;
            Portable = portable ?? string.Empty;
            Fax = fax ?? string.Empty;
            RS = rs ?? string.Empty;
            Email = email ?? string.Empty;
            Adresse = adresse ?? string.Empty;
            Ville = ville ?? string.Empty;
            Patente = patente ?? string.Empty;
            ICE = ice ?? string.Empty;
            RC = rc ?? string.Empty;
            IF = ifValue ?? string.Empty;
            Pays = pays ?? string.Empty;
        }
    }
}
