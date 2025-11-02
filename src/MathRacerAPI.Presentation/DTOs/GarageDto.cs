using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.DTOs
{
    public class GarageItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public bool IsOwned { get; set; }
        public bool IsActive { get; set; }
    }

    public class GarageItemsResponseDto
    {
        public List<GarageItemDto> Items { get; set; } = new List<GarageItemDto>();
        public GarageItemDto? ActiveItem { get; set; }
        public string ItemType { get; set; } = string.Empty;
    }

    public class ActivateItemRequestDto
    {
        public int PlayerId { get; set; }
        public int ProductId { get; set; }
        public string ProductType { get; set; } = string.Empty;
    }

    public class ApiErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    public class ActivateItemResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}