using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class PlatformsController:ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;

        public PlatformsController(IPlatformRepo repository,IMapper mapper,ICommandDataClient commandDataClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
        }

        [HttpGet]
        [Route("[Action]")]
        public IActionResult GetPlatforms(){
            Console.WriteLine("--> Getting Platforms ...");
            var platforms =  _repository.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet]
        [Route("[Action]/{Id}")]
        public IActionResult GetPlatformById([FromRoute]int Id){
            Console.WriteLine($"--> Getting Platform with {Id} ...");
            var Platform = _repository.GetPlatformById(Id);
            if(Platform!=null)
                return Ok(_mapper.Map<PlatformReadDto>(Platform));
            else
                return NotFound();
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> CreatePlatform([FromBody]PlatformCreateDto platformCreateDto){
            Console.WriteLine($"--> Creating Platform - {platformCreateDto.Name}");
            var platformModel = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();
            var PlatformReadDto = _mapper.Map<PlatformReadDto>(platformModel);
            try{
            await _commandDataClient.SendPlatformToCommand(PlatformReadDto);
            }
            catch(Exception ex){
            Console.WriteLine("--> Could not send synchronously:"+ex.Message.ToString());
            }
            return CreatedAtAction(nameof(GetPlatformById),new {Id = PlatformReadDto.Id},PlatformReadDto);
        }
    }
}