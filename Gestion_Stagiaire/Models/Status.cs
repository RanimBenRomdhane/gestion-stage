namespace Gestion_Stagiaire.Models
{
    public class Status
    {
        public Guid Id { get; set; }
        public String Reponse { get; set; }
        public List<DemandeStage>? DemandesStage { get; set; } = new List<DemandeStage>();

    }
}