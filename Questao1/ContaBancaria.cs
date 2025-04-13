using System;
using System.Globalization;

namespace Questao1
{
    class ContaBancaria {

        public int NumeroConta { get; }
        public string NomeTitular { get; set; }
        public double DepositoInicial { get; set; }
        private double Saldo { get; set; }

        private const double TAXA_SAQUE = 3.50;

        public ContaBancaria(int NumeroConta, string NomeTitular, double DepositoInicial = 0)
        {
            this.NumeroConta = NumeroConta;
            this.NomeTitular = NomeTitular;
            this.Saldo = this.DepositoInicial = DepositoInicial;
        }

        public void Saque(double valorSaque)
        {
            this.Saldo -= valorSaque + TAXA_SAQUE;
        }

        public void Deposito(double valorDeposito)
        {
            this.Saldo += valorDeposito;
        }

        public string DadosDaConta()
        {
            return $"Conta {this.NumeroConta}, Titular {this.NomeTitular}, Saldo ${this.Saldo} ";
        }

    }

}
