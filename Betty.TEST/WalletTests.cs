using Betty.Core.Entities;
using Betty.Core.Exceptions;
using Betty.Core.Interfaces;
using Moq;

namespace Betty.TEST;

public class WalletTests
{
    private readonly Mock<IWalletRepository> _repo;
    private readonly Wallet _wallet;

    public WalletTests()
    {
        _repo = new Mock<IWalletRepository>();
        _repo.Setup(r => r.GetCurrentBalance()).Returns(0m);
        _wallet = new Wallet(_repo.Object);
    }

    #region Deposit Validation
    [Fact]
    public void Deposit_ValidAmount_ShouldIncreaseBalance()
    {
        //arange
        _wallet.Deposit(10m);

        //Act & Asert
        Assert.Equal(10m, _wallet.Balance);
    }

    [Fact]

    public void Deposit_ValidAmount_ShouldCreateDepositTransactionData()
    {
        //arange
        _wallet.Deposit(100m);

        //act & asert
        _repo.Verify(r => r.AddTransactionData(It.Is<TransactionData>(t =>
            t.Type == TransactionDataType.Deposit &&
            t.Amount == 100m &&
            t.BalanceAfter == 100m)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    
    public void Deposit_Zero_Throws_ArgumentException(decimal amount)
    {
        Assert.Throws<WalletException>(() => _wallet.Deposit(amount));
    }
   
    #endregion


    #region Withdrawal validation

    [Fact]

    public void Withdrawal_ValidAmount_DecreasesBalance()
    {
        //arange
        _wallet.Deposit(100m);
        _wallet.Withdrawal(50m);

        //act & asert
        Assert.Equal(50m, _wallet.Balance);
    }

    [Fact]
    public void Withdrawal_Should_Create_Withdrawal_TransactionData()
    {
        _wallet.Deposit(100m);
        _wallet.Withdrawal(50m);
        _repo.Verify(r => r.AddTransactionData(It.Is<TransactionData>(t =>
            t.Type == TransactionDataType.Withdrawal &&
            t.Amount == 50m &&
            t.BalanceAfter == 50m)), Times.Once);
    }

    [Fact]
    public void Withdrawal_MoreThanBalance_Should_Throws_InvalidOperationException_And_NotChange_Balance()
    {
        //arange
        _wallet.Deposit(100m);

        //act & asert
        Assert.Throws<WalletException>(() => _wallet.Withdrawal(101m));
        Assert.Equal(100m, _wallet.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Withdrawal_NonPoistiveAmount_Throws_ARgumentEception(decimal amount)
    {
        Assert.Throws<WalletException>(() => _wallet.Withdrawal(amount));
    }

    [Fact]
    public void Withdrawal_InsufficientFunds_DoesNotAddTransaction()
    {
        //arange
        _wallet.Deposit(50m);

        //act & assert
        Assert.Throws<WalletException>(() => _wallet.Withdrawal(100m));
        _repo.Verify(r => r.AddTransactionData(It.Is<TransactionData>(t =>
            t.Type == TransactionDataType.Withdrawal)), Times.Never);
    }
    #endregion

    #region BetOtucomeValidation

    [Fact]

    public void BetWIN_IncreaseBalance()
    {
        _wallet.Deposit(100m);
        _wallet.ChangeBalanceBaseOnBetOutcome(10m, 20m);
        Assert.Equal(110m, _wallet.Balance);
    }


    [Fact]
    public void BetLOSE_ReduceBalance()
    {
        _wallet.Deposit(100m);
        _wallet.ChangeBalanceBaseOnBetOutcome(20m, 0m);
        Assert.Equal(80m, _wallet.Balance);
    }
    [Fact]
    public void BetWin_CreteBetWinTransactionData()
    {
        _wallet.Deposit(100m);
        _wallet.ChangeBalanceBaseOnBetOutcome(10, 20m);
        _repo.Verify(r => r.AddTransactionData(It.Is<TransactionData>(t =>
            t.Type == TransactionDataType.BetWin &&
            t.Amount == 10m &&
            t.BalanceAfter == 110m)), Times.Once);
    }

    [Fact]
    public void BetLOSE_CreteBetLossTransactionData()
    {
        _wallet.Deposit(100m);
        _wallet.ChangeBalanceBaseOnBetOutcome(10, 0m);
        _repo.Verify(r => r.AddTransactionData(It.Is<TransactionData>(t =>
            t.Type == TransactionDataType.BetLoss &&
            t.Amount == 10m &&
            t.BalanceAfter == 90m)), Times.Once);
    }
 
    #endregion

}
