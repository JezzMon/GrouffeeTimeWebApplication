﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrouffeeTimeWebApplication.Models
{
    [Table("Food")]
    public class Food
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string? FoodName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        public double Price { get; set; }

        public string? Image { get; set; }
        [Required]
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public List<OrderDetail> OrderDetail { get; set; }

        public List<CartDetail> CartDetail { get; set; }

        public Stock Stock { get; set; }

        [NotMapped]
        public string CategoryName { get; set; }

        [NotMapped]
        public int Quantity { get; set; }
    }
}
