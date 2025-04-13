using Moq;

using Questao5.Application.Commands.Requests;
using Questao5.Application.Exceptions;
using Questao5.Application.Handlers;
using Questao5.Domain.Entities;
using Questao5.Domain.Enumerators;
using Questao5.Infrastructure.Repositories.Interfaces;

using System.Data;

using Xunit;
[]
public class MovimentacaoCommandHandlerTests
{
    private readonly Mock<IMovimentoDapperRepository> _mockMovimentoRepository;
    private readonly Mock<IContaCorrenteDapperRepository> _mockContaRepository;
    private readonly Mock<IIdempotenciaRepository> _mockIdempotenciaRepository;
    private readonly Mock<IDbConnection> _mockDbConnection;
    private readonly MovimentacaoCommandHandler _handler;

    public MovimentacaoCommandHandlerTests()
    {
        _mockMovimentoRepository = new Mock<IMovimentoDapperRepository>();
        _mockContaRepository = new Mock<IContaCorrenteDapperRepository>();
        _mockIdempotenciaRepository = new Mock<IIdempotenciaRepository>();vc]ghgj
        _mockDbConnection = new Mock<IDbConnection>();

        _mockDbConnection.Setup(conn => conn.BeginTransaction()).Returns(new Mock<IDbTransaction>().Object);

        _handler = new MovimentacaoCommandHandler(
            _mockMovimentoRepository.Object,
            _mockContaRepository.Object,
            _mockIdempotenciaRepository.Object,
            _mockDbConnection.Object
        );
    }

    [Fact]
    public async Task MovimentacaoComContaValida_DeveRetornarResultadoMovimentacao()
    {
        // Arrange
        var request = new MovimentacaoCommand
        {
            IdContaCorrente = "12345",
            IdRequisicao = "req123",
            TipoMovimento = TipoMovimento.C,
            Valor = 100.00M
        };

        _mockIdempotenciaRepository.Setup(repo => repo.ObterResultado(It.IsAny<string>())).ReturnsAsync((string)null);
        _mockContaRepository.Setup(repo => repo.obterPorID(It.IsAny<string>())).ReturnsAsync(new ContaCorrente { ativo = 1 });
        _mockMovimentoRepository.Setup(repo => repo.movimentar(It.IsAny<MovimentacaoRequest>())).ReturnsAsync("sucesso");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal("sucesso", result);
        _mockDbConnection.Verify(conn => conn.BeginTransaction(), Times.Once);
        _mockDbConnection.Verify(conn => conn.BeginTransaction().Commit(), Times.Once);
    }

    [Fact]
    public async Task MovimentacaoComValorInvalido_DeveLancarExcecaoBusinessException()
    {
        // Arrange
        var request = new MovimentacaoCommand
        {
            IdContaCorrente = "12345",
            IdRequisicao = "req123",
            TipoMovimento = TipoMovimento.C, 
            Valor = -1.00M 
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));
        Assert.Equal("O valor da movimentação deve ser positivo.", exception.Message);
    }

    [Fact]
    public async Task MovimentacaoComTipoMovimentoInvalido_DeveLancarExcecaoBusinessException()
    {
        // Arrange
        var request = new MovimentacaoCommand
        {
            IdContaCorrente = "12345",
            IdRequisicao = "req123",
            TipoMovimento = (TipoMovimento)99,
            Valor = 100.00M
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));
        Assert.Equal("Tipo de movimentação inválido.", exception.Message);
    }

    [Fact]
    public async Task MovimentacaoComIdempotenciaExistente_DeveRetornarResultadoExistente()
    {
        // Arrange
        var request = new MovimentacaoCommand
        {
            IdContaCorrente = "12345",
            IdRequisicao = "req123",
            TipoMovimento = TipoMovimento.C,
            Valor = 100.00M
        };

        _mockIdempotenciaRepository.Setup(repo => repo.ObterResultado(It.IsAny<string>())).ReturnsAsync("resultadoExistente");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal("resultadoExistente", result);
        _mockDbConnection.Verify(conn => conn.BeginTransaction(), Times.Never);
    }

    [Fact]
    public async Task MovimentacaoComContaNaoEncontrada_DeveLancarExcecaoBusinessException()
    {
        // Arrange
        var request = new MovimentacaoCommand
        {
            IdContaCorrente = "12345",
            IdRequisicao = "req123",
            TipoMovimento = TipoMovimento.C, 
            Valor = 100.00M
        };

        _mockContaRepository.Setup(repo => repo.obterPorID(It.IsAny<string>())).ReturnsAsync((ContaCorrente)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));
        Assert.Equal("Conta corrente inválida.", exception.Message);
    }

    [Fact]
    public async Task Movimentacao_DeveReverterTransacaoEmCasoDeExcecao()
    {
        // Arrange
        var request = new MovimentacaoCommand
        {
            IdContaCorrente = "12345",
            IdRequisicao = "req123",
            TipoMovimento = TipoMovimento.C,
            Valor = 100.00M
        };

        _mockIdempotenciaRepository.Setup(repo => repo.ObterResultado(It.IsAny<string>())).ReturnsAsync((string)null);
        _mockContaRepository.Setup(repo => repo.obterPorID(It.IsAny<string>())).ReturnsAsync(new ContaCorrente { ativo = 1 });
        _mockMovimentoRepository.Setup(repo => repo.movimentar(It.IsAny<MovimentacaoRequest>())).ThrowsAsync(new Exception("Erro inesperado"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        Assert.Equal("Erro inesperado", exception.Message);
        _mockDbConnection.Verify(conn => conn.BeginTransaction().Rollback(), Times.Once);
    }
}