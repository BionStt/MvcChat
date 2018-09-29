
using System.ComponentModel.DataAnnotations;

namespace MvcChat.Model
{
    public class Client
    {
        [Required, RegularExpression(@"\S{1,64}")]
        public string name;
    }
}