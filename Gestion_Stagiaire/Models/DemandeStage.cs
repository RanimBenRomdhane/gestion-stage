using Gestion_Stagiaires.Models;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Stagiaire.Models
{
    public class DemandeStage 
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Le stagiaire est obligatoire.")]

        public Guid StagiaireId { get; set; }
        public Stagiaire? Stagiaire { get; set; }

        [Required(ErrorMessage = "Le type de stage est obligatoire.")]
        public Guid Type_StageId { get; set; }
        public Type_Stage? Type_Stage { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire.")]
        public DateTime Date_Debut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [DateGreaterThan("Date_Debut", ErrorMessage = "La date de fin doit être postérieure à la date de début.")]
        public DateTime Date_Fin { get; set; }

        public String? Path_Demande_Stage { get; set; }

        [Required(ErrorMessage = "La date de la demande est obligatoire.")]
        public DateTime Date_Demande { get; set; }

        public Guid? DepartementId { get; set; }
        public Departement? Departement { get; set; }
        public String? Encadrant { get; set; }
        public String? Titre_Projet { get; set; }
        public  String? Path_Rapport { get; set; }
        public String? Commentaire { get; set; }




    }
}

public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var currentValue = (DateTime?)value;
        var comparisonValue = (DateTime?)validationContext.ObjectType
            .GetProperty(_comparisonProperty)?
            .GetValue(validationContext.ObjectInstance);

        if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}