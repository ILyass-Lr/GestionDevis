using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GestionClientFactures
{
    public enum DevisStatus
    {
        EnAttente,
        Approuve,
        Refuse,
        Facture
    }

    public class Devis
    {
        public int DevisId { get; set; }
        public int Quantity { get; set; }

        public float MontantTVA { get; set; }
        public float MontantHT { get; set; }
        public DateTime Date { get; set; }
        public int ClientId { get; set; }
        public DevisStatus Status { get; set; }
        public string RS { get; set; }

        public string Number { get; set; }
        public Devis(int devisId, int quantity, float montantHT, float montantTVA, DateTime date, int clientId, DevisStatus status, string rs, string number)
        {
            DevisId = devisId;
            Quantity = quantity;
            MontantHT = montantHT;
            MontantTVA = montantTVA;
            Date = date;
            ClientId = clientId;
            Status = status;
            RS = rs;
            Number = number;
        }
        public Devis() { }


    }
}
