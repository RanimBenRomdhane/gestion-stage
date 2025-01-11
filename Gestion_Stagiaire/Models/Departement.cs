namespace Gestion_Stagiaire.Models
{
    public class Departement
    {
        public Guid Id { get; set; }
        public String Nom_Departement { get; set; }
        public List<Affectation> Affectations { get; set; } = new List<Affectation>();
    }
}