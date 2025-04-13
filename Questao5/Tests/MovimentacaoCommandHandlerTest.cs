using Moq;

using Questao5.Application.Commands.Requests;
using Questao5.Application.Exceptions;
using Questao5.Application.Handlers;
using Questao5.Domain.Entities;
using Questao5.Domain.Enumerators;
using Questao5.Infrastructure.Repositories.Interfaces;

using System.Data;

using Xunit;

namespace Questao5.Tests
{

    public class MovimentacaoCommandHandlerTests
    {
        private readonly Mock<IMovimentoDapperRepository> _mockMovimentoRepository;
        private readonly Mock<IContaCorrenteDapperRepository> _mockContaRepository;
        private readonly Mock<IIdempotenciaRepository> _mockIdempotenciaRepository;
        private readonly Mock<IDbConnection> _mockDbConnection;
        private readonly Mock<IDbTransaction> _mockTransaction;
        private readonly MovimentacaoCommandHandler _handler;

        public MovimentacaoCommandHandlerTests()
        {
            _mockMovimentoRepository = new Mock<IMovimentoDapperRepository>();
            _mockContaRepository = new Mock<IContaCorrenteDapperRepository>();
            _mockIdempotenciaRepository = new Mock<IIdempotenciaRepository>();
            _mockDbConnection = new Mock<IDbConnection>();

            _mockTransaction = new Mock<IDbTransaction>();
            _mockDbConnection.Setup(conn => conn.BeginTransaction()).Returns(_mockTransaction.Object);

            _handler = new MovimentacaoCommandHandler(_mockMovimentoRepository.Object,
                                                      _mockContaRepository.Object,
                                                      _mockIdempotenciaRepository.Object,
                                                      _mockDbConnection.Object);
        }

        //TODO: ADICIONEI AQUI
        [Fact]
        public async Task MovimentacaoComContaValida_DeveRetornarResultadoMovimentacao()
        {
            // Arrange
            MovimentacaoCommand request = new ()
            {
                IdContaCorrente = "12345",
                IdRequisicao = "req123",
                TipoMovimento = TipoMovimento.C,
                Valor = 100.00M
            };

            string response = Guid.NewGuid().ToString();

            _mockContaRepository.Setup(repo => repo.ObterPorID(It.IsAny<string>())).ReturnsAsync(new ContaCorrente { idcontacorrente = "12345", ativo = 1 });
            _mockMovimentoRepository.Setup(repo => repo.Movimentar(It.IsAny<MovimentacaoRequest>())).ReturnsAsync(response);

            // Act
            string result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(response, result);
            _mockDbConnection.Verify(conn => conn.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(tran => tran.Commit(), Times.Once);
            _mockTransaction.Verify(tran => tran.Dispose(), Times.Once);
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

            // Act
            var exception = await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            Assert.Equal("O valor da movimentação deve ser positivo.", exception.Message);
            Assert.Equal(BusinessErrorType.INVALID_VALUE, exception.ErrorType);
            _mockDbConnection.Verify(conn => conn.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(tran => tran.Rollback(), Times.Once);
            _mockTransaction.Verify(tran => tran.Dispose(), Times.Once);
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

            // Act
            var exception = await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            Assert.Equal("Tipo de movimentação inválido.", exception.Message);
            Assert.Equal(BusinessErrorType.INVALID_TYPE, exception.ErrorType);
            _mockDbConnection.Verify(conn => conn.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(tran => tran.Rollback(), Times.Once);
            _mockTransaction.Verify(tran => tran.Dispose(), Times.Once);
        }

        [Fact]
        public async Task MovimentacaoComIdempotenciaExistente_DeveRetornarResultadoExistente()
        {
            // Arrange
            MovimentacaoCommand request = new ()
            {
                IdContaCorrente = "12345",
                IdRequisicao = "req123",
                TipoMovimento = TipoMovimento.C,
                Valor = 100.00M
            };

            string response = Guid.NewGuid().ToString();

            var exception = await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(request, CancellationToken.None));

            _mockIdempotenciaRepository.Setup(repo => repo.ObterResultado(It.IsAny<string>())).ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(response, result);
            _mockDbConnection.Verify(conn => conn.BeginTransaction(), Times.Exactly(2));
            _mockTransaction.Verify(conn => conn.Commit(), Times.Never);
            _mockTransaction.Verify(conn => conn.Rollback(), Times.Once);
            _mockTransaction.Verify(conn => conn.Dispose(), Times.Exactly(2));
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

            _mockContaRepository.Setup(repo => repo.ObterPorID(It.IsAny<string>())).ReturnsAsync((ContaCorrente)null);

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

            _mockContaRepository.Setup(repo => repo.ObterPorID(It.IsAny<string>())).ReturnsAsync(new ContaCorrente { ativo = 1 });
            _mockMovimentoRepository.Setup(repo => repo.Movimentar(It.IsAny<MovimentacaoRequest>())).ThrowsAsync(new Exception("Erro inesperado"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
            Assert.Equal("Erro inesperado", exception.Message);
            _mockTransaction.Verify(conn => conn.Rollback(), Times.Once);
        }
    }
}