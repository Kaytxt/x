using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotIA.API.Data;
using DotIA.API.Models;
using TabelasDoBanco;

namespace DotIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("pendentes")]
        public async Task<ActionResult<List<TicketDTO>>> ObterTicketsPendentes()
        {
            try
            {
                var tickets = await (
                    from ticket in _context.Tickets
                    join solicitante in _context.Solicitantes on ticket.IdSolicitante equals solicitante.Id
                    join chat in _context.ChatsHistorico on ticket.Id equals chat.IdTicket into chatGroup
                    from chat in chatGroup.DefaultIfEmpty()
                    where ticket.IdStatus == 1 // Status 1 = Pendente
                    orderby ticket.DataAbertura descending
                    select new TicketDTO
                    {
                        Id = ticket.Id,
                        NomeSolicitante = solicitante.Nome,
                        DescricaoProblema = ticket.DescricaoProblema,
                        Status = "Pendente",
                        DataAbertura = ticket.DataAbertura,
                        Solucao = ticket.Solucao,
                        ChatId = chat != null ? chat.Id : 0,
                        PerguntaOriginal = chat != null ? chat.Pergunta : "",
                        RespostaIA = chat != null ? chat.Resposta : ""
                    }
                ).ToListAsync();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = $"Erro ao buscar tickets: {ex.Message}" });
            }
        }

        [HttpPost("resolver")]
        public async Task<ActionResult> ResolverTicket([FromBody] ResolverTicketRequest request)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(request.TicketId);

                if (ticket == null)
                {
                    return NotFound(new { erro = "Ticket não encontrado" });
                }

                // ✅ CORREÇÃO: Usando formato consistente com prefixo TÉCNICO
                if (!string.IsNullOrEmpty(request.Solucao))
                {
                    var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
                    var novaMensagem = $"[TÉCNICO - {timestamp}] {request.Solucao}";

                    if (!string.IsNullOrEmpty(ticket.Solucao))
                    {
                        ticket.Solucao += "\n\n" + novaMensagem;
                    }
                    else
                    {
                        ticket.Solucao = novaMensagem;
                    }
                }

                // Se marcar como resolvido
                if (request.MarcarComoResolvido)
                {
                    ticket.IdStatus = 2; // Resolvido
                    ticket.DataEncerramento = DateTime.UtcNow;

                    // Atualiza o chat relacionado para status 4 (Resolvido pelo Técnico)
                    var chat = await _context.ChatsHistorico
                        .FirstOrDefaultAsync(c => c.IdTicket == ticket.Id);

                    if (chat != null)
                    {
                        chat.Status = 4; // Resolvido pelo Técnico
                    }
                }
                // Senão, apenas salva a solução mas mantém pendente
                else
                {
                    ticket.IdStatus = 1; // Mantém pendente para acompanhamento
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    sucesso = true,
                    mensagem = request.MarcarComoResolvido
                        ? "Ticket resolvido com sucesso!"
                        : "Resposta enviada! Ticket ainda em acompanhamento.",
                    ticketStatus = ticket.IdStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = $"Erro ao resolver ticket: {ex.Message}" });
            }
        }

        [HttpGet("{ticketId}")]
        public async Task<ActionResult> ObterTicket(int ticketId)
        {
            try
            {
                var ticketCompleto = await (
                    from ticket in _context.Tickets
                    join solicitante in _context.Solicitantes on ticket.IdSolicitante equals solicitante.Id
                    join chat in _context.ChatsHistorico on ticket.Id equals chat.IdTicket into chatGroup
                    from chat in chatGroup.DefaultIfEmpty()
                    where ticket.Id == ticketId
                    select new
                    {
                        Ticket = ticket,
                        NomeSolicitante = solicitante.Nome,
                        EmailSolicitante = solicitante.Email,
                        Chat = chat != null ? new
                        {
                            chat.Id,
                            chat.Pergunta,
                            chat.Resposta,
                            chat.DataHora,
                            chat.Status
                        } : null
                    }
                ).FirstOrDefaultAsync();

                if (ticketCompleto == null)
                {
                    return NotFound(new { erro = "Ticket não encontrado" });
                }

                return Ok(ticketCompleto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = $"Erro ao buscar ticket: {ex.Message}" });
            }
        }
    }
}