using BaseClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionClientFactures
{
    public class GestionClient
    {
        public Client Client { get; set; }
        public List<Devis> ListeDevis { get; set; }

        public List<Facture> ListeFactures { get; set; }

        public GestionClient(Client client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client), "Client cannot be null");
            ListeDevis = new List<Devis>();
            ListeFactures = new List<Facture>();
        }
        public GestionClient(Client client, List<Devis> listeDevis, List<Facture> listeFactures)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client), "Client cannot be null");
            ListeDevis = listeDevis ?? throw new ArgumentNullException(nameof(listeDevis), "ListeDevis cannot be null");
            ListeFactures = listeFactures ?? throw new ArgumentNullException(nameof(listeFactures), "ListeFactures cannot be null");
        }
    }
}
