using DocumentFormat.OpenXml.Bibliography;

namespace Gestion_Stagiaire.Models
{
    public class Affectation
    {
        public Guid Id { get; set; }
        public Guid DemandeStageId { get; set; }
        public DemandeStage? DemandeStage { get; set; }
        public List<Departement> Departement { get; set; } = new List<Departement>();

        public String Encadrant { get; set; }
        public DateTime Date_Affectation { get; set; }
    }
}