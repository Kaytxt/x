using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotIA.API.Data;
using DotIA.API.Models;
using DotIA.API.Services;
using TabelasDoBanco;

namespace DotIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpenAIService _openAIService;

        public ChatController(ApplicationDbContext context, IOpenAIService openAIService)
        {
            _context = context;
            _openAIService = openAIService;
        }

        [HttpPost("enviar")]
        public async Task<ActionResult<ChatResponse>> EnviarPergunta([FromBody] ChatRequest request)
        {
            try
            {
                // ✅ CORREÇÃO: O usuário pode criar novos chats mesmo tendo tickets pendentes
                // Não há mais bloqueio para criar novos chats

                var resposta = await _openAIService.ObterRespostaAsync(request.Pergunta);

                var historico = new ChatHistorico
                {
                    IdSolicitante = request.UsuarioId,
                    Titulo = request.Pergunta.Length > 30
                        ? request.Pergunta.Substring(0, 30) + "..."
                        : request.Pergunta,
                    Pergunta = request.Pergunta,
                    Resposta = resposta,
                    DataHora = DateTime.UtcNow,
                    Status = 1 // Em andamento
                };

                _context.ChatsHistorico.Add(historico);
                await _context.SaveChangesAsync();

                return Ok(new ChatResponse
                {
                    Sucesso = true,
                    Resposta = resposta,
                    DataHora = DateTime.UtcNow,
                    ChatId = historico.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ChatResponse
                {
                    Sucesso = false,
                    Resposta = $"Erro: {ex.Message}"
                });
            }
        }

        // ✅ Enviar mensagem do usuário para o técnico
        [HttpPost("enviar-para-tecnico")]
        public async Task<ActionResult> EnviarMensagemParaTecnico([FromBody] MensagemUsuarioRequest request)
        {
            try
            {
                var chat = await _context.ChatsHistorico.FindAsync(request.ChatId);

                if (chat == null)
                {
                    return NotFound(new { erro = "Chat não encontrado" });
                }

                if (chat.Status != 3 || !chat.IdTicket.HasValue)
                {
                    return BadRequest(new { erro = "Este chat não está com ticket pendente" });
                }

                var ticket = await _context.Tickets.FindAsync(chat.IdTicket.Value);

                if (ticket == null)
                {
                    return NotFound(new { erro = "Ticket não encontrado" });
                }

                // ✅ CONCATENA mensagem do usuário ao ticket
                var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
                var novaMensagem = $"[USUÁRIO - {timestamp}] {request.Mensagem}";

                if (!string.IsNullOrEmpty(ticket.Solucao))
                {
                    ticket.Solucao += "\n\n" + novaMensagem;
                }
                else
                {
                    ticket.Solucao = novaMensagem;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Mensagem enviada ao técnico"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("historico/{usuarioId}")]
        public async Task<ActionResult> ObterHistorico(int usuarioId)
        {
            try
            {
                var historico = await _context.ChatsHistorico
                    .Where(h => h.IdSolicitante == usuarioId)
                    .OrderByDescending(h => h.DataHora)
                    .Select(h => new
                    {
                        h.Id,
                        h.Titulo,
                        h.Pergunta,
                        h.Resposta,
                        h.DataHora,
                        Status = h.Status,
                        h.IdTicket,
                        StatusTexto = h.Status == 1 ? "Em andamento" :
                                      h.Status == 2 ? "Concluído" :
                                      h.Status == 3 ? "Pendente" :
                                      h.Status == 4 ? "Resolvido" : "Desconhecido"
                    })
                    .ToListAsync();

                return Ok(historico);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPost("avaliar")]
        public async Task<ActionResult> AvaliarResposta([FromBody] AvaliacaoRequest request)
        {
            try
            {
                ChatHistorico chat = null;

                if (request.ChatId > 0)
                {
                    chat = await _context.ChatsHistorico.FindAsync(request.ChatId);
                }
                else
                {
                    chat = await _context.ChatsHistorico
                        .Where(c => c.IdSolicitante == request.UsuarioId)
                        .OrderByDescending(c => c.DataHora)
                        .FirstOrDefaultAsync(c => c.Pergunta == request.Pergunta && c.Resposta == request.Resposta);
                }

                if (request.FoiUtil)
                {
                    // Salva como útil
                    _context.HistoricoUtil.Add(new HistoricoUtil
                    {
                        IdSolicitante = request.UsuarioId,
                        Pergunta = request.Pergunta,
                        Resposta = request.Resposta,
                        DataHora = DateTime.UtcNow
                    });

                    // Atualiza status do chat para concluído
                    if (chat != null)
                    {
                        chat.Status = 2; // Concluído
                    }
                }
                else
                {
                    // Cria ticket para técnico resolver
                    var ticket = new Ticket
                    {
                        IdSolicitante = request.UsuarioId,
                        IdTecnico = 1,
                        IdSubcategoria = 1,
                        IdNivel = 1,
                        DescricaoProblema = request.Pergunta,
                        IdStatus = 1, // Pendente
                        DataAbertura = DateTime.UtcNow
                    };

                    _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync(); // Salva para obter o ID

                    // Atualiza status do chat para pendente e vincula ticket
                    if (chat != null)
                    {
                        chat.Status = 3; // Pendente com Técnico
                        chat.IdTicket = ticket.Id;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    sucesso = true,
                    ticketId = chat?.IdTicket,
                    chatId = chat?.Id,
                    novoStatus = chat?.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, erro = ex.Message });
            }
        }

        [HttpPost("abrir-ticket-direto")]
        public async Task<ActionResult> AbrirTicketDireto([FromBody] AbrirTicketDiretoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Titulo))
                {
                    return BadRequest(new { erro = "O título é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(request.Descricao))
                {
                    return BadRequest(new { erro = "A descrição do problema é obrigatória" });
                }

                // Cria o ticket
                var ticket = new Ticket
                {
                    IdSolicitante = request.UsuarioId,
                    IdTecnico = 1, // Técnico padrão
                    IdSubcategoria = 1, // Subcategoria padrão
                    IdNivel = 1, // Nível padrão
                    DescricaoProblema = request.Descricao,
                    IdStatus = 1, // Pendente
                    DataAbertura = DateTime.UtcNow
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Cria o chat histórico vinculado ao ticket
                var chatHistorico = new ChatHistorico
                {
                    IdSolicitante = request.UsuarioId,
                    Titulo = request.Titulo.Length > 50
                        ? request.Titulo.Substring(0, 50) + "..."
                        : request.Titulo,
                    Pergunta = request.Descricao,
                    Resposta = "Ticket aberto diretamente. Aguardando atendimento do técnico.",
                    DataHora = DateTime.UtcNow,
                    Status = 3, // Pendente com Técnico
                    IdTicket = ticket.Id
                };

                _context.ChatsHistorico.Add(chatHistorico);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Ticket criado com sucesso! Um técnico irá atendê-lo em breve.",
                    ticketId = ticket.Id,
                    chatId = chatHistorico.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("verificar-resposta/{chatId}")]
        public async Task<ActionResult> VerificarRespostaTecnico(int chatId)
        {
            try
            {
                var chat = await _context.ChatsHistorico.FindAsync(chatId);

                if (chat == null)
                {
                    return NotFound(new { erro = "Chat não encontrado" });
                }

                // Se tem ticket vinculado, buscar a solução
                if (chat.IdTicket.HasValue)
                {
                    var ticket = await _context.Tickets.FindAsync(chat.IdTicket.Value);

                    if (ticket != null && !string.IsNullOrEmpty(ticket.Solucao))
                    {
                        return Ok(new
                        {
                            temResposta = true,
                            solucao = ticket.Solucao,
                            status = chat.Status,
                            statusTicket = ticket.IdStatus,
                            dataResposta = ticket.DataEncerramento
                        });
                    }
                }

                return Ok(new { temResposta = false, status = chat.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("detalhes/{chatId}")]
        public async Task<ActionResult> ObterDetalhesChat(int chatId)
        {
            try
            {
                var chat = await _context.ChatsHistorico
                    .Where(c => c.Id == chatId)
                    .Select(c => new
                    {
                        c.Id,
                        c.Titulo,
                        c.Pergunta,
                        c.Resposta,
                        c.DataHora,
                        Status = c.Status,
                        c.IdTicket,
                        StatusTexto = c.Status == 1 ? "Em andamento" :
                                      c.Status == 2 ? "Concluído" :
                                      c.Status == 3 ? "Pendente" :
                                      c.Status == 4 ? "Resolvido" : "Desconhecido"
                    })
                    .FirstOrDefaultAsync();

                if (chat == null)
                {
                    return NotFound(new { erro = "Chat não encontrado" });
                }

                // Se tem ticket, buscar informações do ticket
                object ticketInfo = null;
                if (chat.IdTicket.HasValue)
                {
                    var ticket = await _context.Tickets.FindAsync(chat.IdTicket.Value);
                    if (ticket != null)
                    {
                        ticketInfo = new
                        {
                            ticket.Id,
                            ticket.Solucao,
                            ticket.IdStatus,
                            ticket.DataEncerramento
                        };
                    }
                }

                return Ok(new { chat, ticket = ticketInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPut("editar-titulo/{chatId}")]
        public async Task<ActionResult> EditarTituloChat(int chatId, [FromBody] EditarTituloRequest request)
        {
            try
            {
                var chat = await _context.ChatsHistorico.FindAsync(chatId);

                if (chat == null)
                {
                    return NotFound(new { erro = "Chat não encontrado" });
                }

                chat.Titulo = request.NovoTitulo;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Título atualizado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpDelete("excluir/{chatId}")]
        public async Task<ActionResult> ExcluirChat(int chatId)
        {
            try
            {
                var chat = await _context.ChatsHistorico.FindAsync(chatId);

                if (chat == null)
                {
                    return NotFound(new { erro = "Chat não encontrado" });
                }

                // Se tem ticket vinculado, também exclui o ticket
                if (chat.IdTicket.HasValue)
                {
                    var ticket = await _context.Tickets.FindAsync(chat.IdTicket.Value);
                    if (ticket != null)
                    {
                        _context.Tickets.Remove(ticket);
                    }
                }

                _context.ChatsHistorico.Remove(chat);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Chat excluído com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }

    public class EditarTituloRequest
    {
        public string NovoTitulo { get; set; } = string.Empty;
    }

    public class AbrirTicketDiretoRequest
    {
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }
}