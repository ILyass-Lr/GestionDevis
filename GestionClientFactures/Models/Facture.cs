using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionClientFactures
{
    public class Facture
    {
        public string DevisNbr { get; set; }
        public int? FactureN { get; set; }
        public string Designation { get; set; }
        public int Quantity { get; set; }
        public float Prix { get; set; }
        public float Tva { get; set; }
        public string Reference { get; set; }
        public DateTime Date { get; set; }
        public int ClientId { get; set; }
        public Facture(string devisNbr, string designation, int quantity, float prix, float tva, string reference, DateTime date, int clientId)
        {
            DevisNbr = devisNbr;
            Designation = designation;
            Quantity = quantity;
            Prix = prix;
            Tva = tva;
            Reference = reference;
            Date = date;
            ClientId = clientId;
        }
        public Facture() { }    
    }
}
