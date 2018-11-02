using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.CodePaste;

namespace Modix.WebServer.Controllers
{
    [Route("~/api")]
    public class PasteController : Controller
    {
        private readonly ICodePasteRepository _codePasteRepository;

        public PasteController(ICodePasteRepository codePasteRepository)
        {
            _codePasteRepository = codePasteRepository;
        }

        [Route("pastes")]
        public IActionResult Index()
        {
            return Ok(_codePasteRepository.GetPastes().OrderByDescending(d=>d.Created).Take(10));
        }

        [Route("pastes/{id}")]
        public IActionResult Index(int id)
        {
            return Ok(_codePasteRepository.GetPaste(id));
        }

        [Route("pastes/{id}/raw")]
        public IActionResult Raw(int id)
        {
            return Content(_codePasteRepository.GetPaste(id).Content, "text/plain");
        }
    }
}
