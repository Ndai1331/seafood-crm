using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Contract.Positions;
using Core.Const;
using Core.Enum;
using Core.Exceptions;
using Domain.Positions;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.Repository.Positions;
using SqlServ4r.Repository.Users;
using Volo.Abp.DependencyInjection;

namespace Application.Positions
{
    public class PositionService : ServiceBase,IPositionService,ITransientDependency
    {
        private readonly PositionRepository _positionRepository;
        private readonly UserRepository _userRepository;


        public PositionService(PositionRepository positionRepository,UserRepository userRepository)
        {
            _positionRepository = positionRepository;
            _userRepository = userRepository;
        }


        public async Task<PositionDto> CreateAsync(CreateUpdatePositionDto input)
        {
            (input.Name, input.Code) = TrimText(input.Name, input.Code);

            var position = ObjectMapper.Map<CreateUpdatePositionDto, Position>(input);
            await _positionRepository.AddAsync(position);
            return ObjectMapper.Map<Position,PositionDto>(position);
        }

        public async Task<PositionDto> UpdateAsync(CreateUpdatePositionDto input, int id)
        {
            (input.Name, input.Code) = TrimText(input.Name, input.Code);

            var item = await _positionRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }
            var position = ObjectMapper.Map(input,item);
             _positionRepository.Update(position);
            return ObjectMapper.Map<Position,PositionDto>(position);
        }



        public async Task DeleteAsync(int id)
        {
            var item = await _positionRepository.FirstOrDefaultAsync(x => x.Id == id);
            
            if (item is null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }
           _positionRepository.Remove(item);
        }

        public async Task<List<PositionDto>> GetListAsync()
        {
            var positions = await _positionRepository.GetQueryable().OrderBy(x=>x.ODX)
                .ToListAsync();
            return ObjectMapper.Map<List<Position>, List<PositionDto>>(positions);
        }
        
        private (string Name, string? Code) TrimText(string name, string code)
        {
            return (name.Trim(), code?.Trim().ToUpper());
        }
    }
}