using System.ComponentModel.DataAnnotations;
namespace CrudOperation.DTOs
{
  public class CreatePaymentOrderDto
  {
    [Range(1,int.MaxValue)]
    public int FeeItemId {get; set;}
  }
}