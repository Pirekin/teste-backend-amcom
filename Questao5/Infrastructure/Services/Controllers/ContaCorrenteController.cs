using MediatR;

using Microsoft.AspNetCore.Mvc;

using Questao5.Application.Commands.Requests;

namespace Questao5.Infrastructure.Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly ILogger<ContaCorrenteController> _logger;
        private readonly IContaCorrenteService _contaCorrenteService;
        private readonly IMediator _mediator;

        public ContaCorrenteController(ILogger<ContaCorrenteController> logger, IContaCorrenteService contaCorrenteService, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            _contaCorrenteService = contaCorrenteService;
        }

        [HttpPost("movimentar")]
        public async Task<ActionResult<string>> Movimentar([FromBody] MovimentacaoCommand command)
        {
            var movimentacaoID = await _mediator.Send(command);
            return Ok(movimentacaoID);
        }

        [HttpGet("{id}/saldo")]
        public async Task<ActionResult<decimal>> ObterSaldo(string id)
        {
            var saldo = await _mediator.Send(new ObterSaldoQuery { IdContaCorrente = id });
            return Ok(saldo);
        }
    }
}