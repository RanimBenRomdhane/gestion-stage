using Gestion_Stagiaires.Models;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Stagiaire.Models
{
    public class DemandeStage 
    {
        public Guid Id { get; set; }
        public Guid StagiaireId { get; set; }
        public Stagiaire? Stagiaire { get; set; }
        public Guid Type_StageId { get; set; }
        public Type_Stage? Type_Stage { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }
        public DateTime Date_Debut { get; set; }
        public DateTime Date_Fin { get; set; }
        public String? Path_Demande_Stage { get; set; }
        public DateTime Date_Demande { get; set; }
        public Guid? AffectationId { get; set; }
        public Affectation? Affectation { get; set; }
        public  String? Path_Rapport { get; set; }
        public String? Commentaire { get; set; }




    }
}
