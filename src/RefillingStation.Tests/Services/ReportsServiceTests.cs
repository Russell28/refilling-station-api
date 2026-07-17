using FluentAssertions;
using Moq;
using RefillingStation.Application.DTOs.Reports;
using RefillingStation.Application.DTOs.Reports.Breakdowns;
using RefillingStation.Application.DTOs.Reports.Dashboard;
using RefillingStation.Application.DTOs.Reports.DailySummary;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;
using RefillingStation.Application.Interfaces.Repositories.Reports;
using RefillingStation.Application.Services;
using RefillingStation.Application.Settings;
using Microsoft.Extensions.Options;

namespace RefillingStation.Tests.Services
{
    public class ReportsServiceTests
    {
        private readonly Mock<IDashboardRepository> _dashboardRepository;
        private readonly Mock<IDailySummaryRepository> _dailySummaryRepository;
        private readonly Mock<IMonthlySummaryRepository> _monthlySummaryRepository;
        private readonly Mock<IOptions<BacklogSettings>> _backlogSettingsOptions;

        private readonly ReportsService _reportsService;

        public ReportsServiceTests()
        {
            _dashboardRepository = new Mock<IDashboardRepository>();
            _dailySummaryRepository = new Mock<IDailySummaryRepository>();
            _monthlySummaryRepository = new Mock<IMonthlySummaryRepository>();
            _backlogSettingsOptions = new Mock<IOptions<BacklogSettings>>();

            _backlogSettingsOptions.Setup(x => x.Value).Returns(new BacklogSettings { OpeningBacklogQty = 1000 });

            _reportsService = new ReportsService(
                _dashboardRepository.Object,
                _dailySummaryRepository.Object,
                _monthlySummaryRepository.Object,
                _backlogSettingsOptions.Object
            );
        }

