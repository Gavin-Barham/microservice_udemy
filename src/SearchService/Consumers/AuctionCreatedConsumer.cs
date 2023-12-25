using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("==> Consuming Auction Created" + context.Message.Id);

        var item = _mapper.Map<Item>(context.Message);

        if (item.Model == "Foo")
        {
            throw new ArgumentException("Cannot have model of Foo");
        }
        else
        {
            Console.WriteLine("Not of type ArgumentException - update dashboard somewhere");
        }

        await item.SaveAsync();
    }
}
