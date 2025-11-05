using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TabelasDoBanco
{
    [Table("departamentos")]
    public class Departamento
    {
        [Key]
        [Column("id_departamento")]
        public int Id { get; set; }

        [Column("nome_departamento")]
        public string Nome { get; set; } = string.Empty;
    }

    [Table("solicitantes")]
    public class Solicitante
    {
        [Key]
        [Column("id_solicitante")]
        public int Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("senha")]
        public string Senha { get; set; } = string.Empty;

        [Column("id_departamento")]
        public int IdDepartamento { get; set; }
    }

    [Table("tecnicos")]
    public class Tecnico
    {
        [Key]
        [Column("id_tecnico")]
        public int Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("senha")]
        public string Senha { get; set; } = string.Empty;

        [Column("id_especialidade")]
        public int IdEspecialidade { get; set; }


        [Column("is_gerente")]              
        public bool IsGerente { get; set; }
    }

    [Table("categorias")]
    public class Categoria
    {
        [Key]
        [Column("id_categoria")]
        public int Id { get; set; }

        [Column("nome_categoria")]
        public string Nome { get; set; } = string.Empty;
    }

    [Table("subcategorias")]
    public class Subcategoria
    {
        [Key]
        [Column("id_subcategoria")]
        public int Id { get; set; }

        [Column("nome_subcategoria")]
        public string Nome { get; set; } = string.Empty;

        [Column("id_categoria")]
        public int IdCategoria { get; set; }
    }

    [Table("niveis_atendimento")]
    public class NivelAtendimento
    {
        [Key]
        [Column("id_nivel")]
        public int Id { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;
    }

    [Table("tickets")]
    public class Ticket
    {
        [Key]
        [Column("id_ticket")]
        public int Id { get; set; }

        [Column("id_solicitante")]
        public int IdSolicitante { get; set; }

        [Column("id_tecnico")]
        public int IdTecnico { get; set; }

        [Column("id_subcategoria")]
        public int IdSubcategoria { get; set; }

        [Column("id_nivel")]
        public int IdNivel { get; set; }

        [Column("descricao_problema")]
        public string DescricaoProblema { get; set; } = string.Empty;

        [Column("id_status")]
        public int IdStatus { get; set; }

        [Column("data_abertura")]
        public DateTime DataAbertura { get; set; }

        [Column("data_encerramento")]
        public DateTime? DataEncerramento { get; set; }

        [Column("solucao")]
        public string? Solucao { get; set; }
    }

    [Table("historico_util")]
    public class HistoricoUtil
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_solicitante")]
        public int IdSolicitante { get; set; }

        [Column("pergunta")]
        public string Pergunta { get; set; } = string.Empty;

        [Column("resposta")]
        public string Resposta { get; set; } = string.Empty;

        [Column("datahora")]
        public DateTime DataHora { get; set; }
    }

    [Table("chat_historico")]
    public class ChatHistorico
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_solicitante")]
        public int IdSolicitante { get; set; }

        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("pergunta")]
        public string Pergunta { get; set; } = string.Empty;

        [Column("resposta")]
        public string Resposta { get; set; } = string.Empty;

        [Column("data_hora")]
        public DateTime DataHora { get; set; }

        // ✅ NOVOS CAMPOS
        [Column("id_ticket")]
        public int? IdTicket { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1; // 1=Em andamento, 2=Concluído, 3=Pendente Técnico, 4=Resolvido Técnico
    }

    [Table("avaliacao_resposta")]
    public class AvaliacaoResposta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_solicitante")]
        public int IdSolicitante { get; set; }

        [Column("pergunta")]
        public string Pergunta { get; set; } = string.Empty;

        [Column("foi_util")]
        public bool FoiUtil { get; set; }

        [Column("datahora")]
        public DateTime DataHora { get; set; }
    }
}