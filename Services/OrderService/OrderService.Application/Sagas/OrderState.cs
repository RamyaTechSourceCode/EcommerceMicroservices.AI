using MassTransit;

namespace OrderService.Application.Sagas
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = default!;

        public Guid OrderId { get; set; }

        public int TotalItems { get; set; }
        //public int ReservedItems { get; set; }

        //public int FailedItems { get; set; }

        public List<Guid> PendingProducts { get; set; } = new();

        public List<Guid> ReservedProducts { get; set; } = new();
    }
}