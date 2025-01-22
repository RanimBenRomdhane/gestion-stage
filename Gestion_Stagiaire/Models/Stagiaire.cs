using DocumentFormat.OpenXml.Spreadsheet;
using Gestion_Stagiaire.Models;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Stagiaires.Models
{
    public class Stagiaire 
    {
       public Guid Id { get; set; }
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public String Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
         public String Prenom { get; set; }

        [Range(10000000, 99999999, ErrorMessage = "Le numero de CIN doit comporter 8 chiffres.")]
        [Required(ErrorMessage = "Le numéro de CIN est obligatoire.")]  
        public int Cin { get; set; }

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        [Range(10000000, 99999999, ErrorMessage = "Le numero de telephone doit comporter 8 chiffres.")]
        public int Telephone { get; set; }

        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
        public String Email { get; set; }

        [Required(ErrorMessage = "L'école est obligatoire.")]
        public String Ecole { get; set; }
        public String? Path_Photo { get; set; }
        public String? Path_CV { get; set; }
        public List<DemandeStage> DemandesStage { get; set; } = new List<DemandeStage>();


    }
}