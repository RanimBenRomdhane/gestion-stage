using Gestion_Stagiaires.Models;

namespace Gestion_Stagiaire.Models
{
    public class DemandeStage 
    {
        public Guid Id { get; set; }
        public Guid StagiaireId { get; set; }
        public Stagiaire? Stagiaire { get; set; }
        public String Type_Stage { get; set; }
        public DateTime Date_Debut { get; set; }
        public DateTime Date_Fin { get; set; }
        public String Status { get; set; }
        public String Path_Demande_Stage { get; set; }
        public DateTime Date_Demande { get; set; }
        public String Affectation { get; set; }
        public String? Commentaire { get; set; }




    }
}
