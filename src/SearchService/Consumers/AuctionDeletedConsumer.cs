using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly IMapper _mapper;
    public AuctionDeletedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("==> Consuming Auction Deleted" + context.Message.Id);

        var didDelete = await DB.DeleteAsync<Item>(context.Message);

        if (!didDelete.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionDeleted), "Problem deleted auction");
        }
    }
}
