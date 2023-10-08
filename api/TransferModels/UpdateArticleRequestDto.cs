using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using api.CustomDataAnnotations;

namespace api.TransferModels;

public class UpdateArticleRequestDto
{
        
        [MinLength(5)]
        [MaxLength(30)]
        public string? Headline { get; set; }
        [ValueIsOneOf(new string[] { "Bob", "Rob", "Lob", "Dob" }, "We only have 4 authors recruited: Rob, Dob, Bob and Lob. The supplied author name does not work here.")]

        public string? Author  { get; set; }
        [NotNull]
        public string? ArticleImgUrl { get; set; }
        [MaxLength(1000)]
        public string? Body { get; set; }
    
}