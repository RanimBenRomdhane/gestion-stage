using Gestion_Stagiaire.Models;

namespace Gestion_Stagiaires.Models
{
    public class Stagiaire
    {
        public Guid Id { get; set; }
        public String Nom { get; set; }
        public String Prenom { get; set; }
        public String Cin { get; set; }
        public int Telephone { get; set; }
        public String Email { get; set; }
        public String Ecole { get; set; }
        public String? Path_Photo { get; set; }
        public String? Path_CV { get; set; }
        public List<DemandeStage> DemandesStage { get; set; } = new List<DemandeStage>();


    }
}