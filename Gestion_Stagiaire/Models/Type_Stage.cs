namespace Gestion_Stagiaire.Models
{
    public class Type_Stage
    {
        public Guid Id { get; set; }
        public String Stage_Type { get; set; }
        public List<DemandeStage>? DemandesStage { get; set; } = new List<DemandeStage>();

    }
}