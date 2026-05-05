using FluentValidation;
using RefillingStation.Application.DTOs.Trips;

namespace RefillingStation.Api.Features.Trips.validators
{
    public class UpdateTripRequestValidator : AbstractValidator<TripUpdateRequest>
    {
        public UpdateTripRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .Must(date => date.Date <= DateTime.Today)
                .WithMessage("Cannot select future date.");

            RuleFor(x => x.TripNumber)
                .GreaterThan(0);

            RuleFor(x => x.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.Source)
                .MaximumLength(100);

            RuleFor(x => x.TripType)
                .MaximumLength(50);

            RuleFor(x => x.CustomerCategory)
                .MaximumLength(50);

            RuleFor(x => x.Notes)
                .MaximumLength(500);

            RuleFor(x => x.CollectedQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.LoadedQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DeliveredQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.FreeQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ReturnedQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ReplacementQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ActualCashCollected).GreaterThanOrEqualTo(0);

            RuleFor(x => x)
                .Must(x => !x.TimeStarted.HasValue 
                        || !x.TimeEnded.HasValue 
                        || x.TimeEnded.Value >= x.TimeStarted.Value)
                .WithMessage("TimeEnded must be greater than or equal to TimeStarted.");

            RuleFor(x => x)
                .Must(x => x.DeliveredQty > 0
                        || x.LoadedQty > 0
                        || x.CollectedQty > 0
                        || x.ActualCashCollected > 0)
                .WithMessage("A trip requires at least one of Delivered Qty, Loaded Qty, Collected Qty, or Actual Cash Collected to be greater than zero.");
        }
    }
    
}
