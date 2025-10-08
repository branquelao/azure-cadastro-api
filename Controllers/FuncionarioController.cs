using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.AspNetCore.Mvc;
using TrilhaNetAzureDesafio.Context;
using TrilhaNetAzureDesafio.Models;

namespace TrilhaNetAzureDesafio.Controllers;

[ApiController]
[Route("[controller]")]
public class FuncionarioController : ControllerBase
{
    private readonly RHContext _context;
    private readonly string _connectionString;
    private readonly string _tableName;

    public FuncionarioController(RHContext context, IConfiguration configuration)
    {
        _context = context;
        // Note: keep these keys in appsettings.json (see README below)
        _connectionString = configuration.GetValue<string>("ConnectionStrings:SAConnectionString");
        _tableName = configuration.GetValue<string>("ConnectionStrings:AzureTableName");
    }

    private TableClient GetTableClient()
    {
        var serviceClient = new TableServiceClient(_connectionString);
        var tableClient = serviceClient.GetTableClient(_tableName);

        tableClient.CreateIfNotExists();
        return tableClient;
    }

    [HttpGet("{id}")]
    public IActionResult ObterPorId(int id)
    {
        var funcionario = _context.Funcionarios.Find(id);

        if (funcionario == null)
            return NotFound();

        return Ok(funcionario);
    }

    [HttpPost]
    public IActionResult Criar(Funcionario funcionario)
    {
        // adiciona no contexto
        _context.Funcionarios.Add(funcionario);
        // salva no SQL Server
        _context.SaveChanges();

        // gera log e salva no Azure Table
        var tableClient = GetTableClient();
        // usa o departamento como partition key (pode ser alterado conforme necessidade)
        var funcionarioLog = new FuncionarioLog(funcionario, TipoAcao.Inclusao, funcionario.Departamento ?? "Geral", Guid.NewGuid().ToString());

        tableClient.UpsertEntity(funcionarioLog, TableUpdateMode.Replace);

        return CreatedAtAction(nameof(ObterPorId), new { id = funcionario.Id }, funcionario);
    }

    [HttpPut("{id}")]
    public IActionResult Atualizar(int id, Funcionario funcionario)
    {
        var funcionarioBanco = _context.Funcionarios.Find(id);

        if (funcionarioBanco == null)
            return NotFound();

        // atualiza todas as propriedades esperadas
        funcionarioBanco.Nome = funcionario.Nome;
        funcionarioBanco.Endereco = funcionario.Endereco;
        funcionarioBanco.Ramal = funcionario.Ramal;
        funcionarioBanco.EmailProfissional = funcionario.EmailProfissional;
        funcionarioBanco.Departamento = funcionario.Departamento;
        funcionarioBanco.Salario = funcionario.Salario;
        funcionarioBanco.DataAdmissao = funcionario.DataAdmissao;

        // informa ao EF que houve alteração e persiste
        _context.Funcionarios.Update(funcionarioBanco);
        _context.SaveChanges();

        var tableClient = GetTableClient();
        var funcionarioLog = new FuncionarioLog(funcionarioBanco, TipoAcao.Atualizacao, funcionarioBanco.Departamento ?? "Geral", Guid.NewGuid().ToString());

        tableClient.UpsertEntity(funcionarioLog, TableUpdateMode.Replace);

        return Ok(funcionarioBanco);
    }

    [HttpDelete("{id}")]
    public IActionResult Deletar(int id)
    {
        var funcionarioBanco = _context.Funcionarios.Find(id);

        if (funcionarioBanco == null)
            return NotFound();

        _context.Funcionarios.Remove(funcionarioBanco);
        _context.SaveChanges();

        var tableClient = GetTableClient();
        var funcionarioLog = new FuncionarioLog(funcionarioBanco, TipoAcao.Remocao, funcionarioBanco.Departamento ?? "Geral", Guid.NewGuid().ToString());

        tableClient.UpsertEntity(funcionarioLog, TableUpdateMode.Replace);

        return NoContent();
    }
}
