using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;
    public AuctionsController(AuctionDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions([FromQuery] string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        // TODO: add validation
        auction.Seller = "Test";

        _context.Auctions.Add(auction);

        var didCreate = await _context.SaveChangesAsync() > 0;

        if (!didCreate)
        {
            return BadRequest("Could not save changes to DB");
        }
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id:Guid}")]
    public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDto.Year ?? auction.Item.Year;
        _context.Auctions.Update(auction);

        var didSave = await _context.SaveChangesAsync() > 0;

        if (!didSave)
        {
            return BadRequest("Could not save changes to DB");
        }

        return Ok("Successfully saved changes to DB");
    }

    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult<AuctionDto>> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
            .FindAsync(id);

        if (auction == null) return NotFound();

        _context.Auctions.Remove(auction);

        var didDelete = await _context.SaveChangesAsync() > 0;

        if (!didDelete)
        {
            return BadRequest("Could not save changes to DB");
        }

        return Ok("Successfully deleted item from DB");
    }
}
