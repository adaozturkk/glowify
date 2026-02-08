using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glowify.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "List Price")]
        [Range(1, 10000)]
        public double ListPrice { get; set; }

        [Required]
        [Display(Name = "Price")]
        [Range(1, 10000)]
        public double Price { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Target Skin Type")]
        public SkinType TargetSkinType { get; set; }

        [Required]
        [Display(Name = "Category")]
        public CategoryType Category {  get; set; }

        [Display(Name = "Key Ingredient")]
        public string? MainIngredient { get; set; }

        [Display(Name = "Ingredients List")]
        public string? IngredientsList { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Display(Name = "Stock Quantity")]
        [Range(0, 50000, ErrorMessage = "Stock count can't be less than 0!")]
        public int Stock { get; set; }
    }
}