        #region GetDashboardAsync
        [Fact]
        public async Task GetDashboardAsync_Should_Return_Dashboard_With_Valid_Data()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, 100, 50, 80, 5, 10, 2, 1000)
                },
                Debts = new List<CustomerDebtReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Customer A", 500)
                },
                Expenses = new List<ExpenseReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Fuel", 200)
                },
                PayrollEntries = new List<PayrollEntryReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Employee A", 500)
                },
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Summary.Should().NotBeNull();
            result.DailyReports.Should().NotBeNull();
            result.ExpenseBreakdown.Should().NotBeNull();
            result.DebtBreakdown.Should().NotBeNull();
            result.PayrollBreakdown.Should().NotBeNull();

            _dashboardRepository.Verify(r => r.GetDashboardAsync(startDate, endDate), Times.Once);
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Calculate_Summary_Metrics_Correctly()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, 100, 50, 80, 0, 0, 0, 1000)
                },
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Fuel", 200)
                },
                PayrollEntries = new List<PayrollEntryReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Employee A", 500)
                },
                PayrollPayments = new List<PayrollPaymentReportItem>
                {
                    new(1, 1, "Employee A", new DateOnly(2024, 1, 1), 500)
                },
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>(),
                PayrollPaymentsRunning = new List<PayrollPaymentReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.Summary.BacklogStartQty.Should().Be(1000);
            result.Summary.BacklogEndQty.Should().Be(1020);
            result.Summary.TripCount.Should().Be(1);
            result.Summary.CollectedQtyTotal.Should().Be(100);
            result.Summary.DeliveredQtyTotal.Should().Be(80);
            result.Summary.ExpensesTotal.Should().Be(200);
            result.Summary.PayrollEarnedTotal.Should().Be(500);
            result.Summary.PayrollPaidTotal.Should().Be(500);
            result.Summary.CashCollectedTotal.Should().Be(1000);
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Return_Empty_Daily_Reports_When_No_Activity()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.DailyReports.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Calculate_Backlog_With_Previous_Data()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, 50, 0, 30, 0, 0, 0, 0)
                },
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>
                {
                    new(2, new DateOnly(2023, 12, 31), 1, 200, 0, 100, 0, 0, 0, 0)
                },
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.Summary.BacklogStartQty.Should().Be(1100);
            result.Summary.BacklogEndQty.Should().Be(1120);
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Return_Expense_Breakdown()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Fuel", 300),
                    new(2, new DateOnly(2024, 1, 2), 2, "Maintenance", 200)
                },
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.ExpenseBreakdown.Should().NotBeNull();
            result.ExpenseBreakdown.TotalExpense.Should().Be(500);
            result.ExpenseBreakdown.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Return_Debt_Breakdown()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Customer A", 500),
                    new(2, new DateOnly(2024, 1, 2), 1, "Customer A", -200)
                },
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>
                {
                    new(3, new DateOnly(2024, 1, 2), 1, "Customer A", 300)
                },
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.DebtBreakdown.Should().NotBeNull();
            result.DebtBreakdown.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Return_Payroll_Breakdown()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "Employee A", 500),
                    new(2, new DateOnly(2024, 1, 15), 2, "Employee B", 600)
                },
                PayrollPayments = new List<PayrollPaymentReportItem>
                {
                    new(1, 1, "Employee A", new DateOnly(2024, 1, 1), 300),
                    new(2, 2, "Employee B", new DateOnly(2024, 1, 15), 600)
                },
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>(),
                PayrollPaymentsRunning = new List<PayrollPaymentReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.PayrollBreakdown.Should().NotBeNull();
            result.PayrollBreakdown.TotalEarned.Should().Be(1100);
            result.PayrollBreakdown.TotalPaid.Should().Be(900);
            result.PayrollBreakdown.Items.Should().HaveCount(2);
        }
        #endregion

        #region GetDailySummaryAsync
        [Fact]
        public async Task GetDailySummaryAsync_Should_Return_Daily_Summary_For_Non_Admin()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 1);
            var isAdmin = false;

            var rawData = new DailySummaryRawData
            {
                Trips = new List<TripReportItem>
                {
                    new(1, date, 1, 100, 50, 80, 5, 10, 2, 1000)
                },
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>
                {
                    new(1, date, 1, "Fuel", 200)
                },
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dailySummaryRepository.Setup(r => r.GetDailySummaryAsync(date))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDailySummaryAsync(date, isAdmin);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<DailySummaryResponse>();

            _dailySummaryRepository.Verify(r => r.GetDailySummaryAsync(date), Times.Once);
        }

        [Fact]
        public async Task GetDailySummaryAsync_Should_Return_Daily_Summary_Admin_For_Admin()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 1);
            var isAdmin = true;

            var rawData = new DailySummaryRawData
            {
                Trips = new List<TripReportItem>
                {
                    new(1, date, 1, 100, 50, 80, 5, 10, 2, 1000)
                },
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>
                {
                    new(1, date, 1, "Fuel", 200)
                },
                PayrollEntries = new List<PayrollEntryReportItem>
                {
                    new(1, date, 1, "Employee A", 500)
                },
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dailySummaryRepository.Setup(r => r.GetDailySummaryAsync(date))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDailySummaryAsync(date, isAdmin);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<DailySummaryAdminResponse>();

            _dailySummaryRepository.Verify(r => r.GetDailySummaryAsync(date), Times.Once);
        }

        [Fact]
        public async Task GetDailySummaryAsync_Should_Include_Expense_Breakdown()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 1);

            var rawData = new DailySummaryRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>
                {
                    new(1, date, 1, "Fuel", 300),
                    new(2, date, 2, "Maintenance", 100)
                },
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dailySummaryRepository.Setup(r => r.GetDailySummaryAsync(date))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDailySummaryAsync(date, false) as DailySummaryResponse;

            // Assert
            result.Should().NotBeNull();
            result!.ExpenseBreakdown.Should().NotBeNull();
            result.ExpenseBreakdown.TotalExpense.Should().Be(400);
            result.ExpenseBreakdown.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetDailySummaryAsync_Should_Include_Debt_Breakdown()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 1);

            var rawData = new DailySummaryRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>
                {
                    new(1, date, 1, "Customer A", 500)
                },
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dailySummaryRepository.Setup(r => r.GetDailySummaryAsync(date))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDailySummaryAsync(date, false) as DailySummaryResponse;

            // Assert
            result.Should().NotBeNull();
            result!.DebtBreakdown.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDailySummaryAsync_Admin_Should_Include_Payroll_Breakdown()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 1);

            var rawData = new DailySummaryRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                PayrollPayments = new List<PayrollPaymentReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>
                {
                    new(1, date, 1, "Employee A", 500)
                },
                PayrollPaymentsRunning = new List<PayrollPaymentReportItem>
                {
                    new(1, 1, "Employee A", date, 300)
                }
            };

            _dailySummaryRepository.Setup(r => r.GetDailySummaryAsync(date))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDailySummaryAsync(date, true) as DailySummaryAdminResponse;

            // Assert
            result.Should().NotBeNull();
            result!.PayrollBreakdown.Should().NotBeNull();
            result.PayrollBreakdown.TotalOwed.Should().Be(200);
        }
        #endregion

        #region GetMonthlySummaryAsync
        [Fact]
        public async Task GetMonthlySummaryAsync_Should_Return_Monthly_Summary()
        {
            // Arrange
            var monthYear = "2024-01";

            var rawData = new MonthlySummaryRawData
            {
                GrossTotal = 5000,
                DebtTotal = 1000,
                ExpenseTotal = 800,
                PayrollEarnedTotal = 2000,
                PayrollPaidTotal = 1500,
                SavedClosing = null
            };

            _monthlySummaryRepository.Setup(r => r.GetMonthlySummaryAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetMonthlySummaryAsync(monthYear);

            // Assert
            result.Should().NotBeNull();
            result.MonthYear.Should().Be(monthYear);
            result.SummaryTotals.Should().NotBeNull();
            result.SummaryTotals.cashCollected.Should().Be(5000);
            result.SummaryTotals.expenseTotal.Should().Be(800);
            result.SummaryTotals.payrollEarnedTotal.Should().Be(2000);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_Should_Calculate_Net_Metrics()
        {
            // Arrange
            var monthYear = "2024-01";

            var rawData = new MonthlySummaryRawData
            {
                GrossTotal = 5000,
                DebtTotal = 500,
                ExpenseTotal = 1000,
                PayrollEarnedTotal = 1500,
                PayrollPaidTotal = 1000,
                SavedClosing = null
            };

            _monthlySummaryRepository.Setup(r => r.GetMonthlySummaryAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetMonthlySummaryAsync(monthYear);

            // Assert
            result.SummaryTotals.netBeforePayroll.Should().Be(4000);
            result.SummaryTotals.netAfterPayroll.Should().Be(2500);
            result.SummaryTotals.netCashFlow.Should().Be(3000);
            result.SummaryTotals.payrollOwedTotal.Should().Be(500);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_Should_Include_Saved_Closing_If_Exists()
        {
            // Arrange
            var monthYear = "2024-01";

            var savedClosing = new MonthlyClosingReportItem(
                "2024-01",
                1000,
                2000,
                "Month closed",
                DateTime.UtcNow
            );

            var rawData = new MonthlySummaryRawData
            {
                GrossTotal = 5000,
                DebtTotal = 500,
                ExpenseTotal = 800,
                PayrollEarnedTotal = 2000,
                PayrollPaidTotal = 1500,
                SavedClosing = savedClosing
            };

            _monthlySummaryRepository.Setup(r => r.GetMonthlySummaryAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetMonthlySummaryAsync(monthYear);

            // Assert
            result.SavedClosing.Should().NotBeNull();
            result.SavedClosing!.MonthYear.Should().Be("2024-01");
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_Should_Return_Null_Closing_When_Not_Saved()
        {
            // Arrange
            var monthYear = "2024-01";

            var rawData = new MonthlySummaryRawData
            {
                GrossTotal = 5000,
                DebtTotal = 500,
                ExpenseTotal = 800,
                PayrollEarnedTotal = 2000,
                PayrollPaidTotal = 1500,
                SavedClosing = null
            };

            _monthlySummaryRepository.Setup(r => r.GetMonthlySummaryAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetMonthlySummaryAsync(monthYear);

            // Assert
            result.SavedClosing.Should().BeNull();
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_Should_Parse_Month_Year_Correctly()
        {
            // Arrange
            var monthYear = "2024-06";

            var rawData = new MonthlySummaryRawData
            {
                GrossTotal = 0,
                DebtTotal = 0,
                ExpenseTotal = 0,
                PayrollEarnedTotal = 0,
                PayrollPaidTotal = 0,
                SavedClosing = null
            };

            _monthlySummaryRepository.Setup(r => r.GetMonthlySummaryAsync(
                It.Is<DateOnly>(d => d.Year == 2024 && d.Month == 6 && d.Day == 1),
                It.Is<DateOnly>(d => d.Year == 2024 && d.Month == 6 && d.Day == 30)
            )).ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetMonthlySummaryAsync(monthYear);

            // Assert
            result.MonthYear.Should().Be(monthYear);
            _monthlySummaryRepository.Verify(
                r => r.GetMonthlySummaryAsync(
                    It.Is<DateOnly>(d => d == new DateOnly(2024, 6, 1)),
                    It.Is<DateOnly>(d => d == new DateOnly(2024, 6, 30))
                ), 
                Times.Once
            );
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task GetDashboardAsync_Should_Handle_Zero_Division_For_Per_Gallon_Metrics()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.Summary.CostPerGal.Should().Be(0);
            result.Summary.RetailPerGal.Should().Be(0);
            result.Summary.ProfitPerGal.Should().Be(0);
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Handle_Negative_Debts_As_Payments()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);

            var rawData = new DashboardRawData
            {
                Trips = new List<TripReportItem>(),
                Debts = new List<CustomerDebtReportItem>
                {
                    new(1, new DateOnly(2024, 1, 1), 1, "A", 1000),
                    new(2, new DateOnly(2024, 1, 1), 1, "A", -300)
                },
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>(),
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dashboardRepository.Setup(r => r.GetDashboardAsync(startDate, endDate))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            // Assert
            result.Summary.DebtCreatedTotal.Should().Be(1000);
            result.Summary.DebtPaymentsTotal.Should().Be(300);
        }

        [Fact]
        public async Task GetDailySummaryAsync_Should_Calculate_Backlog_For_Specific_Date()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 15);

            var rawData = new DailySummaryRawData
            {
                Trips = new List<TripReportItem>
                {
                    new(1, date, 1, 50, 0, 40, 0, 0, 0, 0)
                },
                Debts = new List<CustomerDebtReportItem>(),
                Expenses = new List<ExpenseReportItem>(),
                PayrollEntries = new List<PayrollEntryReportItem>(),
                TripsBefore = new List<TripReportItem>
                {
                    new(2, new DateOnly(2023, 12, 31), 1, 100, 0, 80, 0, 0, 0, 0)
                },
                DebtsRunning = new List<CustomerDebtReportItem>(),
                PayrollEntriesRunning = new List<PayrollEntryReportItem>()
            };

            _dailySummaryRepository.Setup(r => r.GetDailySummaryAsync(date))
                .ReturnsAsync(rawData);

            // Act
            var result = await _reportsService.GetDailySummaryAsync(date, false) as DailySummaryResponse;

            // Assert
            result.Should().NotBeNull();
            result!.Summary.backlogStartQty.Should().Be(1020);
            result.Summary.backlogEndQty.Should().Be(1030);
        }
        #endregion
    }
}
